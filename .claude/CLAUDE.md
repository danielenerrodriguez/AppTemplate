# AppTemplate - Claude Code Configuration

This directory contains Claude Code rules, hooks, and slash commands for the project.

## Slash Commands
See `.claude/commands/` for custom slash commands:
- `/new-feature {name}` - Scaffold a complete vertical slice (CRUD endpoints, service, Blazor page, tests)
- `/kickoff {topic}` - Run the hackathon kickoff flow (update CLAUDE.md, propose features, scaffold all)
- `/status` - Show git, build, test, and feature status report
- `/onboard` - Run environment setup checks (or direct user to `bash scripts/onboard.sh`)

## Rules
See `.claude/rules/` for topic-specific coding rules:
- `csharp-style.md` - C# coding conventions
- `testing.md` - Testing rules and patterns
- `blazor.md` - Blazor component conventions
- `api.md` - Minimal API conventions
- `git-workflow.md` - Git workflow, auto-setup, and commit rules

## Hooks
See `.claude/hooks/` for automated workflow scripts:
- `verify-build.sh` - Verifies project builds before Claude stops
- `git-sync.sh` - Auto-commits, pushes, and merges to main on stop (after build passes)
- `format-check.sh` - Runs dotnet format after file edits
- `protect-files.sh` - Prevents edits to sensitive files
