#!/bin/bash
# scripts/onboard.sh - Interactive environment setup for AppTemplate
# Run this directly in your terminal: bash scripts/onboard.sh
#
# Checks each prerequisite and either auto-fixes it or prints a
# copy-paste command and waits for you to confirm.

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/detect-env.sh"

# --- helpers ----------------------------------------------------------------

green()  { printf '\033[1;32m%s\033[0m' "$*"; }
red()    { printf '\033[1;31m%s\033[0m' "$*"; }
yellow() { printf '\033[1;33m%s\033[0m' "$*"; }
bold()   { printf '\033[1m%s\033[0m' "$*"; }

ok()   { echo "  [$(green OK)] $*"; }
fail() { echo "  [$(red '!!')] $*"; }
skip() { echo "  [$(yellow '--')] $*"; }
info() { echo "       $*"; }

# Pause until the user presses Enter
wait_for_user() {
    echo ""
    read -rp "  Press Enter when ready..." _
    echo ""
}

# --- checks -----------------------------------------------------------------

check_dotnet() {
    echo ""
    bold "1. .NET SDK"
    echo ""

    ensure_native_dotnet  # from detect-env.sh (installs to ~/.dotnet if needed)

    local ver
    ver=$(dotnet --version 2>/dev/null || true)
    if [[ -n "$ver" ]]; then
        ok ".NET SDK $ver"
        return 0
    else
        fail ".NET SDK not found after install attempt"
        info "Try manually: curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 10.0"
        return 1
    fi
}

check_gh() {
    echo ""
    bold "2. GitHub CLI (gh)"
    echo ""

    if command -v gh &>/dev/null; then
        local ver
        ver=$(gh --version 2>/dev/null | head -1 | awk '{print $3}')
        ok "GitHub CLI $ver"
        return 0
    fi

    fail "GitHub CLI not found"
    echo ""
    info "Run this command in your terminal, then press Enter to continue:"
    echo ""
    cat <<'CMD'
    sudo mkdir -p -m 755 /etc/apt/keyrings \
      && wget -qO- https://cli.github.com/packages/githubcli-archive-keyring.gpg \
           | sudo tee /etc/apt/keyrings/githubcli-archive-keyring.gpg > /dev/null \
      && sudo chmod go+r /etc/apt/keyrings/githubcli-archive-keyring.gpg \
      && echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/githubcli-archive-keyring.gpg] https://cli.github.com/packages stable main" \
           | sudo tee /etc/apt/sources.list.d/github-cli.list > /dev/null \
      && sudo apt update && sudo apt install gh -y
CMD
    echo ""
    wait_for_user

    if command -v gh &>/dev/null; then
        local ver
        ver=$(gh --version 2>/dev/null | head -1 | awk '{print $3}')
        ok "GitHub CLI $ver"
        return 0
    else
        fail "GitHub CLI still not found -- install it from https://cli.github.com"
        return 1
    fi
}

check_gh_auth() {
    echo ""
    bold "3. GitHub authentication"
    echo ""

    if gh auth status &>/dev/null; then
        local user
        user=$(gh api user --jq '.login' 2>/dev/null || echo "unknown")
        ok "Authenticated as @$user"
        return 0
    fi

    fail "Not authenticated with GitHub"
    info "A browser window will open -- sign in with your company email."
    echo ""
    gh auth login --hostname github.com --git-protocol https --web || true

    if gh auth status &>/dev/null; then
        local user
        user=$(gh api user --jq '.login' 2>/dev/null || echo "unknown")
        ok "Authenticated as @$user"
        return 0
    else
        fail "GitHub authentication failed -- try again or ask for help"
        return 1
    fi
}

check_git_creds() {
    echo ""
    bold "4. Git credential helper"
    echo ""

    gh auth setup-git 2>/dev/null || true
    ok "Git credentials configured"
}

check_git_identity() {
    echo ""
    bold "5. Git identity"
    echo ""

    local name email
    name=$(git config user.name 2>/dev/null || true)
    email=$(git config user.email 2>/dev/null || true)

    if [[ -n "$name" && -n "$email" ]]; then
        ok "Git identity: $name <$email>"
        return 0
    fi

    fail "Git identity not configured"

    if [[ -z "$name" ]]; then
        read -rp "  Your name (e.g. Jane Smith): " name
        git config --global user.name "$name"
    fi
    if [[ -z "$email" ]]; then
        read -rp "  Your email (e.g. jane@company.com): " email
        git config --global user.email "$email"
    fi

    ok "Git identity: $name <$email>"
}

check_api_key() {
    echo ""
    bold "6. Anthropic API key (optional)"
    echo ""

    ANTHROPIC_API_KEY="${ANTHROPIC_API_KEY:-}"
    detect_api_key  # from detect-env.sh

    if [[ -n "${ANTHROPIC_API_KEY:-}" ]]; then
        local masked="${ANTHROPIC_API_KEY:0:7}****${ANTHROPIC_API_KEY: -4}"
        ok "API key detected ($masked)"
    else
        skip "Anthropic API key: not found (optional -- enter in chat bubble)"
    fi
}

# --- main -------------------------------------------------------------------

echo ""
echo "  $(bold 'AppTemplate Environment Setup')"
echo "  ================================="
echo ""
echo "  Checking prerequisites..."

errors=0

check_dotnet  || ((errors++))
check_gh      || ((errors++))
check_gh_auth || ((errors++))
check_git_creds
check_git_identity
check_api_key

echo ""
echo "  ---------------------------------"
if [[ $errors -eq 0 ]]; then
    echo "  $(green 'All checks passed!') Run $(bold 'claude') to start."
else
    echo "  $(red "$errors check(s) failed.") Fix the issues above and re-run:"
    echo "  $(bold 'bash scripts/onboard.sh')"
fi
echo ""
