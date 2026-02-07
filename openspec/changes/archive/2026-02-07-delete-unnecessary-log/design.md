## Context

Narrator clipboard capture currently appends the confirmation line "Copied last phrase to clipboard" to the clipboard contents, and the capture timing can lag behind the currently focused element during tab traversal. This produces noisy NarratorText and mismatched element-to-text pairing.

## Goals / Non-Goals

**Goals:**
- Filter out the confirmation line from NarratorText.
- Capture NarratorText after the element focus is stable (tab first, then copy).
- Keep JSON output schema unchanged.

**Non-Goals:**
- Changing the Narrator key chord implementation.
- Adding new CLI flags or output fields.
- Altering non-Narrator clipboard usage.

## Decisions

- Add a lightweight post-processing filter to strip the confirmation line from captured clipboard text.
- Reorder capture in the tab loop so focus is acquired before invoking clipboard capture.
- Use existing `get_focused_element_info` override to inject NarratorText per element.

## Risks / Trade-offs

- **Potential language variation of the confirmation line** ? Keep a simple exact-match filter; update if other locales appear.
- **Slight additional delay** ? Acceptable to ensure focus/text alignment.

## Migration Plan

- Update `pc_automation_clipboard.py` and `pc_automation_cli.py` in place.
- Rollback by reverting those files if needed.

## Open Questions

- None.