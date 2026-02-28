# VAutoTraps

VAutoTraps is the trap automation module for V Rising servers, built with `VAutomationCore`.

## Description
- Trap and zone automation plugin for trap spawning, container traps, and trap-zone execution.

## Quick Commands
```powershell
# Build
dotnet build VAutoTraps/VAutoTraps.csproj -c Release --nologo

# Deploy (copies DLL to configured BepInEx plugins path)
dotnet build VAutoTraps/VAutoTraps.csproj -c Release --nologo --no-restore /p:DeployToServer=true
```

## Services
- `TrapZoneService`
- `ContainerTrapService`
- `ChestSpawnService`

## User GUIDs
- User-scoped trap operations should use player `platformId`/GUID values.
- Core alias helper: `.jobs alias user <alias> [platformId]`.

## Community
- Join the V Rising Mods Community on Discord: [V Rising Mods Discord](https://discord.gg/68JZU5zaq7)
- Need ownership support? Visit: [Ownership Support Discord](https://discord.gg/Se4wU3s6md)

## Contributors
Special thanks to our contributors:
1. coyoteq1
