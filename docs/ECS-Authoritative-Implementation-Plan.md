# ECS Authoritative Implementation Plan

## Critical Ownership Rule
- `VAutomationCore` is the runtime engine owner.
- `Bluelock` is a feature/plugin owner.
- `CycleBorn` is lifecycle content/orchestration owner.

Do not keep long-term ECS runtime logic as plugin-only code.

## Final Target Ownership

### VAutomationCore owns
- Runtime mode model and runtime controller
- ECS components/systems/router/detection
- ECS transition contracts and adapters
- Validation contracts and deletion gates

### Bluelock owns
- Zone definitions and plugin bootstrap
- Prefab/template specifics and patch hooks
- Config loading and adapter injection into core

### CycleBorn owns
- Lifecycle content/flow templates
- Lifecycle config content and migration behavior

## IL2CPP Interop Constraint
- In this V Rising environment, ECS structs must not implement `: IComponentData`.
- Keep ECS components as plain structs and rely on server interop/runtime ECS binding.

## Determinism Requirements
1. Fixed detection tick (`Runtime.EcsDetectionTickSeconds`).
2. Stable zone ordering:
   - `Priority` descending
   - `EntryRadiusSq` descending
   - `ZoneHash` ascending
3. Single-owner event lifecycle:
   - only router consumes and destroys `ZoneTransitionEvent`.

## Runtime Modes
- Canonical mode selector: `Runtime.ZoneRuntimeMode`
  - `Legacy`
  - `Hybrid`
  - `EcsOnly`
- Boot mode is resolved once and treated as immutable until restart/reload.

## Required Core Contracts
- `IZoneConfigProvider`
- `IZoneTemplateAdapter`
- `IFlowExecutor`

These contracts isolate plugin dependencies and allow core-owned ECS reuse.

## Migration Phases
1. Introduce and adopt core contracts (adapter boundary).
2. Move ECS systems from plugin ownership to core ownership.
3. Keep Hybrid mode as safety path during soak.
4. Promote to `EcsOnly` after runtime stability and metrics validation.
5. Retire legacy branch only after one release cycle of successful operation.

## Deletion Gates
- Green build matrix (`VAutomationCore`, `CycleBorn`, `Bluelock`, `tests/Bluelock.Tests`).
- Single-owner router guardrail test passes.
- No duplicate transition dispatch in staging logs.
- Rollback path validated before destructive deletion.

## Observability Baseline
Track transition metrics with mode and workload context:
- player count
- zone count
- transitions per minute
- active runtime mode
