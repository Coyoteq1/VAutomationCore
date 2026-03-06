# ECS Utilities

VAutomationCore provides safe, optimized helpers for working with V Rising's Entity Component System (ECS). These utilities abstract away the complexity of Unity.Entities while providing performance optimizations and safety guarantees.

---

## 🧬 Why Use ECS Utilities?

Working directly with Unity.Entities in V Rising can be challenging:

* **Complex API** - Low-level entity queries and component access
* **Safety Risks** - Easy to corrupt game state with incorrect operations
* **Performance Pitfalls** - Inefficient queries can impact server performance
* **Memory Management** - Manual allocation and cleanup concerns

VAutomationCore's ECS utilities solve these problems with:

* **Fluent Query API** - Intuitive, readable entity queries
* **Safe Operations** - Built-in validation and error handling
* **Optimized Performance** - Chunk-based iteration and caching
* **Memory Safety** - Automatic cleanup and pooling

---

## 🏗️ Architecture Overview

The ECS Utilities module is composed of several sub-systems that work together to provide a safe and efficient way to interact with V Rising's Entity Component System (ECS).

## 🔍 Query Builder API

The fluent query API makes finding entities intuitive and efficient.

### **Basic Query Structure**

```csharp
// Find all players in a specific zone
var playersInZone = ECS.Query<PlayerCharacter>()
    .WithinZone("castle_area")
    .Execute();
```

### **Query Chain Methods**

| Method | Purpose | Example |
|--------|---------|---------|
| `WithinZone(string)` | Filter by zone | `.WithinZone("arena")` |
| `WithHealthBelow(float)` | Health filter | `.WithHealthBelow(50)` |
| `WithHealthAbove(float)` | Health filter | `.WithHealthAbove(75)` |
| `ByPrefab(PrefabGUID)` | Prefab filter | `.ByPrefab(PrefabGUID.Wolf)` |
| `Alive()` | Living entities only | `.Alive()` |
| `Dead()` | Dead entities only | `.Dead()` |
| `WithComponent<T>()` | Has component | `.WithComponent<Buff>()` |
| `WithoutComponent<T>()` | Missing component | `.WithoutComponent<MarkedForCleanup>()` |

### **Complex Query Examples**

```csharp
// Find wounded vampires in the arena
var woundedVampires = ECS.Query<VampireEntity>()
    .WithinZone("arena")
    .WithHealthBelow(30)
    .Alive()
    .WithComponent<Buff>()
    .Execute();

// Find all dead bosses for cleanup
var deadBosses = ECS.Query<BossEntity>()
    .Dead()
    .ByPrefab(PrefabGUID.Vampire_Lord)
    .WithoutComponent<ProtectedFromCleanup>()
    .Execute();
```

---

## 🛠️ Component Manipulation

Safe component access prevents common ECS errors and provides automatic validation.

### **Safe Component Modification**

```csharp
// Safe component modification with validation
var entity = player.GetEntity();
var success = ECS.SafeModifyComponent<Health>(entity, health => {
    if (health.Value > 0) {
        health.Value = Math.Max(0, health.Value - 10);
        return true; // Modification successful
    }
    return false; // Skip modification
});

if (!success) {
    Logger.Warning("Failed to modify health component");
}
```

### **Component Access Patterns**

```csharp
// Read component safely
if (ECS.TryGetComponent<Health>(entity, out var health)) {
    var currentHealth = health.Value;
    Logger.Info($"Player health: {currentHealth}");
}

// Add component with validation
if (ECS.CanAddComponent<Buff>(entity)) {
    ECS.AddComponent(entity, new Buff { Type = BuffType.Speed, Duration = 300 });
}

// Remove component safely
ECS.RemoveComponent<MarkedForCleanup>(entity);
```

---

## 📦 Batch Operations

Batch operations provide efficient ways to modify multiple entities at once.

### **Batch Modification**

```csharp
// Apply damage to all enemies in a zone
var enemies = ECS.Query<EnemyEntity>()
    .WithinZone("combat_zone")
    .Alive()
    .Execute();

ECS.BatchModify(enemies, (entity) => {
    ECS.SafeModifyComponent<Health>(entity, health => {
        health.Value -= 25;
        return true;
    });
});
```

### **Entity Cleanup**

```csharp
// Mark entities for cleanup
var cleanupTargets = ECS.Query<Entity>()
    .WithinZone("temporary_zone")
    .WithComponent<Temporary>()
    .Execute();

ECS.BatchModify(cleanupTargets, (entity) => {
    ECS.AddComponent<MarkedForCleanup>(entity);
});

// Process cleanup
ECS.ProcessMarkedEntities();
```

---

## ⚡ Performance Optimizations

VAutomationCore's ECS utilities are designed for high-performance server environments.

### **Chunk-Based Iteration**

The system uses Unity's chunk-based ECS architecture for optimal performance:

```csharp
// This query is optimized using chunk iteration
var allPlayers = ECS.Query<PlayerCharacter>()
    .Alive()
    .Execute(); // Processes entire chunks at once
```

### **Query Caching**

Frequently used queries are automatically cached:

```csharp
// First call builds and caches the query
var players1 = ECS.Query<PlayerCharacter>().WithinZone("arena").Execute();

// Subsequent calls use cached query plan
var players2 = ECS.Query<PlayerCharacter>().WithinZone("arena").Execute();
```

### **Memory Pooling**

Temporary objects are pooled to reduce garbage collection:

```csharp
// Results arrays are pooled and reused
var results = ECS.Query<PlayerCharacter>().Execute(); // Uses pooled memory
```

---

## 📊 Performance Metrics

