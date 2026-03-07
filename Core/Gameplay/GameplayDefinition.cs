using System;
using System.Collections.Generic;

namespace VAutomationCore.Core.Gameplay
{
    public sealed class GameplayDefinition
    {
        public string Id { get; init; } = string.Empty;
        public GameplayType GameplayType { get; init; }
        public string DisplayName { get; init; } = string.Empty;
        public bool Enabled { get; init; }
        public string ConfigDirectory { get; init; } = string.Empty;
        public IReadOnlyList<string> ZoneTypes { get; init; } = Array.Empty<string>();
        public IReadOnlyList<string> FlowNames { get; init; } = Array.Empty<string>();
    }
}
