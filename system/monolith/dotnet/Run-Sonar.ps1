<#
.SYNOPSIS
    Runs SonarScanner analysis on the monolith .NET app.

.DESCRIPTION
    Local helper that pushes a SonarCloud analysis using your personal token.
    CI runs the same analysis from monolith-dotnet-commit-stage.yml; this
    script is for manual runs.
    Get token: https://sonarcloud.io/account/security

.PARAMETER Token
    SonarCloud token. Or set $env:SONAR_TOKEN.

.EXAMPLE
    .\Run-Sonar.ps1
    .\Run-Sonar.ps1 -Token "your-sonarcloud-token"
#>

param(
    [string]$Token = $env:SONAR_TOKEN
)

$ErrorActionPreference = "Stop"

if (-not $Token) {
    Write-Host "ERROR: Sonar token required. Set SONAR_TOKEN env var or pass -Token" -ForegroundColor Red
    Write-Host "Get token: https://sonarcloud.io/account/security" -ForegroundColor Yellow
    exit 1
}

$projectKey = "optivem_shop-monolith-dotnet"
$projectName = "shop-monolith-dotnet"
$solution = "MyCompany.MyShop.Monolith.sln"

Write-Host "Running SonarScanner for monolith .NET..." -ForegroundColor Cyan

dotnet tool install --global dotnet-sonarscanner 2>$null

dotnet sonarscanner begin `
    /k:$projectKey `
    /n:$projectName `
    /o:"optivem" `
    /d:sonar.host.url="https://sonarcloud.io" `
    /d:sonar.token=$Token

dotnet build $solution --no-incremental

dotnet sonarscanner end /d:sonar.token=$Token

Write-Host "Sonar analysis complete." -ForegroundColor Green
