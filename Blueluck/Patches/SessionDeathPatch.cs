using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using ProjectM;
using Unity.Collections;
using Unity.Entities;

namespace Blueluck.Patches
{
    internal static class DownedSystemPatch
    {
        internal static void Register(Harmony harmony)
        {
            foreach (var method in ResolveTargetMethods())
            {
                harmony.Patch(method, postfix: new HarmonyMethod(typeof(DownedSystemPatch).GetMethod(nameof(OnUpdatePostfix), BindingFlags.Static | BindingFlags.NonPublic)));
                Plugin.Logger.LogInfo($"[GameSession] Hooked death/downed ECS system: {method.DeclaringType?.FullName}");
            }
        }

        private static IEnumerable<MethodBase> ResolveTargetMethods()
        {
            var methods = new List<MethodBase>();
            var candidateTypeNames = new[]
            {
                "ProjectM.DownedSystem",
                "ProjectM.Gameplay.Systems.DownedSystem",
                "ProjectM.DeathSystem",
                "ProjectM.Gameplay.Systems.DeathSystem"
            };
            foreach (var candidate in candidateTypeNames)
            {
                var type = Type.GetType(candidate, throwOnError: false);
                var method = type?.GetMethod("OnUpdate", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (method != null)
                {
                    methods.Add(method);
                }
                else
                {
                    Plugin.Logger.LogWarning($"[GameSession] Hook target not found: {candidate}.OnUpdate");
                }
            }

            if (methods.Count > 0)
            {
                return methods.Distinct();
            }

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var assemblyName = assembly.GetName().Name ?? string.Empty;
                if (!assemblyName.StartsWith("ProjectM", StringComparison.Ordinal))
                {
                    continue;
                }

                Type[]? types = null;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    types = ex.Types;
                }
                catch
                {
                    continue;
                }

                foreach (var type in types.Where(t => t != null))
                {
                    try
                    {
                        var queryField = type.GetField("__DeathEventQuery", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        var onUpdate = type.GetMethod("OnUpdate", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        if (queryField != null && onUpdate != null)
                        {
                            methods.Add(onUpdate);
                        }
                    }
                    catch
                    {
                        // Ignore incomplete IL2CPP metadata types.
                    }
                }
            }

            if (methods.Count == 0)
            {
                Plugin.Logger.LogWarning("[GameSession] No runtime downed/death ECS system exposing __DeathEventQuery was found; session death tracking patch skipped.");
            }

            return methods.Distinct();
        }

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

                var queryField = __instance.GetType().GetField("__DeathEventQuery", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
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
                        var deadField = deathEvent.GetType().GetField("Dead", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
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
