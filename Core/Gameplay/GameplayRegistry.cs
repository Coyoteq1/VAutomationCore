using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VAutomationCore.Core.Gameplay.Shared.Contracts;

namespace VAutomationCore.Core.Gameplay
{
    /// <summary>
    /// Registry for managing gameplay modules.
    /// Supports both legacy GameplayDefinition registration and new IGameplayModule registration.
    /// </summary>
    public static class GameplayRegistry
    {
        private static readonly ConcurrentDictionary<string, GameplayDefinition> GameplayById = new(StringComparer.OrdinalIgnoreCase);
        private static readonly ConcurrentDictionary<GameplayType, IGameplayModule> ModulesByType = new();
        private static readonly ConcurrentDictionary<string, IGameplayModule> ModulesById = new(StringComparer.OrdinalIgnoreCase);
        
        private static bool _isInitialized = false;

        public static void Initialize()
        {
            GameplayById.Clear();
            ModulesByType.Clear();
            ModulesById.Clear();
            _isInitialized = false;
        }

        /// <summary>
        /// Register a gameplay module using the new IGameplayModule interface.
        /// This is the preferred method for new gameplay modules.
        /// </summary>
        public static bool RegisterModule(IGameplayModule module, bool replace = false)
        {
            if (module == null || string.IsNullOrWhiteSpace(module.ModuleId))
            {
                return false;
            }

            var moduleId = module.ModuleId.Trim();
            var gameType = module.GameplayType;

            // Register by module ID
            if (!replace && ModulesById.ContainsKey(moduleId))
            {
                return false;
            }
            ModulesById[moduleId] = module;

            // Register by gameplay type
            if (!replace && ModulesByType.ContainsKey(gameType))
            {
                return false;
            }
            ModulesByType[gameType] = module;

            // Also create a legacy GameplayDefinition for backward compatibility
            var legacyDef = CreateLegacyDefinition(module);
            GameplayById[moduleId] = legacyDef;

            // Call OnEnable if the module should be enabled
            if (module.IsEnabled)
            {
                module.OnEnable();
            }

            return true;
        }

        /// <summary>
        /// Register using legacy GameplayDefinition (for backward compatibility).
        /// </summary>
        public static bool Register(GameplayDefinition definition, bool replace = false)
        {
            if (definition == null || string.IsNullOrWhiteSpace(definition.Id))
            {
                return false;
            }

            var key = definition.Id.Trim();
            if (replace)
            {
                GameplayById[key] = definition;
                return true;
            }

            return GameplayById.TryAdd(key, definition);
        }

        /// <summary>
        /// Get a module by its gameplay type.
        /// </summary>
        public static IGameplayModule? GetModule(GameplayType gameType)
        {
            return ModulesByType.TryGetValue(gameType, out var module) ? module : null;
        }

        /// <summary>
        /// Get a module by its module ID.
        /// </summary>
        public static IGameplayModule? GetModule(string moduleId)
        {
            return ModulesById.TryGetValue(moduleId, out var module) ? module : null;
        }

        /// <summary>
        /// Get all registered modules.
        /// </summary>
        public static IReadOnlyCollection<IGameplayModule> GetAllModules()
        {
            return ModulesByType.Values.ToList();
        }

        /// <summary>
        /// Get the validator for a specific gameplay type.
        /// </summary>
        public static IFlowArgValidator? GetValidator(GameplayType gameType)
        {
            return GetModule(gameType) as IFlowArgValidator;
        }

        /// <summary>
        /// Validate all registered modules.
        /// </summary>
        public static IReadOnlyList<ModuleValidationResult> ValidateAllModules()
        {
            var results = new List<ModuleValidationResult>();
            
            foreach (var module in ModulesByType.Values)
            {
                var validationResult = module.GetValidationResult();
                results.Add(validationResult);
            }

            return results;
        }

        /// <summary>
        /// Enable all modules.
        /// </summary>
        public static void EnableAllModules()
        {
            foreach (var module in ModulesByType.Values)
            {
                if (!module.IsEnabled)
                {
                    module.OnEnable();
                }
            }
            _isInitialized = true;
        }

        /// <summary>
        /// Disable all modules.
        /// </summary>
        public static void DisableAllModules()
        {
            foreach (var module in ModulesByType.Values)
            {
                if (module.IsEnabled)
                {
                    module.OnDisable();
                }
            }
        }

        /// <summary>
        /// Check if initialization is complete.
        /// </summary>
        public static bool IsInitialized => _isInitialized;

        /// <summary>
        /// Get comprehensive summary of all modules.
        /// </summary>
        public static string GetSummary()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Gameplay Module Registry ===");
            sb.AppendLine($"Total Modules: {ModulesByType.Count}");
            sb.AppendLine($"Total Legacy Definitions: {GameplayById.Count}");
            sb.AppendLine();

            foreach (var module in ModulesByType.Values.OrderBy(m => m.GameplayType))
            {
                var validation = module.GetValidationResult();
                sb.AppendLine($"Module: {module.DisplayName} ({module.ModuleId})");
                sb.AppendLine($"  Type: {module.GameplayType}");
                sb.AppendLine($"  Enabled: {module.IsEnabled}");
                sb.AppendLine($"  Flows: {validation.FlowCount}");
                sb.AppendLine($"  Zones: {validation.ZoneCount}");
                sb.AppendLine($"  Entity Roles: {validation.EntityRoleCount}");
                sb.AppendLine($"  Prefab Categories: {validation.PrefabCategoryCount}");
                sb.AppendLine($"  Valid: {validation.IsValid}");
                
                if (validation.Errors.Count > 0)
                {
                    sb.AppendLine($"  Errors: {validation.Errors.Count}");
                    foreach (var error in validation.Errors.Take(5))
                    {
                        sb.AppendLine($"    - {error}");
                    }
                }
                
                if (validation.Warnings.Count > 0)
                {
                    sb.AppendLine($"  Warnings: {validation.Warnings.Count}");
                    foreach (var warning in validation.Warnings.Take(3))
                    {
                        sb.AppendLine($"    - {warning}");
                    }
                }
                sb.AppendLine();
            }

            // Legacy definitions summary
            if (GameplayById.Count > 0)
            {
                sb.AppendLine("=== Legacy Gameplay Definitions ===");
                foreach (var gameplay in GameplayById.Values.OrderBy(g => g.GameplayType).ThenBy(g => g.Id))
                {
                    sb.AppendLine($"- {gameplay.GameplayType} ({gameplay.Id}) enabled={gameplay.Enabled} zones={gameplay.ZoneTypes.Count} flows={gameplay.FlowNames.Count}");
                }
            }

            return sb.ToString();
        }

        private static GameplayDefinition CreateLegacyDefinition(IGameplayModule module)
        {
            var flowDefs = module.GetFlowDefinitions();
            var zoneDefs = module.GetZoneDefinitions();

            return new GameplayDefinition
            {
                Id = module.ModuleId,
                GameplayType = module.GameplayType,
                DisplayName = module.DisplayName,
                Enabled = module.IsEnabled,
                ConfigDirectory = $"config/gameplay/{module.GameplayType.ToString().ToLowerInvariant()}/",
                ZoneTypes = zoneDefs.Select(z => z.ZoneType).Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
                FlowNames = flowDefs.Select(f => f.Name).ToArray()
            };
        }
    }
}
