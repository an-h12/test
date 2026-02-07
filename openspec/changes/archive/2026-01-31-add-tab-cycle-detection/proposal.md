## Why

Lệnh `tab` trong `pc_automation.py` hiện tại chạy vô hạn cho đến khi người dùng nhấn ESC. Điều này không thuận tiện và không an toàn cho automated testing - nếu quên nhấn ESC hoặc script chạy tự động, vòng lặp sẽ chạy mãi. UI Automation cung cấp `RuntimeId` - một identifier duy nhất cho mỗi element - cho phép detect cycle tự động khi Tab quay lại element đã gặp.

## What Changes

- Thêm cycle detection vào `run_tab_sequence()` trong `pc_automation.py`
- Track các `RuntimeId` đã gặp trong một `set()`
- Tự động dừng khi phát hiện element lặp lại (cycle detected)
- Giữ nguyên JSON output format - không thêm metadata
- Giữ nguyên ESC mechanism - người dùng vẫn có thể dừng manual
- Handle edge cases: RuntimeId không có (skip cycle check + warning), focus lost (break)
- Log thông tin cycle detection ra stderr khi detect

## Capabilities

### New Capabilities
- `tab-cycle-detection`: Tự động phát hiện và dừng khi Tab loop quay về element đã gặp, sử dụng RuntimeId từ UI Automation API

### Modified Capabilities
<!-- Không có capability hiện tại nào bị thay đổi requirements -->

## Impact

**Affected code:**
- `TalkBackAutoTest/module/pc_automation.py` - hàm `run_tab_sequence()`

**Behavior changes:**
- Lệnh `tab` sẽ tự động dừng khi detect cycle (thay vì chạy vô hạn)
- Output vẫn giống cũ: JSON trên stdout, logs trên stderr
- Mỗi element chỉ xuất hiện 1 lần trong output (không có duplicate)

**No breaking changes:**
- JSON schema không thay đổi
- CLI interface không thay đổi
- C# integration code không cần update
