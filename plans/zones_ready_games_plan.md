# Blueluck Zone Games v1 Implementation Plan

## Summary

Implement a full session-based game system inside `Blueluck` for arena and boss zones only. Keep `Core` unchanged as shared infrastructure. Reuse the existing zone/preset/flow architecture by extending current arena/boss config models with session, lifecycle, and objective data. The system is flow-first: services manage state and timing, while NPCs, kits, glows, visuals, prep/reset, and announcements are driven through Blueluck lifecycle flows.

## Scope

### In scope
- Session-enabled `ArenaZone` and `BossZone`
- Lobby, ready-up, countdown, start, end, and reset lifecycle
- Session admission and participant tracking
- Arena objectives: `last_player_standing`, `timer_only`
- Boss objective: `boss_defeat`
- Session-owned NPC spawning and cleanup
- Match-start kit and ability application
- Ready/countdown/start/victory/reset visuals
- Player and admin chat commands
- Startup validation that disables broken session configs per-zone

### Out of scope for v1
- Team scoring
- Capture the flag
- Wave survival
- Respawn scoring
- New `Core` abstractions
- Replacing the existing arena/boss family split

## Boundary

- `Core` remains pack-only and unchanged.
- All new concepts live in `Blueluck`.
- Do not introduce new session or game APIs into `Core`.
- Do not use `Core`'s global `ZUIInputBlocker` for countdown freeze.
- Zone presence still comes from the existing radius-based zone detection path:
  - `ZoneDetectionSystem`
  - `ZoneTransitionRouterSystem`
  - `ZoneTransitionService.OnZoneEnter/OnZoneExit`
- `GameSessionManager` must never perform its own raw radius math. It only reacts to zone enter and exit events.

## Ownership Rules

### Session-scoped ownership

These are created for a single round and must be removed by session cleanup:

- ready glows
- countdown visuals
- victory and defeat visuals
- session-only arena markers
- boss encounter markers
- session-owned NPCs
- session-owned VFX
- other runtime entities tracked by `ZonePrepService`

Cleanup path:
- `PlayerUnreadyFlows` for player-specific ready visuals
- `ResetFlows` for all session visuals and runtime entities
- `ZonePrepService.ClearSessionRuntime(sessionId)` as the final safety net

### Zone-scoped dedication

These persist while the player remains inside the zone and are not removed on round reset:

- PvP state
- kit/loadout dedication
- ability set dedication

Cleanup path:
- apply on zone participation start
- keep across multiple rounds while the player remains in-zone
- clear only on physical zone exit via zone exit handling and exit flows

### Operational rule

- `StartFlows` may run each round, but player dedication logic must be idempotent.
- `ResetFlows` must not strip PvP, kit, or ability dedication from players who remain in the zone.
- `ExitFlows` and zone-exit logic are the only place where PvP, kits, and abilities are unwound.
- Session reset clears visuals and runtime entities; zone exit clears player dedication.

## Defaults Chosen

- Gameplay families in v1: arena + boss only
- Config model: extend existing arena/boss split files
- Orchestration: flow-first
- Ready rule: all currently enrolled present players must be ready
- Arena late join: not allowed after state becomes `Countdown`
- Boss late join: allowed during `Countdown` and `InProgress` until grace expires
- Countdown freeze: session-local buff-based freeze only
- Broken session config: disable session behavior for that zone only, keep the zone loaded
- `last_player_standing` requires `RespawnEnabled = false`
- Player dedication is zone-scoped; visuals and spawned runtime are session-scoped

## State Machine

`Waiting -> Ready -> Countdown -> InProgress -> Ending -> Ended -> Waiting`

### Transition Rules

| From | To | Trigger |
|---|---|---|
| `Waiting` | `Ready` | Enrolled players >= `MinPlayers` and all enrolled players ready |
| `Ready` | `Countdown` | `AutoStartWhenReady = true` or admin `.game forcestart` |
| `Countdown` | `Waiting` | A ready player unreadies, leaves, or player count drops below `MinPlayers` |
| `Countdown` | `InProgress` | Countdown reaches zero |
| `InProgress` | `Ending` | Objective met, timeout reached, admin end, or session abort |
| `Ending` | `Ended` | End flows complete |
| `Ended` | `Waiting` | Reset flows complete and lobby reopens |

