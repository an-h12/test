## MODIFIED Requirements

### Requirement: Failure returns None and restores clipboard
The system SHALL return None when the clipboard sequence number does not change after the capture hotkey is sent within a short timeout, and SHALL attempt to restore the original clipboard text.

#### Scenario: Clipboard sequence does not change
- **WHEN** the Narrator capture hotkey is sent and the clipboard sequence number does not change within the timeout
- **THEN** the system returns None and restores the original clipboard text

## ADDED Requirements

### Requirement: Detect clipboard update via sequence number
The system SHALL read the clipboard sequence number before sending the Narrator capture hotkey and SHALL wait for it to change before reading clipboard text.

#### Scenario: Capture attempt starts
- **WHEN** a Narrator capture attempt begins
- **THEN** the system waits for the clipboard sequence number to change before reading clipboard text

### Requirement: Do not write sentinel text during capture
The system SHALL NOT write a sentinel string to the clipboard as part of Narrator capture detection.

#### Scenario: Capture in progress
- **WHEN** Narrator capture is performed
- **THEN** the system does not write a sentinel string to the clipboard
