# AppTemplate

## Session Start (ALWAYS do this FIRST on every session)

On EVERY session, before doing any other work, follow this flow:

### Detect if New or Returning User
Check these signals:
- `git branch --show-current` -- are they on `main` or a feature branch?
- `git branch --list 'feature/*'` -- do any local feature branches exist?
- `gh auth status 2>&1` -- is GitHub auth configured?
- `dotnet --version 2>&1` -- is .NET installed?

### New User (on `main`, no feature branches, or missing tools/auth)
Greet them warmly and introduce yourself:

> Welcome to AppTemplate! I'm your AI coding assistant. I'll handle all the
> technical setup, git, and code management -- you just tell me what you
> want to build in plain English.
>
> Let me first make sure your environment is ready...

Then:
1. Run the full first-time setup checks from the git-workflow rules (install .NET SDK, gh CLI, GitHub auth via company email, git identity) -- narrate what you're doing in plain language so non-dev participants aren't confused
2. **Auto-detect Anthropic API key** (see "API Key Auto-Detection" section below)
3. After setup passes, give a brief orientation:
   - "This project has a .NET API backend (port 5050) and a Blazor web frontend (port 8080)"
   - "I'll write the code, run builds and tests, and sync your work with the team automatically"
   - "When you're done, just say 'I'm done' and I'll save and merge everything"
   - If API key was auto-detected: "I found your Anthropic API key from your Claude/OpenCode setup -- the AI chat is ready to use"
   - If not found: "To use the AI chat, you'll need an Anthropic API key. You can enter it in the chat bubble (bottom-right corner)"
4. **Read `PLAN.md`** and show a brief summary: what the team is building, tasks done/in progress/remaining
5. Ask: "What's your name and what feature would you like to work on?"
6. Create their feature branch (`feature/{short-name}`)
7. Pull latest main and they're ready to go

### Returning User (already on a feature branch)
Brief greeting:

> Welcome back! Let me pull the latest code and sync your branch...

Then run the normal session start workflow:
1. `git pull origin main`
2. `git merge main --no-edit` on their feature branch
3. Resolve any conflicts if they exist (for `PLAN.md` conflicts, combine both sides -- never discard tasks or decisions)
4. **Auto-detect Anthropic API key** if `ANTHROPIC_API_KEY` is not already set (see "API Key Auto-Detection" below)
5. **Read `PLAN.md`** and show a brief summary: tasks done, in progress, remaining. If the user has assigned tasks, remind them: "Your task: {name}"
6. Ask what they'd like to work on today

---

## Project Overview
<!-- FILL IN: Describe the project topic and goals here when decided -->
**Team**: <!-- FILL IN: Team name -->
**Goal**: <!-- FILL IN: Project goal -->
**Repo**: https://github.com/danielenerrodriguez/AppTemplate

## Quick Reference
- **Solution**: `AppTemplate.slnx`
- **Backend**: `src/AppTemplate.Api/` -- .NET Minimal API (port 5050)
- **Frontend**: `src/AppTemplate.Web/` -- Blazor Server (port 8080)
- **Tests**: `tests/`
- **SDK**: .NET 10 (pinned in `global.json`, `rollForward: latestMajor`, `allowPrerelease: true`)
- **Build config**: `Directory.Build.props` centralizes TargetFramework, Nullable, ImplicitUsings, TreatWarningsAsErrors
- **Docker**: `Dockerfile` uses `mcr.microsoft.com/dotnet/sdk:10.0` and `aspnet:10.0`
- **CI**: GitHub Actions uses `dotnet-version: 10.0.x`

## Commands
- Build all: `dotnet build`
- Run API: `dotnet run --project src/AppTemplate.Api`
- Run Web: `dotnet run --project src/AppTemplate.Web`
- Run both: `./launch-all.sh` (Linux/Mac) or `.\launch-all.ps1` (Windows)
- Run all tests: `dotnet test`
- Format code: `dotnet format`
- Watch API: `dotnet watch --project src/AppTemplate.Api`
- Watch Web: `dotnet watch --project src/AppTemplate.Web`
- Docker: `docker compose up --build`

## Slash Commands
- `/new-feature {name}` -- Scaffold a complete vertical slice (CRUD endpoints, service, Blazor page, tests)
- `/kickoff {topic}` -- Run the hackathon kickoff flow (update CLAUDE.md, propose features, scaffold everything)
- `/status` -- Show git, build, test, plan, and feature status report
- `/plan [action]` -- View or update the project plan (`PLAN.md`). Actions: `add`, `done`, `progress`, `assign`, `note`, `sync`, or no args to show summary

