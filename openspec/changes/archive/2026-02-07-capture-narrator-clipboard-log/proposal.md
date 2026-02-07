## Why

We need a reliable way to capture the real Narrator speech output for each focused UI element during automated Windows UI testing. Today we only infer from UIA properties, which can drift from what Narrator actually speaks. Using Narrator's built-in "Copy last spoken" hotkey provides ground-truth text without audio/STT.

## What Changes

- Add Narrator speech capture via clipboard hotkeys (Narrator key + Ctrl + X).
- Integrate the captured speech text into `pc_automation.py` output for `get_focused` and `tab` runs.
- Add fallback behavior when clipboard capture fails (retry with alternate Narrator key, optional re-hear).
- Document usage, timing, and troubleshooting in `README_DEV.txt`.

## Capabilities

### New Capabilities
- `narrator-clipboard-log`: Capture Narrator's last spoken text via clipboard hotkey and emit it alongside UIA element info for comparison.

### Modified Capabilities
- (none)

## Impact

- `TalkBackAutoTest/module/pc_automation.py` (new hotkey handling + clipboard access + output field).
- `TalkBackAutoTest/module/README_DEV.txt` (usage and output documentation).
- Optional consumers of JSON output (if they want to read the new field).
- Requires Windows Narrator enabled and hotkeys available.
