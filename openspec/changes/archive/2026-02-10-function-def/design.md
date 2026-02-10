## Context

Current Python PC-automation code under `TalkBackAutoTest/module` uses long, mixed naming (`pc_automation_*`) and a wrapper entrypoint. This makes the CLI integration path unclear and forces integrators to scan multiple files to understand responsibilities. The change focuses on renaming and reorganizing modules without changing runtime behavior.

Constraints:
- Keep a single CLI file as the entrypoint (no `pc_entry.py`).
- Preserve stdout JSON contract and stderr logging.
- Do not implement C# integration changes in this change set.

## Goals / Non-Goals

**Goals:**
- Define a clear, function-based Python module layout with short names.
- Keep one CLI file responsible for argument parsing and output contract.
- Update imports so the renamed modules work without behavior changes.

**Non-Goals:**
- Change the CLI contract or output schema.
- Refactor C# integration or update its Python invocation path.
- Add new automation behaviors or dependencies.

## Decisions

- **Single CLI entrypoint (`pc_cli.py`)**: Keep all CLI wiring in one file for clarity and to avoid multiple entrypoints.  
  *Alternative*: keep `pc_entry.py` as a wrapper. Rejected because it adds indirection without benefit for current integration plan.

- **Function-based module names**: Rename files to short, responsibility-based names (`pc_clipboard`, `pc_keys`, `pc_uia`, `pc_element_info`).  
  *Alternative*: keep existing `pc_automation_*` names. Rejected because it obscures responsibilities and makes integration harder.

- **No C# changes in this change**: Integration will be handled by another team.  
  *Alternative*: update C# now to point at new CLI. Rejected to avoid coupling and because integration scope is explicitly out.

## Risks / Trade-offs

- **Risk**: External scripts that import old module names will break.  
  **Mitigation**: Communicate rename list; consider temporary shims only if needed later.

- **Risk**: CLI path changes could break future integration assumptions.  
  **Mitigation**: Keep CLI contract unchanged; document the new entrypoint name.

## Migration Plan

1. Rename Python modules and update intra-module imports.
2. Remove the wrapper entrypoint file (`pc_automation.py`) and keep a single CLI file.
3. Smoke-test CLI commands (`get_focused`, `narrator`, `tab`).

## Open Questions

- Do we need temporary compatibility shims for old module names?