## Architecture
This project uses **feature-based vertical slices**. Each feature is self-contained:

- Backend features: `src/AppTemplate.Api/Features/{FeatureName}/`
  - `{Name}Endpoints.cs` -- Minimal API route definitions
  - `{Name}Service.cs` -- Business logic
  - `I{Name}Service.cs` -- Interface
  - `{Name}Dtos.cs` -- Data transfer objects (records)
- Frontend features: `src/AppTemplate.Web/Components/Features/{FeatureName}/`
  - `{Name}Page.razor` -- Markup
  - `{Name}Page.razor.cs` -- Code-behind logic
- Shared services: `src/AppTemplate.Api/Shared/`
- AI integration: `src/AppTemplate.Api/Shared/AI/ClaudeService.cs`
- Common models: `src/AppTemplate.Api/Shared/Models/` (`PagedResult<T>` -- opt-in pagination)
- Database entities: `src/AppTemplate.Api/Shared/Data/` (`ApiKeyEntity.cs`, `ChatMessageEntity.cs`)
- Database: SQLite via EF Core (`src/AppTemplate.Api/Shared/Auth/AppDbContext.cs`)
- Global chat bubble: `src/AppTemplate.Web/Components/Layout/ChatBubble.razor` (layout-level, visible on all pages)
- UI library: MudBlazor 9.0 (all components, no Bootstrap)

### Pre-built Features
- **Weather** -- Example CRUD feature (`/api/weather`)
- **AI Chat** -- Global floating chat bubble with Claude integration (`/api/chat`, `/api/apikeys`)
  - Chat bubble visible on all pages (bottom-right corner)
  - Per-user API key management with device-based identity (encrypted at rest with IDataProtector)
  - **Model selector** -- Users pick which Claude model to use via Settings (gear icon)
    - Models fetched dynamically from Anthropic API (`GET /api/chat/models`)
    - Selected model persisted in browser localStorage
    - Passed to `SendMessageAsync` for per-request model override
  - Persistent chat history in SQLite
  - Falls back to `ANTHROPIC_API_KEY` env var if no user key stored

### Adding a New Feature
Use the slash command: `/new-feature {Name}` -- it scaffolds everything automatically.

Or manually:
1. Create folder in both `Api/Features/{Name}/` and `Web/Components/Features/{Name}/`
2. Add service interface + implementation
3. Register in `Shared/Extensions/ServiceCollectionExtensions.cs`
4. Add endpoints in `{Name}Endpoints.cs`, map in `Program.cs`
5. Create Blazor page + code-behind
6. Write tests mirroring the source structure

## Rules
- Always read `PLAN.md` on session start for project context. Update it when tasks complete or decisions are made. Commit PLAN.md changes with feature work, not separately.
- Always update CLAUDE.md and README.md when adding or modifying features
- Always write unit tests for business logic (services)
- Use `AppDbContext` for data persistence, raw `TypedResults` for API responses, MudBlazor for UI
- Use dependency injection -- register services in `ServiceCollectionExtensions.cs`
- After meaningful code changes (build + tests pass), relaunch running servers so the user can see changes immediately
- Use async/await for all I/O operations
- API endpoints return proper HTTP status codes (200, 201, 400, 404, 500)
- Blazor components: use code-behind (.razor.cs) for logic, keep .razor clean
- Never commit API keys or secrets -- use environment variables or user-secrets
- Keep methods under 30 lines; extract to services when they grow
- Use records for DTOs, classes for services
- File-scoped namespaces everywhere
- Nullable reference types enabled

## Git Strategy (Fully Automated)
Git is fully automated. You (Claude) manage all git operations -- the user never needs to run git commands.

### First-Time Setup (auto-detected)
On every session start, check the user's environment and auto-install anything missing:
1. .NET SDK -- detect platform (WSL2/Windows/Mac), install if missing
2. GitHub CLI (`gh`) -- detect platform, install if missing
3. GitHub auth -- `gh auth login --web` (sign in with company email), then `gh auth setup-git`
4. Git identity -- set name/email if not configured
See git-workflow rules for full platform-specific install commands.

