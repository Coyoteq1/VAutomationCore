using System;
using System.Collections.Generic;
using System.Linq;
using VAutomationCore.Core.Gameplay;
using VAutomationCore.Core.Gameplay.Shared.Contracts;
using VAutomationCore.Core.Gameplay.Shared.Types;

namespace VAutomationCore.Core.Gameplay.Harvest
{
    /// <summary>
    /// Harvest gameplay module - implements IGameplayModule.
    /// Third gameplay module following the same structure as Arena and Boss.
    /// </summary>
    public sealed class HarvestGameplayModule : IGameplayModule, IFlowArgValidator
    {
        private const string ModuleIdValue = "vautomationcore.harvest";
        
        private bool _isEnabled;
        private readonly List<string> _errors = new();
        private readonly List<string> _warnings = new();

        /// <summary>
        /// Get the singleton instance.
        /// </summary>
        public static HarvestGameplayModule Instance { get; } = new();

        #region IGameplayModule Implementation

        public string ModuleId => ModuleIdValue;

        public GameplayType GameplayType => GameplayType.Harvest;

        public string DisplayName => "Harvest";

        public bool IsEnabled => _isEnabled;

        public IReadOnlyList<FlowDefinition> GetFlowDefinitions()
        {
            return new List<FlowDefinition>
            {
                // Harvest Zone Management
                CreateFlow("harvest_zone_create", "Create harvest zone", true, new[] { "harvest_zone" }),
                CreateFlow("harvest_zone_delete", "Delete harvest zone", true, new[] { "harvest_zone" }),
                CreateFlow("harvest_zone_enable", "Enable harvest zone", true, new[] { "harvest_zone" }),
                CreateFlow("harvest_zone_disable", "Disable harvest zone", true, new[] { "harvest_zone" }),
                CreateFlow("harvest_zone_list", "List harvest zones", false, Array.Empty<string>()),
                
                // Resource Management
                CreateFlow("harvest_resource_add", "Add resource to zone", true, new[] { "harvest_zone", "resource" }),
                CreateFlow("harvest_resource_remove", "Remove resource from zone", true, new[] { "harvest_zone", "resource" }),
                CreateFlow("harvest_resource_spawn", "Spawn resources in zone", true, new[] { "harvest_zone" }),
                CreateFlow("harvest_resource_clear", "Clear resources from zone", true, new[] { "harvest_zone" }),
                
                // Settings
                CreateFlow("harvest_settings_apply", "Apply harvest settings", true, new[] { "harvest_zone", "settings" }),
                CreateFlow("harvest_respawn_set", "Set resource respawn time", true, new[] { "harvest_zone", "respawn_timer" })
            };
        }

        public IReadOnlyList<ZoneDefinition> GetZoneDefinitions()
        {
            return new List<ZoneDefinition>
            {
                new ZoneDefinition
                {
                    ZoneId = "harvest_zone_default",
                    GameplayType = GameplayType.Harvest,
                    ZoneType = "harvest_zone",
                    ZoneShape = "Sphere",
                    Enabled = false,
                    DisplayName = "Default Harvest Zone",
                    Description = "Default resource harvest zone"
                }
            };
        }

        public IReadOnlyDictionary<string, EntityRoleDefinition> GetEntityRoles()
        {
            return new Dictionary<string, EntityRoleDefinition>
            {
                ["HarvestableResource"] = new EntityRoleDefinition
                {
                    RoleName = "HarvestableResource",
                    Description = "A harvestable resource entity",
                    IsPlayer = false,
                    IsTarget = false,
                    IsObjective = true
                },
                ["ResourceNode"] = new EntityRoleDefinition
                {
                    RoleName = "ResourceNode",
                    Description = "A resource node",
                    IsPlayer = false,
                    IsTarget = false,
                    IsObjective = true
                }
            };
        }

        public IReadOnlyDictionary<string, PrefabCategoryDefinition> GetPrefabCategories()
        {
            return new Dictionary<string, PrefabCategoryDefinition>
            {
                ["Resources"] = new PrefabCategoryDefinition
                {
                    CategoryName = "Resources",
                    Description = "Harvestable resource prefabs",
                    AllowedPrefabGuids = new[] { "wood_node", "ore_node", "herb_node" }
                },
                ["Containers"] = new PrefabCategoryDefinition
                {
                    CategoryName = "Containers",
                    Description = "Resource container prefabs",
                    AllowedPrefabGuids = new[] { "crate", "barrel", "chest" }
                }
            };
        }

