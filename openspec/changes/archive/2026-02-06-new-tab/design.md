## Context

The current tab flow performs Narrator auto-toggle and clipboard checks for every element via `get_focused_element_info`, and only prints after each Tab press. This adds per-element latency and omits the initial focused element from output. The new flow should preflight once, include the current element, and stop on RuntimeId cycles without printing duplicates.

## Goals / Non-Goals

**Goals:**
- Preflight Narrator and clipboard once per tab sequence.
- Print the currently focused element before the first Tab.
- Stop traversal when RuntimeId repeats, without printing the duplicate.
- Continue tabbing when clipboard preflight fails, but set `narrator_text = None`.

**Non-Goals:**
- Changing CLI command surface or adding flags.
- Modifying JSON schema or field order beyond the extra initial line.
- Changing key chord implementation or UIA selection logic.

## Decisions

- Split clipboard capture into session/preflight/capture steps so `tab` can manage Narrator once per loop.
- Add a `narrator_text` override to `get_focused_element_info` to avoid internal auto-capture when the loop supplies text (or None).
- Keep `get_focused` behavior unchanged by using the existing `copy_narrator_last_spoken` wrapper.

## Risks / Trade-offs

- **stdout changes for `tab`** ? Accept output change and update tests/specs accordingly.
- **Preflight failure still allows tab loop** ? Use `None` narrator_text consistently to avoid misleading data.

## Migration Plan

- Update code in place; adjust tests/docs if they assert tab output line count/order.
- Rollback by reverting `pc_automation_cli.py`, `pc_automation_uia.py`, and `pc_automation_clipboard.py` changes.

## Open Questions

- None.