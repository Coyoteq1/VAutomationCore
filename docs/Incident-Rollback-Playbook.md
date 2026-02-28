# Incident Rollback Playbook

## When to roll back
- ECS transition errors or repeated zone-processing failures.
- Config hot-reload repeatedly fails validation.
- Runtime action failures with player-impacting regressions.

## Immediate rollback
1. Set `Runtime.ZoneRuntimeMode=Legacy`.
2. Keep `Runtime.EcsDetectionTickSeconds` at a safe non-zero value (default `0.2`).
3. Reload plugin/config or restart server.
4. Confirm logs report boot mode as `Legacy`.

Note:
- Runtime mode is boot-locked. A full plugin reload/restart is required for guaranteed mode change.

## Config rollback
1. Restore last `*.bak.*` generated during migration.
2. Re-run with JSON canonical file only.
3. Confirm `VAuto.ZoneLifecycle.json` and `VAuto.Zones.json` pass validation.

## Verification commands
- Build:
  - `dotnet build VAutomationCore.csproj -c Debug --no-restore`
  - `dotnet build CycleBorn/Vlifecycle.csproj -c Debug --no-restore`
  - `dotnet build Bluelock/VAutoZone.csproj -c Debug --no-restore`
- Optional tests:
  - `dotnet test tests/Bluelock.Tests/Bluelock.Tests.csproj -c Debug --no-build`

## Required evidence before re-enable ECS
- No router dispatch errors in logs for at least one full test session.
- No config semantic validation errors.
- Manual zone enter/exit checks successful.

## Legacy retirement policy
- Deprecate first: ship one release with explicit warning logs before deletion.
- Soak window: keep deprecated path for one release window while collecting runtime error metrics.
- Removal gate: require green build matrix + guardrail tests + no repeated runtime dispatch/config errors.
- Rollback rule: if post-removal instability appears, force `Runtime.ZoneRuntimeMode=Legacy`, restore latest config backup, and re-run smoke checks before reopening ECS modes.
