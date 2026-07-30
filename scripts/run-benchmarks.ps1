#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Runs the XLBench suite in Release and publishes the markdown results into docs/.

.PARAMETER Filter
    BenchmarkDotNet glob(s) selecting which benchmarks to run. Default: everything.
    Examples: '*Read*', '*Write*', '*Report*', '*ClosedXml*'.

.NOTES
    The report benchmarks also write output/stock-report-<library>.xlsx for manual review
    (git-ignored, overwritten each run). To refresh just those without measuring, run:
        dotnet run -c Release --project src/XLBench -- report

    IronXL is commercial and is skipped unless $env:XLBENCH_IRONXL_KEY holds a licence key.
    A run that has one refreshes snapshots/ironxl.json; runs without one replay that snapshot
    into the results, marked with the carried-over symbol. Commit the refreshed snapshot.

.PARAMETER Job
    Optional BenchmarkDotNet job to trade fidelity for speed, e.g. 'short' or 'dry'.
    Omit for the full default job (most trustworthy numbers).

.EXAMPLE
    ./scripts/run-benchmarks.ps1
    ./scripts/run-benchmarks.ps1 -Filter '*Write*'
    ./scripts/run-benchmarks.ps1 -Filter '*Read*' -Job short
#>
param(
    [string]$Filter = '*',
    [string]$Job = ''
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot

Push-Location $repoRoot
try {
    $runArgs = @('--filter', $Filter)
    if ($Job) { $runArgs += @('--job', $Job) }

    Write-Host "Running: dotnet run -c Release --project src/XLBench -- $($runArgs -join ' ')" -ForegroundColor Cyan
    dotnet run -c Release --project src/XLBench -- @runArgs

    Write-Host ''
    Write-Host 'Done. Results written to docs/results.md.' -ForegroundColor Green
    Write-Host 'Commit and push docs/ to update GitHub Pages.' -ForegroundColor Green
    if ($env:XLBENCH_IRONXL_KEY) {
        Write-Host 'IronXL ran: commit the refreshed snapshots/ironxl.json too.' -ForegroundColor Green
    }
}
finally {
    Pop-Location
}
