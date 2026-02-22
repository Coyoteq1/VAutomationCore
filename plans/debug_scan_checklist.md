# PvP + Cross-Project Scan Checklist

## Artifact Generation
- [x] plans/debug_scan_findings_summary.md
- [x] plans/debug_scan_per_project_task_lists.md
- [x] plans/debug_scan_status_board.md
- [x] plans/debug_scan_dependency_map.md
- [x] plans/debug_scan_checklist.md

## Input Dataset Validation
- [x] Orders are sequential (1..N)
- [x] No duplicate (Order, ItemName, System, Project)
- [x] Scan rows include ProjectPath/FileTargets/Status
- [x] Required columns present

## Bootstrap Files
- [x] Plugin.cs
- [x] Bluelock/Plugin.cs
- [x] CycleBorn/Plugin.cs
- [x] Swapkits/Plugin.cs
- [x] VAutoTraps/Plugin.cs
- [x] VAutoannounce/Plugin.cs
- [x] Core/Api/CoreConsoleCommands.cs
- [x] Core/Api/ConsoleRoleAuthService.cs

## Mandatory Globs
- [x] **/*Commands*.cs
- [x] **/*Service*.cs
- [x] **/*System*.cs
- [x] **/Plugin.cs
- [x] **/*Models*.cs
- [x] **/*Data*.cs
- [x] **/*.csproj

## Project Metrics (Recomputed)
| Project | Files | Types | Methods | Entrypoints | Static Mentions | Auth Mentions | try/catch Mentions |
|---|---:|---:|---:|---:|---:|---:|---:|
| Core | 60 | 136 | 525 | 20 | 641 | 104 | 207 |
| Bluelock | 78 | 150 | 567 | 68 | 876 | 40 | 418 |
| CycleBorn | 49 | 97 | 214 | 50 | 308 | 12 | 138 |
| Swapkits | 9 | 9 | 62 | 7 | 45 | 16 | 14 |
| VAutoTraps | 20 | 39 | 132 | 20 | 206 | 23 | 23 |
| VAutoannounce | 14 | 20 | 38 | 7 | 106 | 8 | 14 |
| VAuto.Extensions | 11 | 5 | 43 | 0 | 69 | 0 | 11 |
| tests/Bluelock.Tests | 8 | 6 | 21 | 1 | 14 | 16 | 4 |

## Per-File Review Fields
- [ ] Type inventory complete
- [ ] Method inventory complete
- [ ] Entrypoints identified
- [ ] Side effects tagged
- [ ] Auth checks documented
- [ ] Failure behavior documented
- [ ] Cross-project dependencies documented

