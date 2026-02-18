param(
    [string]$Source = "Core/Data/PrefabsAll.cs"
)

if (-not (Test-Path $Source)) { Write-Error "Source file not found: $Source"; exit 1 }

# Pattern: ["Name"] = new PrefabGUID(123);
$pattern = '\["(?<k>[^"]+)"\]\s*=\s*new\s+PrefabGUID\((?<v>-?\d+)\)'
$entries = @()
Get-Content $Source | ForEach-Object {
    if ($_ -match $pattern) {
        $entries += [pscustomobject]@{ Name=$matches.k; Guid=[int]$matches.v }
    }
}

function New-Alias([string]$name){
    $a = $name -replace '^(AB_|Spell_|Item_|Tech_|BloodType_|TM_|Chain_)',''
    $a = $a -replace '[^A-Za-z0-9]+','_'
    if($a.Length -gt 48){ $a = $a.Substring($a.Length-48) }
    return $a
}

function Emit-Catalog($items, $namespace, $className, $path){
    $dir = Split-Path $path -Parent
    if(-not (Test-Path $dir)){ New-Item -ItemType Directory -Path $dir | Out-Null }
    $used = @{}
    $lines = @()
    $lines += 'using System.Collections.Generic;'
    $lines += ''
    $lines += "namespace $namespace"
    $lines += '{'
    $lines += "    public static class $className"
    $lines += '    {'
    $lines += '        public static readonly Dictionary<string, string> ByShortName = new()'
    $lines += '        {'
    foreach($i in $items){
        $alias = New-Alias $i.Name
        if([string]::IsNullOrWhiteSpace($alias)){ continue }
        if($used.ContainsKey($alias)){ $used[$alias]++; $alias = "$alias" + '_' + $used[$alias] } else { $used[$alias]=1 }
        $prefab = $i.Name.Replace('"','\\"')
        $lines += "            [`"$alias`"] = `"$prefab`","
    }
    $lines += '        };'
    $lines += '    }'
    $lines += '}'
    Set-Content -Path $path -Value ($lines -join "`n") -Encoding UTF8
}

function Emit-WeaponsCatalog($allWeapons, $namespace, $path){
    $dir = Split-Path $path -Parent
    if(-not (Test-Path $dir)){ New-Item -ItemType Directory -Path $dir | Out-Null }

    $aliasUsed = @{}
    $artUsed   = @{}
    $lwUsed    = @{}

    $lines = @()
    $lines += 'using System.Collections.Generic;'
    $lines += ''
    $lines += "namespace $namespace"
    $lines += '{'
    $lines += '    public static class Weapons'
    $lines += '    {'
    $lines += '        public static readonly Dictionary<string, string> ByShortName = new()'
    $lines += '        {'
    foreach($i in $allWeapons){
        $alias = New-Alias $i.Name
        if([string]::IsNullOrWhiteSpace($alias)){ continue }
        if($aliasUsed.ContainsKey($alias)){ $aliasUsed[$alias]++; $alias = "$alias" + '_' + $aliasUsed[$alias] } else { $aliasUsed[$alias]=1 }
        $prefab = $i.Name.Replace('"','\\"')
        $lines += "            [`"$alias`"] = `"$prefab`","
    }
    $lines += '        };'
    $lines += ''
    $lines += '        public static class ArtCatalog'
    $lines += '        {'
    $lines += '            public static readonly Dictionary<string, string> ByShortName = new()'
    $lines += '            {'
    foreach($i in $allWeapons){
        if($i.Name -like '*Shattered*'){ continue }
        if($i.Name -match '^Item_Weapon_(?<type>[^_]+)_T09_Artifact_(?<element>[^_]+)_123$'){
            $alias = "$($matches.type)_$($matches.element)_123"
            if($artUsed.ContainsKey($alias)){ $artUsed[$alias]++; $alias = "$alias" + '_' + $artUsed[$alias] } else { $artUsed[$alias]=1 }
            $prefab = $i.Name.Replace('"','\\"')
            $lines += "                [`"$alias`"] = `"$prefab`","
        }
    }
    $lines += '            };'
    $lines += '        }'
    $lines += ''
    $lines += '        public static class LwCatalog'
    $lines += '        {'
    $lines += '            public static readonly Dictionary<string, string> ByShortName = new()'
    $lines += '            {'
    foreach($i in $allWeapons){
        if($i.Name -like '*Shattered*'){ continue }
        if($i.Name -match '^Item_Weapon_(?<type>[^_]+)_T09_Ancestral_(?<element>[^_]+)_12$'){
            $alias = "$($matches.type)_$($matches.element)_12"
            if($lwUsed.ContainsKey($alias)){ $lwUsed[$alias]++; $alias = "$alias" + '_' + $lwUsed[$alias] } else { $lwUsed[$alias]=1 }
            $prefab = $i.Name.Replace('"','\\"')
            $lines += "                [`"$alias`"] = `"$prefab`","
        }
    }
    $lines += '            };'
    $lines += '        }'
    $lines += '    }'
    $lines += '}'

    Set-Content -Path $path -Value ($lines -join "`n") -Encoding UTF8
}

