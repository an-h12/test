## Design: Fix UI Automation Pattern Mappings

### Architecture Overview

The fix maintains the existing hybrid handler-based pattern extraction architecture while adding support for SelectionItemPattern and correcting control type mappings.

```
pc_automation.py Structure (Modified Sections):
├─ [SECTION 1] Imports (unchanged)
├─ [SECTION 2] Keyboard Control (unchanged)
├─ [SECTION 3] UI Automation
│   ├─ Constants (unchanged)
│   ├─ Pattern Handlers
│   │   ├─ _extract_value_pattern (existing)
│   │   ├─ _extract_toggle_pattern (existing)
│   │   ├─ _extract_expand_collapse_pattern (existing)
│   │   └─ _extract_selection_item_pattern (NEW)
│   ├─ Control Type Mapping (MODIFIED)
│   │   ├─ CONTROL_PATTERNS - updated mappings
│   │   └─ PATTERN_HANDLERS - added IsSelected handler
│   └─ get_focused_element_info (MODIFIED)
│       ├─ Result initialization - add IsSelected
│       └─ Docstring update
└─ [SECTION 4] CLI (unchanged)
```

### Core Changes

#### 1. New SelectionItemPattern Handler

**Location:** After existing pattern handlers (~line 90)

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

**Pattern ID:** `PatternId.SelectionItemPattern` (should be available in uiautomation library)

**Return Values:**
- `"Selected"` - when `IsSelected` property is `True`
- `"NotSelected"` - when `IsSelected` property is `False`  
- `None` - when pattern not supported or extraction fails

#### 2. Updated Control Type Mappings

**Location:** CONTROL_PATTERNS dictionary (~line 98)

**Before:**
```python
CONTROL_PATTERNS = {
    50004: ['Value'],                          # EditControl
    50030: ['Value'],                          # DocumentControl
    50002: ['ToggleState'],                    # CheckBoxControl
    50003: ['ToggleState'],                    # RadioButtonControl ❌ WRONG
    50006: ['Value', 'ExpandCollapseState'],   # ComboBoxControl
}
```

**After:**
```python
CONTROL_PATTERNS = {
    50004: ['Value'],                          # EditControl
    50030: ['Value'],                          # DocumentControl
    50002: ['ToggleState'],                    # CheckBoxControl
    50003: ['IsSelected'],                     # RadioButtonControl ✓ FIXED
    50000: ['ToggleState'],                    # ButtonControl (toggle variant) ✓ NEW
    50006: ['Value', 'ExpandCollapseState'],   # ComboBoxControl
    50007: ['IsSelected', 'ToggleState', 'ExpandCollapseState'],  # ListItemControl ✓ NEW
    50025: ['ExpandCollapseState'],            # TreeItemControl ✓ NEW
}
```

**Changes:**
- Line 4: RadioButton changed from `ToggleState` to `IsSelected` (critical fix)
- Line 5: Added ButtonControl with `ToggleState` for toggle button variant
- Line 7: Added ListItemControl with all three patterns (multi-pattern support)
- Line 8: Added TreeItemControl with `ExpandCollapseState`

#### 3. Updated Handler Registry

**Location:** PATTERN_HANDLERS dictionary (~line 107)

**Before:**
```python
PATTERN_HANDLERS = {
    'Value': _extract_value_pattern,
    'ToggleState': _extract_toggle_pattern,
    'ExpandCollapseState': _extract_expand_collapse_pattern,
}
```

**After:**
```python
PATTERN_HANDLERS = {
    'Value': _extract_value_pattern,
    'ToggleState': _extract_toggle_pattern,
    'ExpandCollapseState': _extract_expand_collapse_pattern,
    'IsSelected': _extract_selection_item_pattern,  # NEW
}
```

#### 4. Updated Result Initialization

**Location:** get_focused_element_info() function (~line 145)

**Before:**
```python
# Initialize all pattern properties to None
result['Value'] = None
result['ToggleState'] = None
result['ExpandCollapseState'] = None
```

**After:**
```python
# Initialize all pattern properties to None
result['Value'] = None
result['ToggleState'] = None
result['ExpandCollapseState'] = None
result['IsSelected'] = None  # NEW
```

#### 5. Updated Docstring

**Location:** get_focused_element_info() function docstring (~line 119)

