# Git Workflow Rules (Automated)

## CRITICAL: You (Claude) manage ALL git operations and environment setup automatically

The user should NEVER need to run git commands or install tools manually. You handle everything.

## First-Time Setup (auto-detect on every session start)

**NOTE**: See the "Session Start" section at the top of CLAUDE.md for the greeting and onboarding flow. Run that FIRST -- it will trigger these setup checks as part of the onboarding.

**Interactive setup script**: Users can run `bash scripts/onboard.sh` directly in their terminal to check and install all prerequisites interactively (handles sudo prompts, browser auth, git identity). Recommend this when Claude cannot handle an interactive step itself.

Before doing ANYTHING else, check the user's environment and set up everything they need:

> **Interactive commands (sudo, browser auth):** Claude Code cannot handle interactive password prompts. When a step requires `sudo`, tell the user to run `bash scripts/onboard.sh` or present the command to the user and ask them to run it in their terminal. For `gh auth login --web`, the browser opens automatically but the user must complete the login in the browser.

### 1. Check .NET SDK
Run `dotnet --version` to check if .NET is installed.
If NOT installed, detect the platform and install it:
- **WSL2 / Ubuntu / Debian**:
  ```
  sudo apt-get update && sudo apt-get install -y dotnet-sdk-10.0
  ```
  If the package is not found, add the Microsoft apt repo first:
  ```
  sudo apt-get install -y wget
  wget https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
  sudo dpkg -i packages-microsoft-prod.deb
  rm packages-microsoft-prod.deb
  sudo apt-get update && sudo apt-get install -y dotnet-sdk-10.0
  ```
- **Windows**: `winget install Microsoft.DotNet.SDK.10`
- **Mac**: `brew install dotnet-sdk`
- Tell the user: "I'm installing the .NET SDK -- you may be prompted for your password."
- Verify after install: `dotnet --version`

### 2. Check GitHub CLI (`gh`)
Run `which gh || which gh.exe` to check if `gh` is installed.
If NOT installed, detect the platform and install it:
- **WSL2 / Ubuntu / Debian** (requires sudo -- ask the user to run this in their terminal):
  ```
  sudo mkdir -p -m 755 /etc/apt/keyrings && wget -qO- https://cli.github.com/packages/githubcli-archive-keyring.gpg | sudo tee /etc/apt/keyrings/githubcli-archive-keyring.gpg > /dev/null && sudo chmod go+r /etc/apt/keyrings/githubcli-archive-keyring.gpg && sudo bash -c 'echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/githubcli-archive-keyring.gpg] https://cli.github.com/packages stable main" > /etc/apt/sources.list.d/github-cli.list' && sudo apt update && sudo apt install gh -y
  ```
- **Windows**: `winget install --id GitHub.cli -e --accept-source-agreements --accept-package-agreements`
- **Mac**: `brew install gh`
- If auto-install fails, tell the user: "Download the GitHub CLI from https://cli.github.com and then come back."
- Tell the user: "I'm installing the GitHub CLI -- you may be prompted for your password."

### 3. Check GitHub Authentication
Run `gh auth status` to check if authenticated.
If NOT authenticated:
- Tell the user: "I need to connect you to GitHub so your work is saved automatically. A browser window will open -- sign in with your company email (the same one you use for work)."
- Run: `gh auth login --hostname github.com --git-protocol https --web`
- After success, run: `gh auth setup-git` to configure git credential helper
- Verify: `gh auth status`

### 4. Check Git Identity
If `git config user.name` is empty, ask the user for their name and email, then set it:
- `git config user.name "Their Name"`
- `git config user.email "their.email@example.com"`

This entire check takes a few seconds if everything is already set up. It completes silently when nothing needs to be done.

## Session Start Workflow

After first-time setup passes, IMMEDIATELY do the following:

1. Run `git pull origin main` to get the latest code
2. Check which branch the user is on with `git branch --show-current`
3. If on `main`, ask the user for their name or what feature they're working on, then create a feature branch:
   - `git checkout -b feature/{short-name}` (e.g., `feature/user-profile`, `feature/dashboard`)
4. If already on a feature branch, merge latest main: `git merge main --no-edit`
5. Resolve any merge conflicts if they occur

## During Work
- Commit after every meaningful change (new file, completed function, fixed bug)
- Use conventional commit prefixes:
  - `feat:` new features
  - `fix:` bug fixes
  - `docs:` documentation
  - `chore:` maintenance
  - `refactor:` code restructuring
  - `test:` tests
  - `style:` formatting
- Keep commits small and focused
- Push to the feature branch periodically: `git push -u origin feature/{name}`

## Before Stopping
Before you finish a session, ALWAYS:
1. Stage and commit all pending changes
2. Run `dotnet build && dotnet test`
3. If build+tests pass, merge to main:
   ```
   git checkout main
   git pull origin main
   git merge feature/{name} --no-edit
   dotnet build && dotnet test
   git push origin main
   git checkout feature/{name}
   git merge main --no-edit
   ```
4. If there are merge conflicts, resolve them before merging
5. The Stop hook (`git-sync.sh`) will also attempt this as a safety net

## Handling Auth & Push Failures
- If any push/pull/fetch fails with an authentication or credential error:
  1. Run `gh auth status` to check
  2. If not authenticated, walk the user through `gh auth login --hostname github.com --git-protocol https --web`
  3. After auth, run `gh auth setup-git` and retry the failed operation
- If push fails with a permission/access error:
  1. Tell the user: "It looks like your GitHub account doesn't have access to this repo yet. This usually means your team admin needs to add you. Let me check your GitHub username..."
  2. Run `gh api user --jq '.login'` to get their username
  3. Tell the user: "Your GitHub username is {username}. Ask your team lead to make sure you've been added to the repo, then tell me to try again."
  4. Keep their changes safe on the local feature branch in the meantime

## Important Notes
- The correct approach for WSL: install `gh` → `gh auth login` → `gh auth setup-git` → native `git push` works
- Always use `git` (not `git.exe`) for all operations once `gh auth setup-git` has been run
- `git.exe` is unreliable from WSL (path translation breaks Git Credential Manager) -- avoid it unless absolutely no other option exists
- Never commit: `.env` files, API keys, `appsettings.*.local.json`, `bin/`, `obj/`
- Branch naming: `feature/{short-name}` or `fix/{short-name}`
- If a merge to main fails (conflict or broken build), leave changes on the feature branch and inform the user
- The user's changes are ALWAYS safe on their feature branch even if merge fails
