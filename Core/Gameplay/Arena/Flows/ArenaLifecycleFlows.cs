using System;
using System.Collections.Generic;
using VAutomationCore.Core.Gameplay;
using VAutomationCore.Core.Gameplay.Shared.Types;

namespace VAutomationCore.Core.Gameplay.Arena.Flows
{
    /// <summary>
    /// Arena lifecycle flows - enable/disable/create/delete operations.
    /// These flows are owned by the Arena module.
    /// </summary>
    public static class ArenaLifecycleFlows
    {
        /// <summary>
        /// Get all lifecycle flow definitions for Arena.
        /// </summary>
        public static IReadOnlyList<FlowDefinition> GetFlowDefinitions()
        {
            return new List<FlowDefinition>
            {
                // ========================================
                // Arena Management Flows
                // ========================================

                CreateFlowDefinition(
                    name: "arena_enable",
                    description: "Enable an arena for gameplay",
                    adminOnly: true,
                    supportedZoneTypes: new[] { "arena" },
                    tags: new[] { "arena", "lifecycle", "admin" },
                    arguments: new[]
                    {
                        CreateArg("arena_id", FlowArgKind.String, "ID of the arena to enable", required: true),
                        CreateArg("enable_reason", FlowArgKind.String, "Reason for enabling", required: false)
                    }
                ),

                CreateFlowDefinition(
                    name: "arena_disable",
                    description: "Disable an arena",
                    adminOnly: true,
                    supportedZoneTypes: new[] { "arena" },
                    tags: new[] { "arena", "lifecycle", "admin" },
                    arguments: new[]
                    {
                        CreateArg("arena_id", FlowArgKind.String, "ID of the arena to disable", required: true),
                        CreateArg("disable_reason", FlowArgKind.String, "Reason for disabling", required: false)
                    }
                ),

                CreateFlowDefinition(
                    name: "arena_create",
                    description: "Create a new arena",
                    adminOnly: true,
                    supportedZoneTypes: new[] { "arena" },
                    tags: new[] { "arena", "lifecycle", "admin", "creation" },
                    arguments: new[]
                    {
                        CreateArg("arena_id", FlowArgKind.String, "ID for the new arena", required: true),
                        CreateArg("arena_name", FlowArgKind.String, "Display name", required: false),
                        CreateArg("center", FlowArgKind.Position, "Center position", required: false),
                        CreateArg("radius", FlowArgKind.Radius, "Zone radius", required: false),
                        CreateArg("rules_profile", FlowArgKind.RuleProfile, "Rule profile to apply", required: false, payloadTypeName: "ArenaRuleProfile"),
                        CreateArg("match_mode", FlowArgKind.Enum, "Match mode", required: false, allowedValues: new[] { "Duel", "TeamDuel", "FreeForAll", "CaptureTheFlag", "KingOfTheHill", "Survival", "WaveDefense", "BossRush" })
                    }
                ),

                CreateFlowDefinition(
                    name: "arena_delete",
                    description: "Delete an arena",
                    adminOnly: true,
                    supportedZoneTypes: new[] { "arena" },
                    tags: new[] { "arena", "lifecycle", "admin", "deletion" },
                    arguments: new[]
                    {
                        CreateArg("arena_id", FlowArgKind.String, "ID of the arena to delete", required: true)
                    }
                ),

                CreateFlowDefinition(
                    name: "arena_status",
                    description: "Get arena status",
                    adminOnly: false,
                    supportedZoneTypes: new[] { "arena" },
                    tags: new[] { "arena", "status", "query" },
                    arguments: new[]
                    {
                        CreateArg("arena_id", FlowArgKind.String, "ID of the arena", required: false)
                    }
                ),

                CreateFlowDefinition(
                    name: "arena_list",
                    description: "List all arenas",
                    adminOnly: false,
                    supportedZoneTypes: Array.Empty<string>(),
                    tags: new[] { "arena", "list", "query" },
                    arguments: Array.Empty<FlowArgDefinition>()
                ),

                // ========================================
                // Zone Binding Flows
                // ========================================

                CreateFlowDefinition(
                    name: "arena_bind_zone",
                    description: "Bind arena to a zone",
                    adminOnly: true,
                    supportedZoneTypes: new[] { "arena", "zone" },
                    tags: new[] { "arena", "zone", "binding", "admin" },
                    arguments: new[]
                    {
                        CreateArg("arena_id", FlowArgKind.String, "Arena ID", required: true),
                        CreateArg("zone", FlowArgKind.Zone, "Zone to bind", required: true),
                        CreateArg("rules_profile", FlowArgKind.RuleProfile, "Rule profile", required: false, payloadTypeName: "ArenaRuleProfile")
                    }
                ),

                CreateFlowDefinition(
                    name: "arena_unbind_zone",
                    description: "Unbind arena from a zone",
                    adminOnly: true,
                    supportedZoneTypes: new[] { "arena", "zone" },
                    tags: new[] { "arena", "zone", "binding", "admin" },
                    arguments: new[]
                    {
                        CreateArg("arena_id", FlowArgKind.String, "Arena ID", required: true),
                        CreateArg("zone", FlowArgKind.Zone, "Zone to unbind", required: true)
                    }
                ),

                CreateFlowDefinition(
                    name: "arena_zone_enable",
                    description: "Enable arena zone",
                    adminOnly: true,
                    supportedZoneTypes: new[] { "arena" },
                    tags: new[] { "arena", "zone", "admin" },
                    arguments: new[]
                    {
                        CreateArg("arena_id", FlowArgKind.String, "Arena ID", required: true),
                        CreateArg("zone", FlowArgKind.Zone, "Zone to enable", required: true)
                    }
                ),

                CreateFlowDefinition(
                    name: "arena_zone_disable",
                    description: "Disable arena zone",
                    adminOnly: true,
                    supportedZoneTypes: new[] { "arena" },
                    tags: new[] { "arena", "zone", "admin" },
                    arguments: new[]
                    {
                        CreateArg("arena_id", FlowArgKind.String, "Arena ID", required: true),
                        CreateArg("zone", FlowArgKind.Zone, "Zone to disable", required: true)
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
                EnabledByDefault = true,
                CooldownSeconds = 0
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
