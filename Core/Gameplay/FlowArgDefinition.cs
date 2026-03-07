using System;
using System.Collections.Generic;

namespace VAutomationCore.Core.Gameplay
{
    public sealed class FlowArgDefinition
    {
        public string Name { get; init; } = string.Empty;
        public FlowArgKind Kind { get; init; }
        public bool Required { get; init; }
        public object DefaultValue { get; init; }
        public string Description { get; init; } = string.Empty;
        public IReadOnlyList<string> AllowedValues { get; init; } = Array.Empty<string>();
        public string PayloadTypeName { get; init; } = string.Empty;
        public string EntityRoleName { get; init; } = string.Empty;
        public string PrefabCategory { get; init; } = string.Empty;
    }
}
