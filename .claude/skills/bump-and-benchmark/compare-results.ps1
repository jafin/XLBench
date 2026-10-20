#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Compares the freshly generated docs/results-data.js against the last published copy and
    lists every (scenario, library) pair whose time or allocation moved past a threshold.

.PARAMETER BaselineRef
    Git ref holding the previous publication. Default: origin/main.

.PARAMETER Threshold
    Percent change (either direction) that counts as a significant swing. Default: 10.

.PARAMETER All
    List every row, not just the significant ones.

.NOTES
    A time swing is only flagged when it also exceeds the two runs' combined Error, so noise on
    a wide-error row is not reported as a regression. Rows replayed from snapshots/ (marked
    snapshot) were not measured in one or both runs and are never flagged.

    SIGNIFICANT marks a swing on a library whose version changed. drift marks a swing on a
    library whose version did not, which points at the host (thermals, background load, runtime
    or OS update) rather than the library, and is a reason to distrust same-sized swings
    elsewhere in the run.
#>
param(
    [string]$BaselineRef = 'origin/main',
    [double]$Threshold = 10,
    [switch]$All
)

$ErrorActionPreference = 'Stop'
$repoRoot = git rev-parse --show-toplevel

function Read-ResultsData([string]$text) {
    $json = $text -replace '^\s*window\.XLBENCH_DATA\s*=\s*', '' -replace ';\s*$', ''
    $json | ConvertFrom-Json
}

function Get-Rows($data) {
    foreach ($s in $data.scenarios) {
        for ($i = 0; $i -lt $s.libraries.Count; $i++) {
            $lib = $s.libraries[$i]
            [pscustomobject]@{
                Key      = "$($s.key)|$lib"
                Scenario = $s.key
                Library  = $lib
                Version  = $data.versions.$lib
                Snapshot = [bool]$s.snapshotOf[$i]
                TimeMs   = [double]$s.timeMs[$i]
                ErrorMs  = [double]$s.errorMs[$i]
                AllocMb  = [double]$s.allocMb[$i]
            }
        }
    }
}

function Get-Delta([double]$old, [double]$new) {
    if ($old -eq 0) { return $null }
    [math]::Round(($new - $old) / $old * 100, 1)
}

$baseline = Read-ResultsData ((git -C $repoRoot show "${BaselineRef}:docs/results-data.js") -join "`n")
$current = Read-ResultsData (Get-Content (Join-Path $repoRoot 'docs/results-data.js') -Raw)

Write-Host "Baseline: $BaselineRef (updated $($baseline.updated))"
Write-Host "Current:  working tree (updated $($current.updated))"
Write-Host ''

$old = @{}
Get-Rows $baseline | ForEach-Object { $old[$_.Key] = $_ }

$rows = foreach ($n in Get-Rows $current) {
    $o = $old[$n.Key]
    if (-not $o) { continue }
    $timeDelta = Get-Delta $o.TimeMs $n.TimeMs
    $allocDelta = Get-Delta $o.AllocMb $n.AllocMb
    $snapshot = $o.Snapshot -or $n.Snapshot
    $timeSignificant = $timeDelta -ne $null -and [math]::Abs($timeDelta) -ge $Threshold -and
        [math]::Abs($n.TimeMs - $o.TimeMs) -gt ($n.ErrorMs + $o.ErrorMs)
    $allocSignificant = $allocDelta -ne $null -and [math]::Abs($allocDelta) -ge $Threshold
    [pscustomobject]@{
        Scenario    = $n.Scenario
        Library     = $n.Library
        Versions    = if ($o.Version -eq $n.Version) { $n.Version } else { "$($o.Version) -> $($n.Version)" }
        'Time ms'   = "$($o.TimeMs) -> $($n.TimeMs)"
        'Time %'    = $timeDelta
        'Alloc MB'  = "$($o.AllocMb) -> $($n.AllocMb)"
        'Alloc %'   = $allocDelta
        Flag        = if ($snapshot) { 'snapshot' }
                      elseif (-not ($timeSignificant -or $allocSignificant)) { '' }
                      elseif ($o.Version -ne $n.Version) { 'SIGNIFICANT' }
                      else { 'drift' }
    }
}

$shown = if ($All) { $rows } else { $rows | Where-Object Flag -in 'SIGNIFICANT', 'drift' }
if (-not $shown) {
    Write-Host "No swings of $Threshold% or more."
    return
}
$shown | Format-Table -AutoSize | Out-String -Width 400
