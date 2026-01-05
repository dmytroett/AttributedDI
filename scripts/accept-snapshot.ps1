#!/usr/bin/env pwsh
<#
.SYNOPSIS
Accepts Verify snapshots for a single test.

.DESCRIPTION
Promotes one TFM's received files to verified files and removes the other received
files. When multiple TFMs exist, the highest TFM version is accepted.

.PARAMETER TestClassName
Test class name used in the snapshot file name.

.PARAMETER TestMethodName
Test method name used in the snapshot file name.

.EXAMPLE
./accept-snapshot.ps1 AddAttributedDiTests GeneratesAddAttributedDiForEntryPoint
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true, Position = 0)]
    [string]$TestClassName,

    [Parameter(Mandatory = $true, Position = 1)]
    [string]$TestMethodName
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$snapshotsDir = Join-Path $repoRoot 'test/AttributedDI.SourceGenerator.UnitTests/Snapshots'

if (-not (Test-Path -Path $snapshotsDir -PathType Container)) {
    throw "Snapshots directory not found: $snapshotsDir"
}

$baseName = "$TestClassName.$TestMethodName"

function Get-ReceivedFilesForTfm([string]$tfm) {
    Get-ChildItem -Path $snapshotsDir -Filter "$baseName.$tfm.received.*" -File -ErrorAction SilentlyContinue
}

$escapedBaseName = [Regex]::Escape($baseName)
$tfmRegex = "^$escapedBaseName\.DotNet(?<major>\d+)_(?<minor>\d+)\.received\."

$receivedFiles = @(Get-ChildItem -Path $snapshotsDir -Filter "$baseName.*.received.*" -File -ErrorAction SilentlyContinue)
$tfms = foreach ($file in $receivedFiles) {
    if ($file.Name -match $tfmRegex) {
        "DotNet$($Matches['major'])_$($Matches['minor'])"
    }
}

$selectedFiles = @()
$selectedTfm = ''

if ($tfms) {
    $selectedTfm = $tfms |
        Sort-Object {
            if ($_ -match '^DotNet(?<major>\d+)_(?<minor>\d+)$') {
                [int]$Matches['major'] * 1000 + [int]$Matches['minor']
            } else {
                -1
            }
        } -Descending |
        Select-Object -First 1

    if ($selectedTfm) {
        $selectedFiles = @(Get-ReceivedFilesForTfm $selectedTfm)
    }
}

if ($selectedFiles.Count -eq 0) {
    $selectedFiles = @(Get-ChildItem -Path $snapshotsDir -Filter "$baseName.received.*" -File -ErrorAction SilentlyContinue)
    $selectedTfm = ''
}

if ($selectedFiles.Count -eq 0) {
    throw "No received files found for $baseName in $snapshotsDir"
}

foreach ($receivedFile in $selectedFiles) {
    $fileName = $receivedFile.Name
    if ($selectedTfm) {
        $verifiedName = $fileName.Replace(".$selectedTfm.received.", '.verified.')
    } else {
        $verifiedName = $fileName.Replace('.received.', '.verified.')
    }

    $verifiedPath = Join-Path $snapshotsDir $verifiedName
    Move-Item -Path $receivedFile.FullName -Destination $verifiedPath -Force
    Write-Output "Accepted: $verifiedName"
}

$remainingReceived = @()
$remainingReceived += Get-ChildItem -Path $snapshotsDir -Filter "$baseName.*.received.*" -File -ErrorAction SilentlyContinue
$remainingReceived += Get-ChildItem -Path $snapshotsDir -Filter "$baseName.received.*" -File -ErrorAction SilentlyContinue

foreach ($receivedFile in $remainingReceived) {
    if (Test-Path -Path $receivedFile.FullName) {
        Remove-Item -Path $receivedFile.FullName -Force
        Write-Output "Removed: $($receivedFile.Name)"
    }
}
