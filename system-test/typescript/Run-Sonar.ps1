<#
.SYNOPSIS
    Runs SonarScanner analysis on the TypeScript system tests.

.DESCRIPTION
    Local helper that pushes a SonarCloud analysis using your personal token.
    Also runs in multitier-typescript-acceptance-stage.yml after tests finish.
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

Write-Host "Running SonarScanner for TypeScript system tests..." -ForegroundColor Cyan

npx -y sonarqube-scanner `
    "-Dsonar.projectKey=optivem_shop-tests-typescript" `
    "-Dsonar.projectName=shop-tests-typescript" `
    "-Dsonar.organization=optivem" `
    "-Dsonar.host.url=https://sonarcloud.io" `
    "-Dsonar.token=$Token" `
    "-Dsonar.sources=src,tests"

Write-Host "Sonar analysis complete." -ForegroundColor Green
