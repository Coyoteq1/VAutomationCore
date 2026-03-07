using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace VAutomationCore.Core.Gameplay
{
    public sealed class GameplayZoneDefinition
    {
        public string ZoneId { get; init; } = string.Empty;
        public GameplayType GameplayType { get; init; }
        public string ZoneType { get; init; } = string.Empty;
        public string ZoneShape { get; init; } = "Sphere";
        public float3 Center { get; init; }
        public float Radius { get; init; }
        public bool Enabled { get; init; }
        public string RuleProfileId { get; init; } = string.Empty;
        public IReadOnlyList<string> EntryFlows { get; init; } = Array.Empty<string>();
        public IReadOnlyList<string> ExitFlows { get; init; } = Array.Empty<string>();
        public IReadOnlyList<string> TickFlows { get; init; } = Array.Empty<string>();
        public IReadOnlyDictionary<string, string> AdminOverrides { get; init; } = new Dictionary<string, string>();
    }
}
