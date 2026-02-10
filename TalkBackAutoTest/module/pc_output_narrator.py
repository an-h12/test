import ctypes
from ctypes import wintypes
import os
import subprocess
import sys
import time

try:
    import psutil # type: ignore
except ImportError:
    psutil = None

try:
    from winrt.windows.applicationmodel.datatransfer import Clipboard as WinrtClipboard # type: ignore
except ImportError:
    WinrtClipboard = None


from pc_keys import (
    VK_CAPITAL,
    VK_CONTROL,
    VK_X,
    send_key_chord,
    toggle_narrator,
)


# region Constants & Win32 API Setup

CF_UNICODETEXT = 13
GMEM_MOVEABLE = 0x0002
SPI_GETSCREENREADER = 0x0046

NARRATOR_CLIPBOARD_DELAY = 0.2
NARRATOR_TOGGLE_DELAY = 0.4
NARRATOR_TOGGLE_RETRIES = 4
CLIPBOARD_SEQUENCE_TIMEOUT = 0.5
CLIPBOARD_SEQUENCE_POLL_DELAY = 0.01
CLIPBOARD_OPEN_RETRIES = 10
CLIPBOARD_OPEN_DELAY = 0.01

_user32 = ctypes.windll.user32
_kernel32 = ctypes.windll.kernel32

_user32.GetClipboardData.restype = wintypes.HANDLE
_user32.GetClipboardData.argtypes = [wintypes.UINT]
_user32.SetClipboardData.restype = wintypes.HANDLE
_user32.SetClipboardData.argtypes = [wintypes.UINT, wintypes.HANDLE]
_user32.GetClipboardSequenceNumber.restype = wintypes.DWORD
_user32.GetClipboardSequenceNumber.argtypes = []
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

# endregion


# region Narrator Process Management

def IsNarratorRunning():
    """Matches C# PCTB.IsNarratorRunning() - Check if Narrator is running"""
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
        if IsNarratorRunning() == expected_running:
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
            subprocess.Popen([candidate], stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
            return True
        except Exception:
            continue
    
    print("Failed to start Narrator.exe directly", file=sys.stderr)
    return False


def _ensure_narrator_on():
    if IsNarratorRunning():
        return False
    
    print("INFO: Narrator is off; auto-enabling for capture", file=sys.stderr)
    toggle_narrator()
    
    if _wait_for_narrator_state(True):
        return True
    if _start_narrator_process() and _wait_for_narrator_state(True):
        return True
    
    print("ERROR: Auto-toggle Narrator failed", file=sys.stderr)
    return None


def _restore_narrator_if_needed(auto_enabled):
    if not auto_enabled:
        return
    
    toggle_narrator()
    if _wait_for_narrator_state(False):
        print("Narrator restored to previous state", file=sys.stderr)
    else:
        print("Failed to restore Narrator state", file=sys.stderr)

# endregion


# region Session Management

def prepare_narrator_capture_session():
    """Ensure Narrator is running for a capture session."""
    return _ensure_narrator_on()


def restore_narrator_capture_session(auto_enabled):
    _restore_narrator_if_needed(auto_enabled)

# endregion


# region Clipboard Operations

def preflight_clipboard_text_format():
    ok, _, has_text = get_clipboard_text()
    if not ok:
        print("ERROR: Clipboard unavailable for Narrator capture", file=sys.stderr)
        return False
    return has_text


def clear_clipboard_history():
    """Clear clipboard history (Win+V) without affecting pinned items."""
    if WinrtClipboard is None:
        print("WARNING: winrt unavailable; clipboard history not cleared", file=sys.stderr)
        return False
    
    try:
        result = WinrtClipboard.clear_history()
    except Exception:
        print("WARNING: Failed to clear clipboard history", file=sys.stderr)
        return False
    
    if result is False:
        print("WARNING: Clipboard history clear returned False", file=sys.stderr)
        return False
    return True


def _open_clipboard(retries=CLIPBOARD_OPEN_RETRIES, delay=CLIPBOARD_OPEN_DELAY):
    for _ in range(retries):
        if _user32.OpenClipboard(None):
            return True
        time.sleep(delay)
    return False


def _get_clipboard_sequence_number():
    return _user32.GetClipboardSequenceNumber()



def _wait_for_clipboard_sequence_change(seq_before):
    if seq_before is None:
        return False
    deadline = time.time() + CLIPBOARD_SEQUENCE_TIMEOUT
    while time.time() < deadline:
        if _user32.GetClipboardSequenceNumber() != seq_before:
            return True
        time.sleep(CLIPBOARD_SEQUENCE_POLL_DELAY)
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

# endregion


# region Narrator Speech Capture

def capture_narrator_last_spoken(allow_no_text_format=False, log_failure=True):
    """Trigger Narrator's copy hotkey and return captured text."""
    ok, original_text, has_text = get_clipboard_text()
    if not ok:
        print("ERROR: Clipboard unavailable for Narrator capture", file=sys.stderr)
        return None
    if not has_text and not allow_no_text_format:
        return None
    
    text = _try_narrator_capture()
    _restore_original_text(has_text, original_text)
    
    if text is None and log_failure:
        print("DEBUG: Narrator speech capture returned empty", file=sys.stderr)
    return text


def _try_narrator_capture():
    text = _do_narrator_capture(VK_CAPITAL)
    if text is None:
        time.sleep(NARRATOR_CLIPBOARD_DELAY)
        text = _do_narrator_capture(VK_CAPITAL)
    return text


def _restore_original_text(had_text_format, original_text):
    if had_text_format and original_text is not None:
        if not set_clipboard_text(original_text):
            print("Failed to restore clipboard text", file=sys.stderr)


def _do_narrator_capture(narrator_vk):
    seq_before = _get_clipboard_sequence_number()
    send_key_chord([narrator_vk, VK_CONTROL, VK_X])
    
    if not _wait_for_clipboard_sequence_change(seq_before):
        return None
    
    ok, text, has_text = get_clipboard_text()
    if not ok or not has_text:
        return None
    
    return _strip_narrator_confirmation(text)


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

# endregion


# region Public API

def getNarratorOutput():
    """Matches C# PCTB.getNarratorOutput() - Capture Narrator text with auto-toggle if needed."""
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
    """Capture if Narrator is already running."""
    if not IsNarratorRunning():
        return None
    if not allow_no_text_format and not preflight_clipboard_text_format():
        return None
    return capture_narrator_last_spoken(allow_no_text_format, log_failure)

# endregion


# region Backward Compatibility

is_narrator_running = IsNarratorRunning
haveNarratorProcess = IsNarratorRunning
copy_narrator_last_spoken = getNarratorOutput

# endregion
