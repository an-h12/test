## ADDED Requirements

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
