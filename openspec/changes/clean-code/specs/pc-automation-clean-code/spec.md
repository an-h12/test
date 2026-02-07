## ADDED Requirements

### Requirement: Preserve CLI output contract
The pc_automation CLI MUST preserve existing stdout/stderr formats and exit codes for `get_focused`, `narrator`, and `tab` actions.

#### Scenario: Invoke get_focused
- **WHEN** the user runs `python pc_automation.py get_focused`
- **THEN** the command emits a JSON object (or null) to stdout and returns the existing success/failure exit code

#### Scenario: Invoke tab
- **WHEN** the user runs `python pc_automation.py tab`
- **THEN** the command emits JSON lines per focused element to stdout and returns the existing success/failure exit code

### Requirement: Handle missing optional dependencies safely
If `uiautomation`, `psutil`, or `win32clipboard` is unavailable, the CLI MUST not crash and MUST emit a clear error or warning message to stderr while preserving exit behavior.

#### Scenario: Missing uiautomation
- **WHEN** `uiautomation` is not installed and the user runs `get_focused`
- **THEN** the command prints an error to stderr and exits with failure

#### Scenario: Missing win32clipboard
- **WHEN** `win32clipboard` is not installed and `tab` is run
- **THEN** the command continues without clipboard clearing and does not crash
