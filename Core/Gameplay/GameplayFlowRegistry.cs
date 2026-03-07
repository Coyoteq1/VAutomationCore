using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VAutomationCore.Core.Gameplay.Shared.Types;

namespace VAutomationCore.Core.Gameplay
{
    /// <summary>
    /// Registry for managing gameplay-specific flow definitions.
    /// Flows are owned by specific gameplay modules.
    /// </summary>
    public static class GameplayFlowRegistry
    {
        private static readonly ConcurrentDictionary<string, FlowDefinition> FlowsByName = new(StringComparer.OrdinalIgnoreCase);
        private static readonly ConcurrentDictionary<GameplayType, List<FlowDefinition>> FlowsByGameplay = new();
        
        private static bool _isInitialized = false;

        public static void Initialize()
        {
            FlowsByName.Clear();
            FlowsByGameplay.Clear();
            _isInitialized = false;
        }

        /// <summary>
        /// Register a flow definition.
        /// </summary>
        public static bool Register(FlowDefinition definition, bool replace = false)
        {
            if (definition == null || string.IsNullOrWhiteSpace(definition.Name))
            {
                return false;
            }

            var key = definition.Name.Trim();
            
            if (replace)
            {
                FlowsByName[key] = definition;
                AddToGameplayIndex(definition, replace);
                return true;
            }

            if (!FlowsByName.TryAdd(key, definition))
            {
                return false;
            }

            AddToGameplayIndex(definition, replace);
            return true;
        }

        /// <summary>
        /// Register multiple flow definitions at once.
        /// </summary>
        public static bool RegisterFlows(IEnumerable<FlowDefinition> flowDefinitions, bool replace = false)
        {
            var definitions = flowDefinitions?.ToArray() ?? Array.Empty<FlowDefinition>();
            if (definitions.Length == 0)
            {
                return false;
            }

            var allSucceeded = true;
            foreach (var definition in definitions)
            {
                if (!Register(definition, replace))
                {
                    allSucceeded = false;
                }
            }

            return allSucceeded;
        }

        /// <summary>
        /// Register using legacy GameplayFlowDefinition (for backward compatibility).
        /// </summary>
        public static bool RegisterFlows(IEnumerable<GameplayFlowDefinition> flowDefinitions, bool replace = false)
        {
            var converted = flowDefinitions?.Select(ConvertLegacyDefinition).ToList();
            return RegisterFlows(converted, replace);
        }

        /// <summary>
        /// Get a flow by name.
        /// </summary>
        public static FlowDefinition? GetFlow(string flowName)
        {
            return FlowsByName.TryGetValue(flowName, out var flow) ? flow : null;
        }

        /// <summary>
        /// Get all flows owned by a specific gameplay type.
        /// </summary>
        public static IReadOnlyList<FlowDefinition> GetFlowsByGameplay(GameplayType gameType)
        {
            return FlowsByGameplay.TryGetValue(gameType, out var flows) 
                ? flows.ToList() 
                : Array.Empty<FlowDefinition>();
        }

        /// <summary>
        /// Get all registered flows.
        /// </summary>
        public static IReadOnlyCollection<FlowDefinition> GetAllFlows()
        {
            return FlowsByName.Values.ToList();
        }

        /// <summary>
        /// Check if a flow exists.
        /// </summary>
        public static bool HasFlow(string flowName)
        {
            return FlowsByName.ContainsKey(flowName);
        }

        /// <summary>
        /// Validate all flow definitions.
        /// </summary>
        public static IReadOnlyList<string> Validate()
        {
            var errors = new List<string>();

            foreach (var flow in FlowsByName.Values)
            {
                // Validate flow name
                if (string.IsNullOrWhiteSpace(flow.Name))
                {
                    errors.Add("Flow has empty name");
                }

                // Validate gameplay type
                if (!Enum.IsDefined(typeof(GameplayType), flow.GameplayType))
                {
                    errors.Add($"Flow '{flow.Name}' has invalid gameplay type: {flow.GameplayType}");
                }

                // Validate arguments
                if (flow.Arguments != null)
                {
                    var argNames = new HashSet<string>();
                    foreach (var arg in flow.Arguments)
                    {
                        if (string.IsNullOrWhiteSpace(arg.Name))
                        {
                            errors.Add($"Flow '{flow.Name}' has argument with empty name");
                        }
                        else if (!argNames.Add(arg.Name.ToLowerInvariant()))
                        {
                            errors.Add($"Flow '{flow.Name}' has duplicate argument: {arg.Name}");
                        }

                        // Validate payload type references
                        if (arg.Kind == FlowArgKind.Settings || arg.Kind == FlowArgKind.RuleProfile)
                        {
                            if (string.IsNullOrWhiteSpace(arg.PayloadTypeName))
                            {
                                errors.Add($"Flow '{flow.Name}' argument '{arg.Name}' is Settings/RuleProfile but has no PayloadTypeName");
                            }
                        }

                        // Validate entity role references
                        if (arg.Kind == FlowArgKind.Entity || arg.Kind == FlowArgKind.Player || arg.Kind == FlowArgKind.TargetEntity)
                        {
                            if (string.IsNullOrWhiteSpace(arg.EntityRoleName))
                            {
                                errors.Add($"Flow '{flow.Name}' argument '{arg.Name}' is Entity type but has no EntityRoleName");
                            }
                        }

                        // Validate prefab category references
                        if (arg.Kind == FlowArgKind.Prefab)
                        {
                            if (string.IsNullOrWhiteSpace(arg.PrefabCategory))
                            {
                                errors.Add($"Flow '{flow.Name}' argument '{arg.Name}' is Prefab but has no PrefabCategory");
                            }
                        }
                    }
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
            sb.AppendLine("=== Gameplay Flow Registry ===");
            sb.AppendLine($"Total Flows: {FlowsByName.Count}");
            sb.AppendLine();

            // Summary by gameplay type
            sb.AppendLine("Flows by Gameplay Type:");
            foreach (var group in FlowsByName.Values.GroupBy(f => f.GameplayType).OrderBy(g => g.Key))
            {
                var adminOnly = group.Count(f => f.AdminOnly);
                sb.AppendLine($"  {group.Key}: {group.Count()} flows ({adminOnly} admin-only)");
            }

            return sb.ToString();
        }

        private static void AddToGameplayIndex(FlowDefinition flow, bool replace)
        {
            var gameType = flow.GameplayType;
            
            if (!FlowsByGameplay.TryGetValue(gameType, out var flows))
            {
                flows = new List<FlowDefinition>();
                FlowsByGameplay[gameType] = flows;
            }

            if (replace)
            {
                flows.RemoveAll(f => f.Name == flow.Name);
            }

            if (!flows.Any(f => f.Name == flow.Name))
            {
                flows.Add(flow);
            }
        }

        private static FlowDefinition ConvertLegacyDefinition(GameplayFlowDefinition legacy)
        {
            return new FlowDefinition
            {
                Name = legacy.Name,
                GameplayType = legacy.GameplayType,
                Description = legacy.Description,
                AdminOnly = legacy.AdminOnly,
                EnabledByDefault = legacy.EnabledByDefault,
                SupportedZoneTypes = legacy.SupportedZoneTypes,
                Tags = legacy.Tags,
                Arguments = legacy.Arguments
            };
        }
    }
}
