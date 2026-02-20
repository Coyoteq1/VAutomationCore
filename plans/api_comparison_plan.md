# API Signature Debug & Comparison Plan
**VAutomationCore / BlueLock vs ScarletCore**

Reference: `C:\temp_reference\ScarletCore`  
Output: `BepInEx/config/Bluelock/debug_api_compare.txt`

---

## 1. Purpose

Compare public API signatures between the current VAutomationCore/BlueLock build and the ScarletCore reference library to:

- Detect broken Harmony patch targets
- Identify missing or misaligned ECS system access
- Validate event system compatibility
- Expose reflection-based access to renamed/moved fields
- Catch unsafe NativeArray disposal patterns
- Catch different target systems for the same logical operation

---

## 2. Inputs

| Source | Paths |
|--------|-------|
| Reference | `C:\temp_reference\ScarletCore\**\*.cs` |
| Current patches | `Patches\*.cs`, `Bluelock\Patches\*.cs` |
| Current services | `Core\Services\*.cs`, `Bluelock\Services\*.cs` |
| Current ECS | `Core\ECS\*.cs` |
| Current events | `Core\Events\*.cs` |

---

## 3. Comparison Areas

### 3A — Harmony Patch Target Comparison

**ScarletCore patch targets (authoritative reference):**

| ScarletCore File | Target System | Target Method | Prefix/Postfix |
|-----------------|--------------|--------------|----------------|
| `DeathEventSystemPatch.cs` | `DeathEventListenerSystem` | `OnUpdate` | Both |
| `UnitSpawnerReactSystemPatch.cs` | `UnitSpawnerReactSystem` | `OnUpdate` | Both |
| `AbilityPatch.cs` | `AbilitySystem` (TBD) | `OnUpdate` | TBD |
| `AdminAuthSystemPatch.cs` | `AdminAuthSystem` | TBD | TBD |
| `ChatMessageSystemPatch.cs` | `ChatMessageSystem` | `OnUpdate` | Both |
| `DestroyTravelBuffPatch.cs` | `DestroyTravelBuff` system | TBD | TBD |
| `InitializationPatch.cs` | `ServerBootstrapSystem` | `OnUpdate` | TBD |
| `InteractPatch.cs` | Interact system | TBD | Both |
| `InventoryPatches.cs` | Inventory system | TBD | Both |
| `PlayerConnectivityPatches.cs` | Player connect/disconnect | TBD | Both |
| `ReplaceAbilityOnSlotPatch.cs` | `ReplaceAbilityOnSlot` system | TBD | Both |
| `SavePatch.cs` | Save system | TBD | TBD |
| `ShapeshiftPatch.cs` | Shapeshift system | TBD | TBD |
| `StatChangeSystemPatch.cs` | `StatChangeSystem` | TBD | TBD |
| `UnitSpawnerReactSystemPatch.cs` | `UnitSpawnerReactSystem` | `OnUpdate` | Both |
| `VampireDownedPatch.cs` | Downed system | TBD | TBD |
| `WarEventsPatch.cs` | War events system | TBD | TBD |
| `WaypointPatch.cs` | `WaypointSystem` | TBD | TBD |

**VAutomationCore patch targets (current):**

| File | Target System | Target Method | Status |
|------|--------------|--------------|--------|
| `Patches/BuffSpawnServerPatch.cs` | `BuffSystem_Spawn_Server` | `OnUpdate` | **NOT LOADED** (bootstrap broken) |
| `Patches/DeathEventSystemPatch.cs` | `DeathEventListenerSystem` | `OnUpdate` | **NOT LOADED** + uses `using` on NativeArray |
| `Patches/ServerBootstrapSystemPatch.cs` | `ServerBootstrapSystem` | `OnUpdate` | **NOT LOADED** |
| `Patches/UnitSpawnerSystemPatch.cs` | `UnitSpawnerSystem` | `OnUpdate` | **NOT LOADED** + wrong target (see below) |
| `Bluelock/Patches/DropInventorySystemPatch.cs` | `DropInventorySystem` | TBD | **NOT LOADED** |

**Critical difference — unit spawn target:**

| | Target System | Query Field | Notes |
|-|--------------|------------|-------|
| **ScarletCore** | `UnitSpawnerReactSystem` | `_Query` | Correct pattern — reacts to spawned entities |
| **VAutomationCore** | `UnitSpawnerSystem` | `_SpawnBuffer` via reflection | Different system; reflection-based access is fragile |

**Action required:**
- VAutomationCore should target `UnitSpawnerReactSystem` to match ScarletCore (same game system, safer query pattern)
- Remove reflection-based `_SpawnBuffer` access — use `__instance._Query.ToEntityArray(Allocator.Temp)` instead

