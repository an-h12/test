## ADDED Requirements

### Requirement: Auto-enable Narrator for clipboard capture
The system SHALL detect whether Narrator is running before attempting clipboard capture and SHALL auto-toggle Narrator on when it is off.

#### Scenario: Narrator is off
- **WHEN** a clipboard capture is requested and Narrator is not running
- **THEN** the system toggles Narrator on before sending the copy-hotkey

#### Scenario: Narrator is already on
- **WHEN** a clipboard capture is requested and Narrator is running
- **THEN** the system does not toggle Narrator and proceeds with capture

### Requirement: Restore Narrator state after capture
The system SHALL restore Narrator to its prior state after capture completes if it was auto-enabled.

#### Scenario: Narrator was auto-enabled
- **WHEN** capture completes after auto-enabling Narrator
- **THEN** the system toggles Narrator off to restore the previous state

### Requirement: Preserve CLI output contract
The system SHALL keep stdout JSON schema and field order unchanged while performing auto-toggle logic.

#### Scenario: CLI output unchanged
- **WHEN** running `get_focused` or `tab`
- **THEN** stdout JSON structure and field order remain identical to current behavior

### Requirement: Log auto-toggle events to stderr
The system SHALL emit concise status messages to stderr when auto-toggle or restore actions occur.

#### Scenario: Auto-toggle logged
- **WHEN** the system toggles Narrator on or off automatically
- **THEN** a status message is written to stderr without changing stdout
