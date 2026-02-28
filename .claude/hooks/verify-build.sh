#!/bin/bash
# verify-build.sh - Ensure the project builds before Claude finishes
# Runs as a Stop hook

INPUT=$(cat)

# Check if this is a recursive stop (prevent infinite loop)
STOP_HOOK_ACTIVE=$(echo "$INPUT" | jq -r '.stop_hook_active // false')
if [ "$STOP_HOOK_ACTIVE" = "true" ]; then
    exit 0
fi

# Try to build the project
BUILD_OUTPUT=$(dotnet build --nologo --verbosity quiet 2>&1)
BUILD_EXIT=$?

if [ $BUILD_EXIT -ne 0 ]; then
    echo "Build failed. Please fix the build errors before finishing:" >&2
    echo "$BUILD_OUTPUT" >&2
    exit 2
fi

exit 0
