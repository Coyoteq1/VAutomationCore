

# 🧠 VAutomationCore

**Automation and ECS utility framework for V Rising server mods.**

VAutomationCore provides a **modding foundation for server automation**, including a **flow engine, ECS helpers, dynamic command system, and cross-mod communication**.

Built for **BepInEx + VampireCommandFramework** and optimized for **V Rising’s hybrid ECS/GameObject architecture**.

---

# 👤 Author

**Coyoteq1**

* GitHub: [https://github.com/Coyoteq1/VAutomationCore](https://github.com/Coyoteq1/VAutomationCore)
* Discord: [https://discord.gg/uJ2ehWv4gR](https://discord.gg/uJ2ehWv4gR)

---

# 📦 Installation

### NuGet Package

Add the package to your mod project:

```xml
<PackageReference Include="VAutomationCore" Version="1.1.0" />
```

Or via CLI:

```
dotnet add package VAutomationCore
```

---

# ⚡ Features

## 🧬 ECS Utilities

Simplified helpers for working with **V Rising’s Entity Component System**.

Includes:

* EntityQuery helpers
* PrefabGUID utilities
* Component access helpers
* Safe entity operations
* ECS-friendly helper methods

Example:

```csharp
var entity = EntityManager.CreateEntity();
EntityManager.AddComponentData(entity, new Translation { Value = position });
```

---

## 🔄 Flow Automation System

Create **automation pipelines** triggered by events, commands, or gameplay systems.

Flow structure:

```
Trigger → Flow → Actions → Game Execution
```

Example configuration:

```json
{
  "flows": {
    "arena_enter": [
      { "action": "zone.setpvp", "value": true },
      { "action": "zone.message", "message": "⚔️ Entering Arena!" }
    ]
  }
}
```

---

## ⚙️ Action Registry

Register custom actions used by flows.

Example registration:

```csharp
FlowRegistry.RegisterAction("zone.message", new ZoneMessageAction());
```

Example action:

```csharp
public class ZoneMessageAction : IFlowAction
{
    public void Execute(Entity player, Dictionary<string, object> args)
    {
        var message = args["message"].ToString();
        GameActionService.SendMessage(player, message);
    }
}
```

---

## 🎮 Game Action Service

Central service for **safe server-side gameplay actions**.

Examples:

* Send player messages
* Spawn entities
* Modify ECS components
* Execute gameplay logic

This prevents **direct unsafe ECS manipulation inside flows**.

---

## 🧾 Dynamic Command System

Create and manage commands **at runtime without restarting the server**.

Example:

```
.vreg spawnwolf spawn prefab.wolf
```

Admin tools:

```
.vlist
.vremove <cmd>
.venable <cmd>
.vdisable <cmd>
.vtest <cmd>
.vclear
```

---

## 🔗 Cross-Mod Communication

Allows **mods to communicate with each other** using the ModCommunication system.

Example:

```
.crossmodscommands Blueluck zone status
```

Capabilities:

* Discover loaded mods
* Execute commands across mods
* Share automation events

---

## 📡 Event System (TypedEventBus)

Strongly-typed event bus for **mod-to-mod and internal communication**.

Example flow:

```
PlayerEnteredZoneEvent
      ↓
TypedEventBus
      ↓
Flow Engine
      ↓
Action Execution
```

Example usage:

```csharp
EventBus.Publish(new PlayerEnteredZoneEvent(player, zone));
```

---

# 📂 Configuration

Automation rules are stored in:

```
automation_rules.json
```

Example:

```json
{
  "commands": {
    "spawnwolf": {
      "actions": [
        { "action": "spawn.prefab", "prefab": "wolf" }
      ]
    }
  }
}
```

---

# 🧩 Framework Architecture

```
VAutomationCore
│
├── Flow Engine
├── Action Registry
├── Game Action Service
├── ECS Utilities
├── Dynamic Command System
├── ModCommunication Bridge
└── Typed Event Bus
```

Designed to support **large-scale server automation and multi-mod ecosystems**.

---

# 🔧 Dependencies

| Dependency              | Purpose           |
| ----------------------- | ----------------- |
| BepInEx                 | Mod runtime       |
| VampireCommandFramework | Command framework |
| Unity.Entities          | ECS integration   |

---

# 🧪 Example Use Cases

VAutomationCore can power:

* Arena event automation
* Boss encounter scripting
* Zone-triggered commands
* Server gameplay automation
* Cross-mod gameplay logic
* Runtime admin commands

---

# 📚 Documentation

Full documentation available in the repository:

[https://github.com/Coyoteq1/VAutomationCore](https://github.com/Coyoteq1/VAutomationCore)

Includes:

* Framework Wiki
* API Reference
* Example Implementations

---

# 🤝 Contributing

Contributions are welcome.

Recommended workflow:

1. Fork repository
2. Create feature branch
3. Submit pull request

---

# 📜 License

MIT License

---

# 🔖 Version

**Current Version:** `1.1.0`

---

If you want, I can also generate a **🔥 “top-tier GitHub README” upgrade** for this project with:

* shields.io **badges**
* **architecture diagrams**
* **flow engine visuals**
* **ECS workflow diagrams**
* **developer quickstart**

That would make **VAutomationCore look like a serious open-source framework**, not just a mod repo.
