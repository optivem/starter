<#
.SYNOPSIS
    Installs the pre-commit hook into .git/hooks.
.DESCRIPTION
    Copies scripts/pre-commit to .git/hooks/pre-commit and makes it executable.
    Equivalent to Java's gradlew installPreCommitHook.
.EXAMPLE
    .\Install-PreCommitHook.ps1
#>

$ErrorActionPreference = "Stop"

$repoRoot = Get-Location
$preCommitSource = Join-Path $repoRoot "scripts\pre-commit"
$gitHooksDir = Join-Path $repoRoot ".git\hooks"
$preCommitDest = Join-Path $gitHooksDir "pre-commit"

if (-not (Test-Path ".git")) {
    Write-Error "Not a Git repository. Run from project root."
    exit 1
}

if (-not (Test-Path $preCommitSource)) {
    Write-Error "Pre-commit script not found at $preCommitSource"
    exit 1
}

Copy-Item -Path $preCommitSource -Destination $preCommitDest -Force

# On Windows, Git Bash will handle execution; ensure no CRLF issues
$content = Get-Content $preCommitDest -Raw
$content = $content -replace "`r`n", "`n"
[System.IO.File]::WriteAllText($preCommitDest, $content)

Write-Host "Pre-commit hook installed to .git/hooks/pre-commit" -ForegroundColor Green
Write-Host "Bypass when needed: git commit --no-verify" -ForegroundColor Gray
