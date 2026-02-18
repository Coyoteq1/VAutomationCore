using HarmonyLib;
using System;
using System.Reflection;

namespace VLifecycle.Services.Lifecycle
{
    /// <summary>
    /// Global respawn prevention gate controlled by lifecycle admin commands.
    /// </summary>
    public static class RespawnPreventionService
    {
        private static int _preventRespawnsCount;

        public static bool IsEnabled => _preventRespawnsCount > 0;
        public static int RefCount => _preventRespawnsCount;

        public static void PreventRespawns()
        {
            _preventRespawnsCount++;
        }

        public static void AllowRespawns()
        {
            _preventRespawnsCount--;
            if (_preventRespawnsCount < 0)
            {
                _preventRespawnsCount = 0;
            }
        }

        public static void Reset()
        {
            _preventRespawnsCount = 0;
        }
    }

    internal static class RespawnPreventionPatches
    {
        public static MethodBase ResolveInitializeNewSpawnChainOnUpdate()
        {
            var candidates = new[]
            {
                "ProjectM.InitializeNewSpawnChainSystem, ProjectM",
                "ProjectM.Gameplay.Systems.InitializeNewSpawnChainSystem, ProjectM.Gameplay.Systems"
            };

            foreach (var candidate in candidates)
            {
                var type = Type.GetType(candidate, throwOnError: false);
                if (type == null)
                {
                    continue;
                }

                var method = type.GetMethod("OnUpdate", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (method != null)
                {
                    return method;
                }
            }

            return null;
        }

        public static bool InitializeNewSpawnChainPrefix()
        {
            return !RespawnPreventionService.IsEnabled;
        }
    }
}
