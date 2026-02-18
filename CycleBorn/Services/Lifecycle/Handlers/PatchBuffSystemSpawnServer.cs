using HarmonyLib;
using ProjectM;
using ProjectM.Gameplay.Systems;
using Unity.Entities;
using Unity.Collections;
using static VAutomationCore.Core.UnifiedCore;

using System;

namespace VAuto.Core.Lifecycle.Handlers
{
    /// <summary>
    /// Non-destructive patch for VBlood feed suppression.
    /// Instead of destroying entities, we tag sandboxed players and schedule delayed despawn.
    /// </summary>
    [HarmonyPatch(typeof(BuffSystem_Spawn_Server))]
    public static class PatchBuffSystemSpawnServer
    {
        private const string LogSource = "PatchBuffSystemSpawnServer";

        // Prefab GUIDs for VBlood feeds (example placeholders)
        private static readonly int FEED_BOSS_03_COMPLETE_TRIGGER = 123456;
        private static readonly int FEED_BOSS_04_COMPLETE_AREA_TRIGGER = 789012;

        [HarmonyPrefix]
        [HarmonyPatch("OnUpdate")]
        public static void Prefix(BuffSystem_Spawn_Server __instance)
        {
            try
            {
                if (!ArenaTracker.IsAnyPlayerInArena) return;
                var em = UnifiedCore.EntityManager;
                if (em == null) return;

                // Safely iterate spawn queue
                using (var entities = __instance._SpawnQueue.ToEntityArray(Allocator.Temp))
                {
                    foreach (var spawnEntity in entities)
                    {
                        if (!em.Exists(spawnEntity)) continue;
                        if (!em.HasComponent<PrefabGUID>(spawnEntity)) continue;
                        
                        var guid = em.GetComponentData<PrefabGUID>(spawnEntity).GuidHash;
                        
                        if (guid == FEED_BOSS_03_COMPLETE_TRIGGER || 
                            guid == FEED_BOSS_04_COMPLETE_AREA_TRIGGER)
                        {
                            if (!em.HasComponent<FromCharacter>(spawnEntity)) continue;
                            var playerEntity = em.GetComponentData<FromCharacter>(spawnEntity).Character;
                            
                            // Non-Destructive Tagging
                            if (!em.HasComponent<SandboxModeTag>(playerEntity))
                            {
                                var steamId = DebugEventBridge.Instance.GetSteamId(playerEntity);
                                em.AddComponentData(playerEntity, new SandboxModeTag
                                {
                                    SteamId = steamId,
                                    ActivatedAt = (float)DateTime.UtcNow.TimeOfDay.TotalSeconds
                                });
                                VAutoLogger.LogInfo($"[{LogSource}] ✅ Active SandboxModeTag for {steamId}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                VAutoLogger.LogException(ex);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnUpdate")]
        public static void Postfix(BuffSystem_Spawn_Server __instance)
        {
            try
            {
                if (!ArenaTracker.IsAnyPlayerInArena) return;
                var em = UnifiedCore.EntityManager;
                if (em == null) return;

                var pendingBosses = ArenaTracker.GetPendingBosses();
                foreach (var boss in pendingBosses)
                {
                    if (!em.Exists(boss)) 
                    { 
                        ArenaTracker.RemovePendingBoss(boss); 
                        continue; 
                    }

                    // Schedule delayed despawn (2s) to allow death animations
                    if (!em.HasComponent<PendingDespawn>(boss))
                    {
                        em.AddComponentData(boss, new PendingDespawn
                        {
                            DespawnAt = (float)DateTime.UtcNow.TimeOfDay.TotalSeconds + 2.0f,
                            Reason = "SandboxCleanup"
                        });
                        VAutoLogger.LogDebug($"[{LogSource}] ⏱️ Scheduled PendingDespawn for boss (2s delay)");
                    }
                }
            }
            catch (Exception ex)
            {
                VAutoLogger.LogException(ex);
            }
        }
    }
}
