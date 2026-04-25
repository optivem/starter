<#
.SYNOPSIS
    Runs SonarScanner analysis on the multitier .NET backend.

.DESCRIPTION
    Local helper that pushes a SonarCloud analysis using your personal token.
    CI runs the same analysis from multitier-backend-dotnet-commit-stage.yml;
    this script is for manual runs.
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

$projectKey = "optivem_shop-multitier-backend-dotnet"
$projectName = "shop-multitier-backend-dotnet"
$solution = "MyCompany.MyShop.Backend.slnx"

Write-Host "Running SonarScanner for multitier .NET backend..." -ForegroundColor Cyan

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
