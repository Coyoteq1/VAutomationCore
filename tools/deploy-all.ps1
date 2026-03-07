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
$inputPath = Resolve-FullPath $ServerBepInExPath
if ((Split-Path -Leaf $inputPath).ToLowerInvariant() -eq "plugins") {
    $plugins = $inputPath
    $bepInExRoot = Split-Path -Parent $plugins
}
elseif ((Split-Path -Leaf $inputPath).ToLowerInvariant() -eq "bepinex") {
    $bepInExRoot = $inputPath
    $plugins = Join-Path $bepInExRoot 'plugins'
}
else {
    throw "[deploy] ServerBepInExPath must point to either a BepInEx folder or a BepInEx/plugins folder. Received: $inputPath"
}

$blueluckConfigDir = Join-Path $bepInExRoot 'config/Blueluck'
$blueluckGameplayDir = Join-Path $blueluckConfigDir 'gameplay'

if (-not $SkipBuild) {
    Write-Host "[deploy] Building projects..." -ForegroundColor Cyan
    dotnet build "$root/VAutomationCore.sln" -c $Configuration --nologo | Out-Host
}

Write-Host "[deploy] Ensuring target directories..." -ForegroundColor Cyan
New-Item -ItemType Directory -Force -Path $plugins | Out-Null
New-Item -ItemType Directory -Force -Path $blueluckConfigDir | Out-Null
New-Item -ItemType Directory -Force -Path $blueluckGameplayDir | Out-Null

Write-Host "[deploy] Collecting artifacts..." -ForegroundColor Cyan
$tfm = 'net6.0'
$artifacts = @(
    (Join-Path $root "bin/$Configuration/$tfm/VAutomationCore.dll"),
    (Join-Path $root "bin/$Configuration/$tfm/VAutomationCore.pdb"),
    (Join-Path $root "Blueluck/bin/$Configuration/$tfm/Blueluck.dll"),
    (Join-Path $root "Blueluck/bin/$Configuration/$tfm/Blueluck.pdb")
) | Where-Object { Test-Path $_ }

if ($artifacts.Count -eq 0) {
    Write-Host "[deploy] No artifacts found. Did the build succeed?" -ForegroundColor Yellow
}

Write-Host "[deploy] Copying DLLs/PDBs to plugins: $plugins" -ForegroundColor Cyan
foreach ($f in $artifacts) {
    Copy-Item -Force -Path $f -Destination $plugins
}

$gameplayFiles = @(
    'arena.settings.json',
    'arena.zones.json',
    'arena.rules.json',
    'arena_flows.config.json',
    'arena_presets.config.json',
    'boss.settings.json',
    'boss.zones.json',
    'boss.rules.json',
    'boss_flows.config.json',
    'boss_presets.config.json'
)

Write-Host "[deploy] Copying Blueluck gameplay configs to: $blueluckGameplayDir" -ForegroundColor Cyan
foreach ($fileName in $gameplayFiles) {
    $source = Join-Path $root "Blueluck/config/$fileName"
    if (Test-Path $source) {
        Copy-Item -Force -Path $source -Destination $blueluckGameplayDir
    }
}

$buffsSource = Join-Path $root 'Blueluck/config/buffs_numbered.txt'
if (Test-Path $buffsSource) {
    Copy-Item -Force -Path $buffsSource -Destination $blueluckConfigDir
}

Write-Host "[deploy] Blueluck configs copied under: $blueluckConfigDir" -ForegroundColor Cyan

Write-Host "[deploy] Done." -ForegroundColor Green
Write-Host "[deploy] BepInEx: $bepInExRoot" -ForegroundColor Green
Write-Host "[deploy] Plugins: $plugins" -ForegroundColor Green
Write-Host "[deploy] Configs(Blueluck): $blueluckConfigDir" -ForegroundColor Green

