## Tasks: Fix UI Automation Pattern Mappings

### 1. Add SelectionItemPattern handler function
**File:** [pc_automation.py](../../../TalkBackAutoTest/module/pc_automation.py)
**Location:** After `_extract_expand_collapse_pattern` function (around line 95)

Add new handler:
```python
def _extract_selection_item_pattern(element):
    """Extract IsSelected from SelectionItemPattern. Returns 'Selected'/'NotSelected' or None."""
    try:
        if element.GetPattern(PatternId.SelectionItemPattern):
            pattern = element.GetSelectionItemPattern()
            return 'Selected' if pattern.IsSelected else 'NotSelected'
    except Exception as e:
        print(f"WARNING: SelectionItem pattern extraction failed: {e}", file=sys.stderr)
        return None
    return None
```

### 2. Fix RadioButton mapping and add new control types
**File:** [pc_automation.py](../../../TalkBackAutoTest/module/pc_automation.py)
**Location:** CONTROL_PATTERNS dictionary (around line 98)

Replace the entire CONTROL_PATTERNS dictionary:
```python
# --- Control Type to Pattern Mapping ---
CONTROL_PATTERNS = {
    50004: ['Value'],                          # EditControl
    50030: ['Value'],                          # DocumentControl
    50002: ['ToggleState'],                    # CheckBoxControl
    50003: ['IsSelected'],                     # RadioButtonControl (FIXED: was ToggleState)
    50000: ['ToggleState'],                    # ButtonControl (toggle variant) (NEW)
    50006: ['Value', 'ExpandCollapseState'],   # ComboBoxControl
    50007: ['IsSelected', 'ToggleState', 'ExpandCollapseState'],  # ListItemControl (NEW)
    50025: ['ExpandCollapseState'],            # TreeItemControl (NEW)
}
```

**Critical changes:**
- Line for `50003` (RadioButtonControl): Change from `['ToggleState']` to `['IsSelected']`
- Add line for `50000` (ButtonControl)
- Add line for `50007` (ListItemControl)
- Add line for `50025` (TreeItemControl)

### 3. Register IsSelected handler in PATTERN_HANDLERS
**File:** [pc_automation.py](../../../TalkBackAutoTest/module/pc_automation.py)
**Location:** PATTERN_HANDLERS dictionary (around line 107)

Add new entry to PATTERN_HANDLERS:
```python
# Map pattern names to handler functions
PATTERN_HANDLERS = {
    'Value': _extract_value_pattern,
    'ToggleState': _extract_toggle_pattern,
    'ExpandCollapseState': _extract_expand_collapse_pattern,
    'IsSelected': _extract_selection_item_pattern,  # NEW
}
```

### 4. Update result initialization in get_focused_element_info
**File:** [pc_automation.py](../../../TalkBackAutoTest/module/pc_automation.py)
**Location:** Inside `get_focused_element_info()` function (around line 165)

Find the section that initializes pattern properties:
```python
# Initialize all pattern properties to None
result['Value'] = None
result['ToggleState'] = None
result['ExpandCollapseState'] = None
```

Add the new property:
```python
# Initialize all pattern properties to None
result['Value'] = None
result['ToggleState'] = None
result['ExpandCollapseState'] = None
result['IsSelected'] = None  # NEW
```

### 5. Update get_focused_element_info docstring
**File:** [pc_automation.py](../../../TalkBackAutoTest/module/pc_automation.py)
**Location:** Docstring of `get_focused_element_info()` function (around line 119)

Replace the docstring:
```python
def get_focused_element_info():
    """
    Get information about currently focused UI element.
    
    Returns dict with base properties + control-specific patterns:
    - Always: Name, LocalizedControlType
    - Type-specific: Value, ToggleState, ExpandCollapseState, IsSelected (depends on control)
    
    Supported control types:
    - EditControl, DocumentControl: Value
    - CheckBoxControl: ToggleState
    - RadioButtonControl: IsSelected
    - ButtonControl (toggle variant): ToggleState
    - ComboBoxControl: Value + ExpandCollapseState
    - ListItemControl: IsSelected + ToggleState + ExpandCollapseState (tries all)
    - TreeItemControl: ExpandCollapseState
    
    Returns None if no element is focused or uiautomation not available.
    Prints errors to stderr if pattern extraction fails.
    """
```

### 6. Test the fixes
**Manual verification:**

1. **Test RadioButton (critical fix):**
   ```bash
   # Focus a radio button in any app (e.g., Windows Settings)
   python module/pc_automation.py get_focused
   ```
   Expected output:
   ```json
   {"Name": "...", "LocalizedControlType": "radio button", "Value": null, "ToggleState": null, "ExpandCollapseState": null, "IsSelected": "Selected"}
   ```
   Verify `IsSelected` is **not** `null`.

2. **Test ListItem (simple selection):**
   ```bash
   # Focus a list item (e.g., in File Explorer sidebar)
   python module/pc_automation.py get_focused
   ```
   Expected output should include `IsSelected` with value.

3. **Test TreeItem:**
   ```bash
   # Focus a tree item (e.g., expanded folder in File Explorer)
   python module/pc_automation.py get_focused
   ```
   Expected output should include `ExpandCollapseState`.

4. **Regression test - CheckBox still works:**
   ```bash
   # Focus a checkbox
   python module/pc_automation.py get_focused
   ```
   Expected output should still have `ToggleState` working correctly.

5. **Test with Tab command:**
   ```bash
   # Navigate through controls with Tab
   python module/pc_automation.py tab
   ```
   Each line of JSON output should show correct pattern values for each control type.

### 7. Update documentation (optional but recommended)
**File:** [README_DEV.txt](../../../TalkBackAutoTest/module/README_DEV.txt)

Update the section describing `get_focused` command to reflect:
- New `IsSelected` property
- New supported control types (RadioButton, ListItem, TreeItem, Button)
- Correct pattern mappings

---

**Dependencies:**
- Tasks 1-5 must be completed in order (each builds on previous)
- Task 6 (testing) depends on tasks 1-5
- Task 7 can be done in parallel after task 5

**Estimated time:**
- Tasks 1-5: ~15-20 minutes (straightforward edits)
- Task 6: ~10-15 minutes (manual testing)
- Task 7: ~10 minutes (documentation update)
- **Total: ~40 minutes**
