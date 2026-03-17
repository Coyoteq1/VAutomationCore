# VAutomationCore

Shared automation and ECS utilities for V Rising server mods.

This repository currently contains the `VAutomationCore` library, test suite, packaging metadata, and integration contracts used by companion mods. The `Blueluck` plugin is treated as an external companion mod; its source is not included in this workspace.

## Scope

- Core flow execution, command/auth services, and event infrastructure
- ECS helpers, zone state mapping, and zone lifecycle bridge logic
- Packaging and deployment metadata for `VAutomationCore`
- Compatibility references for the external `Blueluck` companion mod

## Build

```powershell
dotnet build VAutomationCore.sln -c Debug
dotnet test tests\Bluelock.Tests\Bluelock.Tests.csproj -c Debug
```

## Key Paths

- `VAutomationCore.csproj`: package metadata and compile surface
- `Core/`: shared core APIs, ECS helpers, lifecycle contracts, and services
- `Services/ZoneEventBridge.cs`: zone state transition bridge used by runtime/tests
- `tests/Bluelock.Tests/`: regression and contract tests for the simplified workspace
- `config/VAuto.unified_config.schema.json`: unified config schema

## Companion Mod Note

Scripts and docs may still reference `Blueluck` because `VAutomationCore` keeps compatibility hooks for that external mod. The solution and deploy flow in this workspace no longer assume local `Blueluck` source.

## License

MIT