### Lock Rules

- Arena admission locks at the instant state becomes `Countdown`.
- Arena joins after that point are admitted as zone occupants but marked non-participant for the current round.
- Boss sessions never hard-lock admission; they enforce `LateJoinGraceSeconds` against match start time.

## Public Types and Config Changes

### New model file

Create `Blueluck/Models/GameSessionModels.cs` with:

```csharp
public enum GameSessionState
{
    Waiting,
    Ready,
    Countdown,
    InProgress,
    Ending,
    Ended
}

public sealed class PlayerSession
{
    public Entity Player { get; set; }
    public ulong SteamId { get; set; }
    public bool IsReady { get; set; }
    public bool IsAlive { get; set; }
    public bool IsParticipant { get; set; }
    public bool JoinedLate { get; set; }
    public bool WasLateJoinRejected { get; set; }
    public DateTime JoinTime { get; set; }
    public DateTime? DeathTime { get; set; }
}

public sealed class GameSession
{
    public string SessionId { get; set; } = Guid.NewGuid().ToString("N");
    public int ZoneHash { get; set; }
    public string ZoneName { get; set; } = string.Empty;
    public string ZoneType { get; set; } = string.Empty;
    public GameSessionState State { get; set; } = GameSessionState.Waiting;
    public List<PlayerSession> Players { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAt { get; set; }
    public DateTime? CountdownStartedAt { get; set; }
    public int RoundNumber { get; set; } = 1;
    public EffectiveSessionDefinition? Definition { get; set; }
    public bool IsAdmissionLocked { get; set; }
}

public sealed class EffectiveSessionDefinition
{
    public GameplayPresetConfig? SourcePreset { get; set; }
    public GameSessionConfig Session { get; set; } = new();
    public SessionLifecycleConfig Lifecycle { get; set; } = new();
    public GameObjectiveConfig Objective { get; set; } = new();
}
```

### Extend `Blueluck/Models/GameplayConfigModels.cs`

Add:

```csharp
public sealed class GameSessionConfig
{
    public bool Enabled { get; set; } = false;
    public int MinPlayers { get; set; } = 1;
    public int MaxPlayers { get; set; } = 16;
    public int CountdownSeconds { get; set; } = 10;
    public int ReadyTimeoutSeconds { get; set; } = 120;
    public int MatchDurationSeconds { get; set; } = 0;
    public bool AutoStartWhenReady { get; set; } = true;
    public bool RequireAllPresentReady { get; set; } = true;
    public bool AllowLateJoin { get; set; } = false;
    public int LateJoinGraceSeconds { get; set; } = 30;
    public int PostMatchResetDelaySeconds { get; set; } = 5;
    public bool FreezeDuringCountdown { get; set; } = true;
    public string? CountdownFreezeBuffPrefab { get; set; } = "Buff_General_Freeze";
    public bool ResetOnEmpty { get; set; } = true;
}

public sealed class SessionLifecycleConfig
{
    public string[] PrepareFlows { get; set; } = Array.Empty<string>();
    public string[] LobbyOpenFlows { get; set; } = Array.Empty<string>();
    public string[] PlayerReadyFlows { get; set; } = Array.Empty<string>();
    public string[] PlayerUnreadyFlows { get; set; } = Array.Empty<string>();
    public string[] CountdownFlows { get; set; } = Array.Empty<string>();
    public string[] StartFlows { get; set; } = Array.Empty<string>();
    public string[] LateJoinFlows { get; set; } = Array.Empty<string>();
    public string[] VictoryFlows { get; set; } = Array.Empty<string>();
    public string[] DefeatFlows { get; set; } = Array.Empty<string>();
    public string[] EndFlows { get; set; } = Array.Empty<string>();
    public string[] ResetFlows { get; set; } = Array.Empty<string>();
    public string[] TickFlows { get; set; } = Array.Empty<string>();
}

public sealed class GameObjectiveConfig
{
    public string ObjectiveType { get; set; } = "timer_only";
    public bool EndMatchOnObjective { get; set; } = true;
    public bool TreatTimeoutAsDraw { get; set; } = true;
}
```

