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
The system SHALL return None when capture fails to replace the clipboard sentinel and SHALL attempt to restore the original clipboard text.

##### Scenario: Clipboard does not update
- **WHEN** the clipboard text remains the sentinel after the capture attempt
- **THEN** the system returns None and restores the original clipboard contents

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
- **WHEN** the tab sequence starts
- **THEN** the system prints the current focused element info prior to the first Tab action

#### Requirement: Stop on repeated RuntimeId without printing duplicate
The system SHALL stop the tab traversal when a RuntimeId repeats and SHALL NOT print the duplicate element.

##### Scenario: RuntimeId repeats
- **WHEN** a newly focused element has a RuntimeId already seen in the current sequence
- **THEN** the system stops the loop without emitting element info for the duplicate

#### Requirement: Continue tabbing when clipboard preflight fails
The system SHALL continue tab traversal when clipboard text format is unavailable and SHALL set narrator_text to None for all emitted elements.

##### Scenario: Clipboard text format unavailable
- **WHEN** clipboard preflight detects no text format
- **THEN** the system continues the sequence and outputs elements with narrator_text = None

### Capability: Narrator clipboard cleanup

#### Requirement: Strip confirmation line from NarratorText
The system SHALL remove the line "Copied last phrase to clipboard" from captured NarratorText before emitting output.

##### Scenario: Confirmation line present
- **WHEN** the captured clipboard text contains the confirmation line
- **THEN** the system removes that line from NarratorText

#### Requirement: Capture after focus change
The system SHALL capture NarratorText only after the focused element has been updated by the Tab action.

##### Scenario: Tab traversal step
- **WHEN** the tab loop moves focus to the next element
- **THEN** the system captures NarratorText after the focus change

### Capability: Narrator capture stability

#### Requirement: Filter failure messages from NarratorText
The system SHALL remove the line "Failed to copy to clipboard" from captured NarratorText before emitting output.

##### Scenario: Failure line present
- **WHEN** captured clipboard text contains the failure line
- **THEN** the system removes that line from NarratorText

#### Requirement: Add short delay and retry for capture
The system SHALL wait briefly after focus change and SHALL retry capture once if NarratorText is missing or mismatched.

##### Scenario: Initial capture mismatch
- **WHEN** the first capture does not match the focused element
- **THEN** the system waits briefly and retries capture once

#### Requirement: Fallback to current clipboard text
The system SHALL use the current clipboard text as NarratorText when capture fails and the clipboard text matches the focused element.

##### Scenario: Capture fails but clipboard matches
- **WHEN** capture returns no NarratorText and clipboard text matches the focused element
- **THEN** the system uses the clipboard text as NarratorText

#### Requirement: Do not auto-toggle Narrator in tab mode
The system SHALL NOT toggle Narrator on/off when running the tab command and SHALL only attempt capture if Narrator is already running.

##### Scenario: Narrator is off
- **WHEN** the tab command runs and Narrator is not running
- **THEN** the system skips NarratorText capture without toggling Narrator

#### Requirement: Emit mismatch warnings
The system SHALL emit `Mismatch: Name` and/or `Mismatch: ControlType` when captured NarratorText does not match the focused element.

##### Scenario: Mismatch detected
- **WHEN** NarratorText lacks the element Name or ControlType
- **THEN** the system logs the appropriate mismatch warning and omits NarratorText in output

#### Requirement: Log captured NarratorText for comparison
The system SHALL log captured NarratorText to stderr for comparison when it is available.

##### Scenario: NarratorText captured
- **WHEN** NarratorText is captured for an element
- **THEN** the system logs NarratorText with the element Name/ControlType to stderr

#### Requirement: Optional stdout NarratorText logging
The system SHALL support optional NarratorText logging to stdout when debugging is enabled.

##### Scenario: Debug stdout enabled
- **WHEN** the environment variable `PC_AUTOMATION_NARRATOR_STDOUT=1` is set
- **THEN** the system logs NarratorText to stdout for inspection

##### Scenario: Direct CLI invocation
- **WHEN** running `python pc_automation_cli.py tab` without the environment override
- **THEN** the system logs NarratorText to stdout for inspection

### Capability: Tab sequence stop log

#### Requirement: Skip initial focused element output
The system SHALL record the initial focused element RuntimeId but SHALL NOT emit JSON for that element.

##### Scenario: Sequence start
- **WHEN** the tab sequence starts
- **THEN** the system records the initial RuntimeId and emits no JSON yet

#### Requirement: Emit output after Tab focus change
The system SHALL emit JSON only after a Tab press changes focus and NarratorText is captured for that focused element.

##### Scenario: Tab step
- **WHEN** a Tab press completes and focus moves to a new element
- **THEN** the system captures NarratorText and emits JSON for that element

#### Requirement: Emit output on repeated RuntimeId then stop
The system SHALL emit JSON for the element whose RuntimeId repeats and SHALL stop the sequence afterward.

##### Scenario: Cycle detected
- **WHEN** the focused element RuntimeId matches a previously seen RuntimeId
- **THEN** the system emits JSON for that element and stops the loop

### Capability: Narrator clipboard log

#### Requirement: Capture last spoken text via clipboard hotkey
The system SHALL trigger Narrator's "Copy last spoken" hotkey and read the clipboard to obtain the most recently spoken text.

##### Scenario: Successful capture after focus change
- **WHEN** focus moves to a new element and Narrator is enabled
- **THEN** the system captures the last spoken text from the clipboard

#### Requirement: Support both Narrator keys
The system SHALL attempt capture using both Caps Lock and Insert as the Narrator key to maximize compatibility.

##### Scenario: Primary key fails, secondary succeeds
- **WHEN** capture with Caps Lock does not update the clipboard
- **THEN** the system retries using Insert and captures the text if available

#### Requirement: Preserve user clipboard contents
The system SHALL save the current clipboard contents before capture and restore them after reading Narrator text.

##### Scenario: Clipboard restored after capture
- **WHEN** the clipboard contains user data prior to capture
- **THEN** the clipboard content is restored after the Narrator text is read

#### Requirement: Emit NarratorText in JSON output
The system SHALL include the captured Narrator speech text as `NarratorText` in JSON output for `get_focused` and `tab` actions.

##### Scenario: get_focused output includes NarratorText
- **WHEN** `get_focused` is executed and capture succeeds
- **THEN** the JSON output includes `NarratorText` with the captured speech

##### Scenario: tab output includes NarratorText per element
- **WHEN** `tab` scans elements and capture succeeds for an element
- **THEN** that element's JSON line includes `NarratorText`

#### Requirement: Handle capture failure gracefully
The system SHALL omit `NarratorText` and log an error to stderr if capture fails after retries.

##### Scenario: Clipboard capture fails
- **WHEN** the clipboard does not change after both Narrator keys and a re-hear retry
- **THEN** the output omits `NarratorText` and stderr contains a failure message
