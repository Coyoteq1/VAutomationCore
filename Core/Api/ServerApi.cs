using System;
using Unity.Entities;

namespace VAutomationCore.Core.Api
{
    /// <summary>
    /// Server-level helpers for readiness and ECS access.
    /// </summary>
    public static class ServerApi
    {
        public static bool IsReady()
        {
            return Core.UnifiedCore.IsInitialized;
        }

        public static bool TryGetServerWorld(out World world)
        {
            world = default!;
            try
            {
                world = Core.UnifiedCore.Server;
                return world != null;
            }
            catch
            {
                return false;
            }
        }

        public static bool TryGetEntityManager(out EntityManager entityManager)
        {
            entityManager = default;
            if (!TryGetServerWorld(out _))
            {
                return false;
            }

            entityManager = Core.UnifiedCore.EntityManager;
            return true;
        }

        public static DateTime GetServerTimeUtc()
        {
            return DateTime.UtcNow;
        }
    }
}
