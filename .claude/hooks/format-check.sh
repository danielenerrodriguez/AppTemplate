#!/bin/bash
# format-check.sh - Auto-format C# files after Claude edits them
# Runs as a PostToolUse hook on Edit|Write

INPUT=$(cat)
FILE_PATH=$(echo "$INPUT" | jq -r '.tool_input.file_path // .tool_input.filePath // empty')

# Only process .cs and .razor files
if [[ "$FILE_PATH" == *.cs ]] || [[ "$FILE_PATH" == *.razor ]]; then
    # Run dotnet format on the specific file (silently)
    dotnet format --include "$FILE_PATH" 2>/dev/null
fi

exit 0
