# CycleBorn

CycleBorn is the lifecycle/automation module built on `VAutomationCore` for V Rising servers.

## Description
- Lifecycle orchestration plugin for snapshots, restore logic, and headless operator workflows.

## Quick Commands
```powershell
# Build
dotnet build CycleBorn/Vlifecycle.csproj -c Release --nologo

# Deploy (copies DLL to configured BepInEx plugins path)
dotnet build CycleBorn/Vlifecycle.csproj -c Release --nologo --no-restore /p:DeployToServer=true
```

## Services
- `AnnouncementService`
- `RespawnPreventionService`
- `ScoreService`
- `EnhancedArenaSnapshotLifecycleService`

## User GUIDs
- Use player `platformId`/GUID values for user-scoped lifecycle actions.
- Alias mapping helper from core: `.jobs alias user <alias> [platformId]`.

## Community
- Join the V Rising Mods Community on Discord: [V Rising Mods Discord](https://discord.gg/68JZU5zaq7)
- Need ownership support? Visit: [Ownership Support Discord](https://discord.gg/Se4wU3s6md)

## Contributors
Special thanks to our contributors:
1. coyoteq1
