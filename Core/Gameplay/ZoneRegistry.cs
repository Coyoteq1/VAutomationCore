using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VAutomationCore.Core.Gameplay.Shared.Types;
using VAutomationCore.Core.Gameplay.Shared.Contracts;

namespace VAutomationCore.Core.Gameplay
{
    /// <summary>
    /// Registry for managing zones across all gameplay modules.
    /// Each zone is owned by exactly one gameplay type.
    /// </summary>
    public static class ZoneRegistry
    {
        private static readonly ConcurrentDictionary<string, ZoneDefinition> ZonesById = new(StringComparer.OrdinalIgnoreCase);
        private static readonly ConcurrentDictionary<string, List<ZoneFlowBinding>> ZoneBindings = new(StringComparer.OrdinalIgnoreCase);
        
        private static bool _isInitialized = false;

        public static void Initialize()
        {
            ZonesById.Clear();
            ZoneBindings.Clear();
            _isInitialized = false;
        }

        /// <summary>
        /// Register a zone definition.
        /// </summary>
        public static bool Register(ZoneDefinition definition, bool replace = false)
        {
            if (definition == null || string.IsNullOrWhiteSpace(definition.ZoneId))
            {
                return false;
            }

            var key = definition.ZoneId.Trim();
            
            // Check for ownership conflict
            if (!replace && ZonesById.TryGetValue(key, out var existing))
            {
                if (existing.GameplayType != definition.GameplayType)
                {
                    // Zone already exists with different owner - reject
                    return false;
                }
            }

            if (replace)
            {
                ZonesById[key] = definition;
                return true;
            }

            return ZonesById.TryAdd(key, definition);
        }

        /// <summary>
        /// Register using legacy GameplayZoneDefinition (for backward compatibility).
        /// </summary>
        public static bool Register(GameplayZoneDefinition definition, bool replace = false)
        {
            var newDef = ConvertLegacyDefinition(definition);
            return Register(newDef, replace);
        }

        /// <summary>
        /// Get a zone by its ID.
        /// </summary>
        public static ZoneDefinition? GetZone(string zoneId)
        {
            return ZonesById.TryGetValue(zoneId, out var zone) ? zone : null;
        }

        /// <summary>
        /// Get all zones owned by a specific gameplay type.
        /// </summary>
        public static IReadOnlyList<ZoneDefinition> GetZonesByGameplay(GameplayType gameType)
        {
            return ZonesById.Values
                .Where(z => z.GameplayType == gameType)
                .ToList();
        }

        /// <summary>
        /// Get all zones.
        /// </summary>
        public static IReadOnlyList<ZoneDefinition> GetAllZones()
        {
            return ZonesById.Values.ToList();
        }