# Filters
$glows      = $entries | Where-Object { $_.Name -match 'Glow|Trail|Aura|Light|Effect|Buff_' }
$abilities  = $entries | Where-Object { $_.Name -like 'AB_*' }
$spells     = $entries | Where-Object { $_.Name -like 'Spell_*' }
$vbloods    = $entries | Where-Object { $_.Name -match 'VBlood|^VIB_' }
$weapons    = $entries | Where-Object { $_.Name -match 'Item_Weapon_' }
$armors     = $entries | Where-Object { $_.Name -match 'Item_Armor_' }
$bloodtypes = $entries | Where-Object { $_.Name -match '^BloodType_' }
$traps      = $entries | Where-Object { $_.Name -match 'Trap|Spike|Mine|Bomb|Chest|Container' }
$tiles      = $entries | Where-Object { $_.Name -like 'TM_*' }
$chains     = $entries | Where-Object { $_.Name -like 'Chain_*' }
$weaponSkills = $entries | Where-Object { $_.Name -match '^AB_Vampire_.*(Sword|Axe|Mace|Spear|Slashers|Reaper|Crossbow|Longbow|Pistols|GreatSword|Whip)' }
$amulets    = $entries | Where-Object { $_.Name -match 'Amulet|MagicSource' }

# Emit per plugin
Emit-Catalog $glows      'VAuto.Zone.Data.DataType'        'Glows'        'Bluelock/Data/datatype/Glows.cs'
Emit-Catalog $abilities  'VAuto.Zone.Data.DataType'        'Abilities'    'Bluelock/Data/datatype/Abilities.cs'
Emit-Catalog $spells     'VAuto.Zone.Data.DataType'        'Spells'       'Bluelock/Data/datatype/Spells.cs'
Emit-Catalog $vbloods    'VAuto.Zone.Data.DataType'        'VBloods'      'Bluelock/Data/datatype/VBloods.cs'
Emit-WeaponsCatalog $weapons 'VAuto.Zone.Data.DataType'    'Bluelock/Data/datatype/Weapons.cs'
Emit-Catalog $weaponSkills 'VAuto.Zone.Data.DataType'      'WeaponSkills' 'Bluelock/Data/datatype/WeaponSkills.cs'
Emit-Catalog $amulets    'VAuto.Zone.Data.DataType'        'Amulets'      'Bluelock/Data/datatype/Amulets.cs'
Emit-Catalog $armors     'VAuto.Zone.Data.DataType'        'Armors'       'Bluelock/Data/datatype/Armors.cs'
Emit-Catalog $bloodtypes 'VAuto.Zone.Data.DataType'        'BloodTypes'   'Bluelock/Data/datatype/BloodTypes.cs'
Emit-Catalog $tiles      'VAuto.Zone.Data.DataType'        'Tiles'        'Bluelock/Data/datatype/Tiles.cs'
Emit-Catalog $chains     'VAuto.Zone.Data.DataType'        'Chains'       'Bluelock/Data/datatype/Chains.cs'
Emit-Catalog $traps      'VAuto.Zone.Data.DataType'        'Traps'        'Bluelock/Data/datatype/Traps.cs'

