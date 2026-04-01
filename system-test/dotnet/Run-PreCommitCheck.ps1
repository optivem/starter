<#
.SYNOPSIS
    Runs pre-commit checks: compilation and optional format verification.
.DESCRIPTION
    Equivalent to Java's gradlew preCommitCheck.
    Runs dotnet restore and dotnet build. SonarAnalyzer runs during build.
.EXAMPLE
    .\Run-PreCommitCheck.ps1
#>

$ErrorActionPreference = "Stop"

Write-Host "Running pre-commit checks..." -ForegroundColor Cyan

dotnet restore
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

dotnet build --no-restore
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "Pre-commit checks passed." -ForegroundColor Green
