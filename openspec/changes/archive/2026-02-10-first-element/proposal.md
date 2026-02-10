## Why

The `dumpScreen()` function skips the first focused element when starting narrator automation. When Narrator is toggled on at the start of a tab sequence, it doesn't read the initially focused element—only elements encountered after Tab keypresses are captured and logged. This creates incomplete test data and missing coverage for the first UI element in any automation session.

## What Changes

- Add Caps Lock/Insert key support to force Narrator to read the currently focused element
- Extract element info gathering without narrator capture (rename `dumpFocusedObject` → `getFocusedElementInfo`)
- Implement `_process_initial_element()` to capture and log the first focused element before tab loop
- Remove duplicate narrator text capture logic (currently happens twice per element)
- Consolidate narrator capture to single call per element in automation layer

## Capabilities

### New Capabilities
- `narrator-force-read`: Ability to trigger Narrator to read current element on demand using Caps Lock or Insert key

### Modified Capabilities
- `pc-automation-clean-code`: Fix element skipping bug and remove duplicate narrator capture architecture

## Impact

**Affected modules:**
- `pc_keys.py`: Add Caps Lock/Insert key constants and force-read function
- `pc_uia.py`: Rename and refactor `dumpFocusedObject()` to separate element info from narrator capture
- `pc_automation.py`: Add initial element processing, remove duplicate capture logic in `_process_tab_iteration()`
- `pc_element_info.py`: No changes (pure function remains unchanged)

**Behavioral changes:**
- First element in tab sequence will now be logged with narrator output
- NarratorText will only appear in outer wrapper (not inside Element dict)
- Single narrator capture per element instead of two attempts
