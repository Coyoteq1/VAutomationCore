using System.Collections.Generic;
using Unity.Entities;
using Unity.Collections;
using ProjectM;
using VAutomationCore.Core;

namespace VAuto.Core.Lifecycle.Handlers
{
    /// <summary>
    /// Simulates the same progression changes that VBlood feeds perform.
    /// Unlocks all tech, recipes, powers, and forms for sandbox gameplay.
    /// Uses DebugEventBridge for proper game integration with backup/restore support.
    /// 
    /// Usage:
    /// - Call OnPlayerIsInZone(player) while player is in sandbox arena
    /// - Call OnPlayerExitZone(player) when player exits sandbox arena
    /// 
    /// Thread Safety:
    /// - All methods are thread-safe via DebugEventBridge's ConcurrentDictionary usage
    /// - Atomic backup creation ensures only one backup per SteamId
    /// - Concurrent enter/exit operations are handled safely
    /// </summary>
    public static class SandboxUnlockUtility
    {
        private const string LogSource = "SandboxUnlockUtility";

        /// <summary>
        /// Called while a player is in a sandbox zone.
        /// Creates a backup if needed, then applies unlocks via game events.
        /// </summary>
        /// <param name="player">The player entity.</param>
        public static void OnPlayerIsInZone(Entity player)
        {
            ValidateParameters(player);

            // Direct is-in-zone flow (idempotent)
            DebugEventBridge.OnPlayerIsInZone(player);
        }

        /// <summary>
        /// Backward-compatible wrapper. Prefer OnPlayerIsInZone.
        /// </summary>
        [System.Obsolete("Use OnPlayerIsInZone for sandbox flow")]
        public static void OnPlayerEnterZone(Entity player)
        {
            OnPlayerIsInZone(player);
        }

        /// <summary>
        /// Called when a player exits a sandbox zone.
        /// Restores from backup if exists, otherwise uses fallback restoration.
        /// </summary>
        /// <param name="player">The player entity.</param>
        public static void OnPlayerExitZone(Entity player)
        {
            ValidateParameters(player);

            // Use DebugEventBridge's static convenience methods
            DebugEventBridge.OnPlayerExitZone(player);
        }

        /// <summary>
        /// Unlocks all VBlood content for the player.
        /// Legacy method - prefer using OnPlayerIsInZone for proper backup/restore.
        /// </summary>
        /// <param name="user">The player entity.</param>
        [System.Obsolete("Use OnPlayerIsInZone instead for proper backup/restore support")]
        public static void UnlockEverythingForPlayer(Entity user)
        {
            if (!VAutomationCore.Core.UnifiedCore.EntityExists(user))
            {
                VAutoLogger.LogWarning($"[{LogSource}] User entity does not exist");
                return;
            }

            // Route to bridge authority to avoid direct ECS mutations in sandbox utility.
            DebugEventBridge.OnPlayerIsInZone(user);
            VAutoLogger.LogInfo($"[{LogSource}] ✅ Legacy unlock request routed to DebugEventBridge");
        }

        /// <summary>
        /// Resets all temporary unlocks applied during arena session.
        /// Legacy method - prefer using OnPlayerExitZone for proper backup/restore.
        /// </summary>
        /// <param name="user">The player entity.</param>
        [System.Obsolete("Use OnPlayerExitZone instead for proper backup/restore support")]
        public static void ResetTemporaryUnlocks(Entity user)
        {
            if (!VAutomationCore.Core.UnifiedCore.EntityExists(user))
            {
                VAutoLogger.LogWarning($"[{LogSource}] User entity does not exist");
                return;
            }

            // Route to bridge authority to avoid direct ECS restore mutations in sandbox utility.
            DebugEventBridge.OnPlayerExitZone(user);
            VAutoLogger.LogInfo($"[{LogSource}] ✅ Legacy reset request routed to DebugEventBridge");
        }

        #region Validation and Helpers

        private static void ValidateParameters(Entity player)
        {
            if (player == Entity.Null)
            {
                throw new System.ArgumentException("Player entity cannot be null", nameof(player));
            }

            if (!VAutomationCore.Core.UnifiedCore.EntityExists(player))
            {
                throw new System.ArgumentException("Player entity does not exist", nameof(player));
            }
        }

