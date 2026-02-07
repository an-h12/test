## ADDED Requirements

### Requirement: System SHALL detect Tab loop cycles automatically

The Tab automation system SHALL detect when the Tab key loop returns to a previously-visited UI element and automatically stop the loop.

#### Scenario: Cycle detected in simple dialog
- **WHEN** Tab sequence visits Button1 → TextBox1 → Button2 → Button1 (cycle back to start)
- **THEN** system SHALL stop after Button2 and output 3 unique elements
- **THEN** system SHALL log "CYCLE DETECTED" message to stderr

#### Scenario: Cycle detected mid-sequence
- **WHEN** Tab sequence visits A → B → C → D → B (cycle to middle element)
- **THEN** system SHALL stop after D and output 4 unique elements (A, B, C, D)
- **THEN** system SHALL NOT output B again

#### Scenario: No cycle in entire application
- **WHEN** Tab sequence visits all N elements without revisiting any
- **THEN** system SHALL output all N elements
- **THEN** system SHALL stop when Tab wraps to first element (detected as cycle)

### Requirement: System SHALL use RuntimeId for cycle detection

The system SHALL use UI Automation's `GetRuntimeId()` method to uniquely identify elements.

#### Scenario: RuntimeId available
- **WHEN** system calls `element.GetRuntimeId()` on a focused element
- **THEN** system SHALL receive a tuple of integers (e.g., (42, 526252, 4, 321))
- **THEN** system SHALL use this tuple as unique identifier in cycle detection set

#### Scenario: Same element revisited
- **WHEN** RuntimeId (42, 526252, 4, 321) appears twice in sequence
- **THEN** system SHALL recognize it as the same element
- **THEN** system SHALL stop the loop (cycle detected)

#### Scenario: Different elements have different RuntimeIds
- **WHEN** two different elements are visited
- **THEN** their RuntimeIds SHALL be different
- **THEN** system SHALL NOT falsely detect a cycle

### Requirement: System SHALL handle RuntimeId unavailability gracefully

The system SHALL handle cases where `GetRuntimeId()` returns None or throws an exception.

#### Scenario: RuntimeId is None
- **WHEN** `element.GetRuntimeId()` returns None for an element
- **THEN** system SHALL log "WARNING: No RuntimeId" to stderr
- **THEN** system SHALL skip cycle check for that element only
- **THEN** system SHALL continue Tab loop (not break)

#### Scenario: GetRuntimeId throws exception
- **WHEN** `element.GetRuntimeId()` throws an exception
- **THEN** system SHALL catch the exception
- **THEN** system SHALL log warning to stderr
- **THEN** system SHALL treat as if RuntimeId is None (skip cycle check, continue)

### Requirement: System SHALL maintain output format

The Tab automation system SHALL maintain backward-compatible output format.

#### Scenario: JSON output unchanged
- **WHEN** system outputs element information
- **THEN** JSON format SHALL be identical to current format (Name, LocalizedControlType, Value, etc.)
- **THEN** JSON SHALL NOT include RuntimeId
- **THEN** JSON SHALL NOT include cycle detection metadata

#### Scenario: Each unique element output once
- **WHEN** cycle is detected at element N
- **THEN** system SHALL have output N-1 unique element JSON objects
- **THEN** system SHALL NOT output the repeated element (element #N)

#### Scenario: Stdout contains only JSON
- **WHEN** system outputs element data
- **THEN** all element JSON SHALL go to stdout only
- **THEN** cycle detection messages SHALL go to stderr only

### Requirement: System SHALL log cycle detection information

The system SHALL provide informative logging about cycle detection to stderr.

#### Scenario: Cycle detected logging
- **WHEN** cycle is detected
- **THEN** system SHALL log "CYCLE DETECTED at element #N" to stderr
- **THEN** system SHALL log "Total unique elements: M" to stderr
- **THEN** N SHALL be the Tab press count, M SHALL be unique element count

#### Scenario: Start message
- **WHEN** Tab loop starts
- **THEN** system SHALL log "Press ESC to stop" to stderr

#### Scenario: End statistics
- **WHEN** Tab loop ends (cycle or ESC)
- **THEN** system SHALL log "Tab pressed N time(s)" to stderr
- **THEN** system SHALL log "Unique elements visited: M" to stderr (if applicable)

### Requirement: System SHALL preserve ESC stop mechanism

The system SHALL preserve the existing ESC key stop functionality.

#### Scenario: User presses ESC before cycle
- **WHEN** user presses ESC key during Tab loop
- **THEN** system SHALL stop immediately (before cycle detection)
- **THEN** system SHALL log "Stopped by user" to stderr

#### Scenario: ESC takes priority
- **WHEN** user presses ESC at the same iteration cycle would be detected
- **THEN** system SHALL stop (both conditions trigger stop)
- **THEN** system SHALL NOT error or hang

### Requirement: System SHALL handle focus loss

The system SHALL handle gracefully when UI focus is lost during Tab loop.

#### Scenario: Focus lost to another window
- **WHEN** `GetFocusedControl()` returns None (focus lost)
- **THEN** system SHALL stop Tab loop
- **THEN** system SHALL log "WARNING: Focus lost" to stderr
- **THEN** system SHALL exit with success status (not error)

#### Scenario: Focus lost before any elements
- **WHEN** focus is lost before any element is visited
- **THEN** system SHALL stop immediately
- **THEN** system SHALL output 0 elements
- **THEN** system SHALL log warning to stderr
