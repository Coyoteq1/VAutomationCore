# Glossary

This glossary defines key terms and concepts used throughout VAutomationCore documentation and codebase.

---

## 🔄 A

### **Action**
A single operation that can be executed within a flow. Actions are the building blocks of automation and include operations like spawning entities, sending messages, or modifying game state.

**Example**: `zone.message`, `spawn.prefab`, `player.modify`

### **Action Registry**
The system that manages and registers all available actions that can be used in flows. Provides validation and execution of actions.

### **Arena System**
A common use case demonstrating VAutomationCore capabilities, including PvP management, boss spawning, and player notifications.

### **Automation Rule**
A configuration that defines how a command should behave, including triggers, actions, and permissions.

---

## 🧬 B

### **Batch Operations**
Operations that affect multiple entities simultaneously, improving performance by reducing individual entity operations.

### **BepInEx**
A modding framework for Unity games that VAutomationCore builds upon for plugin loading and lifecycle management.

### **Boss Event**
A type of combat event specifically for boss encounters, including phases, defeat, and reward distribution.

### **Buff**
A temporary effect applied to players or entities that modifies their abilities or stats.

---

## 🧾 C

### **Command**
A text-based instruction that can be executed by players or administrators. VAutomationCore supports both static and dynamic commands.

### **Command Context**
The execution context for a command, containing information about the player, arguments, and execution environment.

### **Command Registry**
The system that manages dynamic command registration and execution.

### **Component**
A piece of data attached to an entity in the Entity Component System (ECS). Components define the properties and state of entities.

### **Configuration**
The set of JSON files that control VAutomationCore behavior, including flows, commands, zones, and framework settings.

### **Cross-Mod Communication**
The ability for different mods to communicate and share data with each other.

---

## 📡 D

### **Dynamic Command**
A command that can be registered, modified, or removed at runtime without restarting the server.

---

## 🧬 E

### **Entity**
An object in the game world that exists in the Entity Component System. Entities are containers for components.

### **Entity Component System (ECS)**
An architectural pattern used by V Rising for managing game objects. VAutomationCore provides safe utilities for working with ECS.

### **Event**
A notification that something has happened in the game world. Events can be published and subscribed to for decoupled communication.

### **Event Bus**
The system that manages event publishing and subscription. VAutomationCore provides a typed event bus for type-safe event handling.

### **Execute**
The process of running a flow, command, or action.

---

## 🔄 F

### **Flow**
A sequence of actions triggered by specific events. Flows are the core automation mechanism in VAutomationCore.

### **Flow Definition**
The complete specification of a flow, including triggers, conditions, actions, and completion handlers.

### **Flow Engine**
The system that manages flow registration, execution, and lifecycle.

### **Flow Execution Result**
The result of executing a flow, including success status, executed steps, and any errors.

---

## 🎮 G

### **Game Action Service**
The central service for safe gameplay operations, providing validated and transactional access to game systems.

### **Generated Documentation**
API documentation automatically generated from XML code comments.

---

## 🌊 H

### **Hot Reloading**
The ability to reload configuration files without restarting the server.

---

## 🧬 I

### **Interface**
A contract that defines the methods and properties that a class must implement.

---

## 🧾 J

### **JSON**
JavaScript Object Notation, the format used for VAutomationCore configuration files.

---

## 🌟 L

### **Lifecycle**
The sequence of states that an object or system goes through from creation to destruction.

---

## 🔧 M

### **Mod**
A plugin or modification that extends the functionality of V Rising.

### **Mod Communication Service**
The system that enables cross-mod communication and service discovery.

---

## 🧬 N

### **Namespace**
A way to organize code and prevent naming conflicts.

---

## 🧬 P

### **Parameter**
A value passed to an action, command, or function that controls its behavior.

### **Permission**
A level of access that determines what commands or actions a user can execute.

### **Player**
A human-controlled entity in the game world.

### **Prefab**
A template for creating entities in the game world.

### **PrefabGUID**
A unique identifier for a prefab in V Rising.

---

## 🧬 Q

### **Query**
A request for entities or components that match specific criteria.

---

## 🧬 R

### **Result**
The outcome of an operation, including success status and any relevant data or errors.

### **Rollback**
The process of undoing changes made during a failed transaction.

---

## 🧬 S

### **Safe Operation**
An operation that includes validation, error handling, and transaction support.

### **Service**
A system that provides specific functionality, such as the Game Action Service or Mod Communication Service.

### **Spawn**
The process of creating a new entity in the game world.

### **Subscriber**
A handler that receives events when they are published.

---

## 🧬 T

### **Transaction**
A group of operations that either all succeed together or all fail together, ensuring data consistency.

