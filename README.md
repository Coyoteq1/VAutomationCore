# VAutomationCore & Blueluck

V Rising server automation framework with zone management, flow system, and ECS utilities.

## Packages

| Package | Version | Description |
|---------|---------|-------------|
| VAutomationCore | 1.1.0 | Core library for V Rising mods |
| Blueluck | 1.1.1 | Zone management system (Boss/Arena zones) |

## Installation

### VAutomationCore (NuGet)
```xml
<PackageReference Include="VAutomationCore" Version="1.1.0" />
```

### Blueluck (BepInEx)
Copy `Blueluck.dll` to `BepInEx/plugins/`

## Features

### VAutomationCore
- Flow-based automation system with action registry
- ECS helpers for V Rising (EntityQuery, PrefabGUID, etc.)
- Configuration management with JSON schema validation
- Command framework integration (VampireCommandFramework)
- Game action service for server-side operations

### Blueluck
- **Zone Detection**: Automatic detection via ECS or fallback polling
- **Arena Zones**: PVP arenas with loadout kits and progress saving
- **Boss Zones**: Co-op boss encounters with shared progress
- **Flow System**: Configurable enter/exit actions
- **Commands**:
  - `.enterarena <zone>` / `.exitarena` - Force arena zone
  - `.enterboss <zone>` / `.exitboss` - Force boss zone
  - `.zone status` / `.zone list` - Zone info

## Configuration

### zones.json
```json
{
  "zones": [
    {
      "name": "PVP Arena",
      "type": "ArenaZone",
      "hash": 2001,
      "center": [-1000, 0, -500],
      "entryRadius": 60,
      "kitOnEnter": "Kit1",
      "flowOnEnter": "arena_enter"
    }
  ]
}
```

### flows.json
```json
{
  "flows": {
    "arena_enter": [
      { "action": "zone.setpvp", "value": true },
      { "action": "zone.message", "message": "⚔️ PVP ARENA!" }
    ]
  }
}
```

## Documentation

- [Framework Wiki](./docs/wiki/framework_wiki.html)
- [API Reference](./docs/api/)

## Support

- Discord: https://discord.gg/68JZU5zaq7
- GitHub: https://github.com/Coyoteq1/VAutomationCore

## Version History

### v1.1.1 (Blueluck)
- Fixed duplicate kit application bug
- Fixed entity validation for progress save
- Improved fallback zone detection

### v1.1.0
- Added fallback zone detection for dedicated servers
- Zone entity spawning improvements
- Manual zone commands (.enterarena, etc.)

### v1.0.x
- Initial releases
