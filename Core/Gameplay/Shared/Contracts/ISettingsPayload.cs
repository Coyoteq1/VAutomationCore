using System;
using System.Collections.Generic;

namespace VAutomationCore.Core.Gameplay.Shared.Contracts
{
    /// <summary>
    /// Base interface for all gameplay-specific settings payloads.
    /// Each gameplay module defines its own settings classes that implement this interface.
    /// </summary>
    public interface ISettingsPayload
    {
        /// <summary>
        /// Unique identifier for this settings profile.
        /// </summary>
        string SettingsId { get; }

        /// <summary>
        /// Display name for this settings profile.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Whether this settings profile is enabled by default.
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// Get all setting key-value pairs for serialization.
        /// </summary>
        IReadOnlyDictionary<string, string> GetSettings();

        /// <summary>
        /// Validate the settings configuration.
        /// </summary>
        /// <returns>Validation errors, if any.</returns>
        IReadOnlyList<string> Validate();
    }

    /// <summary>
    /// Base interface for all gameplay-specific rule profiles.
    /// Each gameplay module defines its own rule profile classes.
    /// </summary>
    public interface IRuleProfile
    {
        /// <summary>
        /// Unique identifier for this rule profile.
        /// </summary>
        string ProfileId { get; }

        /// <summary>
        /// Display name for this rule profile.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Description of what this rule profile controls.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Whether PvP rules are enabled in this profile.
        /// </summary>
        bool PvpEnabled { get; }

        /// <summary>
        /// Whether friendly fire is allowed.
        /// </summary>
        bool FriendlyFireEnabled { get; }

        /// <summary>
        /// Whether mounts are allowed.
        /// </summary>
        bool MountsAllowed { get; }

        /// <summary>
        /// Whether spectators are allowed.
        /// </summary>
        bool SpectatorsAllowed { get; }

        /// <summary>
        /// Get all rule key-value pairs.
        /// </summary>
        IReadOnlyDictionary<string, string> GetRules();

        /// <summary>
        /// Validate the rule profile configuration.
        /// </summary>
        /// <returns>Validation errors, if any.</returns>
        IReadOnlyList<string> Validate();
    }

    /// <summary>
    /// Marker attribute for settings payload types.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class SettingsPayloadAttribute : Attribute
    {
        public string SettingsTypeName { get; }
        public string DisplayName { get; }
        public string Description { get; }

        public SettingsPayloadAttribute(string settingsTypeName, string displayName, string description = "")
        {
            SettingsTypeName = settingsTypeName;
            DisplayName = displayName;
            Description = description;
        }
    }

    /// <summary>
    /// Marker attribute for rule profile types.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class RuleProfileAttribute : Attribute
    {
        public string ProfileTypeName { get; }
        public string DisplayName { get; }
        public string Description { get; }

        public RuleProfileAttribute(string profileTypeName, string displayName, string description = "")
        {
            ProfileTypeName = profileTypeName;
            DisplayName = displayName;
            Description = description;
        }
    }
}
