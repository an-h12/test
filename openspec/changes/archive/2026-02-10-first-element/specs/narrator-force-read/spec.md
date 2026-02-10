## Purpose

Define requirements for forcing Windows Narrator to read the currently focused element on demand, enabling capture of initial element state before tab navigation begins.

## Requirements

### Requirement: Force Narrator to read current element
The system SHALL provide a mechanism to trigger Windows Narrator to read aloud the currently focused UI element without changing focus.

#### Scenario: Trigger read with Caps Lock key
- **WHEN** Caps Lock + Tab key combination is sent to the system
- **THEN** Narrator reads the currently focused element and copies the speech text to clipboard

#### Scenario: Trigger read with Insert key
- **WHEN** Insert + Tab key combination is sent to the system  
- **THEN** Narrator reads the currently focused element and copies the speech text to clipboard

### Requirement: Key event simulation for force read
The system SHALL simulate the Narrator hotkey (Caps Lock or Insert + Tab) using Windows SendInput API to trigger speech capture.

#### Scenario: Send force read key sequence
- **WHEN** force read function is invoked
- **THEN** system sends key down events for Narrator key (Caps Lock or Insert), followed by Tab key, then key up events in reverse order

#### Scenario: Configurable hold time
- **WHEN** force read keys are sent
- **THEN** system holds all keys pressed for a configurable duration before releasing to ensure Narrator processes the command

### Requirement: Initial element capture in tab sequence
When starting a tab navigation sequence with Narrator enabled, the system SHALL capture and log the initially focused element before any Tab keypresses.

#### Scenario: Process initial element before loop
- **WHEN** dumpScreen() starts with Narrator ready
- **THEN** system forces Narrator to read current element, captures the output, and logs it before entering the tab loop

#### Scenario: Initial element includes narrator text
- **WHEN** initial element is processed
- **THEN** output includes both element info and Narrator speech text in the same format as tab iteration elements

### Requirement: No focus change during force read
Force reading the current element SHALL NOT change the focused element or alter navigation state.

#### Scenario: Focus remains stable after force read
- **WHEN** force read is triggered on element A
- **THEN** element A remains focused after the operation completes
