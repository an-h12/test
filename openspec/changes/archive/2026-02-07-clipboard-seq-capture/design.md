## Context

Narrator capture currently writes a sentinel string to the clipboard to detect whether the copy hotkey succeeded. These sentinel writes are recorded in Windows clipboard history, creating noisy entries and occasionally confusing downstream checks. We need a detection mechanism that avoids writing to the clipboard while keeping capture reliability and existing clear behavior.

## Goals / Non-Goals

**Goals:**
- Detect successful Narrator clipboard copy without writing a sentinel.
- Use `GetClipboardSequenceNumber()` to wait for clipboard changes after the hotkey.
- Read clipboard only after a sequence change, validate content, and retry once if needed.
- Keep current per-capture clear behavior to stabilize subsequent iterations.
- Preserve stdout JSON contract and stderr logging patterns.

**Non-Goals:**
- Clearing or disabling Windows clipboard history.
- Changing CLI flags or output formats.
- Altering the existing clear behavior or pywin32 fallback logic.

## Decisions

- **Use `GetClipboardSequenceNumber()` for detection** instead of a sentinel string. This avoids injecting artificial text into clipboard history.
- **Poll for sequence change with a short timeout** after sending the Narrator hotkey; if the sequence does not change, treat capture as failed.
- **Validate captured text** (non-empty and not a confirmation/failure line) and **retry once** if the first attempt is empty or mismatched.
- **Keep clipboard restoration behavior** as currently implemented (restore original text after reading) and rely on the tab-loop clear step to ensure stability between iterations.

## Risks / Trade-offs

- **[Other apps change clipboard]** Could cause false positives ? Mitigation: short polling window + content validation + existing mismatch checks.
- **[Clipboard contention]** Clipboard may be locked briefly ? Mitigation: reuse existing open/retry logic and non-fatal failures.
- **[Timing sensitivity]** Narrator copy may be slow on some machines ? Mitigation: configurable polling/timeout constants and single retry.

## Migration Plan

- No data migration required.
- Rollback by restoring sentinel-based detection if needed.

## Open Questions

- None.
