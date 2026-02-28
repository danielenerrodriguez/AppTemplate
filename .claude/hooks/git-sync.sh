#!/bin/bash
# git-sync.sh - Auto-commit, push, and merge to main when build passes
# Runs as a Stop hook AFTER verify-build.sh succeeds
# Uses git.exe for push/pull to leverage Windows credential manager

INPUT=$(cat)

# Use git.exe for remote operations (Windows credentials), git for local ops
GIT_REMOTE="git.exe"
GIT_LOCAL="git"

# Get the project directory
PROJECT_DIR="$CLAUDE_PROJECT_DIR"
if [ -z "$PROJECT_DIR" ]; then
    PROJECT_DIR=$(pwd)
fi

cd "$PROJECT_DIR" || exit 0

# Check if we're in a git repo
if ! $GIT_LOCAL rev-parse --git-dir >/dev/null 2>&1; then
    exit 0
fi

CURRENT_BRANCH=$($GIT_LOCAL branch --show-current 2>/dev/null)

# Skip if on main -- users should be on a feature branch
if [ "$CURRENT_BRANCH" = "main" ] || [ -z "$CURRENT_BRANCH" ]; then
    exit 0
fi

# Check if there are any changes to commit
if $GIT_LOCAL diff --quiet HEAD 2>/dev/null && $GIT_LOCAL diff --cached --quiet 2>/dev/null && [ -z "$($GIT_LOCAL ls-files --others --exclude-standard 2>/dev/null)" ]; then
    # No changes to commit, but still try to push if there are unpushed commits
    UNPUSHED=$($GIT_LOCAL log origin/$CURRENT_BRANCH..$CURRENT_BRANCH --oneline 2>/dev/null)
    if [ -z "$UNPUSHED" ]; then
        exit 0
    fi
else
    # Stage all changes
    $GIT_LOCAL add -A 2>/dev/null

    # Generate a commit message based on what changed
    CHANGED_FILES=$($GIT_LOCAL diff --cached --name-only 2>/dev/null | head -5)
    FILE_COUNT=$($GIT_LOCAL diff --cached --name-only 2>/dev/null | wc -l | tr -d ' ')

    if [ "$FILE_COUNT" -eq 0 ]; then
        exit 0
    fi

    # Build a useful commit message
    FIRST_FILE=$(echo "$CHANGED_FILES" | head -1 | xargs basename 2>/dev/null)
    if [ "$FILE_COUNT" -eq 1 ]; then
        COMMIT_MSG="feat: update $FIRST_FILE"
    else
        COMMIT_MSG="feat: update $FIRST_FILE and $((FILE_COUNT - 1)) other file(s)"
    fi

    # Commit
    $GIT_LOCAL commit -m "$COMMIT_MSG" 2>/dev/null
    if [ $? -ne 0 ]; then
        echo "Warning: Auto-commit failed. You may need to commit manually." >&2
        exit 0
    fi
fi

# Push the feature branch
PUSH_OUTPUT=$($GIT_REMOTE push -u origin "$CURRENT_BRANCH" 2>&1)
PUSH_EXIT=$?

if [ $PUSH_EXIT -ne 0 ]; then
    # Check if it's an auth failure
    if echo "$PUSH_OUTPUT" | grep -qi "authentication\|credential\|403\|fatal: could not read Username\|Authorization"; then
        echo "Push failed: GitHub authentication is not set up." >&2
        echo "Next time you start Claude, it will automatically set up your GitHub access." >&2
        echo "Or run this command manually: gh auth login --web" >&2
    else
        echo "Warning: Could not push to origin/$CURRENT_BRANCH. Check your network connection." >&2
        echo "Your changes are committed locally and safe." >&2
    fi
    exit 0
fi

# Now attempt to merge into main
# Pull latest main first
$GIT_LOCAL checkout main 2>/dev/null
$GIT_REMOTE pull origin main 2>/dev/null

# Verify main builds before we modify it
BUILD_OUTPUT=$(dotnet build --nologo --verbosity quiet 2>&1)
if [ $? -ne 0 ]; then
    echo "Warning: main branch has build errors. Skipping merge. Your changes are safely on origin/$CURRENT_BRANCH." >&2
    $GIT_LOCAL checkout "$CURRENT_BRANCH" 2>/dev/null
    exit 0
fi

# Merge the feature branch into main
MERGE_OUTPUT=$($GIT_LOCAL merge "$CURRENT_BRANCH" --no-edit 2>&1)
MERGE_EXIT=$?

if [ $MERGE_EXIT -ne 0 ]; then
    echo "Warning: Merge conflict detected. Your changes are safely pushed to origin/$CURRENT_BRANCH." >&2
    echo "Ask Claude to help resolve: 'pull main and resolve any merge conflicts'" >&2
    $GIT_LOCAL merge --abort 2>/dev/null
    $GIT_LOCAL checkout "$CURRENT_BRANCH" 2>/dev/null
    exit 0
fi

# Verify the merged code still builds
BUILD_OUTPUT=$(dotnet build --nologo --verbosity quiet 2>&1)
if [ $? -ne 0 ]; then
    echo "Warning: Build fails after merge. Reverting merge. Your changes are safely on origin/$CURRENT_BRANCH." >&2
    $GIT_LOCAL reset --hard HEAD~1 2>/dev/null
    $GIT_LOCAL checkout "$CURRENT_BRANCH" 2>/dev/null
    exit 0
fi

# Run tests on merged code
TEST_OUTPUT=$(dotnet test --nologo --verbosity quiet 2>&1)
if [ $? -ne 0 ]; then
    echo "Warning: Tests fail after merge. Reverting merge. Your changes are safely on origin/$CURRENT_BRANCH." >&2
    $GIT_LOCAL reset --hard HEAD~1 2>/dev/null
    $GIT_LOCAL checkout "$CURRENT_BRANCH" 2>/dev/null
    exit 0
fi

# Everything passed -- push main
$GIT_REMOTE push origin main 2>&1
if [ $? -ne 0 ]; then
    echo "Warning: Could not push main to origin. Your merge is local only." >&2
fi

# Switch back to the feature branch
$GIT_LOCAL checkout "$CURRENT_BRANCH" 2>/dev/null

# Pull the updated main back into the feature branch
$GIT_LOCAL merge main --no-edit 2>/dev/null

echo "Auto-sync complete: $CURRENT_BRANCH -> main (build + tests verified)" >&2
exit 0
