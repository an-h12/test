## Why

Python PC-automation modules currently use verbose, inconsistent file names and a mixed entrypoint pattern. This makes integration harder because it is unclear which file owns which responsibility and which file should be invoked for CLI execution.

## What Changes

- Rename Python modules in `TalkBackAutoTest/module` to short, function-based names.
- Keep a single CLI file as the entrypoint (no separate `pc_entry.py`).
- Update intra-module imports to match new file names.
- Preserve CLI output contract and exit codes.

## Capabilities

### New Capabilities
- `pc-python-module-refactor`: Define the required Python module layout, naming, and CLI entrypoint contract for PC automation.

### Modified Capabilities
- (none)

## Impact

- Python package layout under `TalkBackAutoTest/module`
- CLI invocation path for PC automation (still Python-only; C# integration remains out of scope)