        /// <summary>
        /// Get all zones of a specific zone type.
        /// </summary>
        public static IReadOnlyList<ZoneDefinition> GetZonesByType(string zoneType)
        {
            return ZonesById.Values
                .Where(z => string.Equals(z.ZoneType, zoneType, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        /// <summary>
        /// Get all enabled zones.
        /// </summary>
        public static IReadOnlyList<ZoneDefinition> GetEnabledZones()
        {
            return ZonesById.Values
                .Where(z => z.Enabled)
                .ToList();
        }

        /// <summary>
        /// Bind a flow to a zone for a specific trigger.
        /// </summary>
        public static bool BindFlow(string zoneId, string flowName, ZoneEventTrigger trigger, int priority = 0)
        {
            if (!ZonesById.ContainsKey(zoneId))
            {
                return false;
            }

            var key = zoneId;
            if (!ZoneBindings.TryGetValue(key, out var bindings))
            {
                bindings = new List<ZoneFlowBinding>();
                ZoneBindings[key] = bindings;
            }

            // Check if binding already exists
            var existing = bindings.FirstOrDefault(b => 
                b.FlowName == flowName && b.Trigger == trigger);
            
            if (existing != null)
            {
                return false; // Already bound
            }

            bindings.Add(new ZoneFlowBinding
            {
                ZoneId = zoneId,
                FlowName = flowName,
                Trigger = trigger,
                Priority = priority,
                IsEnabled = true
            });

            return true;
        }

        /// <summary>
        /// Unbind a flow from a zone.
        /// </summary>
        public static bool UnbindFlow(string zoneId, string flowName, ZoneEventTrigger trigger)
        {
            if (!ZoneBindings.TryGetValue(zoneId, out var bindings))
            {
                return false;
            }

            var binding = bindings.FirstOrDefault(b => 
                b.FlowName == flowName && b.Trigger == trigger);
            
            if (binding == null)
            {
                return false;
            }

            bindings.Remove(binding);
            return true;
        }

        /// <summary>
        /// Get flow bindings for a zone.
        /// </summary>
        public static IReadOnlyList<ZoneFlowBinding> GetBindings(string zoneId)
        {
            return ZoneBindings.TryGetValue(zoneId, out var bindings) 
                ? bindings.OrderBy(b => b.Priority).ToList() 
                : Array.Empty<ZoneFlowBinding>();
        }

        /// <summary>
        /// Get flow bindings for a zone filtered by trigger.
        /// </summary>
        public static IReadOnlyList<ZoneFlowBinding> GetBindings(string zoneId, ZoneEventTrigger trigger)
        {
            return ZoneBindings.TryGetValue(zoneId, out var bindings)
                ? bindings.Where(b => b.Trigger == trigger).OrderBy(b => b.Priority).ToList()
                : Array.Empty<ZoneFlowBinding>();
        }

        /// <summary>
        /// Enable a zone.
        /// </summary>
        public static bool EnableZone(string zoneId)
        {
            if (!ZonesById.TryGetValue(zoneId, out var zone))
            {
                return false;
            }

            var updated = new ZoneDefinition
            {
                ZoneId = zone.ZoneId,
                GameplayType = zone.GameplayType,
                ZoneType = zone.ZoneType,
                ZoneShape = zone.ZoneShape,
                Center = zone.Center,
                Radius = zone.Radius,
                BoxDimensions = zone.BoxDimensions,
                CylinderHeight = zone.CylinderHeight,
                Enabled = true,
                RuleProfileId = zone.RuleProfileId,
                EntryFlows = zone.EntryFlows,
                ExitFlows = zone.ExitFlows,
                TickFlows = zone.TickFlows,
                MatchStartFlows = zone.MatchStartFlows,
                MatchEndFlows = zone.MatchEndFlows,
                Metadata = zone.Metadata,
                DisplayName = zone.DisplayName,
                Description = zone.Description,
                MaxPlayers = zone.MaxPlayers,
                MinPlayers = zone.MinPlayers,
                TickIntervalSeconds = zone.TickIntervalSeconds
            };

            ZonesById[zoneId] = updated;
            return true;
        }

        /// <summary>
        /// Disable a zone.
        /// </summary>
        public static bool DisableZone(string zoneId)
        {
            if (!ZonesById.TryGetValue(zoneId, out var zone))
            {
                return false;
            }

            var updated = new ZoneDefinition
            {
                ZoneId = zone.ZoneId,
                GameplayType = zone.GameplayType,
                ZoneType = zone.ZoneType,
                ZoneShape = zone.ZoneShape,
                Center = zone.Center,
                Radius = zone.Radius,
                BoxDimensions = zone.BoxDimensions,
                CylinderHeight = zone.CylinderHeight,
                Enabled = false,
                RuleProfileId = zone.RuleProfileId,
                EntryFlows = zone.EntryFlows,
                ExitFlows = zone.ExitFlows,
                TickFlows = zone.TickFlows,
                MatchStartFlows = zone.MatchStartFlows,
                MatchEndFlows = zone.MatchEndFlows,
                Metadata = zone.Metadata,
                DisplayName = zone.DisplayName,
                Description = zone.Description,
                MaxPlayers = zone.MaxPlayers,
                MinPlayers = zone.MinPlayers,
                TickIntervalSeconds = zone.TickIntervalSeconds
            };

            ZonesById[zoneId] = updated;
            return true;
        }

        /// <summary>
        /// Validate all zones for ownership and conflicts.
        /// </summary>
        public static IReadOnlyList<string> Validate()
        {
            var errors = new List<string>();

            // Check for duplicate zone IDs with different owners
            var zoneIdGroups = ZonesById.Values.GroupBy(z => z.ZoneId.ToLowerInvariant());
            foreach (var group in zoneIdGroups)
            {
                var owners = group.Select(z => z.GameplayType).Distinct().ToList();
                if (owners.Count > 1)
                {
                    errors.Add($"Zone '{group.Key}' has conflicting owners: {string.Join(", ", owners)}");
                }
            }

            // Check for invalid references
            foreach (var zone in ZonesById.Values)
            {
                if (!Enum.IsDefined(typeof(GameplayType), zone.GameplayType))
                {
                    errors.Add($"Zone '{zone.ZoneId}' has invalid gameplay type: {zone.GameplayType}");
                }

                if (string.IsNullOrWhiteSpace(zone.ZoneType))
                {
                    errors.Add($"Zone '{zone.ZoneId}' has empty zone type");
                }
            }

            return errors;
        }

        /// <summary>
        /// Mark initialization as complete.
        /// </summary>
        public static void SetInitialized()
        {
            _isInitialized = true;
        }

        /// <summary>
        /// Check if initialization is complete.
        /// </summary>
        public static bool IsInitialized => _isInitialized;

        /// <summary>
        /// Get comprehensive summary.
        /// </summary>
        public static string GetSummary()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Zone Registry ===");
            sb.AppendLine($"Total Zones: {ZonesById.Count}");
            sb.AppendLine($"Enabled Zones: {ZonesById.Values.Count(z => z.Enabled)}");
            sb.AppendLine($"Total Bindings: {ZoneBindings.Values.Sum(b => b.Count)}");
            sb.AppendLine();

            // Summary by gameplay type
            sb.AppendLine("Zones by Gameplay Type:");
            foreach (var group in ZonesById.Values.GroupBy(z => z.GameplayType).OrderBy(g => g.Key))
            {
                var enabled = group.Count(z => z.Enabled);
                sb.AppendLine($"  {group.Key}: {group.Count()} zones ({enabled} enabled)");
            }

            // Summary by zone type
            sb.AppendLine();
            sb.AppendLine("Zones by Type:");
            foreach (var group in ZonesById.Values.GroupBy(z => z.ZoneType).OrderBy(g => g.Key))
            {
                sb.AppendLine($"  {group.Key}: {group.Count()} zones");
            }

            return sb.ToString();
        }

        private static ZoneDefinition ConvertLegacyDefinition(GameplayZoneDefinition legacy)
        {
            return new ZoneDefinition
            {
                ZoneId = legacy.ZoneId,
                GameplayType = legacy.GameplayType,
                ZoneType = legacy.ZoneType,
                ZoneShape = legacy.ZoneShape,
                Center = legacy.Center,
                Radius = legacy.Radius,
                Enabled = legacy.Enabled,
                RuleProfileId = legacy.RuleProfileId,
                EntryFlows = legacy.EntryFlows,
                ExitFlows = legacy.ExitFlows,
                TickFlows = legacy.TickFlows,
                Metadata = legacy.AdminOverrides,
                DisplayName = legacy.ZoneId,
                Description = string.Empty
            };
        }
    }
}
