## 1. Capture Stability

- [x] 1.1 Add delay + single retry for NarratorText capture
- [x] 1.2 Filter "Failed to copy to clipboard" and related failure lines
- [x] 1.3 Log mismatch warnings for Name/ControlType mismatches
- [x] 1.4 Log NarratorText to stderr for comparison
- [x] 1.5 Add optional NarratorText stdout logging
- [x] 1.6 Default stdout logging for direct CLI invocation
- [x] 1.7 Avoid auto-toggling Narrator during tab
- [x] 1.8 Fallback to clipboard text when capture fails

## 2. Documentation & Validation

- [x] 2.1 Update README_DEV.txt with mismatch warning behavior
- [x] 2.2 Add/adjust manual verification note for capture stability

## 3. Clipboard History Cleanup

- [x] 3.1 Add WinRT ClearHistory support for non-pinned clips
- [x] 3.2 Invoke history cleanup before clipboard preflight
- [x] 3.3 Document winrt dependency in README_DEV.txt
