# Changelog

## [Unreleased]

### Changed

- Keep tab sequence running when focus is temporarily lost; TalkBackAutoTest/module/pc_automation_cli.py.
- Clarify Narrator/psutil logging and format Win32 API calls; TalkBackAutoTest/module/pc_automation_clipboard.py.
- Remove rehear retries and Insert fallback from Narrator clipboard capture; TalkBackAutoTest/module/pc_automation_clipboard.py.
- Preflight Narrator/clipboard for tab sequences and emit current element before tabbing; TalkBackAutoTest/module/pc_automation_cli.py.
- Strip Narrator clipboard confirmation line and capture after focus change; TalkBackAutoTest/module/pc_automation_clipboard.py.
- Change tab output ordering to skip initial focus and emit repeated element before stop; TalkBackAutoTest/module/pc_automation_cli.py.
- Add capture retry, filter clipboard failure line, log mismatch warnings, and keep NarratorText stderr logging; TalkBackAutoTest/module/pc_automation_cli.py.
