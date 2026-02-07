## Why

Current `pc_automation.py` is a single file mixing UIA, clipboard, keyboard input, and CLI behavior, which limits reuse and tool integration. We need a functional core and stable module boundaries while preserving existing CLI output for tests.

## What Changes

- Split `pc_automation.py` into focused modules under `TalkBackAutoTest/module`.
- Create a pure-function core that preserves JSON schema and field order.
- Keep CLI behavior identical (stdout/stderr strings, exit codes, and JSON line format).
- Introduce a thin CLI wrapper and a compatibility stub in `pc_automation.py`.

## Capabilities

### New Capabilities
- `pc-automation-modularization`: Modular, pure-function API surface for UIA, clipboard, keyboard, and CLI wiring while preserving existing CLI output contract.

### Modified Capabilities
- 

## Impact

- Files under `TalkBackAutoTest/module`.
- CLI test flow that uses `pc_automation.py`.
- No external API changes; output contract preserved.
