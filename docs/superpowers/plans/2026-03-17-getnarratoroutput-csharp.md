# getNarratorOutput C# Implementation Plan

> **For agentic workers:** REQUIRED: Use superpowers:subagent-driven-development (if subagents available) or superpowers:executing-plans to implement this plan.

**Goal:** Convert `getNarratorOutput()` from Python to C# as native methods in PCTB.cs (inside `#region Narrator`), including all required helper functions. Also update Python backup file to match C# logic.

**Architecture:** All code integrated directly into PCTB.cs inside `#region Narrator` as static helper methods with Win32 P/Invoke declarations. Python backup file will be updated to match same logic.

**Tech Stack:** C# .NET Framework 4.5, System.Windows.Forms.SendKeys, Win32 Clipboard API

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

    // Vòng lặp chính
    change_focus [shape=box, label="changeFocus()\n(TAB key)"];
    force_read [shape=box, label="ForceNarratorRead()\n(Caps+Tab) - ÉP ĐỌC ELEMENT ⬅️"];
    narrator_output [shape=box, label="getNarratorOutput()\n(Capture + Clear)"];
    process_object [shape=box, label="Process Object"];
    check_exit [shape=diamond, label="Exit\nConditions?"];
    end_loop [shape=doublecircle, label="End Loop"];

    // Clear clipboard options
    clear_current [shape=box, label="ClearCurrentClipboard()\n(Clipboard.Clear) - xóa item đầu"];

    start_loop -> change_focus;
    change_focus -> force_read;
    force_read -> narrator_output;
    narrator_output -> process_object;
    process_object -> clear_current;
    clear_current -> check_exit;
    check_exit -> change_focus [label="Continue"];
    check_exit -> end_loop [label="Exit"];
}
```

**Điểm quan trọng đã cập nhật:**

1. **ForceNarratorRead() (Caps+Tab)** chỉ gọi cho **LẦN ĐẦU TIÊN** (phần tử đầu tiên trong vòng lặp) - đây là phím tắt để ép Narrator đọc element đầu tiên.

2. **Clipboard.Clear() thay vì ClearClipboardHistory():**
   - Mỗi lần capture, clipboard chỉ chứa 1 item (của element vừa được Narrator đọc)
   - `Clipboard.Clear()` xóa **phần tử đầu tiên** (clipboard hiện tại) - trong flow này chỉ có 1 item nên = xóa toàn bộ clipboard hiện tại
   - **KHÔNG cần** `clear_clipboard_history()` vì đã có `Clipboard.Clear()` thay thế

3. **Luồng đúng:**
   - Lần đầu: TAB → ForceNarratorRead() → Capture → checkIssue() → ClearCurrentClipboard()
   - Các lần tiếp theo: TAB → Capture → checkIssue() → ClearCurrentClipboard()

### How Narrator is Started

**Critical: Narrator has no "Start" method — it is toggled via keyboard shortcut.**

Narrator is not a regular application you launch and leave running. It is a **toggle** — the same shortcut that starts it also stops it:

| Action | Shortcut | Notes |
|--------|----------|-------|
| Toggle Narrator **ON/OFF** | `Win + Ctrl + Enter` | Python: `toggle_narrator()` = `SendKeyChord([Ctrl, Win, Return])` |
| Force Narrator **read** current element + copy to clipboard | `Caps Lock + Tab` | Only works when Narrator is already ON |
| Narrator **capture** current element to clipboard | `Caps Lock + Ctrl + X` | Only works when Narrator is already ON |

**Key insight:** `StartNarratorProcess()` does NOT start Narrator. It calls `ToggleNarrator()` (Win+Ctrl+Enter) to **turn Narrator on**. If Narrator is already on, `ToggleNarrator()` turns it off — so we must check `IsNarratorRunning()` first.

### Narrator Startup Flow (Correct Implementation)

```
1. IsNarratorRunning() → false?
   └── SendKeyChord([Ctrl, Win, Return])  ← toggle ON (Caps+Ctrl+X hotkey only works when Narrator is ON)
2. Wait ~200ms
3. IsNarratorRunning() → true? → proceed
   └── false? → retry up to 3x with backoff