### Extend `GameplayPresetConfig`

Add:

- `string? DisplayName`
- `string? Description`
- `GameSessionConfig? Session`
- `SessionLifecycleConfig? SessionLifecycle`
- `GameObjectiveConfig? Objective`

### Extend `ZoneDefinition`

Add:

- `GameSessionConfig? Session`
- `SessionLifecycleConfig? SessionLifecycle`
- `GameObjectiveConfig? Objective`

Zone values override preset values field-by-field.

## Services

### `GamePresetService`

Create `Blueluck/Services/GamePresetService.cs`.

Responsibilities:
- Resolve a zone's effective preset from `presetIds`
- Merge preset defaults and zone overrides
- Return `EffectiveSessionDefinition`
- Reject invalid objective/session combinations
- Cache resolved definitions by zone hash and invalidate on config reload

### `GameSessionManager`

Refactor `Blueluck/Services/GameSessionManager.cs`.

Responsibilities:
- Own session registry per zone hash
- Create sessions for session-enabled zones only
- Admit players into lobby or spectator/non-participant state
- Coordinate all service calls
- Lock and unlock admission
- Start and cancel countdown
- Start, end, and reset sessions
- Track late-join decisions
- Expose query methods for commands and other services

Mandatory fixes:
- Replace the current invalid `GamePresetConfig` reference with `EffectiveSessionDefinition` or `GameplayPresetConfig`
- Remove direct immediate-start stub behavior
- Remove direct flow execution from `StartGame` and `EndGame`; delegate lifecycle stages to `ZonePrepService`

### `ReadyLobbyService`

Create `Blueluck/Services/ReadyLobbyService.cs`.

Responsibilities:
- Mark ready and unready
- Enforce all-present-ready rule
- Broadcast lobby counts and ready changes
- Run `PlayerReadyFlows` and `PlayerUnreadyFlows`
- Start a ready-timeout timer when the first player joins
- On ready-timeout expiration, clear ready flags and keep the lobby open

### `SessionTimerService`

Create `Blueluck/Services/SessionTimerService.cs`.

Responsibilities:
- Use the existing scheduler from `Core`
- Manage:
  - ready timeout
  - countdown ticks
  - match duration
  - post-match reset delay
- Publish timer callbacks only to `GameSessionManager`
- Cancel all outstanding timers when a session aborts or resets

### `ZonePrepService`

Create `Blueluck/Services/ZonePrepService.cs`.

Responsibilities:
- Execute lifecycle flow groups
- Manage session-owned runtime entities
- Track spawned NPC groups, spawned VFX, and other cleanup targets by composite key
- Composite key format: `{sessionId}:{groupId}`
- Cleanup is idempotent
- Before every new round, clear all runtime entities from the prior round first

### `SessionOutcomeService`

Create `Blueluck/Services/SessionOutcomeService.cs`.

Responsibilities:
- Evaluate arena and boss win conditions
- Arena `last_player_standing`
- Arena `timer_only`
- Timeout draw when `TreatTimeoutAsDraw = true`
- Boss `boss_defeat`
- Multi-boss encounters do not end on partial defeat

## Reusable Flow Patch: Teleport and Stun

Add reusable Blueluck-only flow actions for teleport and stun. These are not session-specific APIs. Sessions may use them in lifecycle flows, but any zone flow can reuse them.

### New Files

#### `Blueluck/Components/PlayerStunState.cs`

```csharp
sealed class PlayerStunState : IComponentData
{
    public bool IsStunned;
    public float StunDurationSeconds;
    public float StunElapsedSeconds;
}
```

#### `Blueluck/Systems/PlayerStunTickSystem.cs`

