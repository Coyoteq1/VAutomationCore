# VAutomationCore Project Roadmap

## Overview

VAutomationCore is a comprehensive modding framework for **V Rising** dedicated servers. It provides zone/arena management, player state lifecycle handling, sandbox progression systems, and REST APIs for server administration.

### Project Structure
```
VAutomationCore/
├── VAutomationCore.csproj     # Core framework DLL (gg.coyote.VAutomationCore)
├── Bluelock/                   # Zone & Arena management plugin
├── CycleBorn/                  # Lifecycle/Progression plugin  
├── Chat/                       # Chat service utilities
├── Configuration/              # Centralized config management
├── ZUtility/                   # REST API plugin
├── Core/                       # Shared core services
├── config/                     # Global config schemas
└── assets/                    # Data files & archives
```

---

## 1. Core Framework (`VAutomationCore`)

The base dependency plugin that all other plugins rely on. Provides unified access to V Rising's ECS systems.

### Key Files

| File | Purpose |
|------|---------|
| [`Core/UnifiedCore.cs`](Core/UnifiedCore.cs) | **Central access point** for Server World, EntityManager, PrefabCollection |
| [`IService.cs`](IService.cs) | **Service interfaces** (IEntityService, IArenaService, IPlayerService, IBuildService, etc.) |
| [`Core/Services/GameActionService.cs`](Core/Services/GameActionService.cs) | **Runtime actions**: buff apply/remove, teleport, messaging |
| [`Core/Commands/CommandBase.cs`](Core/Commands/CommandBase.cs) | **Base class** for chat commands with permissions, cooldowns, feedback |

### Core APIs

#### UnifiedCore
```csharp
// Core access - thread-safe singleton
UnifiedCore.Server              // Get Server World
UnifiedCore.EntityManager       // Get EntityManager
UnifiedCore.PrefabCollection    // Get PrefabCollectionSystem
UnifiedCore.TryGetPrefabEntity(PrefabGUID)  // Resolve prefab
UnifiedCore.CreateEntity(position, rotation, scale)  // Spawn entity
UnifiedCore.DestroyEntity(entity)  // Remove entity
UnifiedCore.EntityExists(entity)   // Check existence
```

#### GameActionService
```csharp
// Buff management
GameActionService.TryApplyBuff(entity, buffGuid, out buffEntity, duration)
GameActionService.TryRemoveBuff(entity, buffGuid)
GameActionService.HasBuff(entity, buffGuid)

// Teleport & Position
GameActionService.TryTeleport(entity, position)
GameActionService.TrySetEntityPosition(entity, position)

// Messaging
GameActionService.TrySendSystemMessageToAll(message)
GameActionService.TrySendSystemMessageToPlatformId(platformId, message)

// Event system
GameActionService.RegisterEventAction(eventName, actionName, argTransform)
GameActionService.TriggerEvent(eventName, args)
GameActionService.TriggerLifecycleFlow(args)
```

#### Service Interfaces
```csharp
IService             // Base: Initialize(), Cleanup(), IsInitialized, Log
IEntityService      // Entity registration: RegisterEntity(), UnregisterEntity()
IArenaService       // Arena CRUD: CreateArena(), DeleteArena(), GetArenaIds()
IPlayerService      // Player tracking: AddPlayer(), RemovePlayer(), GetPlayerIds()
IBuildService       // Building: BuildStructure(), RemoveStructure(), SetBuildPermission()
IVisualEffectService // VFX: CreateEffect(), UpdateEffect(), RemoveEffect()
ILifecycleService   // Zone entry/exit: EnterArena(), ExitArena(), IsPlayerInArena()
IZoneService        // Zone detection: IsInZone(), EnterZone(), ExitZone()
IHealingService    // Health: ApplyHeal(), HealPlayer(), SetHealth()
ITeleportService   // Teleport: Teleport(), TeleportTo(), CanTeleport()
```

