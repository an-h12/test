## Context

Narrator clipboard capture currently retries using rehear and falls back to the Insert modifier if Caps Lock fails. Rehear replays the last spoken element but does not generate a new log line, so retries can return stale output while adding delay.

## Goals / Non-Goals

**Goals:**
- Make capture deterministic by removing rehear retries.
- Use a single capture attempt with Caps Lock + Ctrl + X only.
- Keep clipboard restore logic and error reporting intact.

**Non-Goals:**
- Changing Narrator auto-toggle behavior.
- Modifying key chord implementation or timing constants unrelated to capture.
- Altering stdout JSON contracts for CLI commands.

## Decisions

- Remove the `_rehear` helper and `NARRATOR_REHEAR_DELAY` constant to eliminate rehear retries.
- Drop `VK_INSERT` fallback so capture uses Caps Lock only.
- Keep the sentinel-based clipboard validation and restore flow to avoid corrupting user clipboard data.

## Risks / Trade-offs

- **Reduced capture resiliency on systems that rely on Insert** ? Caller can re-run capture or adjust Narrator settings; we keep a clear error log when capture fails.
- **Fewer retries may reduce success rate in transient failure cases** ? Preserve fast failure so higher-level logic can decide whether to retry.

## Migration Plan

- Update `pc_automation_clipboard.py` in place; no data migration required.
- Rollback by reverting the file to restore rehear and Insert fallback.

## Open Questions

- None.