Responsibilities:
- Query all entities with `PlayerStunState` where `IsStunned == true`
- Increment `StunElapsedSeconds`
- When `StunElapsedSeconds >= StunDurationSeconds`, set `IsStunned = false`
- Integrate with player control enforcement so stunned players cannot move or cast
- Activate only when relevant entities are present

Implementation note:
- This remains fully inside `Blueluck`
- Do not route through `Core`'s global input blocker

### Flow Actions

Patch `Blueluck/Services/FlowRegistryService.cs` to add:

#### `player.teleport`

Parameters:
- `targetZoneHash` optional
- `targetPosition` optional
- `snapRotation` optional

Behavior:
- If `targetZoneHash` is provided, resolve that zone and move the player to its center or configured spawn point
- If `targetPosition` is provided, set the player directly to that coordinate
- If `snapRotation` is provided, apply the facing direction after teleport
- Validation fails if neither `targetZoneHash` nor `targetPosition` is provided
- Log teleport operations for debugging

Clarification:
- This is a direct player flow action, not a session admission mechanic
- Zone membership still comes from normal radius-based zone detection after teleport settles

#### `player.stun`

Parameters:
- `durationSeconds` required
- `message` optional

Behavior:
- Find or add `PlayerStunState`
- Set `IsStunned = true`
- Set `StunDurationSeconds = durationSeconds`
- Reset `StunElapsedSeconds = 0`
- If `message` exists, send it to the player
- Validation fails if `durationSeconds <= 0`

Multiple stun rule:
- A later stun overrides the current duration and restarts elapsed time

#### `player.unstun`

Parameters:
- none

Behavior:
- Find `PlayerStunState`
- Set `IsStunned = false`
- Clear duration and elapsed values
- Log the unstun action

### Validation

Patch `Blueluck/Services/FlowValidationService.cs` to validate:
- `player.teleport`: requires either `targetZoneHash` or `targetPosition`
- `player.stun`: requires `durationSeconds > 0`
- `player.unstun`: no required parameters

Failure behavior:
- Invalid action references should raise flow validation errors during startup
- A broken flow should disable the affected session-enabled zone session behavior, not the whole plugin

### Plugin Wiring

Patch `Blueluck/Plugin.cs`:
- register `PlayerStunTickSystem` with the ECS initialization path
- keep it inside Blueluck initialization with the other custom zone systems

### Session Usage

Sessions may use these actions in lifecycle flows.

Examples:
- arena countdown lockdown
- arena match-start teleport to combat pads
- boss late-join teleport into encounter staging
- reset unstun safety cleanup

Rule:
- sessions may depend on `player.unstun` in `ResetFlows`
- sessions must not require teleport/stun to exist for basic lobby functionality unless validation confirms those flows

### Optional Command Integration

Patch `Blueluck/Commands/GameSessionCommands.cs` with optional admin commands:
- `.game tpall <targetZoneHash>`
- `.game tpall <x> <y> <z>`
- `.game stun <durationSeconds>`
- `.game unstun`

Rules:
- operate only on the caller's current session-enabled zone
- affect current session participants only
- log admin usage
- `tpall` teleports active session participants only, never observers or non-participants
- `.game tpall <targetZoneHash>` teleports participants to the target zone center or configured spawn point
- `.game tpall <x> <y> <z>` teleports participants to an explicit world position

### Optional Ready Rule Integration

Optional behavior in `ReadyLobbyService`:
- block `.game ready` while `PlayerStunState.IsStunned == true`

Default:
- do not require this for v1
- only add if stun is later used during lobby state rather than countdown state only

## ECS Systems

### `SessionCombatEventRouterSystem`

Create `Blueluck/Systems/SessionCombatEventRouterSystem.cs`.

Responsibilities:
- Read death events directly from ECS in Blueluck
- Resolve:
  - player death in active session
  - boss death for session-owned encounter NPCs
- Forward events to `GameSessionManager`

