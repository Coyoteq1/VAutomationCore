param(
    [string[]]$Projects = @(
        "Bluelock/VAutoZone.csproj",
        "CycleBorn/Vlifecycle.csproj",
        "VAutoTraps/VAutoTraps.csproj",
        "VAutoannounce/VAutoannounce.csproj"
    )
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Normalize-RelativePath {
    param([string]$PathValue)
    if ([string]::IsNullOrWhiteSpace($PathValue)) {
        return ""
    }

    $normalized = $PathValue.Replace("/", "\\")
    while ($normalized.StartsWith(".\\", [System.StringComparison]::Ordinal)) {
        $normalized = $normalized.Substring(2)
    }
    return $normalized
}

function Convert-ToAuditPattern {
    param([string]$Pattern)
    $normalized = Normalize-RelativePath $Pattern
    # WildcardPattern does not need explicit ** semantics for this use case.
    return $normalized.Replace("**", "*")
}

function Test-AuditMatch {
    param(
        [string]$RelativePath,
        [string]$Pattern
    )

    $wildcard = [System.Management.Automation.WildcardPattern]::new($Pattern, [System.Management.Automation.WildcardOptions]::IgnoreCase)
    return $wildcard.IsMatch($RelativePath)
}

function Get-CompileNodeValues {
    param(
        [xml]$ProjectXml,
        [string]$AttributeName
    )

    $values = New-Object System.Collections.Generic.List[string]
    $compileNodes = $ProjectXml.SelectNodes("//Compile[@$AttributeName]")
    foreach ($node in $compileNodes) {
        $raw = $node.GetAttribute($AttributeName)
        if ([string]::IsNullOrWhiteSpace($raw)) {
            continue
        }

        foreach ($entry in $raw.Split(';')) {
            $trimmed = $entry.Trim()
            if (-not [string]::IsNullOrWhiteSpace($trimmed)) {
                $values.Add($trimmed)
            }
        }
    }

    return $values
}

function Get-IncludedSourceFiles {
    param([string]$CsprojPath)

    $projectFullPath = Resolve-Path $CsprojPath
    $projectDir = Split-Path $projectFullPath -Parent
    [xml]$projectXml = Get-Content -Path $projectFullPath

    $removePatterns = (Get-CompileNodeValues -ProjectXml $projectXml -AttributeName "Remove") |
        ForEach-Object { Convert-ToAuditPattern $_ }

    $includePatterns = (Get-CompileNodeValues -ProjectXml $projectXml -AttributeName "Include") |
        ForEach-Object { Convert-ToAuditPattern $_ }

    $allProjectFiles = Get-ChildItem -Path $projectDir -Recurse -File -Filter *.cs | Select-Object -ExpandProperty FullName
    $included = New-Object System.Collections.Generic.HashSet[string]([System.StringComparer]::OrdinalIgnoreCase)

    foreach ($file in $allProjectFiles) {
        $relative = Normalize-RelativePath $file.Substring($projectDir.Length + 1)

        $isRemoved = $false
        foreach ($pattern in $removePatterns) {
            if (Test-AuditMatch -RelativePath $relative -Pattern $pattern) {
                $isRemoved = $true
                break
            }
        }

        if (-not $isRemoved) {
            [void]$included.Add($file)
            continue
        }

        foreach ($pattern in $includePatterns) {
            if ($pattern.StartsWith("..", [System.StringComparison]::Ordinal)) {
                continue
            }

            if (Test-AuditMatch -RelativePath $relative -Pattern $pattern) {
                [void]$included.Add($file)
                break
            }
        }
    }

    return [string[]]$included
}

function Get-CommandEntries {
    param(
        [string]$ProjectName,
        [string[]]$Files
    )

    $entries = New-Object System.Collections.Generic.List[object]

    foreach ($file in $Files) {
        $content = @(Get-Content -Path $file)
        $groupRoot = $null

        for ($i = 0; $i -lt $content.Count; $i++) {
            $line = $content[$i]

            if ($line -match '\[CommandGroup\("([^"]+)"') {
                $groupRoot = $matches[1].Trim()
            }

            $commandText = $null
            if ($line -match '\[Command\("([^"]+)"\s*,\s*"([^"]+)"') {
                $commandText = ($matches[1] + " " + $matches[2]).Trim()
            }
            elseif ($line -match '\[Command\("([^"]+)"') {
                $commandText = $matches[1].Trim()
            }

            if ($null -ne $commandText) {
                if ([string]::IsNullOrWhiteSpace($commandText)) {
                    continue
                }

                $fullCommand = $commandText
                if (-not [string]::IsNullOrWhiteSpace($groupRoot)) {
                    if (-not $commandText.StartsWith("$groupRoot ", [System.StringComparison]::OrdinalIgnoreCase)) {
                        $fullCommand = "$groupRoot $commandText"
                    }
                }

                $entries.Add([pscustomobject]@{
                    Project = $ProjectName
                    Command = $fullCommand.ToLowerInvariant()
                    File = $file
                    Line = $i + 1
                })
            }
        }
    }

    return $entries
}

$allEntries = New-Object System.Collections.Generic.List[object]

foreach ($project in $Projects) {
    if (-not (Test-Path $project)) {
        throw "Project not found: $project"
    }

    $projectPath = Resolve-Path $project
    $projectName = [System.IO.Path]::GetFileNameWithoutExtension($projectPath)
    $sourceFiles = Get-IncludedSourceFiles -CsprojPath $projectPath
    $commandEntries = Get-CommandEntries -ProjectName $projectName -Files $sourceFiles

    foreach ($entry in $commandEntries) {
        $allEntries.Add($entry)
    }
}

$duplicates = @(
    $allEntries |
        Group-Object -Property Command |
        Where-Object { $_.Count -gt 1 } |
        Sort-Object Name
)

if ($duplicates.Count -gt 0) {
    Write-Host "Duplicate command paths found:" -ForegroundColor Red
    foreach ($group in $duplicates) {
        Write-Host "- $($group.Name)"
        foreach ($entry in $group.Group) {
            $relativeFile = Resolve-Path -Relative $entry.File
            Write-Host ("  [{0}] {1}:{2}" -f $entry.Project, $relativeFile, $entry.Line)
        }
    }

    exit 1
}

Write-Host "No duplicate command paths found across audited projects." -ForegroundColor Green
Write-Host "Audited command entries: $($allEntries.Count)"
exit 0
