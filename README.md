# AppTemplate

A project template with .NET 10, Blazor Server, and AI integration pre-configured. Teams can clone and start building in under 5 minutes.

## Features

- **AI Chat Bubble** -- Global floating chat (bottom-right corner) with Claude integration, per-user API keys, model selector, and persistent history
- **Weather** -- Example CRUD feature demonstrating the vertical slice pattern
- **API Key Encryption** -- API keys encrypted at rest with ASP.NET DataProtection (IDataProtector)
- **Authentication** -- ASP.NET Identity with SQLite storage
- **MudBlazor UI** -- Component library configured globally (buttons, cards, tables, dialogs, etc.)

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Backend | .NET 10 Minimal API |
| Frontend | Blazor Server |
| Database | SQLite via EF Core |
| AI | Anthropic SDK (Claude) |
| UI | MudBlazor 9.0 |
| Testing | xUnit, FluentAssertions, NSubstitute, bUnit |
| CI | GitHub Actions |

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (pinned via `global.json`)

## Quick Start

```bash
# Clone
git clone https://github.com/danielenerrodriguez/AppTemplate.git
cd AppTemplate

# Run both API (port 5050) and Web (port 8080)
./launch-all.sh        # Linux/Mac
.\launch-all.ps1       # Windows

# Or run separately
dotnet run --project src/AppTemplate.Api
dotnet run --project src/AppTemplate.Web

# Build & test
dotnet build
dotnet test

# Docker
docker compose up --build
```

## Build Infrastructure

| File | Purpose |
|------|---------|
| `global.json` | Pins .NET 10 SDK version with `rollForward: latestMajor` |
| `Directory.Build.props` | Centralizes TargetFramework, Nullable, ImplicitUsings, TreatWarningsAsErrors |
| `.dockerignore` | Keeps Docker build context small (excludes tests, docs, IDE files) |
| `.editorconfig` | Code style rules (file-scoped namespaces, var preferences, indent) |
| `nuget.config` | Local NuGet config with `<clear />` to avoid broken sources |

## Project Structure

```
src/
  AppTemplate.Api/           # .NET Minimal API (port 5050)
    Features/                # Vertical slice features
      Weather/               # Example CRUD feature
      Chat/                  # AI chat service + endpoints
      ApiKeys/               # Per-device API key management
    Shared/
      AI/                    # IAIService + ClaudeService
      Auth/                  # Identity + AppDbContext
      Data/                  # Entity definitions (ApiKeyEntity, ChatMessageEntity)
      Models/                # PagedResult<T> (opt-in pagination)
  AppTemplate.Web/           # Blazor Server (port 8080)
    Components/
      Layout/                # MudBlazor layout (MainLayout, NavMenu, ChatBubble)
      Features/              # Feature pages
      Pages/                 # Home, Error
tests/
  AppTemplate.Api.Tests/     # API unit + integration tests (61 tests)
  AppTemplate.Web.Tests/     # Blazor component tests (9 tests)
```

## AI Integration

The built-in chat bubble supports two modes:
1. **Server key** -- Set `ANTHROPIC_API_KEY` env var and all users share it
2. **Per-user key** -- Each user enters their own API key (encrypted at rest with IDataProtector, stored per device in SQLite)

Users can select which Claude model to use via the Settings panel in the chat bubble (gear icon). The model preference persists in the browser's localStorage. Available models are fetched dynamically from the Anthropic API (`GET /api/chat/models`).

## Team Onboarding

This repo is managed by Claude Code. Teammates just need to:

1. Get added as a GitHub collaborator
2. Open the repo in Claude Code
3. Follow the automated onboarding prompts

Claude handles all git operations, environment setup, and code scaffolding. Use `/new-feature {name}` to scaffold features instantly.

## LAN Access

Both servers bind to `0.0.0.0` so teammates on the same network can access the app. To enable this:

1. **Open Windows Firewall** (run once in admin PowerShell):
   ```powershell
   New-NetFirewallRule -DisplayName "AppTemplate Web" -Direction Inbound -LocalPort 8080 -Protocol TCP -Action Allow
   New-NetFirewallRule -DisplayName "AppTemplate API" -Direction Inbound -LocalPort 5050 -Protocol TCP -Action Allow
   ```
2. **Find your IP**:
   ```powershell
   (Get-NetIPAddress -AddressFamily IPv4 | Where-Object { $_.InterfaceAlias -notlike '*Loopback*' -and $_.InterfaceAlias -notlike '*vEthernet*' }).IPAddress
   ```
3. Other devices access the app at `http://<your-ip>:8080`
