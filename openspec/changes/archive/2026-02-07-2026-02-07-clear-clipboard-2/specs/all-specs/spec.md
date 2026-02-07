## MODIFIED Requirements

### Requirement: Clear non-pinned clipboard history after comparison
The system SHALL clear the current clipboard contents after each Narrator capture attempt in the CLI tab loop, after element-vs-NarratorText comparison when available, and SHALL perform the clear even if capture fails.

#### Scenario: Capture attempt complete
- **WHEN** a tab iteration completes its Narrator capture attempt and comparison (if any)
- **THEN** the system clears the current clipboard contents

#### Scenario: Capture fails
- **WHEN** a tab iteration fails to capture NarratorText
- **THEN** the system still performs the clipboard clear step

## ADDED Requirements

### Requirement: Fallback clear without pywin32
The system SHALL set a safe sentinel text when win32clipboard is unavailable to ensure the clipboard is clean for the next iteration.

#### Scenario: pywin32 missing
- **WHEN** the clipboard clear helper is invoked and win32clipboard is not available
- **THEN** the system writes a safe sentinel text to the clipboard

### Requirement: Scope clipboard clear to tab loop
The system SHALL perform clipboard clearing only during the CLI tab loop and SHALL NOT clear clipboard contents for other CLI actions.

#### Scenario: Non-tab command
- **WHEN** `get_focused` or `narrator` is executed
- **THEN** the system does not invoke the clipboard clear step
