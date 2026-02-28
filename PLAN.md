# Project Plan

> Auto-maintained by Claude. Last updated: 2026-02-28

## Goal
<!-- Filled in by /kickoff or manually. Describe the project topic and what you're building. -->

## Tasks
<!-- 
Format: - [status] Task name (@person, feature/branch-name) - optional notes
Statuses: [ ] = todo, [~] = in progress, [x] = done
-->
- [x] Set up project scaffolding (main) - solution, projects, nuget packages
- [x] Weather vertical slice example (main) - CRUD endpoints + Blazor page
- [x] AI Chat bubble with per-user API keys (main) - global floating chat
- [x] SQLite database + EF Core (main) - ApiKeys, ChatMessages, Identity
- [x] MudBlazor UI configuration (main) - theme, providers, global imports
- [x] CI pipeline + Docker (main) - GitHub Actions, Dockerfile, docker-compose
- [x] LAN access (main) - bind 0.0.0.0, firewall rules, crypto fallback
- [x] Model selector for chat bubble (main) - dynamic model list, per-user preference in localStorage
- [x] Infrastructure alignment (main) - global.json, Directory.Build.props, .dockerignore, Dockerfile/CI/compose fixed for .NET 10
- [x] Code quality fixes (main) - 10 fixes from comprehensive code review
- [x] Maintainability audit Phase 1 (main) - Remove Bootstrap, full MudBlazor conversion (NavMenu, MainLayout, WeatherPage)
- [x] Maintainability audit Phase 2 (main) - Drop ApiResponse<T> wrapper, standardize on raw TypedResults
- [x] Maintainability audit Phase 3 (main) - Fix Weather feature (Fahrenheit formula, null for unknown cities, DTO duplication comment)
- [x] Maintainability audit Phase 4 (main) - XML docs on interfaces, extract MaxTokens to config, fix .http file, delete Counter.razor
- [x] Maintainability audit Phase 5 (main) - File reorg (ApiKeys feature folder, entity split), Identity docs, CORS constant

- [x] Group A (main) - Fix MudAppBar clipping page content (margin-top on MudMainContent)
- [x] Group B: Test coverage (main) - Integration tests + expanded unit tests, total coverage 70 tests (was 36)
- [x] Group C: Security hardening (main) - Encrypt API keys at rest with IDataProtector (IApiKeyProtector)

### Remaining Review Groups (Not Yet Started)
- [ ] Group #6: Add auth to API key/chat endpoints (require Identity or device token)

## Decisions
<!-- Key technical and design decisions the team has made. Helps new teammates get context fast. -->
- Using .NET 10 Minimal API + Blazor Server
- SQLite for storage (no external DB needed)
- MudBlazor 9.0 for UI components (Bootstrap fully removed)
- Feature-based vertical slices architecture
- AI chat bubble built-in with per-user API key support
- Centralized build props via Directory.Build.props (DRY - change TFM in one place)
- TreatWarningsAsErrors enabled globally
- Docker images use GA dotnet/sdk:10.0 and dotnet/aspnet:10.0 tags
- docker-compose defaults to Production environment (teams override for dev)
- Dropped ApiResponse<T> wrapper — endpoints return DTOs directly via TypedResults (idiomatic Minimal API)
- PagedResult<T> kept as opt-in for pagination scenarios
- Identity pre-wired as scaffolding (not protecting any endpoints yet)
- Duplicate WeatherForecastDto in Web project — intentional to avoid cross-project SDK.Web conflicts
- MaxTokens configurable via Anthropic:MaxTokens in appsettings.json (default 1024)

## Notes
<!-- Anything else: links, blockers, ideas for later, etc. -->
- Windows elevation prompt during `dotnet test` is a one-time OS-level security prompt (Windows Firewall/Defender), not caused by the project
- The installed SDK is 10.0.200-preview but global.json uses `rollForward: latestMajor` + `allowPrerelease: true` to accommodate both preview and GA SDKs
- Build: 0 errors, 0 warnings. Tests: 70 passing (61 API + 9 Web)
- Integration tests use CustomWebApplicationFactory (InMemory EF, mocked IAIService + IApiKeyProtector)
- bUnit tests for MudBlazor components require IAsyncLifetime due to PopoverService IAsyncDisposable
