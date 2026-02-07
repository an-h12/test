## Why

Narrator clipboard capture returns empty when Narrator is off, which blocks reliable CLI demos and automated checks. Auto-toggling Narrator when capture is requested removes that setup friction and makes the feature usable out of the box.

## What Changes

- Detect Narrator running state before clipboard capture.
- Auto-toggle Narrator on when capture is requested and Narrator is off.
- Optionally restore the prior Narrator state after capture to avoid side effects.
- Log clear status messages when auto-toggle occurs.

## Capabilities

### New Capabilities
- `narrator-auto-toggle`: Ensure Narrator is available for clipboard capture by auto-enabling it when needed, with optional restore behavior.

### Modified Capabilities
-

## Impact

- `TalkBackAutoTest/module` clipboard and CLI wiring.
- JSON output timing/behavior for `get_focused` and `tab` when Narrator is off.
- Documentation updates for auto-toggle behavior and safety notes.
