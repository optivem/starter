<#
.SYNOPSIS
    Runs SonarScanner analysis on the multitier Java backend.

.DESCRIPTION
    Local helper that pushes a SonarCloud analysis using your personal token.
    CI runs the same analysis from multitier-backend-java-commit-stage.yml;
    this script is for manual runs.
    Project key: optivem_shop-multitier-backend-java (config in build.gradle).
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

Write-Host "Running SonarScanner for multitier Java backend..." -ForegroundColor Cyan

& ./gradlew build sonar --info "-Dsonar.token=$Token"

Write-Host "Sonar analysis complete." -ForegroundColor Green