### Session Start (ALWAYS do this first)
1. Run first-time setup check (silent if already configured)
2. `git pull origin main` -- get latest code
3. Check current branch: `git branch --show-current`
4. If on `main`: ask user their name/feature, then `git checkout -b feature/{name}`
5. If on feature branch: `git merge main --no-edit` to sync

### API Key Auto-Detection
On every session start, check if `ANTHROPIC_API_KEY` is already set. If not, auto-detect from the user's local Claude/OpenCode config:

```bash
# Check if already set
if [ -z "$ANTHROPIC_API_KEY" ]; then
    # Use the shared helper (handles jq, python3, and grep fallbacks)
    source scripts/detect-env.sh
    detect_api_key
fi
```

**Security rules:**
- NEVER write the API key to any project file (no `.env`, no `appsettings.json`, no code)
- NEVER commit, log, or display the full API key -- only show masked version (`sk-ant-****xxxx`)
- NEVER read `~/.claude.json` for any purpose other than extracting `primaryApiKey`
- The key lives ONLY as an in-memory environment variable for the current session
- The chat bubble's "env-key-available" endpoint will detect it automatically

**WSL2 note:** If the SQLite database already exists from a previous run, `EnsureCreated` won't add new tables. If you see "no such table" errors, delete the `.db` file and restart:
```bash
rm -f src/AppTemplate.Api/AppTemplate.db
```

If auto-detection succeeds, tell the user: "Found your API key from Claude/OpenCode setup -- AI chat is ready."
If it fails, no error -- the chat bubble will show the manual key entry form as a fallback.

### During Work
- Commit after every meaningful change
- Use conventional prefixes: `feat:`, `fix:`, `docs:`, `chore:`, `refactor:`, `test:`
- Push periodically: `git push -u origin feature/{name}`
- After completing a meaningful milestone (task done, key decision made), update `PLAN.md` -- mark tasks `[x]`, add decisions, note new discoveries. Include the `PLAN.md` change in the same commit as the feature work, not as a separate commit.
- After completing a feature or fix where `dotnet build && dotnet test` pass, **relaunch the running servers** so the user can verify changes immediately:
  1. Kill existing processes on ports 5050 and 8080:
     ```bash
     # WSL2 (preferred)
     fuser -k 5050/tcp 8080/tcp 2>/dev/null
     # Windows fallback
     powershell.exe -Command "Get-NetTCPConnection -LocalPort 5050,8080 -ErrorAction SilentlyContinue | ForEach-Object { Stop-Process -Id \$_.OwningProcess -Force -ErrorAction SilentlyContinue }"
     ```
  2. Start both servers in background:
     ```bash
     source scripts/detect-env.sh
     ensure_native_dotnet
     detect_api_key
     ANTHROPIC_API_KEY="${ANTHROPIC_API_KEY:-}" dotnet run --project src/AppTemplate.Api &
     sleep 3
     ANTHROPIC_API_KEY="${ANTHROPIC_API_KEY:-}" dotnet run --project src/AppTemplate.Web &
     ```
  3. Confirm to the user: "Servers relaunched -- API on 5050, Web on 8080"

### Before Stopping (ALWAYS do this)
1. Update `PLAN.md` with final session status (mark completed tasks, add any decisions made)
2. Commit all pending changes (including `PLAN.md`)
3. Verify: `dotnet build && dotnet test`
4. Merge to main: checkout main, pull, merge feature, verify build+tests, push, switch back
5. The Stop hook (`git-sync.sh`) also does this as a safety net

### Remote Operations
- Once `gh auth setup-git` has been run, both `git` and `git.exe` work for remote ops
- For fresh setups before `gh` is configured, use `git.exe` as fallback (Windows Credential Manager)
- Use `git` for local operations (add, commit, checkout, merge, status)

- See @docs/BRANCHING.md for full details

## Testing Strategy
- Unit tests for all services (xUnit + FluentAssertions)
- Integration tests for API endpoints (`CustomWebApplicationFactory` swaps SQLite for InMemory, mocks IAIService + IApiKeyProtector)
- Component tests for Blazor pages (bUnit)
- Mock external dependencies with NSubstitute
- Naming: `MethodName_Scenario_ExpectedResult`
- Test files mirror source: `tests/.../Features/Weather/WeatherServiceTests.cs`
- Current coverage: 70 tests (61 API + 9 Web)
- **bUnit + MudBlazor note**: MudBlazor's PopoverService only implements `IAsyncDisposable`. Test classes using MudBlazor must implement `IAsyncLifetime` and call `DisposeAsync()` rather than inheriting `BunitContext` directly.
- See @docs/TESTING.md for full details

