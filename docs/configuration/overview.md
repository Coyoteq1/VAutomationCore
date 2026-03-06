# Configuration System

VAutomationCore uses a comprehensive configuration system that allows you to customize behavior, enable/disable features, and tune performance settings without modifying code.

---

## 📂 Configuration Overview

All configuration files are located in the `config/VAutomationCore/` directory:

```
config/VAutomationCore/
├── VAuto.unified_config.json     # Main framework configuration
├── automation_rules.json          # Flow automation definitions
├── commands.json                  # Dynamic command definitions
└── zones.json                     # Zone definitions and settings
```

### **Configuration Loading**

* **Hot Reloading** - Most configuration files support hot reloading
* **Validation** - All configurations are validated before applying
* **Fallbacks** - Default values are used when configuration is missing
* **Error Handling** - Invalid configurations are logged with helpful error messages

---

## ⚙️ Unified Configuration

The main configuration file controls core framework behavior:

### **VAuto.unified_config.json Structure**

```json
{
  "framework": {
    "log_level": "Info",
    "enable_metrics": true,
    "max_concurrent_flows": 100,
    "transaction_timeout": 30000,
    "enable_performance_monitoring": false
  },
  "ecs": {
    "chunk_size": 1024,
    "enable_caching": true,
    "cache_ttl": 60000,
    "max_query_results": 10000,
    "enable_profiling": false
  },
  "game_actions": {
    "enable_transactions": true,
    "transaction_timeout": 30000,
    "max_concurrent_operations": 100,
    "enable_audit_logging": true,
    "validation_level": "strict"
  },
  "commands": {
    "enable_rate_limiting": true,
    "max_commands_per_minute": 30,
    "enable_audit_logging": true,
    "enable_hot_reload": true
  },
  "events": {
    "enable_performance_monitoring": false,
    "max_subscribers_per_event": 100,
    "enable_event_history": false,
    "event_history_size": 1000
  },
  "communication": {
    "enable_cross_mod": true,
    "discovery_interval": 30000,
    "timeout": 5000,
    "max_message_size": 1024
  }
}
```

### **Framework Settings**

| Setting | Default | Description |
|---------|---------|-------------|
| `log_level` | Info | Minimum log level (Debug, Info, Warning, Error) |
| `enable_metrics` | true | Enable performance metrics collection |
| `max_concurrent_flows` | 100 | Maximum flows running simultaneously |
| `transaction_timeout` | 30000 | Transaction timeout in milliseconds |
| `enable_performance_monitoring` | false | Enable detailed performance tracking |

### **ECS Settings**

| Setting | Default | Description |
|---------|---------|-------------|
| `chunk_size` | 1024 | ECS chunk processing size |
| `enable_caching` | true | Enable query result caching |
| `cache_ttl` | 60000 | Cache time-to-live in milliseconds |
| `max_query_results` | 10000 | Maximum entities per query |
| `enable_profiling` | false | Enable ECS profiling |

### **Game Actions Settings**

| Setting | Default | Description |
|---------|---------|-------------|
| `enable_transactions` | true | Enable transaction support |
| `transaction_timeout` | 30000 | Transaction timeout in milliseconds |
| `max_concurrent_operations` | 100 | Maximum concurrent operations |
| `enable_audit_logging` | true | Log all game modifications |
| `validation_level` | strict | Input validation strictness (strict, normal, relaxed) |

---

## 🔄 Automation Rules Configuration

Flow automation definitions control the behavior of automated systems:

