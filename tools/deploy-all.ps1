param(
    [string]$Configuration = "Debug",
    [string]$ServerBepInExPath = "../../DedicatedServerLauncher/VRisingServer/BepInEx",
    [switch]$SkipBuild
)

$ErrorActionPreference = 'Stop'

function Resolve-FullPath([string]$Path) {
    if ([System.IO.Path]::IsPathRooted($Path)) { return $Path }
    return (Resolve-Path -Path $Path).Path
}

Write-Host "[deploy] Starting deploy (Configuration=$Configuration)" -ForegroundColor Cyan

$root = Resolve-FullPath "."
$serverRoot = Resolve-FullPath $ServerBepInExPath
$plugins = Join-Path $serverRoot 'plugins'
$configDir = Join-Path $serverRoot 'config/Bluelock'

if (-not $SkipBuild) {
    Write-Host "[deploy] Building projects..." -ForegroundColor Cyan
    dotnet build "$root/VAutomationCore.csproj" -c $Configuration | Out-Host
    dotnet build "$root/Bluelock/VAutoZone.sln" -c $Configuration | Out-Host
    dotnet build "$root/CycleBorn/Vlifecycle.sln" -c $Configuration | Out-Host
}

Write-Host "[deploy] Ensuring target directories..." -ForegroundColor Cyan
New-Item -ItemType Directory -Force -Path $plugins | Out-Null
New-Item -ItemType Directory -Force -Path $configDir | Out-Null

Write-Host "[deploy] Collecting artifacts..." -ForegroundColor Cyan
$tfm = 'net6.0'
$artifacts = @(
    (Join-Path $root "bin/$Configuration/$tfm/VAutomationCore.dll"),
    (Join-Path $root "bin/$Configuration/$tfm/VAutomationCore.pdb"),
    (Join-Path $root "Bluelock/bin/$Configuration/$tfm/BlueLock.dll"),
    (Join-Path $root "Bluelock/bin/$Configuration/$tfm/BlueLock.pdb"),
    (Join-Path $root "CycleBorn/bin/$Configuration/$tfm/Cycleborn.dll"),
    (Join-Path $root "CycleBorn/bin/$Configuration/$tfm/Cycleborn.pdb")
) | Where-Object { Test-Path $_ }

if ($artifacts.Count -eq 0) {
    Write-Host "[deploy] No artifacts found. Did the build succeed?" -ForegroundColor Yellow
}

Write-Host "[deploy] Copying DLLs/PDBs to plugins: $plugins" -ForegroundColor Cyan
foreach ($f in $artifacts) {
    Copy-Item -Force -Path $f -Destination $plugins
}

Write-Host "[deploy] Copying Bluelock config JSONs to: $configDir" -ForegroundColor Cyan
$bluelockConfigSrc = Join-Path $root 'Bluelock/config'
if (Test-Path $bluelockConfigSrc) {
    Get-ChildItem -Path $bluelockConfigSrc -Filter '*.json' | ForEach-Object {
        Copy-Item -Force -Path $_.FullName -Destination $configDir
    }
}

Write-Host "[deploy] Done." -ForegroundColor Green
Write-Host "[deploy] Plugins: $plugins" -ForegroundColor Green
Write-Host "[deploy] Configs:  $configDir" -ForegroundColor Green

