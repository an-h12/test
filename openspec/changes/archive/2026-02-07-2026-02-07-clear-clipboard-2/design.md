## Context

Narrator capture relies on the system clipboard. During the tab loop, stale clipboard text can persist across iterations and cause incorrect comparisons. We need a deterministic cleanup step after each capture to keep automation stable, while preserving pinned clipboard items and not altering stdout JSON output.

## Goals / Non-Goals

**Goals:**
- Clear clipboard after each Narrator capture in the CLI tab loop, including capture failures.
- Preserve pinned clipboard history items (Win+V).
- Keep sentinel-based capture for reliability.
- Provide a fallback clear behavior when pywin32 is unavailable.
- Log clear failures to stderr without breaking the tab loop.

**Non-Goals:**
- Clearing pinned clipboard items.
- Changing get_focused behavior or other CLI actions.
- Introducing new CLI flags or changing stdout JSON format.

## Decisions

- **Keep sentinel capture** to reliably detect whether Narrator copy succeeded; alternatives like snapshot+timestamp were rejected due to lower reliability when text is unchanged.
- **Clear after each capture in the tab loop** (not only at the end) to prevent stale data from affecting subsequent iterations.
- **Use win32clipboard.EmptyClipboard when available** to clear current clipboard contents without affecting pinned history.
- **Fallback to setting a safe sentinel text** when pywin32 is missing, ensuring the clipboard is still clean and predictable.
- **Best-effort clearing**: failures are logged to stderr and do not interrupt the loop.

## Risks / Trade-offs

- **[Dependency availability]** pywin32 may be missing ? Mitigation: fallback to set sentinel text and log a warning.
- **[Clipboard contention]** other apps may lock the clipboard ? Mitigation: retry/backoff and treat failures as non-fatal.
- **[User clipboard impact]** clearing overrides clipboard content ? Mitigation: scope only to the CLI tab loop and document behavior.
