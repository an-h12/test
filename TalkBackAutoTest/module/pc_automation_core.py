"""Core JSON assembly helpers (pure functions)."""


def build_element_info(
    name,
    localized_control_type,
    position,
    pattern_order,
    pattern_values,
    narrator_text,
):
    result = {
        "Name": name or "",
        "LocalizedControlType": localized_control_type or "",
    }

    if position:
        result["Position"] = position

    for pattern_name in pattern_order:
        value = pattern_values.get(pattern_name)
        if value is not None:
            result[pattern_name] = value

    if narrator_text:
        result["NarratorText"] = narrator_text

    return result