        public IReadOnlyDictionary<string, Type> GetSettingsPayloadTypes()
        {
            return new Dictionary<string, Type>
            {
                ["HarvestZoneSettings"] = typeof(HarvestZoneSettings),
                ["HarvestResourceSettings"] = typeof(HarvestResourceSettings)
            };
        }

        public IReadOnlyDictionary<string, Type> GetRuleProfileTypes()
        {
            return new Dictionary<string, Type>
            {
                ["HarvestRuleProfile"] = typeof(HarvestRuleProfile)
            };
        }

        public void OnEnable()
        {
            if (_isEnabled) return;
            _isEnabled = true;
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

            foreach (var zone in GetZoneDefinitions())
            {
                if (zone.GameplayType != GameplayType.Harvest)
                {
                    _errors.Add($"Zone '{zone.ZoneId}' is in Harvest module but has wrong GameplayType");
                }
            }

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

        public GameplayType GameplayTypeForValidator => GameplayType.Harvest;

        public FlowArgValidationResult Validate(FlowArgDefinition argDefinition, object? value)
        {
            if (argDefinition == null)
            {
                return FlowArgValidationResult.Error("Argument definition is null");
            }

            if (argDefinition.Required && value == null)
            {
                return FlowArgValidationResult.Error($"Required argument '{argDefinition.Name}' is missing");
            }

            return FlowArgValidationResult.Success();
        }

        public bool ValidatePayloadType(string payloadTypeName, FlowArgKind argKind)
        {
            var settingsTypes = GetSettingsPayloadTypes();
            var ruleTypes = GetRuleProfileTypes();

            if (argKind == FlowArgKind.Settings)
                return settingsTypes.ContainsKey(payloadTypeName);

            if (argKind == FlowArgKind.RuleProfile)
                return ruleTypes.ContainsKey(payloadTypeName);

            return false;
        }

        public bool ValidatePrefabCategory(string prefabCategory)
        {
            return GetPrefabCategories().ContainsKey(prefabCategory);
        }

        public bool ValidateEntityRole(string entityRoleName)
        {
            return GetEntityRoles().ContainsKey(entityRoleName);
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

        private static FlowDefinition CreateFlow(string name, string description, bool adminOnly, string[] supportedZoneTypes)
        {
            return new FlowDefinition
            {
                Name = name,
                GameplayType = GameplayType.Harvest,
                Description = description,
                AdminOnly = adminOnly,
                SupportedZoneTypes = supportedZoneTypes,
                Tags = new[] { "harvest" },
                Arguments = Array.Empty<FlowArgDefinition>()
            };
        }
    }

    #region Harvest Data Types

    public sealed class HarvestZoneSettings : ISettingsPayload
    {
        public string SettingsId { get; init; } = "default";
        public string DisplayName { get; init; } = "Default Harvest Zone";
        public bool IsEnabled { get; init; } = true;
        public bool AutoRespawn { get; init; } = true;
        public float RespawnHours { get; init; } = 1f;

        public IReadOnlyDictionary<string, string> GetSettings() => new Dictionary<string, string>
        {
            ["AutoRespawn"] = AutoRespawn.ToString(),
            ["RespawnHours"] = RespawnHours.ToString()
        };

        public IReadOnlyList<string> Validate() => new List<string>();
    }

    public sealed class HarvestResourceSettings : ISettingsPayload
    {
        public string SettingsId { get; init; } = "default";
        public string DisplayName { get; init; } = "Default Resource Settings";
        public bool IsEnabled { get; init; } = true;
        public int ResourceCapacity { get; init; } = 100;

        public IReadOnlyDictionary<string, string> GetSettings() => new Dictionary<string, string>
        {
            ["ResourceCapacity"] = ResourceCapacity.ToString()
        };

        public IReadOnlyList<string> Validate() => new List<string>();
    }

    public sealed class HarvestRuleProfile : IRuleProfile
    {
        public string ProfileId { get; init; } = "default";
        public string DisplayName { get; init; } = "Default Harvest Rules";
        public string Description { get; init; } = "Standard harvest zone rules";
        public bool PvpEnabled { get; init; } = false;
        public bool FriendlyFireEnabled { get; init; } = false;
        public bool MountsAllowed { get; init; } = true;
        public bool SpectatorsAllowed { get; init; } = true;

        public IReadOnlyDictionary<string, string> GetRules() => new Dictionary<string, string>
        {
            ["PvpEnabled"] = PvpEnabled.ToString(),
            ["MountsAllowed"] = MountsAllowed.ToString()
        };

        public IReadOnlyList<string> Validate() => new List<string>();
    }

    #endregion
}
