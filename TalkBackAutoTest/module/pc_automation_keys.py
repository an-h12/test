import ctypes
from ctypes import wintypes
import time


class KEYBDINPUT(ctypes.Structure):
    """Keyboard input structure for SendInput"""

    _fields_ = [
        ("wVk", wintypes.WORD),           # Virtual Key Code
        ("wScan", wintypes.WORD),         # Hardware scan code
        ("dwFlags", wintypes.DWORD),      # Flags (0 = press, KEYEVENTF_KEYUP = release)
        ("time", wintypes.DWORD),         # Timestamp (0 = use system)
        ("dwExtraInfo", ctypes.POINTER(ctypes.c_ulong)),
    ]


class INPUT(ctypes.Structure):
    """Input structure for SendInput"""

    _fields_ = [
        ("type", wintypes.DWORD),         # INPUT_KEYBOARD = 1
        ("ki", KEYBDINPUT),
        ("padding", ctypes.c_ubyte * 8),  # Padding for structure alignment
    ]


VK_TAB = 0x09           # Tab key
VK_RETURN = 0x0D        # Enter key
VK_CONTROL = 0x11       # Ctrl key
VK_LWIN = 0x5B          # Left Windows key
VK_ESCAPE = 0x1B        # Escape key
VK_CAPITAL = 0x14       # Caps Lock key (Narrator key)
VK_INSERT = 0x2D        # Insert key (Narrator key)
VK_X = 0x58             # X key

# Key event flags
KEYEVENTF_KEYUP = 0x0002


def send_key_event(vk_code, is_key_up=False):
    """
    Send keyboard event using Windows SendInput API

    """
    # Create INPUT structure
    inputs = INPUT()
    inputs.type = 1  # INPUT_KEYBOARD
    inputs.ki.wVk = vk_code
    inputs.ki.wScan = 0
    inputs.ki.dwFlags = KEYEVENTF_KEYUP if is_key_up else 0
    inputs.ki.time = 0
    inputs.ki.dwExtraInfo = None

    # Call SendInput
    result = ctypes.windll.user32.SendInput(1, ctypes.byref(inputs), ctypes.sizeof(inputs))
    return result


def send_key_chord(vk_codes, hold_time=0.05):
    """
    Send a key chord: press all keys in order, wait, then release in reverse.
    """
    for vk_code in vk_codes:
        send_key_event(vk_code, is_key_up=False)
    time.sleep(hold_time)
    for vk_code in reversed(vk_codes):
        send_key_event(vk_code, is_key_up=True)


def toggle_narrator(log_to_stdout=True):
    """
    Toggle Windows Narrator ON/OFF using Ctrl + Win + Enter shortcut

    """
    # Press keys in sequence: Ctrl -> Win -> Enter
    send_key_event(VK_CONTROL, is_key_up=False)

    send_key_event(VK_LWIN, is_key_up=False)

    send_key_event(VK_RETURN, is_key_up=False)

    # Hold for system to recognize key combination
    time.sleep(0.1)

    # Release keys in reverse order: Enter -> Win -> Ctrl
    send_key_event(VK_RETURN, is_key_up=True)
    send_key_event(VK_LWIN, is_key_up=True)
    send_key_event(VK_CONTROL, is_key_up=True)

    if log_to_stdout:
        print("toggled ")
    return True


def press_tab():
    """
    Press Tab key one time for UI navigation
    """
    # Press Tab
    send_key_event(VK_TAB, is_key_up=False)

    time.sleep(0.05)  # Short delay while key is held

    # Release Tab
    send_key_event(VK_TAB, is_key_up=True)

    time.sleep(0.3)  # Delay after Tab press
    return True


def is_escape_pressed():
    """
    Returns True if ESC is pressed
    """
    state = ctypes.windll.user32.GetAsyncKeyState(VK_ESCAPE)

    return (state & 0x8000) != 0 or (state & 0x0001) != 0
