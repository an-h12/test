## Proposal: Fix UI Automation Pattern Mappings

### Problem
The current `pc_automation.py` implementation has critical mismatches between control types and their UI Automation patterns based on Accessibility Insights data:

1. **RadioButton incorrectly mapped to TogglePattern** - Should use SelectionItemPattern
2. **Missing support for Button (toggle variant)** - Toggle buttons exist but aren't handled
3. **Missing support for ListItem** - Very common control type with flexible pattern support
4. **Missing support for TreeItem** - Used in tree views and hierarchical lists

These mapping errors cause:
- `get_focused_element_info()` returns `None` for RadioButton state (critical bug)
- Cannot test toggle buttons, list items, or tree items
- False negatives in automated accessibility tests

### Solution
Fix the control type to pattern mappings to match actual UI Automation API behavior:

**1. Add SelectionItemPattern Handler**
- Create `_extract_selection_item_pattern()` function
- Extract `IsSelected` property (boolean)
- Return string values: "Selected" / "NotSelected" for consistency

**2. Correct RadioButton Mapping**
- Change from: `50003: ['ToggleState']` (WRONG)
- Change to: `50003: ['IsSelected']` (CORRECT)

**3. Add Missing Control Types**
- Button (50000): `['ToggleState']` - for toggle button variant
- ListItem (50007): `['IsSelected', 'ToggleState', 'ExpandCollapseState']` - try all patterns
- TreeItem (50025): `['ExpandCollapseState']` - for expandable tree nodes

**4. Update Result Initialization**
- Add `result['IsSelected'] = None` to initialization
- Update docstring to reflect new property and supported types

### Key Design Decisions

**IsSelected Return Format:** String ("Selected" / "NotSelected")
- Matches existing pattern: ToggleState, ExpandCollapseState all return strings
- More readable in test reports than boolean
- Easier for C# to parse consistently

**ListItem Multi-Pattern Support:** Try all three patterns
- ListItem is flexible: can be selectable, checkable, expandable, or combinations
- Existing try-catch architecture safely handles unsupported patterns
- Returns `None` for patterns not supported by specific control instance

**Property Name:** `IsSelected` (not `SelectionState` or `Selected`)
- Matches SelectionItemPattern API directly
- Standard UI Automation terminology
- Clear semantic meaning

### Scope

**In scope:**
- Add SelectionItemPattern handler
- Fix RadioButton mapping (critical bug fix)
- Add Button, ListItem, TreeItem mappings
- Update initialization and docstring
- Test with RadioButton, ListItem examples

**Out of scope:**
- Other SelectionPattern features (GetSelection, multiple selection containers)
- Advanced list patterns (GridItemPattern, etc.)
- Performance optimization
- C# integration changes (JSON format unchanged)

### Success Criteria
- RadioButton returns `IsSelected: "Selected"` or `IsSelected: "NotSelected"`
- ListItem returns appropriate pattern values based on control capabilities
- No breaking changes to existing EditControl, CheckBox, ComboBox behavior
- All pattern extraction failures safely return `None`
