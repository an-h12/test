## Context

Narrator capture relies on clipboard contents. Over time, non-pinned clipboard history items can accumulate and lead to stale data being read or compared. We need a deterministic cleanup step after element-vs-NarratorText comparison while preserving pinned items.

## Goals / Non-Goals

**Goals:**
- Clear non-pinned clipboard history after comparing element attributes with NarratorText.
- Preserve pinned clipboard items.
- Keep stdout JSON output unchanged; only log to stderr.

**Non-Goals:**
- Clearing pinned clipboard items.
- Changing get_focused behavior.
- Introducing new CLI flags for this change.

## Decisions

- **Use Win32 clipboard APIs (pywin32/win32clipboard)** to clear clipboard history while preserving pinned items. This aligns with the requirement to avoid affecting pinned entries and keeps the implementation Windows-native.
- **Execute clear after comparison** (post element attribute vs NarratorText check) to avoid interfering with capture or mismatch evaluation.
- **Best-effort clearing**: log failures to stderr but do not fail the tab sequence.

## Risks / Trade-offs

- **[Dependency availability]** ? Mitigation: detect pywin32 presence and log a warning if unavailable.
- **[Clipboard contention]** ? Mitigation: use retries with short backoff; treat failures as non-fatal.
- **[OS limitations]** ? Mitigation: keep clear logic gated to Windows; no-op elsewhere.
