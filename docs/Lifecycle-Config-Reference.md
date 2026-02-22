# Lifecycle Config Reference

This page documents the unified lifecycle JSON config and key runtime flags.

Primary file:
- `VAuto.Lifecycle.json` under BepInEx config path.

Config model source:
- `CycleBorn/Plugin.cs`

## Top-level schema

- `version`
- `lifecycle`
- `sandbox`
- `stages`

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

## Migration notes

Legacy source:
- `VAutoZone/config/sandbox_defaults.json`

Migration helper:
- `LifecycleConfigMigration.RunMigration(...)` in `CycleBorn/Plugin.cs`
