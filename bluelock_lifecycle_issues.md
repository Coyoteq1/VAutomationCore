# BlueLock + Lifecycle Issue Tracker

Last updated: 2026-02-19

## Resolved

1) ArenaBuild loadout command fails - **RESOLVED**
- Symptom: `[KitService] ArenaBuild apply failed ... give_build ... Unmatched`
- Root cause: ArenaBuild command is not being matched at runtime.
- Fix applied: Added command fallback attempts: `give_build` + `giveb`, and `clear_build` + `clearb`. Default build id changed to `brute`.
- Files: `Bluelock/Services/KitService.cs`

## Verified from latest logs

2) Curl/TLS certificate failures
- Symptom: `Curl error 60: Cert verify failed. Certificate has expired.`
- Scope: external HTTPS endpoint(s), not BlueLock core logic.
- Action:
  - Check system clock/timezone.
  - Check Windows certificate store updates.
  - Identify which plugin/endpoint is calling HTTPS and renew/fix cert chain.

3) PrimaryAttackSpeed out of range
- Symptom: `UserControllerData_PrimaryAttackSpeed Value: 10, Max: 4`
- Scope: gameplay stat sync warning from buff/stat modifiers.
- Action:
  - Clamp attack speed modifiers to <= 4 in the mod/system setting this value.

4) Glow tile prefab unavailable
- Symptom: `Glow auto spawn failed ... prefab not configured or unavailable`
- Scope: zone glow config resolution.
- Action:
  - Validate zone glow prefab ids/names in:
    - `BepInEx/config/Bluelock/VAuto.Zones.json`
    - `BepInEx/config/Bluelock/Prefabsref.json`

5) AbilityUi defers preset slots on enter
- Symptom: `Deferring preset slots ... not readable yet`
- Scope: timing race on zone enter.
- Action:
  - Keep as warning unless persistent; this is often transient and resolves on next tick.

6) Rapid enter/exit around zone boundary
- Symptom: repeated enter/exit events and snapshot save/restore loops.
- Action:
  - Increase zone hysteresis/cooldown or reduce teleport bounce at zone edge.

7) Sandbox progression journal file format issue
- Symptom: Issues with `sandbox_progression_journal.jsonl` file.
- Scope: State persistence/sandbox system.
- Action:
  - Verify the file is valid JSONL format (one JSON object per line).
  - Check for parsing errors in sandbox state loading/saving.
  - Files:
    - `Core/Services/SandboxSnapshotStore.cs`
    - Core/Services/SandboxPersistenceService.cs`

8) Lifecycle flow broken - only kits and give commands work
- Symptom: Zone enter/exit only executes kits and `give` commands. No bosses, tiles, units, restore items, clear items, or glow are spawning.
- Scope: Complete lifecycle step failure on zone enter.
- What works: Kit application, give commands (via KitService)
- What is broken:
  - Boss spawning (ZoneBossSpawnerService)
  - Tile/structure spawning (ZoneStructureLoader, SchematicZoneService)
  - Item restoration (sandbox snapshot restore)
  - Item clearing
  - Glow border rendering (GlowTileService, ZoneGlowBorderService)
- Root cause: Multiple issues found:
  1. **Config vs Code mismatch**: `VAuto.ZoneLifecycle.json` uses `apply_kit` but Plugin.cs expects `kit_apply`
     - Config line 12: `"apply_kit"`
     - Plugin.cs line 2528: `case "kit_apply":`
  2. **Missing glow_spawn**: `glow_spawn` is not included in onEnter actions
     - Glow steps exist in Plugin.cs (lines 2548-2556) but config doesn't reference them
  3. **Empty templates**: All zones have `"templates": {}` (empty object) in VAuto.Zones.json
     - Lines 55, 94, 133, 172, 211 all show `"templates": {}`
     - ApplyZoneTemplatesOnEnter exits early when templates are empty
     - This explains why no bosses, tiles, or units spawn
  4. **Action tokens may be failing silently**: TryRunZoneEnterStep catches exceptions but may not be logging all failures
- Files to investigate:
  - `Bluelock/config/VAuto.ZoneLifecycle.json`
  - `Bluelock/Plugin.cs` lines 2509-2622 (action token switch)
  - `Bluelock/Services/ZoneBossSpawnerService.cs`
  - `Bluelock/Services/ZoneStructureLoader.cs`
  - `Bluelock/Services/GlowTileService.cs`

## Immediate checklist

- [ ] If TLS errors continue, identify offending endpoint/plugin and rotate certificate.
- [ ] Validate glow prefab mapping for zone `1`.
- [ ] **FIX ISSUE #8**: Add templates to zone configs OR remove empty templates check
- [ ] **FIX ISSUE #8**: Add `glow_spawn` to onEnter actions in VAuto.ZoneLifecycle.json
- [ ] **FIX ISSUE #8**: Change `apply_kit` to `kit_apply` in VAuto.ZoneLifecycle.json OR add fallback in Plugin.cs

## Current deployment note

BlueLock/Cycleborn/Core/Traps/Announcement were rebuilt and deployed to:
- `D:\DedicatedServerLauncher\VRisingServer\BepInEx\plugins`