---

## 2. Bluelock Plugin (`gg.coyote.BlueLock`)

Main zone and arena management plugin. Handles zone detection, player state transitions, kit system, ability management, and arena territories.

### Plugin Entry Point

| File | Purpose |
|------|---------|
| [`Bluelock/Plugin.cs`](Bluelock/Plugin.cs) | **Main plugin class** - registers commands, patches, services |
| [`Bluelock/README.md`](Bluelock/README.md) | Plugin documentation |

### Commands (`Bluelock/Commands/Core/`)

| Command File | Aliases | Purpose |
|--------------|---------|---------|
| [`ZoneCommands.cs`](Bluelock/Commands/Core/ZoneCommands.cs) | `.z`, `.zone` | Zone management (list, info, teleport) |
| [`TemplateCommands.cs`](Bluelock/Commands/Core/TemplateCommands.cs) | `.template` | Spawn templates (structures, NPCs) |
| [`MatchCommands.cs`](Bluelock/Commands/Core/MatchCommands.cs) | `.match` | Arena match control |
| [`SpawnCommands.cs`](Bluelock/Commands/Core/SpawnCommands.cs) | `.spawn` | Entity spawning |
| [`TagCommands.cs`](Bluelock/Commands/Core/TagCommands.cs) | `.tag` | Player tagging/naming |
| [`QuickZoneCommands.cs`](Bluelock/Commands/Core/QuickZoneCommands.cs) | `.enter`, `.exit` | Quick zone access |
| [`VBloodUnlockCommands.cs`](Bluelock/Commands/Core/VBloodUnlockCommands.cs) | `.vblood` | VBlood unlock management |

### Core Services (`Bluelock/Services/`)

| Service | Purpose |
|---------|---------|
| [`ZoneConfigService.cs`](Bluelock/Services/ZoneConfigService.cs) | **Load/manage** zone configurations from JSON |
| [`KitService.cs`](Bluelock/Services/KitService.cs) | **Kit system**: auto-equip gear sets on zone entry |
| [`PlayerSnapshotService.cs`](Bluelock/Services/PlayerSnapshotService.cs) | **Save/restore** player state (inventory, buffs, progression) |
| [`ZonePlayerTagService.cs`](Bluelock/Services/ZonePlayerTagService.cs) | **Tag players** with zone-specific names |
| [`AbilityUi.cs`](Bluelock/Services/AbilityUi.cs) | **Manage ability bars** for zones |
| [`GlowService.cs`](Bluelock/Services/GlowService.cs) | **Visual glow effects** for zone borders |
| [`GlowTileService.cs`](Bluelock/Services/GlowTileService.cs) | **Tile-based glow** rendering |
| [`ArenaMatchManager.cs`](Bluelock/Services/ArenaMatchManager.cs) | **Arena match** state machine |
| [`ArenaDeathTracker.cs`](Bluelock/Services/ArenaDeathTracker.cs) | **Track deaths** in arena |
| [`ZoneBossSpawnerService.cs`](Bluelock/Services/ZoneBossSpawnerService.cs) | **Spawn bosses** on zone entry |
| [`TemplateRepository.cs`](Bluelock/Services/TemplateRepository.cs) | **Template storage** and lookup |
| [`ZoneTrackingService.cs`](Bluelock/Services/ZoneTrackingService.cs) | **Track players** in zones |
| [`SchematicZoneService.cs`](Bluelock/Services/SchematicZoneService.cs) | **Load schematics** for zones |
| [`PrefabsCatalog/`](Bluelock/PrefabsCatalog/) | **Prefab reference** database |

### Zone Lifecycle (`Bluelock/Services/Lifecycle/`)

| Service | Purpose |
|---------|---------|
| [`SpellbookLifecycleService.cs`](Bluelock/Services/Lifecycle/SpellbookLifecycleService.cs) | **Spell management** on zone enter/exit |
| [`VBloodLifecycleService.cs`](Bluelock/Services/Lifecycle/VBloodLifecycleService.cs) | **VBlood state** preservation |

