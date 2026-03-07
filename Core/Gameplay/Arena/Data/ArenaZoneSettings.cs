using System;
using System.Collections.Generic;
using VAutomationCore.Core.Gameplay.Shared.Contracts;

namespace VAutomationCore.Core.Gameplay.Arena.Data
{
    /// <summary>
    /// Arena-specific zone settings.
    /// This type is owned by Arena module - not shared.
    /// </summary>
    [SettingsPayload("ArenaZoneSettings", "Arena Zone Settings", "Settings for arena zone behavior")]
    public sealed class ArenaZoneSettings : ISettingsPayload
    {
        public string SettingsId { get; init; } = "default";
        public string DisplayName { get; init; } = "Default Arena Zone";
        public bool IsEnabled { get; init; } = true;

        /// <summary>
        /// Whether PvP is enabled in this zone.
        /// </summary>
        public bool PvpEnabled { get; init; } = true;

        /// <summary>
        /// Whether friendly fire is allowed.
        /// </summary>
        public bool FriendlyFireEnabled { get; init; }

        /// <summary>
        /// Whether mounts are allowed.
        /// </summary>
        public bool MountsAllowed { get; init; }

        /// <summary>
        /// Whether spectators are allowed.
        /// </summary>
        public bool SpectatorsAllowed { get; init; } = true;

        /// <summary>
        /// Whether respawning is enabled.
        /// </summary>
        public bool RespawnEnabled { get; init; } = true;

        /// <summary>
        /// Respawn timer in seconds.
        /// </summary>
        public float RespawnTimerSeconds { get; init; } = 10f;

        /// <summary>
        /// Whether spells are allowed.
        /// </summary>
        public bool SpellsAllowed { get; init; } = true;

        /// <summary>
        /// Whether abilities are allowed.
        /// </summary>
        public bool AbilitiesAllowed { get; init; } = true;

        /// <summary>
        /// Zone boundary enforcement type.
        /// </summary>
        public string BoundaryType { get; init; } = "Soft"; // Soft, Hard, None

        /// <summary>
        /// What happens when player exits boundary.
        /// </summary>
        public string ExitAction { get; init; } = "TeleportBack"; // TeleportBack, Damage, Warning

        public IReadOnlyDictionary<string, string> GetSettings()
        {
            return new Dictionary<string, string>
            {
                ["PvpEnabled"] = PvpEnabled.ToString(),
                ["FriendlyFireEnabled"] = FriendlyFireEnabled.ToString(),
                ["MountsAllowed"] = MountsAllowed.ToString(),
                ["SpectatorsAllowed"] = SpectatorsAllowed.ToString(),
                ["RespawnEnabled"] = RespawnEnabled.ToString(),
                ["RespawnTimerSeconds"] = RespawnTimerSeconds.ToString(),
                ["SpellsAllowed"] = SpellsAllowed.ToString(),
                ["AbilitiesAllowed"] = AbilitiesAllowed.ToString(),
                ["BoundaryType"] = BoundaryType,
                ["ExitAction"] = ExitAction
            };
        }

        public IReadOnlyList<string> Validate()
        {
            var errors = new List<string>();

            if (RespawnTimerSeconds < 0)
            {
                errors.Add("RespawnTimerSeconds cannot be negative");
            }

            if (!Enum.TryParse<BoundaryType>(BoundaryType, true, out _))
            {
                errors.Add($"Invalid BoundaryType: {BoundaryType}");
            }

            return errors;
        }
    }

    public enum BoundaryType
    {
        Soft,
        Hard,
        None
    }

    public enum ExitActionType
    {
        TeleportBack,
        Damage,
        Warning
    }
}
