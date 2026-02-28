using System;
using Unity.Collections;
using Unity.Entities;
using VAutomationCore.Core.ECS;
using VAutomationCore.Core.ECS.Components;
using VAutomationCore.Core.Lifecycle;

namespace VAuto.Zone.Systems
{
    /// <summary>
    /// Single owner for ZoneTransitionEvent consumption.
    /// Dispatches transition envelopes to flow/template handlers and then disposes event entities.
    /// </summary>
    public class ZoneTransitionRouterSystem : SystemBase, IZoneTransitionRouter
    {
        private EntityQuery _transitionQuery;

        public override void OnCreate()
        {
            _transitionQuery = GetEntityQuery(ComponentType.ReadOnly<ZoneTransitionEvent>());
            RequireForUpdate<ZoneTransitionEvent>();
        }

        public override void OnUpdate()
        {
            var options = Plugin.GetZoneRuntimeModeOptions();
            if (!options.EcsFlowExecutionEnabled && !options.EcsTemplateLifecycleEnabled)
            {
                return;
            }

            var em = EntityManager;
            var events = _transitionQuery.ToEntityArray(Allocator.Temp);

            try
            {
                foreach (var evtEntity in events)
                {
                    var evt = em.GetComponentData<ZoneTransitionEvent>(evtEntity);
                    var correlationId = $"{evt.Player.Index}:{DateTime.UtcNow.Ticks}";
                    var transition = new ZoneTransitionEnvelope(
                        evt.Player,
                        evt.OldZoneHash,
                        evt.NewZoneHash,
                        ZoneHashUtility.GetZoneId(evt.OldZoneHash),
                        ZoneHashUtility.GetZoneId(evt.NewZoneHash),
                        DateTime.UtcNow,
                        $"ecs.zone_detection:{correlationId}",
                        em);

                    Dispatch(transition);
                    em.DestroyEntity(evtEntity);
                }
            }
            finally
            {
                events.Dispose();
            }
        }

        public LifecycleExecutionResult Dispatch(ZoneTransitionEnvelope transition)
        {
            var options = Plugin.GetZoneRuntimeModeOptions();

            if (options.EcsTemplateLifecycleEnabled)
            {
                var templateResult = ZoneTemplateLifecycleSystem.ApplyTransition(transition);
                if (!templateResult.Success)
                {
                    return templateResult;
                }
            }

            if (options.EcsFlowExecutionEnabled)
            {
                var flowResult = FlowExecutionSystem.ApplyTransition(transition);
                if (!flowResult.Success)
                {
                    return flowResult;
                }
            }

            if (Plugin.ZoneDetectionDebug)
            {
                Plugin.Logger.LogInfo($"[ZoneTransition][route] src={transition.Source} player={transition.Player.Index} old={transition.OldZoneId} new={transition.NewZoneId}");
            }

            return LifecycleExecutionResult.Ok();
        }
    }
}
