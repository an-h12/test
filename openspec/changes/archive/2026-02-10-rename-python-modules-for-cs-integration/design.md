## Context

Python CLI modules in `TalkBackAutoTest/module/` use `pc_cli.py` as the main entry point, but C# integration code and documentation references `pc_automation.py`. This naming mismatch creates confusion and requires developers to know the "real" file name differs from documented expectations.

Current structure:
- `pc_cli.py` - actual CLI entry point
- `pc_clipboard.py`, `pc_keys.py`, `pc_uia.py`, `pc_element_info.py` - helper modules
- README_DEV.txt - references `pc_automation.py`
- C# code - expects `module/pc_automation.py`

## Goals / Non-Goals

**Goals:**
- Rename `pc_cli.py` to `pc_automation.py` to match C# integration expectations
- Update all import references to reflect new naming
- Preserve git history using `git mv`
- Ensure zero functional changes - pure refactoring
- Update documentation to match actual file names

**Non-Goals:**
- Changing any Python module functionality or behavior
- Modifying C# integration code (should already reference correct name)
- Restructuring module architecture or adding new features
- Changing the CLI interface or output formats

## Decisions

### Decision 1: Rename via git mv, not delete+create

**Rationale**: Using `git mv pc_cli.py pc_automation.py` preserves file history in version control, making it easier to trace changes and understand evolution.

**Alternatives considered**:
- Delete and recreate: Loses git blame and history
- Keep both files: Creates maintenance burden and confusion

### Decision 2: Update imports in all dependent modules

**Approach**: Any module that imports from `pc_cli` must update imports, but since `pc_cli.py` is the entry point (not imported by others), only its own module declaration needs attention.

**Verification**: Run all Python modules standalone after rename to ensure no import errors.

### Decision 3: Single-file rename, not mass module renaming

**Rationale**: Only `pc_cli.py` has naming mismatch. Other modules (`pc_clipboard.py`, `pc_keys.py`, etc.) already follow the `pc_<name>.py` pattern and match documentation.

**Alternatives considered**:
- Rename all to `pc_automation_*.py`: Creates unnecessary churn, breaks existing imports
- Keep `pc_cli.py`: Perpetuates confusion between code and documentation

### Decision 4: Update README simultaneously

**Rationale**: Documentation and code must stay in sync. Update README_DEV.txt in the same commit as the file rename to prevent confusion.

**Changes needed**:
- Replace all `pc_cli.py` references with `pc_automation.py`
- Verify C# integration examples are accurate

## Risks / Trade-offs

**Risk**: C# code already hardcodes `pc_cli.py` in some places
→ **Mitigation**: Search TalkBackAutoTest/*.cs for "pc_cli" string literals before committing

**Risk**: Python import caching might cause issues during transition
→ **Mitigation**: Test by running each CLI action after rename; Python reimports modules on each subprocess call

**Trade-off**: Git blame on renamed file requires `--follow` flag
→ **Acceptable**: Standard git practice for renames, minimal impact

**Risk**: Archived OpenSpec changes reference old file names
→ **Mitigation**: Archive contains historical context; no need to update past docs
