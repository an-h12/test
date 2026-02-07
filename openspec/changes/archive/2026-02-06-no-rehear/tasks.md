## 1. Capture Flow Simplification

- [x] 1.1 Remove rehear helper and `NARRATOR_REHEAR_DELAY` constant from `pc_automation_clipboard.py`
- [x] 1.2 Drop `VK_INSERT` import and fallback capture logic; keep Caps Lock only
- [x] 1.3 Update error logging to reflect a single capture attempt

## 2. Documentation & Verification

- [x] 2.1 Update `CHANGELOG.md` to note removal of rehear and Insert fallback
- [x] 2.2 Smoke-test Narrator capture path (success and failure) to confirm clipboard restore behavior
