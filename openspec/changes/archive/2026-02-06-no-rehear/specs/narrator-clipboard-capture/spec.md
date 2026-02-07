## ADDED Requirements

### Requirement: Single-attempt Narrator clipboard capture
The system SHALL attempt Narrator clipboard capture using Caps Lock + Ctrl + X only and SHALL NOT invoke rehear or Insert fallback paths.

#### Scenario: Capture requested
- **WHEN** Narrator clipboard capture is requested
- **THEN** the system sends the Caps Lock + Ctrl + X chord once and does not send rehear or Insert-based chords

### Requirement: Failure returns None and restores clipboard
The system SHALL return None when capture fails to replace the clipboard sentinel and SHALL attempt to restore the original clipboard text.

#### Scenario: Clipboard does not update
- **WHEN** the clipboard text remains the sentinel after the capture attempt
- **THEN** the system returns None and restores the original clipboard contents