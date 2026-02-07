import ctypes
from ctypes import wintypes
import os
import subprocess
import sys
import time

try:
    import psutil
except ImportError:
    psutil = None

try:
    import win32clipboard
except ImportError:
    win32clipboard = None


from pc_automation_keys import (
    VK_CAPITAL,
    VK_CONTROL,
    VK_X,
    send_key_chord,
    toggle_narrator,
)


CF_UNICODETEXT = 13
GMEM_MOVEABLE = 0x0002
SPI_GETSCREENREADER = 0x0046

NARRATOR_CLIPBOARD_DELAY = 0.2
NARRATOR_TOGGLE_DELAY = 0.4
NARRATOR_TOGGLE_RETRIES = 4
CLIPBOARD_CLEAR_RETRIES = 3
CLIPBOARD_CLEAR_DELAY = 0.05

_user32 = ctypes.windll.user32
_kernel32 = ctypes.windll.kernel32

_user32.GetClipboardData.restype = wintypes.HANDLE
_user32.GetClipboardData.argtypes = [wintypes.UINT]
_user32.SetClipboardData.restype = wintypes.HANDLE
_user32.SetClipboardData.argtypes = [wintypes.UINT, wintypes.HANDLE]
_user32.SystemParametersInfoW.restype = wintypes.BOOL
_user32.SystemParametersInfoW.argtypes = [
    wintypes.UINT,
    wintypes.UINT,
    ctypes.c_void_p,
    wintypes.UINT,
]

_kernel32.GlobalAlloc.restype = wintypes.HGLOBAL
_kernel32.GlobalAlloc.argtypes = [wintypes.UINT, ctypes.c_size_t]
_kernel32.GlobalFree.argtypes = [wintypes.HGLOBAL]
_kernel32.GlobalLock.restype = wintypes.LPVOID
_kernel32.GlobalLock.argtypes = [wintypes.HGLOBAL]
_kernel32.GlobalUnlock.argtypes = [wintypes.HGLOBAL]


def is_narrator_running():
    if psutil is not None:
        for proc in psutil.process_iter(["name"]):
            name = (proc.info.get("name") or "").lower()
            if name == "narrator.exe":
                return True
        return False
    print("WARNING: psutil unavailable; falling back to SPI", file=sys.stderr)

    enabled = wintypes.BOOL()
    ok = _user32.SystemParametersInfoW(SPI_GETSCREENREADER, 0, ctypes.byref(enabled), 0)
    if not ok:
        print("WARNING: Unable to determine Narrator state", file=sys.stderr)
        return False
    return bool(enabled.value)


def _wait_for_narrator_state(expected_running):
    delay = NARRATOR_TOGGLE_DELAY
    for _ in range(NARRATOR_TOGGLE_RETRIES):
        if is_narrator_running() == expected_running:
            return True
        time.sleep(delay)
        delay *= 1.5
    return False


def _start_narrator_process():
    system_root = os.environ.get("SystemRoot", r"C:\Windows")
    narrator_path = os.path.join(system_root, "System32", "Narrator.exe")
    candidates = [narrator_path, "Narrator.exe"]
    for candidate in candidates:
        try:
            subprocess.Popen(
                [candidate], stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL
            )
            return True
        except Exception:
            continue
    print("Failed to start Narrator.exe directly", file=sys.stderr)
    return False


def _ensure_narrator_on():
    if is_narrator_running():
        return False
    print("INFO: Narrator is off; auto-enabling for capture", file=sys.stderr)
    toggle_narrator(log_to_stdout=False)
    if _wait_for_narrator_state(True):
        return True
    if _start_narrator_process() and _wait_for_narrator_state(True):
        return True
    print("ERROR: Auto-toggle Narrator failed; capture skipped", file=sys.stderr)
    return None


def _restore_narrator_if_needed(auto_enabled):
    if not auto_enabled:
        return
    toggle_narrator(log_to_stdout=False)
    if _wait_for_narrator_state(False):
        print("Narrator restored to previous state", file=sys.stderr)
    else:
        print("Failed to restore Narrator state", file=sys.stderr)


def prepare_narrator_capture_session():
    """Ensure Narrator is running for a capture session."""
    return _ensure_narrator_on()


def restore_narrator_capture_session(auto_enabled):
    """Restore Narrator state after a capture session."""
    _restore_narrator_if_needed(auto_enabled)


def preflight_clipboard_text_format():
    """Check once if clipboard has text format available."""
    ok, _, has_text = get_clipboard_text()
    if not ok:
        print("ERROR: Clipboard unavailable for Narrator capture", file=sys.stderr)
        return False
    if not has_text:
        print(
            "ERROR: Clipboard has no text format; capture skipped to preserve data",
            file=sys.stderr,
        )
        return False
    return True


