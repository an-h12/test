"""CLI wiring for PC automation; preserves legacy output contract."""

import json
import sys
import time

import pc_keys as keys
import pc_clipboard as clipboard
import pc_uia as uia

NARRATOR_CAPTURE_RETRY_DELAY = 0.2


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
    
    return len(mismatches) == 0, mismatches


def _capture_with_retry(element_info):
    time.sleep(NARRATOR_CAPTURE_RETRY_DELAY)
    first = clipboard.try_capture_narrator_last_spoken(
        allow_no_text_format=True, log_failure=False
    )
    
    is_match, _ = _check_narrator_match(element_info, first)
    if first and is_match:
        return first
    
    time.sleep(NARRATOR_CAPTURE_RETRY_DELAY)
    second = clipboard.try_capture_narrator_last_spoken(
        allow_no_text_format=True, log_failure=False
    )
    return second or first


def _maybe_capture_for_element(element_info, narrator_ready):
    if not narrator_ready:
        return None, []
    
    text = _capture_with_retry(element_info)
    _, mismatches = _check_narrator_match(element_info, text)
    
    if text:
        if not clipboard.clear_clipboard_history():
            print("WARNING: Clipboard history clear failed", file=sys.stderr)
    
    return text, mismatches


def _build_output_with_narrator(element_info, narrator_text, mismatches):
    if narrator_text and mismatches:
        narrator_text = f"{narrator_text}[Mismatch: {', '.join(mismatches)}]"
    return {"NarratorText": narrator_text, "Element": element_info}


def _has_cycle_detected(runtime_id, seen_ids):
    return runtime_id is not None and runtime_id in seen_ids


def _track_initial_element(seen_ids):
    element = uia.get_focused_control()
    runtime_id = uia.get_runtime_id(element)
    if runtime_id is not None:
        seen_ids.add(runtime_id)


def _process_tab_iteration(narrator_ready):
    """Process one tab iteration, return (output_payload, runtime_id)."""
    element = uia.get_focused_control()
    runtime_id = uia.get_runtime_id(element)
    element_info = uia.get_focused_element_info(narrator_text=None)
    narrator_text, mismatches = _maybe_capture_for_element(element_info, narrator_ready)
    return _build_output_with_narrator(element_info, narrator_text, mismatches), runtime_id


def run_tab_sequence():
    """Execute tab navigation with automatic cycle detection."""
    if not uia.ensure_available():
        return False
    
    print("Press ESC to stop", file=sys.stderr)
    time.sleep(1)

    count = 0
    seen_ids = set()
    auto_enabled = clipboard.prepare_narrator_capture_session()
    
    try:
        narrator_ready = auto_enabled is not None
        _track_initial_element(seen_ids)

        while True:
            if keys.is_escape_pressed():
                print("Stopped by user", file=sys.stderr)
                break

            if not keys.press_tab():
                print(f"Failed at iteration {count + 1}", file=sys.stderr)
                return False
            count += 1

            output, runtime_id = _process_tab_iteration(narrator_ready)

            if _has_cycle_detected(runtime_id, seen_ids):
                print(json.dumps(output, ensure_ascii=False))
                print(f"Total unique elements: {len(seen_ids)}", file=sys.stderr)
                break

            if runtime_id is not None:
                seen_ids.add(runtime_id)

            print(json.dumps(output, ensure_ascii=False))
    finally:
        clipboard.restore_narrator_capture_session(auto_enabled)

    sys.stdout.flush()
    print(f"Tab pressed {count} time(s)", file=sys.stderr)
    print(f"Unique elements visited: {len(seen_ids)}", file=sys.stderr)
    return True


def main():
    if len(sys.argv) < 2:
        print("ERROR: Missing action argument", file=sys.stderr)
        sys.exit(1)

    action = sys.argv[1].lower()

    if action == "get_focused":
        result = uia.get_focused_element_info()
        print(json.dumps(result, ensure_ascii=False))
        sys.exit(0 if result is not None else 1)

    if action == "narrator":
        success = keys.toggle_narrator()
        sys.exit(0 if success else 1)

    if action == "tab":
        success = run_tab_sequence()
        sys.exit(0 if success else 1)

    print(f"ERROR: Unknown action '{action}'", file=sys.stderr)
    sys.exit(1)


if __name__ == "__main__":
    main()
