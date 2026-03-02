VAutomationCore — V Rising Server Modding Engine Framework 
A collection of BepInEx plugins for V Rising dedicated servers providing zone-based arenas, sandbox ability testing, template-driven structures, and lifecycle-controlled player state management boss pv3 ,pv4pv5 — all without touching world progression.

---

## Plugin Overview

| Plugin | GUID | Description |
|--------|------|-------------|
| **VAutomationCore** | `gg.coyote.VAutomationCore` | Shared core library — ECS utilities, config services, logging, event bus |
| **BlueLock** | `gg.coyote.BlueLock` | Zone management, arenas, sandbox, templates, kits, ability overrides |
| **lifecycle (CycleBorn)** | `gg.coyote.lifecycle` | Player state snapshot & restore engine |

Load order: `VAutomationCore` → `lifecycle` → `BlueLock`

---

## Dependencies

| Dependency | Version | Required By |
|-----------|---------|-------------|
| BepInEx IL2CPP | latest | All |
| VampireCommandFramework | `0.10.4` | BlueLock, lifecycle |
| VAutomationCore | `1.0.0` | BlueLock, lifecycle |
| lifecycle (CycleBorn) | `1.0.0` | BlueLock |

---

## Directory Structure

```
BepInEx/
├── plugins/
│   ├── VAutomationCore.dll
│   ├── BlueLock.dll          (VAutoZone)
│   └── lifecycle.dll         (CycleBorn)
└── config/
    ├── VAuto.Core.cfg         ← VAutomationCore general config
    ├── VAuto.Lifecycle.json   ← CycleBorn lifecycle config
    └── Bluelock/
        ├── VAuto.Zones.json          ← Zone definitions
        ├── VAuto.ZoneLifecycle.json  ← Zone enter/exit step mappings
        ├── VAuto.Kits.json           ← Equipment & spell kit definitions
        ├── ability_prefabs.json      ← Ability prefab GUID aliases
        ├── ability_zones.json        ← Ability zone settings
        └── Prefabsref.json           ← Full prefab name → GUID catalog
```

All Bluelock configuration is authoritative only when located under `BepInEx/config/Bluelock/`.  
Legacy paths from `VAuto.Zone/config/` are migrated on first run and can be deleted afterward.

---

## Features

### Zone System
- Circular zones with configurable center, radius, and shape
- Enable / disable zones independently
- Per-zone tags, display names, and enter / exit messages
- Automatic teleport on enter with return-to-origin on exit
- Zone detection via position polling (configurable interval and threshold)
- Default zone fallback for `.enter` with no argument

### Template System
Templates are **runtime-only** structures spawned inside a zone.  
They are visual and mechanical aids — they never affect zone logic or player state.

| Type | Purpose |
|------|---------|
| `template` | Core embedded structures |
| `border` | Perimeter markers |
| `floor` | Ground tile fills |
| `roof` | Overhead tiles |
| `glow` | Glow tile decoration |

Templates are zone-scoped, disposable, and safe to rebuild at any time.

### Arena System
- PvP arenas with death tracking and respawn handling
- Match lifecycle: start, end, reset
- Arena territory grid visualization
- Damage mode toggle per zone
- Holder immunity support

### Sandbox System
- Sandboxed ability and spell testing zones
- VBlood feed suppression
- No world progression mutation
- Auto-apply unlock kits
- Despawn timer for sandbox entities

### Player Lifecycle (via CycleBorn)
On **zone enter**: inventory snapshot, buff snapshot, equipment/blood/spells/health snapshot, kit grant, teleport, ability overrides  
On **zone exit**: kit restore, ability restore, teleport return, buff cleanup, cooldown reset  
On **error**: automatic rollback to last snapshot

---

## Configuration Files

### `VAuto.Zones.json`
Defines all zones. Key fields per zone:

```json
{
  "id": "1",
  "displayName": "Arena 1",
  "shape": "Circle",
  "centerX": -1000.0,
  "centerY": 0.0,
  "centerZ": -500.0,
  "radius": 45.0,
  "kitToApplyId": "Kit1",
  "teleportOnEnter": true,
  "returnOnExit": true,
  "enterMessage": "...",
  "exitMessage": "...",
  "templates": {},
  "tags": ["sandbox", "event_tier_1"]
}
```

### `VAuto.ZoneLifecycle.json`
Maps zone IDs (or `"*"` for all) to ordered step sequences:

```json
{
  "mappings": {
    "*": {
      "onEnter": ["snapshot_save", "apply_kit", "teleport_enter", "apply_abilities"],
      "onExit":  ["restore_kit_snapshot", "restore_abilities", "teleport_return"]
    }
  }
}
```

### `VAuto.Kits.json`
Defines named equipment and spell loadouts applied on zone entry.

### `ability_prefabs.json`
Maps friendly alias names to PrefabGUIDs for ability overrides.

