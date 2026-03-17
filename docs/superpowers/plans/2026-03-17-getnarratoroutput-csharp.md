# getNarratorOutput C# Implementation Plan

> **For agentic workers:** REQUIRED: Use superpowers:subagent-driven-development (if subagents available) or superpowers:executing-plans to implement this plan.

**Goal:** Convert `getNarratorOutput()` from Python to C# as native methods in PCTB.cs (inside `#region Narrator`), including all required helper functions. Also update Python backup file to match C# logic.

**Architecture:** All code integrated directly into PCTB.cs inside `#region Narrator` as static helper methods with Win32 P/Invoke declarations. Python backup file will be updated to match same logic.

**Tech Stack:** C# .NET Framework 4.8, System.Windows.Forms.SendKeys, Win32 Clipboard API

---

## Context

### Problem Statement
- Currently `getNarratorOutput()` in PCTB.cs calls Python CLI via HTTP API
- Need to convert to native C# to eliminate Python dependency
- Must include `ForceNarratorRead()` to ensure correct first element capture

### Workflow Diagram

```dot
digraph NarratorCaptureWorkflow {
    rankdir=TB;
    start [shape=doublecircle, label="Start"];

    ensure_narrator [shape=diamond, label="Narrator\nRunning?"];
    toggle_narrator [shape=box, label="ToggleNarrator()\n(Ctrl+Win+Enter)"];
    force_read [shape=box, label="ForceNarratorRead()\n(Caps+Tab)"];
    capture [shape=box, label="DoNarratorCapture()\n(Caps+Ctrl+X)"];
    get_clipboard [shape=box, label="GetClipboardText()"];
    filter [shape=box, label="FilterNarratorOutput()"];
    clear_current [shape=box, label="ClearCurrentClipboard()\n(Clipboard.Clear)"];
    restore [shape=box, label="RestoreNarratorState()"];
    end [shape=doublecircle, label="End"];

    start -> ensure_narrator;
    ensure_narrator -> toggle_narrator [label="No"];
    ensure_narrator -> force_read [label="Yes"];
    toggle_narrator -> force_read;
    force_read -> capture;
    capture -> get_clipboard;
    get_clipboard -> filter;
    filter -> clear_current;
    clear_current -> restore;
    restore -> end;
}
```

### Workflow Diagram - Test Loop (MainForm.getObjectInScreenPC)

```dot
digraph TestLoopWorkflow {
    rankdir=TB;
    start_loop [shape=doublecircle, label="Start Loop"];

    change_focus [shape=box, label="changeFocus()\n(TAB key)"];
    narrator_output [shape=box, label="getNarratorOutput()\n(Capture + ClearCurrentClipboard)"];
    process_object [shape=box, label="Process Object"];
    check_exit [shape=diamond, label="Exit\nConditions?"];
    end_loop [shape=doublecircle, label="End Loop"];

    clear_history_python [shape=box, label="ClearClipboardHistoryViaPython()\n(WinRT.clear_history)"];

    start_loop -> change_focus;
    change_focus -> narrator_output;
    narrator_output -> process_object;
    process_object -> check_exit;
    check_exit -> change_focus [label="Continue"];
    check_exit -> clear_history_python [label="Exit"];
    clear_history_python -> end_loop;
}
```

**Workflow chi tiết:**
1. **Trong vòng lặp**: Mỗi lần gọi `getNarratorOutput()` sẽ dùng `Clipboard.Clear()` để xóa nhanh nội dung hiện tại
2. **Khi thoát vòng lặp**: Gọi Python để xóa clipboard history hoàn toàn qua WinRT

### Required Functions (Python → C# Naming)

All functions will be implemented inside `#region Narrator` in PCTB.cs