```

### Startup Mechanism Comparison

| Approach | Method | Pros | Cons |
|----------|--------|------|------|
| ~~Launch Narrator.exe directly~~ | `Process.Start("narrator.exe")` | ✅ Simple | ❌ Narrator is toggle — launching when already ON turns it OFF |
| ✅ **Toggle via hotkey** | `SendKeyChord([Ctrl, Win, Return])` | ✅ Reliable, matches user behavior | ⚠️ Must check state first |
| Settings API | `SystemParametersInfo(SPI_GETSCREENREADER)` | ✅ Query-only, no side effects | ❌ Read-only — cannot change Narrator state |

### Required Functions (Python → C# Naming)

All functions will be implemented inside `#region Narrator` in PCTB.cs

| Python (snake_case) | C# (PascalCase) | Source File | Purpose |
|---------------------|-----------------|-------------|---------|
| `IsNarratorRunning()` | `IsNarratorRunning()` | pc_output_narrator.py | Check if narrator.exe running |
| `toggle_narrator()` | `ToggleNarrator()` | pc_keys.py | Toggle Narrator on/off (Ctrl+Win+Enter) |
| `force_narrator_read()` | **`ForceNarratorRead()`** | pc_keys.py | **Force read current element (Caps+Tab) - QUAN TRỌNG cho element đầu tiên** |
| `_do_narrator_capture()` | `DoNarratorCapture()` | pc_output_narrator.py | Main capture logic (Caps+Ctrl+X) |
| `capture_narrator_last_spoken()` | `CaptureNarratorLastSpoken()` | pc_output_narrator.py | Full capture workflow |
| `_strip_narrator_confirmation()` | `FilterNarratorOutput()` | pc_output_narrator.py | Remove "copied last phrase to clipboard", "failed to copy to clipboard" messages |
| **`N/A`** | **`ClearCurrentClipboard()`** | System.Windows.Forms.Clipboard | **Clear current clipboard - xóa item đầu = xóa toàn bộ** |
| ~~`clear_clipboard_history()`~~ | ~~`ClearClipboardHistoryViaPython()`~~ | pc_output_narrator.py | ~~**KHÔNG CẦN** - đã có Clipboard.Clear()~~ |
| `send_key_event()` | `SendKeyEvent()` | pc_keys.py | Send single key |
| `send_key_chord()` | `SendKeyChord()` | pc_keys.py | Send multiple keys |

> **Lưu ý quan trọng về Clipboard.Clear():**
> - **ForceNarratorRead()** chỉ cần gọi cho **element ĐẦU TIÊN** (phần tử đầu tiên sau khi TAB sang)
> - **ClearCurrentClipboard()** được gọi **SAU** `checkIssue()` - sau khi so sánh element info vs Narrator log xong (PASS/FAIL)
> - **KHÔNG cần** `GetClipboardSequenceNumber()` vì ForceNarratorRead() đã đảm bảo text là mới
> - **KHÔNG cần** gọi WinRT `clear_clipboard_history()`

---

## Implementation Tasks

### Task 1: Add Win32 Clipboard P/Invoke Layer to PCTB.cs

**Files:**
- Modify: `TalkBackAutoTest/PCTB.cs` - Add at top of class

**Context:**
- **Win32 API đã có sẵn** trong .NET Framework 4.8 - **không cần cài NuGet**
- Chỉ cần khai báo `using System.Runtime.InteropServices;` và `using System.Windows.Forms;`
- `System.Windows.Forms.Clipboard.Clear()` đã có sẵn - dùng được ngay

**Steps:**
- [ ] Add `using System.Runtime.InteropServices;` (nếu chưa có)
- [ ] ~~Add Win32 P/Invoke declarations~~ - **KHÔNG CẦN** vì dùng System.Windows.Forms.Clipboard có sẵn
- [ ] Verify compilation
- [ ] *(Các hàm khác sẽ implement sau nếu cần)*

**Note:** Keyboard input uses Win32 SendInput via P/Invoke (see Task 2). Clipboard operations use `System.Windows.Forms.Clipboard` — no extra P/Invoke needed.

