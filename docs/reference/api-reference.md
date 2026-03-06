# API Reference

This section provides comprehensive API documentation for VAutomationCore. The complete API reference is available as generated documentation, but this guide will help you navigate the most important namespaces and classes.

---

## 📚 Generated Documentation

The complete API documentation is generated from XML comments and available at:

**[📖 Complete API Documentation](https://github.com/Coyoteq1/VAutomationCore/docs/api)**

*Generated using DocFX from XML documentation comments*

---

## 🗂️ Namespace Overview

### **VAutomationCore.Core**
Core framework initialization and main entry points.

```csharp
// Main entry point
public static class VAutoCore
{
    public static void Initialize()
    public static string Version { get; }
    public static bool IsInitialized { get; }
}

// Core logger
public class CoreLogger
{
    public CoreLogger(string context)
    public void Info(string message)
    public void Warning(string message)
    public void Error(string message)
    public void Debug(string message)
}
```

### **VAutomationCore.Flows**
Flow automation engine and execution system.

```csharp
// Flow service for registration and execution
public static class FlowService
{
    public static bool RegisterFlow(string name, IEnumerable<FlowStep> steps)
    public static bool TryGetFlow(string name, out FlowDefinition definition)
    public static FlowExecutionResult ExecuteFlow(FlowDefinition definition, Entity context)
    public static IReadOnlyCollection<string> GetFlowNames()
}

// Flow definition and execution
public class FlowDefinition
{
    public string Name { get; }
    public IReadOnlyList<FlowStep> Steps { get; }
}

public class FlowExecutionResult
{
    public bool Success { get; }
    public int TotalSteps { get; }
    public int ExecutedSteps { get; }
    public string ErrorMessage { get; }
    public TimeSpan ExecutionTime { get; }
}
```

### **VAutomationCore.ECS**
Entity Component System utilities and helpers.

```csharp
// Main ECS query builder
public static class ECS
{
    public static ECSQuery<T> Query<T>() where T : IComponentData
    public static bool SafeModifyComponent<T>(Entity entity, Action<T> modifier) where T : IComponentData
    public static bool TryGetComponent<T>(Entity entity, out T component) where T : IComponentData
    public static void BatchModify(IEnumerable<Entity> entities, Action<Entity> action)
}

// Fluent query API
public class ECSQuery<T> where T : IComponentData
{
    public ECSQuery<T> WithinZone(string zoneName)
    public ECSQuery<T> WithHealthBelow(float health)
    public ECSQuery<T> WithHealthAbove(float health)
    public ECSQuery<T> ByPrefab(PrefabGUID prefab)
    public ECSQuery<T> Alive()
    public ECSQuery<T> Dead()
    public IReadOnlyList<Entity> Execute()
}
```

### **VAutomationCore.Commands**
Dynamic command system and registration.

```csharp
// Command registry for dynamic registration
public static class CommandRegistry
{
    public static void RegisterCommand<T>() where T : BaseCommand, new()
    public static void RegisterCommand(string name, BaseCommand command)
    public static bool UnregisterCommand(string name)
    public static bool TryGetCommand(string name, out BaseCommand command)
}

// Base command class
public abstract class BaseCommand
{
    public abstract Task ExecuteAsync(CommandContext context);
    public virtual bool HasPermission(CommandContext context) => true;
}

// Command context
public class CommandContext
{
    public Entity Player { get; }
    public string CommandName { get; }
    public string[] Args { get; }
    public Task ReplyAsync(string message, bool isError = false)
    public bool HasPermission(string permission)
}
```

### **VAutomationCore.GameActions**
Safe gameplay operations and entity management.

```csharp
// Main game action service
public static class GameActionService
{
    public static Task<SpawnResult> SpawnEntitiesAsync(PrefabGUID prefab, Vector3 position, int count, SpawnContext context = null)
    public static Task<bool> SetZonePvPAsync(string zoneName, bool enabled)
    public static Task<bool> ApplyBuffAsync(Entity target, PrefabGUID buff, float duration = 0)
    public static Task<bool> SendMessageToPlayerAsync(Entity player, string message)
    public static ITransaction BeginTransaction()
}

// Transaction support
public interface ITransaction : IDisposable
{
    Task CommitAsync()
    Task RollbackAsync()
    bool IsCommitted { get; }
}

// Spawn result
public class SpawnResult
{
    public bool Success { get; }
    public IReadOnlyList<Entity> SpawnedEntities { get; }
    public string ErrorMessage { get; }
}
```

### **VAutomationCore.Communication**
Cross-mod communication and service discovery.

```csharp
// Mod communication service
public static class ModCommunication
{
    public static Task<IEnumerable<ModInfo>> DiscoverModsAsync()
    public static Task<CommandResult> ExecuteCommandAsync(string targetMod, string command, object parameters = null)
    public static void BroadcastEvent<T>(T eventData) where T : class
    public static void RegisterEventHandler<T>(Action<T> handler) where T : class
}

// Mod information
public class ModInfo
{
    public string Name { get; }
    public string Version { get; }
    public IReadOnlyList<string> AvailableCommands { get; }
    public IReadOnlyDictionary<string, object> Metadata { get; }
}
```

### **VAutomationCore.Events**
Typed event bus for event-driven architecture.

```csharp
// Typed event bus
public static class TypedEventBus
{
    public static void Subscribe<T>(Action<T> handler) where T : class
    public static IDisposable SubscribeScoped<T>(Action<T> handler) where T : class
    public static void Unsubscribe<T>(Action<T> handler) where T : class
    public static void Publish<T>(T evt) where T : class
    public static int PublishAndCount<T>(T evt) where T : class
    public static Task PublishAsync<T>(T evt) where T : class
    public static void PublishBatch<T>(IEnumerable<T> events) where T : class
}

// Built-in event types
public record PlayerEnteredZoneEvent(Entity Player, string ZoneName, DateTime Timestamp);
public record PlayerDeathEvent(Entity Player, Entity Killer, Vector3 Position, DateTime Timestamp);
public record BossDefeatedEvent(Entity Boss, Entity Killer, string ZoneName, TimeSpan CombatDuration, DateTime Timestamp);
public record ZoneStateChangedEvent(string ZoneName, ZoneState OldState, ZoneState NewState, DateTime Timestamp);
```

---

## 🔧 Key Classes and Interfaces

### **IComponentData**
Base interface for ECS components used in queries.

```csharp
public interface IComponentData
{
    // Marker interface for ECS components
}
```

### **IFlowAction**
Interface for custom flow actions.

```csharp
public interface IFlowAction
{
    string ActionType { get; }
    Task ExecuteAsync(Entity target, Dictionary<string, object> parameters);
    ValidationResult ValidateParameters(Dictionary<string, object> parameters);
}
```

### **ICommand**
Interface for dynamic commands.

```csharp
public interface ICommand
{
    string Name { get; }
    string Description { get; }
    Task<CommandResult> ExecuteAsync(CommandContext context);
    bool HasPermission(CommandContext context);
}
```

### **IGameAction**
Interface for game action implementations.

```csharp
public interface IGameAction
{
    string ActionType { get; }
    Task<ActionResult> ExecuteAsync(GameActionContext context);
    bool ValidateParameters(GameActionContext context);
}
```

---

## 📖 Common Usage Patterns

### **Flow Registration**

```csharp
// Register a simple flow
var steps = new List<FlowStep>
{
    new FlowStep("zone.message", new Dictionary<string, object>
    {
        ["message"] = "Welcome to the arena!",
        ["type"] = "info"
    })
};

FlowService.RegisterFlow("arena_welcome", steps);

// Execute a flow
if (FlowService.TryGetFlow("arena_welcome", out var flow))
{
    var result = FlowService.ExecuteFlow(flow, playerEntity);
    if (result.Success)
    {
        Logger.Info($"Flow executed successfully: {result.ExecutedSteps} steps");
    }
}
```

### **ECS Queries**

```csharp
// Find all players in a zone with low health
var woundedPlayers = ECS.Query<PlayerCharacter>()
    .WithinZone("arena")
    .WithHealthBelow(30)
    .Alive()
    .Execute();

// Safe component modification
foreach (var player in woundedPlayers)
{
    ECS.SafeModifyComponent<Health>(player, health =>
    {
        health.Value = Math.Min(health.Value + 10, health.MaxHealth);
        return true;
    });
}
```

### **Game Actions**

```csharp
// Spawn entities with transaction support
using var transaction = GameActionService.BeginTransaction();
try
{
    var spawnResult = await GameActionService.SpawnEntitiesAsync(
        PrefabGUID.Wolf,
        new Vector3(0, 0, 0),
        5,
        new SpawnContext { Zone = "forest_area" }
    );
    
    if (spawnResult.Success)
    {
        await GameActionService.SendMessageToPlayerAsync(
            playerEntity, 
            $"Spawned {spawnResult.SpawnedEntities.Count} wolves!"
        );
    }
    
    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
}
```

### **Event Handling**

```csharp
// Subscribe to events
TypedEventBus.Subscribe<PlayerEnteredZoneEvent>(OnPlayerEnteredZone);
TypedEventBus.Subscribe<BossDefeatedEvent>(OnBossDefeated);

// Event handlers
private void OnPlayerEnteredZone(PlayerEnteredZoneEvent evt)
{
    Logger.Info($"Player entered zone: {evt.ZoneName}");
    // Handle zone entry logic
}

private void OnBossDefeated(BossDefeatedEvent evt)
{
    Logger.Info($"Boss defeated in {evt.ZoneName}");
    // Handle boss defeat logic
}

// Publish events
TypedEventBus.Publish(new PlayerEnteredZoneEvent(playerEntity, "arena", DateTime.UtcNow));
```

---

## 🔍 Finding What You Need

### **For Flow Automation**
- **Namespace**: `VAutomationCore.Flows`
- **Key Classes**: `FlowService`, `FlowDefinition`, `FlowExecutionResult`
- **Start Here**: [Flows Overview](../flows/overview.md)

### **For ECS Operations**
- **Namespace**: `VAutomationCore.ECS`
- **Key Classes**: `ECS`, `ECSQuery<T>`
- **Start Here**: [ECS Overview](../ecs/overview.md)

### **For Safe Game Operations**
- **Namespace**: `VAutomationCore.GameActions`
- **Key Classes**: `GameActionService`, `SpawnResult`, `ITransaction`
- **Start Here**: [Game Actions Overview](../game-actions/overview.md)

### **For Dynamic Commands**
- **Namespace**: `VAutomationCore.Commands`
- **Key Classes**: `CommandRegistry`, `BaseCommand`, `CommandContext`
- **Start Here**: [Commands Overview](../commands/overview.md)

### **For Cross-Mod Communication**
- **Namespace**: `VAutomationCore.Communication`
- **Key Classes**: `ModCommunication`, `ModInfo`, `CommandResult`
- **Start Here**: [Communication Overview](../communication/overview.md)

### **For Event-Driven Architecture**
- **Namespace**: `VAutomationCore.Events`
- **Key Classes**: `TypedEventBus`, built-in event types
- **Start Here**: [Events Overview](../events/overview.md)

---

## 🚀 Advanced API Usage

### **Custom Flow Actions**

```csharp
public class CustomTeleportAction : IFlowAction
{
    public string ActionType => "zone.teleport";
    
    public async Task ExecuteAsync(Entity target, Dictionary<string, object> parameters)
    {
        var zoneName = parameters["zone"].ToString();
        var zone = ZoneService.GetZone(zoneName);
        
        if (zone != null)
        {
            await GameActionService.TeleportEntityAsync(target, zone.SpawnPoint);
            TypedEventBus.Publish(new PlayerTeleportedEvent(target, zone));
        }
    }
    
    public ValidationResult ValidateParameters(Dictionary<string, object> parameters)
    {
        if (!parameters.ContainsKey("zone"))
            return ValidationResult.Error("Zone parameter is required");
            
        return ValidationResult.Success();
    }
}
```

### **Custom Commands**

```csharp
[Command("customcommand", "Custom command example")]
public class CustomCommand : BaseCommand
{
    [CommandParam("target", "Target player")]
    public string Target { get; set; }
    
    [CommandParam("action", "Action to perform")]
    public string Action { get; set; }
    
    public override async Task ExecuteAsync(CommandContext ctx)
    {
        // Command implementation
        await ctx.ReplyAsync($"Executed {Action} on {Target}");
    }
}
```

### **Custom Events**

```csharp
// Define custom event
public record CustomGameEvent(
    string EventType,
    Entity Source,
    Dictionary<string, object> Data,
    DateTime Timestamp
) : IEvent;

// Publish custom event
TypedEventBus.Publish(new CustomGameEvent(
    "SpecialAction",
    playerEntity,
    new Dictionary<string, object> { { "action", "dance" } },
    DateTime.UtcNow
));
```

---

## 📚 Additional Resources

### **Generated Documentation**
- **Complete API Reference**: [Online Documentation](https://github.com/Coyoteq1/VAutomationCore/docs/api)
- **XML Documentation**: Included in NuGet package
- **Sample Code**: [GitHub Examples](https://github.com/Coyoteq1/VAutomationCore/tree/main/examples)

### **Development Tools**
- **IntelliSense Support**: Full XML documentation included
- **API Browser**: Built-in API documentation in Visual Studio
- **Code Examples**: Comprehensive sample implementations

### **Community Resources**
- **Discord**: [API Discussion Channel](https://discord.gg/uJ2ehWv4gR)
- **GitHub Issues**: [API Questions](https://github.com/Coyoteq1/VAutomationCore/issues)
- **Stack Overflow**: Tag with `VAutomationCore`

---

## 🔧 API Versioning

VAutomationCore follows Semantic Versioning:

* **Major (X.0.0)**: Breaking changes to public API
* **Minor (X.Y.0)**: New features, backward-compatible changes
* **Patch (X.Y.Z)**: Bug fixes, internal improvements

### **Version Compatibility**

| Version | Status | API Changes |
|---------|--------|-------------|
| 1.0.x | Stable | Initial release API |
| 1.1.x | Current | Added transaction support, enhanced ECS |
| 2.0.0 | Future | Planned breaking changes (if needed) |

---

## 📖 Next Steps

Ready to dive deeper?

* **[Complete API Documentation](https://github.com/Coyoteq1/VAutomationCore/docs/api)** - Full generated docs
* **[Glossary](glossary.md)** - Key terms and concepts
* **[Examples](https://github.com/Coyoteq1/VAutomationCore/tree/main/examples)** - Sample implementations

---

<div align="center">

**[🔝 Back to Top](#api-reference)** • [**← Documentation Home**](../index.md)** • **[Glossary →](glossary.md)**

</div>
