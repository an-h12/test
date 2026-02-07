import sys
import time

try:
    import uiautomation as auto
    from uiautomation import PatternId, PropertyId
except ImportError:
    auto = None
    PatternId = None
    PropertyId = None

from pc_automation_clipboard import copy_narrator_last_spoken
from pc_automation_core import build_element_info


NARRATOR_FOCUS_DELAY = 0.35
UIA_MISSING_MESSAGE = "ERROR: uiautomation library not available"


TOGGLE_STATE_NAMES = {
    0: "Off",
    1: "On",
}

EXPAND_COLLAPSE_STATE_NAMES = {
    0: "Collapsed",
    1: "Expanded",
}


def _extract_value_pattern(element):
    """Extract Value from ValuePattern. Returns string or None."""
    if element.GetPattern(PatternId.ValuePattern):
        pattern = element.GetValuePattern()
        return pattern.Value
    return None


def _extract_toggle_pattern(element):
    """Extract ToggleState from TogglePattern. Returns 'On'/'Off'/etc or None."""
    if element.GetPattern(PatternId.TogglePattern):
        pattern = element.GetTogglePattern()
        state = pattern.ToggleState
        return TOGGLE_STATE_NAMES.get(state, str(state))

    return None


def _extract_expand_collapse_pattern(element):
    """Extract ExpandCollapseState. Returns 'Collapsed'/'Expanded'/etc or None."""
    if element.GetPattern(PatternId.ExpandCollapsePattern):
        pattern = element.GetExpandCollapsePattern()
        state = pattern.ExpandCollapseState
        return EXPAND_COLLAPSE_STATE_NAMES.get(state, str(state))
    return None


def _extract_selection_item_pattern(element):
    """Extract IsSelected from SelectionItemPattern. Returns 'selected'/'non-selected' or None."""
    if element.GetPattern(PatternId.SelectionItemPattern):
        pattern = element.GetSelectionItemPattern()
        return "selected" if pattern.IsSelected else "non-selected"
    return None


def _extract_position_in_set(element):
    """Extract PositionInSet and SizeOfSet properties.

    Returns dict { 'Index': pos, 'Total': size } if both values > 0.
    Returns None if properties are not set (value = 0).
    """
    pos = element.GetPropertyValue(PropertyId.PositionInSetProperty)
    size = element.GetPropertyValue(PropertyId.SizeOfSetProperty)

    if pos > 0 and size > 0:
        return {"Index": pos, "Total": size}
    return None


CONTROL_PATTERNS = {
    50004: ["Value"],                          # EditControl
    50030: ["Value"],                          # DocumentControl
    50002: ["ToggleState"],                    # CheckBoxControl
    50013: ["IsSelected"],                     # RadioButtonControl
    50000: ["ToggleState"],                    # ButtonControl (toggle variant)
    50003: ["Value", "ExpandCollapseState"],   # ComboBoxControl
    50007: ["IsSelected", "ToggleState", "ExpandCollapseState"],  # ListItemControl
    50024: ["ExpandCollapseState"],            # TreeItemControl
}

PATTERN_HANDLERS = {
    "Value": _extract_value_pattern,
    "ToggleState": _extract_toggle_pattern,
    "ExpandCollapseState": _extract_expand_collapse_pattern,
    "IsSelected": _extract_selection_item_pattern,
}


def ensure_available():
    if auto is None:
        print(UIA_MISSING_MESSAGE, file=sys.stderr)
        return False
    return True


def get_focused_control():
    if auto is None:
        return None
    return auto.GetFocusedControl()


def get_runtime_id(element):
    if element is None:
        return None
    return element.GetRuntimeId()


_NARRATOR_TEXT_MISSING = object()


def get_focused_element_info(narrator_text=_NARRATOR_TEXT_MISSING):
    """
    Get information about currently focused UI element.

    dict: Element info with base properties and control-specific patterns.
            Only includes fields that have values (no None fields).
            - Always: Name, LocalizedControlType
            - If set: Position, Value, ToggleState, ExpandCollapseState, IsSelected
        narrator_text: Optional override; pass None to skip capture.
        None: If no element focused
    """
    if not ensure_available():
        return None

    time.sleep(NARRATOR_FOCUS_DELAY)
    element = auto.GetFocusedControl()

    if element is None:
        print("ERROR: No focused element found", file=sys.stderr)
        return None

    name = element.Name or ""
    localized_control_type = element.LocalizedControlType or ""

    position = _extract_position_in_set(element)

    control_type_id = element.ControlType
    pattern_names = CONTROL_PATTERNS.get(control_type_id, [])
    pattern_values = {}

    for pattern_name in pattern_names:
        handler = PATTERN_HANDLERS.get(pattern_name)
        if handler:
            value = handler(element)
            if value is not None:
                pattern_values[pattern_name] = value

    if narrator_text is _NARRATOR_TEXT_MISSING:
        narrator_text = copy_narrator_last_spoken()

    return build_element_info(
        name=name,
        localized_control_type=localized_control_type,
        position=position,
        pattern_order=pattern_names,
        pattern_values=pattern_values,
        narrator_text=narrator_text,
    )
