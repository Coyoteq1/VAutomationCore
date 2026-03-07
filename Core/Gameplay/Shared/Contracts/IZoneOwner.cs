using System;
using VAutomationCore.Core.Gameplay;
using VAutomationCore.Core.Gameplay.Shared.Types;

namespace VAutomationCore.Core.Gameplay.Shared.Contracts
{
    /// <summary>
    /// Interface for zones that are owned by a specific gameplay module.
    /// Each zone must be associated with exactly one gameplay type.
    /// </summary>
    public interface IZoneOwner
    {
        /// <summary>
        /// The zone ID.
        /// </summary>
        string ZoneId { get; }

        /// <summary>
        /// The gameplay type that owns this zone.
        /// </summary>
        GameplayType OwningGameplayType { get; }

        /// <summary>
        /// The type of zone (e.g., "arena", "boss_lair", "harvest_zone").
        /// </summary>
        string ZoneType { get; }

        /// <summary>
        /// Whether this zone is currently enabled.
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// Create this zone.
        /// </summary>
        bool Create();

        /// <summary>
        /// Delete this zone.
        /// </summary>
        bool Delete();

        /// <summary>
        /// Enable this zone.
        /// </summary>
        bool Enable();

        /// <summary>
        /// Disable this zone.
        /// </summary>
        bool Disable();

        /// <summary>
        /// Bind a flow to this zone.
        /// </summary>
        bool BindFlow(string flowName, ZoneEventTrigger trigger);

        /// <summary>
        /// Unbind a flow from this zone.
        /// </summary>
        bool UnbindFlow(string flowName, ZoneEventTrigger trigger);

        /// <summary>
        /// Get current zone status.
        /// </summary>
        ZoneStatus GetStatus();

        /// <summary>
        /// Get list of all zones for this owner.
        /// </summary>
        IReadOnlyList<string> ListZones();
    }

    /// <summary>
    /// Zone status information.
    /// </summary>
    public sealed class ZoneStatus
    {
        public string ZoneId { get; init; } = string.Empty;
        public string ZoneType { get; init; } = string.Empty;
        public GameplayType OwnerType { get; init; }
        public bool IsEnabled { get; init; }
        public int PlayerCount { get; init; }
        public string MatchState { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
        public DateTime? LastMatchAt { get; init; }
    }
}
