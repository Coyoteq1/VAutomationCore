# Your First Flow

Learn to create your first automation flow with VAutomationCore. This guide will walk you through creating a simple zone-based automation that welcomes players when they enter a specific area.

---

## 🎯 What We'll Build

We'll create a simple flow that:

* Triggers when a player enters the "arena" zone
* Sends a welcome message to the player
* Logs the event for administrators

This demonstrates the core concepts: **triggers → actions → completion**.

---

## 📝 Step 1: Create the Flow Configuration

Create or edit `config/VAutomationCore/automation_rules.json`:

```json
{
  "flows": {
    "arena_welcome": {
      "triggers": [
        {
          "type": "zone.enter",
          "zone": "arena"
        }
      ],
      "actions": [
        {
          "action": "zone.message",
          "message": "⚔️ Welcome to the Arena! Fight with honor!",
          "type": "info"
        },
        {
          "action": "zone.log",
          "message": "Player entered arena zone",
          "level": "info"
        }
      ],
      "on_complete": [
        {
          "action": "zone.log",
          "message": "Arena welcome flow completed successfully",
          "level": "debug"
        }
      ]
    }
  }
}
```

---

## 🔧 Step 2: Wire It Into Your Mod

Update your plugin to load the automation rules:

```csharp
using BepInEx;
using VAutomationCore;
using VAutomationCore.Core.Automation;
using VAutomationCore.Core.Logging;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class MyModPlugin : BasePlugin
{
    private static CoreLogger Logger { get; set; }

    public override void Load()
    {
        Logger = new CoreLogger("MyMod");
        
        try
        {
            // Initialize VAutomationCore
            VAutoCore.Initialize();
            
            // Load automation rules
            LoadAutomationRules();
            
            Logger.LogInfo("MyMod with VAutomationCore loaded successfully!");
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to load MyMod: {ex.Message}");
        }
    }
    
    private void LoadAutomationRules()
    {
        var automationService = AutomationService.Instance;
        
        // Load rules from configuration file
        var rulesLoaded = automationService.LoadRulesFromFile();
        
        if (rulesLoaded > 0)
        {
            Logger.LogInfo($"Loaded {rulesLoaded} automation rules");
        }
        else
        {
            Logger.Warning("No automation rules loaded - check configuration file");
        }
    }
}
```

---

## 🧪 Step 3: Test the Flow

### **Method 1: In-Game Testing**

1. Start your server with the mod loaded
2. Enter the "arena" zone (or define a zone you can access)
3. Check for the welcome message in chat
4. Check server logs for the flow execution

### **Method 2: Manual Trigger Testing**

Add a test command to manually trigger the flow:

```csharp
[VampireCommandFramework.Command("testarena", "Test arena welcome flow")]
public static void TestArenaCommand(ChatCommandContext ctx)
{
    try
    {
        // Manually trigger the flow
        var success = FlowService.TryGetFlow("arena_welcome", out var flow);
        
        if (success)
        {
            ctx.Reply("✅ Arena welcome flow found!");
            
            // Execute with test context
            var result = FlowService.ExecuteFlow(flow, ctx.Event.SenderUserEntity);
            
            if (result.Success)
            {
                ctx.Reply($"✅ Flow executed successfully! ({result.ExecutedSteps} steps)");
            }
            else
            {
                ctx.Reply($"❌ Flow execution failed: {result.ErrorMessage}");
            }
        }
        else
        {
            ctx.Reply("❌ Arena welcome flow not found!");
        }
    }
    catch (Exception ex)
    {
        ctx.Reply($"❌ Error testing flow: {ex.Message}");
    }
}
```

---

## 🔍 Step 4: Monitor and Debug

### **Enable Verbose Logging**

Add this to your plugin initialization:

```csharp
// Enable verbose logging for debugging
VAutoCore.SetLogLevel(LogLevel.Debug);
```

### **Check Server Logs**

Look for these log entries:

```
[INFO] MyMod: Loaded 1 automation rules
[INFO] FlowService: Registered flow: arena_welcome
[DEBUG] FlowService: Trigger fired: zone.enter for zone: arena
[INFO] GameActionService: Executing action: zone.message
[INFO] GameActionService: Executing action: zone.log
[DEBUG] FlowService: Flow arena_welcome completed successfully
```

### **Common Issues & Solutions**

