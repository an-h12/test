"""CLI wiring for pc_automation; preserves legacy output contract."""

import json
import sys
import time

import pc_automation_keys as keys
import pc_automation_clipboard as clipboard
import pc_automation_uia as uia

NARRATOR_CAPTURE_RETRY_DELAY = 0.2


def run_tab_sequence():
    """
    Executes the tab navigation sequence with automatic cycle detection.
    Uses RuntimeId to detect when Tab loop returns to a previously-visited element.
    """
    print("Press ESC to stop", file=sys.stderr)
    time.sleep(1)

    count = 0
    seen_runtime_ids = set()

    auto_enabled = clipboard.prepare_narrator_capture_session()
    try:
        narrator_ready = auto_enabled is not None

        def _narrator_text_matches(element_info, narrator_text):
            if not element_info or not narrator_text:
                return True
            text = narrator_text.lower()
            name = (element_info.get("Name") or "").strip().lower()
            control_type = (
                (element_info.get("LocalizedControlType") or "").strip().lower()
            )
            missing_name = bool(name) and name not in text
            missing_type = bool(control_type) and control_type not in text
            return not (missing_name or missing_type)

        def _mismatch_labels(element_info, narrator_text):
            if not element_info or not narrator_text:
                return []
            text = narrator_text.lower()
            name = (element_info.get("Name") or "").strip().lower()
            control_type = (
                (element_info.get("LocalizedControlType") or "").strip().lower()
            )
            labels = []
            if name and name not in text:
                labels.append("Name")
            if control_type and control_type not in text:
                labels.append("ControlType")
            return labels

        def _maybe_capture_for_element(element_info):
            if not narrator_ready:
                return None, []
            time.sleep(NARRATOR_CAPTURE_RETRY_DELAY)
            first_text = clipboard.try_capture_narrator_last_spoken(
                allow_no_text_format=True,
                log_failure=False,
            )
            if first_text and _narrator_text_matches(element_info, first_text):
                final_text = first_text
            else:
                time.sleep(NARRATOR_CAPTURE_RETRY_DELAY)
                second_text = clipboard.try_capture_narrator_last_spoken(
                    allow_no_text_format=True,
                    log_failure=False,
                )
                final_text = second_text or first_text

            mismatch_labels = _mismatch_labels(element_info, final_text)
            if final_text:
                clipboard.clear_clipboard_history_best_effort()
            else:
                print("ERROR: Narrator speech capture failed", file=sys.stderr)
            return final_text, mismatch_labels

        element = uia.get_focused_control()
        runtime_id = uia.get_runtime_id(element) if element is not None else None
        if runtime_id is not None:
            seen_runtime_ids.add(runtime_id)

        while True:
            if keys.is_escape_pressed():
                print("Stopped by user", file=sys.stderr)
                break

            if not keys.press_tab():
                print(f"Failed at iteration {count + 1}", file=sys.stderr)
                return False
            count += 1

            element = uia.get_focused_control()
            runtime_id = uia.get_runtime_id(element) if element is not None else None

            element_info = uia.get_focused_element_info(narrator_text=None)
            narrator_text, mismatch_labels = _maybe_capture_for_element(element_info)
            if narrator_text and mismatch_labels:
                narrator_text = (
                    f"{narrator_text}[Mismatch: {', '.join(mismatch_labels)}]"
                )
            output_payload = {
                "NarratorText": narrator_text,
                "Element": element_info,
            }

            if runtime_id is not None and runtime_id in seen_runtime_ids:
                print(json.dumps(output_payload, ensure_ascii=False))
                print(
                    f"Total unique elements: {len(seen_runtime_ids)}",
                    file=sys.stderr,
                )
                break

            if runtime_id is not None:
                seen_runtime_ids.add(runtime_id)

            print(json.dumps(output_payload, ensure_ascii=False))
    finally:
        clipboard.restore_narrator_capture_session(auto_enabled)

    sys.stdout.flush()

    print(f"Tab pressed {count} time(s)", file=sys.stderr)
    print(f"Unique elements visited: {len(seen_runtime_ids)}", file=sys.stderr)
    return True


def main():
    # Check if action argument provided
    if len(sys.argv) < 2:
        print("ERROR: Missing action argument", file=sys.stderr)
        sys.exit(1)

    action = sys.argv[1].lower()

    # Execute action
    if action == "get_focused":
        result = uia.get_focused_element_info()
        print(json.dumps(result, ensure_ascii=False))
        sys.exit(0 if result is not None else 1)

    elif action == "narrator":
        success = keys.toggle_narrator()
        sys.exit(0 if success else 1)

    elif action == "tab":
        success = run_tab_sequence()
        sys.exit(0 if success else 1)


if __name__ == "__main__":
    main()
    input()