**Before:**
```python
"""
Get information about currently focused UI element.

Returns dict with base properties + control-specific patterns:
- Always: Name, LocalizedControlType, BoundingRect
- Type-specific: Value, ToggleState, ExpandCollapseState (depends on control)

Returns None if no element is focused or uiautomation not available.
Prints errors to stderr if pattern extraction fails.
"""
```

**After:**
```python
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

### Pattern Detection Logic

The existing control-type aware extraction remains unchanged:

1. Get control type ID from focused element
2. Lookup patterns in `CONTROL_PATTERNS` by control type ID
3. For each pattern name, call corresponding handler from `PATTERN_HANDLERS`
4. Handler tries to get pattern, returns string value or `None`
5. All properties initialized to `None`, only supported patterns get non-null values

**ListItem Multi-Pattern Behavior:**
```
ListItem (50007) → patterns: ['IsSelected', 'ToggleState', 'ExpandCollapseState']

Example 1: Simple list item (only selection)
  → IsSelected: "Selected"
  → ToggleState: None (pattern not supported)
  → ExpandCollapseState: None (pattern not supported)

Example 2: Checkable list item
  → IsSelected: "Selected"
  → ToggleState: "On"
  → ExpandCollapseState: None (pattern not supported)

Example 3: Tree-style expandable list item
  → IsSelected: "NotSelected"
  → ToggleState: None
  → ExpandCollapseState: "Collapsed"
```

### Data Flow Changes

**Before (RadioButton - BROKEN):**
```
RadioButton focused
  ↓
ControlType: 50003
  ↓
Lookup CONTROL_PATTERNS[50003] → ['ToggleState']
  ↓
Call _extract_toggle_pattern()
  ↓
element.GetPattern(PatternId.TogglePattern) → None (RadioButton doesn't have this!)
  ↓
Return None
  ↓
Result: {'ToggleState': None} ❌ WRONG
```

**After (RadioButton - FIXED):**
```
RadioButton focused
  ↓
ControlType: 50003
  ↓
Lookup CONTROL_PATTERNS[50003] → ['IsSelected']
  ↓
Call _extract_selection_item_pattern()
  ↓
element.GetPattern(PatternId.SelectionItemPattern) → valid pattern object ✓
  ↓
pattern.IsSelected → True
  ↓
Return "Selected"
  ↓
Result: {'IsSelected': 'Selected'} ✓ CORRECT
```

### Output Format Examples

```json
// RadioButton (fixed)
{
  "Name": "Male",
  "LocalizedControlType": "radio button",
  "Value": null,
  "ToggleState": null,
  "ExpandCollapseState": null,
  "IsSelected": "Selected"
}

// ListItem (simple selection)
{
  "Name": "Item 1",
  "LocalizedControlType": "list item",
  "Value": null,
  "ToggleState": null,
  "ExpandCollapseState": null,
  "IsSelected": "Selected"
}

// ListItem (with checkbox)
{
  "Name": "Task",
  "LocalizedControlType": "list item",
  "Value": null,
  "ToggleState": "On",
  "ExpandCollapseState": null,
  "IsSelected": "Selected"
}

// TreeItem
{
  "Name": "Folder",
  "LocalizedControlType": "tree item",
  "Value": null,
  "ToggleState": null,
  "ExpandCollapseState": "Collapsed",
  "IsSelected": null
}
```

### Backward Compatibility

**No breaking changes:**
- Existing control types (Edit, CheckBox, ComboBox, Document) unchanged
- JSON structure same: new `IsSelected` property added (always present, often `null`)
- All existing tests continue to work
- C# parsing code doesn't need changes (can ignore `IsSelected` if not used)

### Error Handling

Unchanged - all pattern extraction uses existing try-catch strategy:
- Pattern not supported → returns `None`
- Exception during extraction → logs to stderr, returns `None`
- Unknown control type → logs INFO to stderr, returns base properties with all pattern values as `None`

### Testing Strategy

Manual verification after implementation:
1. Focus RadioButton → verify `IsSelected` is "Selected" or "NotSelected"
2. Focus ListItem in dropdown → verify `IsSelected` is set
3. Focus checkable ListItem → verify both `IsSelected` and `ToggleState` are set
4. Focus TreeItem → verify `ExpandCollapseState` is set
5. Verify existing controls (CheckBox, Edit, ComboBox) still work correctly