### `Prefabsref.json`
Full catalog of prefab name → GUID mappings used by the template and spawn systems.

### `VAuto.Lifecycle.json` (CycleBorn)
Controls player state snapshot/restore behavior, respawn rules, and safety options.

---

## Chat Commands

### Zone Management — `.zone` / `.z`

| Command | Short | Description | Admin |
|---------|-------|-------------|-------|
| `.z help` | `.z h` | Show command help | No |
| `.z create [radius]` | `.z c` | Create zone at current position | Yes |
| `.z remove [id]` | `.z rem` | Remove zone by ID | Yes |
| `.z list` | `.z l` | List all zones | Yes |
| `.z on [id]` | `.z enable` | Enable zone | Yes |
| `.z off [id]` | `.z disable` | Disable zone | Yes |
| `.z center [id]` | `.z cen` | Move zone center to your position | Yes |
| `.z radius [id] [r]` | `.z r` | Set zone radius | Yes |
| `.z tp [id]` | `.z teleport` | Teleport to zone center | Yes |
| `.z status [id]` | `.z s` | Show zone details and lifecycle status | Yes |
| `.z diag` | `.z dg` | Live runtime diagnostics for your player | Yes |
| `.z default [id]` | `.z d` | Set default zone | Yes |
| `.z arena [id] [on/off]` | — | Toggle arena damage mode | Yes |
| `.z holder [id] [player\|clear]` | — | Set / clear holder immunity | Yes |
| `.z kit verify [id]` | — | Verify kit resolution (no items granted) | Yes |
| `.z kit verifykit [kitId]` | — | Verify kit by ID (no items granted) | Yes |

### Template Management — `.template` / `.tm`

| Command | Short | Description | Admin |
|---------|-------|-------------|-------|
| `.tm list [zoneId]` | `.tm l` | List templates and spawn status | Yes |
| `.tm spawn [zoneId] [type]` | `.tm s` | Spawn a specific template type | Yes |
| `.tm spawnall [zoneId]` | `.tm sa` | Spawn all configured templates | Yes |
| `.tm clear [zoneId] [type]` | `.tm c` | Clear a specific template type | Yes |
| `.tm clearall [zoneId]` | `.tm ca` | Clear all templates in zone | Yes |
| `.tm rebuild [zoneId]` | `.tm rb` | Clear then respawn all templates | Yes |
| `.tm status [zoneId]` | `.tm st` | Show entity counts per template type | Yes |

### Match Control — `.match` / `.m`

| Command | Short | Description | Admin |
|---------|-------|-------------|-------|
| `.match start [zoneId] [duration]` | `.m s` | Start a match (default 300s) | Yes |
| `.match end [zoneId]` | `.m e` | End current match | Yes |
| `.match reset [zoneId]` | `.m r` | Reset arena state | Yes |

### Quick Enter / Exit

| Command | Short | Description | Admin |
|---------|-------|-------------|-------|
| `.enter [zoneId]` | `.en` | Enter zone (uses default if no ID) | Yes |
| `.exit` | `.ex` | Exit current zone | Yes |

### Spawn — `.spawn` / `.sp`

| Command | Short | Description | Admin |
|---------|-------|-------------|-------|
| `.sp unit [prefab] [count] [level] [spread]` | `.sp u` | Spawn units near you | Yes |
| `.sp boss [prefab] [level]` | `.sp b` | Spawn boss near you | Yes |

### Zone Tags — `.tag`

| Command | Short | Description | Admin |
|---------|-------|-------------|-------|
| `.tag rename [tag]` | `.tag r` | Rename your own zone tag | No |
| `.tag clear` | — | Clear your zone tag | No |
| `.tag list` | `.tag l` | List all active zone tags | Yes |
| `.tag set [player] [tag]` | — | Set another player's tag | Yes |

### Utilities

| Command | Description | Admin |
|---------|-------------|-------|
| `.unlockprefab [vbloodName]` | Unlock VBlood Tech_Collection prefab for testing | Yes |

---

## Template Lifecycle

```
Zone Definition (VAuto.Zones.json)
       |
       v
.tm spawnall [zoneId]
       |
       v
PrefabResolver resolves aliases → PrefabGUIDs
       |
       v
ECS entities spawned (runtime only)
       |
       v
Registered in ZoneTemplateRegistry
       |
       v
.tm clearall [zoneId]   ← destroys all, unregisters
       |
       v
.tm rebuild [zoneId]    ← clearall + spawnall in one step
```

**Rules:**
- Templates are never persisted
- Templates are zone-scoped
- Templates never affect ownership, PvP rules, or zone logic
- Clearing templates never deletes zones
- Rebuild after any config change or server migration

---

## Player Lifecycle Flow

