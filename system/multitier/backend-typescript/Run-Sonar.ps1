<#
.SYNOPSIS
    Runs SonarScanner analysis on the multitier TypeScript backend.

.DESCRIPTION
    Local helper that pushes a SonarCloud analysis using your personal token.
    CI runs the same analysis from multitier-backend-typescript-commit-stage.yml;
    this script is for manual runs.
    Ignore rules in sonar-project.properties (auto-loaded from this dir).
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

Write-Host "Running SonarScanner for multitier TypeScript backend..." -ForegroundColor Cyan

npx -y sonarqube-scanner `
    "-Dsonar.projectKey=optivem_shop-multitier-backend-typescript" `
    "-Dsonar.projectName=shop-multitier-backend-typescript" `
    "-Dsonar.organization=optivem" `
    "-Dsonar.host.url=https://sonarcloud.io" `
    "-Dsonar.token=$Token" `
    "-Dsonar.sources=src"

Write-Host "Sonar analysis complete." -ForegroundColor Green
