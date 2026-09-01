#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Checks nuget.org for newer versions of the packages referenced by src/XLBench, and
    optionally applies the bumps.

.DESCRIPTION
    Reads every PackageReference out of XLBench.csproj, asks the nuget.org flat container
    for that package's version list, and reports the newest version each one could move to.
    Nothing is written unless -Apply is passed.

    Packages are grouped into the Excel libraries under test (the ones whose numbers appear
    in the published results) and tooling/pins (BenchmarkDotNet, security overrides).

.PARAMETER Apply
    Actually bump the outdated packages, via `dotnet add package` (never by editing the XML).
    Without this the script only reports.

.PARAMETER LibrariesOnly
    Restrict both the report and -Apply to the Excel libraries under test.

.PARAMETER Package
    One or more package IDs to limit the check to. Matched case-insensitively.

.PARAMETER IncludePrerelease
    Consider prerelease versions for every package. Prereleases are considered anyway for a
    package that is already pinned to one (e.g. an XLibur beta), since staying on the stable
    channel there would be a downgrade in intent.

.NOTES
    A bump changes what the published results describe, so re-run the suite afterwards:
        ./scripts/run-benchmarks.ps1

.EXAMPLE
    ./scripts/update-libraries.ps1
    ./scripts/update-libraries.ps1 -Apply -LibrariesOnly
    ./scripts/update-libraries.ps1 -Package XLibur.Bundle -Apply
    ./scripts/update-libraries.ps1 -IncludePrerelease