| Python (snake_case) | C# (PascalCase) | Source File | Purpose |
|---------------------|-----------------|-------------|---------|
| `IsNarratorRunning()` | `IsNarratorRunning()` | pc_output_narrator.py | Check if narrator.exe running |
| `toggle_narrator()` | `ToggleNarrator()` | pc_keys.py | Toggle Narrator on/off (Ctrl+Win+Enter) |
| `force_narrator_read()` | `ForceNarratorRead()` | pc_keys.py | Force read current element (Caps+Tab) |
| `_do_narrator_capture()` | `DoNarratorCapture()` | pc_output_narrator.py | Main capture logic (Caps+Ctrl+X) |
| `capture_narrator_last_spoken()` | `CaptureNarratorLastSpoken()` | pc_output_narrator.py | Full capture workflow |
| `_strip_narrator_confirmation()` | `FilterNarratorOutput()` | pc_output_narrator.py | Remove "copied to clipboard" messages |
| `clear_clipboard_history()` | `ClearClipboardHistoryViaPython()` | pc_output_narrator.py | Clear full history (on exit) - gọi Python |
| **`N/A`** | **`ClearCurrentClipboard()`** | System.Windows.Forms.Clipboard | Clear current clipboard (in-loop) |
| `send_key_event()` | `SendKeyEvent()` | pc_keys.py | Send single key |
| `send_key_chord()` | `SendKeyChord()` | pc_keys.py | Send multiple keys |

---

## Implementation Tasks

### Task 1: Add Win32 Clipboard P/Invoke Layer to PCTB.cs

**Files:**
- Modify: `TalkBackAutoTest/PCTB.cs` - Add at top of class

**Steps:**
- [ ] Add constants: VK_*, SPI_*, CF_*
- [ ] Add Win32 Clipboard API declarations (initial):
  - `GetClipboardData()` - Lấy data từ clipboard
  - `GetClipboardSequenceNumber()` - Theo dõi thay đổi clipboard
  - `EmptyClipboard()` - Xóa clipboard
- [ ] Verify compilation
- [ ] *(Các hàm khác sẽ implement sau nếu cần)*

**Note:** Using SendKeys.SendWait() for keyboard (no P/Invoke needed). Only clipboard needs P/Invoke.

### Task 2: Add Keyboard Input Methods (using SendKeys.SendWait)

**Files:**
- Modify: `TalkBackAutoTest/PCTB.cs`

**Steps:**
- [ ] Add `SendKeyEvent(int vkCode)` - Send single key using SendKeys
- [ ] Add `SendKeyChord(string keyCombo)` - Send key combination using SendKeys format
- [ ] Add `ToggleNarrator()` - Ctrl+Win+Enter
- [ ] Add `ForceNarratorRead()` - **Caps+Tab** for first element (cần đợi 1s trước khi gửi phím để tránh bị lặp)
- [ ] Verify compilation

**Note:** Using `SendKeys.SendWait()` instead of Win32 SendInput - already available via System.Windows.Forms

### Task 3: Add Clipboard Operations

**Files:**
- Modify: `TalkBackAutoTest/PCTB.cs`

**Steps:**
- [ ] Add `GetClipboardText()` - open, read, close clipboard (Win32 hoặc Clipboard class)
- [ ] Add `GetClipboardSequenceNumber()` - track clipboard changes (Win32)
- [ ] Add `ClearCurrentClipboard()` - xóa nội dung hiện tại (System.Windows.Forms.Clipboard.Clear())
- [ ] Add `ClearClipboardHistoryViaPython()` - gọi Python/WinRT để xóa full history
- [ ] Verify compilation

**Note:** Sử dụng `Clipboard.Clear()` cho trong vòng lặp, gọi Python cho khi thoát.

### Task 4: Add Narrator Process Management

**Files:**
- Modify: `TalkBackAutoTest/PCTB.cs`

**Steps:**
- [ ] Add `IsNarratorRunning()` - iterate Process.GetProcesses() or use SPI_GETSCREENREADER
- [ ] Add `WaitForNarratorState(bool expectedRunning)` - retry with backoff
- [ ] Add `StartNarratorProcess()` - launch Narrator.exe
- [ ] Add `EnsureNarratorOn()` - auto-enable if off, returns bool? (null if failed)
- [ ] Add `RestoreNarratorState(bool? autoEnabled)` - restore previous state
- [ ] Verify compilation

