## Why

Current Python modules use inconsistent naming (`pc_cli.py`, `pc_clipboard.py`) that doesn't match C# integration expectations. C# code calls `python module/pc_automation.py <action>` but the actual entry point is `pc_cli.py`, causing confusion and potential integration errors.

## What Changes

- Rename `pc_cli.py` → `pc_automation.py` (main CLI entry point)
- Update internal imports to reflect new naming
- Align module structure with C# subprocess integration pattern
- Update README_DEV.txt to reference correct file names
- Ensure all existing C# integration patterns work without modification

## Capabilities

### New Capabilities
- `python-module-naming`: Standardize Python module naming to match C# subprocess integration expectations and improve developer clarity

### Modified Capabilities
<!-- No requirement changes - this is purely a refactoring -->

## Impact

- **TalkBackAutoTest/module/pc_cli.py**: Renamed to pc_automation.py
- **TalkBackAutoTest/module/README_DEV.txt**: Updated file references
- **C# integration code**: No changes needed if already using correct path
- **Build configuration**: Verify .csproj CopyToOutputDirectory patterns
