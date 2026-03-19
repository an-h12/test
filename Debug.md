# Debug Notes - TalkBackAutoTest

Ngày: 2026-03-19
Project: `C:\Users\Haha\Desktop\Talk\owner\Talk-new\TalkBackAutoTest`

---

## 1. ComboBox trắng — Dropdown chọn app không hiển thị danh sách

**Mô tả:** Khi click vào dropdown "App Name For Testing" thì trắng tinh, không có app nào hiển thị.

**Nguyên nhân:** Hàm `GetRunningUIApplications()` trong `PCTB.cs` chỉ gọi API server `BASE_URL + "/api/system/ui-apps"`. Nếu server không chạy → `WebException` → trả về list rỗng → ComboBox trắng.

**Files đã sửa:**

### `PCTB.cs` — `GetRunningUIApplications()` (dòng ~941)
- Phần API call đã bị comment (không phụ thuộc server)
- Fallback dùng `Process.GetProcesses()` trực tiếp đang active
- Thêm debug log: `Console.WriteLine("GetRunningUIApplications: found X apps (of Y with window handle)")`
- Xử lý exception đúng cách

### `MainForm.cs` — `updateListApponComboBox()` (dòng ~8850)
**Vấn đề:**
1. `SelectedIndexChanged +=` được gắn MỖI LẦN gọi hàm → nhiều event handlers trùng nhau
2. Không reset hoàn toàn `DataSource` trước khi gán mới
3. Không có log khi list rỗng

**Fix:**
- Reset hoàn toàn `DataSource`, `Items`, `DisplayMember`, `ValueMember` trước khi gán
- Thêm check `listApp.Count == 0` với log "Không tìm thấy app nào đang chạy."
- Log số app tìm được: `"Đã tìm thấy {X} app đang chạy."`

---

## 2. STA Thread Error — `Current thread must be set to single thread apartment (STA) mode`

**Mô tả:** Khi ấn Run, log hiển thị:
```
getObjectInScreen Inside:Current thread must be set to single thread apartment (STA) mode before OLE calls can be made.
```

**Nguyên nhân:** `RunProjectWithNewThreadPC` được chạy trên background thread mới. Background thread mặc định là **MTA (Multi-Threaded Apartment)**, nhưng `Clipboard.GetText()` / `Clipboard.SetText()` trong các hàm Narrator capture yêu cầu **STA**.

**File đã sửa:** `MainForm.cs` — `RunPCMode()` (dòng ~3641)

**Fix:**
```csharp
// TRƯỚC
threadRunProject = new Thread(RunProjectWithNewThreadPC);
threadRunProject.Name = "Run Project with new Thread";
if (!threadRunProject.IsAlive)
    threadRunProject.Start();

// SAU
threadRunProject = new Thread(RunProjectWithNewThreadPC);
threadRunProject.Name = "Run Project with new Thread";
threadRunProject.SetApartmentState(ApartmentState.STA); // Required for Clipboard/OLE calls
if (!threadRunProject.IsAlive)
    threadRunProject.Start();
```

---

## 3. `_user32` does not exist in the current context

**Mô tả:** Lỗi compile khi dùng `_user32.GetForegroundWindow()`.

**Nguyên nhân:** `_user32` chưa được khai báo. Code cũ dùng `windll.user32` nhưng bị comment.

**File đã sửa:** `PCTB.cs` (dòng ~308)

**Fix:** Thêm DllImport trực tiếp:
```csharp
[DllImport("user32.dll")]
private static extern IntPtr GetForegroundWindow();
```
Sau đó gọi `GetForegroundWindow()` trực tiếp (không qua `_user32`).

---

## 4. `Unexpected character: T` khi ấn Run

**Mô tả:**
```
getObjectInScreen Inside:Unexpected character encountered while parsing value: T. Path '', line 0, position 0.
getObjectInScreen Inside:Thread was being aborted.
```

**Nguyên nhân:** `dumpFocusedObject()` gọi API `/api/ui/focused_element` — server trả HTML (404/502) thay vì JSON → `JsonConvert.DeserializeObject` fail → trả "NA" → test không capture được gì.

**File đã sửa:** `PCTB.cs` — `dumpFocusedObject()` (dòng ~729)

**Fix:**
1. Giữ API call nếu `BaseUrl` có giá trị
2. Nếu API fail hoặc `BaseUrl` rỗng → dùng **UI Automation fallback** trực tiếp
3. Thêm hàm `GetFocusedElementFromWindow(IntPtr windowHandle)`:
   - Dùng `GetForegroundWindow()` lấy cửa sổ đang active
   - Dùng `AutomationElement.FromHandle()` để query UI tree
   - Trả về JSON string với `{name, className, automationId, controlType, isEnabled, isVisible}`
