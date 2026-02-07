## Why

Rehear only replays the last spoken element without generating a new log line, which makes clipboard capture retries misleading and adds latency without improving accuracy. Removing rehear and the Insert fallback makes capture behavior deterministic and avoids false “success” when the log is unchanged.

## What Changes

- Remove rehear retry logic from Narrator clipboard capture.
- Capture uses Caps Lock + Ctrl + X only (drop Insert fallback).
- Error messaging reflects a single capture attempt.

## Capabilities

### New Capabilities
- `narrator-clipboard-capture`: Define the expected single-attempt Narrator capture flow and its failure behavior.

### Modified Capabilities
- (none)

## Impact

- TalkBackAutoTest/module/pc_automation_clipboard.py capture flow and logging.
- Any tests or scripts relying on Insert fallback or rehear retries.