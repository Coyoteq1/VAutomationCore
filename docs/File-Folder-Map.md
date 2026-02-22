# File and Folder Map

Quick map of important code and docs paths.

## Core framework

- `Core/Api`:
  - API surfaces (`ServerApi`, `PlayerApi`, `CommandApi`, `CastleApi`, flow/auth APIs)
- `Core/Services`:
  - runtime services (actions, snapshot bridge, persistence helpers, integration)
- `Core/ECS`:
  - ECS job utilities (`JobSystemExtensions`, templates, builder/flow helpers)
- `Core/Config`:
  - config and watcher utilities
- `Core/Commands`:
  - command-level integration if present in module context

## Lifecycle module

- `CycleBorn/Plugin.cs`:
  - unified lifecycle config, runtime accessors, migration path
- `CycleBorn/Commands/LifecycleCommands.cs`:
  - operator lifecycle commands
- `CycleBorn/Commands/LifecycleHeadlessCommands.cs`:
  - headless-safe lifecycle commands
- `CycleBorn/Services/Lifecycle`:
  - lifecycle service handlers and stage orchestration

## Snapshot system

- `Core/Services/DebugEventBridge.cs`:
  - baseline capture, delta computation, persistence, restore triggers
- `Core/Services/SandboxSnapshotStore.cs`:
  - in-memory pending/active snapshot state
- `Core/Services/Sandbox/*.cs`:
  - snapshot abstraction interfaces and service implementations

## Documentation index

- `docs/Core-System-Usage.md`:
  - main usage doc (entry point)
- `docs/Lifecycle-Snapshot-Usage.md`:
  - lifecycle snapshot flow and troubleshooting
- `docs/Command-Cheat-Sheet.md`:
  - operator/developer commands
- `docs/Lifecycle-Config-Reference.md`:
  - lifecycle config schema and keys
- `docs/api/*.md`:
  - split API references
