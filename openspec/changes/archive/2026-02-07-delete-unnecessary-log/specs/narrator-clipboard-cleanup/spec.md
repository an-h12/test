## ADDED Requirements

### Requirement: Strip confirmation line from NarratorText
The system SHALL remove the line "Copied last phrase to clipboard" from captured NarratorText before emitting output.

#### Scenario: Confirmation line present
- **WHEN** the captured clipboard text contains the confirmation line
- **THEN** the system removes that line from NarratorText

### Requirement: Capture after focus change
The system SHALL capture NarratorText only after the focused element has been updated by the Tab action.

#### Scenario: Tab traversal step
- **WHEN** the tab loop moves focus to the next element
- **THEN** the system captures NarratorText after the focus change