---

### 3B — NativeArray Disposal Pattern Comparison

**ScarletCore pattern (correct for V Rising):**
```csharp
var deathEvents = __instance._DeathEventQuery.ToEntityArray(Allocator.Temp);
try {
    // process
} catch (Exception ex) {
    Log.Error(ex);
} finally {
    deathEvents.Dispose();
}
```

**VAutomationCore pattern (unsafe):**
```csharp
using var deathEvents = __instance._DeathEventQuery.ToComponentDataArrayAccessor<DeathEvent>(Allocator.Temp);
```

**Problem:** `using` statements for native collections are unreliable in V Rising's IL2CPP environment. V Rising modding rules explicitly prohibit this pattern.

**Action required:** Replace all `using var` patterns on NativeArray/NativeList with explicit `try/finally` disposal.

**Files to check:**
- [`Patches/DeathEventSystemPatch.cs`](../Patches/DeathEventSystemPatch.cs)
- [`Patches/BuffSpawnServerPatch.cs`](../Patches/BuffSpawnServerPatch.cs)
- [`Patches/UnitSpawnerSystemPatch.cs`](../Patches/UnitSpawnerSystemPatch.cs)
- [`Bluelock/Patches/DropInventorySystemPatch.cs`](../Bluelock/Patches/DropInventorySystemPatch.cs)

---

### 3C — Event System Comparison

**ScarletCore event model:**
```csharp
// EventManager with enum keys
EventManager.Emit(PrefixEvents.OnDeath, deathEvents);
EventManager.GetSubscriberCount(PrefixEvents.OnDeath)
```

Event enums: `ServerEvents`, `CommandEvents`, `PlayerEvents`, `PrefixEvents`, `PostfixEvents`

**VAutomationCore event model:**
```csharp
// TypedEventBus with concrete event classes
TypedEventBus.Publish(new DeathOccurredEvent { ... });
```

Event types: `DeathOccurredEvent`, `UnitSpawnedEvent`, `BuffInitializedEvent`, `BuffDestroyedEvent`, `SpawnTravelBuffAppliedEvent`, `ServerStartedEvent`, `WorldReadyEvent`, `WorldInitializedEvent`

**Assessment:** Both systems are valid. They are parallel implementations. No breaking change required. However:

- ScarletCore offers a `GetSubscriberCount()` guard that prevents unnecessary ECS work when no handlers are registered — VAutomationCore should add a similar optimization.
- VAutomationCore's typed events are more type-safe and IDE-friendly.

**Gap:** VAutomationCore has no equivalent to ScarletCore's `PlayerEvents` (PlayerJoined, PlayerLeft, PlayerKicked, PlayerBanned, CharacterCreated, CharacterRenamed). These are missing from the event bus.

---

### 3D — ECS Extension Comparison

**ScarletCore pattern — extensions on `Entity` directly:**
```csharp
// entity.Read<T>(), entity.Has<T>(), entity.With(...)
entity.Read<LifeTime>()
entity.SetPosition(position)
entity.With((ref LifeTime lt) => { lt.Duration = duration; })
```

**VAutomationCore pattern — extensions on `EntityManager`:**
```csharp
// em.HasComponent<T>(entity), em.GetComponent<T>(entity)
em.HasComponent<T>(entity)
em.GetComponent<T>(entity)
```

**Gap:** ScarletCore's entity-direct extension pattern (`entity.Read<T>()`) is more ergonomic and is the community standard. VAutomationCore's `EntityManager`-first pattern works but is more verbose.

**Impact on patches:** ScarletCore patches like `UnitSpawnerReactSystemPatch` use `entity.Read<LifeTime>()` and `entity.With(...)` which require `ECSExtensions.cs` to be imported. VAutomationCore code that tries to call these methods without the ScarletCore extensions will fail to compile if mixed.

---

### 3E — GameSystems / World Access Comparison

**ScarletCore:**
```csharp
GameSystems.EntityManager
GameSystems.PrefabCollectionSystem
GameSystems.UnitSpawnerUpdateSystem
GameSystems.ServerGameManager
GameSystems.DebugEventsSystem
```

**VAutomationCore:**
```csharp
ZoneCore.EntityManager        // in Bluelock
UnifiedCore.EntityManager     // in Core
```

**Gap:** VAutomationCore does not expose `PrefabCollectionSystem`, `DebugEventsSystem`, or `UnitSpawnerUpdateSystem` through a static accessor. ScarletCore's `SpawnerService` requires `GameSystems.PrefabCollectionSystem._PrefabLookupMap.GetName(prefabGUID)` and `GameSystems.UnitSpawnerUpdateSystem.SpawnUnit(...)`.

