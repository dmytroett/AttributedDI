param(
    [Parameter(Mandatory = $true)]
    [string]$Version,
    [Parameter(Mandatory = $true)]
    [string]$OutputPath
)

$ErrorActionPreference = 'Stop'

$changelogPath = Join-Path $PSScriptRoot '..' 'CHANGELOG.md'
if (-not (Test-Path $changelogPath)) {
    Write-Error "CHANGELOG.md not found."
}

$versionValue = $Version.Trim()
if ([string]::IsNullOrWhiteSpace($versionValue)) {
    Write-Error "Version is required."
}

$escapedVersion = [regex]::Escape($versionValue)
$headerPattern = "^##\s+\[?$escapedVersion\]?(?:\s+-\s+.*)?$"

$lines = Get-Content -Path $changelogPath
$startIndex = $null

for ($i = 0; $i -lt $lines.Count; $i++) {
    if ($lines[$i].Trim() -match $headerPattern) {
        $startIndex = $i + 1
        break
    }
}

if ($null -eq $startIndex) {
    Write-Error "Version '$versionValue' not found in CHANGELOG.md."
}

$endIndex = $lines.Count
for ($i = $startIndex; $i -lt $lines.Count; $i++) {
    if ($lines[$i].StartsWith('## ')) {
        $endIndex = $i
        break
    }
}

$section = @()
for ($i = $startIndex; $i -lt $endIndex; $i++) {
    $section += $lines[$i]
}

while ($section.Count -gt 0 -and [string]::IsNullOrWhiteSpace($section[0])) {
    $section = $section[1..($section.Count - 1)]
}
while ($section.Count -gt 0 -and [string]::IsNullOrWhiteSpace($section[-1])) {
    $section = $section[0..($section.Count - 2)]
}

if ($section.Count -eq 0) {
    Write-Error "No release notes found under version '$versionValue'."
}

$outputFullPath = Resolve-Path -Path (Join-Path $PWD $OutputPath) -ErrorAction SilentlyContinue
if (-not $outputFullPath) {
    $outputFullPath = Join-Path $PWD $OutputPath
}

$outputDir = Split-Path -Path $outputFullPath -Parent
if (-not (Test-Path $outputDir)) {
    New-Item -Path $outputDir -ItemType Directory -Force | Out-Null
}

$content = ($section -join "`n").TrimEnd() + "`n"
Set-Content -Path $outputFullPath -Value $content -Encoding utf8
