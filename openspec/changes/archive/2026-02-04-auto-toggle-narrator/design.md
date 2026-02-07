## Context

Narrator clipboard capture uses the Narrator hotkey. When Narrator is off, the hotkey produces no clipboard output, so `NarratorText` is always empty and demos/tests are unreliable. We need an automatic way to ensure Narrator is available during capture while keeping side effects minimal.

## Goals / Non-Goals

**Goals:**
- Detect when Narrator is off before attempting clipboard capture.
- Auto-toggle Narrator on for capture when needed.
- Optionally restore the previous Narrator state after capture.
- Keep CLI output contract unchanged (JSON schema and ordering).

**Non-Goals:**
- Speech Recap UIA parsing or audio/STT capture.
- New CLI flags or configuration options for this change (future work).
- Changing existing JSON fields or reordering output.

## Decisions

- **Detect Narrator state by process:** check for `Narrator.exe` process presence.
  - **Why:** Simple and reliable across Windows 10/11 without extra dependencies.
  - **Alternative:** Query Accessibility settings or UIA. More complex and less consistent.

- **Auto-toggle only when off:** call existing toggle hotkey (Ctrl+Win+Enter) if Narrator is not running.
  - **Why:** Minimizes side effects and keeps behavior predictable.

- **Restore prior state:** if we started Narrator, toggle it off after capture completes.
  - **Why:** Avoid leaving Narrator enabled unexpectedly after CLI runs.

- **Log status to stderr:** emit a concise message when auto-toggle happens and when restore occurs.
  - **Why:** Makes the behavior observable without changing stdout contract.

## Risks / Trade-offs

- **Process detection false negatives** ? **Mitigation:** retry detection after short delay post-toggle.
- **Toggle hotkey blocked by app** ? **Mitigation:** log failure and proceed without `NarratorText`.
- **Side effects from toggling** ? **Mitigation:** only toggle when off and restore state immediately after capture.

## Migration Plan

- No data migration.
- Deploy by updating Narrator capture flow to include state check and auto-toggle.
- Rollback by removing auto-toggle logic and leaving capture behavior unchanged.

## Open Questions

- Should we add a CLI/config option to disable auto-toggle for environments where toggling is not allowed?
- Should we cache Narrator state across `tab` loops to avoid repeated toggles?
