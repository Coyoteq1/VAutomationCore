using VAuto.Zone.Services;
using VAutomationCore.Core.Lifecycle;

namespace VAuto.Zone.Systems
{
    /// <summary>
    /// Template transition handler invoked by <see cref="ZoneTransitionRouterSystem"/>.
    /// This is intentionally not an ECS system to ensure ZoneTransitionEvent has a single consumer.
    /// </summary>
    public static class ZoneTemplateLifecycleSystem
    {
        public static LifecycleExecutionResult ApplyTransition(ZoneTransitionEnvelope transition)
        {
            var em = transition.EntityManager;

            if (transition.OldZoneHash != 0)
            {
                ZoneTemplateService.ClearAllZoneTemplates(transition.OldZoneId, em);
            }

            if (transition.NewZoneHash != 0)
            {
                ZoneTemplateService.SpawnAllZoneTemplates(transition.NewZoneId, em);
            }

            return LifecycleExecutionResult.Ok();
        }
    }
}
