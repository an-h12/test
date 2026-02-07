## ADDED Requirements

### Requirement: Capture last spoken text via clipboard hotkey
The system SHALL trigger Narrator's "Copy last spoken" hotkey and read the clipboard to obtain the most recently spoken text.

#### Scenario: Successful capture after focus change
- **WHEN** focus moves to a new element and Narrator is enabled
- **THEN** the system captures the last spoken text from the clipboard

### Requirement: Support both Narrator keys
The system SHALL attempt capture using both Caps Lock and Insert as the Narrator key to maximize compatibility.

#### Scenario: Primary key fails, secondary succeeds
- **WHEN** capture with Caps Lock does not update the clipboard
- **THEN** the system retries using Insert and captures the text if available

### Requirement: Preserve user clipboard contents
The system SHALL save the current clipboard contents before capture and restore them after reading Narrator text.

#### Scenario: Clipboard restored after capture
- **WHEN** the clipboard contains user data prior to capture
- **THEN** the clipboard content is restored after the Narrator text is read

### Requirement: Emit NarratorText in JSON output
The system SHALL include the captured Narrator speech text as `NarratorText` in JSON output for `get_focused` and `tab` actions.

#### Scenario: get_focused output includes NarratorText
- **WHEN** `get_focused` is executed and capture succeeds
- **THEN** the JSON output includes `NarratorText` with the captured speech

#### Scenario: tab output includes NarratorText per element
- **WHEN** `tab` scans elements and capture succeeds for an element
- **THEN** that element's JSON line includes `NarratorText`

### Requirement: Handle capture failure gracefully
The system SHALL omit `NarratorText` and log an error to stderr if capture fails after retries.

#### Scenario: Clipboard capture fails
- **WHEN** the clipboard does not change after both Narrator keys and a re-hear retry
- **THEN** the output omits `NarratorText` and stderr contains a failure message
