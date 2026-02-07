## Why

NarratorText currently includes the confirmation line "Copied last phrase to clipboard" and can lag behind the focused element, which makes the output noisy and sometimes incorrect for the element currently in focus. We need clean, synchronized Narrator text per element.

## What Changes

- Filter out the "Copied last phrase to clipboard" line from captured Narrator text.
- Adjust the capture ordering so that tabbing focuses an element before clipboard capture.
- Keep output format unchanged aside from cleaned NarratorText content.

## Capabilities

### New Capabilities
- `narrator-clipboard-cleanup`: Define sanitized Narrator clipboard capture and correct capture ordering per element.

### Modified Capabilities
- (none)

## Impact

- TalkBackAutoTest/module/pc_automation_clipboard.py capture post-processing.
- TalkBackAutoTest/module/pc_automation_cli.py tab sequencing of capture vs focus.
- TalkBackAutoTest/module/pc_automation_uia.py NarratorText injection timing.