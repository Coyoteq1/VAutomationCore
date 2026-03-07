using System;
using System.Collections.Generic;
using System.Linq;
using VAutomationCore.Core.Gameplay;
using VAutomationCore.Core.Gameplay.Arena.Data;
using VAutomationCore.Core.Gameplay.Arena.Flows;
using VAutomationCore.Core.Gameplay.Arena.Zones;
using VAutomationCore.Core.Gameplay.Shared.Contracts;
using VAutomationCore.Core.Gameplay.Shared.Types;

namespace VAutomationCore.Core.Gameplay.Arena
{
    /// <summary>
    /// Arena gameplay module - implements IGameplayModule.
    /// This is the first fully isolated gameplay module and serves as the template.
    /// </summary>
    public sealed class ArenaGameplayModule : IGameplayModule, IFlowArgValidator
    {
        private const string ModuleIdValue = "vautomationcore.arena";
        
        private bool _isEnabled;
        private readonly List<string> _errors = new();
        private readonly List<string> _warnings = new();

        /// <summary>
        /// Get the singleton instance.
        /// </summary>
        public static ArenaGameplayModule Instance { get; } = new();

        #region IGameplayModule Implementation

        public string ModuleId => ModuleIdValue;

        public GameplayType GameplayType => GameplayType.Arena;

        public string DisplayName => "Arena";

        public bool IsEnabled => _isEnabled;

        public IReadOnlyList<FlowDefinition> GetFlowDefinitions()
        {
            var flows = new List<FlowDefinition>();

            // Aggregate all arena flows
            flows.AddRange(ArenaLifecycleFlows.GetFlowDefinitions());
            flows.AddRange(ArenaMatchFlows.GetFlowDefinitions());
            flows.AddRange(ArenaPlayerFlows.GetFlowDefinitions());
            flows.AddRange(ArenaRuleFlows.GetFlowDefinitions());
            flows.AddRange(ArenaRewardFlows.GetFlowDefinitions());
            flows.AddRange(ArenaAdminFlows.GetFlowDefinitions());
            flows.AddRange(ArenaMonitoringFlows.GetFlowDefinitions());

            return flows;
        }

        public IReadOnlyList<ZoneDefinition> GetZoneDefinitions()
        {
            return ArenaZones.GetAllZones()
                .Select(z => z.ToZoneDefinition())
                .ToList();
        }

        public IReadOnlyDictionary<string, EntityRoleDefinition> GetEntityRoles()
        {
            return ArenaEntityRoles.GetRoles();
        }

        public IReadOnlyDictionary<string, PrefabCategoryDefinition> GetPrefabCategories()
        {
            var categories = new Dictionary<string, PrefabCategoryDefinition>();
            var prefabSet = new ArenaPrefabSet();
            var rawCategories = prefabSet.GetPrefabCategories();

            foreach (var kvp in rawCategories)
            {
                categories[kvp.Key] = new PrefabCategoryDefinition
                {
                    CategoryName = kvp.Key,
                    Description = $"Arena {kvp.Key} prefabs",
                    AllowedPrefabGuids = kvp.Value
                };
            }

            return categories;
        }

        public IReadOnlyDictionary<string, Type> GetSettingsPayloadTypes()
        {
            return new Dictionary<string, Type>
            {
                ["ArenaZoneSettings"] = typeof(ArenaZoneSettings),
                ["ArenaMatchSettings"] = typeof(ArenaMatchSettings)
            };
        }

        public IReadOnlyDictionary<string, Type> GetRuleProfileTypes()
        {
            return new Dictionary<string, Type>
            {
                ["ArenaRuleProfile"] = typeof(ArenaRuleProfile)
            };
        }

        public void OnEnable()
        {
            if (_isEnabled) return;

            _isEnabled = true;
            ArenaZones.Initialize();
        }

        public void OnDisable()
        {
            if (!_isEnabled) return;

            _isEnabled = false;
        }

        public IReadOnlyList<string> Validate()
        {
            _errors.Clear();
            _warnings.Clear();

            // Validate zone definitions
            var zoneErrors = ArenaZones.Validate();
            _errors.AddRange(zoneErrors);

            // Validate flow argument references
            var flowErrors = ValidateFlowArgumentReferences();
            _errors.AddRange(flowErrors);

            // Validate zone ownership
            var zoneOwnershipErrors = ValidateZoneOwnership();
            _errors.AddRange(zoneOwnershipErrors);

            return _errors;
        }

        public ModuleValidationResult GetValidationResult()
        {
            Validate();

            var flows = GetFlowDefinitions();
            var zones = GetZoneDefinitions();
            var roles = GetEntityRoles();
            var prefabs = GetPrefabCategories();

            return new ModuleValidationResult
            {
                IsValid = _errors.Count == 0,
                Errors = _errors,
                Warnings = _warnings,
                FlowCount = flows.Count,
                ZoneCount = zones.Count,
                EntityRoleCount = roles.Count,
                PrefabCategoryCount = prefabs.Count
            };
        }

        #endregion

        #region IFlowArgValidator Implementation

        public GameplayType GameplayTypeForValidator => GameplayType.Arena;

