using System;
using System.Collections.Generic;
using VAutomationCore.Core.Gameplay;
using VAutomationCore.Core.Gameplay.Shared.Types;

namespace VAutomationCore.Core.Gameplay.Arena.Flows
{
    /// <summary>
    /// Arena match control flows.
    /// These flows are owned by the Arena module.
    /// </summary>
    public static class ArenaMatchFlows
    {
        /// <summary>
        /// Get all match flow definitions for Arena.
        /// </summary>
        public static IReadOnlyList<FlowDefinition> GetFlowDefinitions()
        {
            return new List<FlowDefinition>
            {
                // ========================================
                // Match Control Flows
                // ========================================

                CreateFlowDefinition(
                    name: "arena_match_start",
                    description: "Start a match in an arena",
                    adminOnly: true,
                    supportedZoneTypes: new[] { "arena" },
                    tags: new[] { "arena", "match", "control" },
                    arguments: new[]
                    {
                        CreateArg("arena_id", FlowArgKind.String, "Arena ID", required: true),
                        CreateArg("participants", FlowArgKind.Player, "Players to join", required: false),
                        CreateArg("match_mode", FlowArgKind.Enum, "Match mode", required: false, allowedValues: new[] { "Duel", "TeamDuel", "FreeForAll", "CaptureTheFlag", "KingOfTheHill", "Survival", "WaveDefense", "BossRush" })
                    }
                ),

                CreateFlowDefinition(
                    name: "arena_match_stop",
                    description: "Stop a match in an arena",
                    adminOnly: true,
                    supportedZoneTypes: new[] { "arena" },
                    tags: new[] { "arena", "match", "control" },
                    arguments: new[]
                    {
                        CreateArg("arena_id", FlowArgKind.String, "Arena ID", required: true),
                        CreateArg("stop_reason", FlowArgKind.String, "Reason for stopping", required: false)
                    }
                ),

                CreateFlowDefinition(
                    name: "arena_match_reset",
                    description: "Reset an arena match",
                    adminOnly: true,
                    supportedZoneTypes: new[] { "arena" },
                    tags: new[] { "arena", "match", "control", "reset" },
                    arguments: new[]
                    {
                        CreateArg("arena_id", FlowArgKind.String, "Arena ID", required: true),
                        CreateArg("reset_mode", FlowArgKind.Enum, "Reset mode", required: false, allowedValues: new[] { "Full", "Scores", "Players" })
                    }
                ),

                CreateFlowDefinition(
                    name: "arena_match_pause",
                    description: "Pause a match",
                    adminOnly: true,
                    supportedZoneTypes: new[] { "arena" },
                    tags: new[] { "arena", "match", "control" },
                    arguments: new[]
                    {
                        CreateArg("arena_id", FlowArgKind.String, "Arena ID", required: true)
                    }
                ),

                CreateFlowDefinition(
                    name: "arena_match_resume",
                    description: "Resume a paused match",
                    adminOnly: true,
                    supportedZoneTypes: new[] { "arena" },
                    tags: new[] { "arena", "match", "control" },
                    arguments: new[]
                    {
                        CreateArg("arena_id", FlowArgKind.String, "Arena ID", required: true)
                    }
                ),

                // ========================================
                // Round/Wave Control
                // ========================================

                CreateFlowDefinition(
                    name: "arena_round_start",
                    description: "Start a new round",
                    adminOnly: true,
                    supportedZoneTypes: new[] { "arena" },
                    tags: new[] { "arena", "round", "control" },
                    arguments: new[]
                    {
                        CreateArg("arena_id", FlowArgKind.String, "Arena ID", required: true),
                        CreateArg("round_number", FlowArgKind.Int, "Round number", required: false)
                    }
                ),

                CreateFlowDefinition(
                    name: "arena_round_end",
                    description: "End current round",
                    adminOnly: true,
                    supportedZoneTypes: new[] { "arena" },
                    tags: new[] { "arena", "round", "control" },
                    arguments: new[]
                    {
                        CreateArg("arena_id", FlowArgKind.String, "Arena ID", required: true)
                    }
                ),

                // ========================================
                // Countdown Control
                // ========================================

                CreateFlowDefinition(
                    name: "arena_countdown_start",
                    description: "Start match countdown",
                    adminOnly: true,
                    supportedZoneTypes: new[] { "arena" },
                    tags: new[] { "arena", "countdown", "control" },
                    arguments: new[]
                    {
                        CreateArg("arena_id", FlowArgKind.String, "Arena ID", required: true),
                        CreateArg("duration", FlowArgKind.Duration, "Countdown duration", required: false)
                    }
                ),

                CreateFlowDefinition(
                    name: "arena_countdown_cancel",
                    description: "Cancel match countdown",
                    adminOnly: true,
                    supportedZoneTypes: new[] { "arena" },
                    tags: new[] { "arena", "countdown", "control" },
                    arguments: new[]
                    {
                        CreateArg("arena_id", FlowArgKind.String, "Arena ID", required: true)
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
            IReadOnlyList<string>? allowedValues = null)
        {
            return new FlowArgDefinition
            {
                Name = name,
                Kind = kind,
                Required = required,
                Description = description,
                PayloadTypeName = payloadTypeName ?? string.Empty,
                AllowedValues = allowedValues ?? Array.Empty<string>()
            };
        }
    }
}
