## Why

The current tab flow checks Narrator and clipboard on every element and only prints after a Tab press, which adds latency and misses the currently focused element. We want a deterministic, single-session Narrator flow and to include the current element before traversal while stopping cleanly on RuntimeId cycles.

## What Changes

- Preflight Narrator and clipboard once for the whole tab loop.
- Include the currently focused element in output before the first Tab.
- Stop traversal when RuntimeId repeats, without printing the duplicate element.
- When clipboard preflight fails, continue tabbing with `narrator_text = None`.

## Capabilities

### New Capabilities
- `tab-sequence-preflight`: Define preflight and traversal order for the tab sequence.

### Modified Capabilities
- (none)

## Impact

- TalkBackAutoTest/module/pc_automation_cli.py tab flow and stdout sequence.
- TalkBackAutoTest/module/pc_automation_uia.py element info gathering (narrator text override).
- TalkBackAutoTest/module/pc_automation_clipboard.py capture session split for loop-wide preflight.
- Any tests that assume tab output starts after the first Tab press.