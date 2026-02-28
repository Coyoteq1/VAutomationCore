# VAutomationCore Framework

`VAutomationCore` is a shared V Rising server framework.
It is live in production and actively used by multiple live modules built on this framework.

## NuGet
- Package: `VAutomationCore`
- Latest package page: `https://www.nuget.org/packages/VAutomationCore`
- Latest prerelease page: `https://www.nuget.org/packages/VAutomationCore/1.0.1-beta.3`

Install latest stable:
```xml
<PackageReference Include="VAutomationCore" Version="1.0.0" />
```

Install specific prerelease:
```xml
<PackageReference Include="VAutomationCore" Version="1.0.1-beta.3" />
```

## Thunderstore (V Rising)
- Browse/install packages: `https://thunderstore.io/c/v-rising/`
- Use Thunderstore Mod Manager or r2modman for server/plugin package installs.
- Framework docs/commands here remain the authoritative source for current runtime behavior.

## Focus
- Register in-game commands with VCF.
- Register and initialize services in one startup path.
- Run ECS jobs from services or chat commands.
- Build and run flows with action aliases and entity aliases (`EntityMap` + `EntityAliasMapper`), so even other mods that use VCF can automate through flows.
- Enforce core role auth for jobs: `Developer` can run job flows, `Admin` alone cannot.

## Description
- `VAutomationCore` is the shared runtime/library layer used by Bluelock, CycleBorn, and related modules.
- It provides common APIs, auth, flow execution, config services, and ECS helpers.

## Services
- `ServiceInitializer` for startup registration/validation orchestration.
- `ConsoleRoleAuthService` for developer/admin auth sessions.
- `FlowService` for action alias and flow execution.
- `ConfigService` for JSON/config file management.
- `GameActionService` for reusable game action helpers.

## User GUIDs
- Use player platform IDs (`platformId`) for user-scoped operations.
- Core mapping command: `.jobs alias user <alias> [platformId]`.
- Keep GUID/platform IDs as exact strings to avoid precision loss.

## Minimal startup pattern
```csharp
using System.Reflection;
using VampireCommandFramework;
using VAutomationCore.Core.Api;
using VAutomationCore.Core.Logging;
using VAutomationCore.Core.Services;

public override void Load()
{
    var log = new CoreLogger("Module");
    ServiceInitializer.InitializeLogger(log);
    ServiceInitializer.RegisterInitializer("your_service", YourService.Initialize);
    ServiceInitializer.RegisterValidator("your_service", () => YourService.IsReady);
    ServiceInitializer.InitializeAll(log);

    CommandRegistry.RegisterAll(Assembly.GetExecutingAssembly());
}
```

## In-game console commands (core)
- `.coreauth login dev <password>`
- `.coreauth login admin <password>`
- `.coreauth status`
- `.coreauth logout`
- `.jobs flow add <flow> <action>`
- `.jobs alias self <alias>` / `.jobs alias user <alias> [platformId]`
- `.jobs run <flow>`

`Developer` auth is required for `.jobs run`.

## Active command roots (authoritative)
- Bluelock: `.zone .match .spawn .template .tag .enter .exit .unlockprefab`
- VAutomationCore: `.coreauth .jobs`
- CycleBorn: `.lifecycle`
- Excluded by build: `.arena` (file exists but excluded by `Bluelock/VAutoZone.csproj`)

