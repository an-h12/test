## Proposal: UI Element Inspection

### Problem
The current `pc_automation.py` module can control keyboard input (toggle Narrator, press Tab) but cannot inspect the properties of focused UI elements. For automated testing, we need to capture element information to validate that:
- Navigation landed on the correct element
- Element properties match expected values
- Narrator output corresponds to actual UI state

### Solution
Extend `pc_automation.py` with UI element inspection functionality using the `uiautomation` (Yinkaisheng) library - a Python wrapper for Microsoft's IUIAutomation COM interface.

**Key capabilities:**
- Get currently focused element
- Extract base properties: Name, LocalizedControlType, BoundingRect
- Extract pattern-specific properties based on control type:
  - **Value** (for Edit, Document, ComboBox)
  - **ToggleState** (for CheckBox, RadioButton)  
  - **ExpandCollapseState** (for ComboBox)

**Integration:**
- Single file deployment (merge into existing `pc_automation.py`)
- CLI interface: `python pc_automation.py get_focused` → JSON output
- Clean separation: stdout for data, stderr for errors
- Returns string enum values ('On'/'Off', 'Collapsed'/'Expanded') for easy C# parsing

### Scope
**In scope:**
- Element inspection for Edit, CheckBox, RadioButton, ComboBox, Document controls
- Base properties: Name, LocalizedControlType, BoundingRect
- Pattern properties: Value, ToggleState, ExpandCollapseState
- Error handling with stderr logging
- Extensible architecture for future patterns

**Out of scope:**
- AutomationId (Narrator doesn't read it)
- SelectionPattern, RangeValuePattern (not needed yet)
- Event listeners (only on-demand inspection)
- TreeView, ListView specific patterns (future)

### Success Criteria
- C# can call Python module and get JSON with element properties
- Unknown control types gracefully return base properties only
- Pattern extraction failures don't crash, return None with stderr warning
- Easy to add new control types and patterns in the future
