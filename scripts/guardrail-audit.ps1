param(
    [string[]]$Paths = @("Core", "Bluelock", "CycleBorn", "VAutoTraps", "VAutoannounce")
)

$ErrorActionPreference = "Stop"

$patterns = @(
    "AppDomain\.CurrentDomain\.GetAssemblies",
    "GetAssemblies\s*\(",
    "Assembly\.GetTypes\s*\(",
    "GetTypes\s*\("
)

$hits = @()
foreach ($p in $Paths) {
    if (-not (Test-Path $p)) { continue }
    foreach ($pattern in $patterns) {
        $result = rg -n --glob "*.cs" $pattern $p 2>$null
        if ($LASTEXITCODE -eq 0 -and $result) {
            $hits += $result
        }
    }
}

if ($hits.Count -gt 0) {
    Write-Host "Guardrail audit failed. Reflection scan API usage found:"
    $hits | Sort-Object -Unique | ForEach-Object { Write-Host $_ }
    exit 1
}

Write-Host "Guardrail audit passed. No banned runtime reflection scan APIs found."
exit 0
