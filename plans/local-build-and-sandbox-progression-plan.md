# Local Build + Sandbox Progression Plan (v2)

## 1) Locked decisions

These are fixed for implementation:

1. Scope: unlock/restore only. Keep current `give_build` path for loadouts.
2. Enablement: per-zone toggle with global default.
3. Global default: unlock enabled for sandbox zones when zone flag is not set.
4. Snapshot storage: memory + persisted JSON file.
5. Unlock set: Research + VBlood + Achievements (same as ArenaBuild `unlock_all`).
6. Timing: apply unlock once per zone entry (not on every heartbeat tick).
7. Exit behavior: if snapshot is missing/corrupt, log warning and do not do destructive fallback.

---

## 2) Goal and non-goals

### Goal
Add optional sandbox progression unlock/restore that:
- unlocks full progression when entering sandbox arena,
- restores exact previous progression when leaving sandbox arena,
- survives server restart/crash via persisted snapshots.

### Non-goals (for this plan)
- Replacing ArenaBuild loadout execution (`give_build`) with a native build engine.
- Adding new gameplay unlock semantics beyond Research/VBlood/Achievements.
- Forcing recovery if restore snapshot is missing.

---

## 3) Current state and gaps

### Current state
- `Bluelock` already calls `VAuto.Core.Services.DebugEventBridge` on sandbox enter/exit/in-zone.
- `DebugEventBridge` currently tracks sandbox presence only.
- `KitService` already applies ArenaBuild loadout and can call unlock-all logic.

### Gaps
- No progression snapshot exists.
- No restore path exists.
- No per-zone unlock toggle exists.
- Build/deploy flow is noisy when server is running because copy targets lock DLLs.

---

## 3.1) Current vs After Plan

| Area | Current | After this plan |
|---|---|---|
| Sandbox progression | Not persisted/restored | Full snapshot + restore on exit |
| Unlock timing | Not controlled by bridge | Unlock once per zone entry |
| Unlock scope | Implicit/manual behavior | Research + VBlood + Achievements (explicit) |
| Zone control | No per-zone switch | `SandboxUnlockEnabled` per zone + global default |
| Failure behavior | No clear policy | Missing/corrupt snapshot => warn only, no destructive fallback |
| Restart behavior | No progression recovery | Snapshot persisted to JSON, restore after restart |
| DebugEventBridge API | Entity-only tracking methods | Entity methods + policy overloads + config method |
| Build/deploy loop | Build can fail copy when server locks DLL | Compile-only by default, deploy gated intentionally |
| ArenaBuild loadouts | Used via `give_build` | Unchanged (kept intentionally) |

---

## 3.2) APIs and contracts (explicit)

### Code APIs to add/change

| Type | API | Change |
|---|---|---|
| `Bluelock/Models/ZoneDefinition.cs` | `bool? SandboxUnlockEnabled` | **Add** nullable per-zone toggle |
| `Bluelock/Services/ZoneConfigService.cs` | `bool IsSandboxUnlockEnabled(string zoneId, bool globalDefault)` | **Add** zone resolution helper |
| `Core/Services/DebugEventBridge.cs` | `OnPlayerEnterZone(Entity, bool enableUnlock)` | **Add** overload |
| `Core/Services/DebugEventBridge.cs` | `OnPlayerIsInZone(Entity, bool enableUnlock)` | **Add** overload |
| `Core/Services/DebugEventBridge.cs` | `OnPlayerExitZone(Entity, bool enableUnlock)` | **Add** overload |
| `Core/Services/DebugEventBridge.cs` | `ConfigureSandboxProgression(bool enabled, bool persistSnapshots, string snapshotPath, bool verboseLogs)` | **Add** runtime config API |
| `Core/Services/DebugEventBridge.cs` | Existing 1-arg methods | **Keep unchanged** for compatibility |

### Config APIs to add

In `Bluelock/Plugin.cs` (`Sandbox Progression` section):
- `Enabled` (bool, default `true`)
- `DefaultZoneUnlockEnabled` (bool, default `true`)
- `PersistSnapshots` (bool, default `true`)
- `SnapshotFilePath` (string, default `BepInEx/config/Bluelock/state/sandbox_progression_snapshots.json`)
- `VerboseLogs` (bool, default `false`)

### JSON contract changes

