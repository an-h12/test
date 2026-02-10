## Purpose

Consolidated OpenSpec requirements for the project. All requirements live in this single capability to keep specs in one file.

## Requirements

### Capability: Narrator auto-toggle

#### Requirement: Auto-enable Narrator for clipboard capture
The system SHALL detect whether Narrator is running before attempting clipboard capture and SHALL auto-toggle Narrator on when it is off.

##### Scenario: Narrator is off
- **WHEN** a clipboard capture is requested and Narrator is not running
- **THEN** the system toggles Narrator on before sending the copy-hotkey

##### Scenario: Narrator is already on
- **WHEN** a clipboard capture is requested and Narrator is running
- **THEN** the system does not toggle Narrator and proceeds with capture

#### Requirement: Restore Narrator state after capture
The system SHALL restore Narrator to its prior state after capture completes if it was auto-enabled.

##### Scenario: Narrator was auto-enabled
- **WHEN** capture completes after auto-enabling Narrator
- **THEN** the system toggles Narrator off to restore the previous state

#### Requirement: Preserve CLI output contract
The system SHALL keep stdout JSON schema and field order unchanged while performing auto-toggle logic.

##### Scenario: CLI output unchanged
- **WHEN** running `get_focused` or `tab`
- **THEN** stdout JSON structure and field order remain identical to current behavior

#### Requirement: Log auto-toggle events to stderr
The system SHALL emit concise status messages to stderr when auto-toggle or restore actions occur.

##### Scenario: Auto-toggle logged
- **WHEN** the system toggles Narrator on or off automatically
- **THEN** a status message is written to stderr without changing stdout

### Capability: Narrator clipboard capture

#### Requirement: Single-attempt Narrator clipboard capture
The system SHALL attempt Narrator clipboard capture using Caps Lock + Ctrl + X only and SHALL NOT invoke rehear or Insert fallback paths.

##### Scenario: Capture requested
- **WHEN** Narrator clipboard capture is requested
- **THEN** the system sends the Caps Lock + Ctrl + X chord once and does not send rehear or Insert-based chords

#### Requirement: Failure returns None and restores clipboard
The system SHALL return None when the clipboard sequence number does not change after the capture hotkey is sent within a short timeout, and SHALL attempt to restore the original clipboard text.

##### Scenario: Clipboard sequence does not change
- **WHEN** the Narrator capture hotkey is sent and the clipboard sequence number does not change within the timeout
- **THEN** the system returns None and restores the original clipboard contents

#### Requirement: Detect clipboard update via sequence number
The system SHALL read the clipboard sequence number before sending the Narrator capture hotkey and SHALL wait for it to change before reading clipboard text.

##### Scenario: Capture attempt starts
- **WHEN** a Narrator capture attempt begins
- **THEN** the system waits for the clipboard sequence number to change before reading clipboard text

#### Requirement: Do not write sentinel text during capture
The system SHALL NOT write a sentinel string to the clipboard as part of Narrator capture detection.

##### Scenario: Capture in progress
- **WHEN** Narrator capture is performed
- **THEN** the system does not write a sentinel string to the clipboard

### Capability: PC automation modularization

#### Requirement: Preserve CLI output contract
The system SHALL preserve the existing CLI output contract for `get_focused`, `tab`, and `narrator`, including stdout/stderr strings, exit codes, JSON schema, and JSON field order.

##### Scenario: CLI output remains identical
- **WHEN** running existing CLI commands used by tests
- **THEN** the output lines and exit codes match the current behavior exactly

#### Requirement: Provide pure-function core API
The system SHALL expose a pure-function core that assembles element info in the same field order and schema as today, without performing I/O or timing.

##### Scenario: Core builds ordered JSON data
- **WHEN** the core is invoked with equivalent input data
- **THEN** it returns a dict with the same ordered fields as the current output

#### Requirement: Split I/O adapters by concern
The system SHALL place UIA access, clipboard access, and keyboard input in separate adapter modules under `TalkBackAutoTest/module`.

##### Scenario: Adapter boundaries are separated
- **WHEN** reviewing module layout
- **THEN** UIA, clipboard, and keyboard logic are in distinct files

#### Requirement: Keep CLI compatibility via stub
The system SHALL keep `pc_automation.py` as a compatibility stub that invokes the new CLI wiring without changing behavior.

##### Scenario: Legacy entrypoint still works
- **WHEN** invoking `pc_automation.py` as before
- **THEN** CLI behavior and outputs are unchanged

### Capability: Tab sequence preflight

#### Requirement: Preflight Narrator and clipboard once per tab sequence
The system SHALL check Narrator state and clipboard text format once before starting the tab traversal and SHALL keep the same session for the full loop.

