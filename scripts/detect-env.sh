#!/bin/bash
# detect-env.sh - Shared environment setup helper
# Source this from other scripts: source scripts/detect-env.sh

# Ensure HOME is set (it can be empty in some execution contexts like Claude Code hooks)
if [[ -z "$HOME" ]]; then
    export HOME=$(eval echo ~)
fi

# Check if a dotnet path is actually the Windows binary (directly or via symlink)
_is_windows_dotnet() {
    local p="$1"
    [[ -z "$p" ]] && return 0
    # Resolve symlinks to get the real path
    local resolved
    resolved=$(readlink -f "$p" 2>/dev/null || realpath "$p" 2>/dev/null || echo "$p")
    # Windows binaries live under /mnt/c/ (or /mnt/d/, etc.)
    [[ "$resolved" == /mnt/* ]]
}

# Ensure a native Linux dotnet is on PATH (not the Windows binary via WSL2 interop)
ensure_native_dotnet() {
    local current_dotnet
    current_dotnet=$(which dotnet 2>/dev/null)

    # If dotnet is not found or is a Windows binary (directly or via symlink), we need native Linux dotnet
    if [[ -z "$current_dotnet" ]] || _is_windows_dotnet "$current_dotnet"; then
        # Check common native install locations
        if [[ -x "$HOME/.dotnet/dotnet" ]]; then
            export PATH="$HOME/.dotnet:$PATH"
        elif [[ -x "/usr/share/dotnet/dotnet" ]]; then
            export PATH="/usr/share/dotnet:$PATH"
        elif [[ -x "/usr/bin/dotnet" ]] && ! _is_windows_dotnet "/usr/bin/dotnet"; then
            export PATH="/usr/bin:$PATH"
        else
            # Auto-install via Microsoft's official install script
            echo "Installing native Linux .NET SDK..." >&2
            local install_script="/tmp/dotnet-install.sh"
            curl -sSL https://dot.net/v1/dotnet-install.sh -o "$install_script" 2>/dev/null \
                || curl -sSLk https://dot.net/v1/dotnet-install.sh -o "$install_script" 2>/dev/null
            if [[ -f "$install_script" ]]; then
                chmod +x "$install_script"
                "$install_script" --channel 10.0 --install-dir "$HOME/.dotnet" >&2
            fi
            export PATH="$HOME/.dotnet:$PATH"
            export DOTNET_ROOT="$HOME/.dotnet"
        fi
    fi
}

# Detect Anthropic API key from Claude/OpenCode config
detect_api_key() {
    if [[ -n "$ANTHROPIC_API_KEY" ]]; then
        return 0
    fi
    local key=""
    if command -v jq >/dev/null 2>&1 && [[ -f ~/.claude.json ]]; then
        key=$(jq -r '.primaryApiKey // ""' ~/.claude.json 2>/dev/null)
    elif command -v python3 >/dev/null 2>&1 && [[ -f ~/.claude.json ]]; then
        key=$(python3 -c "import json; print(json.load(open('$HOME/.claude.json')).get('primaryApiKey',''))" 2>/dev/null)
    elif [[ -f ~/.claude.json ]]; then
        key=$(grep -o '"primaryApiKey":"[^"]*"' ~/.claude.json 2>/dev/null | head -1 | sed 's/.*:"//;s/"//')
    fi
    if [[ -n "$key" ]]; then
        export ANTHROPIC_API_KEY="$key"
    fi
}
