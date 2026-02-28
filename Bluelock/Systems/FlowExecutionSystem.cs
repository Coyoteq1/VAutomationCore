using Unity.Entities;
using VAuto.Zone.Services;
using VAutomationCore.Core.Lifecycle;

namespace VAuto.Zone.Systems
{
    /// <summary>
    /// Flow transition handler invoked by <see cref="ZoneTransitionRouterSystem"/>.
    /// This is intentionally not an ECS system to ensure ZoneTransitionEvent has a single consumer.
    /// </summary>
    public static class FlowExecutionSystem
    {
        private static IFlowLifecycle _flowLifecycle;

        public static void SetFlowLifecycle(IFlowLifecycle lifecycle)
        {
            _flowLifecycle = lifecycle;
        }

        public static LifecycleExecutionResult ApplyTransition(ZoneTransitionEnvelope transition)
        {
            if (_flowLifecycle == null)
            {
                return LifecycleExecutionResult.Fail(
                    LifecycleExecutionFailureCode.DependencyUnavailable,
                    "Flow lifecycle manager is not registered.");
            }

            if (transition.NewZoneHash != 0)
            {
                var enterZone = ZoneConfigService.GetZoneById(transition.NewZoneId);
                if (enterZone != null)
                {
                    _flowLifecycle.ExecuteEnterFlow(enterZone.FlowId, transition.Player);
                }
            }

            if (transition.OldZoneHash != 0)
            {
                var exitZone = ZoneConfigService.GetZoneById(transition.OldZoneId);
                if (exitZone != null)
                {
                    _flowLifecycle.ExecuteExitFlow(exitZone.FlowId, transition.Player);
                }
            }

            return LifecycleExecutionResult.Ok();
        }
    }
}
