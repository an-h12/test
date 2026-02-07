## Why

Sentinel strings written to the clipboard during Narrator capture are being recorded in Windows clipboard history, creating noisy entries and causing occasional warnings/mis-capture. We need a capture mechanism that avoids writing sentinel text while keeping detection reliable.

## What Changes

- Stop writing sentinel text in `capture_narrator_last_spoken`.
- Use `GetClipboardSequenceNumber()` to wait for clipboard updates after the Narrator hotkey.
- Read clipboard only after the sequence number changes, validate content, and retry once if needed.
- Keep the current per-capture clear behavior for stability between iterations.
- Update specs/design/tasks to reflect the new capture mechanism.

## Capabilities

### New Capabilities
- (none)

### Modified Capabilities
- `all-specs`: update requirements for Narrator clipboard capture to use sequence-number detection and avoid sentinel writes, while keeping clear behavior unchanged.

## Impact

- Affects capture logic in `TalkBackAutoTest/module/pc_automation_clipboard.py`.
- May require small adjustments in `TalkBackAutoTest/module/pc_automation_cli.py` if validation/retry handling changes.
- Updates OpenSpec artifacts (specs/design/tasks).
