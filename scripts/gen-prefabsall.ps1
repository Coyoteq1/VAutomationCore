param(
    [string]$Source = "Data/All.json",
    [string]$Out = "Core/Data/PrefabsAll.cs"
)

if (-not (Test-Path $Source)) {
    Write-Error "Source file not found: $Source"
    exit 1
}

$pattern = '^\s*"(?<k>[^"]+)"\s*:\s*(?<v>-?\d+)\s*,?\s*$'
$entries = New-Object System.Collections.Generic.List[string]

Get-Content $Source | ForEach-Object {
    if ($_ -match $pattern) {
        $k = $matches.k
        $v = $matches.v
        $kk = $k.Replace('\','\\').Replace('"','\"')
        $entries.Add("                [""$kk""] = new PrefabGUID($v),")
    }
}

$sb = New-Object System.Text.StringBuilder
$null = $sb.AppendLine("using System;")
$null = $sb.AppendLine("using System.Collections.Generic;")
$null = $sb.AppendLine("using Stunlock.Core;")
$null = $sb.AppendLine("")
$null = $sb.AppendLine("namespace VAutomationCore.Core.Data")
$null = $sb.AppendLine("{")
$null = $sb.AppendLine("    public static class PrefabsAll")
$null = $sb.AppendLine("    {")
$null = $sb.AppendLine("        public static readonly Dictionary<string, PrefabGUID> ByName =")
$null = $sb.AppendLine("            new(StringComparer.OrdinalIgnoreCase)")
$null = $sb.AppendLine("            {")
foreach ($e in $entries) { $null = $sb.AppendLine($e) }
$null = $sb.AppendLine("            };")
$null = $sb.AppendLine("")
$null = $sb.AppendLine("        public static bool TryGet(string name, out PrefabGUID guid)")
$null = $sb.AppendLine("        {")
$null = $sb.AppendLine("            guid = PrefabGUID.Empty;")
$null = $sb.AppendLine("            return !string.IsNullOrWhiteSpace(name) && ByName.TryGetValue(name, out guid);")
$null = $sb.AppendLine("        }")
$null = $sb.AppendLine("    }")
$null = $sb.AppendLine("}")

Set-Content -Path $Out -Value $sb.ToString() -Encoding UTF8
Write-Host "PrefabsAll generated from $Source -> $Out with $($entries.Count) entries."
