param(
    [string[]]$Languages = @("dotnet", "java", "typescript"),

    [ValidateSet("local", "pipeline")]
    [string]$Mode = "local",

    [ValidateSet("multitier", "monolith")]
    [string]$Architecture,

    [switch]$Rebuild
)

$ErrorActionPreference = "Continue"
$RepoRoot = $PSScriptRoot
$SystemTestRoot = Join-Path $RepoRoot "system-test"

function Write-Heading {
    param(
        [string]$Text,
        [string]$Color = "Cyan"
    )
    Write-Host ""
    Write-Host "================================================" -ForegroundColor $Color
    Write-Host $Text -ForegroundColor $Color
    Write-Host "================================================" -ForegroundColor $Color
    Write-Host ""
}

function Invoke-PhaseInParallel {
    param(
        [string]$Phase,
        [string[]]$Languages,
        [hashtable]$ScriptArgs
    )

    Write-Heading -Text "PHASE: $Phase (parallel: $($Languages -join ', '))"

    $jobs = @()
    foreach ($lang in $Languages) {
        $langDir = Join-Path $SystemTestRoot $lang
        $script  = Join-Path $langDir "Run-SystemTests.ps1"

        if (-not (Test-Path $script)) {
            Write-Host "[$lang] Script not found: $script" -ForegroundColor Red
            continue
        }

        $exitCodeFile = Join-Path ([System.IO.Path]::GetTempPath()) "shop-systemtest-$Phase-$lang-$PID.exit"
        if (Test-Path $exitCodeFile) { Remove-Item $exitCodeFile -Force }

        $job = Start-Job -Name "$Phase/$lang" -ScriptBlock {
            param($LangDir, $ScriptArgs, $ExitCodeFile)
            Set-Location $LangDir
            & ".\Run-SystemTests.ps1" @ScriptArgs *>&1
            $LASTEXITCODE | Out-File -FilePath $ExitCodeFile -Encoding ascii
        } -ArgumentList $langDir, $ScriptArgs, $exitCodeFile

        $jobs += [pscustomobject]@{
            Job          = $job
            Language     = $lang
            Phase        = $Phase
            ExitCodeFile = $exitCodeFile
        }
    }

    # Stream output as it arrives, prefixed with [lang]
    while ($jobs.Job | Where-Object { $_.State -eq 'Running' -or $_.HasMoreData }) {
        foreach ($entry in $jobs) {
            $data = Receive-Job -Job $entry.Job -ErrorAction SilentlyContinue
            if ($data) {
                foreach ($line in $data) {
                    Write-Host "[$($entry.Language)] $line"
                }
            }
        }
        Start-Sleep -Milliseconds 300
    }

    # Build results
    $results = @()
    foreach ($entry in $jobs) {
        $exitCode = $null
        if (Test-Path $entry.ExitCodeFile) {
            $raw = (Get-Content $entry.ExitCodeFile -Raw).Trim()
            if ($raw -ne '') { $exitCode = [int]$raw }
            Remove-Item $entry.ExitCodeFile -Force -ErrorAction SilentlyContinue
        }

        $status = if ($exitCode -eq 0 -or $null -eq $exitCode) { "PASSED" } else { "FAILED" }

        $results += [pscustomobject]@{
            Phase    = $entry.Phase
            Language = $entry.Language
            Status   = $status
            ExitCode = $exitCode
        }

        Remove-Job -Job $entry.Job -Force -ErrorAction SilentlyContinue
    }

    return $results
}

$baseArgs = @{ Mode = $Mode }
if ($Architecture) { $baseArgs.Architecture = $Architecture }

$allResults = @()
$overallStart = Get-Date

if ($Rebuild) {
    $rebuildArgs = $baseArgs.Clone()
    $rebuildArgs.Rebuild   = $true
    $rebuildArgs.SkipTests = $true
    $allResults += Invoke-PhaseInParallel -Phase "Rebuild" -Languages $Languages -ScriptArgs $rebuildArgs
}

$allResults += Invoke-PhaseInParallel -Phase "Latest" -Languages $Languages -ScriptArgs $baseArgs

$legacyArgs = $baseArgs.Clone()
$legacyArgs.Legacy = $true
$allResults += Invoke-PhaseInParallel -Phase "Legacy" -Languages $Languages -ScriptArgs $legacyArgs

$overallDuration = (Get-Date) - $overallStart

# Summary
Write-Heading -Text "SUMMARY" -Color Cyan
Write-Host ("{0,-10} {1,-12} {2,-8} {3}" -f "Phase", "Language", "Status", "Exit") -ForegroundColor Cyan
Write-Host ("-" * 50) -ForegroundColor Cyan
foreach ($r in $allResults) {
    $color = if ($r.Status -eq "PASSED") { "Green" } else { "Red" }
    Write-Host ("{0,-10} {1,-12} {2,-8} {3}" -f $r.Phase, $r.Language, $r.Status, $r.ExitCode) -ForegroundColor $color
}
Write-Host ("-" * 50) -ForegroundColor Cyan
Write-Host ("Total duration: {0}" -f $overallDuration.ToString('hh\:mm\:ss')) -ForegroundColor Cyan

$failed = $allResults | Where-Object { $_.Status -eq "FAILED" }
if ($failed) {
    Write-Host ""
    Write-Host "$($failed.Count) run(s) FAILED" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "All runs PASSED" -ForegroundColor Green
exit 0