        public FlowArgValidationResult Validate(FlowArgDefinition argDefinition, object? value)
        {
            if (argDefinition == null)
            {
                return FlowArgValidationResult.Error("Argument definition is null");
            }

            // Check required
            if (argDefinition.Required && value == null)
            {
                return FlowArgValidationResult.Error($"Required argument '{argDefinition.Name}' is missing");
            }

            // Skip further validation if value is null and not required
            if (value == null)
            {
                return FlowArgValidationResult.Success();
            }

            // Validate payload type references
            if (argDefinition.Kind == FlowArgKind.Settings || argDefinition.Kind == FlowArgKind.RuleProfile)
            {
                if (!string.IsNullOrEmpty(argDefinition.PayloadTypeName))
                {
                    if (!ValidatePayloadType(argDefinition.PayloadTypeName, argDefinition.Kind))
                    {
                        return FlowArgValidationResult.Error(
                            $"Invalid payload type '{argDefinition.PayloadTypeName}' for argument '{argDefinition.Name}'");
                    }
                }
            }

            // Validate entity role references
            if (argDefinition.Kind == FlowArgKind.Entity || 
                argDefinition.Kind == FlowArgKind.Player || 
                argDefinition.Kind == FlowArgKind.TargetEntity)
            {
                if (!string.IsNullOrEmpty(argDefinition.EntityRoleName))
                {
                    if (!ValidateEntityRole(argDefinition.EntityRoleName))
                    {
                        return FlowArgValidationResult.Error(
                            $"Invalid entity role '{argDefinition.EntityRoleName}' for argument '{argDefinition.Name}'");
                    }
                }
            }

            // Validate prefab category references
            if (argDefinition.Kind == FlowArgKind.Prefab)
            {
                if (!string.IsNullOrEmpty(argDefinition.PrefabCategory))
                {
                    if (!ValidatePrefabCategory(argDefinition.PrefabCategory))
                    {
                        return FlowArgValidationResult.Error(
                            $"Invalid prefab category '{argDefinition.PrefabCategory}' for argument '{argDefinition.Name}'");
                    }
                }
            }

            return FlowArgValidationResult.Success();
        }

        public bool ValidatePayloadType(string payloadTypeName, FlowArgKind argKind)
        {
            var settingsTypes = GetSettingsPayloadTypes();
            var ruleTypes = GetRuleProfileTypes();

            if (argKind == FlowArgKind.Settings)
            {
                return settingsTypes.ContainsKey(payloadTypeName);
            }

            if (argKind == FlowArgKind.RuleProfile)
            {
                return ruleTypes.ContainsKey(payloadTypeName);
            }

            return false;
        }

        public bool ValidatePrefabCategory(string prefabCategory)
        {
            var categories = GetPrefabCategories();
            return categories.ContainsKey(prefabCategory);
        }

        public bool ValidateEntityRole(string entityRoleName)
        {
            var roles = GetEntityRoles();
            return roles.ContainsKey(entityRoleName);
        }

        public IReadOnlyList<string> GetSupportedPayloadTypes()
        {
            var types = new List<string>();
            types.AddRange(GetSettingsPayloadTypes().Keys);
            types.AddRange(GetRuleProfileTypes().Keys);
            return types;
        }

        public IReadOnlyList<string> GetSupportedEntityRoles()
        {
            return GetEntityRoles().Keys.ToList();
        }

        public IReadOnlyList<string> GetSupportedPrefabCategories()
        {
            return GetPrefabCategories().Keys.ToList();
        }

        #endregion

        #region Validation Helpers

        private List<string> ValidateFlowArgumentReferences()
        {
            var errors = new List<string>();

            foreach (var flow in GetFlowDefinitions())
            {
                if (flow.Arguments == null) continue;

                foreach (var arg in flow.Arguments)
                {
                    // Check payload type
                    if ((arg.Kind == FlowArgKind.Settings || arg.Kind == FlowArgKind.RuleProfile) 
                        && !string.IsNullOrEmpty(arg.PayloadTypeName))
                    {
                        if (!ValidatePayloadType(arg.PayloadTypeName, arg.Kind))
                        {
                            errors.Add($"Flow '{flow.Name}': Invalid PayloadTypeName '{arg.PayloadTypeName}'");
                        }
                    }

                    // Check entity role
                    if ((arg.Kind == FlowArgKind.Entity || arg.Kind == FlowArgKind.Player || arg.Kind == FlowArgKind.TargetEntity)
                        && !string.IsNullOrEmpty(arg.EntityRoleName))
                    {
                        if (!ValidateEntityRole(arg.EntityRoleName))
                        {
                            errors.Add($"Flow '{flow.Name}': Invalid EntityRoleName '{arg.EntityRoleName}'");
                        }
                    }

                    // Check prefab category
                    if (arg.Kind == FlowArgKind.Prefab && !string.IsNullOrEmpty(arg.PrefabCategory))
                    {
                        if (!ValidatePrefabCategory(arg.PrefabCategory))
                        {
                            errors.Add($"Flow '{flow.Name}': Invalid PrefabCategory '{arg.PrefabCategory}'");
                        }
                    }
                }
            }

            return errors;
        }

        private List<string> ValidateZoneOwnership()
        {
            var errors = new List<string>();

            foreach (var zone in GetZoneDefinitions())
            {
                if (zone.GameplayType != GameplayType.Arena)
                {
                    errors.Add($"Zone '{zone.ZoneId}' is in Arena module but has wrong GameplayType");
                }
            }

            return errors;
        }

        #endregion
    }
}
