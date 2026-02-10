## 1. Pre-rename verification

- [x] 1.1 Search TalkBackAutoTest/*.cs files for "pc_cli" string literal references
- [x] 1.2 Verify current module imports in pc_cli.py have no issues
- [x] 1.3 Test all CLI actions work before rename (get_focused, narrator, tab)

## 2. File rename

- [x] 2.1 Execute git mv pc_cli.py pc_automation.py in TalkBackAutoTest/module/
- [x] 2.2 Verify git history preserved with git log --follow pc_automation.py
- [x] 2.3 Check no leftover references to pc_cli.py in any .py files

## 3. Documentation updates

- [x] 3.1 Replace all "pc_cli.py" references with "pc_automation.py" in README_DEV.txt
- [x] 3.2 Verify C# integration examples reference correct path (module/pc_automation.py)
- [x] 3.3 Update .github/copilot-instructions.md file references if needed

## 4. Post-rename verification

- [x] 4.1 Test python pc_automation.py get_focused returns JSON
- [x] 4.2 Test python pc_automation.py narrator toggles Narrator
- [x] 4.3 Test python pc_automation.py tab executes tab sequence
- [x] 4.4 Verify no import errors in any CLI action
- [x] 4.5 Check stderr logs are clean (no module not found errors)

## 5. C# integration check

- [x] 5.1 Verify MainForm.cs ProcessStartInfo calls still reference correct path
- [x] 5.2 Verify PCTB.cs subprocess calls work (if any direct references)
- [x] 5.3 Test one full C# → Python integration flow end-to-end
