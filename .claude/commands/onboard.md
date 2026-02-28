Run the environment setup checker. Verify all prerequisites are installed and configured.

Run each of these checks and report results using `[OK]`, `[!!]` (needs action), or `[--]` (skipped) prefixes:

1. **.NET SDK**: Run `source scripts/detect-env.sh && ensure_native_dotnet && dotnet --version`. Report the version if installed, `[!!]` if missing.
2. **GitHub CLI**: Run `which gh && gh --version`. Report the version if installed, `[!!]` if missing.
3. **GitHub Auth**: Run `gh auth status 2>&1`. Report `[OK]` if authenticated, `[!!]` if not.
4. **Git Identity**: Run `git config user.name` and `git config user.email`. Report `[OK]` with name/email if set, `[!!]` if either is missing.
5. **Anthropic API Key**: Run `source scripts/detect-env.sh && detect_api_key`. Report `[OK]` (masked) if found, `[--] Not configured (optional)` if not.

After running all checks:

- **If all pass**: Print a green summary — "Environment is ready! All prerequisites are installed and configured."
- **If anything needs sudo or interactive input** (gh not installed, auth missing, git identity missing, .NET missing): Tell the user to run `bash scripts/onboard.sh` in their terminal to fix the issues interactively, then re-run `/onboard` to verify.

Present results in a clean summary like:
```
Environment Setup Check
=======================
[OK] .NET SDK: 10.0.xxx
[OK] GitHub CLI: 2.x.x
[OK] GitHub Auth: logged in as user
[OK] Git Identity: Name <email>
[--] Anthropic API Key: Not configured (optional)
```