Rules:
- Arena player death marks the participant dead and reevaluates objective
- Boss late joiner death converts that player to spectator/non-participant for the rest of the round
- A late joiner may not rejoin the same round after leaving or dying

### `PlayerStunTickSystem`

Add `Blueluck/Systems/PlayerStunTickSystem.cs` as a reusable control system backing `player.stun` and `player.unstun`.

This system is not part of session state logic. It is shared Blueluck infrastructure for flow-driven player control.

### `SessionTickSystem`

Create only if needed after implementation review.

Default behavior:
- Do not add it unless timer or periodic session state checks cannot be handled by `SessionTimerService`

## Zone Transition Behavior

### Update `ZoneTransitionService`

For non-session zones:
- Keep current behavior unchanged

For session-enabled zones:
- Physical zone entry means lobby admission, not immediate gameplay start
- Do not apply session visuals on physical entry
- Zone dedication may be established for zone residents, but must be idempotent and must not be torn down by round reset
- Admit player according to current session state:
  - `Waiting` or `Ready`: enrolled participant candidate
  - `Countdown` or `InProgress` in arena: non-participant
  - `Countdown` or `InProgress` in boss and within grace window: late-join participant
  - `Countdown` or `InProgress` in boss after grace: non-participant
- Physical zone exit:
  - remove player from session tracking
  - if countdown is active and the ready rule is broken, cancel countdown
  - if the session becomes empty and `ResetOnEmpty = true`, abort and reset the session

## Flows and Lifecycle

### Preserve Existing Flow Semantics

- `entryFlows` and `exitFlows` remain physical zone transition flows
- `tickFlows` remain existing zone/preset tick hooks
- Existing `arena_*_core` and `boss_encounter_core` flows remain valid
- Session lifecycle flows must respect the zone-vs-session ownership split

### Session Lifecycle Flow Groups

- `PrepareFlows`
  - clear previous session runtime
  - neutralize zone state
  - pre-stage NPCs and visuals
- `LobbyOpenFlows`
  - open lobby message and waiting visuals
- `PlayerReadyFlows`
  - apply per-player ready glow or buff
- `PlayerUnreadyFlows`
  - clear per-player ready glow or buff
- `CountdownFlows`
  - apply countdown visuals
  - apply countdown freeze buff to participants if enabled
- `StartFlows`
  - apply match visuals and encounter start
  - player dedication work must be safe to re-run and must not duplicate grants
- `LateJoinFlows`
  - apply boss late-join kit, ability, and coop assignment
- `VictoryFlows`
  - winner or boss-clear visuals, messages, and rewards
- `DefeatFlows`
  - failed encounter or draw/loss messaging
- `EndFlows`
  - unwind match state
- `ResetFlows`
  - clear session-owned runtime and reopen lobby
  - do not remove PvP, kit, or ability dedication from players still in-zone
- `TickFlows`
  - optional periodic flows during active session only

## Countdown Freeze Decision

Use session-local buff freeze only.

Implementation rules:
- On transition to `Countdown`, if `FreezeDuringCountdown = true` and `CountdownFreezeBuffPrefab` validates, apply the buff to current participants
- On countdown cancel or countdown complete, remove the buff from those same participants
- If the buff prefab is missing or invalid, log a validation warning and disable freeze for that zone session
- No global input blocking and no `Core` edits

## NPC and Runtime Entity Tracking

Use a composite-key registry in `ZonePrepService`, not ECS marker components.

Data structures:

```csharp
Dictionary<string, List<Entity>> _runtimeEntitiesByGroup;
Dictionary<string, SessionRuntimeSummary> _runtimeSummaryBySession;
```

Rules:
- All spawned NPC groups and spawned VFX must register under the current `sessionId`
- Cleanup methods:
  - `ClearNpcGroup(sessionId, groupId)`
  - `ClearSessionRuntime(sessionId)`
- Old runtime must be cleared before a new session round starts
- `boss.spawnencounter` must delegate to the same tracking layer as generic group spawns

## Player Dedication Tracking

Add explicit per-player zone dedication tracking in Blueluck.

