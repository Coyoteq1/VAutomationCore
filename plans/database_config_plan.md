# Database & Config Cleanup Plan

## Current State Issues

### Config Files (Problems)
| Location | File | Issue |
|----------|------|-------|
| `Bluelock/config/` | `VAuto.Zones.json` | Uses `VAuto.` prefix, should be `Bluelock.` |
| `Bluelock/config/` | `VAuto.ZoneLifecycle.json` | Uses `VAuto.` prefix |
| `Bluelock/config/` | `VAuto.Kits.json` | Uses `VAuto.` prefix |
| `Bluelock/config/` | `ability_zones.json` | Not namespaced |
| `Bluelock/config/` | `ability_prefabs.json` | Not namespaced |
| `Bluelock/config/` | `Prefabsref.json` | Not namespaced |
| `config/` | `VAuto.unified_config.schema.json` | Root-level schema, not used |
| Root | Multiple .cs files in `Bluelock/Data/datatype/` | Hardcoded data, should be JSON/CSV |

### Missing Files
- No BepInEx `.cfg` files for runtime settings (currently hardcoded in Plugin.cs)
- No dedicated database directory with JSON/CSV reference data
- No unified `Bluelock.cfg` for general plugin settings

---

## Target Structure

```
BepInEx/
└── config/
    └── Bluelock/
        ├── Bluelock.cfg                    ← NEW: BepInEx runtime settings
        ├── Bluelock.zones.json              ← RENAMED: was VAuto.Zones.json
        ├── Bluelock.lifecycle.json           ← RENAMED: was VAuto.ZoneLifecycle.json
        ├── Bluelock.kits.json               ← RENAMED: was VAuto.Kits.json
        ├── Bluelock.ability_zones.json      ← RENAMED: was ability_zones.json
        ├── Bluelock.ability_prefabs.json    ← RENAMED: was ability_prefabs.json
        ├── Bluelock.prefabs.json            ← RENAMED: was Prefabsref.json
        └── database/                        ← NEW: reference data
            ├── weapons.json
            ├── armors.json
            ├── abilities.json
            ├── buffs.json
            ├── vbloods.json
            └── items.csv

VAutomationCore/
├── config/                                   ← Source configs (copied to BepInEx on build)
│   └── Bluelock/
│       ├── Bluelock.cfg
│       ├── Bluelock.zones.json
│       ├── Bluelock.lifecycle.json
│       ├── Bluelock.kits.json
│       ├── database/
│       │   ├── weapons.json
│       │   ├── armors.json
│       │   ├── abilities.json
│       │   ├── buffs.json
│       │   └── vbloods.json
│       └── templates/                        ← NEW: template definitions
│           ├── borders.json
│           ├── floors.json
│           └── roofs.json
```

---

## Files to Create

### 1. Bluelock.cfg (BepInEx Config)

```ini
[General]
## Enable or disable the plugin
Enabled = true

## Log level: Debug, Info, Warning, Error
LogLevel = Info

[ZoneDetection]
## How often to check player position (ms)
CheckIntervalMs = 100

## Position change threshold to trigger recheck (units)
PositionThreshold = 1.0

## Enable debug mode for zone detection
DebugMode = false

[GlowSystem]
## Enable glow border system
Enabled = true

## Update interval for glow effects (seconds)
UpdateInterval = 5.0

## Enable auto-rotation of glow colors
AutoRotateEnabled = false

[Arena]
## Enable arena territory features
TerritoryEnabled = true

## Show arena territory grid
ShowGrid = false

## Grid cell size
GridCellSize = 10.0

[Sandbox]
## Enable sandbox mode
Enabled = true

## Auto-apply unlocks in sandbox
AutoApplyUnlocks = true

## Despawn delay for sandbox entities (seconds)
DespawnDelaySeconds = 2
```

### 2. Bluelock.zones.json (Renamed from VAuto.Zones.json)

Keep same structure, just rename file.

### 3. Bluelock.lifecycle.json (Renamed from VAuto.ZoneLifecycle.json)

Keep same structure, just rename file.

### 4. Bluelock.kits.json (Renamed from VAuto.Kits.json)

Keep same structure, just rename file.

### 5. Bluelock.ability_zones.json (Renamed)

```json
{
  "version": "1.0.0",
  "zones": [
    {
      "zoneId": "1",
      "resetCooldownsOnEnter": true,
      "resetCooldownsOnExit": false,
      "saveAndRestoreSlots": true,
      "saveAndRestoreCooldowns": true,
      "presetSlots": [
        "AB_Vampire_VeilOfBlood_Group",
        "AB_Vampire_VeilOfChaos_Group",
        "AB_Vampire_VeilOfFrost_Group",
        "AB_Vampire_VeilOfBones_AbilityGroup"
      ],
      "restrictedAbilities": [],
      "allowedAbilities": []
    },
    {
      "zoneId": "*",
      "resetCooldownsOnEnter": true,
      "resetCooldownsOnExit": false,
      "saveAndRestoreSlots": true,
      "saveAndRestoreCooldowns": true,
      "presetSlots": [
        "AB_Vampire_VeilOfBlood_Group",
        "AB_Blood_BloodRite_AbilityGroup"
      ],
      "restrictedAbilities": [],
      "allowedAbilities": []
    }
  ]
}
```

