# Team Onboarding Guide

Welcome to the team! No coding experience required.

## Prerequisites

You only need one thing installed:

- **Claude Code** -- follow install instructions at: https://docs.anthropic.com/en/docs/claude-code
  - You'll need an API key -- ask the team lead if you don't have one

That's it. Claude will install everything else for you.

## Getting Started

After cloning the repo, open a terminal in the project folder and run the setup checker:

```
bash scripts/onboard.sh
```

This interactive script checks all prerequisites (.NET SDK, GitHub CLI, auth, git identity) and walks you through installing anything that's missing. Once everything is green, start Claude:

```
claude
```

**Claude handles everything automatically on first launch:**

1. Verifies your environment (or runs the same checks as `onboard.sh`)
2. Pulls the latest code
3. Creates a feature branch for your work
4. Asks what you'd like to build

If Claude says you need collaborator access, send your GitHub username to the repo owner so they can add you. Accept the invite at github.com/notifications, then tell Claude to try again.

## Working (Every Time)

### Step 1: Open terminal in the project folder

```
cd path/to/AppTemplate
```

### Step 2: Start Claude Code

```
claude
```

### Step 3: Tell Claude what you're working on

Just type in plain English. Examples:

- "I'm Sarah and I'm working on the user profile feature"
- "I'm working on the dashboard, pull the latest code and set me up"
- "Continue working on feature/ai-chat"

**Claude will automatically:**
- Pull the latest code from the team
- Create or switch to your feature branch
- Set everything up for you

### Step 4: Describe what you want

Tell Claude what to build, fix, or change in plain English:

- "Create a page that shows a list of users"
- "Add a button that exports data to CSV"
- "The page is showing an error, fix it" (paste the error)
- "Make the home page look better"

### Step 5: When you're done

Just tell Claude "I'm done for now" or close the terminal.

**Claude will automatically:**
- Save and commit your work
- Push your changes so the team can see them
- Merge your work into the main project (if everything passes tests)
- If there's a conflict with someone else's work, Claude will tell you

## That's It!

You don't need to learn git commands, install developer tools, memorize terminal commands, or understand the code structure. Claude handles all of that for you.

## If Something Goes Wrong

| Problem | What to say to Claude |
|---------|----------------------|
| "Build failed" | "The build is broken, can you fix it?" |
| Merge conflict | "There's a merge conflict, help me resolve it" |
| Want to undo changes | "Undo my last changes" |
| Lost or confused | "What branch am I on and what have I changed?" |
| App won't start | "Help me start the API and web app" |
| Need to start fresh | "Switch me back to main and create a new branch" |
| Can't push to GitHub | "Help me set up GitHub access" |
| Authentication error | "My GitHub auth isn't working, help me fix it" |
| Missing tool or SDK | "Check my setup and install anything that's missing" |

## Project Quick Reference

| What | Where |
|------|-------|
| API (backend) | http://localhost:5050 |
| Web (frontend) | http://localhost:8080 |
| LAN access | http://\<host-ip\>:8080 (see README for firewall setup) |
| Claude cheat sheet | `docs/CHEATSHEET.md` |
| Testing guide | `docs/TESTING.md` |
| Branch strategy | `docs/BRANCHING.md` |

## For Experienced Developers

If you prefer working without Claude Code, see the full project structure and conventions in `CLAUDE.md` at the project root. Key points:

- **Architecture**: Feature-based vertical slices
- **Backend**: .NET Minimal API at `src/AppTemplate.Api/`
- **Frontend**: Blazor Server at `src/AppTemplate.Web/`
- **Tests**: xUnit + FluentAssertions + NSubstitute + bUnit
- **IDE**: Open `AppTemplate.slnx` in Visual Studio or Rider
- **Git**: Run `gh auth login` then `gh auth setup-git` to configure credentials. After that, native `git` works for all operations.
