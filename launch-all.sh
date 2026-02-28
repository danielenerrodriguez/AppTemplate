#!/bin/bash
# launch-all.sh - Start both API and Web projects in parallel
# Usage: ./launch-all.sh

echo "Starting AppTemplate..."
echo "  API: http://localhost:5050"
echo "  Web: http://localhost:8080"
echo "  Press Ctrl+C to stop both"
echo ""

# Build first
dotnet build --nologo --verbosity quiet
if [ $? -ne 0 ]; then
    echo "Build failed. Fix errors before launching."
    exit 1
fi

# Start API in background
dotnet run --project src/AppTemplate.Api --no-build 2>&1 | sed 's/^/[API] /' &
API_PID=$!

# Start Web in background
dotnet run --project src/AppTemplate.Web --no-build 2>&1 | sed 's/^/[WEB] /' &
WEB_PID=$!

# Trap Ctrl+C to kill both
trap "echo 'Stopping...'; kill $API_PID $WEB_PID 2>/dev/null; echo 'Stopped.'; exit 0" SIGINT SIGTERM

# Wait for both
wait $API_PID $WEB_PID