### **Trigger**
An event or condition that causes a flow to execute.

### **Typed Event Bus**
An event system that provides compile-time type checking for events.

---

## 🧬 U

### **Utility**
A helper function or class that simplifies common operations.

---

## 🧬 V

### **Validation**
The process of checking that input or configuration is correct and safe.

### **VAutoCore**
The main entry point for initializing VAutomationCore.

### **VampireCommandFramework**
A command framework for V Rising that VAutomationCore extends with dynamic capabilities.

---

## 🧬 W

### **World State**
The current state of the game world, including all entities, components, and systems.

---

## 🔍 Related Terms

### **V Rising Specific**
- **VBlood**: Special boss entities in V Rising
- **Castle**: Player home base in V Rising
- **Servant**: AI-controlled helpers in V Rising
- **Gear Level**: Equipment quality tiers in V Rising

### **General Programming**
- **API**: Application Programming Interface
- **SDK**: Software Development Kit
- **Framework**: A set of tools and libraries for building applications
- **Library**: A collection of reusable code

### **Game Development**
- **Server**: The computer that hosts the game world
- **Client**: The player's game instance
- **Network**: Communication between server and clients
- **Latency**: Delay in network communication

---

## 💡 Usage Examples

### **Flow Context**
```
"When a player enters the arena zone (trigger), the flow executes actions to enable PvP and spawn a boss."
```

### **ECS Context**
```
"Use ECS.Query<PlayerCharacter>() to find all players, then use SafeModifyComponent to safely change their health."
```

### **Event Context**
```
"When a boss is defeated, publish a BossDefeatedEvent that other systems can subscribe to."
```

### **Command Context**
```
"Players can use the '.spawnboss vampire' command to spawn a vampire boss at their location."
```

---

## 📚 Quick Reference

| Term | Category | Related To |
|------|----------|------------|
| **Flow** | Automation | Trigger, Action, Execution |
| **Entity** | ECS | Component, Query, Spawn |
| **Event** | Communication | Publish, Subscribe, Bus |
| **Command** | Interface | Registry, Context, Permission |
| **Transaction** | Safety | Rollback, Commit, Game Action |
| **Zone** | World State | PvP, Message, Population |

---

## 🔍 Finding Terms

### **By Category**
- **Automation**: Flow, Trigger, Action, Automation Rule
- **ECS**: Entity, Component, Query, Batch Operations
- **Communication**: Event, Bus, Subscriber, Cross-Mod
- **Interface**: Command, Context, Permission, Registry
- **Safety**: Transaction, Rollback, Validation, Safe Operation
- **Configuration**: JSON, Hot Reloading, Settings

### **By System**
- **Flow Engine**: Flow, Trigger, Action, Execution Result
- **ECS Utilities**: Entity, Component, Query, Safe Modify
- **Game Actions**: Transaction, Spawn, Service, Validation
- **Command System**: Command, Registry, Context, Permission
- **Event System**: Event, Bus, Publish, Subscribe
- **Communication**: Mod, Service, Discovery, Message

---

## 🚀 Learning Path

### **Beginner Terms**
Start with: **Flow**, **Entity**, **Command**, **Event**

These are the fundamental concepts you'll use most frequently.

### **Intermediate Terms**
Learn next: **Transaction**, **Query**, **Trigger**, **Permission**

These concepts enable more sophisticated automation and safety.

### **Advanced Terms**
Master last: **Component**, **PrefabGUID**, **Cross-Mod**, **Batch Operations**

These provide deep control and optimization capabilities.

---

## 📖 Additional Resources

### **Documentation**
- **[Flows Overview](../flows/overview.md)** - Flow system concepts
- **[ECS Overview](../ecs/overview.md)** - Entity Component System
- **[Commands Overview](../commands/overview.md)** - Command system
- **[Events Overview](../events/overview.md)** - Event system

### **Code Examples**
- **[API Reference](api-reference.md)** - Complete API documentation
- **[Examples Repository](https://github.com/Coyoteq1/VAutomationCore/tree/main/examples)** - Sample implementations
- **[Tutorials](https://github.com/Coyoteq1/VAutomationCore/tree/main/tutorials)** - Step-by-step guides

### **Community**
- **[Discord](https://discord.gg/uJ2ehWv4gR)** - Ask questions about terminology
- **[GitHub Issues](https://github.com/Coyoteq1/VAutomationCore/issues)** - Request clarification
- **[Stack Overflow](https://stackoverflow.com/questions/tagged/vautomationcore)** - Search for answers

---

<div align="center">

**[🔝 Back to Top](#glossary)** • [**← Documentation Home**](../index.md)** • **[API Reference →](api-reference.md)**

</div>
