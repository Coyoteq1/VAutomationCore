using System;
using System.Collections.Generic;
using System.Linq;
using VAutomationCore.Core.Gameplay;
using VAutomationCore.Core.Gameplay.Shared.Types;

namespace VAutomationCore.Core.Gameplay
{
    /// <summary>
    /// Migration system for existing flows.
    /// Maintains backward compatibility while registering flows with proper module ownership.
    /// This enables gradual migration without breaking existing flow names.
    /// </summary>
    public static class FlowMigrationSystem
    {
        /// <summary>
        /// Legacy flow domains and their corresponding GameplayTypes.
        /// </summary>
        private static readonly Dictionary<string, GameplayType> DomainToGameplayMap = new(StringComparer.OrdinalIgnoreCase)
        {
            // Core gameplay modules
            ["Arena"] = GameplayType.Arena,
            ["Boss"] = GameplayType.Boss,
            ["Dungeon"] = GameplayType.Dungeon,
            ["Harvest"] = GameplayType.Harvest,
            ["Raid"] = GameplayType.Raid,
            ["Event"] = GameplayType.Event,
            ["Escort"] = GameplayType.Escort,
            
            // Territory/Zone flows - these become part of Territory gameplay type
            ["Zone"] = GameplayType.Territory,
            ["CastleBuilding"] = GameplayType.Territory,
            ["CastleTerritory"] = GameplayType.Territory,
            ["PlacementRestriction"] = GameplayType.Territory,
            
            // These could become separate modules but are currently generic
            ["GameObjects"] = GameplayType.Progression,
            ["Glow"] = GameplayType.Progression,
            ["VBlood"] = GameplayType.Progression,
            ["Abilities"] = GameplayType.Progression,
            ["FXAndGameObjects"] = GameplayType.Progression,
            ["EquipmentAndKits"] = GameplayType.Progression,
            ["SpawnTag"] = GameplayType.Progression,
            ["VisibilityAndStealth"] = GameplayType.Progression
        };

        /// <summary>
        /// Migrate a legacy flow registration to the new system.
        /// </summary>
        public static void MigrateFlow(
            string flowName,
            string domain,
            string description,
            string[] parameters,
            bool isAdminOnly,
            bool requiresPermission,
            string[] tags)
        {
            var gameType = GetGameplayTypeForDomain(domain);
            
            var flowDef = new FlowDefinition
            {
                Name = flowName,
                GameplayType = gameType,
                Description = description,
                AdminOnly = isAdminOnly,
                SupportedZoneTypes = GetDefaultZoneTypes(gameType),
                Tags = tags ?? Array.Empty<string>(),
                Arguments = parameters?.Select(p => new FlowArgDefinition
                {
                    Name = p,
                    Kind = InferArgKind(p),
                    Required = true,
                    Description = $"Parameter: {p}"
                }).ToArray() ?? Array.Empty<FlowArgDefinition>()
            };

            GameplayFlowRegistry.Register(flowDef, replace: true);
        }

        /// <summary>
        /// Migrate all legacy flows from the old registry to the new system.
        /// This maintains backward compatibility.
        /// </summary>
        public static void MigrateAllLegacyFlows()
        {
            // These will be called after the old FlowRegistrySystem.Initialize() completes
            // to pick up all registered flows and migrate them
            
            // For now, we provide the mapping - actual migration happens after
            // FlowRegistrySystem.Initialize() is called
        }

        /// <summary>
        /// Get GameplayType for a legacy domain name.
        /// </summary>
        public static GameplayType GetGameplayTypeForDomain(string domain)
        {
            if (DomainToGameplayMap.TryGetValue(domain, out var gameType))
            {
                return gameType;
            }
            
            // Default to Progression for unknown domains
            return GameplayType.Progression;
        }

        /// <summary>
        /// Get default zone types for a gameplay type.
        /// </summary>
        private static string[] GetDefaultZoneTypes(GameplayType gameType)
        {
            return gameType switch
            {
                GameplayType.Arena => new[] { "arena" },
                GameplayType.Boss => new[] { "boss_lair" },
                GameplayType.Dungeon => new[] { "dungeon" },
                GameplayType.Harvest => new[] { "harvest_zone" },
                GameplayType.Raid => new[] { "raid_zone" },
                GameplayType.Event => new[] { "event_zone" },
                GameplayType.Escort => new[] { "escort_zone" },
                GameplayType.Territory => new[] { "territory", "castle_territory" },
                GameplayType.SafeZone => new[] { "safe_zone" },
                _ => Array.Empty<string>()
            };
        }

