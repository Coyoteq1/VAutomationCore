# BLUELOCK.md — BlueLock Quick Reference

**Plugin GUID:** `gg.coyote.BlueLock`  
**Namespace:** `VAuto.Zone`  
**Config dir:** `BepInEx/config/Bluelock/`

## Essential Commands

```
.z list                         — all zones
.z status <id>                  — zone detail + lifecycle status
.z on <id> / .z off <id>        — enable / disable zone
.z tp <id>                      — teleport to zone
.enter [zoneId]                 — force-enter zone (default if no ID)
.exit                           — force-exit current zone
.tm status <id>                 — template entity counts
.tm rebuild <id>                — clear + respawn all templates
.match start <id> [seconds]     — start arena match
.match end <id>                 — end match
```

## Config Files

| File | Purpose |
|------|---------|
| `Bluelock/VAuto.Zones.json` | Zone definitions |
| `Bluelock/VAuto.ZoneLifecycle.json` | Enter/exit step order |
| `Bluelock/VAuto.Kits.json` | Equipment & spell kits |
| `Bluelock/ability_prefabs.json` | Ability GUID aliases |
| `Bluelock/Prefabsref.json` | Full prefab name → GUID catalog |

## After Any Config Change

```
.tm clearall <zoneId>
.tm rebuild <zoneId>
```

## See Also

- Full documentation: [`README.md`](README.md)
- Lifecycle reference: [`LIFECYCLE.md`](LIFECYCLE.md)

