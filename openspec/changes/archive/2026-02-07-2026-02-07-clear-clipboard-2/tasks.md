## 1. Clipboard clear helper

- [x] 1.1 Add `CLIPBOARD_CLEAR_SENTINEL` constant in `pc_automation_clipboard.py`
- [x] 1.2 Extend `clear_clipboard_history` to accept optional fallback text and use `set_clipboard_text` when pywin32 is missing
- [x] 1.3 Ensure clear failures are logged to stderr without raising

## 2. Tab loop integration

- [x] 2.1 Invoke clipboard clear after each capture attempt in `_maybe_capture_for_element` regardless of success
- [x] 2.2 Pass fallback sentinel when clearing in the tab loop
- [x] 2.3 Keep other CLI actions (`get_focused`, `narrator`) unchanged

## 3. Docs & checks

- [x] 3.1 Update `README_DEV.txt` to describe per-capture clear and fallback behavior
- [x] 3.2 Manual check: run `python pc_automation.py tab` with/without pywin32 and verify clipboard handling
