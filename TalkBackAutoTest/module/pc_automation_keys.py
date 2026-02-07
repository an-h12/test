import ctypes
from ctypes import wintypes
import time


class KEYBDINPUT(ctypes.Structure):
    """Keyboard input structure for SendInput."""

    _fields_ = [
        ("wVk", wintypes.WORD),
        ("wScan", wintypes.WORD),
        ("dwFlags", wintypes.DWORD),
        ("time", wintypes.DWORD),
        ("dwExtraInfo", ctypes.POINTER(ctypes.c_ulong)),
    ]


class INPUT(ctypes.Structure):
    """Input structure for SendInput."""

    _fields_ = [
        ("type", wintypes.DWORD),
        ("ki", KEYBDINPUT),
        ("padding", ctypes.c_ubyte * 8),
    ]


INPUT_KEYBOARD = 1

VK_TAB = 0x09
VK_RETURN = 0x0D
VK_CONTROL = 0x11
VK_LWIN = 0x5B
VK_ESCAPE = 0x1B
VK_CAPITAL = 0x14
VK_INSERT = 0x2D
VK_X = 0x58

KEYEVENTF_KEYUP = 0x0002

KEY_HOLD_DELAY = 0.05
TAB_POST_DELAY = 0.3
NARRATOR_TOGGLE_HOLD = 0.1


def send_key_event(vk_code, is_key_up=False):
    """Send a single keyboard event via SendInput."""
    input_packet = INPUT()
    input_packet.type = INPUT_KEYBOARD
    input_packet.ki.wVk = vk_code
    input_packet.ki.wScan = 0
    input_packet.ki.dwFlags = KEYEVENTF_KEYUP if is_key_up else 0
    input_packet.ki.time = 0
    input_packet.ki.dwExtraInfo = None

    return ctypes.windll.user32.SendInput(
        1, ctypes.byref(input_packet), ctypes.sizeof(input_packet)
    )


def send_key_chord(vk_codes, hold_time=KEY_HOLD_DELAY):
    """Send a key chord: press all keys in order, wait, then release in reverse."""
    for vk_code in vk_codes:
        send_key_event(vk_code, is_key_up=False)
    time.sleep(hold_time)
    for vk_code in reversed(vk_codes):
        send_key_event(vk_code, is_key_up=True)


def toggle_narrator(log_to_stdout=True):
    """Toggle Windows Narrator ON/OFF using Ctrl + Win + Enter."""
    send_key_event(VK_CONTROL)
    send_key_event(VK_LWIN)
    send_key_event(VK_RETURN)
    time.sleep(NARRATOR_TOGGLE_HOLD)
    send_key_event(VK_RETURN, is_key_up=True)
    send_key_event(VK_LWIN, is_key_up=True)
    send_key_event(VK_CONTROL, is_key_up=True)

    if log_to_stdout:
        print("toggled ")
    return True


def press_tab():
    """Press Tab once for UI navigation."""
    send_key_event(VK_TAB)
    time.sleep(KEY_HOLD_DELAY)
    send_key_event(VK_TAB, is_key_up=True)
    time.sleep(TAB_POST_DELAY)
    return True


def is_escape_pressed():
    """Return True if ESC is pressed."""
    state = ctypes.windll.user32.GetAsyncKeyState(VK_ESCAPE)
    return (state & 0x8000) != 0 or (state & 0x0001) != 0
