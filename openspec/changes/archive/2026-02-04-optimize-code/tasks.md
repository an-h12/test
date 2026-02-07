## 1. Module Scaffolding

- [x] 1.1 Create new module files under TalkBackAutoTest/module
- [x] 1.2 Add minimal header notes in pc_automation_core.py and pc_automation_cli.py

## 2. Core Extraction

- [x] 2.1 Move ordered JSON assembly logic into pc_automation_core.py
- [x] 2.2 Ensure core preserves field order and schema exactly

## 3. Adapter Extraction

- [x] 3.1 Move UIA access into pc_automation_uia.py
- [x] 3.2 Move clipboard/Narrator capture into pc_automation_clipboard.py
- [x] 3.3 Move keyboard input into pc_automation_keys.py

## 4. CLI Wiring

- [x] 4.1 Implement pc_automation_cli.py to mirror current CLI behavior
- [x] 4.2 Replace pc_automation.py with a compatibility stub

## 5. Verification

- [x] 5.1 Run CLI-based tests or spot checks to confirm identical output