1. Zone config (`VAuto.Zones.json`)
   - New optional field on each zone:
     - `"SandboxUnlockEnabled": true|false`
   - If omitted, use global default.

2. Snapshot state file (new)
   - Path: `BepInEx/config/Bluelock/state/sandbox_progression_snapshots.json`
   - Envelope:
     - `Version` (int)
     - `Players` (map keyed by platformId string)
   - Player snapshot stores component+buffer serialized state for restore.

### Build/deploy API (msbuild property)

For these csproj files:
- `VAutomationCore.csproj`
- `Bluelock/VAutoZone.csproj`
- `CycleBorn/Vlifecycle.csproj`

Add property contract:
- `/p:DeployToServer=false` -> compile only (default)
- `/p:DeployToServer=true` -> run copy-to-server target

---

## 4) Implementation plan

## Track A - local build/deploy reliability (small, required)

### A1. Build without deploy-by-default
Update project post-build copy targets to support a non-deploy build mode:

- `VAutomationCore.csproj`
- `Bluelock/VAutoZone.csproj`
- `CycleBorn/Vlifecycle.csproj`

Add property gate:
- `DeployToServer` (default `false`)
- run copy target only when `DeployToServer=true`.

### A2. Scripted workflow
Use two explicit steps:
1. Compile-only while server is running.
2. Deploy only when server process is stopped (or restart window).

No behavior change in game; this is only to stabilize iteration.

---

## Track B - sandbox progression unlock/restore (main deliverable)

### B1. Config surface

### `Bluelock/Plugin.cs` (CFG entries)
Add new config keys:

- Section: `Sandbox Progression`
  - `Enabled` (bool, default `true`)
  - `DefaultZoneUnlockEnabled` (bool, default `true`)
  - `PersistSnapshots` (bool, default `true`)
  - `SnapshotFilePath` (string, default `BepInEx/config/Bluelock/state/sandbox_progression_snapshots.json`)
  - `VerboseLogs` (bool, default `false`)

Expose value accessors like existing `*Value` accessors.

### `Bluelock/Models/ZoneDefinition.cs`
Add nullable zone-level flag:

- `bool? SandboxUnlockEnabled`

Reason: nullable allows explicit true/false per zone while preserving fallback behavior when omitted.

### `Bluelock/Services/ZoneConfigService.cs`
Add resolver:

- `public static bool IsSandboxUnlockEnabled(string zoneId, bool globalDefault)`

Resolution order:
1. zone exists and `SandboxUnlockEnabled` has value -> use it
2. else -> use `globalDefault`

---

### B2. Bridge contract and behavior

### `Core/Services/DebugEventBridge.cs`
Keep current contract methods unchanged:
- `OnPlayerEnterZone(Entity)`
- `OnPlayerIsInZone(Entity)`
- `OnPlayerExitZone(Entity)`

Add overloads for policy-aware calls:
- `OnPlayerEnterZone(Entity, bool enableUnlock)`
- `OnPlayerIsInZone(Entity, bool enableUnlock)`
- `OnPlayerExitZone(Entity, bool enableUnlock)`

Add configuration method:
- `ConfigureSandboxProgression(bool enabled, bool persistSnapshots, string snapshotPath, bool verboseLogs)`

Internal state (keyed by platform id):
- `_inSandbox`
- `_unlockAppliedThisSession`
- `_snapshots`

### Snapshot model (inside `DebugEventBridge` to avoid extra compile includes)

`SandboxProgressionSnapshot`:
- `ulong PlatformId`
- `DateTime CapturedUtc`
- `Dictionary<string, SnapshotComponentState> Components`
- `Dictionary<string, SnapshotBufferState> Buffers`

`SnapshotComponentState`:
- `bool Existed`
- `string? JsonPayload`

`SnapshotBufferState`:
- `bool Existed`
- `List<string> JsonElements`

File envelope:
- `Version` (int)
- `Dictionary<string, SandboxProgressionSnapshot> Players` (string key = platform id)

---

### B3. Capture/unlock flow (enter once)

Trigger source stays in BlueLock sandbox lifecycle.

On `OnPlayerEnterZone/OnPlayerIsInZone(..., enableUnlock=true)`:
1. Resolve character -> user -> platform id.
2. Mark `_inSandbox`.
3. If global `Enabled=false` or `enableUnlock=false`, stop here.
4. If `_unlockAppliedThisSession` already true for player, stop here.
5. If no snapshot exists for player:
   - capture progression state from user entity (see B4 type filter).
   - save in memory.
   - persist file when enabled.
