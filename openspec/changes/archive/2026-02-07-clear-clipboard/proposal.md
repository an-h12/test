## Why

Clipboard history accumulates non-pinned entries during Narrator capture, which can leave stale data and confuse later comparisons. We need a reliable way to clear non-pinned clipboard history after comparison so capture stays aligned.

## What Changes

- Add a clear-clipboard step using Windows clipboard APIs that preserves pinned items.
- Invoke clear after comparing element attributes with NarratorText.
- Log clear attempts and failures to stderr for visibility.

## Capabilities

### New Capabilities
- `all-specs`: Document clipboard clear behavior and when it executes.

### Modified Capabilities
- `all-specs`: Add requirements for clearing clipboard history while preserving pinned entries and sequencing after comparison.

## Impact

- Affects clipboard handling in `TalkBackAutoTest/module/pc_automation_clipboard.py` and tab flow in `TalkBackAutoTest/module/pc_automation_cli.py`.
- Requires Windows clipboard APIs (pywin32 or win32clipboard) if not already available.