| Operation | Performance | Notes |
|-----------|-------------|-------|
| **Simple Query** | < 1ms | Single component filter |
| **Complex Query** | 1-5ms | Multiple filters and conditions |
| **Batch Modify** | 0.1ms/entity | Chunk-based optimization |
| **Component Access** | < 0.1ms | Direct component lookup |
| **Memory Allocation** | Minimal | Pooling reduces GC pressure |

---

## 🛡️ Safety Features

### **Automatic Validation**

```csharp
// This won't crash if the entity doesn't exist
ECS.SafeModifyComponent<Health>(potentiallyInvalidEntity, health => {
    health.Value = 100; // Only runs if entity and component are valid
});
```

### **Error Handling**

```csharp
try {
    var entities = ECS.Query<PlayerCharacter>().Execute();
    Logger.Info($"Found {entities.Count} players");
}
catch (ECSQueryException ex) {
    Logger.Error($"Query failed: {ex.Message}");
}
```

### **Thread Safety**

All ECS operations are thread-safe and can be used from multiple contexts:

```csharp
// Safe to call from any thread
Task.Run(() => {
    var entities = ECS.Query<PlayerCharacter>().Execute();
    // Process entities asynchronously
});
```

---

## 🔧 Configuration

ECS performance can be tuned via configuration:

```json
{
  "ecs": {
    "chunk_size": 1024,
    "enable_caching": true,
    "cache_ttl": 60000,
    "max_query_results": 10000,
    "enable_profiling": false
  }
}
```

### **Configuration Options**

| Setting | Default | Description |
|---------|---------|-------------|
| `chunk_size` | 1024 | ECS chunk processing size |
| `enable_caching` | true | Enable query result caching |
| `cache_ttl` | 60000 | Cache time-to-live in milliseconds |
| `max_query_results` | 10000 | Maximum entities per query |
| `enable_profiling` | false | Enable performance profiling |

---

## 🎮 Integration Examples

### **Game Action Service Integration**

```csharp
public class SpawnBossAction : IFlowAction
{
    public void Execute(Entity target, Dictionary<string, object> args)
    {
        // Find suitable spawn locations
        var spawnPoints = ECS.Query<SpawnPoint>()
            .WithinZone(args["zone"].ToString())
            .WithoutComponent<Occupied>()
            .Execute();
            
        if (spawnPoints.Count > 0) {
            var spawnPoint = spawnPoints.First();
            GameActionService.SpawnEntity(bossPrefab, spawnPoint.Position);
        }
    }
}
```

### **Flow Integration**

```json
{
  "action": "ecs.query",
  "type": "PlayerCharacter",
  "filter": {
    "zone": "arena",
    "health_below": 50
  },
  "then": {
    "action": "zone.message",
    "message": "Low health players detected!"
  }
}
```

---

## 🚀 Advanced Features

### **Custom Query Extensions**

```csharp
// Create reusable query extensions
public static class ECSExtensions
{
    public static ECSQuery<PlayerCharacter> InCombat(this ECSQuery<PlayerCharacter> query)
    {
        return query.WithComponent<InCombat>()
                   .WithHealthBelow(100)
                   .Alive();
    }
}

// Use custom extensions
var fightingPlayers = ECS.Query<PlayerCharacter>()
    .InCombat()
    .WithinZone("arena")
    .Execute();
```

### **Query Profiling**

```csharp
// Enable profiling for performance analysis
ECS.EnableProfiling();

var results = ECS.Query<PlayerCharacter>().Execute();
var profile = ECS.GetLastQueryProfile();

Logger.Info($"Query took {profile.ExecutionTimeMs}ms, " +
          $"processed {profile.EntitiesProcessed} entities");
```

---

## 📚 Best Practices

### **Query Optimization**
1. **Use specific filters** - Avoid broad queries when possible
2. **Cache frequently used queries** - Let the system cache repeated patterns
3. **Batch operations** - Modify multiple entities in one operation
4. **Avoid hot loops** - Don't query every frame in tight loops

### **Memory Management**
1. **Reuse results** - Don't create unnecessary collections
2. **Process quickly** - Handle query results promptly
3. **Use batch operations** - Reduce individual entity operations
4. **Monitor memory** - Use profiling in development

### **Error Handling**
1. **Always validate** - Use safe modification methods
2. **Handle exceptions** - Wrap complex operations in try-catch
3. **Log important events** - Track query performance and failures
4. **Test edge cases** - Handle empty results and invalid entities

---

## 🔍 Debugging

### **Query Debugging**

```csharp
// Enable detailed logging
ECS.SetLogLevel(LogLevel.Debug);

// Get query explanation
var explanation = ECS.Query<PlayerCharacter>()
    .WithinZone("arena")
    .WithHealthBelow(50)
    .Explain();
    
Logger.Info($"Query plan: {explanation}");
```

### **Performance Monitoring**

```csharp
// Monitor query performance
var stopwatch = Stopwatch.StartNew();
var results = ECS.Query<PlayerCharacter>().Execute();
stopwatch.Stop();

if (stopwatch.ElapsedMilliseconds > 10) {
    Logger.Warning($"Slow query detected: {stopwatch.ElapsedMilliseconds}ms");
}
```

---

## 📖 Next Steps

Ready to dive deeper?

* **[Queries Deep Dive](queries.md)** - Advanced query techniques
* **[Component Helpers](components.md)** - Component manipulation patterns
* **[Performance Tuning](performance-tuning.md)** - Optimization guide

---

<div align="center">

**[🔝 Back to Top](#ecs-utilities)** • [**← Documentation Home**](../index.md)** • **[Queries Deep Dive →](queries.md)**

</div>
