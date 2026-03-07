using System;
using System.Reflection;
using HarmonyLib;
using ProjectM;
using Unity.Collections;
using Unity.Entities;

namespace Blueluck.Patches
{
    [HarmonyPatch]
    internal static class DownedSystemPatch
    {
        private static Type? ResolveTargetType()
        {
            var candidates = new[]
            {
                "ProjectM.DownedSystem",
                "ProjectM.Gameplay.Systems.DownedSystem",
                "ProjectM.DeathSystem",
                "ProjectM.Gameplay.Systems.DeathSystem"
            };

            foreach (var candidate in candidates)
            {
                var type = AccessTools.TypeByName(candidate);
                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }

        private static bool Prepare()
        {
            var targetType = ResolveTargetType();
            if (targetType != null)
            {
                return true;
            }

            Plugin.Logger.LogWarning("[GameSession] Downed/death system type not found; session death tracking patch skipped.");
            return false;
        }

        private static MethodBase? TargetMethod()
        {
            var systemType = ResolveTargetType();
            return systemType == null ? null : AccessTools.Method(systemType, "OnUpdate");
        }

        [HarmonyPostfix]
        private static void OnUpdatePostfix(object __instance)
        {
            try
            {
                if (Plugin.GameSessions?.IsInitialized != true)
                {
                    return;
                }

                var world = World.DefaultGameObjectInjectionWorld;
                if (world == null || !world.IsCreated)
                {
                    return;
                }

                var entityManager = world.EntityManager;

                var queryField = AccessTools.Field(__instance.GetType(), "__DeathEventQuery");
                if (queryField == null)
                {
                    return;
                }

                if (queryField.GetValue(__instance) is not EntityQuery queryHandle)
                {
                    return;
                }

                var query = queryHandle.ToEntityArray(Allocator.Temp);
                try
                {
                    foreach (var entity in query)
                    {
                        if (!entityManager.HasComponent<DeathEvent>(entity))
                        {
                            continue;
                        }

                        var deathEvent = entityManager.GetComponentData<DeathEvent>(entity);
                        var deadField = AccessTools.Field(deathEvent.GetType(), "Dead");
                        if (deadField?.GetValue(deathEvent) is Entity deadEntity)
                        {
                            Plugin.GameSessions.HandleEntityDeath(deadEntity);
                        }
                    }
                }
                finally
                {
                    query.Dispose();
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"[GameSession] Failed processing death events: {ex.Message}");
            }
        }
    }
}
