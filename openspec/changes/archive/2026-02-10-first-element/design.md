## Context

The `dumpScreen()` function in `pc_automation.py` orchestrates tab-based UI element discovery with Windows Narrator speech capture. Currently, it has architectural issues:

1. **Initial element skipped**: When Narrator is auto-toggled on at start, the initially focused element is tracked for cycle detection but never captured or logged
2. **Duplicate capture**: Narrator text is captured twice per element—once inside `dumpFocusedObject()` and again in `_process_tab_iteration()`
3. **Inconsistent data model**: NarratorText appears both inside Element dict (from UIA layer) and in wrapper (from automation layer), causing confusion about which is authoritative

The current flow is:
- Toggle Narrator on → Track initial element ID → Start loop → Tab → Capture element #2 → ...

This means element #1 is never logged, and each subsequent element triggers two narrator capture attempts (one often fails due to timing).

## Goals / Non-Goals

**Goals:**
- Capture and log the initially focused element before tab loop begins
- Eliminate duplicate narrator capture by enforcing single-responsibility: UIA layer does element info, automation layer does narrator capture
- Clean data model: NarratorText only in automation wrapper, not in Element dict
- Force Narrator to read initial element on demand (Caps Lock/Insert hotkey simulation)

**Non-Goals:**
- Changing the JSON output format for existing consumers (preserve `{"NarratorText": "...", "Element": {...}}` structure)
- Supporting NVDA or other screen readers (focus remains on Windows Narrator)
- Optimizing narrator capture timing or retry logic (keep existing retry mechanism)

## Decisions

### Decision 1: Force read via Caps Lock/Insert + Tab simulation

**Rationale**: Windows Narrator has a built-in hotkey (Narrator key + Tab) that forces it to read the current element and copy the speech text to clipboard. The Narrator key can be either Caps Lock or Insert depending on user settings. This is different from the standard Tab navigation—when Narrator key is held, Tab triggers a read without changing focus.

**Implementation**:
- Add `VK_CAPITAL` (0x14) and `VK_INSERT` (0x2D) constants to `pc_keys.py`
- Create `force_narrator_read()` function that sends Caps Lock/Insert + Tab using `send_key_chord()`
- Default to Caps Lock (more common setting), with Insert as fallback if needed later
- Use short hold time to ensure Narrator processes the command before releasing

**Alternative considered**: Use UI Automation's `AutomationElement.SetFocus()` to re-focus the element and trigger Narrator naturally. Rejected because SetFocus can have side effects (focus events, state changes) and may not reliably trigger Narrator speech.

### Decision 2: Rename dumpFocusedObject → getFocusedElementInfo

**Rationale**: The function name `dumpFocusedObject` implies it dumps/logs something when it actually just extracts element properties. The explorer analysis revealed it was calling `getNarratorOutput()` internally, mixing concerns.

**Implementation**:
- Rename function to `getFocusedElementInfo()` reflecting its true purpose: get element info
- Remove `getNarratorOutput()` call from inside the function
- Remove `narrator_text` parameter (no longer needed)
- Return only element properties: Name, LocalizedControlType, Position, patterns
- Keep backward compatibility alias for existing callers

**Alternative considered**: Keep two versions—one with narrator capture, one without. Rejected because it perpetuates the dual-responsibility problem and makes it unclear which to use when.

### Decision 3: Single capture in automation layer

**Rationale**: Narrator capture should happen once per element at the automation orchestration level (`pc_automation.py`), not scattered across layers. This makes timing and retry logic centralized and debuggable.

**Implementation**:
- `getFocusedElementInfo()` returns pure element info (no narrator capture)
- `_process_tab_iteration()` calls `getFocusedElementInfo()` then `_maybe_capture_for_element()` separately
- Keep existing retry logic in `_capture_with_retry()` (two attempts with 0.2s delay)
- Element dict never contains NarratorText field
- Narrator text only appears in wrapper: `{"NarratorText": "...", "Element": {...}}`

**Alternative considered**: Capture narrator text inside `_process_tab_iteration` first, then get element info and match against it. Rejected because we need element info to check narrator match, so getting element info first is the logical order.

### Decision 4: Process initial element before loop

**Rationale**: To capture the first element, we need to explicitly process it before entering the tab loop. This requires forcing Narrator to read it since Narrator may not have announced it yet.

**Implementation**:
- Add `_process_initial_element(narrator_ready)` function that:
  1. Calls `force_narrator_read()` to trigger Narrator
  2. Calls `_process_tab_iteration(narrator_ready)` to capture and format output (code reuse)
  3. Returns formatted output
- In `dumpScreen()`, after `prepare_narrator_capture_session()`:
  - Call `_process_initial_element()` and print its output
  - Call `_track_initial_element()` to add runtime ID to cycle detection
  - Then enter normal tab loop
- Initial element output format matches tab iteration format for consistency

**Alternative considered**: Add special handling to track runtime ID of initial element and when we encounter it again during tab loop, print it then. Rejected because it complicates the loop logic and delays output (user doesn't see first element until cycle completes).

## Risks / Trade-offs

### Risk: Force read timing
**Trade-off**: Forcing Narrator to read initial element adds ~0.5s delay before tab loop starts  
**Mitigation**: This is acceptable for test automation scenarios; if it becomes problematic, make force read configurable

### Risk: Caps Lock vs Insert setting
**Trade-off**: We default to Caps Lock but some users configure Narrator to use Insert key  
**Mitigation**: Current implementation uses Caps Lock; if failures occur, we can detect from registry or try both keys

### Risk: Breaking change for Element dict structure
**Trade-off**: Removing NarratorText from Element dict could break consumers expecting it there  
**Mitigation**: Our exploration revealed this field was inconsistently populated (often empty in wrapper context). The outer NarratorText is the authoritative one and is preserved. If needed, we can check git history for actual consumers.

### Risk: Initial element captured twice
**Trade-off**: If the element we start on is also encountered in the cycle, we'll log it at the start and again at the end  
**Mitigation**: This is actually correct behavior—we want to log all elements encountered, and the cycle detection still works (it just means we made a full loop back to start)

## Migration Plan

**Deployment**:
1. Update `pc_keys.py` with new key constants and force read function
2. Update `pc_uia.py` to rename function and remove narrator capture  
3. Update `pc_automation.py` to add initial element processing
4. Test with sample UWP app to verify initial element appears in output

**Rollback**: If critical issues arise, revert to previous behavior where initial element is skipped (existing behavior). No data model changes require migration since output format is preserved.

**Validation**: Compare before/after output—new output should have one additional JSON line at the start corresponding to initial element.
