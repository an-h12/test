## ADDED Requirements

### Requirement: Preserve CLI output contract
The system SHALL preserve the existing CLI output contract for `get_focused`, `tab`, and `narrator`, including stdout/stderr strings, exit codes, JSON schema, and JSON field order.

#### Scenario: CLI output remains identical
- **WHEN** running existing CLI commands used by tests
- **THEN** the output lines and exit codes match the current behavior exactly

### Requirement: Provide pure-function core API
The system SHALL expose a pure-function core that assembles element info in the same field order and schema as today, without performing I/O or timing.

#### Scenario: Core builds ordered JSON data
- **WHEN** the core is invoked with equivalent input data
- **THEN** it returns a dict with the same ordered fields as the current output

### Requirement: Split I/O adapters by concern
The system SHALL place UIA access, clipboard access, and keyboard input in separate adapter modules under `TalkBackAutoTest/module`.

#### Scenario: Adapter boundaries are separated
- **WHEN** reviewing module layout
- **THEN** UIA, clipboard, and keyboard logic are in distinct files

### Requirement: Keep CLI compatibility via stub
The system SHALL keep `pc_automation.py` as a compatibility stub that invokes the new CLI wiring without changing behavior.

#### Scenario: Legacy entrypoint still works
- **WHEN** invoking `pc_automation.py` as before
- **THEN** CLI behavior and outputs are unchanged
