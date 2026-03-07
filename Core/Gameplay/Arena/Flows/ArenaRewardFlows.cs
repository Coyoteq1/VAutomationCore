using System;
using System.Collections.Generic;
using VAutomationCore.Core.Gameplay;
using VAutomationCore.Core.Gameplay.Shared.Types;

namespace VAutomationCore.Core.Gameplay.Arena.Flows
{
    /// <summary>
    /// Arena reward and scoring flows.
    /// These flows are owned by the Arena module.
    /// </summary>
    public static class ArenaRewardFlows
    {
        /// <summary>
        /// Get all reward flow definitions for Arena.
        /// </summary>
        public static IReadOnlyList<FlowDefinition> GetFlowDefinitions()
        {
            return new List<FlowDefinition>
            {
                // ========================================
                // Scoring Flows
                // ========================================

                CreateFlowDefinition(
                    name: "arena_score_add",
                    description: "Add points to player/team",
                    adminOnly: true,
                    supportedZoneTypes: new[] { "arena" },
                    tags: new[] { "arena", "score", "admin" },
                    arguments: new[]
                    {
                        CreateArg("arena_id", FlowArgKind.String, "Arena ID", required: true),
                        CreateArg("player", FlowArgKind.Player, "Player to add points", required: false, entityRoleName: "ArenaPlayer"),
                        CreateArg("team", FlowArgKind.Team, "Team to add points", required: false),
                        CreateArg("points", FlowArgKind.Int, "Points to add", required: true)
                    }
                ),

                CreateFlowDefinition(
                    name: "arena_score_get",
                    description: "Get player/team score",
                    adminOnly: false,
                    supportedZoneTypes: new[] { "arena" },
                    tags: new[] { "arena", "score", "query" },
                    arguments: new[]
                    {
                        CreateArg("arena_id", FlowArgKind.String, "Arena ID", required: true),
                        CreateArg("player", FlowArgKind.Player, "Player to query", required: false, entityRoleName: "ArenaPlayer"),
                        CreateArg("team", FlowArgKind.Team, "Team to query", required: false)
                    }
                ),

                CreateFlowDefinition(
                    name: "arena_leaderboard_get",
                    description: "Get arena leaderboard",
                    adminOnly: false,
                    supportedZoneTypes: new[] { "arena" },
                    tags: new[] { "arena", "leaderboard", "query" },
                    arguments: new[]
                    {
                        CreateArg("arena_id", FlowArgKind.String, "Arena ID", required: true),
                        CreateArg("limit", FlowArgKind.Int, "Number of entries", required: false)
                    }
                ),

                // ========================================
                // Reward Flows
                // ========================================

                CreateFlowDefinition(
                    name: "arena_reward_grant",
                    description: "Grant reward to player",
                    adminOnly: true,
                    supportedZoneTypes: new[] { "arena" },
                    tags: new[] { "arena", "reward", "admin" },
                    arguments: new[]
                    {
                        CreateArg("arena_id", FlowArgKind.String, "Arena ID", required: true),
                        CreateArg("player", FlowArgKind.Player, "Player to reward", required: true, entityRoleName: "ArenaPlayer"),
                        CreateArg("reward_table", FlowArgKind.RewardTable, "Reward table to use", required: false)
                    }
                ),

                CreateFlowDefinition(
                    name: "arena_reward_distribute",
                    description: "Distribute rewards to participants",
                    adminOnly: true,
                    supportedZoneTypes: new[] { "arena" },
                    tags: new[] { "arena", "reward", "admin" },
                    arguments: new[]
                    {
                        CreateArg("arena_id", FlowArgKind.String, "Arena ID", required: true),
                        CreateArg("reward_table", FlowArgKind.RewardTable, "Reward table to use", required: false)
                    }
                ),

                // ========================================
                // Kill/Death Tracking
                // ========================================

                CreateFlowDefinition(
                    name: "arena_kill_count",
                    description: "Get player kill count",
                    adminOnly: false,
                    supportedZoneTypes: new[] { "arena" },
                    tags: new[] { "arena", "kills", "query" },
                    arguments: new[]
                    {
                        CreateArg("arena_id", FlowArgKind.String, "Arena ID", required: true),
                        CreateArg("player", FlowArgKind.Player, "Player to query", required: true, entityRoleName: "ArenaPlayer")
                    }
                ),

                CreateFlowDefinition(
                    name: "arena_death_count",
                    description: "Get player death count",
                    adminOnly: false,
                    supportedZoneTypes: new[] { "arena" },
                    tags: new[] { "arena", "deaths", "query" },
                    arguments: new[]
                    {
                        CreateArg("arena_id", FlowArgKind.String, "Arena ID", required: true),
                        CreateArg("player", FlowArgKind.Player, "Player to query", required: true, entityRoleName: "ArenaPlayer")
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
            string? payloadTypeName = null,
            string? entityRoleName = null)
        {
            return new FlowArgDefinition
            {
                Name = name,
                Kind = kind,
                Required = required,
                Description = description,
                PayloadTypeName = payloadTypeName ?? string.Empty,
                EntityRoleName = entityRoleName ?? string.Empty
            };
        }
    }
}
