param(
    [string]$PluginPath = "D:\DedicatedServerLauncher\VRisingServer\BepInEx\plugins",
    [string]$Configuration = "Debug",
    [int]$Retries = 5,
    [int]$RetryDelayMs = 1000
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
Set-Location $repoRoot

$artifacts = @(
    @{ Name = "VAutomationCore.dll"; Source = "bin/$Configuration/net6.0/VAutomationCore.dll" },
    @{ Name = "Cycleborn.dll";      Source = "CycleBorn/bin/$Configuration/net6.0/Cycleborn.dll" },
    @{ Name = "BlueLock.dll";       Source = "BlueLock/bin/$Configuration/net6.0/BlueLock.dll" },
    @{ Name = "VAutoTraps.dll";     Source = "VAutoTraps/bin/$Configuration/net6.0/VAutoTraps.dll" },
    @{ Name = "VAutoannounce.dll";  Source = "VAutoannounce/bin/$Configuration/net6.0/VAutoannounce.dll" }
)

if (-not (Test-Path $PluginPath)) {
    throw "Plugin path not found: $PluginPath"
}

$results = New-Object System.Collections.Generic.List[object]

foreach ($item in $artifacts) {
    $sourcePath = Join-Path $repoRoot $item.Source
    $targetPath = Join-Path $PluginPath $item.Name

    if (-not (Test-Path $sourcePath)) {
        $results.Add([pscustomobject]@{
            File = $item.Name
            Status = "MISSING_SOURCE"
            Detail = $sourcePath
        })
        continue
    }

    $copied = $false
    for ($i = 1; $i -le $Retries; $i++) {
        try {
            Copy-Item -Path $sourcePath -Destination $targetPath -Force
            $copied = $true
            break
        }
        catch {
            if ($i -lt $Retries) {
                Start-Sleep -Milliseconds $RetryDelayMs
            }
            else {
                $results.Add([pscustomobject]@{
                    File = $item.Name
                    Status = "COPY_FAILED"
                    Detail = $_.Exception.Message
                })
            }
        }
    }

    if ($copied) {
        $results.Add([pscustomobject]@{
            File = $item.Name
            Status = "COPIED"
            Detail = $targetPath
        })
    }
}

$results | Format-Table -AutoSize

$failed = $results | Where-Object { $_.Status -ne "COPIED" }
if ($failed.Count -gt 0) {
    exit 1
}

exit 0
