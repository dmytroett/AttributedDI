#!/usr/bin/env pwsh
param(
  [string]$ResultsDirectory,
  [string]$Logger = "trx",
  [switch]$NoRestoreProps,
  [string]$PackageOutputDirectory = "artifacts/e2e"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Resolve-Path (Join-Path $scriptDir "..\..") | Select-Object -ExpandProperty Path
$artifactsDir = Join-Path $repoRoot $PackageOutputDirectory
$timestamp = Get-Date -Format "yyyyMMddHHmmss"
$packageVersion = "99.0.0-e2e.$timestamp"

New-Item -ItemType Directory -Force -Path $artifactsDir | Out-Null

dotnet pack (Join-Path $repoRoot "src\AttributedDI\AttributedDI.csproj") `
  -c Release `
  -o $artifactsDir `
  -p:PackageVersion=$packageVersion

Write-Host "Packed AttributedDI $packageVersion to $artifactsDir"

$propsFile = Join-Path $repoRoot "test\integration\assets\Directory.Build.props"
$propsText = Get-Content -Raw -Path $propsFile
$updatedText = $propsText -replace "<AttributedDIPackageVersion[^>]*>[^<]*</AttributedDIPackageVersion>", "<AttributedDIPackageVersion Condition=""'`$(IsE2E)' == 'true'"">$packageVersion</AttributedDIPackageVersion>"

if ($updatedText -eq $propsText) {
  throw "Failed to update $propsFile (pattern not found)."
}

try {
  Set-Content -Path $propsFile -Value $updatedText
  Write-Host "Updated $propsFile to use $packageVersion"

  $solutionPath = Join-Path $repoRoot "AttributedDI.slnx"

  if ([string]::IsNullOrWhiteSpace($ResultsDirectory)) {
    dotnet test $solutionPath -p:IsE2E=true
  } else {
    $resultsPath = Join-Path $repoRoot $ResultsDirectory
    New-Item -ItemType Directory -Force -Path $resultsPath | Out-Null

    dotnet test $solutionPath `
      -p:IsE2E=true `
      --logger $Logger `
      --results-directory $resultsPath
  }
}
finally {
  if (-not $NoRestoreProps) {
    Set-Content -Path $propsFile -Value $propsText
    Write-Host "Restored $propsFile"
  }
}
