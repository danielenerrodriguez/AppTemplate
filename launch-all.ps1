# launch-all.ps1 - Start both API and Web projects in parallel
# Usage: .\launch-all.ps1

Write-Host "Starting AppTemplate..." -ForegroundColor Cyan
Write-Host "  API: http://localhost:5050" -ForegroundColor Green
Write-Host "  Web: http://localhost:8080" -ForegroundColor Green
Write-Host "  Press Ctrl+C to stop both" -ForegroundColor Yellow
Write-Host ""

# Auto-detect Anthropic API key from ~/.claude.json
if (-not $env:ANTHROPIC_API_KEY) {
    $claudeJson = Join-Path $HOME ".claude.json"
    if (Test-Path $claudeJson) {
        try {
            $config = Get-Content $claudeJson -Raw | ConvertFrom-Json
            if ($config.primaryApiKey) {
                $env:ANTHROPIC_API_KEY = $config.primaryApiKey
                Write-Host "  Anthropic API key detected -- AI chat is ready" -ForegroundColor Green
                Write-Host ""
            }
        } catch {
            # Silently continue -- manual key entry via chat bubble is the fallback
        }
    }
}

# Build first
Write-Host "Building..." -ForegroundColor Yellow
dotnet build --nologo --verbosity quiet
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed. Fix errors before launching." -ForegroundColor Red
    exit 1
}
Write-Host "Build succeeded." -ForegroundColor Green
Write-Host ""

$apiJob = Start-Job -ScriptBlock {
    Set-Location $using:PWD
    $env:ANTHROPIC_API_KEY = $using:env:ANTHROPIC_API_KEY
    dotnet run --project src/AppTemplate.Api --no-build 2>&1
}

$webJob = Start-Job -ScriptBlock {
    Set-Location $using:PWD
    $env:ANTHROPIC_API_KEY = $using:env:ANTHROPIC_API_KEY
    dotnet run --project src/AppTemplate.Web --no-build 2>&1
}

try {
    while ($true) {
        # Stream output from both jobs
        Receive-Job -Job $apiJob -ErrorAction SilentlyContinue | ForEach-Object { Write-Host "[API] $_" -ForegroundColor Blue }
        Receive-Job -Job $webJob -ErrorAction SilentlyContinue | ForEach-Object { Write-Host "[WEB] $_" -ForegroundColor Magenta }
        Start-Sleep -Milliseconds 500
    }
}
finally {
    Write-Host "`nStopping..." -ForegroundColor Yellow
    Stop-Job -Job $apiJob, $webJob -ErrorAction SilentlyContinue
    Remove-Job -Job $apiJob, $webJob -Force -ErrorAction SilentlyContinue
    Write-Host "Stopped." -ForegroundColor Red
}