Emit-Catalog $glows      'VAuto.Core.Lifecycle.Data.DataType' 'Glows'      'CycleBorn/Data/datatype/Glows.cs'
Emit-Catalog $abilities  'VAuto.Core.Lifecycle.Data.DataType' 'Abilities'  'CycleBorn/Data/datatype/Abilities.cs'
Emit-Catalog $spells     'VAuto.Core.Lifecycle.Data.DataType' 'Spells'     'CycleBorn/Data/datatype/Spells.cs'
Emit-Catalog $vbloods    'VAuto.Core.Lifecycle.Data.DataType' 'VBloods'    'CycleBorn/Data/datatype/VBloods.cs'
Emit-WeaponsCatalog $weapons 'VAuto.Core.Lifecycle.Data.DataType' 'CycleBorn/Data/datatype/Weapons.cs'
Emit-Catalog $armors     'VAuto.Core.Lifecycle.Data.DataType' 'Armors'     'CycleBorn/Data/datatype/Armors.cs'
Emit-Catalog $bloodtypes 'VAuto.Core.Lifecycle.Data.DataType' 'BloodTypes' 'CycleBorn/Data/datatype/BloodTypes.cs'
Emit-Catalog $tiles      'VAuto.Core.Lifecycle.Data.DataType' 'Tiles'      'CycleBorn/Data/datatype/Tiles.cs'
Emit-Catalog $chains     'VAuto.Core.Lifecycle.Data.DataType' 'Chains'     'CycleBorn/Data/datatype/Chains.cs'
Emit-Catalog $traps      'VAuto.Core.Lifecycle.Data.DataType' 'Traps'      'CycleBorn/Data/datatype/Traps.cs'

Emit-Catalog $traps      'VAuto.Traps.Data.DataType'          'Traps'      'VAutoTraps/Data/DataType/Traps.cs'
Emit-Catalog $glows      'VAuto.Traps.Data.DataType'          'Glows'      'VAutoTraps/Data/DataType/Glows.cs'
Emit-WeaponsCatalog $weapons 'VAuto.Traps.Data.DataType'      'VAutoTraps/Data/DataType/Weapons.cs'

Emit-Catalog $vbloods    'VAuto.Announce.Data.DataType'       'VBloods'    'VAutoannounce/Data/DataType/VBloods.cs'
Emit-Catalog $spells     'VAuto.Announce.Data.DataType'       'Spells'     'VAutoannounce/Data/DataType/Spells.cs'
# ... [Keep your New-Alias and setup code above] ...

# New Filter for Tech & Progress (The stuff for Resetting)
$techUnlocks = $entries | Where-Object { $_.Name -match 'Tech_Collection_VBlood_|Tech_Ability_|Tech_Consumable_' }

# Emit the new Tech Catalog (Used for resetting progress/locking UI)
Emit-Catalog $techUnlocks 'VAuto.Core.Lifecycle.Data.DataType' 'TechUnlocks' 'CycleBorn/Data/datatype/TechUnlocks.cs'

# ... [Keep your existing Glows, Abilities, Spells, etc.] ...

# Special grouping for Boss Reset (Logic to link Boss -> Tech)
# This lets you do: TechUnlocks.ByShortName.Where(x => x.Key.Contains("Jade"))
Emit-Catalog $techUnlocks 'VAuto.Announce.Data.DataType' 'BossTech' 'VAutoannounce/Data/DataType/BossTech.cs'

Write-Host "Generated catalogs including TechUnlocks for UI Locking." -ForegroundColor Green
Write-Host "Generated per-type catalogs from $Source"
