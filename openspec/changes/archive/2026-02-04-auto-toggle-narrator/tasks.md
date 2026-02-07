## 1. Narrator State Detection

- [x] 1.1 Add a helper to detect whether Narrator.exe is running
- [x] 1.2 Add a small retry/backoff after toggle to confirm state

## 2. Auto-toggle Flow

- [x] 2.1 Integrate auto-toggle before clipboard capture when Narrator is off
- [x] 2.2 Restore Narrator state after capture if it was auto-enabled
- [x] 2.3 Emit concise stderr logs for auto-toggle and restore actions

## 3. Verification & Docs

- [x] 3.1 Manual CLI checks with Narrator off/on to confirm capture behavior
- [x] 3.2 Update README_DEV.txt with auto-toggle behavior notes
