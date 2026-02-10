## ADDED Requirements

### Requirement: Python module names are function-based and short
The system SHALL rename PC automation Python modules to short, function-based names that reflect their responsibilities.

#### Scenario: Module naming map is applied
- **WHEN** the module folder is inspected after refactor
- **THEN** files exist with the new names (`pc_cli.py`, `pc_clipboard.py`, `pc_keys.py`, `pc_uia.py`, `pc_element_info.py`)

### Requirement: Single CLI entrypoint is used
The system SHALL expose CLI execution only through the CLI file, without an additional wrapper entrypoint.

#### Scenario: CLI is invoked directly
- **WHEN** a user runs `python pc_cli.py <action>`
- **THEN** the CLI executes the requested action without requiring a wrapper module

### Requirement: CLI contract remains compatible
The CLI MUST preserve existing stdout JSON payloads, stderr logging, and exit codes for supported actions.

#### Scenario: Tab action output contract
- **WHEN** `pc_cli.py` is executed with the `tab` action
- **THEN** stdout emits JSON per element and stderr logs status messages with the same structure as before
