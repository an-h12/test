## 1. Clipboard sequence detection

- [x] 1.1 Add `GetClipboardSequenceNumber` binding (user32) in `pc_automation_clipboard.py`
- [x] 1.2 Implement helper to wait for sequence change with polling/timeout

## 2. Capture logic update

- [x] 2.1 Remove sentinel write from `capture_narrator_last_spoken`
- [x] 2.2 Use sequence-change helper to decide when to read clipboard text
- [x] 2.3 Retry capture once if the first attempt yields no valid text
- [x] 2.4 Keep clipboard restore behavior and existing clear logic unchanged

## 3. Docs & verification

- [x] 3.1 Update README_DEV.txt notes to reflect sequence-based capture (no sentinel writes)
- [ ] 3.2 Manual check: run `python pc_automation.py tab` and confirm no sentinel entries in clipboard history
