# VAutomationCore - Developer API Reference

This is the developer documentation for the VAutomationCore NuGet package. For end-user documentation, see the main [README.md](./README.md).

## Related Projects

The VAutomationCore ecosystem includes several standalone plugins that can be used independently:

| Project | Description | Repository |
|---------|-------------|------------|
| **Bluelock** | Zone/Arena management with kit system, glow borders, sandbox progression | [GitHub](https://github.com/Coyoteq1/bluelock) |
| **CycleBorn** | Lifecycle management with arena snapshots, PvP flow system | [GitHub](https://github.com/Coyoteq1/cycleborn) |

### Quick Integration

These plugins integrate with VAutomationCore through the Flow API:

```csharp
// Use FlowService to trigger zone actions
var entityMap = new EntityMap();
entityMap.Set("zone", zoneEntity);
FlowService.Execute("zone.enter", entityMap);
```

## Installation

### NuGet Package
```xml
<PackageReference Include="VAutomationCore" Version="1.0.0" />
```

For prerelease versions:
```xml
<PackageReference Include="VAutomationCore" Version="1.0.1-beta.3" />
```

- NuGet: https://www.nuget.org/packages/VAutomationCore
- Latest prerelease: https://www.nuget.org/packages/VAutomationCore/1.0.1-beta.3

## Quick Start

```csharp
using System.Reflection;
using VampireCommandFramework;
using VAutomationCore.Core.Api;
using VAutomationCore.Core.Logging;
using VAutomationCore.Core.Services;

public override void Load()
{
    var log = new CoreLogger("Module");
    ServiceInitializer.InitializeLogger(log);
    ServiceInitializer.RegisterInitializer("your_service", YourService.Initialize);
    ServiceInitializer.RegisterValidator("your_service", () => YourService.IsReady);
    ServiceInitializer.InitializeAll(log);

    CommandRegistry.RegisterAll(Assembly.GetExecutingAssembly());
}
```

## Core API Surface

### Runtime/Execution
- `CoreExecution`: Safe sync/async execution wrappers with retry support
- `OperationResult` / `OperationResult<T>`: Standard success/failure return model
- `RetryPolicy`: Retry configuration for resilient operations

```csharp
var op = CoreExecution.RunWithRetry(
    () => { /* work */ },
    operationName: "startup-work",
    retryPolicy: RetryPolicy.Default,
    logger: logger
);
```

### Service/State
- `ServiceRegistry`: Singleton registration/resolution for module services
- `EntityMap`: Alias-to-entity reference map used by flow/job execution
- `EntityAliasMapper`: Component alias registration + component query/set helpers

### Flow APIs
- `FlowService`: Register, resolve, and execute action flows
- `FlowDefinition` / `FlowStep`: Flow model types
- `FlowExecutionResult`: Execution outcome with success/failure details

```csharp
FlowService.RegisterActionAlias("heal", "HealSelf");
FlowService.RegisterFlow("startup", new[]
{
    new FlowStep("heal")
}, replace: true);

var map = new EntityMap();
var result = FlowService.Execute("startup", map);
```

### Game Action APIs
- `GameActionService`: Runtime action helpers for flow execution
- Supports dynamic action invocation with typed arguments

Available Actions:
| Action | Description | Arguments |
|--------|-------------|-----------|
| `applybuff` | Apply buff to entity | userEntity, targetEntity, buffGuid, duration (optional) |
| `cleanbuff` | Clean all buffs from entity | targetEntity |
| `removebuff` | Remove specific buff | targetEntity, buffGuid |
| `teleport` | Teleport entity to position | targetEntity, position (float3) |
| `setposition` | Set entity position | targetEntity, position (float3) |
| `sendmessagetoall` | Send message to all players | message |
| `sendmessagetoplatform` | Send message to platform ID | platformId, message |
| `sendmessagetouser` | Send message to user entity | userEntity, message |
| `spawnboss` | Spawn boss entity | zoneId, x, y, z, level, prefabName |
| `removeboss` | Remove boss entity | zoneId |

```csharp
// Direct action invocation
GameActionService.InvokeAction("applybuff", new object[] { userEntity, targetEntity, buffGuid, 60f });

// Via flow execution
var map = new EntityMap();
map.Set("player", playerEntity);
FlowService.Execute("spawn_boss", map);
```

### Auth/Console APIs
- `ConsoleRoleAuthService`: Admin/developer auth session handling
- `CoreAuthCommands`: Built-in VCF commands (`.coreauth ...`)
- `CoreJobFlowCommands`: Built-in VCF commands (`.jobs ...`)

### Configuration
- `ConfigService<T>`: Generic config file management with JSON serialization
- `ServiceInitializer`: Startup orchestration for services
- `JsonConfigManager`: JSON-based configuration management

### Server Access
- `UnifiedCore`: Safe access to server ECS objects

```csharp
if (!UnifiedCore.IsInitialized)
{
    // module can defer work until server world is available
    return;
}

var world = UnifiedCore.Server;
var em = UnifiedCore.EntityManager;
```

## Built-in Commands

### Auth Commands (`.coreauth`)
- `.coreauth login dev <password>` - Developer login
- `.coreauth login admin <password>` - Admin login
- `.coreauth status` - Check auth status
- `.coreauth logout` - End session

### Job Commands (`.jobs`)
- `.jobs flow add/remove/list` - Manage flows
- `.jobs alias self/user/clear/list` - Manage aliases
- `.jobs component alias add/list` - Manage component aliases
- `.jobs component has <entity> <component>` - Check component existence
- `.jobs run <flow>` - Execute flow (requires Developer auth)

## Service Registration Pattern

```csharp
// 1. Create a service class
public class MyService
{
    public static bool IsReady { get; private set; }
    
    public static void Initialize(CoreLogger log)
    {
        log.LogInfo("Initializing MyService...");
        // Initialize logic
        IsReady = true;
    }
}

// 2. Register in Load()
ServiceInitializer.RegisterInitializer("myservice", MyService.Initialize);
ServiceInitializer.RegisterValidator("myservice", () => MyService.IsReady);
ServiceInitializer.InitializeAll(log);
```

## ECS Integration

The framework provides helpers for working with V Rising's ECS system:

- Use predefined `EntityQueries` when possible
- Always dispose `NativeArray` with try-finally blocks
- Check component existence with `EntityManager.HasComponent<T>()`

```csharp
var entities = query.ToEntityArray(Allocator.Temp);
try
{
    foreach (var entity in entities)
    {
        if (!EntityManager.HasComponent<SomeComponent>(entity)) continue;
        // Process entity
    }
}
finally
{
    entities.Dispose();
}
```

## Flow Action Reference

### Available Flow Actions

| Action Name | Alias | Description | Parameters |
|-------------|-------|-------------|------------|
| `ApplyBuff` | `applybuff` | Apply buff to target | targetEntity, buffGuid, duration |
| `CleanBuff` | `cleanbuff` | Clean all buffs | targetEntity |
| `RemoveBuff` | `removebuff` | Remove specific buff | targetEntity, buffGuid |
| `Teleport` | `teleport` | Teleport entity | targetEntity, position |
| `SetPosition` | `setposition` | Set entity position | targetEntity, position |
| `SendMessageToAll` | `sendmessagetoall` | Broadcast message | message |
| `SendMessageToPlatform` | `sendmessagetoplatform` | Send to platform ID | platformId, message |
| `SendMessageToUser` | `sendmessagetouser` | Send to user entity | userEntity, message |
| `SpawnBoss` | `spawnboss` | Spawn boss entity | zoneId, x, y, z, level, prefabName |
| `RemoveBoss` | `removeboss` | Remove boss entity | zoneId |

### Custom Flow Actions

Register custom actions:

```csharp
FlowService.RegisterActionAlias("my_action", "CustomActionName");
FlowService.RegisterFlow("custom_flow", new[]
{
    new FlowStep("my_action", new Dictionary<string, object>
    {
        { "arg1", "value1" },
        { "arg2", 123 }
    })
}, replace: true);
```

## Namespace Reference

| Namespace | Purpose |
|-----------|---------|
| `VAutomationCore.Core` | Core services and utilities |
| `VAutomationCore.Core.Api` | Public API (FlowService, EntityMap, etc.) |
| `VAutomationCore.Core.Services` | Service infrastructure |
| `VAutomationCore.Core.Logging` | Logging abstractions |
| `VAutomationCore.Core.Commands` | Command handlers |
| `VAutomationCore.Core.Config` | Configuration management |
| `VAutomationCore.Core.ECS` | ECS helpers and extensions |
| `VAutomationCore.Abstractions` | Abstract types and contracts |

## Related Documentation

- [Jobs and Flows API](./api/Jobs-and-Flows-API.md)
- [Server API](./api/Server-API.md)
- [Player API](./api/Player-API.md)
- [Command API](./api/Command-API.md)
- [Templates and ECS Jobs API](./api/Templates-and-ECS-Jobs-API.md)
- [Core System Usage](./Core-System-Usage.md)
- [Lifecycle Snapshot Usage](./Lifecycle-Snapshot-Usage.md)

## Support

- Discord: [V Rising Mods Community](https://discord.gg/68JZU5zaq7)
- Issues: https://github.com/Coyoteq1/D-VAutomationCore-VAutomationCore/issues