## AI Integration
- Anthropic SDK is pre-installed (`Anthropic.SDK` NuGet)
- `IAIService` interface supports both default client and per-user API key overloads
- **API key auto-detection**: On session start, Claude reads `~/.claude.json` → `primaryApiKey` and sets it as `ANTHROPIC_API_KEY` env var (in-memory only, never written to project files)
- API key resolution order: (1) per-user key from DB, (2) `ANTHROPIC_API_KEY` env var (auto-detected or manually set), (3) `Anthropic:ApiKey` config
- Global chat bubble provides a ready-to-use AI chat on every page
- If API key is auto-detected, the chat bubble skips setup and goes straight to chat
- Users can also manually enter their own API key in the chat bubble (stored per device in SQLite)
- Chat history persists per device in SQLite (`ChatMessageEntity`)
- Model configurable in `appsettings.json` under `Anthropic:Model`
- See `src/AppTemplate.Api/Shared/AI/` for the wrapper, `Features/Chat/` for chat service

## Database
- **SQLite** via EF Core (`Microsoft.EntityFrameworkCore.Sqlite`)
- Connection string in `appsettings.json` under `ConnectionStrings:DefaultConnection`
- Database auto-created on startup via `EnsureCreatedAsync()`
- Current entities: `ApiKeyEntity` (per-device API keys), `ChatMessageEntity` (chat history), plus Identity tables
- Entity definitions in `Shared/Data/ApiKeyEntity.cs`, `Shared/Data/ChatMessageEntity.cs`; DbSets in `AppDbContext.cs`
- Add entities to `AppDbContext.cs`, add DbSets, configure in `OnModelCreating`
- For quick prototyping, `EnsureCreated` is fine (no migrations needed)

## Security
- **API key encryption at rest**: Per-user API keys are encrypted before storing in SQLite using ASP.NET DataProtection
- `IApiKeyProtector` / `ApiKeyProtector` in `Shared/Security/ApiKeyProtector.cs`
- `Protect()` called on save in `ApiKeyEndpoints.cs`, `Unprotect()` on read
- `ChatService.ResolveApiKeyAsync` decrypts stored keys before passing to AI service
- Registered via `AddSecurityServices()` in `ServiceCollectionExtensions.cs`
- DataProtection keys stored in default location (`~/.aspnet/DataProtection-Keys/`)
- In tests, `IApiKeyProtector` is mocked as a passthrough (Protect/Unprotect return input unchanged)

## UI Library
- **MudBlazor 9.0** is installed and configured globally
- All MudBlazor components available in any `.razor` file (e.g., `<MudButton>`, `<MudCard>`, `<MudDataGrid>`)
- Theme providers are in `MainLayout.razor` (`MudThemeProvider`, `MudDialogProvider`, `MudSnackbarProvider`)
- Docs: https://mudblazor.com/components

## LAN Access
- Both servers bind to `0.0.0.0` (all network interfaces) for LAN accessibility
- **Ports**: API on `0.0.0.0:5050`, Web on `0.0.0.0:8080`
- **Windows Firewall**: Inbound rules must be added for ports 8080 and 5050 (run in admin PowerShell):
  ```powershell
  New-NetFirewallRule -DisplayName "AppTemplate Web" -Direction Inbound -LocalPort 8080 -Protocol TCP -Action Allow
  New-NetFirewallRule -DisplayName "AppTemplate API" -Direction Inbound -LocalPort 5050 -Protocol TCP -Action Allow
  ```
- **Find your IP**: `powershell.exe -Command "(Get-NetIPAddress -AddressFamily IPv4 | Where-Object { $_.InterfaceAlias -notlike '*Loopback*' -and $_.InterfaceAlias -notlike '*vEthernet*' }).IPAddress"`
- LAN users access the app at `http://<your-ip>:8080`
- **Important**: `crypto.randomUUID()` requires HTTPS or localhost. For plain HTTP over LAN, the chat device ID generation falls back to `crypto.getRandomValues()` (see `wwwroot/js/chat-device.js`)
- **JS interop**: Blazor Server cannot access browser APIs (`localStorage`, `crypto`) directly. The small `chat-device.js` file provides this bridge via `IJSRuntime` -- this is standard Blazor Server practice.

@docs/CHEATSHEET.md
@docs/BRANCHING.md
@docs/TESTING.md
