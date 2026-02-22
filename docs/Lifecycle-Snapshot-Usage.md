# Lifecycle Snapshot Usage

This page documents lifecycle snapshot behavior and commands, so operators do not need to manually request ad-hoc scan/docs each time.

## What lifecycle snapshot does

Lifecycle snapshot captures player progression around lifecycle transitions:
- baseline snapshot before transition
- post-transition snapshot
- computed delta rows (component/tech/entity changes)
- persisted baseline+delta files when persistence is enabled

Primary implementation path:
- `Core/Services/DebugEventBridge.cs`
- `Core/Services/SandboxSnapshotStore.cs`

## Runtime flow

1. Enter lifecycle zone:
- baseline context is created and cached per player key

2. Transition processing:
- unlock/actions run (depending on lifecycle mode/config)
- post snapshot is captured
- delta is computed and stamped

3. Exit lifecycle zone:
- progression restore runs from baseline
- cleanup/validation executes
- active snapshot state is removed for that player

## Lifecycle commands

Available command group: `.lifecycle` (`.lc`)

- `.lifecycle help`
- `.lifecycle status`
- `.lifecycle enter [zone]`
- `.lifecycle exit`
- `.lifecycle config`
- `.lifecycle stages`
- `.lifecycle trigger [stage]`

Source:
- `CycleBorn/Commands/LifecycleCommands.cs`
- `CycleBorn/Commands/LifecycleHeadlessCommands.cs`

## Snapshot persistence outputs

When persistence is enabled, outputs include:
- `sandbox_progression_baseline.csv.gz`
- `sandbox_progression_delta.csv.gz`
- `sandbox_progression_journal.jsonl`

These are managed through `DebugEventBridge` snapshot load/persist routines.

## Troubleshooting

- `status` shows not initialized:
  - verify lifecycle module is enabled and initialized in plugin startup.
- snapshots not updating:
  - verify snapshot path/persistence configuration.
  - verify player identity resolution succeeds (platform id and user entity).
- restore not applied on exit:
  - verify active baseline exists for the player key and zone flow is firing enter/exit hooks.

## Operator checklist

1. Confirm `.lifecycle status` reports enabled+initialized.
2. Use `.lifecycle enter` for controlled validation.
3. Trigger gameplay actions.
4. Use `.lifecycle exit` and verify restore behavior.
5. Check baseline/delta outputs and journal append.
