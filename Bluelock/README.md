# Bluelock

Bluelock is the zone/gameplay module built on top of `VAutomationCore` for V Rising servers.

## Description
- Zone runtime plugin for detection, transitions, templates, arena flows, and operator commands.

## Quick Commands
```powershell
# Build
dotnet build Bluelock/VAutoZone.csproj -c Release --nologo

# Deploy (copies DLL to configured BepInEx plugins path)
dotnet build Bluelock/VAutoZone.csproj -c Release --nologo --no-restore /p:DeployToServer=true
```

## Services
- `ZoneConfigService`
- `ZoneTemplateService`
- `ZoneBossSpawnerService`
- `StaggeredManifestationService`
- `ZonePlayerTagService`
- `ProcessConfigService`

## User GUIDs
- Use player `platformId`/GUID values for user-scoped operations.
- Alias mapping helper from core: `.jobs alias user <alias> [platformId]`.

## Community
- Join the V Rising Mods Community on Discord: [V Rising Mods Discord](https://discord.gg/68JZU5zaq7)
- Need ownership support? Visit: [Ownership Support Discord](https://discord.gg/Se4wU3s6md)

## Contributors
Special thanks to our contributors:
1. coyoteq1