### Core Zone Logic (`Bluelock/Core/`)

| File | Purpose |
|------|---------|
| [`ZoneCore.cs`](Bluelock/Core/ZoneCore.cs) | **Main zone logic**: detection, transitions, state |
| [`LifecycleCore.cs`](Bluelock/Core/LifecycleCore.cs) | **Lifecycle actions**: enter/exit steps |
| [`PrefabResolver.cs`](Bluelock/Core/PrefabResolver.cs) | **Prefab resolution** from names/GUIDs |
| [`PrefabReferenceCatalog.cs`](Bluelock/Core/PrefabReferenceCatalog.cs) | **Catalog of prefab** references |
| [`Arena/ArenaLifecycleManager.cs`](Bluelock/Core/Arena/ArenaLifecycleManager.cs) | **Arena lifecycle** coordination |

### Models (`Bluelock/Models/`)

| Model | Purpose |
|-------|---------|
| [`ZoneDefinition.cs`](Bluelock/Models/ZoneDefinition.cs) | Zone configuration schema |
| [`ZoneLifecycleConfig.cs`](Bluelock/Models/ZoneLifecycleConfig.cs) | Lifecycle action config |
| [`ZoneAbilityConfig.cs`](Bluelock/Models/ZoneAbilityConfig.cs) | Ability bar config |
| [`ZoneBorderConfig.cs`](Bluelock/Models/ZoneBorderConfig.cs) | Border/glow config |
| [`TemplateSnapshot.cs`](Bluelock/Models/TemplateSnapshot.cs) | Spawn template data |
| [`ZonesConfig.cs`](Bluelock/Models/ZonesConfig.cs) | Multi-zone config |

### Configuration Files

| File | Purpose |
|------|---------|
| `Bluelock/config/VAuto.Zones.json` | Zone definitions (positions, radii, settings) |
| `Bluelock/config/VAuto.ZoneLifecycle.json` | Lifecycle actions (enter/exit steps) |
| `Bluelock/config/VAuto.Kits.json` | Kit definitions (gear sets) |
| `Bluelock/config/ability_zones.json` | Zone-specific abilities |
| `Bluelock/config/ability_prefabs.json` | Ability prefab mappings |
| `Bluelock/config/Prefabsref.json` | Prefab reference data |

### Patches (`Bluelock/Patches/`)

| Patch | Purpose |
|-------|---------|
| [`DropInventorySystemPatch.cs`](Bluelock/Patches/DropInventorySystemPatch.cs) | **Suppress loot drops** in arena areas |

---

## 3. CycleBorn / VLifecycle Plugin (`gg.coyote.lifecycle`)

Lifecycle and progression management plugin. Handles player state preservation, arena transitions, respawn handling, and sandbox mode.

### Plugin Entry Point

| File | Purpose |
|------|---------|
| [`CycleBorn/Plugin.cs`](CycleBorn/Plugin.cs) | **Main plugin** - config, commands, lifecycle |
| [`CycleBorn/README.md`](CycleBorn/README.md) | Plugin documentation |

### Lifecycle Services (`CycleBorn/Services/Lifecycle/`)

| Service | Purpose |
|---------|---------|
| [`RespawnPreventionPatches.cs`](CycleBorn/Services/Lifecycle/RespawnPreventionPatches.cs) | **Prevent unintended respawns** in arena |
| [`AnnouncementService.cs`](CycleBorn/Services/Lifecycle/AnnouncementService.cs) | **Broadcast messages** on events |

### Configuration

| File | Purpose |
|------|---------|
| `config/VAuto.Lifecycle.json` | Unified lifecycle config (sandbox, stages, transitions) |

### Lifecycle Actions (JSON Config)