4. Thêm hàm `EscapeJson()` để escape ký tự đặc biệt trong JSON

---

## 5. Không chuyển sang App cần test khi ấn Run

**Mô tả:** App không tự động activate cửa sổ app được chọn để test.

**Nguyên nhân:** Tất cả các hàm PC mode đều phụ thuộc API server `BASE_URL`. Nếu server không chạy → tất cả đều fail âm thầm.

**Files đã sửa:**

### `PCTB.cs` — Imports mới (dòng ~308)
```csharp
[DllImport("user32.dll")]
private static extern IntPtr GetForegroundWindow();

[DllImport("user32.dll")]
private static extern int SetForegroundWindow(IntPtr hWnd);

[DllImport("user32.dll")]
private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

[DllImport("user32.dll")]
private static extern bool IsIconic(IntPtr hWnd);

[DllImport("user32.dll")]
private static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

[DllImport("user32.dll", CharSet = CharSet.Unicode)]
private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);

private const int SW_RESTORE = 9;
private const int SW_SHOW = 5;
```

### `PCTB.cs` — `ActivateWindowByPID(int pid)` (dòng ~231)
- Thử API server trước (nếu `BaseUrl` không rỗng)
- Fallback: Windows API
  1. Lấy `MainWindowHandle` từ PID qua `Process.GetProcessById()`
  2. Nếu cửa sổ minimized → `ShowWindow(hWnd, SW_RESTORE)`
  3. Nếu không → `ShowWindow(hWnd, SW_SHOW)`
  4. `SetForegroundWindow(hWnd)` để bring lên foreground
  5. Sleep 100ms để đợi cửa sổ active

### `PCTB.cs` — `getWindowTitle(int pId)` (dòng ~119)
- Thử API server trước
- Fallback: `Process.GetProcessById(pId).MainWindowTitle`

### `PCTB.cs` — `changeFocus()` (dòng ~688)
- Thử API server trước
- Fallback: `keybd_event(VK_TAB, 0, 0)` → press Tab → `keybd_event(VK_TAB, 0, KEYEVENTF_KEYUP)` → release Tab

### `PCTB.cs` — `dumpFocusedObject()` (dòng ~729)
- Thử API server trước
- Fallback: UI Automation trực tiếp (đã mô tả ở mục 4)

---

## 6. Talkback Text không hiển thị — Cột trống trong bảng kết quả

**Mô tả:** Cột "Talkback Text" trong bảng kết quả hiển thị giá trị hardcode `"TalkbackText"` thay vì nội dung Narrator thực tế đọc được.

**Nguyên nhân:** Hàm `getFocusedObjectFake()` nhận 4 tham số và luôn truyền `"talkbackText"` cố định vào Object constructor — không có cách truyền giá trị Narrator thực tế.

**Files đã sửa:**

### `PCTB.cs` — `getFocusedObjectFake()` (dòng ~1100)

**Fix:** Thêm overload mới với param `talkbackText`:
```csharp
public Object getFocusedObjectFake(int objIdx, string objElement, string windowTitle, PkgInfo pkgInfo)
{
    return getFocusedObjectFake(objIdx, objElement, windowTitle, pkgInfo, "TalkbackText_NA");
}

public Object getFocusedObjectFake(int objIdx, string objElement, string windowTitle, PkgInfo pkgInfo, string talkbackText)
{
    // ... logic giữ nguyên ...
    return new Object(..., talkbackText, ...); // truyền biến thay vì hardcode
}
```

### `MainForm.cs` — `getObjectInScreenPC()` (dòng ~2501)

**Fix:** Bắt kết quả Narrator vào biến, giảm delay:
```csharp
// TRƯỚC
PCManager.GetNarratorOutput();          // bỏ kết quả
Thread.Sleep(5000);                       // delay 5s

// SAU
string narratorOutput = PCManager.GetNarratorOutput();
// PCManager.StopRecordVideo();         // tạm comment
Thread.Sleep(1000);                       // delay 1s
```

Sau đó truyền vào Object:
```csharp
Object o = PCManager.getFocusedObjectFake(
    numberOfObject + 1, objElement, windowTitle, pkgInfo,
    narratorOutput ?? "TalkbackText_NA");
```

### `PCTB.cs` — `GetNarratorOutput()` (dòng ~632)

**Fix:** Bỏ auto-toggle Narrator — Narrator phải GIỮ BẬT suốt vòng loop:
```csharp
public string GetNarratorOutput()
{
    if (!IsNarratorRunning())
    {
        Console.WriteLine("GetNarratorOutput: Narrator chua bat - bo qua capture");
        return null;
    }
    return CaptureNarratorLastSpoken();
}
```

