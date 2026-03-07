using System;
using System.Collections.Generic;
using System.Linq;
using VAutomationCore.Core.Gameplay;
using VAutomationCore.Core.Gameplay.Shared.Contracts;
using VAutomationCore.Core.Gameplay.Shared.Types;

namespace VAutomationCore.Core.Gameplay.Boss
{
    /// <summary>
    /// Boss gameplay module - implements IGameplayModule.
    /// Second gameplay module following the same structure as Arena.
    /// </summary>
    public sealed class BossGameplayModule : IGameplayModule, IFlowArgValidator
    {
        private const string ModuleIdValue = "vautomationcore.boss";
        
        private bool _isEnabled;
        private readonly List<string> _errors = new();
        private readonly List<string> _warnings = new();

        /// <summary>
        /// Get the singleton instance.
        /// </summary>
        public static BossGameplayModule Instance { get; } = new();

        #region IGameplayModule Implementation

        public string ModuleId => ModuleIdValue;

        public GameplayType GameplayType => GameplayType.Boss;

        public string DisplayName => "Boss";

        public bool IsEnabled => _isEnabled;

        public IReadOnlyList<FlowDefinition> GetFlowDefinitions()
        {
            return new List<FlowDefinition>
            {
                // Boss Spawning
                CreateFlow("boss_spawn", "Spawn a boss", true, new[] { "boss" }),
                CreateFlow("boss_despawn", "Despawn a boss", true, new[] { "boss" }),
                CreateFlow("boss_list", "List active bosses", false, Array.Empty<string>()),
                
                // Boss Lifecycle
                CreateFlow("boss_enable", "Enable boss spawns", true, new[] { "zone" }),
                CreateFlow("boss_disable", "Disable boss spawns", true, new[] { "zone" }),
                
                // Boss Tracking
                CreateFlow("boss_health_get", "Get boss health", false, new[] { "boss" }),
                CreateFlow("boss_status", "Get boss status", false, new[] { "boss" }),
                
                // Rewards
                CreateFlow("boss_reward_configure", "Configure boss rewards", true, new[] { "boss", "reward_table" }),
                CreateFlow("boss_loot_table_set", "Set boss loot table", true, new[] { "boss", "loot_table" })
            };
        }

        public IReadOnlyList<ZoneDefinition> GetZoneDefinitions()
        {
            // Boss zones would be defined here
            return new List<ZoneDefinition>
            {
                new ZoneDefinition
                {
                    ZoneId = "boss_lair_default",
                    GameplayType = GameplayType.Boss,
                    ZoneType = "boss_lair",
                    ZoneShape = "Sphere",
                    Enabled = false,
                    DisplayName = "Default Boss Lair",
                    Description = "Default boss lair zone"
                }
            };
        }

        public IReadOnlyDictionary<string, EntityRoleDefinition> GetEntityRoles()
        {
            return new Dictionary<string, EntityRoleDefinition>
            {
                ["BossEntity"] = new EntityRoleDefinition
                {
                    RoleName = "BossEntity",
                    Description = "A boss entity",
                    IsPlayer = false,
                    IsTarget = true,
                    IsObjective = true
                },
                ["BossMinion"] = new EntityRoleDefinition
                {
                    RoleName = "BossMinion",
                    Description = "A minion spawned by a boss",
                    IsPlayer = false,
                    IsTarget = true,
                    IsObjective = false
                }
            };
        }

        public IReadOnlyDictionary<string, PrefabCategoryDefinition> GetPrefabCategories()
        {
            return new Dictionary<string, PrefabCategoryDefinition>
            {
                ["Boss"] = new PrefabCategoryDefinition
                {
                    CategoryName = "Boss",
                    Description = "Boss prefabs",
                    AllowedPrefabGuids = new[] { "boss_spawner", "boss_throne" }
                },
                ["Rewards"] = new PrefabCategoryDefinition
                {
                    CategoryName = "Rewards",
                    Description = "Boss reward prefabs",
                    AllowedPrefabGuids = new[] { "reward_chest", "loot_drop" }
                }
            };
        }

        public IReadOnlyDictionary<string, Type> GetSettingsPayloadTypes()
        {
            return new Dictionary<string, Type>
            {
                ["BossZoneSettings"] = typeof(BossZoneSettings),
                ["BossSpawnSettings"] = typeof(BossSpawnSettings)
            };
        }

        public IReadOnlyDictionary<string, Type> GetRuleProfileTypes()
        {
            return new Dictionary<string, Type>
            {
                ["BossRuleProfile"] = typeof(BossRuleProfile)
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

            // Validate zone ownership
            foreach (var zone in GetZoneDefinitions())
            {
                if (zone.GameplayType != GameplayType.Boss)
                {
                    _errors.Add($"Zone '{zone.ZoneId}' is in Boss module but has wrong GameplayType");
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

        public GameplayType GameplayTypeForValidator => GameplayType.Boss;

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
                GameplayType = GameplayType.Boss,
                Description = description,
                AdminOnly = adminOnly,
                SupportedZoneTypes = supportedZoneTypes,
                Tags = new[] { "boss" },
                Arguments = Array.Empty<FlowArgDefinition>()
            };
        }
    }

    #region Boss Data Types

    public sealed class BossZoneSettings : ISettingsPayload
    {
        public string SettingsId { get; init; } = "default";
        public string DisplayName { get; init; } = "Default Boss Zone";
        public bool IsEnabled { get; init; } = true;
        public bool RespawnEnabled { get; init; } = true;
        public float RespawnTimerHours { get; init; } = 24f;

        public IReadOnlyDictionary<string, string> GetSettings() => new Dictionary<string, string>
        {
            ["RespawnEnabled"] = RespawnEnabled.ToString(),
            ["RespawnTimerHours"] = RespawnTimerHours.ToString()
        };

        public IReadOnlyList<string> Validate() => new List<string>();
    }

    public sealed class BossSpawnSettings : ISettingsPayload
    {
        public string SettingsId { get; init; } = "default";
        public string DisplayName { get; init; } = "Default Boss Spawn";
        public bool IsEnabled { get; init; } = true;
        public string BossPrefab { get; init; } = string.Empty;
        public int MinionCount { get; init; } = 5;

        public IReadOnlyDictionary<string, string> GetSettings() => new Dictionary<string, string>
        {
            ["BossPrefab"] = BossPrefab,
            ["MinionCount"] = MinionCount.ToString()
        };

        public IReadOnlyList<string> Validate() => new List<string>();
    }

    public sealed class BossRuleProfile : IRuleProfile
    {
        public string ProfileId { get; init; } = "default";
        public string DisplayName { get; init; } = "Default Boss Rules";
        public string Description { get; init; } = "Standard boss encounter rules";
        public bool PvpEnabled { get; init; } = false;
        public bool FriendlyFireEnabled { get; init; } = false;
        public bool MountsAllowed { get; init; } = false;
        public bool SpectatorsAllowed { get; init; } = true;

        public IReadOnlyDictionary<string, string> GetRules() => new Dictionary<string, string>
        {
            ["PvpEnabled"] = PvpEnabled.ToString(),
            ["SpectatorsAllowed"] = SpectatorsAllowed.ToString()
        };

        public IReadOnlyList<string> Validate() => new List<string>();
    }

    #endregion
}
