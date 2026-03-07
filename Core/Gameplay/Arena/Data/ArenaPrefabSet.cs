using System.Collections.Generic;

namespace VAutomationCore.Core.Gameplay.Arena.Data
{
    /// <summary>
    /// Arena-specific prefab definitions.
    /// This type is owned by Arena module - not shared.
    /// </summary>
    public sealed class ArenaPrefabSet
    {
        /// <summary>
        /// Unique identifier for this prefab set.
        /// </summary>
        public string SetId { get; init; } = "default";

        /// <summary>
        /// Display name for this prefab set.
        /// </summary>
        public string DisplayName { get; init; } = "Default Arena Prefabs";

        // ========================================
        // Border / Boundary Prefabs
        // ========================================
        
        /// <summary>
        /// Prefab GUID for arena border effect.
        /// </summary>
        public string BorderPrefab { get; init; } = string.Empty;

        /// <summary>
        /// Prefab GUID for invisible boundary trigger.
        /// </summary>
        public string BoundaryTriggerPrefab { get; init; } = string.Empty;

        // ========================================
        // Spawn Point Prefabs
        // ========================================

        /// <summary>
        /// Prefab GUID for team 1 spawn point.
        /// </summary>
        public string Team1SpawnPrefab { get; init; } = string.Empty;

        /// <summary>
        /// Prefab GUID for team 2 spawn point.
        /// </summary>
        public string Team2SpawnPrefab { get; init; } = string.Empty;

        /// <summary>
        /// Prefab GUID for spectator spawn point.
        /// </summary>
        public string SpectatorSpawnPrefab { get; init; } = string.Empty;

        /// <summary>
        /// Prefab GUID for free-for-all spawn points.
        /// </summary>
        public List<string> FfaSpawnPrefabs { get; init; } = new();

        // ========================================
        // Spectator / UI Prefabs
        // ========================================

        /// <summary>
        /// Prefab GUID for spectator marker.
        /// </summary>
        public string SpectatorMarkerPrefab { get; init; } = string.Empty;

        /// <summary>
        /// Prefab GUID for scoreboard UI.
        /// </summary>
        public string ScoreboardPrefab { get; init; } = string.Empty;

        /// <summary>
        /// Prefab GUID for timer display.
        /// </summary>
        public string TimerPrefab { get; init; } = string.Empty;

        // ========================================
        // Reward / Objective Prefabs
        // ========================================

        /// <summary>
        /// Prefab GUID for reward chest.
        /// </summary>
        public string RewardChestPrefab { get; init; } = string.Empty;

        /// <summary>
        /// Prefab GUID for flag (CTF mode).
        /// </summary>
        public string FlagPrefab { get; init; } = string.Empty;

        /// <summary>
        /// Prefab GUID for capture point (KOTH mode).
        /// </summary>
        public string CapturePointPrefab { get; init; } = string.Empty;

        /// <summary>
        /// Prefab GUID for boss spawner.
        /// </summary>
        public string BossSpawnerPrefab { get; init; } = string.Empty;

        // ========================================
        // Effect Prefabs
        // ========================================

        /// <summary>
        /// Prefab GUID for kill effect.
        /// </summary>
        public string KillEffectPrefab { get; init; } = string.Empty;

        /// <summary>
        /// Prefab GUID for spawn effect.
        /// </summary>
        public string SpawnEffectPrefab { get; init; } = string.Empty;

        /// <summary>
        /// Prefab GUID for victory effect.
        /// </summary>
        public string VictoryEffectPrefab { get; init; } = string.Empty;

        /// <summary>
        /// Prefab GUID for start countdown effect.
        /// </summary>
        public string CountdownEffectPrefab { get; init; } = string.Empty;

        // ========================================
        // Enemy Prefabs (PvE/Wave Mode)
        // ========================================

        /// <summary>
        /// Prefab GUID for basic enemy.
        /// </summary>
        public string BasicEnemyPrefab { get; init; } = string.Empty;

        /// <summary>
        /// Prefab GUID for elite enemy.
        /// </summary>
        public string EliteEnemyPrefab { get; init; } = string.Empty;

        /// <summary>
        /// Prefab GUID for boss enemy.
        /// </summary>
        public string BossEnemyPrefab { get; init; } = string.Empty;

        /// <summary>
        /// Get all prefab categories for this set.
        /// </summary>
        public Dictionary<string, List<string>> GetPrefabCategories()
        {
            var categories = new Dictionary<string, List<string>>
            {
                ["Border"] = new List<string> { BorderPrefab, BoundaryTriggerPrefab },
                ["Spawns"] = new List<string> { Team1SpawnPrefab, Team2SpawnPrefab, SpectatorSpawnPrefab },
                ["Spectator"] = new List<string> { SpectatorMarkerPrefab, ScoreboardPrefab, TimerPrefab },
                ["Rewards"] = new List<string> { RewardChestPrefab, FlagPrefab, CapturePointPrefab },
                ["Effects"] = new List<string> { KillEffectPrefab, SpawnEffectPrefab, VictoryEffectPrefab, CountdownEffectPrefab },
                ["Enemies"] = new List<string> { BasicEnemyPrefab, EliteEnemyPrefab, BossEnemyPrefab }
            };

            // Add FFA spawns
            if (FfaSpawnPrefabs.Count > 0)
            {
                categories["FfaSpawns"] = FfaSpawnPrefabs;
            }

            return categories;
        }
    }

    /// <summary>
    /// Category names for arena prefabs.
    /// </summary>
    public static class ArenaPrefabCategories
    {
        public const string Border = "Border";
        public const string Spawns = "Spawns";
        public const string Spectator = "Spectator";
        public const string Rewards = "Rewards";
        public const string Effects = "Effects";
        public const string Enemies = "Enemies";
        public const string FfaSpawns = "FfaSpawns";
    }
}