### Task 5: Add Capture Implementation

**Files:**
- Modify: `TalkBackAutoTest/PCTB.cs`

**Steps:**
- [ ] Add `WaitForClipboardSequenceChange(uint seqBefore)` - wait for clipboard update
- [ ] Add `FilterNarratorOutput(string text)` - remove "copied to clipboard" messages
- [ ] Add `DoNarratorCapture()` - main capture logic with Caps+Ctrl+X
- [ ] Add `TryNarratorCapture()` - with retry logic
- [ ] Add `CaptureNarratorLastSpoken()` - full workflow: ForceNarratorRead() → DoNarratorCapture() → GetClipboardText() → FilterNarratorOutput() → **ClearCurrentClipboard()**
- [ ] Verify compilation

### Task 6: Add Main Public API

**Files:**
- Modify: `TalkBackAutoTest/PCTB.cs`

**Steps:**
- [ ] Add `GetNarratorOutput()` - main public API: EnsureNarratorOn() → CaptureNarratorLastSpoken() → RestoreNarratorState()
- [ ] Add `TryCaptureNarratorIfRunning()` - no auto-toggle version
- [ ] Find and replace existing Python-call implementation
- [ ] Verify compilation

### Task 7: Update Python Backup File (Integrate pc_keys.py into pc_output_narrator.py)

**Files:**
- Modify: `TalkBackAutoTest/module/pc_output_narrator.py` - Merge all functions from pc_keys.py into this file

**Steps:**
- [ ] Copy all constants (VK_*, KEY_*) from pc_keys.py to pc_output_narrator.py
- [ ] Copy KEYBDINPUT, INPUT structures from pc_keys.py to pc_output_narrator.py
- [ ] Copy Win32 API declarations (SendInput, GetAsyncKeyState) from pc_keys.py to pc_output_narrator.py
- [ ] Copy all keyboard functions (send_key_event, send_key_chord, toggle_narrator, force_narrator_read) from pc_keys.py to pc_output_narrator.py
- [ ] Update `capture_narrator_last_spoken()` to follow new flow: ForceNarratorRead() → DoNarratorCapture() → GetClipboardText() → FilterNarratorOutput() → **KHÔNG gọi clear_clipboard_history()** (C# sẽ tự xử lý)
- [ ] Add 1 second delay in `force_narrator_read()` before sending keys (already done in Python)
- [ ] Update imports in pc_output_narrator.py to use local functions instead of pc_keys
- [ ] Keep `clear_clipboard_history()` function for external use (called by C# when needed)
- [ ] Verify Python code still works
- [ ] Commit

### Task 8: Update MainForm to Call Python on Exit

**Files:**
- Modify: `TalkBackAutoTest/MainForm.cs`

**Steps:**
- [ ] In `getObjectInScreenPC()`, add call to `ClearClipboardHistoryViaPython()` when exiting loop
- [ ] Exit conditions: `i == 10` or `o == null`
- [ ] Verify compilation

### Task 9: Testing

**Files:**
- Test: Manual testing in application

**Steps:**
- [ ] Run application
- [ ] Ensure Narrator is OFF
- [ ] Call `PCManager.GetNarratorOutput()` (via test button or breakpoint)
- [ ] Verify: Narrator auto-turns ON → element captured → clipboard cleared → Narrator restored to OFF
- [ ] Verify clipboard history is cleared after test loop exits

---

## Verification

### Compilation
- Build solution in Visual Studio
- No errors in Output window

### Runtime Test
1. Start app with Narrator OFF
2. Trigger GetNarratorOutput()
3. Check logs for:
   - "INFO: Narrator is off; auto-enabling for capture"
   - "Narrator restored to previous state"
4. Verify Narrator state is restored

### Edge Cases
- Narrator already running → should use existing session
- Clipboard locked → retry logic should handle
- Capture fails → should return null gracefully
