# VAutomationCore Quick Guide

## BlueLock
- Plugin: `BlueLock` (`gg.coyote.BlueLock`)
- Main cfg: `BepInEx/config/Bluelock/VAuto.Zone.cfg`
- Main json:
  - `BepInEx/config/Bluelock/VAuto.Zones.json`
  - `BepInEx/config/Bluelock/VAuto.ZoneLifecycle.json`
  - `BepInEx/config/Bluelock/ability_zones.json`
  - `BepInEx/config/Bluelock/ability_prefabs.json`
- Arena loadout source: `BepInEx/config/ArenaBuilds/builds.json`
- Commands:
  - Zone: `.z ...` / `.zone ...`
  - Template: `.template ...`
  - Match: `.match ...`
  - Quick: `.enter`, `.exit`

## lifecycle (CycleBorn)
- Plugin: `lifecycle` (`gg.coyote.lifecycle`)
- Main json: `BepInEx/config/VAuto.Lifecycle.json`
- Legacy json (migration source): `BepInEx/config/VAutoZone/config/sandbox_defaults.json`
- Commands: `.lifecycle ...` / `.lc ...`

## Notes
- `olddocs/` is ignored by git.
- Restart server after changing cfg/json or replacing plugin DLLs.
- Short module docs:
  - `BLUELOCK.md`
  - `LIFECYCLE.md`