```json
{
  "lifecycle": {
    "arena": { "saveInventory", "restoreInventory", "saveBuffs", "restoreBuffs" },
    "playerState": { "saveEquipment", "saveBlood", "saveSpells", "saveHealth" },
    "respawn": { "forceArenaRespawn", "teleportToArenaSpawn", "clearTemporaryDebuffs" },
    "transitions": { "enterDelayMs", "exitDelayMs", "lockMovementDuringTransition" },
    "safety": { "restoreOnError", "blockEntryOnSaveFailure" }
  },
  "sandbox": {
    "enabled": true,
    "autoApplyUnlocks": true,
    "suppressVBloodFeed": true
  }
}
```

---

## 4. Configuration Module (`Configuration/`)

Centralized configuration management with JSON + CFG support, validation, migration, and hot reload.

### Key Files

| File | Purpose |
|------|---------|
| [`Configuration/VAutoConfigService.cs`](Configuration/VAutoConfigService.cs) | **Main config service** with hot reload |
| [`Configuration/IVAutoConfigService.cs`](Configuration/IVAutoConfigService.cs) | Service interface |
| [`Configuration/JsonConfigManager.cs`](Configuration/JsonConfigManager.cs) | JSON file management |
| [`Configuration/UnifiedZoneLifecycleConfig.cs`](Configuration/UnifiedZoneLifecycleConfig.cs) | Unified lifecycle schema |
| [`Configuration/ZoneLifecycleConfig.cs`](Configuration/ZoneLifecycleConfig.cs) | Zone-specific config |
| [`Configuration/PluginManifest.cs`](Configuration/PluginManifest.cs) | Plugin metadata |

### Configuration APIs

```csharp
VAutoConfigService.Register<T>("moduleName", "1.0")
VAutoConfigService.GetConfig<T>("moduleName")
VAutoConfigService.Reload("moduleName")
VAutoConfigService.RegisterValidator<T>(validator)
VAutoConfigService.RegisterMigrator<T>(migrator)
VAutoConfigService.RegisterReloadListener<T>(listener)
```

### Config Schema

```json
// config/VAuto.unified_config.schema.json
{
  "VAuto.Zone": { /* Zone config */ },
  "VAuto.Lifecycle": { /* Lifecycle config */ },
  "VAuto.Kits": { /* Kit definitions */ }
}
```

---

## 5. Chat Module (`Chat/`)

Chat and messaging utilities for server-wide announcements.

### Key Files

| File | Purpose |
|------|---------|
| [`Chat/ChatService.cs`](Chat/ChatService.cs) | **Message broadcasting** service |

### APIs

```csharp
ChatService.TryBroadcastSystemMessage(message, out error)
ChatService.TrySendSystemMessage(platformId, message, out error)
```

---

## 6. ZUtility API (`ZUtility/`)

REST API plugin for external integrations and server administration.

### Key Files

| File | Purpose |
|------|---------|
| [`ZUtility/API/PlayerAPI.cs`](ZUtility/API/PlayerAPI.cs) | **Player endpoints** |

### REST Endpoints

| Endpoint | Method | Purpose |
|----------|--------|---------|
| `/test` | GET | Health check |
| `/players` | GET | List all players |
| `/player/{id}` | GET | Get player details by userIndex |

### Response Types

```csharp
PlayersListResponse { List<ApiPlayerDetails> Players, int Count }
ApiPlayerDetails { 
  userIndex, characterName, steamID, clanId, gearLevel,
  lastValidPositionX/Y, timeLastConnected, isBot, isAdmin, isConnected 
}
```

---

## 7. Core Systems (`Core/`)

Shared systems used by all plugins.

### Commands (`Core/Commands/`)

| File | Purpose |
|------|---------|
| [`Core/Commands/CommandBase.cs`](Core/Commands/CommandBase.cs) | **Base command class** with permission/cooldown handling |
| [`Core/Commands/ChatColor.cs`](Core/Commands/ChatColor.cs) | Chat color constants |
| [`Core/Commands/CommandException.cs`](Core/Commands/CommandException.cs) | Command exceptions |

