# CycleBorn Plugin

## Purpose
- Lifecycle progression systems, arena lifecycle handlers, unlock/respawn/score flows.

## Primary Entry Points
- `Plugin.cs`: plugin bootstrap and lifecycle orchestration.
- `Commands/*`: lifecycle command surface.
- `Services/Lifecycle/*`: lifecycle handlers, patches, and orchestration.

## Key Config
- `Configuration/pvp_item.json`
- `Configuration/pvp_item.toml`

## Dependencies
- `VAutomationCore` (shared contracts/services).

## Ownership Rule
- Keep lifecycle-specific features in `CycleBorn/*`.
- Promote only stable/shared contracts into `Core/*`.
