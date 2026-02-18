param(
    [string]$Root = "."
)

$ErrorActionPreference = "Stop"

$targetDirs = @(
    "Bluelock",
    "CycleBorn",
    "VAutoannounce",
    "VAutoTraps",
    "Core",
    "Services",
    "Chat",
    "Configuration",
    "Models"
)

$patterns = @(
    @{
        Name = "AppDomain Assembly Enumeration"
        Regex = 'AppDomain\s*\.\s*CurrentDomain\s*\.\s*GetAssemblies\s*\('
    },
    @{
        Name = "Assembly.GetTypes Scan"
        Regex = 'Assembly\s*\.\s*GetTypes\s*\('
    },
    @{
        Name = "Generic .GetTypes Scan"
        Regex = '\.GetTypes\s*\('
    },
    @{
        Name = "Assembly.GetExportedTypes Scan"
        Regex = 'Assembly\s*\.\s*GetExportedTypes\s*\('
    },
    @{
        Name = "Generic .GetExportedTypes Scan"
        Regex = '\.GetExportedTypes\s*\('
    }
)

$projectFiles = @()
foreach ($dir in $targetDirs)
{
    $fullPath = Join-Path $Root $dir
    if (-not (Test-Path $fullPath))
    {
        continue
    }

    $projectFiles += Get-ChildItem -Path $fullPath -Recurse -File -Filter "*.cs" |
        Where-Object {
            $_.FullName -notmatch '\\bin\\' -and
            $_.FullName -notmatch '\\obj\\' -and
            $_.FullName -notmatch '\\_build\\'
        }
}

$findings = @()
foreach ($file in $projectFiles)
{
    $content = Get-Content -Path $file.FullName
    for ($lineIndex = 0; $lineIndex -lt $content.Count; $lineIndex++)
    {
        $line = $content[$lineIndex]
        foreach ($pattern in $patterns)
        {
            if ($line -match $pattern.Regex)
            {
                $relativePath = Resolve-Path -Relative $file.FullName
                $findings += [PSCustomObject]@{
                    Pattern = $pattern.Name
                    File = $relativePath
                    Line = $lineIndex + 1
                    Text = $line.Trim()
                }
            }
        }
    }
}

if ($findings.Count -eq 0)
{
    Write-Host "No runtime assembly scanning patterns found in audited runtime projects."
    exit 0
}

Write-Host "Runtime assembly scanning patterns detected:`n"
foreach ($finding in $findings)
{
    Write-Host "[$($finding.Pattern)] $($finding.File):$($finding.Line)"
    Write-Host "  $($finding.Text)"
}

Write-Host "`nTotal findings: $($findings.Count)"
exit 1