### Task 2: Add Keyboard Input Methods (Win32 SendInput via P/Invoke)

**Files:**
- Modify: `TalkBackAutoTest/PCTB.cs`

**Context:**
- **SendKeys does NOT support the Win key (VK_LWIN)** — `SendKeys.SendWait()` cannot send `Win+...` shortcuts
- Must use **Win32 `SendInput`** via P/Invoke (same approach as Python's `send_key_event()` in `pc_keys.py`)
- Import `SendInput`, `GetAsyncKeyState` from `user32.dll`
- .NET 4.5 does not have `User32.SendInput` built-in — declare it with `[DllImport("user32.dll")]`

**Steps:**
- [ ] Add Win32 P/Invoke declarations: `SendInput`, `GetAsyncKeyState`
- [ ] Add `KEYBDINPUT` and `INPUT` structs (same as Python pc_keys.py)
- [ ] Add `SendKeyEvent(int vkCode)` - Send single key using SendInput
- [ ] Add `SendKeyChord(int[] vkCodes, double holdTime)` - Send key combination using SendInput
- [ ] Add `ToggleNarrator()` - `SendKeyChord([VK_CONTROL, VK_LWIN, VK_RETURN], 0.1s)` (same as Python `toggle_narrator()`)
- [ ] Add `ForceNarratorRead()` - **Caps+Tab** for first element (sleep 1s before sending to avoid repeat)
- [ ] Verify compilation

### Task 3: Add Clipboard Operations

**Files:**
- Modify: `TalkBackAutoTest/PCTB.cs`

**Steps:**
- [ ] Add `GetClipboardText()` - open, read, close clipboard (sử dụng System.Windows.Forms.Clipboard)
- [ ] Add `ClearCurrentClipboard()` - xóa nội dung hiện tại (System.Windows.Forms.Clipboard.Clear())
- [ ] ~~Add `GetClipboardSequenceNumber()`~~ - **KHÔNG CẦN** vì ForceNarratorRead() đã đảm bảo text mới
- [ ] ~~Add `ClearClipboardHistoryViaPython()`~~ - **KHÔNG CẦN**
- [ ] Verify compilation

**Note:**
- **KHÔNG cần** `GetClipboardSequenceNumber()` vì `ForceNarratorRead()` đã đảm bảo text lấy được là mới
- **KHÔNG cần** gọi WinRT `clear_clipboard_history()`

### Task 4: Add Narrator Process Management

**Files:**
- Modify: `TalkBackAutoTest/PCTB.cs`

**Context:**
- **CRITICAL: Narrator is a toggle — `StartNarratorProcess()` calls `ToggleNarrator()` (Win+Ctrl+Enter), NOT `Process.Start("narrator.exe")`**
- Launching narrator.exe directly when Narrator is already ON will turn it OFF
- Always use keyboard shortcut to toggle Narrator state

**Steps:**
- [ ] Add `IsNarratorRunning()` - check `Process.GetProcessesByName("Narrator")` for Narrator.exe
- [ ] Add `WaitForNarratorState(bool expectedRunning)` - retry with 200ms backoff, max 3 attempts
- [ ] Add `StartNarratorProcess()` — **FIXED**: Send hotkey `Win+Ctrl+Enter` via `SendKeyChord()` to toggle Narrator ON
  ```csharp
  // CORRECT: toggle via shortcut (Win+Ctrl+Enter)
  SendKeyChord("^(%){ENTER}");  // Ctrl+Alt+Enter in SendKeys format
  // OR use Win32 SendInput equivalent in C#
  SendKeyChord(ctrl: true, win: true, key: RETURN);

  // WRONG: Process.Start("narrator.exe") — DON'T DO THIS
  ```
- [ ] Add `EnsureNarratorOn()` - check IsNarratorRunning(), call StartNarratorProcess() if off, return bool
- [ ] Add `RestoreNarratorState(bool? autoEnabled)` - if autoEnabled==true (we started it), call ToggleNarrator() to turn it off
- [ ] Verify compilation

**Key code pattern:**
```csharp
public bool StartNarratorProcess() {
    // Narrator is a toggle — only send if not already running
    if (IsNarratorRunning()) return true;

    // Send Win+Ctrl+Enter to toggle Narrator ON
    SendKeyChord(VK_CONTROL, VK_LWIN, VK_RETURN, NARRATOR_TOGGLE_HOLD);

    // Wait for Narrator to initialize
    System.Threading.Thread.Sleep(200);
    return WaitForNarratorState(expectedRunning: true);
}
```

### Task 5: Add Capture Implementation

**Files:**
- Modify: `TalkBackAutoTest/PCTB.cs`

**Steps:**
- [ ] Add `FilterNarratorOutput(string text)` - remove "copied last phrase to clipboard", "failed to copy to clipboard" messages
- [ ] Add `DoNarratorCapture()` - main capture logic with Caps+Ctrl+X
- [ ] Add `TryNarratorCapture()` - with retry logic
- [ ] Add `CaptureNarratorLastSpoken()` - workflow: DoNarratorCapture() → GetClipboardText() → FilterNarratorOutput()

**Note:**
- **ForceNarratorRead()** được gọi ở MainForm (Task 8), không cần trong hàm này
- **ClearCurrentClipboard()** được gọi ở MainForm sau checkIssue(), không cần trong hàm này
- **ForceNarratorRead()** chỉ cần cho **LẦN ĐẦU TIÊN** (phần tử đầu tiên)
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
- [ ] Update flow trong `dumpScreen()`:
  - **Lần đầu tiên:** `force_narrator_read()` → `capture_narrator_last_spoken()` → **`Clipboard.Clear()`**
  - **Các lần tiếp theo:** `capture_narrator_last_spoken()` → **`Clipboard.Clear()`**
  - **Logic mới:** `Clipboard.Clear()` xóa phần tử đầu tiên - trong flow này chỉ có 1 item nên = xóa toàn bộ clipboard hiện tại
- [ ] Add 1 second delay in `force_narrator_read()` before sending keys (already done in Python)
- [ ] Update imports in pc_output_narrator.py to use local functions instead of pc_keys
- [ ] ~~Remove `clear_clipboard_history()` function~~ - **KHÔNG cần** vì đã có `Clipboard.Clear()`
- [ ] Verify Python code still works
- [ ] Commit

### Task 8: Update MainForm Loop Flow (CRITICAL - Thêm ForceNarratorRead)

**Files:**
- Modify: `TalkBackAutoTest/MainForm.cs`

**Context luồng hiện tại:**
```csharp
PCManager.changeFocus();
PCManager.getNarratorOutput();
Thread.Sleep(5000);
string objElement = PCManager.dumpFocusedObject();
PCManager.dumpScreen();
PCManager.checkIssue("1", "2");
```

**Luồng MỚI cần implement:**
```csharp
// Lần đầu tiên - ÉP ĐỌC element đầu
PCManager.changeFocus();
PCManager.ForceNarratorRead();  // ← THÊM - Chỉ cho element ĐẦU TIÊN
PCManager.getNarratorOutput();
Thread.Sleep(5000);
string objElement = PCManager.dumpFocusedObject();
PCManager.dumpScreen();
PCManager.checkIssue("1", "2");
PCManager.ClearCurrentClipboard();  // ← THÊM - Sau khi so sánh xong
```

**Steps:**
- [ ] Trong `getObjectInScreenPC()`, thêm `PCManager.ForceNarratorRead()` **SAU** `PCManager.changeFocus()` (chỉ cho element ĐẦU TIÊN trong vòng lặp)
- [ ] Thêm `PCManager.ClearCurrentClipboard()` **SAU** `PCManager.checkIssue("1", "2")` - sau khi so sánh element info vs Narrator log xong (PASS/FAIL)
- [ ] ~~Remove `ClearClipboardHistoryViaPython()` when exiting loop~~ - không cần thiết
- [ ] Verify compilation

> **ĐIỂM QUAN TRỌNG:**
> - **ForceNarratorRead()** chỉ cần cho element ĐẦU TIÊN - để đảm bảo Narrator đọc đúng element
> - **ClearCurrentClipboard()** đặt SAU checkIssue() để đảm bảo so sánh xong mới xóa

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