## Community
- Join the V Rising Mods Community on Discord: [V Rising Mods Discord](https://discord.gg/68JZU5zaq7)
- Need ownership support? Visit: [Ownership Support Discord](https://discord.gg/Se4wU3s6md)

## Auth
- coyoteq1

## Note
- Admin and console commands are disabled and do not work in this package.
- This package is a pure library.

## Build and deploy commands

Run all commands from repository root.

Build core library only:
```bash
dotnet build VAutomationCore.csproj -c Release /p:UseSharedCompilation=false
```

Build and deploy VAutomationCore to configured BepInEx plugins path:
```bash
dotnet build VAutomationCore.csproj -c Release /p:UseSharedCompilation=false /p:DeployToServer=true
```

Build BlueLock plugin (without building referenced projects):
```bash
dotnet build Bluelock/VAutoZone.csproj -c Release /p:BuildProjectReferences=false /p:UseSharedCompilation=false
```

Build and deploy BlueLock to configured BepInEx plugins path:
```bash
dotnet build Bluelock/VAutoZone.csproj -c Release /p:BuildProjectReferences=false /p:UseSharedCompilation=false /p:DeployToServer=true
```

Build CycleBorn plugin (without building referenced projects):
```bash
dotnet build CycleBorn/Vlifecycle.csproj -c Release /p:BuildProjectReferences=false /p:UseSharedCompilation=false
```

Build and deploy CycleBorn to configured BepInEx plugins path:
```bash
dotnet build CycleBorn/Vlifecycle.csproj -c Release /p:BuildProjectReferences=false /p:UseSharedCompilation=false /p:DeployToServer=true
```

When a target DLL is locked by a running server process, auto-deploy writes `*.dll.new` in the plugins folder so build still completes.

If the Roslyn/MSBuild servers cache stale state, restart them:
```bash
dotnet build-server shutdown
```

## Manifest locations

| Path | Package name | Version | Notes |
|---|---|---|---|
| `manifest.json` | `VAutomationCore` | `1.0.1` | Root/core Thunderstore manifest |
| `Bluelock/manifest.json` | `VAutomationZone` | `1.0.0` | BlueLock source manifest |
| `CycleBorn/manifest.json` | `CycleBorn` | `1.0.0` | CycleBorn source manifest |
| `packaging/VAutomationCore/manifest.json` | `VAutomationCore` | `1.0.1` | Packaging manifest used for release artifact |
| `packaging/VAutomationZone/manifest.json` | `VAutomationZone` | `1.0.1` | Packaging manifest used for release artifact |
| `packaging/lifecycle/manifest.json` | `lifecycle` | `1.0.1` | Packaging manifest used for lifecycle release artifact |

Source manifests and packaging manifests should be kept version-aligned when publishing.

## Migration Notes

### v1.0.1
- **Config file renamed**: Configuration file changed from `VAuto.Core.cfg` to `gg.coyote.VAutomationCore.cfg`
- Users upgrading from v1.0.0 will need to reconfigure their settings (or migrate manually)
- New flow-based zone configuration system introduced with support for `FlowId`, `EntryRadius`, `ExitRadius`, and `MustFlows`

## Core API Surface

### Runtime/Execution
- `CoreExecution`: safe sync/async execution wrappers with retry (`Run`, `RunAsync`, `RunWithRetry`).
- `OperationResult` / `OperationResult<T>`: standard success/failure return model.
- `RetryPolicy`: retry configuration for resilient operations.

### Service/State
- `ServiceRegistry`: singleton registration/resolution for module services.
- `EntityMap`: alias-to-entity reference map used by flow/job execution.
- `EntityAliasMapper`: component alias registration + component query/set helpers.

### Flow APIs
- `FlowService`: register, resolve, and execute action flows.
- `FlowDefinition` / `FlowStep`: flow model types used by `FlowService`.

### Auth/Console APIs
- `ConsoleRoleAuthService`: admin/developer auth session handling.
- `CoreAuthCommands` / `CoreJobFlowCommands`: built-in VCF command handlers.

## Quick API Examples

```csharp
using VAutomationCore.Core.Api;
using VAutomationCore.Core.Logging;

// 1) Register and execute a flow
FlowService.RegisterActionAlias("heal", "HealSelf");
FlowService.RegisterFlow("startup", new[]
{
    new FlowStep("heal")
});
var map = new EntityMap();
var result = FlowService.Execute("startup", map);

// 2) Safe execution with retry
var logger = new CoreLogger("Example");
var op = CoreExecution.RunWithRetry(
    () => { /* work */ },
    operationName: "startup-work",
    retryPolicy: RetryPolicy.Default,
    logger: logger
);
```

## Framework Wiki (HTML)

- `docs/wiki/framework-wiki.html`

## Docs Hosting (GitHub Pages)

- Workflow: `.github/workflows/docs-pages.yml`
- Repository: `https://github.com/Coyoteq1/D-VAutomationCore-VAutomationCore`
- Published root: `https://coyoteq1.github.io/D-VAutomationCore-VAutomationCore/`
- Framework wiki URL: `https://coyoteq1.github.io/D-VAutomationCore-VAutomationCore/wiki/framework-wiki.html`
- In GitHub repository settings, set Pages source to `GitHub Actions` once.

## Runtime Config Alignment (Current)

- Bluelock runtime mode is configured by `Runtime.ZoneRuntimeMode` (`Legacy`, `Hybrid`, `EcsOnly`).
- ECS detection scheduling is controlled by `Runtime.EcsDetectionTickSeconds`.
- High-load warning threshold is controlled by `Runtime.ZoneDetectionOpsWarningThreshold`.
- Bluelock lifecycle JSON uses `schemaVersion` and `configVersion` (`1.1.0`).

## Operations References

- `docs/ECS-Authoritative-Implementation-Plan.md`
- `docs/Architecture-Ownership-Map.md`
- `docs/Incident-Rollback-Playbook.md`
- `docs/README.md`