Suggested shape:

```csharp
Dictionary<ulong, HashSet<int>> _dedicatedZonesByPlayer;
```

Rules:
- a player can be marked dedicated for a zone hash
- repeated round starts in the same zone must not repeatedly grant the same zone dedication
- round reset leaves dedication intact
- physical zone exit removes that zone's dedication record and performs unwind logic

Arena note:
- if spectators or non-participants should remain geared but not fight, keep kit/ability dedication zone-scoped and gate active combat state separately
- if all in-zone players should remain PvP-enabled, keep PvP zone-scoped too
- if spectators must be safe, make PvP enablement session-scoped while leaving kit/ability dedication zone-scoped

## Flow Registry Changes

### Add New Action Aliases in `FlowRegistryService`

Add:
- `player.teleport`
- `player.stun`
- `player.unstun`
- `zone.spawnnpcgroup`
- `zone.clearnpcgroup`
- `zone.clearsessionruntime`
- `game.broadcastsessionstate`

### Backward Compatibility

- Keep `boss.spawnencounter`
- Internally map `boss.spawnencounter` to the same execution path as `zone.spawnnpcgroup`
- Log a one-time deprecation warning per flow ID when a legacy alias is used

### Validation

Update `FlowValidationService` to validate:
- teleport and stun actions
- new action aliases
- lifecycle flow references in session configs
- objective/session compatibility
- countdown freeze buff existence when enabled
- arena `last_player_standing` plus `RespawnEnabled = true` as invalid for sessions

Failure behavior:
- invalid session-enabled zone:
  - zone remains loaded
  - session behavior is disabled for that zone
  - base physical zone enter/exit behavior continues
  - startup logs a clear error with zone hash and cause

## Commands

### New File

Create `Blueluck/Commands/GameSessionCommands.cs`

### Player Commands

- `.game ready`
- `.game unready`
- `.game lobby`
- `.game status`

### Admin Commands

- `.game forcestart`
- `.game end`
- `.game reset`
- `.game reload`
- `.game debug`
- `.game stun <durationSeconds>`
- `.game unstun`
- `.game tpall <targetZoneHash>`
- `.game tpall <x> <y> <z>`

### Command Rules

- All failures return a chat message; no silent failure
- Add a per-player cooldown of 500ms for `.game ready` and `.game unready`
- `.game forcestart` bypasses the ready rule but still requires `MinPlayers`
- `.game forcestart` logs admin override with zone hash and player count
- Commands only operate in session-enabled zones unless admin specifies target zone
- Implement as a VCF command group:

```csharp
[CommandGroup("game", "g")]
```

## File-by-File Work Plan

### Create

- `Blueluck/Components/PlayerStunState.cs`
- `Blueluck/Models/GameSessionModels.cs`
- `Blueluck/Services/GamePresetService.cs`
- `Blueluck/Services/ReadyLobbyService.cs`
- `Blueluck/Services/SessionTimerService.cs`
- `Blueluck/Services/ZonePrepService.cs`
- `Blueluck/Services/SessionOutcomeService.cs`
- `Blueluck/Systems/PlayerStunTickSystem.cs`
- `Blueluck/Systems/SessionCombatEventRouterSystem.cs`
- `Blueluck/Commands/GameSessionCommands.cs`

### Update

- `Blueluck/Plugin.cs`
- `Blueluck/Models/GameplayConfigModels.cs`
- `Blueluck/Models/ZoneDefinition.cs`
- `Blueluck/Services/GameplayRegistrationSupport.cs`
- `Blueluck/Services/GameSessionManager.cs`
- `Blueluck/Services/ZoneTransitionService.cs`
- `Blueluck/Services/FlowRegistryService.cs`
- `Blueluck/Services/FlowValidationService.cs`
- `Blueluck/config/arena_presets.config.json`
- `Blueluck/config/boss_presets.config.json`
- `Blueluck/config/arena.zones.json`
- `Blueluck/config/boss.zones.json`

## Config Migration Plan

### Arena Preset Additions

