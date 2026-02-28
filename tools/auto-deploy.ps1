param(
    [Parameter(Mandatory = $true)]
    [string]$SourcePath,
    [Parameter(Mandatory = $true)]
    [string]$DestinationDir,
    [string]$ProjectName = "Unknown"
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path -LiteralPath $SourcePath)) {
    throw "[AutoDeploy][$ProjectName] Source file not found: $SourcePath"
}

New-Item -ItemType Directory -Path $DestinationDir -Force | Out-Null

$fileName = Split-Path -Leaf $SourcePath
$destinationPath = Join-Path $DestinationDir $fileName

try {
    Copy-Item -LiteralPath $SourcePath -Destination $destinationPath -Force -ErrorAction Stop
    Write-Host "[AutoDeploy][$ProjectName] Copied $fileName -> $destinationPath"
    exit 0
}
catch {
    $fallbackPath = "$destinationPath.new"
    Copy-Item -LiteralPath $SourcePath -Destination $fallbackPath -Force -ErrorAction Stop
    Write-Warning "[AutoDeploy][$ProjectName] Target locked. Wrote fallback: $fallbackPath"
    exit 0
}
