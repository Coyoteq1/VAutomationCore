using System;
using System.Collections.Generic;
using VAutomationCore.Core.Gameplay;
using VAutomationCore.Core.Gameplay.Shared.Types;

namespace VAutomationCore.Core.Gameplay.Arena.Flows
{
    /// <summary>
    /// Arena monitoring and event tracking flows.
    /// These flows are owned by the Arena module.
    /// </summary>
    public static class ArenaMonitoringFlows
    {
        /// <summary>
        /// Get all monitoring flow definitions for Arena.
        /// </summary>
        public static IReadOnlyList<FlowDefinition> GetFlowDefinitions()
        {
            return new List<FlowDefinition>
            {
                // ========================================
                // PvE / Wave Mode Flows
                // ========================================

                CreateFlowDefinition(
                    name: "arena_wave_start",
                    description: "Start a wave in arena",
                    adminOnly: true,
                    supportedZoneTypes: new[] { "arena" },
                    tags: new[] { "arena", "wave", "pve", "admin" },
                    arguments: new[]
                    {
                        CreateArg("arena_id", FlowArgKind.String, "Arena ID", required: true),
                        CreateArg("wave_number", FlowArgKind.Int, "Wave number", required: false)
                    }
                ),

                CreateFlowDefinition(
                    name: "arena_wave_complete",
                    description: "Complete current wave",
                    adminOnly: false,
                    supportedZoneTypes: new[] { "arena" },
                    tags: new[] { "arena", "wave", "pve" },
                    arguments: new[]
                    {
                        CreateArg("arena_id", FlowArgKind.String, "Arena ID", required: true),
                        CreateArg("wave_number", FlowArgKind.Int, "Wave number", required: false)
                    }
                ),

                CreateFlowDefinition(
                    name: "arena_wave_skip",
                    description: "Skip current wave",
                    adminOnly: true,
                    supportedZoneTypes: new[] { "arena" },
                    tags: new[] { "arena", "wave", "pve", "admin" },
                    arguments: new[]
                    {
                        CreateArg("arena_id", FlowArgKind.String, "Arena ID", required: true),
                        CreateArg("current_wave", FlowArgKind.Int, "Current wave", required: false),
                        CreateArg("next_wave", FlowArgKind.Int, "Next wave", required: false)
                    }
                ),

                // ========================================
                // Boss Mode Flows
                // ========================================

                CreateFlowDefinition(
                    name: "arena_boss_spawn",
                    description: "Spawn a boss in arena",
                    adminOnly: true,
                    supportedZoneTypes: new[] { "arena" },
                    tags: new[] { "arena", "boss", "pve", "admin", "spawn" },
                    arguments: new[]
                    {
                        CreateArg("arena_id", FlowArgKind.String, "Arena ID", required: true),
                        CreateArg("boss_type", FlowArgKind.String, "Boss type", required: true),
                        CreateArg("spawn_position", FlowArgKind.Position, "Spawn position", required: false)
                    }
                ),

                CreateFlowDefinition(
                    name: "arena_boss_defeat",
                    description: "Boss defeated in arena",
                    adminOnly: false,
                    supportedZoneTypes: new[] { "arena" },
                    tags: new[] { "arena", "boss", "pve", "event" },
                    arguments: new[]
                    {
                        CreateArg("arena_id", FlowArgKind.String, "Arena ID", required: true),
                        CreateArg("boss_entity", FlowArgKind.Entity, "Boss entity", required: true, entityRoleName: "ArenaBoss")
                    }
                ),

                CreateFlowDefinition(
                    name: "arena_boss_escape",
                    description: "Boss escaped from arena",
                    adminOnly: false,
                    supportedZoneTypes: new[] { "arena" },
                    tags: new[] { "arena", "boss", "pve", "event" },
                    arguments: new[]
                    {
                        CreateArg("arena_id", FlowArgKind.String, "Arena ID", required: true),
                        CreateArg("boss_entity", FlowArgKind.Entity, "Boss entity", required: true, entityRoleName: "ArenaBoss")
                    }
                ),

                // ========================================
                // Enemy Spawning Flows
                // ========================================

                CreateFlowDefinition(
                    name: "arena_enemy_spawn",
                    description: "Spawn enemy in arena",
                    adminOnly: true,
                    supportedZoneTypes: new[] { "arena" },
                    tags: new[] { "arena", "enemy", "pve", "admin", "spawn" },
                    arguments: new[]
                    {
                        CreateArg("arena_id", FlowArgKind.String, "Arena ID", required: true),
                        CreateArg("enemy_type", FlowArgKind.String, "Enemy type", required: true),
                        CreateArg("spawn_position", FlowArgKind.Position, "Spawn position", required: false)
                    }
                ),

                CreateFlowDefinition(
                    name: "arena_enemy_clear",
                    description: "Clear all enemies from arena",
                    adminOnly: true,
                    supportedZoneTypes: new[] { "arena" },
                    tags: new[] { "arena", "enemy", "pve", "admin", "clear" },
                    arguments: new[]
                    {
                        CreateArg("arena_id", FlowArgKind.String, "Arena ID", required: true)
                    }
                ),

                // ========================================
                // Statistics Flows
                // ========================================

                CreateFlowDefinition(
                    name: "arena_stats_get",
                    description: "Get arena statistics",
                    adminOnly: false,
                    supportedZoneTypes: new[] { "arena" },
                    tags: new[] { "arena", "stats", "query" },
                    arguments: new[]
                    {
                        CreateArg("arena_id", FlowArgKind.String, "Arena ID", required: true),
                        CreateArg("stat_type", FlowArgKind.Enum, "Statistics type", required: false, allowedValues: new[] { "Match", "Player", "Team", "All" })
                    }
                ),

                CreateFlowDefinition(
                    name: "arena_event_log",
                    description: "Get arena event log",
                    adminOnly: true,
                    supportedZoneTypes: new[] { "arena" },
                    tags: new[] { "arena", "event", "log", "admin", "query" },
                    arguments: new[]
                    {
                        CreateArg("arena_id", FlowArgKind.String, "Arena ID", required: true),
                        CreateArg("limit", FlowArgKind.Int, "Number of events", required: false)
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
            string? entityRoleName = null,
            IReadOnlyList<string>? allowedValues = null)
        {
            return new FlowArgDefinition
            {
                Name = name,
                Kind = kind,
                Required = required,
                Description = description,
                EntityRoleName = entityRoleName ?? string.Empty,
                AllowedValues = allowedValues ?? Array.Empty<string>()
            };
        }
    }
}
