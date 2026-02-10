## Purpose

TBD: Ensure the main CLI entrypoint and internal module import naming match C# subprocess integration expectations.

## Requirements

### Requirement: Main CLI entry point is pc_automation.py

The main Python CLI entry point MUST be named `pc_automation.py` to match C# subprocess integration expectations.

#### Scenario: C# calls Python with get_focused action
- **WHEN** C# executes `python module/pc_automation.py get_focused`
- **THEN** Python returns focused element JSON to stdout

#### Scenario: C# calls Python with narrator action
- **WHEN** C# executes `python module/pc_automation.py narrator`
- **THEN** Python toggles Narrator and returns success/failure code

#### Scenario: C# calls Python with tab action
- **WHEN** C# executes `python module/pc_automation.py tab`
- **THEN** Python executes tab sequence with cycle detection

### Requirement: Module imports use consistent naming

All internal Python module imports MUST use the standardized `pc_<name>.py` naming pattern for clarity.

#### Scenario: pc_automation.py imports other modules
- **WHEN** pc_automation.py needs to import helper modules
- **THEN** imports use names like `import pc_clipboard`, `import pc_keys`, `import pc_uia`

#### Scenario: No circular dependencies
- **WHEN** any module imports another module
- **THEN** import succeeds without circular dependency errors

### Requirement: Existing functionality preserved

All existing CLI actions MUST work identically after renaming, with no behavior changes.

#### Scenario: get_focused returns same JSON structure
- **WHEN** pc_automation.py get_focused is called with an element focused
- **THEN** JSON output matches previous pc_cli.py output format exactly

#### Scenario: Narrator capture still works
- **WHEN** pc_automation.py tab is called with Narrator running
- **THEN** NarratorText field is populated in output

#### Scenario: Error codes unchanged
- **WHEN** any CLI action encounters an error
- **THEN** exit codes match previous pc_cli.py behavior (0=success, 1=failure)

### Requirement: Documentation reflects actual file names

README_DEV.txt MUST reference the correct entry point file name in all examples.

#### Scenario: CLI usage examples are correct
- **WHEN** developer reads README_DEV.txt
- **THEN** all command examples use `python pc_automation.py <action>`

#### Scenario: C# integration examples are accurate
- **WHEN** developer reads C# integration section
- **THEN** ProcessStartInfo examples reference `module/pc_automation.py`

### Merged Requirement: Function-based module names + single CLI entrypoint

The system SHALL use short, function-based Python module names for PC automation helpers (e.g. `pc_clipboard.py`, `pc_keys.py`, `pc_uia.py`, `pc_element_info.py`) and SHALL expose CLI execution through a single entrypoint file matching C# expectations (`pc_automation.py`).

#### Scenario: Module layout and entrypoint
- **WHEN** the module folder is inspected after refactor
- **THEN** files exist with the new names (`pc_automation.py`, `pc_clipboard.py`, `pc_keys.py`, `pc_uia.py`, `pc_element_info.py` or `pc_cli.py` as a documented alias) and there is only one recommended CLI entrypoint

#### Scenario: CLI compatibility
- **WHEN** a user or C# integration invokes the CLI (`python module/pc_automation.py <action>`)
- **THEN** the command executes the requested action and preserves the stdout/stderr JSON contract and exit codes

### Requirement: Documentation and migration notes

Documentation (README_DEV.txt and any C# examples) MUST reference the canonical entrypoint name. If a historical alias exists (e.g., `pc_cli.py`), document it as an alias or provide a small shim, but the primary supported path must be `pc_automation.py`.

## Summary of changes to be applied from delta specs

- Add capability: standardize Python module names to `pc_<name>.py`.
- Ensure single canonical CLI entrypoint: `pc_automation.py` (document any alias/shim).
- Preserve all existing CLI output contracts (JSON on stdout, status on stderr, exit codes).
- Ensure imports use consistent naming and avoid circular dependencies.

