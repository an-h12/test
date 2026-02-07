## Why

Current element inspection in `pc_automation.py` lacks critical structural and state information that Narrator provides, such as "Item X of Y" (Position/Size), exact values for sliders/progress bars, and enabled/disabled status. To match Narrator's behavior for automated accessibility testing, we need to extract deeper properties and support more control patterns.

## What Changes

-   **Upgrade `get_focused_element_info`**:
    -   Add logic to extract `PositionInSet` and `SizeOfSet`.
    -   ~~Add `IsEnabled` and `ItemStatus` to base properties.~~ **(DEFERRED - code commented out)**
-   **JSON Schema Update**:
    -   Backwards compatible extension: adding key `Position`.
    -   ~~`IsEnabled`, `ItemStatus`~~ **(DEFERRED)**

## Capabilities

### New Capabilities
-   `enhance-element-inspection`: Richer element data extraction including structural position, range values, and legacy accessibility properties.

### Modified Capabilities
-   `element-inspection`: Enhanced with more fields.

## Impact

**Affected code:**
-   `TalkBackAutoTest/module/pc_automation.py`: `get_focused_element_info` and `_extract_position_in_set` helper function.

**Behavior changes:**
-   `tab` command output will now include `Position` (Index/Total) when available.
-   ~~`IsEnabled`, `ItemStatus`~~ **(DEFERRED - code commented out, needs future review)**
-   No breaking changes to existing JSON fields.