#>
param(
    [switch]$Apply,
    [switch]$LibrariesOnly,
    [string[]]$Package,
    [switch]$IncludePrerelease
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$csprojPath = Join-Path $repoRoot 'src/XLBench/XLBench.csproj'

# The libraries whose numbers are published. Everything else in the csproj is tooling.
$librariesUnderTest = @(
    'ClosedXML'
    'EPPlus'
    'DocumentFormat.OpenXml'
    'NPOI'
    'MiniExcel'
    'XLibur.Bundle'
    'IronXL.Excel'
    # Two packages, one library: the RadSpreadProcessing model and its .xlsx format provider.
    # They are versioned in lockstep and must be bumped together.
    'Telerik.Documents.Spreadsheet'
    'Telerik.Documents.Spreadsheet.FormatProviders.OpenXml'
)

# --- NuGet version comparison -------------------------------------------------------------
# SemVer 2.0 ordering, which is what NuGet uses: numeric release parts first, then a release
# with no prerelease tag outranks one that has it, then the dot-separated prerelease
# identifiers (numeric ones compare numerically and sort below alphanumeric ones).

function Split-NuGetVersion([string]$version) {
    $plus = $version.IndexOf('+')
    if ($plus -ge 0) { $version = $version.Substring(0, $plus) }   # drop build metadata

    $dash = $version.IndexOf('-')
    if ($dash -ge 0) {
        $core = $version.Substring(0, $dash)
        $pre = $version.Substring($dash + 1)
    }
    else {
        $core = $version
        $pre = ''
    }

    $release = @(0, 0, 0, 0)
    $parts = $core -split '\.'
    for ($i = 0; $i -lt [Math]::Min($parts.Count, 4); $i++) {
        $n = 0
        if ([int]::TryParse($parts[$i], [ref]$n)) { $release[$i] = $n }
    }

    [pscustomobject]@{
        Release      = $release
        Prerelease   = $pre
        IsPrerelease = -not [string]::IsNullOrEmpty($pre)
    }
}

function Compare-NuGetVersion([string]$left, [string]$right) {
    $a = Split-NuGetVersion $left
    $b = Split-NuGetVersion $right

    for ($i = 0; $i -lt 4; $i++) {
        if ($a.Release[$i] -ne $b.Release[$i]) {
            return [Math]::Sign($a.Release[$i] - $b.Release[$i])
        }
    }

    if (-not $a.IsPrerelease -and -not $b.IsPrerelease) { return 0 }
    if (-not $a.IsPrerelease) { return 1 }
    if (-not $b.IsPrerelease) { return -1 }

    $aIds = $a.Prerelease -split '\.'
    $bIds = $b.Prerelease -split '\.'
    for ($i = 0; $i -lt [Math]::Max($aIds.Count, $bIds.Count); $i++) {
        if ($i -ge $aIds.Count) { return -1 }   # shorter set of identifiers sorts lower
        if ($i -ge $bIds.Count) { return 1 }

        $an = 0; $bn = 0
        $aNumeric = [int]::TryParse($aIds[$i], [ref]$an)
        $bNumeric = [int]::TryParse($bIds[$i], [ref]$bn)

        if ($aNumeric -and $bNumeric) {
            if ($an -ne $bn) { return [Math]::Sign($an - $bn) }
        }
        elseif ($aNumeric) { return -1 }        # numeric identifiers sort below alphanumeric
        elseif ($bNumeric) { return 1 }
        else {
            $cmp = [string]::Compare($aIds[$i], $bIds[$i], [StringComparison]::OrdinalIgnoreCase)
            if ($cmp -ne 0) { return [Math]::Sign($cmp) }
        }
    }

    return 0
}

function Get-NuGetVersions([string]$id) {
    $url = "https://api.nuget.org/v3-flatcontainer/$($id.ToLowerInvariant())/index.json"
    try {
        return (Invoke-RestMethod -Uri $url -TimeoutSec 60).versions
    }
    catch {
        Write-Warning "Could not query nuget.org for $id : $($_.Exception.Message)"
        return @()
    }
}

# --- Read the current references ----------------------------------------------------------

[xml]$csproj = Get-Content -Raw -LiteralPath $csprojPath
$references = @($csproj.Project.ItemGroup.PackageReference |
    Where-Object { $_.Include } |
    ForEach-Object { [pscustomobject]@{ Id = $_.Include; Version = $_.Version } })

if (-not $references) { throw "No PackageReference entries found in $csprojPath." }

if ($LibrariesOnly) {
    $references = @($references | Where-Object { $librariesUnderTest -contains $_.Id })
}
if ($Package) {
    $references = @($references | Where-Object { $Package -contains $_.Id })
    if (-not $references) { throw "None of the requested packages are referenced: $($Package -join ', ')" }
}

# --- Check each one against nuget.org -----------------------------------------------------

Write-Host "Checking $($references.Count) package(s) against nuget.org..." -ForegroundColor Cyan

$results = foreach ($reference in $references) {
    $versions = Get-NuGetVersions $reference.Id
    $isLibrary = $librariesUnderTest -contains $reference.Id

    # Already on a prerelease? Then prereleases stay in scope for this package.
    $allowPrerelease = $IncludePrerelease -or (Split-NuGetVersion $reference.Version).IsPrerelease
    $candidates = @($versions | Where-Object {
        $allowPrerelease -or -not (Split-NuGetVersion $_).IsPrerelease
    })

    $latest = $reference.Version
    foreach ($candidate in $candidates) {
        if ((Compare-NuGetVersion $candidate $latest) -gt 0) { $latest = $candidate }
    }

    $status = if (-not $versions) { 'unknown' }
    elseif ($latest -eq $reference.Version) { 'current' }
    else { 'outdated' }

    [pscustomobject]@{
        Group     = if ($isLibrary) { 'library' } else { 'tooling' }
        Id        = $reference.Id
        Current   = $reference.Version
        Latest    = $latest
        Status    = $status
        IsLibrary = $isLibrary
    }
}

foreach ($group in @('library', 'tooling')) {
    $rows = @($results | Where-Object Group -eq $group)
    if (-not $rows) { continue }

    $heading = if ($group -eq 'library') { 'Excel libraries under test' } else { 'Tooling / pins' }
    Write-Host ''
    Write-Host $heading -ForegroundColor Cyan
    $rows | Format-Table Id, Current, Latest, Status -AutoSize | Out-String | Write-Host
}

$outdated = @($results | Where-Object Status -eq 'outdated')
if (-not $outdated) {
    Write-Host 'Everything is up to date.' -ForegroundColor Green
    return
}

if (-not $Apply) {
    Write-Host "$($outdated.Count) package(s) can be bumped. Re-run with -Apply to fetch them." -ForegroundColor Yellow
    return
}

# --- Apply --------------------------------------------------------------------------------

# The version list above came from a direct flat container request, but `dotnet add package`
# resolves through NuGet's HTTP cache, which can still be serving an index that predates a
# freshly published version. When that happens the restore fails with NU1102 and then, having
# no package to inspect, reports the misleading "incompatible with 'all' frameworks". Clearing
# the cache once here keeps the apply path reading the same nuget.org the report path did.
Write-Host 'Clearing the NuGet HTTP cache so the restore sees the same versions this report did.' -ForegroundColor DarkGray
dotnet nuget locals http-cache --clear | Out-Null
if ($LASTEXITCODE -ne 0) { throw 'Failed to clear the NuGet HTTP cache.' }

Push-Location $repoRoot
try {
    foreach ($row in $outdated) {
        Write-Host "Updating $($row.Id): $($row.Current) -> $($row.Latest)" -ForegroundColor Cyan
        dotnet add $csprojPath package $row.Id --version $row.Latest
        if ($LASTEXITCODE -ne 0) { throw "dotnet add package failed for $($row.Id)." }
    }

    Write-Host ''
    Write-Host "Updated $($outdated.Count) package(s)." -ForegroundColor Green
    if ($outdated | Where-Object IsLibrary) {
        Write-Host 'Libraries under test changed — re-run ./scripts/run-benchmarks.ps1 before publishing.' -ForegroundColor Green
    }
}
finally {
    Pop-Location
}
