#!/bin/bash
# format-check.sh - Auto-format C# files after Claude edits them
# Runs as a PostToolUse hook on Edit|Write

INPUT=$(cat)
FILE_PATH=$(echo "$INPUT" | jq -r '.tool_input.file_path // .tool_input.filePath // empty')

# Only process .cs and .razor files
if [[ "$FILE_PATH" == *.cs ]] || [[ "$FILE_PATH" == *.razor ]]; then
    # Source platform detection helper
    SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
    DETECT_ENV="$SCRIPT_DIR/../../scripts/detect-env.sh"
    if [[ -f "$DETECT_ENV" ]]; then
        source "$DETECT_ENV"
        ensure_native_dotnet
    fi

    # Run dotnet format on the specific file (silently)
    dotnet format --include "$FILE_PATH" 2>/dev/null
fi

exit 0
