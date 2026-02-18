# VAutoannounce Plugin

## Purpose
- Announcement workflows, menu commands, and event-driven messaging.

## Primary Entry Points
- `Plugin.cs`: plugin bootstrap.
- `Commands/Core/*`: announcement command surface.
- `Services/AnnouncementService.cs`: announcement runtime behavior.

## Key Config
- `manifest.json` and plugin-level config values.

## Dependencies
- `VAutomationCore` (shared contracts/services).

## Ownership Rule
- Keep announcement-specific logic in `VAutoannounce/*`.
- Share only stable contracts via `Core/*`.