If VAutomationCore ever calls ScarletCore's `SpawnerService`, it must ensure `GameSystems.Initialize()` has been called.

---

### 3F — Service Layer Comparison

**ScarletCore services (reference):**

| Service | Key Methods |
|---------|-------------|
| `SpawnerService` | `Spawn(PrefabGUID, float3, minRange, maxRange, lifeTime, count, postSpawnAction)` |
| `BuffService` | `AddBuff()`, `RemoveBuff()`, `HasBuff()` |
| `InventoryService` | `AddItemToInventory()`, `RemoveItemFromInventory()`, `HasItem()` |
| `TeleportService` | `TeleportTo()`, `TeleportToPosition()` |
| `PlayerService` | `GetPlayerByName()`, `GetAllPlayers()` |
| `MessageService` | `Send()`, `Broadcast()`, `SendToUser()` |
| `AbilityService` | `ReplaceAbilityOnSlot()`, `ResetCooldown()` |
| `MapService` | `SpawnMapIcon()`, `RemoveMapIcon()` |
| `AdminService` | `IsAdmin()`, `AddAdmin()`, `RemoveAdmin()` |

**VAutomationCore services (current):**

| Service | Purpose |
|---------|---------|
| `GameActionService` | ECS action dispatch |
| `SnapshotCaptureService` | Player state capture |
| `SandboxSnapshotStore` | Snapshot persistence |
| `ZoneConfigService` | Zone config CRUD |
| `KitService` | Kit grant/restore |
| `BuildingService` | Structure spawning |
| `ArenaMatchManager` | Match lifecycle |
| `ZoneTemplateService` | Template spawn/clear |

**Gap analysis:**

| Needed Capability | ScarletCore Equivalent | VAutomationCore Status |
|------------------|----------------------|----------------------|
| Spawn units with post-action | `SpawnerService.Spawn()` | Uses direct ECS spawn — no post-action support |
| Teleport player | `TeleportService.TeleportTo()` | Inline in `ZoneConfigService` |
| Inventory manipulation | `InventoryService` | `KitService` (partial overlap) |
| Ability slot replace | `AbilityService.ReplaceAbilityOnSlot()` | `AbilityUi.cs` |
| Player lookup by name | `PlayerService.GetPlayerByName()` | Inline queries per-command |
| Server message | `MessageService.Send()` | `ChatCommandContext.Reply()` only |

---

### 3G — Missing Patch Targets in VAutomationCore

Comparing ScarletCore's 18 patch targets against VAutomationCore, these patches exist in ScarletCore but have **no equivalent in VAutomationCore**:

| Missing Capability | ScarletCore Patch | Impact |
|-------------------|------------------|--------|
| Player join/leave | `PlayerConnectivityPatches.cs` | No player connect event in VAutoCore |
| Chat interception | `ChatMessageSystemPatch.cs` | No chat event bus |
| Ability cast tracking | `AbilityPatch.cs` | No cast started/finished events |
| Stat change tracking | `StatChangeSystemPatch.cs` | No damage dealt event |
| Shapeshift tracking | `ShapeshiftPatch.cs` | No shapeshift event |
| Inventory change | `InventoryPatches.cs` | No inventory change event |
| Waypoint teleport | `WaypointPatch.cs` | No waypoint event |
| War events | `WarEventsPatch.cs` | No war event |
| Admin auth | `AdminAuthSystemPatch.cs` | No admin change event |
| Travel buff destroy | `DestroyTravelBuffPatch.cs` | No travel buff event |
| Interaction | `InteractPatch.cs` | No interact event |
| Downed player | `VampireDownedPatch.cs` | No downed event |
| World save | `SavePatch.cs` | No save event |

---

## 4. Output Report Format

The debug file at `BepInEx/config/Bluelock/debug_api_compare.txt` should contain:

