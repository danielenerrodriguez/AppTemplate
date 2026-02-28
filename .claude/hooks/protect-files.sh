#!/bin/bash
# protect-files.sh - Prevent edits to sensitive/config files
# Runs as a PreToolUse hook on Edit|Write

INPUT=$(cat)
FILE_PATH=$(echo "$INPUT" | jq -r '.tool_input.file_path // .tool_input.filePath // empty')

# List of protected file patterns
PROTECTED_PATTERNS=(
    ".env"
    "appsettings.Production.json"
    ".claude/settings.json"
    ".claude/hooks/"
    ".github/workflows/"
)

for pattern in "${PROTECTED_PATTERNS[@]}"; do
    if [[ "$FILE_PATH" == *"$pattern"* ]]; then
        echo "Blocked: '$FILE_PATH' matches protected pattern '$pattern'. This file should be edited manually, not by Claude." >&2
        exit 2
    fi
done

exit 0