```
Player enters zone boundary
       |
       v
OnEnter steps (ordered, from VAuto.ZoneLifecycle.json)
  ├─ capture_return_position
  ├─ snapshot_save          ← CycleBorn captures full state
  ├─ zone_enter_message
  ├─ apply_kit
  ├─ teleport_enter
  ├─ apply_templates
  ├─ apply_abilities
  ├─ player_tag
  ├─ boss_enter
  └─ integration_events_enter

Player exits zone boundary
       |
       v
OnExit steps (ordered)
  ├─ zone_exit_message
  ├─ restore_kit_snapshot   ← CycleBorn restores full state
  ├─ restore_abilities
  ├─ boss_exit
  ├─ player_tag
  ├─ teleport_return
  └─ integration_events_exit

Error at any step
       |
       v
Automatic rollback to last snapshot (if safety.restoreOnError = true)
```

---

---

## Sandbox System — Isolation Guarantee

Sandbox zones are **sealed simulation environments**. Players test spells, abilities, kits, and units without ever touching world progression or leaking state outside the zone.

### No Progression Leakage

Sandbox zones **never modify** the persistent world:

- **VBlood unlocks are suppressed** — feeding a VBlood in sandbox does nothing; unlocks are discarded
- **No world progression is modified** — no tech, spell, crafting, or research unlocks
- **Ability overrides are zone-scoped** — restored from snapshot on exit
- **Inventory and buffs are temporary** — all restored on exit
- **Sandbox entities despawn automatically** — units, bosses, and props are cleaned up after a configurable delay

### Sandbox-Safe Commands

These commands are safe because lifecycle snapshots guarantee full reversibility:

| Command | Why It Is Safe |
|---------|---------------|
| `.enter <sandbox>` | Saves full player snapshot before applying sandbox rules |
| `.exit` | Restores snapshot — removes all sandbox effects |
| `.sp unit` / `.sp boss` | Spawns runtime-only entities that auto-despawn |
| `.unlockprefab` | Grants temporary unlock for testing; never persisted |

### Sandbox Flow

```
Player enters sandbox zone
       |
       v
CycleBorn: snapshot_save  (inventory, equipment, spells, buffs, blood, health, cooldowns, return position)
       |
       v
Apply sandbox rules
  - Ability overrides active
  - Kit applied
  - Teleport to sandbox origin
  - VBlood feed suppressed
  - Progression writes blocked
       |
       v
Player performs tests
(.sp unit / .sp boss / .unlockprefab / ability use)
       |
       v
Player exits / .exit
       |
       v
CycleBorn: snapshot_restore  (all fields restored exactly)
       |
       v
Cleanup
  - Sandbox entities despawned
  - Sandbox buffs removed
  - Abilities restored
  - Inventory restored
       |
       v
Player returns to world state — unchanged
```

### Three Layers of Protection

| Layer | Mechanism | What It Prevents |
|-------|-----------|-----------------|
| **Lifecycle Snapshot** | CycleBorn captures full player state before entry | State leakage on crash or error |
| **Zone Isolation** | Zone overrides VBlood feed, ability unlocks, progression writes, entity persistence | World mutation |
| **Automatic Cleanup** | All sandbox entities tagged, tracked, and despawned on a timer | Orphaned ECS entities, startup pressure |

If any restore step fails, [`safety.restoreOnError = true`](BepInEx/config/VAuto.Lifecycle.json) forces rollback to the last valid snapshot.

> **Sandbox is a sealed simulation: everything inside is temporary, reversible, and isolated from the world.**

---

## Post-Refactor Operational Notes

### Required After Any Config or Code Migration

1. Stop the server completely
2. Verify all config files exist under `BepInEx/config/Bluelock/`
3. Delete any legacy config folders: `VAuto.Zone/`, `CycleBorn/` (under config)
4. Start server
5. Run per-zone template rebuild:
   ```
   .tm clearall <zoneId>
   .tm rebuild <zoneId>
   ```
6. Verify with `.z status <zoneId>` and `.tm status <zoneId>`

### Startup Validation Checklist

- [ ] No repeated `Still waiting for world startup` log lines after 2 minutes
- [ ] Patch activation messages appear in BepInEx log on load
- [ ] `.z list` returns expected zones
- [ ] `.tm status <zoneId>` shows spawned entity counts
- [ ] `.enter` / `.exit` correctly save and restore player state
- [ ] No prefab resolve warnings in log

### Known Critical Issues (Pending Fix)

| Severity | Location | Issue |
|----------|----------|-------|
| CRITICAL | [`Plugin.cs`](Plugin.cs) `Load()` | Core patches are not loaded — `PatchAll` is called on a non-existent `typeof(Patches)` and a reflection lookup that will always fail after namespace refactor |
| WARNING | [`Bluelock/Plugin.cs`](Bluelock/Plugin.cs) ~line 2948 | References deleted `VAuto.Core.Lifecycle.ArenaLifecycleManager` — null guard prevents crash but arena lifecycle bridge is inactive |

