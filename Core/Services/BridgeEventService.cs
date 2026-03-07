using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using VAutomationCore.Core.Events;
using VAutomationCore.Core.Logging;

namespace VAutomationCore.Core.Services
{
    /// <summary>
    /// Shared bridge service for publishing gameplay-facing events without depending on fragile patch targets.
    /// </summary>
    public static class BridgeEventService
    {
        private static readonly CoreLogger Log = new("BridgeEventService");

        public static bool PublishVisibilityModified(Entity target, float visibilityLevel)
        {
            try
            {
                if (!UnifiedCore.EntityManager.Exists(target))
                {
                    return false;
                }

                TypedEventBus.Publish(new VisibilityModifiedEvent
                {
                    Target = target,
                    VisibilityLevel = visibilityLevel
                });

                Log.LogDebug($"Published visibility modified for {target} -> {visibilityLevel}");
                return true;
            }
            catch (Exception ex)
            {
                Log.LogError($"Failed to publish visibility modified event: {ex.Message}");
                return false;
            }
        }

        public static bool PublishDetectionTriggered(Entity detector, Entity detected, string detectionType)
        {
            try
            {
                var em = UnifiedCore.EntityManager;
                if (!em.Exists(detector) || !em.Exists(detected))
                {
                    return false;
                }

                TypedEventBus.Publish(new DetectionTriggeredEvent
                {
                    Detector = detector,
                    Detected = detected,
                    DetectionType = detectionType ?? string.Empty
                });

                Log.LogDebug($"Published detection triggered: {detector} -> {detected} ({detectionType})");
                return true;
            }
            catch (Exception ex)
            {
                Log.LogError($"Failed to publish detection event: {ex.Message}");
                return false;
            }
        }

        public static bool PublishDetectionRangeChanged(Entity target, float range)
        {
            try
            {
                if (!UnifiedCore.EntityManager.Exists(target))
                {
                    return false;
                }

                TypedEventBus.Publish(new DetectionRangeChangedEvent
                {
                    Target = target,
                    Range = range
                });

                Log.LogDebug($"Published detection range changed for {target} -> {range}");
                return true;
            }
            catch (Exception ex)
            {
                Log.LogError($"Failed to publish detection range event: {ex.Message}");
                return false;
            }
        }

        public static bool CheckAndPublishLineOfSight(Entity observer, Entity target, out bool hasLineOfSight)
        {
            hasLineOfSight = false;

            try
            {
                var em = UnifiedCore.EntityManager;
                if (!em.Exists(observer) || !em.Exists(target))
                {
                    return false;
                }

                var observerPosition = em.HasComponent<Translation>(observer)
                    ? em.GetComponentData<Translation>(observer).Value
                    : float3.zero;
                var targetPosition = em.HasComponent<Translation>(target)
                    ? em.GetComponentData<Translation>(target).Value
                    : float3.zero;

                hasLineOfSight = math.distance(observerPosition, targetPosition) <= 50f;

                TypedEventBus.Publish(new LineOfSightCheckedEvent
                {
                    Observer = observer,
                    Target = target,
                    HasLineOfSight = hasLineOfSight
                });

                Log.LogDebug($"Published line-of-sight check: {observer} -> {target} = {hasLineOfSight}");
                return true;
            }
            catch (Exception ex)
            {
                Log.LogError($"Failed to publish line-of-sight event: {ex.Message}");
                hasLineOfSight = false;
                return false;
            }
        }
    }
}
