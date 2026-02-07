## Why

Chuẩn hóa lại code để dễ đọc, dễ bảo trì và an toàn hơn khi thiếu thư viện phụ trợ, đồng thời giữ nguyên hành vi CLI hiện tại.

## What Changes

- Tách và gom các helper nhỏ trong CLI để giảm lặp và rõ luồng xử lý.
- Xử lý thiếu `uiautomation`, `psutil`, `win32clipboard` an toàn hơn (fallback, cảnh báo rõ ràng).
- Đặt hằng số delay/flag rõ tên, giảm comment thừa trong module phím.
- Chuẩn hóa định dạng hàm/khối code cho dễ đọc.

## Capabilities

### New Capabilities
- `all-specs`: Summary spec cho các thay đổi clean code trong module pc_automation.

### Modified Capabilities
- None

## Impact

- `TalkBackAutoTest/module/pc_automation_cli.py`
- `TalkBackAutoTest/module/pc_automation_uia.py`
- `TalkBackAutoTest/module/pc_automation_clipboard.py`
- `TalkBackAutoTest/module/pc_automation_keys.py`
- `TalkBackAutoTest/module/pc_automation_core.py`
