## 0. Spike: Research PositionInSet API

- [x] 0.1 Create test script với List control (5 items) hoặc dùng existing app
- [x] 0.2 Focus vào item thứ 3 trong list
- [x] 0.3 Test API: `pos = element.GetPropertyValue(PropertyId.PositionInSetProperty)` (30152) và `size = element.GetPropertyValue(PropertyId.SizeOfSetProperty)` (30153)
- [x] 0.4 Document behavior:
  - Return value khi property IS set: integer (e.g., pos=3, size=5)
  - Return value khi property NOT set: `0` (int)
  - No exceptions thrown
- [x] 0.5 Update implementation approach in task 1.3 based on findings

## 1. Upgrade Base Property Extraction

- [ ] 1.1 In `get_focused_element_info`, extract `element.IsEnabled` (bool). **(DEFERRED - commented out)**
- [ ] 1.2 In `get_focused_element_info`, extract `element.ItemStatus` (str). **(DEFERRED - commented out)**
- [x] 1.3 Implement logic to extract `PositionInSet` and `SizeOfSet`:
  - Use `element.GetPropertyValue(PropertyId.PositionInSetProperty)` (30152)
  - Use `element.GetPropertyValue(PropertyId.SizeOfSetProperty)` (30153)
  - No try-except needed (returns 0 when unsupported)
  - Return as dict `{ 'Index': val, 'Total': val }` if both values > 0
  - Return None if values are 0

## 2. Main Function Integration

- [x] 2.1 Update `get_focused_element_info` to merge `Position` field into the result dictionary. **(IsEnabled, ItemStatus deferred)**
- [x] 2.2 Verify handling of `None`/Exception cases for Position extraction (return None if properties not supported).

## 3. Verification

- [x] 3.1 Compile check: `python -m py_compile TalkBackAutoTest/module/pc_automation.py`
- [x] 3.2 Test với List Item: Verify JSON output includes `Position` { Index, Total } if supported by app.
- [ ] 3.3 Test với disabled control: Verify `IsEnabled` = false. **(DEFERRED)**
- [ ] 3.4 Test với control có ItemStatus: Verify JSON output includes `ItemStatus`. **(DEFERRED)**

## 4. Out of Scope (Future Changes)

The following are explicitly deferred to future tasks:
- **IsEnabled extraction** (code commented out - needs review)
- **ItemStatus extraction** (code commented out - needs review)
- Pattern-based extraction: RangeValuePattern, GridItemPattern, TableItemPattern, LegacyIAccessiblePattern
- Automated testing strategy with Narrator comparison

## 5. Refactoring (Completed Post-Archive)

- [x] 5.1 Refactor `get_focused_element_info` to Option B (Only Include Set Values):
  - JSON output chỉ chứa fields có giá trị thực sự (không còn null fields)
  - Xóa hardcoded pattern initialization (`result['Value'] = None`, etc.)
  - Dynamic pattern extraction từ `PATTERN_HANDLERS` keys
  - Giảm code từ 51 lines xuống 38 lines (-25%)
- [x] 5.2 Update README_DEV.txt với JSON format mới:
  - Documented new minimal output format
  - Updated ví dụ output (không còn `"Value": null`)
  - Updated control type mappings với Control Type IDs
