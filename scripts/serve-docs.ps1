#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Serves the docs/ GitHub Pages site locally with Jekyll, so you can preview it before pushing.

.DESCRIPTION
    Runs `jekyll serve` inside a Ruby Docker container, so Ruby does not need to be installed.
    The github-pages gem (see scripts/jekyll/Gemfile) gives the same Jekyll, theme and plugin
    versions that GitHub Pages uses.

    docs/ is mounted into the container. The built site stays inside the
    container, so nothing is written to docs/. Installed gems are kept in a named Docker volume,
    so only the first run is slow.

    Edits to files in docs/ trigger a rebuild, and the browser reloads the page automatically.
    Changes to docs/_config.yml need a restart. Press Ctrl+C to stop.

    Needs Docker Desktop to be running.

.PARAMETER Port
    Local port for the site. Default: 4000.

.PARAMETER NoOpen
    Do not open the site in the browser.

.PARAMETER Reinstall
    Delete the cached gems volume first, which forces a fresh `bundle install`.

.EXAMPLE
    ./scripts/serve-docs.ps1
    ./scripts/serve-docs.ps1 -Port 4001 -NoOpen
#>
[CmdletBinding()]
param(
    [int]$Port = 4000,
    [switch]$NoOpen,
    [switch]$Reinstall
)

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$docsDir = Join-Path $repoRoot 'docs'
$gemfileDir = Join-Path $PSScriptRoot 'jekyll'
$gemsVolume = 'xlbench-jekyll-gems'
$image = 'ruby:3.3'
$liveReloadPort = 35729

if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    throw 'Docker was not found. Install Docker Desktop: https://www.docker.com/products/docker-desktop/'
}
docker info *> $null
if ($LASTEXITCODE -ne 0) {
    throw 'Docker is installed but not running. Start Docker Desktop and try again.'
}

if ($Reinstall) {
    Write-Host "Removing gems volume '$gemsVolume'..."
    docker volume rm $gemsVolume *> $null
}

$url = "http://localhost:$Port/"
Write-Host "Serving $docsDir at $url (Ctrl+C to stop)"
Write-Host 'The first run installs gems and takes a few minutes.'

if (-not $NoOpen) {
    # Open the browser once the server answers, without blocking the container output.
    Start-Job -ArgumentList $url -ScriptBlock {
        param($url)
        for ($i = 0; $i -lt 600; $i++) {
            try {
                Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 2 | Out-Null
                Start-Process $url
                return
            }
            catch { Start-Sleep -Seconds 1 }
        }
    } | Out-Null
}

# --force_polling: file-change events do not cross a Windows bind mount, so poll instead.
# --baseurl "": serve at the root locally, whatever the Pages project path is.
# The Gemfile is copied out of the read-only mount so Bundler can write Gemfile.lock.
# Jekyll 3 only loads the github-pages plugins (default layout, relative links, ...) when a
# Gemfile is in the working directory, so it runs from /tmp/jekyll with /site as the source.
$jekyll = 'cp /gemfile/Gemfile /tmp/jekyll/ && ' +
    'bundle install --quiet && bundle exec jekyll serve --source /site ' +
    "--host 0.0.0.0 --port $Port --baseurl '' " +
    "--livereload --livereload-port $liveReloadPort --force_polling " +
    '--destination /tmp/_site'

# The theme layout needs the GitHub owner/repo, which it normally reads from the git remote.
# The container has no .git, so read it here and pass it in.
$remote = git -C $repoRoot remote get-url origin 2>$null
$repoNwo = if ($remote -match 'github\.com[:/](?<nwo>[^/]+/[^/]+?)(\.git)?$') { $Matches.nwo } else { 'jafin/XLBench' }

# -it lets Ctrl+C stop the container, but docker refuses it when stdin is not a terminal.
$ttyArgs = if ([Console]::IsInputRedirected) { @() } else { @('-it') }

docker run --rm @ttyArgs `
    --name xlbench-docs `
    -p "${Port}:${Port}" `
    -p "${liveReloadPort}:${liveReloadPort}" `
    -v "${docsDir}:/site" `
    -v "${gemfileDir}:/gemfile:ro" `
    -v "${gemsVolume}:/usr/local/bundle" `
    -e BUNDLE_APP_CONFIG=/usr/local/bundle `
    -e PAGES_REPO_NWO=$repoNwo `
    -w /tmp/jekyll `
    $image `
    bash -c $jekyll
