## 1. Clipboard Clear Capability

- [x] 1.1 Add clear-history helper using Win32 clipboard APIs (pywin32/win32clipboard)
- [x] 1.2 Handle missing dependency gracefully and log warnings to stderr
- [x] 1.3 Add retry/backoff for clipboard contention

## 2. Tab Flow Integration

- [x] 2.1 Invoke clear after element-vs-NarratorText comparison in tab flow
- [x] 2.2 Ensure failures do not interrupt tab output
- [x] 2.3 Update stderr logging for clear attempt/failure

## 3. Specs/Docs Alignment

- [x] 3.1 Sync updated behavior into main specs (all-specs)
- [x] 3.2 Add brief note in README_DEV if needed
