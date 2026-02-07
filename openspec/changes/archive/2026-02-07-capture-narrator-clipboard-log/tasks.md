## 1. Hotkey & Key Chord Support

- [x] 1.1 Add VK constants for CapsLock and Insert in `pc_automation.py`
- [x] 1.2 Implement a reusable key-chord helper (press/hold/release sequence)

## 2. Clipboard Capture Utilities

- [x] 2.1 Implement Win32 clipboard read/write helpers (ctypes) with error handling
- [x] 2.2 Add clipboard preservation (save before capture, restore after)
- [x] 2.3 Implement `copy_narrator_last_spoken()` with CapsLock then Insert retry and optional re-hear fallback

## 3. Integrate NarratorText into Output

- [x] 3.1 In `get_focused`, capture speech and add `NarratorText` when available
- [x] 3.2 In `run_tab_sequence`, capture speech per element with timing delays and stderr logging on failure

## 4. Documentation & Validation

- [x] 4.1 Update `README_DEV.txt` with NarratorText output and hotkey usage notes
- [ ] 4.2 Run manual checks on button/checkbox/edit/list-item to confirm clipboard capture
