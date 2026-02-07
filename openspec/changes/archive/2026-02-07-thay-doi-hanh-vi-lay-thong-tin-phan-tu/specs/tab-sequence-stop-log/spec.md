## ADDED Requirements

### Requirement: Skip initial focused element output
The system SHALL record the initial focused element RuntimeId but SHALL NOT emit JSON for that element.

#### Scenario: Sequence start
- **WHEN** the tab sequence starts
- **THEN** the system records the initial RuntimeId and emits no JSON yet

### Requirement: Emit output after Tab focus change
The system SHALL emit JSON only after a Tab press changes focus and NarratorText is captured for that focused element.

#### Scenario: Tab step
- **WHEN** a Tab press completes and focus moves to a new element
- **THEN** the system captures NarratorText and emits JSON for that element

### Requirement: Emit output on repeated RuntimeId then stop
The system SHALL emit JSON for the element whose RuntimeId repeats and SHALL stop the sequence afterward.

#### Scenario: Cycle detected
- **WHEN** the focused element RuntimeId matches a previously seen RuntimeId
- **THEN** the system emits JSON for that element and stops the loop