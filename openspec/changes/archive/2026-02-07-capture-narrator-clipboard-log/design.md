## Context

`pc_automation.py` currently uses UI Automation (UIA) to capture focused element properties and drive Tab navigation. There is no reliable way to read the actual Narrator speech output from the script. Windows Narrator provides built-in hotkeys to copy the last spoken text to the clipboard. We can use those hotkeys to capture the real speech string and attach it to the JSON output per focused element.

Constraints:
- Must work on Windows 10/11 with Narrator enabled.
- Narrator key can be Caps Lock, Insert, or both, depending on user settings.
- Clipboard access can be sensitive (may overwrite user clipboard).
- Timing matters: Narrator speech happens after focus changes.

## Goals / Non-Goals

**Goals:**
- Capture Narrator's last spoken text via clipboard hotkey (Narrator key + Ctrl + X).
- Support both Caps Lock and Insert as the Narrator key.
- Integrate captured speech into `get_focused` and `tab` JSON output (new `NarratorText`).
- Provide a minimal fallback strategy when clipboard capture fails.

**Non-Goals:**
- Speech-to-text or audio loopback capture.
- Full Narrator history parsing (Speech Recap UIA ingestion) as a requirement.
- Perfect semantic matching with UIA fields; we only provide raw spoken text.

## Decisions

1) **Use clipboard hotkey for ground-truth speech**
- **Choice:** Trigger Narrator's “Copy last spoken” (Narrator key + Ctrl + X) and read clipboard.
- **Alternative:** Speech Recap window parsing; audio/STT.
- **Why:** Clipboard is the simplest, local, and official path to the actual spoken string without external dependencies.

2) **Support both Narrator keys (Caps Lock and Insert)**
- **Choice:** Attempt Caps Lock first, then Insert if clipboard does not change.
- **Alternative:** Ask user to configure a single key.
- **Why:** Reduces setup friction and handles different Narrator settings.

3) **Clipboard preservation**
- **Choice:** Save current clipboard, attempt copy, read result, then restore.
- **Alternative:** Leave clipboard modified.
- **Why:** Avoids disrupting user data during automation runs.

4) **Minimal fallback strategy**
- **Choice:** If clipboard text does not update, issue “Re-hear last spoken” (Narrator key + X), then retry copy once.
- **Alternative:** No retry; or depend on Speech Recap.
- **Why:** Increases reliability without adding heavy UIA parsing logic.

5) **Expose speech as a new JSON field**
- **Choice:** Add `NarratorText` to the JSON output for each element.
- **Alternative:** Separate command or external log file.
- **Why:** Keeps correlation simple for automation that already parses JSON output.

## Risks / Trade-offs

- **Clipboard conflicts** ? Save/restore clipboard; log warning if clipboard access fails.
- **Timing sensitivity** ? Add small delays after Tab and after copy hotkey; make delays configurable constants.
- **Narrator hotkey blocked by some apps** ? Document limitations; log when repeated failures occur.
- **Narrator may speak extra system messages** ? Keep raw `NarratorText` and let comparison logic handle discrepancies.

## Migration Plan

- No data migration required.
- Deploy by updating `pc_automation.py` and documentation.
- Rollback: remove `NarratorText` field and hotkey/clipboard logic.

## Open Questions

- Should Speech Recap UIA parsing be added as an optional secondary capture path?
- Should we expose a flag to disable clipboard preservation if performance is an issue?
