# VAutomationCore Usage Guide

This guide explains how to use the core framework in practical sections:
- Core system access
- Service registration
- Configuration
- Snapshot/debug workflows
- API surfaces
- Lifecycle snapshot operations
- Command reference
- Lifecycle config reference
- File/folder map

## 1) Core System Access

`UnifiedCore` provides safe access to server ECS objects.

```csharp
using VAutomationCore.Core;

if (!UnifiedCore.IsInitialized)
{
    // module can defer work until server world is available
    return;
}

var world = UnifiedCore.Server;
var em = UnifiedCore.EntityManager;
```

Typical use:
- Read and write ECS components through `EntityManager`
- Resolve prefab entities through `UnifiedCore.TryGetPrefabEntity(...)`
- Use `UnifiedCore.LogInfo/LogWarning/LogError` for module-level diagnostics

## 2) Service Registration

Use `ServiceInitializer` for startup wiring, and `ServiceRegistry` for cross-module service resolution.

```csharp
using VAutomationCore.Core.Logging;
using VAutomationCore.Core.Services;
using VAutomationCore.Core.Api;

var log = new CoreLogger("MyModule");
ServiceInitializer.InitializeLogger(log);

ServiceInitializer.RegisterInitializer("my_service", MyService.Initialize);
ServiceInitializer.RegisterValidator("my_service", () => MyService.IsReady);
ServiceInitializer.InitializeAll(log);

ServiceRegistry.RegisterSingleton<IMyService>(new MyService(), replace: true);
```

Resolve later:

```csharp
if (ServiceRegistry.TryResolve<IMyService>(out var svc))
{
    svc.DoWork();
}
```

## 3) Config Usage

`ConfigService` centralizes config files under BepInEx config path.

```csharp
using VAutomationCore.Core.Config;

ConfigService.Initialize();
var value = ConfigService.Get("mymodule.enabled", defaultValue: "true");
ConfigService.Set("mymodule.enabled", "false");
ConfigService.Save();
```

Notes:
- Keep keys stable (`module.section.key`)
- Save only after a batch of updates
- Validate parsed values before applying runtime behavior

## 4) Snapshot / Debug Workflows

Snapshot helpers are exposed via core services:
- `SandboxSnapshotStore`
- `SandboxCsvWriter`
- `SandboxDeltaComputer`
- `Core.Services.Sandbox.*` interfaces/services

Recommended flow:
1. Capture baseline snapshot before major state change.
2. Apply operation (job/flow/config).
3. Capture second snapshot.
4. Compute delta and export to CSV for review.

Example pattern:

```csharp
// pseudocode-style flow (use your concrete service wiring)
// captureA = snapshotCapture.Capture("before");
// perform action
// captureB = snapshotCapture.Capture("after");
// delta = snapshotDiff.Compute(captureA, captureB);
// snapshotPersistence.SaveDelta(delta);
```

## 5) API Docs (Separate Files)

- `docs/api/Server-API.md`
- `docs/api/Player-API.md`
- `docs/api/Command-API.md`
- `docs/api/Castle-API.md`
- `docs/api/Jobs-and-Flows-API.md`
- `docs/api/Teleport-and-Actions-API.md`
- `docs/api/Templates-and-ECS-Jobs-API.md`
- `docs/api/Mount-Chunk-Template-Notes.md`

## 6) Game Action APIs

`GameActionService` provides runtime actions for flow execution:

### Available Actions

| Action | Description | Parameters |
|--------|-------------|------------|
| `applybuff` | Apply buff to entity | userEntity, targetEntity, buffGuid, duration |
| `cleanbuff` | Clean all buffs from entity | targetEntity |
| `removebuff` | Remove specific buff | targetEntity, buffGuid |
| `teleport` | Teleport entity to position | targetEntity, position (float3) |
| `setposition` | Set entity position | targetEntity, position (float3) |
| `sendmessagetoall` | Send message to all players | message |
| `sendmessagetoplatform` | Send message to platform ID | platformId, message |
| `sendmessagetouser` | Send message to user entity | userEntity, message |
| `spawnboss` | Spawn boss entity | zoneId, x, y, z, level, prefabName |
| `removeboss` | Remove boss entity | zoneId |

### Using Actions

```csharp
// Direct invocation
var success = GameActionService.InvokeAction("applybuff", new object[] { 
    userEntity, 
    targetEntity, 
    new PrefabGUID(12345), 
    60f 
});

// Via flow
var entityMap = new EntityMap();
entityMap.Set("player", playerEntity);
entityMap.Set("target", targetEntity);
FlowService.Execute("apply_damage_buff", entityMap);
```

## 7) Lifecycle Snapshot Docs

- `docs/Lifecycle-Snapshot-Usage.md`

## 8) Command Docs

- `docs/api/Command-API.md`

## 9) Lifecycle Config Docs

- `docs/Lifecycle-Config-Reference.md`

## 10) File and Folder Map

- `docs/File-Folder-Map.md`

## 11) Flow and Command Auth

Job/flow execution is guarded by `ConsoleRoleAuthService`.

Core commands:
- `.coreauth login dev <password>`
- `.coreauth login admin <password>`
- `.coreauth status`
- `.coreauth logout`
- `.jobs run <flow>`

Important:
- Developer role is required for `.jobs run`
- Admin role alone is intentionally insufficient for job flow execution

## 12) Build and Deploy Commands

Core build:

```powershell
dotnet build VAutomationCore.csproj -c Release --nologo
```

Deploy (if project target supports it):

```powershell
dotnet build VAutomationCore.csproj -c Release --nologo --no-restore /p:DeployToServer=true
```

## 13) Recommended Module Startup

1. Ensure `UnifiedCore` is available.
2. Initialize logger and services through `ServiceInitializer`.
3. Register runtime services in `ServiceRegistry`.
4. Register VCF commands from module assembly.
5. Validate required config values before enabling gameplay logic.

## 14) Entity Alias Mapper

The framework provides component aliasing for easier flow scripting:

```csharp
// Register component aliases
EntityAliasMapper.RegisterComponentAlias<Health>("health");
EntityAliasMapper.RegisterComponentAlias<Destroying>("destroying");

// Use in flows
var entityMap = new EntityMap();
entityMap.Set("player", playerEntity);

if (EntityAliasMapper.HasComponent(em, entityMap, "player", "health", out var has, out var error))
{
    // Check component
}
```
