# VAuto.Extensions

VAuto.Extensions contains shared extension helpers used across VAutomation modules.

## Description
- Shared extension/helper project for reusable utility methods and cross-module helpers.

## Quick Commands
```powershell
# Build
dotnet build VAuto.Extensions/VAuto.Extensions.csproj -c Release --nologo
```

## Services
- No standalone runtime service registration; this project provides extension/helper primitives.

## User GUIDs
- GUID/platform ID handling is delegated to consuming modules and core commands.
- Use `.jobs alias user <alias> [platformId]` from `VAutomationCore` when user mapping is needed.

## Community
- Join the V Rising Mods Community on Discord: [V Rising Mods Discord](https://discord.gg/68JZU5zaq7)
- Need ownership support? Visit: [Ownership Support Discord](https://discord.gg/Se4wU3s6md)

## Contributors
Special thanks to our contributors:
1. coyoteq1