**Patch bootstrap fix:**
```csharp
// In Bluelock/Plugin.cs Load()
_harmony = new Harmony("gg.coyote.BlueLock");
_harmony.PatchAll(typeof(Plugin).Assembly);
```

---

## Design Principles

Bluelock is built on four core principles. Each principle defines how a system behaves, which commands implement it, and what flow enforces it.

---

### 1. Zones Define Space

Zones are the spatial contract of Bluelock. They determine **where** special rules apply — not what happens inside them.

- A zone is a geometric boundary: circle, center, radius
- Entering or exiting a zone triggers lifecycle steps
- Zones do not store gameplay state; they only declare territory

**Commands:**
```
.z create    .z center    .z radius
.z on        .z off       .z tp
.z default   .z status
```

**Flow:**
```
Player moves → Position check → Zone boundary crossed → Lifecycle triggered
```

---

### 2. Templates Define Structure

Templates are runtime-only structures spawned inside a zone. They define what the player sees and interacts with — they never affect zone logic or progression.

- Templates are disposable and zone-scoped
- Templates never persist across restarts
- Templates never modify player state

**Commands:**
```
.tm spawn     .tm spawnall
.tm clear     .tm clearall
.tm rebuild   .tm status
```

**Flow:**
```
Zone config → Template definitions → PrefabResolver → ECS entities spawned
                                                            |
                                                     .tm clearall
                                                            |
                                                     .tm spawnall
                                                            |
                                                     Fresh structures
```

---

### 3. Lifecycle Defines Safety

Lifecycle is the state contract of Bluelock. It ensures that entering a zone is **safe, reversible, and deterministic**.

- Player state is snapshotted on enter (via CycleBorn)
- Player state is restored on exit
- Errors trigger automatic rollback
- No state leaks between zones

**On Enter steps** (ordered from [`VAuto.ZoneLifecycle.json`](BepInEx/config/Bluelock/VAuto.ZoneLifecycle.json)):
```
snapshot_save → apply_kit → teleport_enter → apply_templates → apply_abilities
```

**On Exit steps:**
```
restore_kit_snapshot → restore_abilities → teleport_return → integration_events_exit
```

**Flow:**
```
Enter zone → Save snapshot → Apply zone rules → Player acts
Exit zone  → Restore snapshot → Return to world state (unchanged)
Error      → restoreOnError = true → Rollback to last snapshot
```

**Snapshot captures:**
Inventory · Equipment · Spells · Buffs · Blood type · Health · Cooldowns · Return position

---

### 4. Sandbox Never Mutates the World

Sandbox zones are fully isolated from world progression. See the full [Sandbox System](#sandbox-system--isolation-guarantee) section above.

- No VBlood unlocks persist
- No world progression is modified
- No abilities or spells leak outside
- Sandbox entities despawn automatically

**Commands:**
```
.enter <sandbox>    .exit
.sp unit            .sp boss
.unlockprefab
```

---

### Unified Flow Summary

```
Player moves
    |
    v
Zone detection
    |
    v
Zone enter
    |
    v
Lifecycle: snapshot_save
    |
    v
Templates spawn (if configured)
    |
    v
Kits, abilities, buffs applied
    |
    v
Player interacts inside zone
(Sandbox adds: VBlood suppression, ability overrides, despawn timers, no progression mutation)
    |
    v
Zone exit
    |
    v
Lifecycle: restore_snapshot
    |
    v
Templates cleared (if configured)
    |
    v
Player returned to world state — unchanged
```

---

### Clear Commands and Their Purpose

| Command | Purpose |
|---------|---------|
| `.tm clear <zone> <type>` | Remove one template type |
| `.tm clearall <zone>` | Remove all templates |
| `.match reset <zone>` | Reset arena state |
| `.exit` | Force lifecycle exit and restore snapshot |
| `.z off <zone>` | Disable zone and stop lifecycle triggers |

These commands are safe because lifecycle guarantees reversibility.

---

### Additional Principles

- **Single lifecycle engine** — CycleBorn is the only source of player state truth; no parallel restore paths
- **Config lives in one place** — `BepInEx/config/Bluelock/` only; legacy paths migrate on first run
- **Patches load by assembly** — never by fragile type-name reflection

---

## Module Documentation

| File | Content |
|------|---------|
| [`BLUELOCK.md`](BLUELOCK.md) | BlueLock quick-reference |
| [`LIFECYCLE.md`](LIFECYCLE.md) | CycleBorn quick-reference |
| [`roadmap.md`](roadmap.md) | Feature roadmap |
| `olddocs/` | Legacy docs (git-ignored) |

> Restart the server after changing any `.cfg` or `.json` file or replacing plugin DLLs.