        private static ulong GetSteamId(Entity user)
        {
            var em = VAutomationCore.Core.UnifiedCore.EntityManager;
            
            if (em.HasComponent<SteamPlayerID>(user))
            {
                var steamId = em.GetComponentData<SteamPlayerID>(user);
                return steamId.SteamId;
            }
            return 0;
        }

        #endregion

        #region Legacy Fallback Methods (disabled by policy)

        /// <summary>
        /// Fallback unlock method when DebugEventBridge is unavailable.
        /// </summary>
        private static void UnlockEverythingForPlayerFallback(Entity user)
        {
            VAutoLogger.LogWarning($"[{LogSource}] Direct fallback unlock disabled by sandbox policy");
        }

        /// <summary>
        /// Fallback restore method when no backup exists.
        /// </summary>
        private static void FallbackRestore(Entity user)
        {
            VAutoLogger.LogWarning($"[{LogSource}] Direct fallback restore disabled by sandbox policy");
        }

        private static void UnlockAllTech(Entity user)
        {
            VAutoLogger.LogWarning($"[{LogSource}] UnlockAllTech direct path disabled");
        }

        private static void UnlockAllRecipes(Entity user)
        {
            VAutoLogger.LogWarning($"[{LogSource}] UnlockAllRecipes direct path disabled");
        }

        private static void UnlockAllPowers(Entity user)
        {
            VAutoLogger.LogWarning($"[{LogSource}] UnlockAllPowers direct path disabled");
        }

        private static void UnlockAllTrophies(Entity user)
        {
            VAutoLogger.LogWarning($"[{LogSource}] UnlockAllTrophies direct path disabled");
        }

        private static void ClearBloodTypes(Entity user)
        {
            VAutoLogger.LogWarning($"[{LogSource}] ClearBloodTypes direct path disabled");
        }

        private static void ClearTechs(Entity user)
        {
            VAutoLogger.LogWarning($"[{LogSource}] ClearTechs direct path disabled");
        }

        private static void ClearRecipes(Entity user)
        {
            VAutoLogger.LogWarning($"[{LogSource}] ClearRecipes direct path disabled");
        }

        private static void ClearPowers(Entity user)
        {
            VAutoLogger.LogWarning($"[{LogSource}] ClearPowers direct path disabled");
        }

        private static void FireUnlockGameplayEvents(Entity user)
        {
            VAutoLogger.LogWarning($"[{LogSource}] FireUnlockGameplayEvents direct path disabled");
        }

        #endregion
    }

    #region Internal Table Placeholders

    /// <summary>
    /// Placeholder for tech ID collection. Populate from PrefabIndex.json or TechCollectionSystem.
    /// </summary>
    public static class InternalTechTable
    {
        public static readonly List<int> AllTechIds = new()
        {
            // TODO: Populate with actual tech prefab GUIDs from TechCollectionSystem
        };

        public static void LoadFromGameData()
        {
            // TODO: Implement loading from TechCollectionSystem or JSON
        }
    }

    /// <summary>
    /// Placeholder for recipe ID collection.
    /// </summary>
    public static class InternalRecipeTable
    {
        public static readonly List<int> AllRecipeIds = new()
        {
            // TODO: Populate with actual recipe prefab GUIDs
        };

        public static void LoadFromGameData()
        {
            // TODO: Implement loading from RecipeCollectionSystem
        }
    }

    /// <summary>
    /// Placeholder for power/vampire ability ID collection.
    /// </summary>
    public static class InternalPowerTable
    {
        public static readonly List<int> AllPowerIds = new()
        {
            // TODO: Populate with actual power prefab GUIDs from AbilitySystem
        };

        public static void LoadFromGameData()
        {
            // TODO: Implement loading from PowerCollectionSystem
        }
    }

    /// <summary>
    /// Placeholder for trophy/blood unlock ID collection.
    /// </summary>
    public static class InternalTrophyTable
    {
        public static readonly List<int> AllTrophyIds = new()
        {
            // TODO: Populate with actual trophy prefab GUIDs
        };

        public static void LoadFromGameData()
        {
            // TODO: Implement loading from TrophyCollectionSystem
        }
    }

    #endregion
}
