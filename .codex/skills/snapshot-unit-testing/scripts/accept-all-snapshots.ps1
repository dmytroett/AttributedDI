#!/usr/bin/env pwsh
<#
.SYNOPSIS
Accepts Verify snapshots for all tests.

.DESCRIPTION
Promotes one TFM's received files per test to verified files and removes the other
received files. When multiple TFMs exist for a test, the highest TFM version is accepted.

.EXAMPLE
./accept-all-snapshots.ps1
#>
[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..\..')).Path
$snapshotsDir = Join-Path $repoRoot 'test/AttributedDI.SourceGenerator.UnitTests/Snapshots'

if (-not (Test-Path -Path $snapshotsDir -PathType Container)) {
    throw "Snapshots directory not found: $snapshotsDir"
}

$receivedFiles = @(Get-ChildItem -Path $snapshotsDir -Filter '*.received.*' -File -ErrorAction SilentlyContinue)

if ($receivedFiles.Count -eq 0) {
    Write-Output "No received files found in $snapshotsDir"
    exit 0
}

$seen = @{}

foreach ($receivedFile in $receivedFiles) {
    $fileName = $receivedFile.Name
    $baseName = ''

    if ($fileName -match '^(?<base>.+)\.DotNet\d+_\d+\.received\.') {
        $baseName = $Matches['base']
    } elseif ($fileName -like '*.received.*') {
        $baseName = $fileName -replace '\.received\..*$', ''
    }

    if (-not $baseName) {
        continue
    }

    if ($seen.ContainsKey($baseName)) {
        continue
    }

    $seen[$baseName] = $true
    $lastDotIndex = $baseName.LastIndexOf('.')
    if ($lastDotIndex -lt 1) {
        continue
    }

    $testClass = $baseName.Substring(0, $lastDotIndex)
    $testMethod = $baseName.Substring($lastDotIndex + 1)

    & (Join-Path $PSScriptRoot 'accept-snapshot.ps1') $testClass $testMethod
}
