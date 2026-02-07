## ADDED Requirements

### Requirement: Clear non-pinned clipboard history after comparison
The system SHALL clear non-pinned clipboard history entries after comparing focused element attributes with NarratorText.

#### Scenario: Comparison complete
- **WHEN** element attributes have been compared with NarratorText
- **THEN** the system clears non-pinned clipboard history entries

### Requirement: Preserve pinned clipboard items
The system SHALL preserve pinned clipboard history items when clearing clipboard history.

#### Scenario: Pinned entries exist
- **WHEN** non-pinned clipboard history is cleared
- **THEN** pinned items remain available in clipboard history

### Requirement: Log clear failures without stopping tab
The system SHALL log clear failures to stderr and continue the tab sequence.

#### Scenario: Clear operation fails
- **WHEN** clearing clipboard history fails
- **THEN** the system logs a warning to stderr and continues