### **automation_rules.json Structure**

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
          "message": "⚔️ Welcome to the Arena!",
          "type": "info"
        },
        {
          "action": "zone.setpvp",
          "value": true
        }
      ],
      "on_complete": [
        {
          "action": "zone.log",
          "message": "Arena welcome flow completed",
          "level": "debug"
        }
      ]
    }
  },
  "settings": {
    "enable_hot_reload": true,
    "validate_on_load": true,
    "max_flows_per_file": 1000
  }
}
```

### **Flow Configuration Elements**

| Element | Required | Description |
|---------|-----------|-------------|
| `triggers` | Yes | Events that trigger the flow |
| `conditions` | No | Conditions that must be met |
| `actions` | Yes | Actions to execute |
| `on_complete` | No | Actions to run on completion |
| `on_error` | No | Actions to run on error |

---

## 🧾 Dynamic Commands Configuration

Command definitions for the dynamic command system:

### **commands.json Structure**

```json
{
  "commands": {
    "spawnboss": {
      "description": "Spawn a boss at your location",
      "enabled": true,
      "permission": "moderator",
      "actions": [
        {
          "action": "spawn.prefab",
          "prefab": "vampire_lord",
          "count": 1,
          "context": { "zone": "current" }
        }
      ]
    },
    "heal": {
      "description": "Heal yourself",
      "enabled": true,
      "permission": "guest",
      "actions": [
        {
          "action": "player.modify",
          "stat": "health",
          "value": 100
        }
      ]
    }
  },
  "settings": {
    "enable_hot_reload": true,
    "validate_permissions": true,
    "max_commands": 1000
  }
}
```

### **Command Configuration Elements**

| Element | Required | Description |
|---------|-----------|-------------|
| `description` | Yes | Human-readable description |
| `enabled` | No | Whether command is active (default: true) |
| `permission` | No | Required permission level |
| `actions` | Yes | Actions to execute when command runs |

---

## 🗺️ Zone Configuration

Zone definitions used by flows and actions:

### **zones.json Structure**

```json
{
  "zones": {
    "arena": {
      "name": "Arena Zone",
      "center": { "x": 0, "y": 0, "z": 0 },
      "radius": 100,
      "shape": "circle",
      "enabled": true,
      "settings": {
        "pvp_enabled": false,
        "no_build": true,
        "no_teleport": false,
        "entry_message": "⚔️ Arena Zone",
        "exit_message": "Left Arena Zone"
      }
    },
    "castle_area": {
      "name": "Castle Area",
      "center": { "x": 500, "y": 0, "z": 500 },
      "radius": 200,
      "shape": "circle",
      "enabled": true,
      "settings": {
        "pvp_enabled": false,
        "no_build": false,
        "safe_zone": true,
        "entry_message": "🏰 Safe Zone - Castle Area"
      }
    },
    "dungeon_entrance": {
      "name": "Dungeon Entrance",
      "bounds": [
        { "x": -50, "y": 0, "z": -50 },
        { "x": 50, "y": 20, "z": 50 }
      ],
      "shape": "box",
      "enabled": true,
      "settings": {
        "pvp_enabled": true,
        "no_teleport": true,
        "entry_message": "⚔️ Dangerous Area - Dungeon Entrance"
      }
    }
  },
  "settings": {
    "enable_hot_reload": true,
    "validate_zones": true,
    "max_zones": 1000
  }
}
```

### **Zone Configuration Elements**

| Element | Required | Description |
|---------|-----------|-------------|
| `name` | Yes | Human-readable zone name |
| `center` | Yes (circle) | Center point for circular zones |
| `bounds` | Yes (box) | Min/max bounds for box zones |
| `radius` | Yes (circle) | Radius for circular zones |
| `shape` | Yes | Zone shape (circle, box) |
| `enabled` | No | Whether zone is active (default: true) |
| `settings` | No | Zone-specific settings |

---

## 🔧 Configuration Management

### **Hot Reloading**

Most configuration files support hot reloading:

```csharp
// Enable hot reloading (enabled by default)
ConfigurationService.EnableHotReloading = true;

// Manual reload
await ConfigurationService.ReloadConfigurationAsync();

// Reload specific file
await ConfigurationService.ReloadFileAsync("automation_rules.json");
```

### **Configuration Validation**

All configurations are validated before applying:

```csharp
// Validate all configurations
var validationResult = ConfigurationService.ValidateAll();
if (!validationResult.IsValid)
{
    Logger.Error($"Configuration validation failed: {validationResult.ErrorMessage}");
}