```
==============================
 API SIGNATURE COMPARISON
 VAutomationCore vs ScarletCore
==============================

[CRITICAL — Patches Not Loaded]
- VAutomationCore.Patches.DeathEventSystemPatch    NOT applied at runtime
- VAutomationCore.Patches.BuffSpawnServerPatch     NOT applied at runtime
- VAutomationCore.Patches.UnitSpawnerSystemPatch   NOT applied at runtime
- VAutomationCore.Patches.ServerBootstrapSystemPatch NOT applied at runtime
- VAuto.Zone.Patches.DropInventorySystemPatch      NOT applied at runtime

[CRITICAL — Wrong Patch Target]
- VAutomationCore.Patches.UnitSpawnerSystemPatch
    Targets:  UnitSpawnerSystem._SpawnBuffer (reflection)
    SC Target: UnitSpawnerReactSystem._Query (direct field)
    Risk: UnitSpawnerSystem field names may change; reflection silently fails

[CRITICAL — Unsafe NativeArray Disposal]
- VAutomationCore.Patches.DeathEventSystemPatch:38
    Pattern: using var deathEvents = ...ToComponentDataArrayAccessor<DeathEvent>()
    Fix:     try/finally with explicit .Dispose()

[WARNING — Missing Patch Targets vs ScarletCore]
- ChatMessageSystemPatch         not in VAutomationCore
- AbilityPatch                   not in VAutomationCore
- PlayerConnectivityPatches      not in VAutomationCore
- StatChangeSystemPatch          not in VAutomationCore
- InventoryPatches               not in VAutomationCore
- VampireDownedPatch             not in VAutomationCore
- (8 more — see section 3G)

[WARNING — Missing Event Types vs ScarletCore]
- PlayerJoined / PlayerLeft / PlayerKicked
- ChatMessageReceived
- AbilityCast events (CastStarted, CastFinished, CastInterrupted)
- DealDamage / StatChange
- InventoryChanged / MoveItem

[INFO — Event Model Difference]
- ScarletCore: EventManager.Emit(enum, NativeArray<Entity>)
- VAutoCore:   TypedEventBus.Publish(typedEvent)
- Both valid.  No breaking change.  VAutoCore is more type-safe.

[INFO — ECS Extension Pattern Difference]
- ScarletCore: entity.Read<T>()     (extension on Entity)
- VAutoCore:   em.GetComponent<T>(entity)  (extension on EntityManager)
- Cross-compatible only if ScarletCore ECSExtensions is referenced

[INFO — GameSystems Access Gap]
- ScarletCore exposes UnitSpawnerUpdateSystem, PrefabCollectionSystem
- VAutoCore does not expose these via static accessor
- Required for SpawnerService.Spawn() compatibility

==============================
 END OF REPORT
==============================
```

---

## 5. Automation Options

### Option A — Static Source Comparison (Recommended)

A PowerShell or C# script that:

1. Scans `ScarletCore/**/*.cs` for `[HarmonyPatch(typeof(X), nameof(X.Y))]` attributes
2. Scans VAutomationCore `Patches/**/*.cs` for the same
3. Diffs the two lists
4. Checks for `using var` on NativeArray types
5. Writes `debug_api_compare.txt`

Can run as a build step without loading assemblies.

### Option B — Runtime Admin Command

Add to VAutomationCore:
```csharp
[Command("debug apis", description: "Compare patch targets vs ScarletCore reference")]
public static void DebugApis(ChatCommandContext ctx) { ... }
```

Outputs a diff to the chat and writes `debug_api_compare.txt`.

### Option C — Assembly Reflection (Full)

Load both DLLs via `Assembly.LoadFile()`, reflect all public types and methods, serialize to JSON, compare JSON trees. Most complete but requires a separate tool project.

---

## 6. Priority Fix Order

| # | Issue | File | Fix |
|---|-------|------|-----|
| 1 | Patch bootstrap broken | [`Plugin.cs`](../Plugin.cs) in Bluelock | `_harmony.PatchAll(typeof(Plugin).Assembly)` |
| 2 | `using var` on NativeArray | [`Patches/DeathEventSystemPatch.cs`](../Patches/DeathEventSystemPatch.cs) | Replace with `try/finally` |
| 3 | Wrong spawn target system | [`Patches/UnitSpawnerSystemPatch.cs`](../Patches/UnitSpawnerSystemPatch.cs) | Retarget to `UnitSpawnerReactSystem` |
| 4 | Dead lifecycle reference | [`Bluelock/Plugin.cs`](../Bluelock/Plugin.cs) ~line 2948 | Remove `ArenaLifecycleManager` code path |
| 5 | Missing player connect events | — | Add `PlayerConnectivityPatches` equivalent |
| 6 | ECS extension ergonomics | [`Core/ECS/EntityExtensions.cs`](../Core/ECS/EntityExtensions.cs) | Add `entity.Read<T>()` / `entity.Has<T>()` style extensions |

---

## 7. What the Comparison Does NOT Need to Validate

- Config file formats (Bluelock-specific, no ScarletCore equivalent)
- Template system APIs (Bluelock-exclusive)
- Zone lifecycle steps (Bluelock-exclusive)
- Command groups (VampireCommandFramework handles registration)
- CycleBorn lifecycle internals (separate plugin)
