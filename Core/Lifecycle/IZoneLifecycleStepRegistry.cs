using System.Collections.Generic;

namespace VAutomationCore.Core.Lifecycle
{
    /// <summary>
    /// Registry for enter/exit lifecycle steps.
    /// </summary>
    public interface IZoneLifecycleStepRegistry
    {
        IReadOnlyList<IZoneEnterStep> GetEnterSteps();
        IReadOnlyList<IZoneExitStep> GetExitSteps();
    }
}
