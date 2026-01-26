#!/usr/bin/env pwsh
[CmdletBinding(SupportsShouldProcess = $true, ConfirmImpact = 'Medium')]
param(
    [Parameter(Mandatory = $true)]
    [string]$Version
)

$ErrorActionPreference = 'Stop'

$versionValue = $Version.Trim()
if ([string]::IsNullOrWhiteSpace($versionValue)) {
    throw 'Version is required.'
}

$repoRoot = Join-Path $PSScriptRoot '..'
$changelogPath = Join-Path $repoRoot 'CHANGELOG.md'
$unshippedPath = Join-Path $repoRoot 'src' 'AttributedDI.SourceGenerator' 'AnalyzerReleases.Unshipped.md'
$shippedPath = Join-Path $repoRoot 'src' 'AttributedDI.SourceGenerator' 'AnalyzerReleases.Shipped.md'

if (-not (Test-Path $changelogPath)) {
    throw 'CHANGELOG.md not found.'
}
if (-not (Test-Path $unshippedPath)) {
    throw 'AnalyzerReleases.Unshipped.md not found.'
}
if (-not (Test-Path $shippedPath)) {
    throw 'AnalyzerReleases.Shipped.md not found.'
}

$releaseDate = Get-Date -Format 'yyyy-MM-dd'

function Clear-EmptyLines {
    param([string[]]$InputLines)

    if ($InputLines.Count -eq 0) {
        return @()
    }

    $startIndex = 0
    $endIndex = $InputLines.Count - 1

    while ($startIndex -le $endIndex -and [string]::IsNullOrWhiteSpace($InputLines[$startIndex])) {
        $startIndex++
    }

    while ($endIndex -ge $startIndex -and [string]::IsNullOrWhiteSpace($InputLines[$endIndex])) {
        $endIndex--
    }

    if ($startIndex -gt $endIndex) {
        return @()
    }

    return $InputLines[$startIndex..$endIndex]
}

$unshippedTemplate = @(
    '### New Rules',
    '',
    'Rule ID   | Category | Severity | Notes',
    '----------|----------|----------|--------------------'
) -join "`n"

$changelogText = Get-Content -Path $changelogPath -Raw
$unreleasedPattern = '(?ms)^##\s+\[?Unreleased\]?\s*$\s*(?<content>.*?)(?=^##\s+|\z)'
$unreleasedMatch = [regex]::Match($changelogText, $unreleasedPattern)

if (-not $unreleasedMatch.Success) {
    throw 'Could not find the [Unreleased] section in CHANGELOG.md.'
}

$escapedVersion = [regex]::Escape($versionValue)
if ([regex]::IsMatch($changelogText, "(?m)^##\s+\[?$escapedVersion\]?(?:\s+-\s+.*)?$")) {
    throw "Version '$versionValue' already exists in CHANGELOG.md."
}

$unreleasedLines = $unreleasedMatch.Groups['content'].Value -split "`r?`n"
$unreleasedTrimmedLines = Clear-EmptyLines -InputLines $unreleasedLines
if ($unreleasedTrimmedLines.Count -eq 0) {
    throw 'No unreleased entries found under [Unreleased]. An empty section means no pending changes.'
}

$unreleasedTrimmed = $unreleasedTrimmedLines -join "`n"
$unreleasedReplacement = @(
    '## [Unreleased]',
    '',
    "## [$versionValue] - $releaseDate",
    '',
    $unreleasedTrimmed
) -join "`n"

$newChangelogText = [regex]::Replace($changelogText, $unreleasedPattern, $unreleasedReplacement, 1)
$newChangelogText = $newChangelogText.TrimEnd() + "`n"

if ($PSCmdlet.ShouldProcess($changelogPath, "Update changelog for version $versionValue")) {
    Set-Content -Path $changelogPath -Value $newChangelogText -Encoding utf8
    Write-Host "Updated CHANGELOG.md with version $versionValue."
}

$unshippedText = Get-Content -Path $unshippedPath -Raw
$unshippedTrimmed = $unshippedText.Trim()

if (-not [string]::IsNullOrWhiteSpace($unshippedTrimmed)) {
    $shippedText = Get-Content -Path $shippedPath -Raw
    $shippedTrimmed = $shippedText.Trim()

    $releaseHeader = "## $versionValue - $releaseDate"

    if ([string]::IsNullOrWhiteSpace($shippedTrimmed)) {
        $newShippedText = "$releaseHeader`n`n$unshippedTrimmed`n"
    }
    else {
        $newShippedText = "$shippedTrimmed`n`n$releaseHeader`n`n$unshippedTrimmed`n"
    }

    if ($PSCmdlet.ShouldProcess($shippedPath, "Append analyzer releases for version $versionValue")) {
        Set-Content -Path $shippedPath -Value $newShippedText -Encoding utf8
        Write-Host "Moved analyzer releases to AnalyzerReleases.Shipped.md."
    }

    if ($PSCmdlet.ShouldProcess($unshippedPath, 'Reset unshipped analyzer releases')) {
        Set-Content -Path $unshippedPath -Value $unshippedTemplate -Encoding utf8
        Write-Host 'Reset AnalyzerReleases.Unshipped.md.'
    }
}
else {
    Write-Host 'AnalyzerReleases.Unshipped.md is empty; skipping analyzer release move.'
}
