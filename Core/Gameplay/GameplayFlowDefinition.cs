using System;
using System.Collections.Generic;

namespace VAutomationCore.Core.Gameplay
{
    public sealed class GameplayFlowDefinition
    {
        public string Name { get; init; } = string.Empty;
        public GameplayType GameplayType { get; init; }
        public string Description { get; init; } = string.Empty;
        public bool AdminOnly { get; init; }
        public bool EnabledByDefault { get; init; } = true;
        public IReadOnlyList<string> SupportedZoneTypes { get; init; } = Array.Empty<string>();
        public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
        public IReadOnlyList<FlowArgDefinition> Arguments { get; init; } = Array.Empty<FlowArgDefinition>();
    }
}
