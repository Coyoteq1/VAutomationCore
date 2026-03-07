using System;
using System.Collections.Generic;
using VAutomationCore.Core.Gameplay;
using VAutomationCore.Core.Gameplay.Shared.Types;

namespace VAutomationCore.Core.Gameplay.Arena.Flows
{
    /// <summary>
    /// Arena admin control flows.
    /// These flows are owned by the Arena module.
    /// </summary>
    public static class ArenaAdminFlows
    {
        /// <summary>
        /// Get all admin flow definitions for Arena.
        /// </summary>
        public static IReadOnlyList<FlowDefinition> GetFlowDefinitions()
        {
            return new List<FlowDefinition>
            {
                // ========================================
                // Admin Match Control
                // ========================================

                CreateFlowDefinition(
                    name: "arena_admin_force_start",
                    description: "Admin: Force start match",
                    adminOnly: true,
                    supportedZoneTypes: new[] { "arena" },
                    tags: new[] { "arena", "admin", "match" },
                    arguments: new[]
                    {
                        CreateArg("arena_id", FlowArgKind.String, "Arena ID", required: true)
                    }
                ),

                CreateFlowDefinition(
                    name: "arena_admin_force_end",
                    description: "Admin: Force end match",
                    adminOnly: true,
                    supportedZoneTypes: new[] { "arena" },
                    tags: new[] { "arena", "admin", "match" },
                    arguments: new[]
                    {
                        CreateArg("arena_id", FlowArgKind.String, "Arena ID", required: true)
                    }
                ),

                // ========================================
                // Admin Teleport
                // ========================================

                CreateFlowDefinition(
                    name: "arena_admin_teleport",
                    description: "Admin: Teleport to arena",
                    adminOnly: true,
                    supportedZoneTypes: new[] { "arena" },
                    tags: new[] { "arena", "admin", "teleport" },
                    arguments: new[]
                    {
                        CreateArg("admin", FlowArgKind.Player, "Admin player", required: true),
                        CreateArg("arena_id", FlowArgKind.String, "Arena ID", required: true),
                        CreateArg("position", FlowArgKind.Position, "Spawn position", required: false)
                    }
                ),

                // ========================================
                // Admin Player Control
                // ========================================

                CreateFlowDefinition(
                    name: "arena_admin_heal",
                    description: "Admin: Heal all players",
                    adminOnly: true,
                    supportedZoneTypes: new[] { "arena" },
                    tags: new[] { "arena", "admin", "player" },
                    arguments: new[]
                    {
                        CreateArg("arena_id", FlowArgKind.String, "Arena ID", required: true)
                    }
                ),

                CreateFlowDefinition(
                    name: "arena_admin_reset_cooldowns",
                    description: "Admin: Reset all cooldowns",
                    adminOnly: true,
                    supportedZoneTypes: new[] { "arena" },
                    tags: new[] { "arena", "admin", "player" },
                    arguments: new[]
                    {
                        CreateArg("arena_id", FlowArgKind.String, "Arena ID", required: true),
                        CreateArg("player", FlowArgKind.Player, "Specific player", required: false, entityRoleName: "ArenaPlayer")
                    }
                ),

                CreateFlowDefinition(
                    name: "arena_admin_give_loadout",
                    description: "Admin: Give loadout to players",
                    adminOnly: true,
                    supportedZoneTypes: new[] { "arena" },
                    tags: new[] { "arena", "admin", "loadout" },
                    arguments: new[]
                    {
                        CreateArg("arena_id", FlowArgKind.String, "Arena ID", required: true),
                        CreateArg("player", FlowArgKind.Player, "Specific player", required: false, entityRoleName: "ArenaPlayer")
                    }
                ),

                // ========================================
                // Admin God Mode
                // ========================================

                CreateFlowDefinition(
                    name: "arena_admin_god_mode_enable",
                    description: "Admin: Enable god mode",
                    adminOnly: true,
                    supportedZoneTypes: new[] { "arena" },
                    tags: new[] { "arena", "admin", "god_mode" },
                    arguments: new[]
                    {
                        CreateArg("arena_id", FlowArgKind.String, "Arena ID", required: true),
                        CreateArg("player", FlowArgKind.Player, "Player to affect", required: true, entityRoleName: "ArenaPlayer")
                    }
                ),

                CreateFlowDefinition(
                    name: "arena_admin_god_mode_disable",
                    description: "Admin: Disable god mode",
                    adminOnly: true,
                    supportedZoneTypes: new[] { "arena" },
                    tags: new[] { "arena", "admin", "god_mode" },
                    arguments: new[]
                    {
                        CreateArg("arena_id", FlowArgKind.String, "Arena ID", required: true),
                        CreateArg("player", FlowArgKind.Player, "Player to affect", required: true, entityRoleName: "ArenaPlayer")
                    }
                ),

                // ========================================
                // Admin Time Control
                // ========================================

                CreateFlowDefinition(
                    name: "arena_admin_set_time",
                    description: "Admin: Set match time",
                    adminOnly: true,
                    supportedZoneTypes: new[] { "arena" },
                    tags: new[] { "arena", "admin", "time" },
                    arguments: new[]
                    {
                        CreateArg("arena_id", FlowArgKind.String, "Arena ID", required: true),
                        CreateArg("time_seconds", FlowArgKind.Int, "Time in seconds", required: true)
                    }
                )
            };
        }

        private static FlowDefinition CreateFlowDefinition(
            string name,
            string description,
            bool adminOnly,
            IReadOnlyList<string> supportedZoneTypes,
            IReadOnlyList<string> tags,
            IReadOnlyList<FlowArgDefinition>? arguments = null)
        {
            return new FlowDefinition
            {
                Name = name,
                GameplayType = GameplayType.Arena,
                Description = description,
                AdminOnly = adminOnly,
                SupportedZoneTypes = supportedZoneTypes,
                Tags = tags,
                Arguments = arguments ?? Array.Empty<FlowArgDefinition>(),
                EnabledByDefault = true
            };
        }

        private static FlowArgDefinition CreateArg(
            string name,
            FlowArgKind kind,
            string description,
            bool required = true,
            string? entityRoleName = null)
        {
            return new FlowArgDefinition
            {
                Name = name,
                Kind = kind,
                Required = required,
                Description = description,
                EntityRoleName = entityRoleName ?? string.Empty
            };
        }
    }
}
