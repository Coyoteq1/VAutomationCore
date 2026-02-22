# Zone Glow/Template Sandbox Focus Plan

## Scope

Focus only on zone runtime behavior in Bluelock:

- Auto glow behavior per zone
- Template spawn safety per zone
- Deterministic zone identification
- Sandbox zones using the same enter/exit lifecycle steps

## Objectives

1. Identify zone consistently when multiple zones can match a position.
2. Prevent template spawn collisions when zones share the same location footprint.
3. Keep glow auto tied to zone activation/reset lifecycle steps.
4. Keep sandbox zones on the exact same lifecycle step pipeline as other zones.

## Implementation Focus

### 1. Zone identification

- Use deterministic zone match ordering:
  - default zone priority
  - smaller footprint wins
  - closer center wins
  - stable ID tie-break
- Expose operator command to inspect matching order at player position.

### 2. Template safety

- Block template spawn when target zone matches location of another zone with active template registry state.
- Track spawned template entities for both template and schematic paths.
- Avoid repeated auto-spawn duplication on repeated enters.

### 3. Glow auto behavior

- Keep glow auto spawn on first active enter.
- Keep glow reset when zone empties.
- Keep glow decisions zone-config-driven (`GlowTileEnabled`, `GlowTileAutoSpawnOnEnter`, `GlowTileAutoSpawnOnReset`).

### 4. Sandbox same steps

- Sandbox zones continue to run through the same lifecycle action pipeline:
  - `apply_templates`
  - `glow_spawn`
  - integration hooks and exit reset steps
- No sandbox-only divergence in lifecycle order.

## Operator Commands

- `.z identify` to show ordered zone matches at current position.
- Existing `.z status`, `.z diag`, and template commands remain in place.

## Handoff To Code Mode

1. Validate compile for `Bluelock` and `VAutomationCore`.
2. Smoke test in server:
   - enter/exit normal zone
   - enter/exit sandbox zone
   - verify glow auto spawn/reset
   - verify template collision block on same-location zones
3. Keep tuning values in `VAuto.Zones.json` only.
