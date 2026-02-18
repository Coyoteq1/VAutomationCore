# VAutoTraps Plugin

## Purpose
- Trap/chest spawn rules and trap-zone gameplay behavior.

## Primary Entry Points
- `Plugin.cs`: plugin bootstrap.
- `Commands/Core/TrapCommands.cs`: command surface.
- `Services/Traps/*`: trap runtime behavior.
- `Services/Rules/*`: spawn constraints and policy.

## Key Config
- `Configuration/killstreak_trap_config.toml`

## Dependencies
- `VAutomationCore` (shared contracts/services).

## Ownership Rule
- Keep trap systems local to `VAutoTraps/*`.
- Avoid direct plugin-to-plugin references.
