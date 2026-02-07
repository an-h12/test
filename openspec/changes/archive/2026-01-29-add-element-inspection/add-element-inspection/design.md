## Design: UI Element Inspection

### Architecture: Hybrid Handler-Based Pattern Extraction

We'll use a **dictionary-based mapping** with **dedicated handler functions** for each UI Automation pattern. This provides optimal performance (O(1) lookups), easy extensibility, and clean separation of concerns.

```
pc_automation.py Structure:
├─ [SECTION 1] Keyboard Control (existing)
│   └─ ctypes-based functions
├─ [SECTION 2] UI Automation Constants
│   ├─ TOGGLE_STATE_NAMES = {0: 'Off', 1: 'On', 2: 'Indeterminate'}
│   └─ EXPAND_COLLAPSE_STATE_NAMES = {0: 'Collapsed', 1: 'Expanded', ...}
├─ [SECTION 3] Pattern Handlers (private functions)
│   ├─ _extract_value_pattern(element) → str | None
│   ├─ _extract_toggle_pattern(element) → str | None
│   └─ _extract_expand_collapse_pattern(element) → str | None
├─ [SECTION 4] Control Type Mapping
│   └─ CONTROL_PATTERNS = {50004: ['Value'], 50002: ['ToggleState'], ...}
├─ [SECTION 5] Main Function
│   └─ get_focused_element_info() → dict | None
└─ [SECTION 6] CLI Interface
    └─ if __name__ == '__main__': handle commands
```

### Key Design Decisions

#### 1. Control-Type Aware Pattern Extraction
Use **integer ControlType IDs** for fast dictionary lookup:
```python
CONTROL_PATTERNS = {
    50004: ['Value'],                          # EditControl
    50030: ['Value'],                          # DocumentControl  
    50002: ['ToggleState'],                    # CheckBoxControl
    50003: ['ToggleState'],                    # RadioButtonControl
    50006: ['Value', 'ExpandCollapseState'],   # ComboBoxControl
}
```

For unknown control types, return only base properties (Name, LocalizedControlType, BoundingRect).

#### 2. Return Format Specifications

**Base Properties (always returned):**
- `Name`: element.Name or '' (empty string if None)
- `LocalizedControlType`: element.LocalizedControlType or ''
- `BoundingRect`: [left, top, right, bottom] as **List** (compact, fast to parse)

**Pattern Properties (conditional):**
- `Value`: String content from ValuePattern, or None
- `ToggleState`: 'On' | 'Off' | 'Indeterminate' (string), or None
- `ExpandCollapseState`: 'Collapsed' | 'Expanded' | 'PartiallyExpanded' | 'LeafNode' (string), or None

**Enum Conversion:** All enum values are converted to strings for readability and easy C# parsing.

**Missing Patterns:** Return `None` (serializes to JSON `null`) when pattern not supported.

#### 3. Error Handling Strategy

**Stdout:** JSON data only (success cases)
**Stderr:** All errors, warnings, and info messages

Error levels:
- `ERROR`: Critical failures (no focused element)
- `WARNING`: Pattern extraction failed but continuing
- `INFO`: Unknown control type, returning base props only

**No Element Found:** Return `None` (JSON `null`) + stderr message.

#### 4. Pattern Handler Structure

Each handler follows this template:
```python
def _extract_<pattern>_pattern(element):
    """Extract <property> from <Pattern>. Returns <type> or None."""
    try:
        # Check if pattern is supported
        if element.GetPattern(PatternId.<Pattern>):
            pattern = element.Get<Pattern>()
            value = pattern.<Property>
            # Convert enum to string if needed
            return ENUM_MAPPING.get(value, str(value))
    except Exception as e:
        print(f"WARNING: <Pattern> extraction failed: {e}", file=sys.stderr)
        return None
    return None
```

#### 5. Extensibility Points

**To add a new pattern type:**
1. Add enum mapping constant (if needed): `PATTERN_STATE_NAMES = {...}`
2. Create handler function: `def _extract_pattern_pattern(element):`
3. Register in control mapping: `CONTROL_PATTERNS[ControlTypeId] = ['PatternName']`

**To add a new control type:**
- Add entry to `CONTROL_PATTERNS` with list of applicable patterns

No changes to core logic required.

### Data Flow

```
C# calls: python pc_automation.py get_focused
    ↓
Python: get_focused_element_info()
    ↓
1. auto.GetFocusedControl() → element or None
    ↓
2. If None → print ERROR to stderr, return None
    ↓
3. Extract base properties (Name, LocalizedControlType, BoundingRect)
    ↓
4. Lookup control type ID in CONTROL_PATTERNS
    ↓
5. If found → execute handlers for each pattern
   If not found → log INFO to stderr, skip patterns
    ↓
6. Merge pattern results into base dict
    ↓
7. Return dict
    ↓
Python CLI: json.dumps(result, ensure_ascii=False)
    ↓
Stdout: {"Name": "...", "Value": "...", ...} or null
Stderr: Any error/warning/info messages
    ↓
C# reads both streams separately
```

### Performance Optimizations

- **O(1) control type lookup** using integer dict keys
- **Pattern check before access** to avoid exception overhead
- **No iteration** - direct mapping from control type to handlers
- **List for BoundingRect** - minimal JSON size
- **String mapping via dict** - fast enum conversion

### Dependencies

**New dependency:** `uiautomation` (Yinkaisheng)
```bash
pip install uiautomation
```

**Import strategy:**
```python
try:
    import uiautomation as auto
    from uiautomation import ControlType, PatternId
except ImportError:
    # Graceful degradation if not installed
    pass
```

### CLI Interface Design

Commands:
- `python pc_automation.py get_focused` - Get focused element info (new)
- `python pc_automation.py toggle_narrator` - Toggle Narrator (existing)
- `python pc_automation.py press_tab` - Press Tab key (existing)

Output format:
- Success: JSON object on stdout
- No element: `null` on stdout + error on stderr  
- Failure: Non-zero exit code + error on stderr
