## Tasks: UI Element Inspection

### 1. Add uiautomation imports and constants
**File:** [pc_automation.py](../../../TalkBackAutoTest/module/pc_automation.py)
**Location:** After existing imports (line ~6)

Add:
```python
# UI Automation imports
try:
    import uiautomation as auto
    from uiautomation import ControlType, PatternId
except ImportError:
    auto = None  # Graceful degradation
```

After existing constants (line ~32), add:
```python
# ============================================================================
# SECTION 3: UI AUTOMATION - ELEMENT INSPECTION
# ============================================================================

# --- Constants: Enum to String Mappings ---
TOGGLE_STATE_NAMES = {
    0: 'Off',
    1: 'On',
    2: 'Indeterminate'
}

EXPAND_COLLAPSE_STATE_NAMES = {
    0: 'Collapsed',
    1: 'Expanded',
    2: 'PartiallyExpanded',
    3: 'LeafNode'
}
```

### 2. Implement pattern handler functions
**File:** [pc_automation.py](../../../TalkBackAutoTest/module/pc_automation.py)
**Location:** After constants section

Add three private handler functions:

```python
def _extract_value_pattern(element):
    """Extract Value from ValuePattern. Returns string or None."""
    try:
        if element.GetPattern(PatternId.ValuePattern):
            pattern = element.GetValuePattern()
            return pattern.Value
    except Exception as e:
        print(f"WARNING: Value pattern extraction failed: {e}", file=sys.stderr)
        return None
    return None


def _extract_toggle_pattern(element):
    """Extract ToggleState from TogglePattern. Returns 'On'/'Off'/etc or None."""
    try:
        if element.GetPattern(PatternId.TogglePattern):
            pattern = element.GetTogglePattern()
            state = pattern.ToggleState
            return TOGGLE_STATE_NAMES.get(state, str(state))
    except Exception as e:
        print(f"WARNING: Toggle pattern extraction failed: {e}", file=sys.stderr)
        return None
    return None


def _extract_expand_collapse_pattern(element):
    """Extract ExpandCollapseState. Returns 'Collapsed'/'Expanded'/etc or None."""
    try:
        if element.GetPattern(PatternId.ExpandCollapsePattern):
            pattern = element.GetExpandCollapsePattern()
            state = pattern.ExpandCollapseState
            return EXPAND_COLLAPSE_STATE_NAMES.get(state, str(state))
    except Exception as e:
        print(f"WARNING: ExpandCollapse pattern extraction failed: {e}", file=sys.stderr)
        return None
    return None
```

### 3. Add control type to pattern mapping
**File:** [pc_automation.py](../../../TalkBackAutoTest/module/pc_automation.py)
**Location:** After handler functions

Add:
```python
# --- Control Type to Pattern Mapping ---
CONTROL_PATTERNS = {
    50004: ['Value'],                          # EditControl
    50030: ['Value'],                          # DocumentControl
    50002: ['ToggleState'],                    # CheckBoxControl
    50003: ['ToggleState'],                    # RadioButtonControl
    50006: ['Value', 'ExpandCollapseState'],   # ComboBoxControl
}

# Map pattern names to handler functions
PATTERN_HANDLERS = {
    'Value': _extract_value_pattern,
    'ToggleState': _extract_toggle_pattern,
    'ExpandCollapseState': _extract_expand_collapse_pattern,
}
```

### 4. Implement get_focused_element_info() function
**File:** [pc_automation.py](../../../TalkBackAutoTest/module/pc_automation.py)
**Location:** After mapping constants

Add:
```python
def get_focused_element_info():
    """
    Get information about currently focused UI element.
    
    Returns dict with base properties + control-specific patterns:
    - Always: Name, LocalizedControlType, BoundingRect
    - Type-specific: Value, ToggleState, ExpandCollapseState (depends on control)
    
    Returns None if no element is focused or uiautomation not available.
    Prints errors to stderr if pattern extraction fails.
    """
    if auto is None:
        print("ERROR: uiautomation library not available", file=sys.stderr)
        return None
    
    try:
        # Get currently focused element
        element = auto.GetFocusedControl()
        
        if element is None:
            print("ERROR: No focused element found", file=sys.stderr)
            return None
        
        # Extract base properties (always available)
        result = {
            'Name': element.Name or '',
            'LocalizedControlType': element.LocalizedControlType or '',
        }
        
        # Extract BoundingRect as list
        try:
            rect = element.BoundingRectangle
            if rect:
                result['BoundingRect'] = [rect.left, rect.top, rect.right, rect.bottom]
            else:
                result['BoundingRect'] = None
        except:
            result['BoundingRect'] = None
        
        # Get control type ID
        control_type_id = element.ControlType
        
        # Look up patterns for this control type
        pattern_names = CONTROL_PATTERNS.get(control_type_id, [])
        
        if not pattern_names:
            # Unknown control type - log info and return base properties only
            print(f"INFO: Unknown control type {control_type_id}, returning base properties only", 
                  file=sys.stderr)
        
        # Initialize all pattern properties to None
        result['Value'] = None
        result['ToggleState'] = None
        result['ExpandCollapseState'] = None
        
        # Extract patterns for this control type
        for pattern_name in pattern_names:
            handler = PATTERN_HANDLERS.get(pattern_name)
            if handler:
                result[pattern_name] = handler(element)
        
        return result
        
    except Exception as e:
        print(f"ERROR: Failed to get focused element info: {e}", file=sys.stderr)
        return None
```

### 5. Update CLI interface to handle get_focused command
**File:** [pc_automation.py](../../../TalkBackAutoTest/module/pc_automation.py)
**Location:** In `main()` function, before existing action handlers

Modify the `main()` function to add the new command:

Add `import json` at the top if not already present.

In the `main()` function, add this case before the "narrator" action:
```python
if action == "get_focused":
    result = get_focused_element_info()
    print(json.dumps(result, ensure_ascii=False))
    sys.exit(0 if result is not None else 1)
```

Also update the `print_usage()` function (or create it if it doesn't exist) to document the new command.

### 6. Test the implementation
**Manual verification steps:**

1. Install uiautomation library:
   ```bash
   pip install uiautomation
   ```

2. Test with a focused element:
   ```bash
   python module/pc_automation.py get_focused
   ```
   
   Expected output (example with a textbox focused):
   ```json
   {"Name": "Search", "LocalizedControlType": "edit", "BoundingRect": [100, 200, 300, 250], "Value": "test", "ToggleState": null, "ExpandCollapseState": null}
   ```

3. Test with no focused element:
   - Minimize all windows
   - Run the command
   - Should output `null` and error message on stderr

4. Test with different control types:
   - Focus on a checkbox → should have ToggleState
   - Focus on a combobox → should have Value and ExpandCollapseState
   - Focus on a button → should have base properties only

### 7. Update documentation
**File:** [README_DEV.txt](../../../TalkBackAutoTest/module/README_DEV.txt)

Add documentation for the new functionality:
- Describe `get_focused` command
- List supported control types and their properties
- Explain JSON output format
- Document error handling (stdout vs stderr)
- Add example usage from C#

---

**Dependencies:**
- Tasks 1-4 must be completed in order
- Task 5 depends on tasks 1-4
- Task 6 can be done after task 5
- Task 7 can be done in parallel with task 6

**Estimated complexity:**
- Tasks 1-3: Simple additions (~20 lines each)
- Task 4: Medium complexity (~60 lines)
- Task 5: Simple modification (~10 lines)
- Task 6: Manual testing (15-30 minutes)
- Task 7: Documentation (~30 minutes)
