## Context

`pc_automation.py` is a single module with UIA queries, clipboard access, keyboard input, and CLI dispatch. Tests depend on exact stdout/stderr and JSON field order. The change must keep those outputs identical while enabling reuse as a functional core.

## Goals / Non-Goals

**Goals:**
- Split logic into focused modules under `TalkBackAutoTest/module`.
- Provide pure-function core APIs that preserve JSON schema and field order.
- Keep CLI behavior identical for tests (stdout/stderr strings, exit codes, JSON lines).

**Non-Goals:**
- Changing JSON schema, adding/removing fields, or reordering output.
- Altering CLI commands or introducing new flags.

## Decisions

- **Module layout:**
  - `pc_automation_core.py` for pure data assembly (field order preserved).
  - `pc_automation_uia.py`, `pc_automation_clipboard.py`, `pc_automation_keys.py` for I/O adapters.
  - `pc_automation_cli.py` for CLI wiring.
  - `pc_automation.py` becomes a compatibility stub.

- **Contract preservation:**
  - Maintain insertion order for JSON fields exactly as today.
  - Keep all stderr/stdout strings and exit codes unchanged.

- **Dependency boundaries:**
  - Core takes plain data and injected adapter functions to remain deterministic.
  - Adapters contain `uiautomation`, `ctypes`, timing, and clipboard access.

## Risks / Trade-offs

- **Risk:** JSON field order changes due to refactor → **Mitigation:** enforce ordered assembly in core and reuse existing pattern order lists.
- **Risk:** CLI output drift → **Mitigation:** keep CLI printing logic in a thin wrapper mirroring current behavior.

## Migration Plan

1. Create new modules in `TalkBackAutoTest/module` and move logic with minimal edits.
2. Replace `pc_automation.py` with a stub that calls `pc_automation_cli.main()`.
3. Run existing CLI-based tests to confirm identical outputs.

## Open Questions

- Do we need a public import surface beyond CLI for tool integration? If yes, define a minimal functional API in `pc_automation_core.py`.
