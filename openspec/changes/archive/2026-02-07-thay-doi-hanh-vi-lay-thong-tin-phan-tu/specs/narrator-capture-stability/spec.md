## ADDED Requirements

### Requirement: Filter failure messages from NarratorText
The system SHALL remove the line "Failed to copy to clipboard" from captured NarratorText before emitting output.

#### Scenario: Failure line present
- **WHEN** captured clipboard text contains the failure line
- **THEN** the system removes that line from NarratorText

### Requirement: Add short delay and retry for capture
The system SHALL wait briefly after focus change and SHALL retry capture once if NarratorText is missing or mismatched.

#### Scenario: Initial capture mismatch
- **WHEN** the first capture does not match the focused element
- **THEN** the system waits briefly and retries capture once

### Requirement: Fallback to current clipboard text
The system SHALL use the current clipboard text as NarratorText when capture fails and the clipboard text matches the focused element.

#### Scenario: Capture fails but clipboard matches
- **WHEN** capture returns no NarratorText and clipboard text matches the focused element
- **THEN** the system uses the clipboard text as NarratorText

### Requirement: Do not auto-toggle Narrator in tab mode
The system SHALL NOT toggle Narrator on/off when running the tab command and SHALL only attempt capture if Narrator is already running.

#### Scenario: Narrator is off
- **WHEN** the tab command runs and Narrator is not running
- **THEN** the system skips NarratorText capture without toggling Narrator

### Requirement: Emit mismatch warnings
The system SHALL emit `Mismatch: Name` and/or `Mismatch: ControlType` when captured NarratorText does not match the focused element.

#### Scenario: Mismatch detected
- **WHEN** NarratorText lacks the element Name or ControlType
- **THEN** the system logs the appropriate mismatch warning and omits NarratorText in output

### Requirement: Log captured NarratorText for comparison
The system SHALL log captured NarratorText to stderr for comparison when it is available.

#### Scenario: NarratorText captured
- **WHEN** NarratorText is captured for an element
- **THEN** the system logs NarratorText with the element Name/ControlType to stderr

### Requirement: Optional stdout NarratorText logging
The system SHALL support optional NarratorText logging to stdout when debugging is enabled.

#### Scenario: Debug stdout enabled
- **WHEN** the environment variable `PC_AUTOMATION_NARRATOR_STDOUT=1` is set
- **THEN** the system logs NarratorText to stdout for inspection

#### Scenario: Direct CLI invocation
- **WHEN** running `python pc_automation_cli.py tab` without the environment override
- **THEN** the system logs NarratorText to stdout for inspection
