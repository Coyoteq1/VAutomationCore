# Changelog

All notable changes to the VAutomation framework are documented here.

## [1.1.0] - 2026-03-06

### Added
- **VAutomationCore v1.1.0** - Core library updates
  - Enhanced automation service and sandboxing
  - HTTP server and event scheduling
  - Improved ECS utilities and lifecycle contracts
  - Configuration infrastructure with JSON schema validation
  - Feature flags and observability improvements
  - Enhanced logging with correlation IDs

- **Blueluck v1.1.0** - Zone management system
  - ECS-based zone detection system with fallback support
  - Complete flow system for zone enter/exit actions
  - Kit system for equipment loadouts
  - Snapshot system for progress save/restore
  - Ability loadouts (server-side buff application)
  - Boss co-op spawning service
  - Full chat command interface with comprehensive commands
  - Flow validation service for runtime safety

### Chat Commands (Blueluck)
- `zone status` / `zs` - Show zone status
- `zone list` / `zl` - List all configured zones
- `zone reload` / `zr` - Reload zone configuration
- `flow reload` - Reload flows.json from disk
- `zone debug` - Toggle zone detection debug mode
- `flow validate` - Validate flow configurations
- `kit list` - List available kits
- `kit [name]` - Apply a kit to yourself
- `snap status` - Show snapshot status
- `snap save [name]` - Save current progress snapshot
- `snap apply [name]` - Apply a snapshot
- `snap restore` - Restore last saved snapshot
- `snap clear` - Clear snapshot data

### Configuration (Blueluck)
- General.Enabled - Enable/disable plugin
- General.LogLevel - Logging level (Debug, Info, Warning, Error)
- Detection.CheckIntervalMs - Zone detection check interval
- Detection.PositionThreshold - Position change threshold
- Detection.DebugMode - Debug logging toggle
- Flow.Enabled - Flow system toggle
- Kits.Enabled - Kit system toggle
- Progress.Enabled - Progress save/restore toggle
- Abilities.Enabled - Ability loadouts toggle

### Flow Actions
- `zone.setpvp` - Enable/disable PvP
- `zone.sendmessage` - Send chat message
- `zone.spawnboss` - Spawn VBlood boss
- `zone.removeboss` - Remove boss entities
- `zone.applykit` - Apply kit to players
- `zone.removekit` - Remove kit from players

### Flow Validation Service
- Validates prefabs before execution to prevent runtime crashes
- Validates boss prefabs against known VBlood list
- Validates VFX prefabs
- Validates buff prefabs
- Validates kit references
- Exposes `flow validate` command for server operators
- Includes known prefab lists for early error detection

### Improved
- Unified configuration system across all components
- Enhanced error handling and logging
- Better performance with optimized ECS queries
- Improved zone detection accuracy and reliability
- Enhanced mod compatibility and integration points

## [1.0.x] - Previous Releases
- Initial framework foundation
- Basic zone management
- Core ECS utilities
- Configuration system foundation