#### **Issue: Flow not found**
```
❌ Arena welcome flow not found!
```
**Solution:** Check that the flow name in your JSON matches exactly (case-sensitive).

#### **Issue: Zone not recognized**
```
❌ Zone 'arena' not found
```
**Solution:** Define the zone in your `zones.json` configuration:

```json
{
  "zones": {
    "arena": {
      "name": "Arena Zone",
      "center": { "x": 0, "y": 0, "z": 0 },
      "radius": 100
    }
  }
}
```

#### **Issue: Actions not executing**
```
❌ Flow execution failed: Action 'zone.message' not found
```
**Solution:** Ensure the action is registered. Built-in actions should be available automatically.

---

## 🚀 Step 5: Enhance Your Flow

Let's make the flow more sophisticated:

```json
{
  "flows": {
    "arena_welcome_enhanced": {
      "triggers": [
        {
          "type": "zone.enter",
          "zone": "arena"
        }
      ],
      "conditions": [
        {
          "type": "player.level",
          "operator": ">=",
          "value": 10
        }
      ],
      "actions": [
        {
          "action": "zone.message",
          "message": "⚔️ Welcome to the Arena, brave warrior!",
          "type": "info"
        },
        {
          "action": "player.applybuff",
          "buff": "arena_entrance_buff",
          "duration": 300
        },
        {
          "action": "zone.setpvp",
          "value": true
        },
        {
          "action": "zone.log",
          "message": "Player {{player_name}} entered arena (level {{player_level}})",
          "level": "info"
        }
      ],
      "on_complete": [
        {
          "action": "zone.message",
          "message": "🎯 May your aim be true and your blade sharp!",
          "type": "emote",
          "delay": 2000
        }
      ]
    }
  }
}
```

### **Enhanced Features Explained**

* **Conditions**: Only execute if player level >= 10
* **Multiple Actions**: Message, buff, PvP toggle, and logging
* **Template Variables**: `{{player_name}}` and `{{player_level}}`
* **Delayed Actions**: Send follow-up message after 2 seconds

---

## 📊 Flow Execution Results

When a flow executes, you get a `FlowExecutionResult`:

```csharp
public class FlowExecutionResult
{
    public bool Success { get; }
    public int TotalSteps { get; }
    public int ExecutedSteps { get; }
    public int FailedSteps { get; }
    public string ErrorMessage { get; }
    public TimeSpan ExecutionTime { get; }
    public Dictionary<string, object> Context { get; }
}
```

Use this for debugging and monitoring:

```csharp
var result = FlowService.ExecuteFlow(flow, playerEntity);

Logger.LogInfo($"Flow execution: {result.Success ? "✅" : "❌"} " +
              $"({result.ExecutedSteps}/{result.TotalSteps} steps) " +
              $"in {result.ExecutionTime.TotalMilliseconds:F2}ms");

if (!result.Success)
{
    Logger.LogError($"Flow failed: {result.ErrorMessage}");
}
```

---

## 🎯 Best Practices

### **1. Naming Conventions**
* Use descriptive flow names: `arena_welcome`, `boss_defeated_cleanup`
* Group related flows with prefixes: `arena_*`, `daily_events_*`

### **2. Error Handling**
* Always include `on_complete` handlers for cleanup
* Use conditional logic to prevent invalid states
* Log important events for debugging

### **3. Performance**
* Keep actions lightweight and fast
* Use async operations for heavy tasks
* Cache frequently used data

### **4. Testing**
* Test flows in development environment first
* Use manual trigger commands for debugging
* Monitor logs for execution patterns

---

## 🎉 Congratulations!

You've successfully created your first VAutomationCore flow! You now understand:

* ✅ **Flow Configuration** - JSON-based automation definitions
* ✅ **Triggers** - Events that start automation
* ✅ **Actions** - Operations performed by flows
* ✅ **Conditions** - Logic to control execution
* ✅ **Testing & Debugging** - How to verify flows work

---

## 🚀 Next Steps

Ready to continue your journey?

* **[First Command](first-command.md)** - Learn dynamic command system
* **[Flow System Deep Dive](../flows/overview.md)** - Advanced flow features
* **[Game Action Service](../game-actions/overview.md)** - Safe gameplay operations
* **[Configuration Guide](folder-layout.md)** - Organize your project

---

<div align="center">

**[🔝 Back to Top](#your-first-flow)** • [**← Installation**](installation.md)** • **[First Command →](first-command.md)**

</div>
