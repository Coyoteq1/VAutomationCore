# Installation

This guide will help you install and set up VAutomationCore for your V Rising server mod.

---

## 📋 Requirements

### **System Requirements**
* **.NET 6.0** or higher
* **BepInEx 6.x** or higher
* **V Rising** server (latest version recommended)
* **VampireCommandFramework** 1.0 or higher

### **Development Environment**
* **Visual Studio 2022** or **JetBrains Rider**
* **NuGet Package Manager**
* Git for version control (recommended)

---

## 📦 Installation Methods

### **Method 1: NuGet Package (Recommended)**

#### Package Manager Console
```powershell
Install-Package VAutomationCore -Version 1.1.0
```

#### .NET CLI
```bash
dotnet add package VAutomationCore --version 1.1.0
```

#### PackageReference in .csproj
```xml
<PackageReference Include="VAutomationCore" Version="1.1.0" />
```

### **Method 2: Manual Installation**

1. **Download the latest release** from [GitHub Releases](https://github.com/Coyoteq1/VAutomationCore/releases)
2. **Extract the archive** to your mod project directory
3. **Add reference** to `VAutomationCore.dll` in your project
4. **Copy dependencies** to your mod's output directory

---

## ⚙️ Minimal BepInEx Plugin Setup

Create a basic plugin to verify VAutomationCore is working:

```csharp
using BepInEx;
using BepInEx.Logging;
using VAutomationCore;
using VAutomationCore.Core.Logging;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class MyModPlugin : BasePlugin
{
    private static ManualLogSource Logger { get; set; }

    public override void Load()
    {
        Logger = Log;
        
        try
        {
            // Initialize VAutomationCore
            VAutoCore.Initialize();
            
            Logger.LogInfo("VAutomationCore initialized successfully!");
            Logger.LogInfo($"VAutomationCore Version: {VAutoCore.Version}");
            
            // Test basic functionality
            TestVAutomationCore();
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to initialize VAutomationCore: {ex.Message}");
        }
    }
    
    private void TestVAutomationCore()
    {
        // Test Flow Service
        var flowCount = FlowService.GetFlowNames().Count;
        Logger.LogInfo($"Registered flows: {flowCount}");
        
        // Test TypedEventBus
        TypedEventBus.Subscribe<TestEvent>(OnTestEvent);
        Logger.LogInfo("Event bus subscription created");
        
        // Test Game Action Service
        Logger.LogInfo("Game Action Service available");
        
        Logger.LogInfo("✅ All VAutomationCore systems operational!");
    }
    
    private void OnTestEvent(TestEvent evt)
    {
        Logger.LogInfo($"Received test event: {evt.Message}");
    }
}

// Test event for verification
public record TestEvent(string Message);
```

### **PluginInfo.cs**
```csharp
public static class PluginInfo
{
    public const string PLUGIN_GUID = "com.yourmod.vautomation";
    public const string PLUGIN_NAME = "Your VAutomation Mod";
    public const string PLUGIN_VERSION = "1.0.0";
}
```

---

## 🔍 Verification Steps

### **1. Check Console Output**

After starting your server with the plugin, you should see:

```
[INFO] Your VAutomation Mod: VAutomationCore initialized successfully!
[INFO] Your VAutomation Mod: VAutomationCore Version: 1.1.0
[INFO] Your VAutomation Mod: Registered flows: 0
[INFO] Your VAutomation Mod: Event bus subscription created
[INFO] Your VAutomation Mod: Game Action Service available
[INFO] Your VAutomation Mod: ✅ All VAutomationCore systems operational!
```

### **2. Test Command System**

Create a simple test command:

```csharp
[VampireCommandFramework.Command("vtest", "Test VAutomationCore functionality")]
public static class TestCommands
{
    [VampireCommandFramework.Command("ping")]
    public static void PingCommand(ChatCommandContext ctx)
    {
        ctx.Reply("🏓 VAutomationCore is working!");
    }
}
```

Test it in-game: `.vtest ping`

### **3. Verify Configuration Files**

Check that these files are created in `BepInEx/config/VAutomationCore/`:

```
VAuto.unified_config.json
automation_rules.json
```

---

## 📁 Project Structure

A typical VAutomationCore mod project looks like this:

```
YourMod/
├── YourMod.csproj
├── Plugin.cs
├── PluginInfo.cs
├── config/
│   └── VAutomationCore/
│       ├── VAuto.unified_config.json
│       ├── automation_rules.json
│       └── zones.json
└── packages/
    └── VAutomationCore.1.1.0/
```

---

## 🚨 Common Issues & Troubleshooting

### **Issue: "VAutomationCore not found"**
**Solution:** Ensure the NuGet package is properly installed and referenced in your project.

### **Issue: "BepInEx not found"**
**Solution:** Install BepInEx 6.x or higher before running your mod.

### **Issue: "VampireCommandFramework missing"**
**Solution:** Install VampireCommandFramework as a dependency:
```xml
<PackageReference Include="VampireCommandFramework" Version="1.0.0" />
```

### **Issue: Configuration files not created**
**Solution:** Ensure your plugin has write permissions to the config directory.

### **Issue: Events not firing**
**Solution:** Make sure you're subscribing to events after VAutoCore.Initialize().

---

## 📚 Next Steps

Once VAutomationCore is installed and verified:

1. **[Create your first flow](first-flow.md)** - Learn the automation system
2. **[Create your first command](first-command.md)** - Dynamic command system
3. **[Set up configuration](folder-layout.md)** - Organize your project
4. **[Explore core concepts](../concepts/architecture-overview.md)** - Understand the framework

---

## 💡 Pro Tips

### **Development Tips**
* Use **hot reload** for faster development iteration
* Enable **verbose logging** during development
* Test flows in a **development environment** first

### **Production Tips**
* Use **semantic versioning** for your releases
* Implement **proper error handling** in production
* Monitor **performance metrics** in live environments

### **Configuration Management**
* **Version control** your configuration files
* Use **environment-specific** configs
* **Backup** automation rules before updates

---

## 🎯 You're Ready!

VAutomationCore is now installed and ready to use. You can:

* ✅ Create automation flows
* ✅ Register dynamic commands  
* ✅ Use ECS utilities safely
* ✅ Communicate between mods
* ✅ Handle typed events

**Ready to build something amazing?** Start with [**First Flow**](first-flow.md) to create your first automation!

---

<div align="center">

**[🔝 Back to Top](#installation)** • [**← Documentation Home**](../index.md)** • **[First Flow →](first-flow.md)**

</div>
