using System;
using System.Collections.Generic;
using VAutomationCore.Core.Gameplay;
using VAutomationCore.Core.Gameplay.Shared.Types;

namespace VAutomationCore.Core.Gameplay.Arena.Flows
{
    /// <summary>
    /// Arena player management flows.
    /// These flows are owned by the Arena module.
    /// </summary>
    public static class ArenaPlayerFlows
    {
        /// <summary>
        /// Get all player flow definitions for Arena.
        /// </summary>
        public static IReadOnlyList<FlowDefinition> GetFlowDefinitions()
        {
            return new List<FlowDefinition>
            {
                // ========================================
                // Player Join/Leave Flows
                // ========================================

                CreateFlowDefinition(
                    name: "arena_player_join",
                    description: "Player joins an arena",
                    adminOnly: false,
                    supportedZoneTypes: new[] { "arena" },
                    tags: new[] { "arena", "player", "join" },
                    arguments: new[]
                    {
                        CreateArg("arena_id", FlowArgKind.String, "Arena ID", required: true),
                        CreateArg("player", FlowArgKind.Player, "Player to join", required: true, entityRoleName: "ArenaPlayer"),
                        CreateArg("team", FlowArgKind.Team, "Team assignment", required: false)
                    }
                ),

                CreateFlowDefinition(
                    name: "arena_player_leave",
                    description: "Player leaves an arena",
                    adminOnly: false,
                    supportedZoneTypes: new[] { "arena" },
                    tags: new[] { "arena", "player", "leave" },
                    arguments: new[]
                    {
                        CreateArg("arena_id", FlowArgKind.String, "Arena ID", required: true),
                        CreateArg("player", FlowArgKind.Player, "Player to remove", required: true, entityRoleName: "ArenaPlayer"),
                        CreateArg("leave_reason", FlowArgKind.String, "Reason for leaving", required: false)
                    }
                ),

                CreateFlowDefinition(
                    name: "arena_player_kick",
                    description: "Kick player from arena",
                    adminOnly: true,
                    supportedZoneTypes: new[] { "arena" },
                    tags: new[] { "arena", "player", "admin", "kick" },
                    arguments: new[]
                    {
                        CreateArg("arena_id", FlowArgKind.String, "Arena ID", required: true),
                        CreateArg("player", FlowArgKind.Player, "Player to kick", required: true, entityRoleName: "ArenaPlayer"),
                        CreateArg("kick_reason", FlowArgKind.String, "Reason for kick", required: false)
                    }
                ),

                CreateFlowDefinition(
                    name: "arena_player_ban",
                    description: "Ban player from arena",
                    adminOnly: true,
                    supportedZoneTypes: new[] { "arena" },
                    tags: new[] { "arena", "player", "admin", "ban" },
                    arguments: new[]
                    {
                        CreateArg("arena_id", FlowArgKind.String, "Arena ID", required: true),
                        CreateArg("player", FlowArgKind.Player, "Player to ban", required: true, entityRoleName: "ArenaPlayer"),
                        CreateArg("ban_duration", FlowArgKind.Duration, "Ban duration", required: false)
                    }
                ),

                // ========================================
                // Team Management Flows
                // ========================================

                CreateFlowDefinition(
                    name: "arena_team_assign",
                    description: "Assign player to team",
                    adminOnly: true,
                    supportedZoneTypes: new[] { "arena" },
                    tags: new[] { "arena", "team", "admin" },
                    arguments: new[]
                    {
                        CreateArg("arena_id", FlowArgKind.String, "Arena ID", required: true),
                        CreateArg("player", FlowArgKind.Player, "Player to assign", required: true, entityRoleName: "ArenaPlayer"),
                        CreateArg("team", FlowArgKind.Team, "Team number", required: true)
                    }
                ),

                CreateFlowDefinition(
                    name: "arena_team_auto",
                    description: "Auto-assign player to team",
                    adminOnly: false,
                    supportedZoneTypes: new[] { "arena" },
                    tags: new[] { "arena", "team" },
                    arguments: new[]
                    {
                        CreateArg("arena_id", FlowArgKind.String, "Arena ID", required: true),
                        CreateArg("player", FlowArgKind.Player, "Player to assign", required: true, entityRoleName: "ArenaPlayer")
                    }
                ),

                CreateFlowDefinition(
                    name: "arena_team_balance",
                    description: "Balance teams",
                    adminOnly: true,
                    supportedZoneTypes: new[] { "arena" },
                    tags: new[] { "arena", "team", "admin" },
                    arguments: new[]
                    {
                        CreateArg("arena_id", FlowArgKind.String, "Arena ID", required: true)
                    }
                ),

                // ========================================
                // Spectator Flows
                // ========================================

                CreateFlowDefinition(
                    name: "arena_spectator_enable",
                    description: "Enable spectator mode for player",
                    adminOnly: true,
                    supportedZoneTypes: new[] { "arena" },
                    tags: new[] { "arena", "spectator", "admin" },
                    arguments: new[]
                    {
                        CreateArg("arena_id", FlowArgKind.String, "Arena ID", required: true),
                        CreateArg("player", FlowArgKind.Player, "Player to enable spectating", required: true, entityRoleName: "ArenaSpectator"),
                        CreateArg("view_mode", FlowArgKind.Enum, "View mode", required: false, allowedValues: new[] { "Free", "Follow", "FirstPerson" })
                    }
                ),

                CreateFlowDefinition(
                    name: "arena_spectator_disable",
                    description: "Disable spectator mode for player",
                    adminOnly: true,
                    supportedZoneTypes: new[] { "arena" },
                    tags: new[] { "arena", "spectator", "admin" },
                    arguments: new[]
                    {
                        CreateArg("arena_id", FlowArgKind.String, "Arena ID", required: true),
                        CreateArg("player", FlowArgKind.Player, "Player to disable spectating", required: true, entityRoleName: "ArenaSpectator")
                    }
                ),

                CreateFlowDefinition(
                    name: "arena_spectator_join",
                    description: "Player joins as spectator",
                    adminOnly: false,
                    supportedZoneTypes: new[] { "arena" },
                    tags: new[] { "arena", "spectator" },
                    arguments: new[]
                    {
                        CreateArg("arena_id", FlowArgKind.String, "Arena ID", required: true),
                        CreateArg("player", FlowArgKind.Player, "Player joining as spectator", required: true, entityRoleName: "ArenaSpectator")
                    }
                ),

                CreateFlowDefinition(
                    name: "arena_spectator_leave",
                    description: "Player leaves spectator mode",
                    adminOnly: false,
                    supportedZoneTypes: new[] { "arena" },
                    tags: new[] { "arena", "spectator" },
                    arguments: new[]
                    {
                        CreateArg("arena_id", FlowArgKind.String, "Arena ID", required: true),
                        CreateArg("player", FlowArgKind.Player, "Player leaving spectator mode", required: true, entityRoleName: "ArenaSpectator")
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
            string? entityRoleName = null,
            IReadOnlyList<string>? allowedValues = null)
        {
            return new FlowArgDefinition
            {
                Name = name,
                Kind = kind,
                Required = required,
                Description = description,
                PayloadTypeName = payloadTypeName ?? string.Empty,
                EntityRoleName = entityRoleName ?? string.Empty,
                AllowedValues = allowedValues ?? Array.Empty<string>()
            };
        }
    }
}
