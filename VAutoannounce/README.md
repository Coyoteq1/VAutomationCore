# VAutoannounce

VAutoannounce provides automated announcement workflows for V Rising servers.

## Description
- Announcement scheduling and broadcast module for server messaging workflows.

## Quick Commands
```powershell
# Build
dotnet build VAutoannounce/VAutoannounce.csproj -c Release --nologo

# Deploy (copies DLL to configured BepInEx plugins path)
dotnet build VAutoannounce/VAutoannounce.csproj -c Release --nologo --no-restore /p:DeployToServer=true
```

## Services
- `AnnouncementService`

## User GUIDs
- Announcement workflows can target player context using `platformId`/GUID values when needed by consuming commands.
- Core alias helper: `.jobs alias user <alias> [platformId]`.

## Community
- Join the V Rising Mods Community on Discord: [V Rising Mods Discord](https://discord.gg/68JZU5zaq7)
- Need ownership support? Visit: [Ownership Support Discord](https://discord.gg/Se4wU3s6md)

## Contributors
Special thanks to our contributors:
1. coyoteq1
