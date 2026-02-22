# Mount, Chunk, Template Notes

This file tracks API coverage for commonly requested gameplay domains.

## Mount API

Current state:
- No dedicated `MountApi` class exists in `Core/Api`.
- Mount workflows should currently be implemented through action aliases/flows or a module-specific API wrapper.

Recommended next step:
- Add `Core/Api/MountApi.cs` with stable signatures before exposing mount operations in commands.

## Chunk API

Current state:
- No dedicated `ChunkApi` class exists in `Core/Api`.
- Chunk-based operations should be isolated in a dedicated service/API pair to avoid direct command-level ECS writes.

Recommended next step:
- Add `Core/Api/ChunkApi.cs` plus service-backed implementation for chunk query/update operations.

## Template coverage

Current state:
- Job templates exist (`JobTemplates`).
- Configuration/content templates are not yet formalized as a separate API surface.

Recommended next step:
- Add a template registry API if you need reusable runtime content templates beyond ECS job templates.