##### Scenario: Preflight succeeds
- **WHEN** the tab sequence starts
- **THEN** the system ensures Narrator is available and verifies clipboard text format once before any Tab press

#### Requirement: Include current focused element before first Tab
The system SHALL output the currently focused element before sending the first Tab press.

##### Scenario: Initial focused element printed
```markdown
## Purpose

Consolidated project specs: combine current delta specs into a single authoritative file for PC automation. This file merges capabilities from `narrator-force-read`, `pc-automation-clean-code`, and `python-module-naming` while removing duplicates.

---

## Capability: CLI contract & modularization

- Requirement: Single canonical CLI entrypoint
	- The primary CLI entrypoint SHALL be `pc_automation.py` under `TalkBackAutoTest/module` to match C# subprocess integration.

- Requirement: Preserve CLI output contract
	- `get_focused`, `narrator`, and `tab` actions SHALL keep existing stdout/stderr formats and exit codes.

- Requirement: Module naming and adapters
	- Helper modules SHALL follow `pc_<name>.py` naming (e.g., `pc_uia.py`, `pc_clipboard.py`, `pc_keys.py`, `pc_element_info.py`).
	- UIA access, clipboard access, and keyboard input SHALL live in separate adapter modules.

---

## Capability: Force Narrator Read (narrator-force-read)

- Requirement: Force Narrator to read current element
	- The system SHALL provide a function to trigger Windows Narrator to read the currently focused element without changing focus.

- Requirement: Key event simulation for force read
	- The function SHALL simulate the Narrator key (Caps Lock or Insert) + Tab using the Windows SendInput API and SHALL support a configurable hold time.

- Requirement: No focus change during force read
	- Forcing a read SHALL NOT change focus or navigation state.

---

## Capability: Initial element capture & tab sequence

- Requirement: Preflight Narrator and clipboard once per tab sequence
	- The system SHALL verify Narrator state and clipboard text format once before starting tab traversal.

- Requirement: Include current focused element before first Tab
	- When a tab sequence starts with Narrator available, the system SHALL force-read and output the initially focused element before any Tab press.

- Requirement: Track runtime id for cycle detection
	- The system SHALL add the initial element's runtime id to the seen set to allow correct cycle detection.

- Requirement: Stop on repeated RuntimeId
	- The system SHALL stop traversal when a RuntimeId repeats; it SHALL NOT emit a duplicate element line.

---

## Capability: Narrator clipboard capture & stability

- Requirement: Centralized single capture per element
	- Narrator capture SHALL happen only at the automation layer (e.g., `_maybe_capture_for_element()`), not inside element-info extraction functions.

- Requirement: Capture via clipboard hotkey(s)
	- The system SHALL trigger Narrator's copy-last-spoken hotkey and read the clipboard; it SHALL attempt primary key (Caps Lock) and MAY fall back to Insert if needed.

- Requirement: Delay and retry
	- After focus change, system SHALL wait briefly and retry capture once if needed; the existing short-retry behavior SHALL be preserved.

- Requirement: Strip confirmation/failure lines
	- The system SHALL remove lines like "Copied last phrase to clipboard" and "Failed to copy to clipboard" from captured text before emitting.

- Requirement: Mismatch detection
	- If captured NarratorText lacks element Name or LocalizedControlType, the system SHALL record mismatch labels (e.g., `Mismatch: Name`) and report them.

---

## Capability: Clipboard handling

- Requirement: Preserve and restore clipboard contents
	- The system SHALL save and restore user clipboard contents around capture operations.

- Requirement: Clear non-pinned clipboard history in tab loop
	- During the CLI `tab` loop the system MAY clear non-pinned clipboard history after comparison; pinned items SHALL be preserved. If platform APIs are unavailable, document fallback behavior.

- Requirement: Detect clipboard update via sequence number
	- Use the clipboard sequence number to detect updates and wait for a change before reading text.

---

## Capability: Backward compatibility & docs

- Requirement: Documentation reflects file names and entrypoint
	- `README_DEV.txt` and C# examples SHALL reference `pc_automation.py` and the `pc_*.py` naming convention.

- Requirement: Preserve existing behavior
	- Any change SHALL avoid breaking existing C# integration; document any behavioral differences (e.g., initial element now emitted).

---

## Notes on deduplication

- This merged file removes duplicated clipboard-clear and capture text paragraphs and consolidates overlapping requirements from multiple delta specs.
- For specifics, the original delta specs are archived in `openspec/changes/archive/2026-02-10-first-element/`.

``` 
The system SHALL emit JSON only after a Tab press changes focus and NarratorText is captured for that focused element.
