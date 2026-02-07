## Why

NarratorText capture can still be noisy or mismatched due to copy failures and timing lag, which makes tab output unreliable for comparison. We need better capture stability, cleanup, and clear mismatch warnings.

## What Changes

- Add short delay + retry to reduce Narrator capture lag.
- Filter out Narrator failure messages like "Failed to copy to clipboard" from captured text.
- When NarratorText does not match the element, emit mismatch warnings instead of silently using it.

## Capabilities

### New Capabilities
- `narrator-capture-stability`: Improve Narrator capture reliability and mismatch visibility.

### Modified Capabilities
- (none)

## Impact

- TalkBackAutoTest/module/pc_automation_clipboard.py capture post-processing.
- TalkBackAutoTest/module/pc_automation_cli.py timing and mismatch logging.
- TalkBackAutoTest/module/pc_automation_uia.py element-to-text association.