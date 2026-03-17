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
| File | Purpose |
|------|---------|
| `TalkBackAutoTest/MainForm.cs` | Main UI form |
| `TalkBackAutoTest/PCTB.cs` | Core functionality (778 lines) |
| `TalkBackAutoTest/module/*.py` | Python automation modules |

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
