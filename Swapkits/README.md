# Swapkits

Swapkits provides inventory/slot and swap-related gameplay utilities for V Rising servers.

## Description
- Inventory and swap utility module for loadout and slot management workflows.

## Quick Commands
```powershell
# Build
dotnet build Swapkits/Swapkits.csproj -c Release --nologo

# Deploy (copies DLL to configured BepInEx plugins path)
dotnet build Swapkits/Swapkits.csproj -c Release --nologo --no-restore /p:DeployToServer=true
```

## Services
- `ExtraSlotsService`

## User GUIDs
- Use player `platformId`/GUID values for user-specific inventory/swap actions.
- Core alias helper: `.jobs alias user <alias> [platformId]`.

## Community
- Join the V Rising Mods Community on Discord: [V Rising Mods Discord](https://discord.gg/68JZU5zaq7)
- Need ownership support? Visit: [Ownership Support Discord](https://discord.gg/Se4wU3s6md)

## Contributors
Special thanks to our contributors:
1. coyoteq1