**Lưu ý:** Giảm `Thread.Sleep` từ 5 giây xuống **1 giây** để tăng tốc độ test.

**Luồng hoạt động đúng:**
```
Iteration 1:
  ForceNarratorRead()  → Caps+Tab (đọc phần tử đầu tiên)
  Sleep(1000)
  Tab()                → Di chuyển sang phần tử tiếp theo
  Sleep(1000)
  GetNarratorOutput() → Caps+Ctrl+X (capture — KHÔNG toggle Narrator)
  Sleep(1000)
  dumpFocusedObject() → Lấy thông tin UI element

Iteration 2+:
  Tab()                → Di chuyển sang phần tử mới (Narrator vẫn bật)
  Sleep(1000)
  GetNarratorOutput() → Caps+Ctrl+X (capture — Narrator đang bật suốt)
  Sleep(1000)
  dumpFocusedObject() → Lấy thông tin UI element

Narrator state:
  StartNarratorNVDA()  → bật 1 lần ở RunProjectWithNewThreadPC
  GetNarratorOutput() → KHÔNG tắt (đã fix)
  StopNarratorNVDA()  → tắt 1 lần khi kết thúc test
```

---

## 7. Dọn dead code — 4 hàm không còn sử dụng

**Mô tả:** Sau khi refactor `GetNarratorOutput()` bỏ auto-toggle, 4 hàm sau trở thành dead code:

**Hàm đã xóa:**

| Hàm | Lý do xóa |
|------|-----------|
| `StartNarratorProcess()` | Chỉ được gọi bởi `EnsureNarratorOn`, không có caller |
| `EnsureNarratorOn()` | Không có caller — luồng mới dùng `StartNarratorNVDA` |
| `RestoreNarratorState(bool)` | Chỉ được gọi bởi `GetNarratorOutput` cũ, không có caller |
| `WaitForNarratorState(bool, int)` | Chỉ được gọi bởi 3 hàm trên, không có caller |

---

## Tổng kết các thay đổi

| # | Issue | File | Dòng | Thay đổi |
|---|-------|------|------|-----------|
| 1 | ComboBox trắng | `PCTB.cs` | ~941 | Fallback `Process.GetProcesses()` + debug log |
| 1 | ComboBox trắng | `MainForm.cs` | ~8850 | Reset binding + log count + bỏ duplicate event handler |
| 2 | STA thread error | `MainForm.cs` | ~3641 | Thêm `SetApartmentState(STA)` cho thread PC |
| 3 | `_user32` not found | `PCTB.cs` | ~308 | Thêm `DllImport GetForegroundWindow` |
| 4 | JSON parse error | `PCTB.cs` | ~729 | Thêm UI Automation fallback cho `dumpFocusedObject` |
| 5 | App không activate | `PCTB.cs` | ~308 | Thêm DllImports: `SetForegroundWindow`, `ShowWindow`, `IsIconic`, `GetWindowText`, constants |
| 5 | App không activate | `PCTB.cs` | ~231 | Fix `ActivateWindowByPID` với Windows API fallback |
| 5 | App không activate | `PCTB.cs` | ~119 | Fix `getWindowTitle` với `Process.MainWindowTitle` fallback |
| 5 | App không activate | `PCTB.cs` | ~688 | Fix `changeFocus` với `keybd_event` fallback |
| 6 | Talkback Text trống | `PCTB.cs` | ~1100 | Thêm overload `getFocusedObjectFake` với param `talkbackText` |
| 6 | Talkback Text trống | `MainForm.cs` | ~2501 | Capture Narrator output + truyền vào Object + giảm delay xuống 1s |
| 6 | Talkback Text trống | `MainForm.cs` | ~2504 | Comment `StopRecordVideo()` (xử lý riêng) |
| 6 | Talkback Text trống | `PCTB.cs` | ~632 | Fix `GetNarratorOutput()` bỏ auto-toggle — Narrator giữ BẬT suốt vòng loop |
| 7 | Dead code | `PCTB.cs` | ~502-534 | Xóa 4 hàm: `StartNarratorProcess`, `EnsureNarratorOn`, `RestoreNarratorState`, `WaitForNarratorState` |

---

## Nguyên tắc chung đã áp dụng

1. **Fallback pattern:** Luôn thử API server trước, nếu fail thì dùng Windows API trực tiếp
2. **Kiểm tra `BaseUrl`:** Nếu `string.IsNullOrEmpty(BaseUrl)` → bỏ qua API, dùng fallback ngay
3. **Debug log:** Thêm `Console.WriteLine` ở mọi nơi có thể fail để dễ trace
4. **STA thread:** Tất cả thread chạy PC mode phải có `SetApartmentState(STA)`