def clear_clipboard_history_best_effort():
    """Clear current clipboard contents without affecting pinned history."""
    if win32clipboard is None:
        return False
    delay = CLIPBOARD_CLEAR_DELAY
    for _ in range(CLIPBOARD_CLEAR_RETRIES):
        try:
            win32clipboard.OpenClipboard()
            try:
                win32clipboard.EmptyClipboard()
            finally:
                win32clipboard.CloseClipboard()
            return True
        except Exception:
            try:
                win32clipboard.CloseClipboard()
            except Exception:
                pass
            time.sleep(delay)
            delay *= 2

    print("Failed to clear clipboard after retries", file=sys.stderr)
    return False


def _open_clipboard(retries=10, delay=0.01):
    for _ in range(retries):
        if _user32.OpenClipboard(None):
            return True
        time.sleep(delay)
    return False


def get_clipboard_text():
    """
    Returns (ok, text, has_text_format).
    has_text_format is False when CF_UNICODETEXT is not available.
    """
    if not _open_clipboard():
        return False, None, False
    try:
        has_text = bool(_user32.IsClipboardFormatAvailable(CF_UNICODETEXT))
        if not has_text:
            return True, None, False
        handle = _user32.GetClipboardData(CF_UNICODETEXT)
        if not handle:
            return False, None, True
        ptr = _kernel32.GlobalLock(handle)
        if not ptr:
            return False, None, True
        try:
            text = ctypes.wstring_at(ptr)
        finally:
            _kernel32.GlobalUnlock(handle)
        return True, text, True
    finally:
        _user32.CloseClipboard()


def set_clipboard_text(text):
    if text is None:
        return False
    if not _open_clipboard():
        return False
    try:
        if not _user32.EmptyClipboard():
            return False
        size = (len(text) + 1) * ctypes.sizeof(ctypes.c_wchar)
        h_global = _kernel32.GlobalAlloc(GMEM_MOVEABLE, size)
        if not h_global:
            return False
        ptr = _kernel32.GlobalLock(h_global)
        if not ptr:
            _kernel32.GlobalFree(h_global)
            return False
        try:
            ctypes.memmove(ptr, ctypes.create_unicode_buffer(text), size)
        finally:
            _kernel32.GlobalUnlock(h_global)
        if not _user32.SetClipboardData(CF_UNICODETEXT, h_global):
            _kernel32.GlobalFree(h_global)
            return False
        return True
    finally:
        _user32.CloseClipboard()


def capture_narrator_last_spoken(allow_no_text_format=False, log_failure=True):
    """
    Trigger Narrator's copy last spoken hotkey and return captured text.
    Requires Narrator to be running. When allow_no_text_format is False,
    clipboard text format must be available.
    Returns None on failure (logs to stderr).
    """
    ok, original_text, has_text = get_clipboard_text()
    if not ok:
        print("ERROR: Clipboard unavailable for Narrator capture", file=sys.stderr)
        return None
    if not has_text and not allow_no_text_format:
        print(
            "ERROR: Clipboard has no text format; capture skipped to preserve data",
            file=sys.stderr,
        )
        return None
    had_text_format = has_text

    sentinel = f"__NARRATOR_CAPTURE_{int(time.time() * 1000)}__"
    if not set_clipboard_text(sentinel):
        print("ERROR: Failed to set clipboard sentinel", file=sys.stderr)
        return None

    def _capture_with_key(narrator_vk):
        send_key_chord([narrator_vk, VK_CONTROL, VK_X])
        time.sleep(NARRATOR_CLIPBOARD_DELAY)
        ok_new, new_text, has_text_new = get_clipboard_text()
        if not ok_new or not has_text_new:
            return None
        if new_text == sentinel:
            return None
        new_text = _strip_narrator_confirmation(new_text)
        if not new_text:
            return None
        return new_text

    text = None
    try:
        text = _capture_with_key(VK_CAPITAL)
    finally:
        if had_text_format and original_text is not None:
            if not set_clipboard_text(original_text):
                print("Failed to restore clipboard text", file=sys.stderr)

    if text is None and log_failure:
        print("ERROR: Narrator speech capture failed", file=sys.stderr)
    return text


def _strip_narrator_confirmation(text):
    if not text:
        return text
    lines = [line.strip() for line in text.splitlines()]
    blocked = {
        "copied last phrase to clipboard",
        "failed to copy to clipboard",
    }
    filtered = [line for line in lines if line and line.lower() not in blocked]
    return "\n".join(filtered)


def copy_narrator_last_spoken():
    """
    Trigger Narrator's copy last spoken hotkey and return captured text.
    Returns None on failure (logs to stderr).
    """
    auto_enabled = prepare_narrator_capture_session()
    if auto_enabled is None:
        return None

    try:
        if not preflight_clipboard_text_format():
            return None
        return capture_narrator_last_spoken()
    finally:
        restore_narrator_capture_session(auto_enabled)


def try_capture_narrator_last_spoken(allow_no_text_format=False, log_failure=True):
    """
    Capture Narrator text only if Narrator is already running.
    Returns None if Narrator is off or capture fails.
    """
    if not is_narrator_running():
        return None
    if not allow_no_text_format:
        if not preflight_clipboard_text_format():
            return None
    return capture_narrator_last_spoken(
        allow_no_text_format=allow_no_text_format,
        log_failure=log_failure,
    )
