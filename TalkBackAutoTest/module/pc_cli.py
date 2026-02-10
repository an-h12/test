"""CLI wiring for PC automation; preserves legacy output contract."""

import json
import sys
import time

import pc_keys as keys
import pc_output_narrator as narrator
import pc_uia as uia


# region Constants

NARRATOR_CAPTURE_RETRY_DELAY = 0.2

# endregion


# region Narrator Text Validation

def _normalize_text(value):
    return (value or "").strip().lower()


def _check_narrator_match(element_info, narrator_text):
    """Returns (is_match, mismatch_labels)."""
    if not element_info or not narrator_text:
        return True, []
    
    text = narrator_text.lower()
    name = _normalize_text(element_info.get("Name"))
    control_type = _normalize_text(element_info.get("LocalizedControlType"))
    
    mismatches = []
    if name and name not in text:
        mismatches.append("Name")
    if control_type and control_type not in text:
        mismatches.append("ControlType")
    
    return not mismatches, mismatches


def _capture_with_retry(element_info):
    time.sleep(NARRATOR_CAPTURE_RETRY_DELAY)
    first = narrator.try_capture_narrator_last_spoken(
        allow_no_text_format=True, log_failure=False
    )
    
    is_match, _ = _check_narrator_match(element_info, first)
    if first and is_match:
        return first
    
    time.sleep(NARRATOR_CAPTURE_RETRY_DELAY)
    second = narrator.try_capture_narrator_last_spoken(
        allow_no_text_format=True, log_failure=False
    )
    return second or first


def _maybe_capture_for_element(element_info, narrator_ready):
    if not narrator_ready:
        return None, []
    
    text = _capture_with_retry(element_info)
    _, mismatches = _check_narrator_match(element_info, text)
    
    if text:
        if not narrator.clear_clipboard_history():
            print("Clipboard history clear failed", file=sys.stderr)
    
    return text, mismatches


def _build_output_with_narrator(element_info, narrator_text, mismatches):
    if narrator_text and mismatches:
        narrator_text = f"{narrator_text}[Mismatch: {', '.join(mismatches)}]"
    return {"NarratorText": narrator_text, "Element": element_info}

# endregion


# region Element Capture & Processing

def _has_cycle_detected(runtime_id, seen_ids):
    return runtime_id is not None and runtime_id in seen_ids


def _capture_focused_element(narrator_ready):
    element_info, runtime_id = uia.get_focused_element_with_info()
    narrator_text, mismatches = _maybe_capture_for_element(element_info, narrator_ready)
    return _build_output_with_narrator(element_info, narrator_text, mismatches), runtime_id


def _process_initial_element(narrator_ready, seen_ids):
    if narrator_ready:
        keys.force_narrator_read()
    output, runtime_id = _capture_focused_element(narrator_ready)
    if runtime_id is not None:
        seen_ids.add(runtime_id)
    return output, runtime_id

# endregion


# region Tab Navigation

def dumpScreen():
    """Execute tab navigation with automatic cycle detection."""
    if not uia.ensure_available():
        return False
    
    print("Press ESC to stop", file=sys.stderr)
    time.sleep(1)

    count = 0
    seen_ids = set()
    auto_enabled = narrator.prepare_narrator_capture_session()
    
    try:
        narrator_ready = auto_enabled is not None

        initial_output, _ = _process_initial_element(narrator_ready, seen_ids)
        print(json.dumps(initial_output, ensure_ascii=False))

        while True:
            if keys.is_escape_pressed():
                print("Stopped by user", file=sys.stderr)
                break

            if not keys.changeFocus():
                print(f"Failed at iteration {count + 1}", file=sys.stderr)
                return False
            count += 1

            output, runtime_id = _capture_focused_element(narrator_ready)

            if _has_cycle_detected(runtime_id, seen_ids):
                print(json.dumps(output, ensure_ascii=False))
                print(f"Total unique elements: {len(seen_ids)}", file=sys.stderr)
                break

            if runtime_id is not None:
                seen_ids.add(runtime_id)

            print(json.dumps(output, ensure_ascii=False))
    finally:
        narrator.restore_narrator_capture_session(auto_enabled)

    sys.stdout.flush()
    print(f"Tab pressed {count} time(s)", file=sys.stderr)
    print(f"Unique elements visited: {len(seen_ids)}", file=sys.stderr)
    return True


run_tab_sequence = dumpScreen

# endregion


# region CLI Entry Point

def main():
    if len(sys.argv) < 2:
        print("Missing action argument", file=sys.stderr)
        sys.exit(1)

    action = sys.argv[1].lower()

    if action == "get_focused":
        result = uia.getFocusedElementInfo()
        print(json.dumps(result, ensure_ascii=False))
        sys.exit(0 if result is not None else 1)

    if action == "narrator":
        success = keys.toggle_narrator()
        sys.exit(0 if success else 1)

    if action == "tab":
        time.sleep(1)
        success = dumpScreen()
        sys.exit(0 if success else 1)

    print(f"Unknown action '{action}'", file=sys.stderr)
    sys.exit(1)


if __name__ == "__main__":
    main()

# endregion
