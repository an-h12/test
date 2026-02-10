## 1. Rename Python modules

- [x] 1.1 Rename `pc_automation_cli.py` to `pc_cli.py`
- [x] 1.2 Rename `pc_automation_clipboard.py` to `pc_clipboard.py`
- [x] 1.3 Rename `pc_automation_keys.py` to `pc_keys.py`
- [x] 1.4 Rename `pc_automation_uia.py` to `pc_uia.py`
- [x] 1.5 Rename `pc_automation_core.py` to `pc_element_info.py`

## 2. Update internal imports and entrypoint

- [x] 2.1 Update imports in `pc_cli.py` to use new module names
- [x] 2.2 Update imports in `pc_clipboard.py` to use `pc_keys`
- [x] 2.3 Update imports in `pc_uia.py` to use `pc_clipboard` and `pc_element_info`
- [x] 2.4 Remove `pc_automation.py` wrapper (no `pc_entry.py`)

## 3. Validate CLI behavior

- [x] 3.1 Smoke-test `python pc_cli.py get_focused`
- [x] 3.2 Smoke-test `python pc_cli.py narrator`
- [x] 3.3 Smoke-test `python pc_cli.py tab`