### ECS Utilities (`Core/ECS/`)

| File | Purpose |
|------|---------|
| [`Core/ECS/EntityExtensions.cs`](Core/ECS/EntityExtensions.cs) | Entity helper methods |
| [`Core/ECS/EntityQueryHelper.cs`](Core/ECS/EntityQueryHelper.cs) | Safe entity querying |
| [`Core/ECS/PrefabGUIDExtensions.cs`](Core/ECS/PrefabGUIDExtensions.cs) | PrefabGUID helpers |

### Logging (`Core/Logging/`)

| File | Purpose |
|------|---------|
| [`Core/Logging/CoreLogger.cs`](Core/Logging/CoreLogger.cs) | **Centralized logger** with source tagging |
| [`Core/Logging/CoreLoggerExtensions.cs`](Core/Logging/CoreLoggerExtensions.cs) | Logger extensions |

### Lifecycle (`Core/Lifecycle/`)

| File | Purpose |
|------|---------|
| [`Core/Lifecycle/IZoneEnterStep.cs`](Core/Lifecycle/IZoneEnterStep.cs) | Zone enter step interface |
| [`Core/Lifecycle/IZoneExitStep.cs`](Core/Lifecycle/IZoneExitStep.cs) | Zone exit step interface |
| [`Core/Lifecycle/IZoneLifecycleContext.cs`](Core/Lifecycle/IZoneLifecycleContext.cs) | Lifecycle context |
| [`Core/Lifecycle/IZoneLifecycleStepRegistry.cs`](Core/Lifecycle/IZoneLifecycleStepRegistry.cs) | Step registry |

### Events (`Core/Events/`)

| File | Purpose |
|------|---------|
| [`Core/Events/LifecycleEvents.cs`](Core/Events/LifecycleEvents.cs) | Lifecycle event definitions |
| [`Core/Events/PatchEvents.cs`](Core/Events/PatchEvents.cs) | Patch event definitions |
| [`Core/Events/TypedEventBus.cs`](Core/Events/TypedEventBus.cs) | Type-safe event bus |

### Systems (`Core/Systems/`)

| File | Purpose |
|------|---------|
| [`Core/Systems/LifecycleEventBridgeSystem.cs`](Core/Systems/LifecycleEventBridgeSystem.cs) | Bridge ECS events to lifecycle |

### Config (`Core/Config/`)

| File | Purpose |
|------|---------|
| [`Core/Config/ConfigService.cs`](Core/Config/ConfigService.cs) | Config loading utilities |
| [`Core/Config/JsonConverters.cs`](Core/Config/JsonConverters.cs) | JSON serialization helpers |
| [`Core/Config/LifecycleConfigWatcher.cs`](Core/Config/LifecycleConfigWatcher.java) | Config file monitoring |

### Sandbox Services (`Core/Services/Sandbox/`)

| File | Purpose |
|------|---------|
| [`Core/Services/SandboxSnapshotStore.cs`](Core/Services/SandboxSnapshotStore.cs) | **Player state storage** |
| [`Core/Services/SandboxDeltaComputer.cs`](Core/Services/SandboxDeltaComputer.cs) | Compute state differences |
| [`Core/Services/SandboxCsvWriter.cs`](Core/Services/SandboxCsvWriter.java) | CSV export |
| [`Core/Services/ServiceInitializer.cs`](Core/Services/ServiceInitializer.cs) | **Service bootstrap** |

### Data (`Core/Data/`)

| File | Purpose |
|------|---------|
| [`Core/Data/TechTeachCatalog.cs`](Core/Data/TechTeachCatalog.cs) | Technology teach data |

---

## 8. Data Files (`Bluelock/Data/`)

Static game data references for prefabs, abilities, items, etc.