Add a session preset such as `arena_elimination_session` with:
- `session.enabled = true`
- `session.minPlayers = 2`
- `session.allowLateJoin = false`
- `objective.objectiveType = "last_player_standing"`
- lifecycle flow sets for prepare, ready, countdown, start, end, and reset

### Boss Preset Additions

Add a session preset such as `boss_encounter_session` with:
- `session.enabled = true`
- `session.minPlayers = 1`
- `session.allowLateJoin = true`
- `session.lateJoinGraceSeconds = 45`
- `objective.objectiveType = "boss_defeat"`
- lifecycle flow sets for prepare, countdown, start, late join, victory, end, and reset

### Zone Migration

- Convert one arena zone and one boss zone first
- Leave current sanctuary and non-session zones unchanged
- Keep old presets active for non-session zones

## Implementation Order

1. Add models and config schema extensions
2. Implement `GamePresetService`
3. Refactor `GameSessionManager`
4. Implement `ReadyLobbyService`
5. Implement `SessionTimerService`
6. Implement `ZonePrepService`
7. Implement `SessionOutcomeService`
8. Update `ZoneTransitionService`
9. Add `SessionCombatEventRouterSystem`
10. Extend `FlowRegistryService`
11. Extend `FlowValidationService`
12. Add commands
13. Migrate sample configs
14. Test and harden edge cases

## Tests and Acceptance Scenarios

### Lobby and Ready

- First player entering a session-enabled arena opens the lobby
- `.game lobby` shows correct enrolled players and ready count
- All-present-ready rule is enforced
- Countdown cancels if a ready player leaves or unreadies
- Ready timeout clears ready flags without breaking the zone

### Arena

- Arena participants do not receive PvP, kit, or abilities on physical entry
- Arena participants receive PvP, kit, and abilities at match start only
- Arena late entrant after `Countdown` becomes non-participant
- Arena player death updates alive-state
- Arena match ends when one participant remains alive
- Arena timer draw ends immediately when timeout hits with more than one participant alive
- `last_player_standing` session config with `RespawnEnabled = true` disables session behavior at validation time
- If teleport/stun countdown flows are configured, participants are teleported and stunned correctly before match start
- `player.unstun` safety cleanup clears any lingering stun on reset

### Boss

- Boss session prepare and start flows spawn tracked session-owned encounter bosses
- Boss late join within grace receives late-join flows
- Boss late join after grace becomes non-participant
- Boss session ends only when all tracked encounter bosses are dead
- Late joiner who dies becomes spectator for the remainder of the round
- Late joiner cannot leave and rejoin the same round as participant
- Boss late-join flow can teleport the late joiner into the encounter safely

### Runtime Cleanup

- `zone.clearnpcgroup` removes only the targeted session group
- `zone.clearsessionruntime` removes all session-owned NPCs and VFX
- Double cleanup is safe and idempotent
- Starting a new round clears leftover runtime from the prior round first
- Stun plus unstun in the same frame resolves to unstunned
- Multiple stuns on the same player override the previous duration

### Commands

- `.game ready` and `.game unready` enforce the 500ms cooldown
- `.game ready` outside a session-enabled zone returns an explanatory message
- `.game forcestart` bypasses the ready rule but respects `MinPlayers`
- `.game reset` clears session runtime and returns the lobby to `Waiting`

### Reload and Failure Paths

- Session-enabled zone with a missing lifecycle flow is loaded as a normal zone but sessions are disabled for that zone
- Config reload during `Countdown` aborts the session cleanly and removes the countdown freeze buff
- Config reload during `InProgress` ends the session with cleanup and revalidation
- Empty session with `ResetOnEmpty = true` resets cleanly

## Assumptions

- Existing arena and boss preset/flow files remain the source of truth
- Existing non-session zones must preserve current behavior
- Existing legacy boss flow aliases must continue working
- Countdown freeze buff prefab can be configured per-zone or per-preset; a default invalid value results in freeze-disabled, not zone-disabled
- State machine logging is mandatory at every transition for debugging
