#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Runs the XLBench suite in Release and publishes the markdown results into docs/.

.PARAMETER Filter
    BenchmarkDotNet glob(s) selecting which benchmarks to run. Default: everything.
    Examples: '*Read*', '*Write*', '*Report*', '*Edit*', '*ClosedXml*'.

.PARAMETER Job
    Optional BenchmarkDotNet job preset to trade fidelity for speed, e.g. 'short' or 'dry'.
    A preset pins the warmup/iteration counts outright, so it takes precedence over the
    -*Count parameters below. Omit it to use the tuned counts.

.PARAMETER MinWarmupCount
.PARAMETER MaxWarmupCount
.PARAMETER MinIterationCount
.PARAMETER MaxIterationCount
    Bounds on BenchmarkDotNet's two adaptive stages. Both stages still stop early once the
    measurements settle — these only move the floor and ceiling, so a stable benchmark
    finishes at the minimum and only a noisy one runs to the maximum.

    The defaults here (warmup 1-3, iterations 5-10) are deliberately below BenchmarkDotNet's
    own (warmup 6-50, iterations 15-100), which cuts a full run roughly in half. Every
    scenario in this suite runs 25 ms to 20 s per operation, so the default floors cost
    minutes per benchmark for accuracy this comparison does not need: the libraries differ
    by 2-10x, far outside any plausible confidence interval.

    The trade is a wider Error column - measured at 2-3x wider on the edit scenario, with
    means within ~5% of a full-fidelity run and the ranking unchanged. That is fine for
    comparing libraries and NOT fine for detecting a few-percent regression between versions
    of one library. Use -FullFidelity for that.

    Note that BenchmarkDotNet validates Max > Min and fails the run otherwise, so lowering a
    maximum below the stock minimum (6 / 15) means lowering the minimum too.

.PARAMETER FullFidelity
    Drops all four count parameters and runs BenchmarkDotNet's stock defaults - the slowest
    and most trustworthy setting. Use this for the numbers you publish after a library bump.

.NOTES
    The report and edit benchmarks also write output/*.xlsx for manual review (git-ignored,
    overwritten each run). To refresh just those without measuring, run:
        dotnet run -c Release --project src/XLBench -- report
        dotnet run -c Release --project src/XLBench -- edit

    IronXL is commercial and is skipped unless $env:XLBENCH_IRONXL_KEY holds a licence key.
    A run that has one refreshes snapshots/ironxl.json; runs without one replay that snapshot
    into the results, marked with the carried-over symbol. Commit the refreshed snapshot.

.EXAMPLE
    ./scripts/run-benchmarks.ps1
    ./scripts/run-benchmarks.ps1 -Filter '*Write*'
    ./scripts/run-benchmarks.ps1 -FullFidelity
    ./scripts/run-benchmarks.ps1 -Filter '*Read*' -Job short
    ./scripts/run-benchmarks.ps1 -MinIterationCount 10 -MaxIterationCount 20
#>
param(
    [string]$Filter = '*',
    [string]$Job = '',
    [int]$MinWarmupCount = 3,
    [int]$MaxWarmupCount = 10,
    [int]$MinIterationCount = 5,
    [int]$MaxIterationCount = 15,
    [switch]$FullFidelity
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot

Push-Location $repoRoot
try {
    $runArgs = @('--filter', $Filter)
    if ($Job) { $runArgs += @('--job', $Job) }

    if (-not $FullFidelity) {
        # Fail fast with a readable message rather than letting BenchmarkDotNet reject the
        # run after it has already spun up (its own error names the characteristic, not the
        # parameter you passed).
        if ($MaxWarmupCount -le $MinWarmupCount) {
            throw "MaxWarmupCount ($MaxWarmupCount) must be greater than MinWarmupCount ($MinWarmupCount)."
        }
        if ($MaxIterationCount -le $MinIterationCount) {
            throw "MaxIterationCount ($MaxIterationCount) must be greater than MinIterationCount ($MinIterationCount)."
        }

        $runArgs += @(
            '--minWarmupCount', $MinWarmupCount,
            '--maxWarmupCount', $MaxWarmupCount,
            '--minIterationCount', $MinIterationCount,
            '--maxIterationCount', $MaxIterationCount)
    }

    Write-Host "Running: dotnet run -c Release --project src/XLBench -- $($runArgs -join ' ')" -ForegroundColor Cyan
    if (-not $FullFidelity -and -not $Job) {
        Write-Host 'Reduced warmup/iteration counts are in effect; pass -FullFidelity for publication numbers.' -ForegroundColor DarkYellow
    }
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
