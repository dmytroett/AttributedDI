#!/usr/bin/env pwsh
Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Resolve-Path (Join-Path $scriptDir "..\..") | Select-Object -ExpandProperty Path
$artifactsDir = Join-Path $repoRoot "artifacts"
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

Set-Content -Path $propsFile -Value $updatedText
Write-Host "Updated $propsFile to use $packageVersion"

dotnet test (Join-Path $repoRoot "AttributedDI.slnx") -p:IsE2E=true
