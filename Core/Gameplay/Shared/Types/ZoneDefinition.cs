using System;
using System.Collections.Generic;
using Unity.Mathematics;
using VAutomationCore.Core.Gameplay;

namespace VAutomationCore.Core.Gameplay.Shared.Types
{
    /// <summary>
    /// Shared zone definition used across all gameplay modules.
    /// This is the infrastructure-level type - gameplay-specific zone data stays in the gameplay module.
    /// </summary>
    public sealed class ZoneDefinition
    {
        /// <summary>
        /// Unique zone identifier.
        /// </summary>
        public string ZoneId { get; init; } = string.Empty;

        /// <summary>
        /// The gameplay type that owns this zone.
        /// </summary>
        public GameplayType GameplayType { get; init; }

        /// <summary>
        /// Zone type (e.g., "arena", "boss_lair", "harvest_zone").
        /// </summary>
        public string ZoneType { get; init; } = string.Empty;

        /// <summary>
        /// Zone shape (Sphere, Box, Cylinder).
        /// </summary>
        public string ZoneShape { get; init; } = "Sphere";

        /// <summary>
        /// Center position of the zone.
        /// </summary>
        public float3 Center { get; init; }

        /// <summary>
        /// Radius (for sphere) or dimensions (for box/cylinder).
        /// </summary>
        public float Radius { get; init; }

        /// <summary>
        /// Box dimensions when ZoneShape is Box.
        /// </summary>
        public float3? BoxDimensions { get; init; }

        /// <summary>
        /// Cylinder height when ZoneShape is Cylinder.
        /// </summary>
        public float? CylinderHeight { get; init; }

        /// <summary>
        /// Whether this zone is enabled.
        /// </summary>
        public bool Enabled { get; init; }

        /// <summary>
        /// Rule profile ID associated with this zone.
        /// </summary>
        public string RuleProfileId { get; init; } = string.Empty;

        /// <summary>
        /// Flows triggered on player entry.
        /// </summary>
        public IReadOnlyList<string> EntryFlows { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Flows triggered on player exit.
        /// </summary>
        public IReadOnlyList<string> ExitFlows { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Flows triggered on zone tick.
        /// </summary>
        public IReadOnlyList<string> TickFlows { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Flows triggered on match start.
        /// </summary>
        public IReadOnlyList<string> MatchStartFlows { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Flows triggered on match end.
        /// </summary>
        public IReadOnlyList<string> MatchEndFlows { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Custom metadata for this zone.
        /// </summary>
        public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();

        /// <summary>
        /// Display name for UI.
        /// </summary>
        public string DisplayName { get; init; } = string.Empty;

        /// <summary>
        /// Description of this zone.
        /// </summary>
        public string Description { get; init; } = string.Empty;

        /// <summary>
        /// Maximum players allowed in this zone (0 = unlimited).
        /// </summary>
        public int MaxPlayers { get; init; }

        /// <summary>
        /// Minimum players required to start.
        /// </summary>
        public int MinPlayers { get; init; }

        /// <summary>
        /// Tick interval in seconds for TickFlows.
        /// </summary>
        public float TickIntervalSeconds { get; init; } = 5f;
    }

    /// <summary>
    /// Zone binding to a specific flow and trigger.
    /// </summary>
    public sealed class ZoneFlowBinding
    {
        /// <summary>
        /// Zone ID this binding applies to.
        /// </summary>
        public string ZoneId { get; init; } = string.Empty;

        /// <summary>
        /// Flow name to execute.
        /// </summary>
        public string FlowName { get; init; } = string.Empty;

        /// <summary>
        /// Event trigger for this binding.
        /// </summary>
        public ZoneEventTrigger Trigger { get; init; }

        /// <summary>
        /// Whether this binding is currently enabled.
        /// </summary>
        public bool IsEnabled { get; init; } = true;

        /// <summary>
        /// Priority order (lower = higher priority).
        /// </summary>
        public int Priority { get; init; }

        /// <summary>
        /// Conditions that must be met for this binding to fire.
        /// </summary>
        public IReadOnlyList<string> Conditions { get; init; } = Array.Empty<string>();
    }

    /// <summary>
    /// Zone event triggers for flow binding.
    /// </summary>
    public enum ZoneEventTrigger
    {
        OnPlayerEnter,
        OnPlayerExit,
        OnMatchStart,
        OnMatchEnd,
        OnTick,
        OnObjectiveComplete,
        OnObjectiveFail,
        OnTimer,
        OnCustom
    }
}