### 6. Bluelock.ability_prefabs.json (Renamed)

Keep same structure.

### 7. Bluelock.prefabs.json (Renamed from Prefabsref.json)

Keep same structure.

### 8. Database Files (NEW)

#### database/weapons.json
```json
{
  "version": "1.0.0",
  "weapons": [
    {
      "id": "Item_Weapon_Sword_T09_ShadowMatter",
      "name": "Shadow Matter Sword",
      "type": "Sword",
      "tier": 9,
      "prefabGuid": 1688563035,
      "damage": 45.2,
      "attackSpeed": 1.15,
      "range": 2.5,
      "slots": ["MainHand"]
    }
  ]
}
```

#### database/armors.json
```json
{
  "version": "1.0.0",
  "armors": [
    {
      "id": "Item_Chest_T09_Dracula_Warrior",
      "name": "Dracula Warrior Chest",
      "type": "Chest",
      "tier": 9,
      "prefabGuid": 1764193512,
      "armor": 12.5,
      "slots": ["Chest"]
    }
  ]
}
```

#### database/abilities.json
```json
{
  "version": "1.0.0",
  "abilities": [
    {
      "id": "AB_Vampire_VeilOfBlood_Group",
      "name": "Veil of Blood",
      "type": "AbilityGroup",
      "prefabGuid": -1431741335,
      "cooldown": 30.0,
      "duration": 8.0
    }
  ]
}
```

#### database/buffs.json
```json
{
  "version": "1.0.0",
  "buffs": [
    {
      "id": "v2_buffs_jail_dracula",
      "name": "Dracula's Prison",
      "type": "Debuff",
      "prefabGuid": -1431741336,
      "duration": 5.0,
      "stackLimit": 1
    }
  ]
}
```

#### database/vbloods.json
```json
{
  "version": "1.0.0",
  "vbloods": [
    {
      "id": "CHAR_Vampire_Trent_01",
      "name": "Trent",
      "level": 62,
      "prefabGuid": 1513824759,
      "territory": "Dunley Farmlands",
      "drops": [
        { "item": "Blood_Extract", "chance": 100 }
      ]
    }
  ]
}
```

### 9. Template Definitions (NEW)

#### templates/borders.json
```json
{
  "version": "1.0.0",
  "templates": [
    {
      "id": "border_standard",
      "prefabName": "TM_Castle_ObjectDecor_TargetDummy_Vampire01",
      "prefabGuid": 230163020,
      "spacing": 3.0,
      "heightOffset": 0.0
    }
  ]
}
```

#### templates/floors.json
```json
{
  "version": "1.0.0",
  "templates": [
    {
      "id": "floor_stone",
      "prefabName": "TM_Castle_FloorDecor_Stone_Small",
      "prefabGuid": 123456789,
      "spacing": 2.0,
      "rotation": 0
    }
  ]
}
```

---

## Migration Steps

1. **Create new directory structure**
   - `Bluelock/config/database/`
   - `Bluelock/config/templates/`

2. **Create Bluelock.cfg** in `Bluelock/config/`

3. **Rename existing JSON files** (in `Bluelock/config/`):
   - `VAuto.Zones.json` → `Bluelock.zones.json`
   - `VAuto.ZoneLifecycle.json` → `Bluelock.lifecycle.json`
   - `VAuto.Kits.json` → `Bluelock.kits.json`
   - `ability_zones.json` → `Bluelock.ability_zones.json`
   - `ability_prefabs.json` → `Bluelock.ability_prefabs.json`
   - `Prefabsref.json` → `Bluelock.prefabs.json`

4. **Create database JSON files** in `Bluelock/config/database/`

5. **Create template JSON files** in `Bluelock/config/templates/`

6. **Update code references** to point to new file paths:
   - `PrefabResolver.cs` line 26: `Prefabsref.json` → `Bluelock.prefabs.json`
   - `ZoneCommands.cs` line 37: `VAuto.Zones.json` → `Bluelock.zones.json`

7. **Update Bluelock/Plugin.cs** to read from `Bluelock.cfg` instead of hardcoded values

---

## Code Changes Required

| File | Change |
|------|--------|
| `Bluelock/Core/PrefabResolver.cs` | Update path from `Prefabsref.json` to `Bluelock.prefabs.json` |
| `Bluelock/Commands/Core/ZoneCommands.cs` | Update path from `VAuto.Zones.json` to `Bluelock.zones.json` |
| `Bluelock/Plugin.cs` | Add BepInEx config file reading for `Bluelock.cfg` |
| `Bluelock/Services/ZoneConfigService.cs` | Update config file paths |
| `Bluelock/Services/ProcessConfigService.cs` | Update config file paths |

---

## What NOT to Change

- `config/VAuto.unified_config.schema.json` — keep as reference for future unified config
- Large `.cs` data files in `Bluelock/Data/datatype/` — these are compile-time references and should remain as C# for now (they're used by the code, not loaded at runtime)
- CycleBorn config structure (separate plugin)
- VAutoTraps config structure (separate plugin)
