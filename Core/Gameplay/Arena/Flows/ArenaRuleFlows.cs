using System;
using System.Collections.Generic;
using VAutomationCore.Core.Gameplay;
using VAutomationCore.Core.Gameplay.Shared.Types;

namespace VAutomationCore.Core.Gameplay.Arena.Flows
{
    /// <summary>
    /// Arena rule control flows (PvP, PvE, difficulty).
    /// These flows are owned by the Arena module.
    /// </summary>
    public static class ArenaRuleFlows
    {
        /// <summary>
        /// Get all rule flow definitions for Arena.
        /// </summary>
        public static IReadOnlyList<FlowDefinition> GetFlowDefinitions()
        {
            return new List<FlowDefinition>
            {
                // ========================================
                // PvP Rule Flows
                // ========================================

                CreateFlowDefinition(
                    name: "arena_pvp_enable",
                    description: "Enable PvP in arena",
                    adminOnly: true,
                    supportedZoneTypes: new[] { "arena" },
                    tags: new[] { "arena", "pvp", "rules", "admin" },
                    arguments: new[]
                    {
                        CreateArg("arena_id", FlowArgKind.String, "Arena ID", required: true)
                    }
                ),

                CreateFlowDefinition(
                    name: "arena_pvp_disable",
                    description: "Disable PvP in arena",
                    adminOnly: true,
                    supportedZoneTypes: new[] { "arena" },
                    tags: new[] { "arena", "pvp", "rules", "admin" },
                    arguments: new[]
                    {
                        CreateArg("arena_id", FlowArgKind.String, "Arena ID", required: true)
                    }
                ),

                CreateFlowDefinition(
                    name: "arena_pvp_toggle",
                    description: "Toggle PvP in arena",
                    adminOnly: true,
                    supportedZoneTypes: new[] { "arena" },
                    tags: new[] { "arena", "pvp", "rules", "admin" },
                    arguments: new[]
                    {
                        CreateArg("arena_id", FlowArgKind.String, "Arena ID", required: true)
                    }
                ),

                CreateFlowDefinition(
                    name: "arena_friendly_fire_enable",
                    description: "Enable friendly fire",
                    adminOnly: true,
                    supportedZoneTypes: new[] { "arena" },
                    tags: new[] { "arena", "friendly_fire", "rules", "admin" },
                    arguments: new[]
                    {
                        CreateArg("arena_id", FlowArgKind.String, "Arena ID", required: true)
                    }
                ),

                CreateFlowDefinition(
                    name: "arena_friendly_fire_disable",
                    description: "Disable friendly fire",
                    adminOnly: true,
                    supportedZoneTypes: new[] { "arena" },
                    tags: new[] { "arena", "friendly_fire", "rules", "admin" },
                    arguments: new[]
                    {
                        CreateArg("arena_id", FlowArgKind.String, "Arena ID", required: true)
                    }
                ),

                // ========================================
                // Respawn Flows
                // ========================================

                CreateFlowDefinition(
                    name: "arena_respawn_enable",
                    description: "Enable respawning",
                    adminOnly: true,
                    supportedZoneTypes: new[] { "arena" },
                    tags: new[] { "arena", "respawn", "rules", "admin" },
                    arguments: new[]
                    {
                        CreateArg("arena_id", FlowArgKind.String, "Arena ID", required: true)
                    }
                ),

                CreateFlowDefinition(
                    name: "arena_respawn_disable",
                    description: "Disable respawning",
                    adminOnly: true,
                    supportedZoneTypes: new[] { "arena" },
                    tags: new[] { "arena", "respawn", "rules", "admin" },
                    arguments: new[]
                    {
                        CreateArg("arena_id", FlowArgKind.String, "Arena ID", required: true)
                    }
                ),

                CreateFlowDefinition(
                    name: "arena_respawn_timer_set",
                    description: "Set respawn timer",
                    adminOnly: true,
                    supportedZoneTypes: new[] { "arena" },
                    tags: new[] { "arena", "respawn", "rules", "admin" },
                    arguments: new[]
                    {
                        CreateArg("arena_id", FlowArgKind.String, "Arena ID", required: true),
                        CreateArg("timer", FlowArgKind.Duration, "Respawn timer in seconds", required: true)
                    }
                ),

                // ========================================
                // Rule Profile Flows
                // ========================================

                CreateFlowDefinition(
                    name: "arena_rule_profile_apply",
                    description: "Apply rule profile to arena",
                    adminOnly: true,
                    supportedZoneTypes: new[] { "arena" },
                    tags: new[] { "arena", "rules", "profile", "admin" },
                    arguments: new[]
                    {
                        CreateArg("arena_id", FlowArgKind.String, "Arena ID", required: true),
                        CreateArg("rules_profile", FlowArgKind.RuleProfile, "Rule profile to apply", required: true, payloadTypeName: "ArenaRuleProfile")
                    }
                ),

                CreateFlowDefinition(
                    name: "arena_rule_profile_list",
                    description: "List available rule profiles",
                    adminOnly: false,
                    supportedZoneTypes: Array.Empty<string>(),
                    tags: new[] { "arena", "rules", "profile", "query" },
                    arguments: Array.Empty<FlowArgDefinition>()
                ),

                // ========================================
                // Difficulty Flows
                // ========================================

                CreateFlowDefinition(
                    name: "arena_difficulty_set",
                    description: "Set arena difficulty",
                    adminOnly: true,
                    supportedZoneTypes: new[] { "arena" },
                    tags: new[] { "arena", "difficulty", "admin" },
                    arguments: new[]
                    {
                        CreateArg("arena_id", FlowArgKind.String, "Arena ID", required: true),
                        CreateArg("difficulty_level", FlowArgKind.Int, "Difficulty level (1-10)", required: true)
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
            string? payloadTypeName = null)
        {
            return new FlowArgDefinition
            {
                Name = name,
                Kind = kind,
                Required = required,
                Description = description,
                PayloadTypeName = payloadTypeName ?? string.Empty
            };
        }
    }
}
