# Repository Organization Standard

## Goals
- Keep plugin code isolated by project.
- Keep shared runtime code in one place.
- Keep non-code artifacts out of source roots.
- Prevent cross-plugin coupling drift.

## Canonical Topology
- `Core/` shared runtime contracts and helpers (`VAutomationCore` only).
- `Bluelock/` plugin-specific runtime code.
- `CycleBorn/` plugin-specific runtime code.
- `VAutoTraps/` plugin-specific runtime code.
- `VAutoannounce/` plugin-specific runtime code.
- `tests/` project-scoped test suites.
- `docs/` architecture and operational documentation.
- `scripts/` and `tools/` automation only.
- `out/` generated output (logs, reports, exports).
- `legacy/` archived source artifacts not in active compile graph.

## Folder Rules
1. `Commands/` orchestration only.
2. `Services/` domain behavior.
3. `Models/` DTO/config only.
4. `Patches/` Harmony hooks only; publish events and return.
5. Shared abstractions go to `Core/` first, plugin implementation stays local.
6. Plugins should reference `VAutomationCore` only (plugin-to-plugin references are exception-only and documented).

## Naming Rules
- One service per file.
- File name matches primary type.
- Use suffixes consistently: `*Service`, `*Config`, `*Patch`, `*Commands`.

## Build Graph Guardrails
- No source files compiled from root miscellany.
- Keep generated/temporary files in `out/`.
- Keep archives/zips in `assets/archive/`.

## Completed in this pass
- Moved `Core_ECSHelper.cs` -> `legacy/misc/Core_ECSHelper.cs`.
- Moved root `*.log` files -> `out/build-logs/`.
- Moved `New Compressed (zipped) Folder.zip` -> `assets/archive/New Compressed (zipped) Folder.zip`.

## Next Phase (safe incremental)
1. Add per-plugin `README.md` ownership docs.
2. Add architecture guardrail tests for forbidden project references.
3. Consolidate any remaining root-level source duplicates into plugin folders.
4. Remove legacy items after two stable releases.