        /// <summary>
        /// Infer argument kind from parameter name.
        /// </summary>
        private static FlowArgKind InferArgKind(string paramName)
        {
            var lower = paramName.ToLowerInvariant();
            
            if (lower.Contains("player") || lower.Contains("user"))
                return FlowArgKind.Player;
            if (lower.Contains("entity") || lower.Contains("target"))
                return FlowArgKind.Entity;
            if (lower.Contains("prefab") || lower.Contains("guid"))
                return FlowArgKind.Prefab;
            if (lower.Contains("zone"))
                return FlowArgKind.Zone;
            if (lower.Contains("position") || lower.Contains("location") || lower.Contains("x") || lower.Contains("y") || lower.Contains("z"))
                return FlowArgKind.Position;
            if (lower.Contains("rotation") || lower.Contains("rotation"))
                return FlowArgKind.Rotation;
            if (lower.Contains("direction"))
                return FlowArgKind.Direction;
            if (lower.Contains("radius") || lower.Contains("distance"))
                return FlowArgKind.Radius;
            if (lower.Contains("bounds"))
                return FlowArgKind.Bounds;
            if (lower.Contains("quantity") || lower.Contains("count") || lower.Contains("amount"))
                return FlowArgKind.Quantity;
            if (lower.Contains("duration") || lower.Contains("time") || lower.Contains("timer"))
                return FlowArgKind.Duration;
            if (lower == "bool" || lower.Contains("enabled") || lower.Contains("disabled"))
                return FlowArgKind.Bool;
            if (lower == "int" || lower.Contains("level") || lower.Contains("count"))
                return FlowArgKind.Int;
            if (lower == "float" || lower.Contains("multiplier") || lower.Contains("scale"))
                return FlowArgKind.Float;
            if (lower.Contains("string") || lower.Contains("name") || lower.Contains("id"))
                return FlowArgKind.String;
            if (lower.Contains("enum") || lower.Contains("type") || lower.Contains("mode"))
                return FlowArgKind.Enum;
            if (lower.Contains("team"))
                return FlowArgKind.Team;
            if (lower.Contains("tag"))
                return FlowArgKind.Tag;
            if (lower.Contains("settings"))
                return FlowArgKind.Settings;
            if (lower.Contains("rule") || lower.Contains("profile"))
                return FlowArgKind.RuleProfile;
            if (lower.Contains("reward"))
                return FlowArgKind.RewardTable;
            if (lower.Contains("condition"))
                return FlowArgKind.Condition;
                
            return FlowArgKind.String;
        }

        /// <summary>
        /// Validate migration - check for conflicts between old and new registrations.
        /// </summary>
        public static IReadOnlyList<string> ValidateMigration()
        {
            var errors = new List<string>();
            
            // Check for duplicate flow names across different gameplay types
            var flowGroups = GameplayFlowRegistry.GetAllFlows()
                .GroupBy(f => f.Name.ToLowerInvariant())
                .Where(g => g.Count() > 1);
                
            foreach (var group in flowGroups)
            {
                var owners = group.Select(f => f.GameplayType).Distinct();
                if (owners.Count() > 1)
                {
                    errors.Add($"Flow '{group.Key}' has conflicting ownership: {string.Join(", ", owners)}");
                }
            }

            return errors;
        }

        /// <summary>
        /// Get migration summary.
        /// </summary>
        public static string GetMigrationSummary()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("=== Flow Migration Summary ===");
            
            var flowsByType = GameplayFlowRegistry.GetAllFlows()
                .GroupBy(f => f.GameplayType)
                .OrderBy(g => g.Key);
                
            foreach (var group in flowsByType)
            {
                sb.AppendLine($"  {group.Key}: {group.Count()} flows");
            }
            
            return sb.ToString();
        }
    }
}
