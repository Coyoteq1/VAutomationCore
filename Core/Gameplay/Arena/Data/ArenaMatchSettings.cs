using System.Collections.Generic;

namespace VAutomationCore.Core.Gameplay.Arena.Data
{
    /// <summary>
    /// Arena match settings configuration.
    /// This type is owned by Arena module - not shared.
    /// </summary>
    public sealed class ArenaMatchSettings
    {
        /// <summary>
        /// Settings ID.
        /// </summary>
        public string SettingsId { get; init; } = "default";

        /// <summary>
        /// Display name.
        /// </summary>
        public string DisplayName { get; init; } = "Default Match Settings";

        /// <summary>
        /// Default match mode.
        /// </summary>
        public ArenaMatchMode DefaultMatchMode { get; init; } = ArenaMatchMode.Duel;

        /// <summary>
        /// Match duration in seconds.
        /// </summary>
        public float MatchDurationSeconds { get; init; } = 300f;

        /// <summary>
        /// Countdown duration in seconds.
        /// </summary>
        public float CountdownSeconds { get; init; } = 10f;

        /// <summary>
        /// Minimum players to start.
        /// </summary>
        public int MinPlayersToStart { get; init; } = 2;

        /// <summary>
        /// Maximum players allowed.
        /// </summary>
        public int MaxPlayers { get; init; } = 10;

        /// <summary>
        /// Score limit.
        /// </summary>
        public int ScoreLimit { get; init; } = 100;

        /// <summary>
        /// Points per kill.
        /// </summary>
        public int PointsPerKill { get; init; } = 10;

        /// <summary>
        /// Points per death.
        /// </summary>
        public int PointsPerDeath { get; init; } = 5;

        /// <summary>
        /// Whether to auto-balance teams.
        /// </summary>
        public bool AutoBalanceTeams { get; init; } = true;

        /// <summary>
        /// Respawn enabled.
        /// </summary>
        public bool RespawnEnabled { get; init; } = true;

        /// <summary>
        /// Respawn timer in seconds.
        /// </summary>
        public float RespawnTimerSeconds { get; init; } = 10f;

        /// <summary>
        /// Wave settings for wave modes.
        /// </summary>
        public ArenaWaveSettings WaveSettings { get; init; } = new();

        /// <summary>
        /// Boss settings for boss modes.
        /// </summary>
        public ArenaBossSettings BossSettings { get; init; } = new();

        /// <summary>
        /// Validate settings.
        /// </summary>
        public List<string> Validate()
        {
            var errors = new List<string>();

            if (MatchDurationSeconds <= 0)
                errors.Add("MatchDurationSeconds must be positive");

            if (CountdownSeconds < 0)
                errors.Add("CountdownSeconds cannot be negative");

            if (MinPlayersToStart < 1)
                errors.Add("MinPlayersToStart must be at least 1");

            if (MaxPlayers < MinPlayersToStart)
                errors.Add("MaxPlayers cannot be less than MinPlayersToStart");

            if (ScoreLimit <= 0)
                errors.Add("ScoreLimit must be positive");

            return errors;
        }
    }

    /// <summary>
    /// Wave mode settings.
    /// </summary>
    public sealed class ArenaWaveSettings
    {
        /// <summary>
        /// Wave duration in seconds.
        /// </summary>
        public float WaveDurationSeconds { get; init; } = 120f;

        /// <summary>
        /// Time between waves.
        /// </summary>
        public float TimeBetweenWaves { get; init; } = 30f;

        /// <summary>
        /// Enemies per wave.
        /// </summary>
        public int EnemiesPerWave { get; init; } = 5;

        /// <summary>
        /// Enemy scaling per wave.
        /// </summary>
        public float EnemyScalingPerWave { get; init; } = 1.1f;
    }

    /// <summary>
    /// Boss mode settings.
    /// </summary>
    public sealed class ArenaBossSettings
    {
        /// <summary>
        /// Boss spawn delay.
        /// </summary>
        public float BossSpawnDelay { get; init; } = 30f;

        /// <summary>
        /// Boss health multiplier.
        /// </summary>
        public float BossHealthMultiplier { get; init; } = 1.0f;

        /// <summary>
        /// Boss damage multiplier.
        /// </summary>
        public float BossDamageMultiplier { get; init; } = 1.0f;

        /// <summary>
        /// Reward multiplier on boss defeat.
        /// </summary>
        public float RewardMultiplier { get; init; } = 2.0f;
    }
}
