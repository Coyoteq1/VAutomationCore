using System;
using System.Collections.Generic;
using Unity.Mathematics;
using VAutomationCore.Core.Gameplay.Shared.Types;

namespace VAutomationCore.Core.Gameplay.Arena.Data
{
    /// <summary>
    /// Arena-specific zone definition.
    /// This type is owned by Arena module - not shared.
    /// </summary>
    public sealed class ArenaZoneDefinition
    {
        /// <summary>
        /// Zone ID.
        /// </summary>
        public string ZoneId { get; init; } = string.Empty;

        /// <summary>
        /// Display name.
        /// </summary>
        public string DisplayName { get; init; } = string.Empty;

        /// <summary>
        /// Zone type (should be "arena").
        /// </summary>
        public string ZoneType { get; init; } = "arena";

        /// <summary>
        /// Zone shape.
        /// </summary>
        public string ZoneShape { get; init; } = "Sphere";

        /// <summary>
        /// Center position.
        /// </summary>
        public float3 Center { get; init; }

        /// <summary>
        /// Radius.
        /// </summary>
        public float Radius { get; init; } = 50f;

        /// <summary>
        /// Box dimensions (if ZoneShape is Box).
        /// </summary>
        public float3? BoxDimensions { get; init; }

        /// <summary>
        /// Whether zone is enabled.
        /// </summary>
        public bool Enabled { get; init; } = true;

        /// <summary>
        /// Default rule profile ID.
        /// </summary>
        public string DefaultRuleProfileId { get; init; } = "default";

        /// <summary>
        /// Default zone settings ID.
        /// </summary>
        public string DefaultZoneSettingsId { get; init; } = "default";

        /// <summary>
        /// Maximum players allowed.
        /// </summary>
        public int MaxPlayers { get; init; } = 10;

        /// <summary>
        /// Minimum players to start.
        /// </summary>
        public int MinPlayers { get; init; } = 2;

        /// <summary>
        /// Default match mode.
        /// </summary>
        public ArenaMatchMode DefaultMatchMode { get; init; } = ArenaMatchMode.Duel;

        /// <summary>
        /// Flow bindings.
        /// </summary>
        public ArenaZoneFlows Flows { get; init; } = new();

        /// <summary>
        /// Convert to shared ZoneDefinition.
        /// </summary>
        public ZoneDefinition ToZoneDefinition()
        {
            return new ZoneDefinition
            {
                ZoneId = ZoneId,
                GameplayType = Core.Gameplay.GameplayType.Arena,
                ZoneType = ZoneType,
                ZoneShape = ZoneShape,
                Center = Center,
                Radius = Radius,
                BoxDimensions = BoxDimensions,
                Enabled = Enabled,
                RuleProfileId = DefaultRuleProfileId,
                EntryFlows = Flows.EntryFlows,
                ExitFlows = Flows.ExitFlows,
                TickFlows = Flows.TickFlows,
                MatchStartFlows = Flows.MatchStartFlows,
                MatchEndFlows = Flows.MatchEndFlows,
                DisplayName = DisplayName,
                Description = $"Arena zone: {DisplayName}",
                MaxPlayers = MaxPlayers,
                MinPlayers = MinPlayers,
                TickIntervalSeconds = 5f
            };
        }
    }

    /// <summary>
    /// Arena zone flow bindings.
    /// </summary>
    public sealed class ArenaZoneFlows
    {
        /// <summary>
        /// Flows triggered on player entry.
        /// </summary>
        public List<string> EntryFlows { get; init; } = new();

        /// <summary>
        /// Flows triggered on player exit.
        /// </summary>
        public List<string> ExitFlows { get; init; } = new();

        /// <summary>
        /// Flows triggered on tick.
        /// </summary>
        public List<string> TickFlows { get; init; } = new();

        /// <summary>
        /// Flows triggered on match start.
        /// </summary>
        public List<string> MatchStartFlows { get; init; } = new();

        /// <summary>
        /// Flows triggered on match end.
        /// </summary>
        public List<string> MatchEndFlows { get; init; } = new();
    }

    /// <summary>
    /// Default arena zone configurations.
    /// </summary>
    public static class ArenaZoneDefaults
    {
        public static readonly ArenaZoneDefinition MainArena = new()
        {
            ZoneId = "arena_main",
            DisplayName = "Main Arena",
            ZoneType = "arena",
            ZoneShape = "Sphere",
            Center = new float3(0, 0, 0),
            Radius = 50f,
            Enabled = true,
            DefaultRuleProfileId = "default",
            DefaultZoneSettingsId = "default",
            MaxPlayers = 10,
            MinPlayers = 2,
            DefaultMatchMode = ArenaMatchMode.Duel
        };

        public static readonly ArenaZoneDefinition LargeArena = new()
        {
            ZoneId = "arena_large",
            DisplayName = "Large Arena",
            ZoneType = "arena",
            ZoneShape = "Sphere",
            Center = new float3(100, 0, 0),
            Radius = 100f,
            Enabled = false,
            DefaultRuleProfileId = "default",
            DefaultZoneSettingsId = "default",
            MaxPlayers = 20,
            MinPlayers = 4,
            DefaultMatchMode = ArenaMatchMode.TeamDuel
        };

        public static IReadOnlyList<ArenaZoneDefinition> GetDefaults()
        {
            return new[] { MainArena, LargeArena };
        }
    }
}
