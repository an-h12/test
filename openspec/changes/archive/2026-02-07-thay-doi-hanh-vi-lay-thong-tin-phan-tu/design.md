## Context

NarratorText capture can still be mismatched with the focused element due to timing lag and clipboard failures. Errors like "Failed to copy to clipboard" leak into NarratorText and degrade output quality.

## Goals / Non-Goals

**Goals:**
- Reduce capture lag with a short delay and single retry.
- Filter failure messages from captured NarratorText.
- Emit explicit mismatch warnings (Name/ControlType) when the captured text does not match the focused element.

**Non-Goals:**
- Changing key chords or narrator toggle behavior.
- Adding new CLI flags or output fields.
- Modifying non-tab commands beyond their current behavior.

## Decisions

- Add a small delay between focus and clipboard capture and allow one retry on mismatch/failure.
- Strip known failure messages ("Failed to copy to clipboard") during post-processing.
- If mismatch persists, keep NarratorText absent and log `Mismatch: Name` and/or `Mismatch: ControlType`.

## Risks / Trade-offs

- **Extra latency per element** ? Acceptable for accurate capture.
- **False negatives on mismatch** ? Use simple substring checks; can be refined if needed.

## Migration Plan

- Update `pc_automation_clipboard.py` and `pc_automation_cli.py` in place.
- Rollback by reverting those files if latency is unacceptable.

## Open Questions

- None.