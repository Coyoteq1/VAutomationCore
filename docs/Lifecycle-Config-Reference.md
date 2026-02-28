# Lifecycle Config Reference

This page documents lifecycle-related JSON config and runtime flags used by the current system.

Primary files:
- `VAuto.Lifecycle.json` under BepInEx config path (CycleBorn runtime config).
- `Bluelock/config/VAuto.ZoneLifecycle.json` (zone-to-lifecycle action mapping).
- `Bluelock/config/VAuto.Zones.json` (zone geometry + flow IDs).

Config model source:
- `CycleBorn/Plugin.cs`

## Top-level schema

- `version`
- `schemaVersion`
- `lifecycle`
- `sandbox`
- `stages`

For `Bluelock/config/VAuto.ZoneLifecycle.json`:
- `schemaVersion`
- `configVersion`
- `enabled`
- `mappings`

## `lifecycle` section

### `lifecycle.enabled`
- Enables lifecycle module behavior.

### `lifecycle.arena`
- `saveInventory`
- `restoreInventory`
- `saveBuffs`
- `restoreBuffs`
- `clearArenaBuffsOnExit`
- `resetAbilityCooldowns`
- `resetCooldownsOnExit`

### `lifecycle.playerState`
- `saveEquipment`
- `saveBlood`
- `saveSpells`
- `saveHealth`
- `restoreHealth`

### `lifecycle.respawn`
- `forceArenaRespawn`
- `teleportToArenaSpawn`
- `clearTemporaryDebuffs`
- `respawnTeleportDelayMs`

### `lifecycle.transitions`
- `enterDelayMs`
- `exitDelayMs`
- `lockMovementDuringTransition`
- `showTransitionMessages`

### `lifecycle.safety`
- `restoreOnError`
- `blockEntryOnSaveFailure`
- `verboseLogging`

### `lifecycle.integration`
- `zoneTriggersLifecycle`
- `allowTrapOverrides`
- `sendLifecycleEvents`

## `sandbox` section

- `enabled`
- `autoApplyUnlocks`
- `suppressVBloodFeed`
- `despawnDelaySeconds`
- `defaultArenaId`
- `experimentalEnabled`
- `experimentalAutoApplyUnlocks`
- `experimentalSuppressVBloodFeed`
- `experimentalDespawnDelaySeconds`
- `experimentalArenaId`
- `experimentalArenaAliases`
- `routeArenaById`
- `experimentalArenaIdMatchToken`

## `stages` section

- `onEnter.enabled`
- `onEnter.description`
- `isInZone.enabled`
- `isInZone.description`
- `onExit.enabled`
- `onExit.description`

## Runtime accessor mapping

Examples in `CycleBorn/Plugin.cs`:
- `Plugin.SandboxEnabled`
- `Plugin.SandboxAutoApplyUnlocks`
- `Plugin.SandboxSuppressVBloodFeed`
- `Plugin.SandboxDespawnDelaySeconds`
- `Plugin.StageOnEnterEnabled`
- `Plugin.StageIsInZoneEnabled`
- `Plugin.StageOnExitEnabled`

Examples in `Bluelock/Plugin.cs`:
- `Runtime.ZoneRuntimeMode` (`Legacy`, `Hybrid`, `EcsOnly`)
- `Runtime.EcsDetectionTickSeconds`
- `Runtime.ZoneDetectionOpsWarningThreshold`

## Migration notes

Legacy source:
- `VAutoZone/config/sandbox_defaults.json`

Migration helper:
- `LifecycleConfigMigration.RunMigration(...)` in `CycleBorn/Plugin.cs`
