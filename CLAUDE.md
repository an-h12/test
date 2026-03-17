# CLAUDE.md - Project Context

## Project Overview
- **Name**: TalkBackAutoTest
- **Type**: Windows Forms Application (C# .NET Framework 4.8)
- **Location**: `C:\Users\Haha\Desktop\Talk\owner\Talk-new`

## Recent Git Changes (last 5 commits)
- `e71319c` - changes
- `a9aad70` - up
- `a7855a6` - changes
- `c065667` - delay
- `624de5c` - change

## Key Files

### C# Files (Main)
| File | Lines | Purpose |
|------|-------|---------|
| `TalkBackAutoTest/MainForm.cs` | 8951 | Main UI form - rất lớn |
| `TalkBackAutoTest/PCTB.cs` | 1051 | Core functionality - gọi API điều khiển Windows UI |
| `TalkBackAutoTest/Object.cs` | - | Data model cho test objects |
| `TalkBackAutoTest/DeviceInfo.cs` | - | Device information model |
| `TalkBackAutoTest/UIAppInfo.cs` | - | UI Application info model |
| `TalkBackAutoTest/AObject.cs` | - | Abstract object model |
| `TalkBackAutoTest/AppName.cs` | - | Application name model |

### Python Modules (`TalkBackAutoTest/module/`)
| File | Purpose |
|------|---------|
| `pc_cli.py` | CLI interface |
| `pc_keys.py` | Keyboard input handling |
| `pc_output_narrator.py` | Narrator output capture |
| `pc_uia.py` | UI Automation |
| `pc_element_info.py` | Element information |

## Architecture
```
MainForm.cs (8951 lines)
    ↓ creates instance
PCTB PCManager = new PCTB();
    ↓ calls methods → REST API calls
BaseUrl + "/api/..."
```

## Environment Variables Used
| Variable | Purpose |
|----------|---------|
| `BASE_URL` | API base URL |
| `SECRET_KEY` | Encryption key (32 chars) |
| `ENCRYPTION_IV` | Encryption IV |
| `API_KEY` | API authentication |
| `API_BASE_URL` | API endpoint |
| `GET_DEV_TOKEN_URL` | Dev token URL |

## Configuration Files
- `.gitignore` - Git ignore patterns
- `.claudeignore` - Claude Code ignore patterns (includes `.env` for security)

## Important Notes
- `.env` file is intentionally ignored in `.claudeignore` to protect secrets
- Project uses `Environment.GetEnvironmentVariable()` (C#) and `os.environ.get()` (Python)
- Solution file: `TalkBackAutoTest.v11.suo`
