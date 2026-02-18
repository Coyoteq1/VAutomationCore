param(
    [switch]$Deploy,
    [string]$Configuration = "Debug"
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
Set-Location $repoRoot

$projects = @(
    "VAutomationCore.csproj",
    "CycleBorn/Vlifecycle.csproj",
    "BlueLock/VAutoZone.csproj",
    "VAutoTraps/VAutoTraps.csproj",
    "VAutoannounce/VAutoannounce.csproj"
)

$target = "Build"
$deployFlag = if ($Deploy) { "true" } else { "false" }
$modeText = if ($Deploy) { "Deploy (Build + copy targets)" } else { "Compile-only (Build, deploy copy disabled)" }

Write-Host "Mode: $modeText"
Write-Host "Configuration: $Configuration"
Write-Host ""

$results = New-Object System.Collections.Generic.List[object]

foreach ($project in $projects) {
    Write-Host "=== $project ==="
    $args = @("msbuild", $project, "/t:$target", "/p:Configuration=$Configuration", "/p:DeployToServer=$deployFlag", "/nologo")
    & dotnet @args
    $code = $LASTEXITCODE

    $status = if ($code -eq 0) { "OK" } else { "FAIL" }
    $results.Add([pscustomobject]@{
        Project = $project
        Status  = $status
        ExitCode = $code
    })

    if ($code -ne 0) {
        Write-Host "Result: FAIL ($code)" -ForegroundColor Red
    } else {
        Write-Host "Result: OK" -ForegroundColor Green
    }

    Write-Host ""
}

Write-Host "Summary"
$results | Format-Table -AutoSize

$failed = $results | Where-Object { $_.ExitCode -ne 0 }
if ($failed.Count -gt 0) {
    exit 1
}

exit 0
