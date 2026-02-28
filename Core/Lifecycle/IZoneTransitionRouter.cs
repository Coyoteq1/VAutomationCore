using Unity.Entities;

namespace VAutomationCore.Core.Lifecycle
{
    public interface IZoneTransitionRouter
    {
        LifecycleExecutionResult Dispatch(ZoneTransitionEnvelope transition);
    }
}
