# BlueLock + Lifecycle Issue Tracker

Last updated: 2026-02-18

## Verified from latest logs

1) ArenaBuild loadout command fails
- Symptom: `[KitService] ArenaBuild apply failed ... give_build ... Unmatched`
- Root cause: ArenaBuild command is not being matched at runtime.
- What was changed:
  - Added command fallback attempts: `give_build` + `giveb`, and `clear_build` + `clearb`.
  - Default build id changed to `brute` (was `EmptyDefault`).
- Files:
  - `BlueLock/Services/KitService.cs`

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

## Immediate checklist

- [ ] Confirm `ArenaBuilds.dll` is installed and loaded on startup if ArenaBuild path is required.
- [ ] In ArenaBuild `builds.json`, ensure build key `brute` exists.
- [ ] Re-test zone enter in zone `1` and confirm no `Unmatched` command warnings.
- [ ] If TLS errors continue, identify offending endpoint/plugin and rotate certificate.
- [ ] Validate glow prefab mapping for zone `1`.

## Current deployment note

BlueLock/Cycleborn/Core/Traps/Announcement were rebuilt and deployed to:
- `D:\DedicatedServerLauncher\VRisingServer\BepInEx\plugins`
