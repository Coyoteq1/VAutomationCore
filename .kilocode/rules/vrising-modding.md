## Brief overview
This project is a V Rising game server modding framework with multiple plugins (VAutomationCore, Bluelock, Chat, etc.). Rules apply to all plugins in this workspace.

## Project structure
- VAutomationCore: Core framework plugin providing ECS access, commands, and shared services
- Bluelock: Zone/arena management plugin with kit system, glow borders, sandbox progression
- Other plugins: Chat, CycleBorn, VAuto.Extensions, VAutoannounce, VAutoTraps, Swapkits
- Dependencies must be declared via [BepInDependency] attributes
- Core services go in Core/ directory, plugin-specific services in plugin folders

## V Rising technical guidelines
- Use HarmonyLib for patching game methods
- Follow ECS/DOTS patterns: use EntityManager, EntityQuery, and NativeArray properly
- Always dispose NativeArray with try-finally blocks (avoid 'using' statements)
- Use predefined EntityQueries when possible instead of creating new ones
- Check component existence with HasComponent<T> before retrieving
- Use Plugin.LogInstance.LogInfo or CoreLog for consistent logging
- Include system name, entity ID, and component data in logs

## Code style
- Use C# conventions: PascalCase for methods/properties, camelCase for local variables
- Namespace pattern: VAutomationCore.Core for core, VAuto.Zone for Bluelock
- Use region blocks (#region) for grouping related code in large classes
- Use static readonly for constants, const when compile-time constant
- Initialize collections in constructor or lazy initialization pattern

## Configuration
- Use BepInEx ConfigurationEntry for settings
- Group config entries by feature with region blocks
- Provide sensible defaults with clear descriptions
- Save config in Load() method, cleanup in Unload()

## Commands
- Use VampireCommandFramework for chat commands
- Register commands in plugin Load() via CommandRegistry.RegisterAll
- Follow naming convention: CommandGroup/CommandName syntax
- Include help text and argument validation

## Communication style
- Be technical and precise in responses
- Explain ECS/game-specific concepts when relevant
- Provide code examples for complex operations
- Ask clarifying questions when requirements are ambiguous
