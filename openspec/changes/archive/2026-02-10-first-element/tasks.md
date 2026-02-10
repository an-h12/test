## 1. Add Force Narrator Read Support

- [x] 1.1 Add VK_CAPITAL (0x14) and VK_INSERT (0x2D) constants to pc_keys.py
- [x] 1.2 Create force_narrator_read() function in pc_keys.py that sends Caps Lock + Tab key sequence
- [x] 1.3 Add configurable hold time for force read keys (similar to NARRATOR_TOGGLE_HOLD)
- [ ] 1.4 Test force_narrator_read() manually to verify Narrator reads current element without changing focus

## 2. Refactor Element Info Extraction

- [x] 2.1 Rename dumpFocusedObject() to getFocusedElementInfo() in pc_uia.py
- [x] 2.2 Remove narrator_text parameter from getFocusedElementInfo()
- [x] 2.3 Remove getNarratorOutput() call from inside getFocusedElementInfo()
- [x] 2.4 Update build_element_info() call to not pass narrator_text parameter
- [x] 2.5 Add backward compatibility alias: dumpFocusedObject = getFocusedElementInfo
- [x] 2.6 Add backward compatibility alias: getFocusedObjectFake = getFocusedElementInfo
- [x] 2.7 Update pc_element_info.py build_element_info() to handle narrator_text=None gracefully (skip adding to dict)

## 3. Fix Duplicate Narrator Capture

- [x] 3.1 Update _process_tab_iteration() to call getFocusedElementInfo() instead of dumpFocusedObject()
- [x] 3.2 Verify getFocusedElementInfo() returns element info WITHOUT NarratorText field
- [x] 3.3 Ensure _maybe_capture_for_element() is the ONLY narrator capture point in tab iteration
- [ ] 3.4 Test tab sequence output to confirm Element dict does not contain NarratorText field

## 4. Process Initial Element

- [x] 4.1 Create _process_initial_element(narrator_ready, seen_ids) helper function in pc_automation.py
- [x] 4.2 Inside _process_initial_element(): call force_narrator_read() if narrator_ready
- [x] 4.3 Inside _process_initial_element(): get element info and runtime_id
- [x] 4.4 Inside _process_initial_element(): add runtime_id to seen_ids for cycle detection
- [x] 4.5 Inside _process_initial_element(): capture narrator text using _maybe_capture_for_element()
- [x] 4.6 Inside _process_initial_element(): build and return output using _build_output_with_narrator()
- [x] 4.7 Update dumpScreen() to call _process_initial_element() after prepare_narrator_capture_session()
- [x] 4.8 Update dumpScreen() to print initial element output before loop
- [x] 4.9 Remove separate _track_initial_element() call since it's now inside _process_initial_element()

## 5. Update CLI Entry Points

- [x] 5.1 Update main() action "get_focused" to call getFocusedElementInfo() instead of dumpFocusedObject()
- [ ] 5.2 Verify get_focused action returns element info without NarratorText
- [ ] 5.3 Verify tab action now outputs initial element as first JSON line

## 6. Testing and Validation

- [ ] 6.1 Test dumpScreen() with sample UWP app - verify initial element appears in output
- [ ] 6.2 Test that initial element has NarratorText field in wrapper (outer level)
- [ ] 6.3 Test that Element dict does NOT contain NarratorText field for any elements
- [ ] 6.4 Test cycle detection still works (detects when returning to initial element)
- [ ] 6.5 Test with Narrator already running vs auto-toggle scenario
- [ ] 6.6 Compare output line count before/after (should have +1 line for initial element)
- [ ] 6.7 Verify backward compatibility: existing code calling dumpFocusedObject still works
