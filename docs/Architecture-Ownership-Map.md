# Zone Transition Ownership Map

- Runtime engine owner: `VAutomationCore`
- Owns: shared contracts, ECS utilities/components used across modules, lifecycle/runtime option contracts, API primitives.
- Feature/plugin owner: `Bluelock`
- Owns: zone runtime behavior, zone config execution, template manifestation path, plugin bootstrap/patch wiring.
- Lifecycle/content owner: `CycleBorn`
- Owns: lifecycle content and headless operator command surface (`.lifecycle`).

## Minimal mode guardrails
- `Bluelock/Plugin.cs` runtime lifecycle JSON models (`ZoneJsonConfig`, `ZoneMapping`, `ZoneMustFlow`) are authoritative in minimal mode.
- Legacy duplicate lifecycle model files in `Bluelock/Models` are removed when unused.
- Command family ownership:
- `Bluelock`: `.zone .match .spawn .template .tag .enter .exit .unlockprefab`
- `VAutomationCore`: `.coreauth .jobs`
- `CycleBorn`: `.lifecycle`
- `Bluelock/Commands/Core/ArenaEcsCommands.cs` remains excluded by csproj and is not an active command surface.

## Runtime modes
- Runtime selector: `Runtime.ZoneRuntimeMode` (`Legacy`, `Hybrid`, `EcsOnly`)
- Derived options contract: `ZoneRuntimeModeOptions.FromMode(...)`
- Boot mode lock: resolved once at startup and stored in `_bootRuntimeMode`
- ECS detection tick: `Runtime.EcsDetectionTickSeconds`
- ECS ops warning threshold: `Runtime.ZoneDetectionOpsWarningThreshold`

## Zone transition event flow
1. `ZoneDetectionSystem` emits `ZoneTransitionEvent`.
2. `ZoneTransitionRouterSystem` is the single event owner.
3. Router dispatches to:
   - `FlowExecutionSystem.ApplyTransition(...)`
   - `ZoneTemplateLifecycleSystem.ApplyTransition(...)`
4. Router destroys transition event entity.

## Config governance ownership
- Zones: `Bluelock/config/VAuto.Zones.json`
- Zone lifecycle mapping: `Bluelock/config/VAuto.ZoneLifecycle.json`
- Flow definitions: `Bluelock/config/flows/*.json`
- Canonical lifecycle runtime config: `VAuto.Lifecycle.json` (CycleBorn)

## Validation entrypoint
- `Bluelock/Services/ProcessConfigService.ValidateAllConfigs(...)`
