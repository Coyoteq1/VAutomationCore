using System;
using HarmonyLib;
using ProjectM;
using Unity.Collections;
using Unity.Entities;
using VAutomationCore.Core;
using VAutomationCore.Core.Events;
using VAutomationCore.Core.Logging;

namespace VAutomationCore.Core.Patches
{
    /// <summary>
    /// Patch for DeathEventListenerSystem to track death events.
    /// Provides events for kills, deaths, and VBlood consumption.
    /// </summary>
    [HarmonyPatch]
    internal static class DeathEventSystemPatch
    {
        public static event EventHandler<DeathEventArgs> OnDeathEvent;
        
        public class DeathEventArgs : EventArgs
        {
            public Entity Killer { get; set; }
            public Entity Victim { get; set; }
            public StatChangeReason Reason { get; set; }
            public bool IsPlayerKill { get; set; }
            public bool IsVBlood { get; set; }
        }

        [HarmonyPatch(typeof(DeathEventListenerSystem), nameof(DeathEventListenerSystem.OnUpdate))]
        [HarmonyPostfix]
        static void OnUpdatePostfix(DeathEventListenerSystem __instance)
        {
            if (!CoreLogger.IsInitialized) return;

            try
            {
                var em = __instance.EntityManager;
                var deathEventEntities = __instance._DeathEventQuery.ToEntityArray(Allocator.Temp);
                var vBloodLookup = __instance.GetComponentLookup<VBloodConsumeSource>(true);
                try
                {
                    for (int i = 0; i < deathEventEntities.Length; i++)
                    {
                        var deathEventEntity = deathEventEntities[i];
                        if (!em.Exists(deathEventEntity) || !em.HasComponent<DeathEvent>(deathEventEntity))
                        {
                            continue;
                        }

                        var deathEvent = em.GetComponentData<DeathEvent>(deathEventEntity);
                    
                        var args = new DeathEventArgs
                        {
                            Killer = deathEvent.Killer,
                            Victim = deathEvent.Died,
                            Reason = deathEvent.StatChangeReason,
                            IsPlayerKill = em.Exists(deathEvent.Killer) && em.HasComponent<PlayerCharacter>(deathEvent.Killer),
                            IsVBlood = vBloodLookup.HasComponent(deathEvent.Died)
                        };

                        OnDeathEvent?.Invoke(__instance, args);
                        TypedEventBus.Publish(new DeathOccurredEvent
                        {
                            Killer = args.Killer,
                            Victim = args.Victim,
                            Reason = args.Reason,
                            IsPlayerKill = args.IsPlayerKill,
                            IsVBlood = args.IsVBlood
                        });

                        if (args.IsVBlood)
                        {
                            EventDispatcher.Publish(new BossKilledEvent
                            {
                                Killer = args.Killer,
                                Boss = args.Victim
                            });
                        }
                    }
                }
                finally
                {
                    if (deathEventEntities.IsCreated)
                    {
                        deathEventEntities.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                CoreLogger.LogException(ex, "Error processing death events");
            }
        }
    }
}
