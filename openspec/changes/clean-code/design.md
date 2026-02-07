## Context

Mục tiêu là sạch hóa các module `pc_automation_*` để dễ bảo trì và an toàn hơn khi thiếu thư viện phụ trợ, đồng thời giữ nguyên hành vi và output CLI hiện tại.

## Goals / Non-Goals

**Goals:**
- Chuẩn hóa cấu trúc hàm và tên hằng số để dễ đọc.
- Xử lý thiếu `uiautomation`, `psutil`, `win32clipboard` an toàn (fallback + cảnh báo).
- Không thay đổi hợp đồng output/exit code của CLI.

**Non-Goals:**
- Thêm tính năng mới cho CLI.
- Thay đổi logic nghiệp vụ hoặc format JSON hiện có.

## Decisions

- Dùng import an toàn với `try/except` cho các thư viện tuỳ chọn để tránh crash khi thiếu.
- Tách các helper nhỏ trong CLI để giảm lặp và rõ luồng capture.
- Đặt hằng số delay/flag có tên rõ ràng để giảm magic numbers.

## Risks / Trade-offs

- [Risk] Khác biệt nhỏ về log khi thiếu thư viện → Mitigation: chỉ thêm cảnh báo, không đổi luồng chính.
- [Risk] Thay đổi formatting ảnh hưởng diff → Mitigation: giữ nguyên cấu trúc dữ liệu và output.