| File | Purpose |
|------|---------|
| `Bluelock/Data/datatype/VBloods.cs` | VBlood prefab references |
| `Bluelock/Data/datatype/Abilities.cs` | Ability prefab references |
| `Bluelock/Data/datatype/Weapons.cs` | Weapon prefab references |
| `Bluelock/Data/datatype/Armors.cs` | Armor prefab references |
| `Bluelock/Data/datatype/Amulets.cs` | Amulet prefab references |
| `Bluelock/Data/datatype/Chains.cs` | Blood type chains |
| `Bluelock/Data/datatype/Tiles.cs` | Tile/schematic references |
| `Bluelock/Data/datatype/Spells.cs` | Spell prefab references |
| `Bluelock/Data/datatype/WeaponSkills.cs` | Weapon skill references |

---

## 9. Build & Scripts (`scripts/`)

| File | Purpose |
|------|---------|
| `scripts/build-all-mods.ps1` | **Build all plugins** (Core, Bluelock, CycleBorn, ZUtility) |

---

## 10. Project Files

| File | Purpose |
|------|---------|
| `VAutomationCore.csproj` | Core framework project |
| `Bluelock/VAutoZone.csproj` | Bluelock plugin project |
| `CycleBorn/Vlifecycle.csproj` | CycleBorn project |
| `Directory.Build.props` | MSBuild configuration |
| `GlobalUsings.cs` | Global using statements |
| `manifest.json` | Mod manifest |
| `PrefabGuidConverter.cs` | PrefabGUID conversion utilities |
| `VAutoLogger.cs` | Logger wrapper |

---

## Architecture Summary

```
┌─────────────────────────────────────────────────────────────┐
│                      V Rising Server                        │
└─────────────────────────────────────────────────────────────┘
                              │
         ┌────────────────────┼────────────────────┐
         ▼                    ▼                    ▼
┌─────────────┐      ┌─────────────┐      ┌─────────────┐
│  Bluelock   │      │ CycleBorn   │      │  ZUtility   │
│   Plugin    │      │   Plugin    │      │  (REST API) │
└─────────────┘      └─────────────┘      └─────────────┘
         │                    │                    
         └────────┬───────────┘                    
                  ▼                                    
┌─────────────────────────────────────────────────────────────┐
│              VAutomationCore (Core Framework)               │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐    │
│  │UnifiedCore│ │GameAction│ │  Command │ │  Config  │    │
│  │          │ │ Service  │ │  Base    │ │ Service  │    │
│  └──────────┘ └──────────┘ └──────────┘ └──────────┘    │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐                 │
│  │  ECS     │ │ Lifecycle│ │  Event   │                 │
│  │ Utilities│ │  Steps   │ │   Bus    │                 │
│  └──────────┘ └──────────┘ └──────────┘                 │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
              ┌───────────────────────────────┐
              │   V Rising ECS (Unity DOTS)   │
              │   - EntityManager             │
              │   - PrefabCollectionSystem    │
              │   - GameSystems               │
              └───────────────────────────────┘
```

---

## Dependencies

- **BepInEx 6.x** - Plugin framework
- **VampireCommandFramework** - Chat commands (`gg.deca.VampireCommandFramework`)
- **Harmony** - Patching
- **Unity.Entities** - ECS access
- **ProjectM** - V Rising game APIs

---

## Command Quick Reference

| Command | Plugin | Description |
|---------|--------|-------------|
| `.z`, `.zone` | Bluelock | Zone management |
| `.template` | Bluelock | Spawn templates |
| `.match` | Bluelock | Arena matches |
| `.enter` | Bluelock | Quick enter zone |
| `.exit` | Bluelock | Quick exit zone |
| `.lifecycle`, `.lc` | CycleBorn | Lifecycle control |
| REST `/players` | ZUtility | List players |
| REST `/player/{id}` | ZUtility | Player details |