6. Apply unlock-all through `DebugEventsSystem`:
   - `UnlockAllResearch`
   - `UnlockAllVBloods`
   - `CompleteAllAchievements`
7. Mark `_unlockAppliedThisSession=true`.

Idempotency guarantee: unlock runs once per entry session.

---

### B4. Progression type selection and serialization

Capture only user-entity component/buffer types where type name contains one of:

- `Research`
- `VBlood`
- `Achievement`
- `Progress`
- `Unlock`
- `Tech`
- `Recipe`

Implementation approach:
- Enumerate component types from user entity.
- For `IComponentData`:
  - read via reflected `EntityManager.GetComponentData<T>()`
  - serialize to JSON string with runtime `Type`.
- For dynamic buffers:
  - read via reflected `EntityManager.GetBuffer<T>()`
  - serialize each element to JSON string.

Restore approach:
- For each tracked component/buffer:
  - if snapshot says `Existed=false` and entity currently has it -> remove it.
  - if snapshot says `Existed=true` and payload exists -> deserialize and write back.

If any individual type fails serialize/deserialize:
- warn and continue (no hard fail for zone transition).

---

### B5. Exit/restore flow

On `OnPlayerExitZone(..., enableUnlock=*)`:
1. Resolve character -> user -> platform id.
2. Clear `_inSandbox` and session marker.
3. If global `Enabled=false`, stop.
4. Try load snapshot for platform id:
   - if missing -> warning only, no destructive fallback.
   - if present -> restore all tracked component/buffer states.
5. On successful restore:
   - remove snapshot from memory
   - remove from persisted file.
6. On restore failure:
   - keep snapshot for manual retry
   - log actionable error.

---

### B6. BlueLock integration changes

### `Bluelock/Plugin.cs`
In sandbox enter/in-zone/exit calls:
1. compute `unlockEnabledForZone = ZoneConfigService.IsSandboxUnlockEnabled(zoneId, SandboxProgressionDefaultValue)`
2. invoke `DebugEventBridge` overload with bool policy.
3. keep fallback to old one-arg method if overload is not found (defensive reflection behavior).

This preserves compatibility while enabling per-zone policy.

---

## 5) Test plan

### Unit-level checks
1. `ZoneConfigService.IsSandboxUnlockEnabled`:
   - zone true, zone false, zone null + global true, zone null + global false.
2. Snapshot file roundtrip:
   - write/read/remove with versioned envelope.
3. Idempotency:
   - repeated `OnPlayerIsInZone` does not duplicate unlock call.

### Integration checks (dedicated server)
1. Enter sandbox zone with unlock enabled:
   - unlock methods called once
   - snapshot file entry created.
2. Exit sandbox zone:
   - state restored
   - snapshot removed from memory and file.
3. Zone with `SandboxUnlockEnabled=false`:
   - no unlock, no snapshot.
4. Missing snapshot on exit:
   - warning only, no reset.
5. Restart between enter and exit:
   - snapshot loaded from disk
   - restore still works on exit.

### Regression checks
1. Existing `give_build` flow still applies loadout.
2. AbilityUi enter/exit behavior unchanged.
3. Non-sandbox zones are unaffected.

---

## 6) Rollout plan

1. Ship with `Enabled=true`, `PersistSnapshots=true`, `VerboseLogs=false`.
2. Enable in one sandbox zone first (`SandboxUnlockEnabled=true`) and keep others explicit false for smoke test.
3. Verify 3 full cycles (enter -> fight -> exit) on same character.
4. Expand to all sandbox zones after validation.

---

## 7) Acceptance criteria

Implementation is complete when all are true:

1. Player entering enabled sandbox zone receives unlock-all exactly once per entry.
2. Player exit restores prior progression state from snapshot.
3. Snapshot persists to JSON and survives restart.
4. Missing/corrupt snapshot does not wipe progression and does not break zone flow.
5. ArenaBuild loadout path remains unchanged and functional.
6. Build workflow supports compile-only without forced deploy/copy errors.

---

## 8) Out-of-scope follow-up (next doc)

After this is stable:
- replace ArenaBuild command dependency with native local build executor,
- add explicit admin commands for snapshot diagnostics and manual restore.
