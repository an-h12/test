## 1. Clipboard/Narrator Session Refactor

- [x] 1.1 Split clipboard capture into session/preflight/capture functions in `pc_automation_clipboard.py`
- [x] 1.2 Keep `copy_narrator_last_spoken()` as wrapper for one-off callers

## 2. UIA Element Info Override

- [x] 2.1 Add narrator_text override parameter to `get_focused_element_info()`
- [x] 2.2 Ensure override bypasses internal capture when provided (including None)

## 3. Tab Loop Update

- [x] 3.1 Preflight Narrator/clipboard once in `run_tab_sequence()`
- [x] 3.2 Emit current focused element before first Tab
- [x] 3.3 Stop on repeated RuntimeId without printing duplicate
- [x] 3.4 When preflight fails, continue with narrator_text = None

## 4. Documentation & Tests

- [x] 4.1 Update specs/docs for new tab sequence behavior
- [x] 4.2 Add/adjust tests or manual verification notes for tab output order
