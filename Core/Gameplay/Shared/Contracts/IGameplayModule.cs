using System;
using System.Collections.Generic;
using VAutomationCore.Core.Gameplay;
using VAutomationCore.Core.Gameplay.Shared.Types;

namespace VAutomationCore.Core.Gameplay.Shared.Contracts
{
    /// <summary>
    /// Base interface for all gameplay modules.
    /// Each gameplay type (Arena, Boss, Harvest, etc.) implements this interface
    /// to provide its own isolated data, flows, and zone management.
    /// </summary>
    public interface IGameplayModule
    {
        /// <summary>
        /// Unique identifier for this gameplay module.
        /// </summary>
        string ModuleId { get; }

        /// <summary>
        /// The gameplay type this module implements.
        /// </summary>
        GameplayType GameplayType { get; }

        /// <summary>
        /// Display name for UI purposes.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Whether this module is currently enabled.
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// Get all flow definitions owned by this module.
        /// </summary>
        IReadOnlyList<FlowDefinition> GetFlowDefinitions();

        /// <summary>
        /// Get all zone definitions owned by this module.
        /// </summary>
        IReadOnlyList<ZoneDefinition> GetZoneDefinitions();

        /// <summary>
        /// Get all entity role definitions for this gameplay.
        /// </summary>
        IReadOnlyDictionary<string, EntityRoleDefinition> GetEntityRoles();

        /// <summary>
        /// Get all prefab categories for this gameplay.
        /// </summary>
        IReadOnlyDictionary<string, PrefabCategoryDefinition> GetPrefabCategories();

        /// <summary>
        /// Get all settings payload types for this gameplay.
        /// </summary>
        IReadOnlyDictionary<string, Type> GetSettingsPayloadTypes();

        /// <summary>
        /// Get all rule profile types for this gameplay.
        /// </summary>
        IReadOnlyDictionary<string, Type> GetRuleProfileTypes();

        /// <summary>
        /// Called when the module is enabled.
        /// </summary>
        void OnEnable();

        /// <summary>
        /// Called when the module is disabled.
        /// </summary>
        void OnDisable();

        /// <summary>
        /// Validate the module configuration.
        /// Returns validation errors, if any.
        /// </summary>
        IReadOnlyList<string> Validate();

        /// <summary>
        /// Get the validation summary for this module.
        /// </summary>
        ModuleValidationResult GetValidationResult();
    }

    /// <summary>
    /// Result of module validation.
    /// </summary>
    public sealed class ModuleValidationResult
    {
        public bool IsValid { get; init; }
        public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();
        public IReadOnlyList<string> Warnings { get; init; } = Array.Empty<string>();
        public int FlowCount { get; init; }
        public int ZoneCount { get; init; }
        public int EntityRoleCount { get; init; }
        public int PrefabCategoryCount { get; init; }
    }

    /// <summary>
    /// Defines an entity role for a specific gameplay type.
    /// </summary>
    public sealed class EntityRoleDefinition
    {
        public string RoleName { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public bool IsPlayer { get; init; }
        public bool IsTarget { get; init; }
        public bool IsObjective { get; init; }
    }

    /// <summary>
    /// Defines a prefab category for a specific gameplay type.
    /// </summary>
    public sealed class PrefabCategoryDefinition
    {
        public string CategoryName { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public IReadOnlyList<string> AllowedPrefabGuids { get; init; } = Array.Empty<string>();
    }
}
