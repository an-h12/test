## ADDED Requirements

### Requirement: Process initial focused element
The dumpScreen function SHALL capture and log the initially focused element before starting the tab navigation loop.

#### Scenario: Log initial element with narrator
- **WHEN** dumpScreen() starts with Narrator enabled
- **THEN** system captures element info and narrator text for the currently focused element and outputs it before the first Tab keypress

#### Scenario: Track initial element in cycle detection
- **WHEN** initial element is processed
- **THEN** system adds the element's runtime ID to the seen set for cycle detection

### Requirement: Single narrator capture per element
The system SHALL capture narrator output exactly once per element during tab sequence, eliminating duplicate capture attempts.

#### Scenario: Capture only in automation layer
- **WHEN** processing an element during tab iteration
- **THEN** narrator text is captured once by _maybe_capture_for_element() and not by element info gathering functions

#### Scenario: Element info without narrator text
- **WHEN** getFocusedElementInfo() is called during tab sequence
- **THEN** it returns element properties without attempting narrator capture

### Requirement: Narrator text in wrapper only
Element information dictionaries SHALL NOT contain NarratorText field; narrator output SHALL only appear in the outer automation wrapper.

#### Scenario: Clean element info structure
- **WHEN** element info is extracted
- **THEN** the returned dictionary contains Name, LocalizedControlType, Position, and pattern properties but NOT NarratorText

#### Scenario: Narrator text in automation wrapper
- **WHEN** tab iteration completes
- **THEN** output format is {"NarratorText": "...", "Element": {...}} where Element does not have its own NarratorText field

## MODIFIED Requirements

### Requirement: Preserve CLI output contract
The pc_automation CLI MUST preserve existing stdout/stderr formats and exit codes for `get_focused`, `narrator`, and `tab` actions.

#### Scenario: Invoke get_focused
- **WHEN** the user runs `python pc_automation.py get_focused`
- **THEN** the command emits a JSON object (or null) to stdout with element info (no narrator text) and returns the existing success/failure exit code

#### Scenario: Invoke tab
- **WHEN** the user runs `python pc_automation.py tab`
- **THEN** the command emits JSON lines per focused element (including initial element) to stdout with format {"NarratorText": "...", "Element": {...}} and returns the existing success/failure exit code

#### Scenario: Tab output includes first element
- **WHEN** user runs tab action
- **THEN** the first JSON line output corresponds to the initially focused element before any Tab keypresses
