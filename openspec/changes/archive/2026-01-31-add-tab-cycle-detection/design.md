## Context

Module `pc_automation.py` cung cấp lệnh `tab` để tự động nhấn Tab và lấy thông tin element qua UI Automation. Hiện tại vòng lặp Tab chạy vô hạn cho đến khi user nhấn ESC.

**Current state:**
- `run_tab_sequence()`: Vòng lặp while True với ESC check
- Output mỗi element dưới dạng JSON ra stdout
- Test script `tess.py` đã confirm: `element.GetRuntimeId()` hoạt động và trả về tuple unique

**Constraint:**
- Giữ nguyên JSON output schema (không thay đổi format)
- Không breaking changes cho C# integration
- Phải handle edge cases an toàn (RuntimeId None, focus lost)

## Goals / Non-Goals

**Goals:**
- Tự động dừng vòng lặp Tab khi phát hiện cycle (element lặp lại)
- Sử dụng `RuntimeId` để track elements đã gặp
- Giữ output sạch: mỗi element chỉ xuất hiện 1 lần
- Log thông tin cycle detection ra stderr
- Xử lý an toàn các edge cases

**Non-Goals:**
- Không thêm metadata vào JSON output
- Không thay đổi CLI interface
- Không optimize cho performance (RuntimeId lookup đã O(1))
- Không support composite key fallback (quá phức tạp, rủi ro cao)

## Decisions

### Decision 1: Dùng `GetRuntimeId()` thay vì composite key

**Rationale:**
- Test confirmed: `GetRuntimeId()` trả về tuple unique cho mỗi element
- Format: `(ProcessId, HWND, ThreadId?, UniqueElementId)` - phần cuối là unique
- Tuple có thể dùng trong set trực tiếp (hashable)
- Không có false positive như composite key (Name + ControlType)

**Alternatives considered:**
- Composite key `(Name, ControlType, HWND)`: Rủi ro false positive cao (2 button "OK" giống nhau)
- AutomationId: Không phải control nào cũng có, developer phải set

**Decision:** `GetRuntimeId()` là lựa chọn tốt nhất - accurate và reliable.

### Decision 2: Không output element lặp lại khi detect cycle

**Rationale:**
- Output sạch: Mỗi element unique chỉ xuất 1 lần
- C# không cần deduplicate
- Logic rõ ràng: "Cycle detected" = đã gặp lại element đầu tiên

**Alternatives considered:**
- Output element lặp rồi mới break: Dư thừa, gây nhầm lẫn

**Decision:** Break ngay khi detect, không output duplicate.

### Decision 3: Skip cycle check nếu RuntimeId = None

**Rationale:**
- Trường hợp cực kỳ hiếm (modern UIA controls luôn có RuntimeId)
- Composite key fallback phức tạp và không đáng tin cậy
- User vẫn có ESC để dừng manual

**Alternatives considered:**
- Composite key fallback: Phức tạp, rủi ro false positive
- Break immediately: Quá aggressive, mất data

**Decision:** Continue loop nhưng skip cycle check cho element đó, log warning ra stderr.

### Decision 4: Log format và content

**Stdout:** JSON element info (không thay đổi)
**Stderr:** 
- "Press ESC to stop" khi bắt đầu
- "WARNING: No RuntimeId for element X" nếu không lấy được
- "CYCLE DETECTED at element #N. Stopping." khi detect
- "Total unique elements: N" khi kết thúc
- "Tab pressed N time(s)" (như cũ)

**Rationale:** Stderr cho logs/status, stdout cho data - separation of concerns.

## Risks / Trade-offs

### Risk 1: RuntimeId không có trong legacy controls
**Mitigation:** Skip cycle check + warning. User có ESC để dừng. Trường hợp hiếm (modern UIA controls support).

### Risk 2: Focus bị mất giữa chừng
**Mitigation:** Check `element is None` sau `GetFocusedControl()`, break với warning nếu None.

### Risk 3: GetRuntimeId() throw exception
**Mitigation:** Wrap trong try-except, log warning, tiếp tục (skip cycle check cho element đó).

### Risk 4: Performance với set lookup
**Impact:** Negligible - set membership check là O(1), RuntimeId tuple nhỏ (4 số)
**Mitigation:** Không cần - performance impact < 0.001s/element.

### Trade-off: Không có composite key fallback
**Cost:** Nếu RuntimeId không có, không detect cycle được (phải ESC manual)
**Benefit:** Code đơn giản, không có false positive risk
**Justification:** RuntimeId support gần như universal trong Windows modern apps, trade-off hợp lý.

## Implementation Notes

**Core logic:**
```python
seen_runtime_ids = set()

while True:
    if is_escape_pressed(): break
    
    press_tab()
    element = GetFocusedControl()
    if not element: break  # Focus lost
    
    try:
        runtime_id = element.GetRuntimeId()
    except:
        runtime_id = None
    
    if runtime_id:
        if runtime_id in seen_runtime_ids:
            # CYCLE DETECTED
            print("CYCLE DETECTED...", file=sys.stderr)
            break
        seen_runtime_ids.add(runtime_id)
    
    # Output element info như cũ
    element_info = get_focused_element_info()
    print(json.dumps(element_info, ensure_ascii=False))
```

**File changes:**
- `pc_automation.py`: Chỉ update `run_tab_sequence()` function
- No changes to: CLI parsing, element inspection, keyboard automation
