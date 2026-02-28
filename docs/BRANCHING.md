# Branching Strategy

## Automated Git Workflow

**Git is managed automatically by Claude Code.** You don't need to run any git commands yourself. When you start a Claude Code session, it will pull the latest code, set up your branch, and when you're done it will commit, push, and merge for you.

The information below is for reference in case you need to understand what's happening or troubleshoot.

---

## Overview
We use a simplified branching strategy optimized for hackathon speed with 10 people.

## Branch Structure

```
main (always buildable, deploy from here)
 |
 |-- feature/user-auth        (Person/Pair A)
 |-- feature/dashboard         (Person/Pair B)
 |-- feature/ai-chat           (Person/Pair C)
 |-- feature/data-export        (Person/Pair D)
 |-- fix/login-error            (Hotfix)
```

## Rules

### Main Branch
- `main` must always build and pass tests
- Never commit directly to `main`
- Merge your feature branch to `main` frequently (every 30-60 min)
- After merging, pull main into your feature branch to stay up to date

### Feature Branches
- Naming: `feature/{short-descriptive-name}`
- One branch per person or pair
- Examples: `feature/user-profile`, `feature/ai-chat`, `feature/dashboard`

### Hotfix Branches
- Naming: `fix/{short-description}`
- For urgent fixes to `main`
- Example: `fix/build-error`, `fix/api-cors`

## What Claude Does Automatically

### Session Start
1. Pulls latest `main` from GitHub
2. Creates or switches to your feature branch
3. Merges latest `main` into your feature branch

### During Work
1. Commits after every meaningful change with conventional prefixes
2. Pushes to your feature branch periodically

### Session End
1. Commits all remaining changes
2. Verifies build and tests pass
3. Merges feature branch into `main`
4. Pushes `main` to GitHub
5. Switches back to your feature branch

### If Something Goes Wrong
- If merge fails (conflict), your changes stay safely on your feature branch
- Claude will tell you about the conflict and help resolve it
- If the build fails after merge, the merge is automatically reverted

## Manual Workflow (Reference Only)

If you ever need to run git commands manually:

### Starting Work
```bash
git checkout main
git.exe pull origin main
git checkout -b feature/my-feature
```

### During Work
```bash
# Commit often with clear messages
git add .
git commit -m "feat: add user profile page"

# Stay up to date with main (do this every 30-60 min)
git checkout main
git.exe pull origin main
git checkout feature/my-feature
git merge main
# Resolve any conflicts, then continue working
```

### Merging to Main
```bash
# Make sure your code builds and tests pass
dotnet build
dotnet test

# Merge to main
git checkout main
git.exe pull origin main
git merge feature/my-feature

# Push (use git.exe for remote operations)
git.exe push origin main

# Go back to your feature branch
git checkout feature/my-feature
```

**Note**: Always use `git.exe` (not `git`) for push/pull/fetch operations -- it has the Windows credentials.

## Commit Message Convention
Use these prefixes to keep history readable:

| Prefix | When to use | Example |
|--------|------------|---------|
| `feat:` | New feature | `feat: add user registration page` |
| `fix:` | Bug fix | `fix: resolve null reference in weather service` |
| `docs:` | Documentation | `docs: update README with setup instructions` |
| `chore:` | Maintenance | `chore: update NuGet packages` |
| `refactor:` | Code restructure | `refactor: extract weather logic to service` |
| `test:` | Tests | `test: add unit tests for WeatherService` |
| `style:` | Formatting | `style: apply dotnet format` |

## Handling Merge Conflicts
1. Don't panic -- conflicts are normal with 10 people
2. Tell Claude: "there's a merge conflict, help me resolve it"
3. Claude will open the conflicted files, resolve them, and verify the build

If resolving manually:
1. Open the conflicted files (marked with `<<<<<<<` and `>>>>>>>`)
2. Decide which changes to keep (or combine both)
3. Remove the conflict markers
4. Build and test: `dotnet build && dotnet test`
5. Commit the resolution: `git add . && git commit -m "fix: resolve merge conflict in [file]"`

## Why No Pull Requests?
In a hackathon, PRs add friction and slow you down. Instead:
- Build and test before merging (automated by Claude and CI)
- Communicate with your team ("I'm merging my auth changes to main")
- Keep changes small and merge often to minimize conflicts
