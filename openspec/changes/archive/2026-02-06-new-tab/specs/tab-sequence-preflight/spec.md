## ADDED Requirements

### Requirement: Preflight Narrator and clipboard once per tab sequence
The system SHALL check Narrator state and clipboard text format once before starting the tab traversal and SHALL keep the same session for the full loop.

#### Scenario: Preflight succeeds
- **WHEN** the tab sequence starts
- **THEN** the system ensures Narrator is available and verifies clipboard text format once before any Tab press

### Requirement: Include current focused element before first Tab
The system SHALL output the currently focused element before sending the first Tab press.

#### Scenario: Initial focused element printed
- **WHEN** the tab sequence starts
- **THEN** the system prints the current focused element info prior to the first Tab action

### Requirement: Stop on repeated RuntimeId without printing duplicate
The system SHALL stop the tab traversal when a RuntimeId repeats and SHALL NOT print the duplicate element.

#### Scenario: RuntimeId repeats
- **WHEN** a newly focused element has a RuntimeId already seen in the current sequence
- **THEN** the system stops the loop without emitting element info for the duplicate

### Requirement: Continue tabbing when clipboard preflight fails
The system SHALL continue tab traversal when clipboard text format is unavailable and SHALL set narrator_text to None for all emitted elements.

#### Scenario: Clipboard text format unavailable
- **WHEN** clipboard preflight detects no text format
- **THEN** the system continues the sequence and outputs elements with narrator_text = None