using System.Collections.Generic;

namespace VAutomationCore.Core.Gameplay
{
    public sealed class FlowStepDefinition
    {
        public string ActionName { get; init; } = string.Empty;
        public IReadOnlyList<string> Arguments { get; init; } = System.Array.Empty<string>();
    }
}
