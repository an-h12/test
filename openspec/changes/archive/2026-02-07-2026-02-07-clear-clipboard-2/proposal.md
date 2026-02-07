## Why

Clipboard text can persist between Narrator captures during the tab loop, causing stale data to leak into later comparisons. We need deterministic clearing after each capture to keep automation stable and predictable.

## What Changes

- Keep the sentinel-based capture to preserve reliability.
- Clear clipboard after each Narrator capture in the CLI tab loop (even on capture failure).
- If pywin32 is unavailable, fall back to setting a safe sentinel text to ensure the clipboard is clean.
- Log clear attempts/failures to stderr without changing stdout JSON.

## Capabilities

### New Capabilities
- (none)

### Modified Capabilities
- `all-specs`: add requirements for per-capture clipboard clearing in the tab loop, including fallback behavior when pywin32 is missing and handling on capture failure.

## Impact

- Affects clipboard handling in `TalkBackAutoTest/module/pc_automation_clipboard.py`.
- Affects tab flow in `TalkBackAutoTest/module/pc_automation_cli.py`.
- May update `TalkBackAutoTest/module/README_DEV.txt` to reflect new behavior.
