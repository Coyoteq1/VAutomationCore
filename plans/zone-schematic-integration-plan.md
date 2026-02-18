# Zone Template Snapshot Standalone Plan

## Overview

This plan defines a standalone Bluelock-native zone snapshot system for save/load/attach workflows. It replaces one-by-one building with dependency-first snapshot replay and immediate zone attachment.

## Inputs

### Functional Inputs
- Zone identifier (`zoneId`)
- Snapshot/template name (`templateName`)
- Zone bounds and origin (`center`, `radius/extent`, optional rotation)
- Lifecycle trigger context (`onEnter`, `onExit`, `onReset`)
- Attach policy (`auto-attach on save`)

### Data Inputs
- Zone config from `Bluelock/config/VAuto.Zones.json`
- Lifecycle config from `Bluelock/config/VAuto.ZoneLifecycle.json`
- Existing template mappings from `zone.Templates`
- Runtime zone state from `ZoneConfigService` and zone event bridge
- Snapshot payload from `config/Bluelock/templates/<zoneId>/<templateName>.template.json`

### Entity/Class Inputs (must be captured)
- Template entities
- Border entities
- Roof entities
- Floor entities
- Glow entities
- Visual state inputs (glow/border/icon-related config fields)

### Technical Inputs
- `EntityManager` world readiness
- Prefab resolution tables (`PrefabResolver` / prefab catalogs)
- Component serializers/delta-appliers used by current template flow
- Registry state from `ZoneTemplateRegistry`
- Attachment metadata file `config/VAuto.TemplateAttachments.json`

### Constraints / Non-Goals
- Standalone implementation only (no external schematic dependency)
- Must support dependency-first load ordering natively
- Must preserve clear+respawn lifecycle behavior for attached templates

## Current Bluelock Architecture

### Existing Template Flow
- `Bluelock/Services/ZoneTemplateService.cs` handles spawn/clear/rebuild by template type.
- `Bluelock/Services/BuildingService.cs` handles neutral template spawning from repository snapshots.
- `Bluelock/Services/ZoneTemplateRegistry.cs` tracks spawned entities and metadata.

### Existing Limitations
- No full component-diff snapshot replay for zone structures/visuals.
- No single attached snapshot source of truth per zone.
- No class-aware status across template/border/roof/floor/glow.

## Target Standalone Architecture

### New Services
- `Bluelock/Services/ZoneTemplateSnapshotService.cs`
  - Save/load/list/attach/detach snapshot files.
- `Bluelock/Services/TemplateEntityClassifier.cs`
  - Classifies entity kinds: Template, Border, Roof, Floor, Glow.
- `Bluelock/Services/ZoneVisualStateMapper.cs`
  - Captures/restores visual config state.

### New/Updated Models
- `Bluelock/Models/ZoneTemplateSnapshot.cs`
- `Bluelock/Models/SnapshotEntity.cs`
- `Bluelock/Models/ZoneTemplateAttachmentMetadata.cs`

## Snapshot Format

```json
{
  "version": "1.0.0",
  "zoneId": "arena_01",
  "templateName": "arena-main",
  "origin": { "x": 0, "y": 0, "z": 0 },
  "bounds": { "min": { "x": 0, "y": 0, "z": 0 }, "max": { "x": 0, "y": 0, "z": 0 } },
  "classCounts": {
    "templateCount": 0,
    "borderCount": 0,
    "roofCount": 0,
    "floorCount": 0,
    "glowCount": 0
  },
  "visualState": {
    "glow": {},
    "border": {},
    "icons": {}
  },
  "entities": []
}
```

## Save Pipeline

1. Resolve zone bounds and origin.
2. Collect zone entities and classify by kind.
3. Capture prefab/transform/tile + component delta/removals.
4. Build dependency graph between captured entities.
5. Capture visual settings (glow/border/icon).
6. Write snapshot JSON to `config/Bluelock/templates/<zoneId>/<templateName>.template.json`.
7. Auto-attach snapshot to zone via attachment metadata file.

## Load Pipeline

1. Validate snapshot file and version.
2. Apply saved visual state to zone/runtime configuration.
3. Clear existing spawned entities for all 5 classes.
4. Instantiate entities dependency-first.
5. Apply component data/removals in second pass.
6. Register entities in `ZoneTemplateRegistry` with class labels.
7. Persist load metadata and attachment state.
8. On failure, cleanup partial spawned entities and return detailed error.

## Command Surface

### Template Commands
- `.tm save <zoneId> <name>`: save + auto-attach
- `.tm load <zoneId> <name>`: clear + load
- `.tm attach <zoneId> <name>`
- `.tm detach <zoneId>`
- `.tm list <zoneId>`
- `.tm active <zoneId>`
- `.tm status <zoneId>`: per-class counts + visual summary

## Lifecycle Behavior

### On Zone Enter/Activation
- If zone has an attached snapshot and is not spawned: load it.

### On Zone Reset/Empty
- Clear all class entities (Template, Border, Roof, Floor, Glow).
- Keep attachment pointer so next activation respawns.

## File/Config Layout

- `Bluelock/Services/ZoneTemplateSnapshotService.cs` (new)
- `Bluelock/Services/TemplateEntityClassifier.cs` (new)
- `Bluelock/Services/ZoneVisualStateMapper.cs` (new)
- `Bluelock/Models/ZoneTemplateSnapshot.cs` (new)
- `Bluelock/config/VAuto.TemplateAttachments.json` (new)
- `Bluelock/config/VAuto.ZoneLifecycle.json` (update lifecycle step usage)

## Risks and Mitigations

- Prefab missing on load: skip entity/group with warning, continue load.
- Dependency cycle: detect cycle, fallback load remaining with warning.
- Partial load failure: rollback entities from this load.
- Visual mismatch: apply visual state before spawn and report status summary.

## Testing

### Unit Tests
- Classifier correctness for all 5 classes.
- Dependency sorting correctness.
- Visual state round-trip mapping.

### Integration Tests
- Save -> clear -> load restores all 5 classes + visuals.
- Reset clears all 5 classes.
- Enter respawns attached snapshot.
- Status reports accurate counts and visual fields.

## Acceptance Criteria

- Standalone only: no external schematic dependency.
- Full class coverage: Template/Border/Roof/Floor/Glow.
- Visual parity restored after load.
- Auto-attach on save works.
- Clear+respawn lifecycle policy works.
