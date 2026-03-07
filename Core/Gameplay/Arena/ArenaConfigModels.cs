using System;
using System.Collections.Generic;

namespace VAutomationCore.Core.Gameplay.Arena
{
    public sealed class ArenaSettingsConfig
    {
        public bool Enabled { get; set; } = true;
        public string DefaultRuleProfileId { get; set; } = "default";
        public string DefaultMatchMode { get; set; } = "duel";
    }

    public sealed class ArenaZoneConfig
    {
        public string ZoneId { get; set; } = "arena_main";
        public string Name { get; set; } = "Main Arena";
        public string ZoneType { get; set; } = "arena";
        public string ZoneShape { get; set; } = "Sphere";
        public float[] Center { get; set; } = new[] { 0f, 0f, 0f };
        public float Radius { get; set; } = 50f;
        public bool Enabled { get; set; } = true;
        public string RuleProfileId { get; set; } = "default";
        public List<string> EntryFlows { get; set; } = new();
        public List<string> ExitFlows { get; set; } = new();
        public List<string> TickFlows { get; set; } = new();
    }

    public sealed class ArenaRuleProfileConfig
    {
        public string Id { get; set; } = "default";
        public bool PvpEnabled { get; set; } = true;
        public bool SpectatorsAllowed { get; set; } = true;
        public bool MountsAllowed { get; set; }
    }

    public sealed class ArenaPrefabConfig
    {
        public string BorderPrefab { get; set; } = string.Empty;
        public string SpectatorMarkerPrefab { get; set; } = string.Empty;
        public string RewardChestPrefab { get; set; } = string.Empty;
    }
}