// Validate specific file
var fileValidation = ConfigurationService.ValidateFile("zones.json");
```

### **Configuration Templates**

Create configuration templates for different environments:

```json
{
  "templates": {
    "development": {
      "framework": {
        "log_level": "Debug",
        "enable_performance_monitoring": true
      },
      "ecs": {
        "enable_profiling": true
      }
    },
    "production": {
      "framework": {
        "log_level": "Info",
        "enable_performance_monitoring": false
      },
      "ecs": {
        "enable_profiling": false
      }
    }
  }
}
```

---

## 🔍 Configuration API

### **Programmatic Configuration**

```csharp
// Get configuration values
var logLevel = ConfigurationService.GetSetting<string>("framework.log_level");
var maxFlows = ConfigurationService.GetSetting<int>("framework.max_concurrent_flows");

// Set configuration values
await ConfigurationService.SetSettingAsync("framework.log_level", "Debug");
await ConfigurationService.SetSettingAsync("ecs.max_query_results", 20000);

// Get entire configuration section
var ecsConfig = ConfigurationService.GetSection<ECSConfiguration>("ecs");

// Watch for configuration changes
ConfigurationService.WatchSetting("framework.log_level", (oldValue, newValue) => {
    Logger.Info($"Log level changed from {oldValue} to {newValue}");
});
```

### **Configuration Export/Import**

```csharp
// Export current configuration
var exportedConfig = ConfigurationService.ExportConfiguration();
await File.WriteAllTextAsync("backup_config.json", exportedConfig);

// Import configuration
var configData = await File.ReadAllTextAsync("new_config.json");
await ConfigurationService.ImportConfigurationAsync(configData);
```

---

## 📊 Configuration Best Practices

### **Environment-Specific Configurations**

```json
{
  "environments": {
    "development": {
      "framework": { "log_level": "Debug" },
      "ecs": { "enable_profiling": true }
    },
    "staging": {
      "framework": { "log_level": "Info" },
      "ecs": { "enable_profiling": false }
    },
    "production": {
      "framework": { "log_level": "Warning" },
      "ecs": { "enable_profiling": false }
    }
  }
}
```

### **Security Considerations**

* **Sensitive Data** - Never store passwords or API keys in configuration files
* **File Permissions** - Restrict write access to configuration files
* **Validation** - Always validate configuration values before use
* **Backups** - Keep backups of working configurations

### **Performance Optimization**

* **Minimal Logging** - Use Info or Warning level in production
* **Disable Profiling** - Turn off profiling in production environments
* **Cache Settings** - Enable caching for frequently accessed configuration
* **Batch Changes** - Group configuration changes to reduce reload frequency

---

## 🔧 Troubleshooting

### **Common Issues**

#### **Configuration Not Loading**
```
ERROR: Failed to load configuration: File not found
```
**Solution**: Ensure configuration files exist in the correct directory and have proper permissions.

#### **Invalid Configuration**
```
ERROR: Configuration validation failed: Invalid value for max_concurrent_flows
```
**Solution**: Check configuration syntax and data types. Use validation to identify specific errors.

#### **Hot Reload Not Working**
```
WARNING: Hot reload disabled for file: automation_rules.json
```
**Solution**: Ensure hot reloading is enabled and file watcher has permission to access the file.

### **Debugging Configuration**

```csharp
// Enable configuration debugging
ConfigurationService.SetLogLevel(LogLevel.Debug);

// Get configuration status
var status = ConfigurationService.GetStatus();
Logger.Info($"Configuration files loaded: {status.FilesLoaded}");
Logger.Info($"Configuration errors: {status.ErrorCount}");

// Get detailed file information
foreach (var fileInfo in status.FileStatus)
{
    Logger.Info($"File: {fileInfo.Name}, Loaded: {fileInfo.IsLoaded}, " +
              $"Last Modified: {fileInfo.LastModified}");
}
```

---

## 📖 Next Steps

Ready to dive deeper?

* **[Unified Config Details](unified-config.md)** - Complete settings reference
* **[Automation Rules](automation-rules.md)** - Flow configuration guide
* **[Commands Config](commands-config.md)** - Dynamic command setup
* **[Zones Config](zones-config.md)** - Zone definition guide

---

<div align="center">

**[🔝 Back to Top](#configuration-system)** • [**← Documentation Home**](../index.md)** • **[Unified Config →](unified-config.md)**

</div>
