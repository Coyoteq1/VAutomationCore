using System;
using System.Collections.Generic;

namespace VAutomationCore.Core.Gameplay.Arena.Data
{
    /// <summary>
    /// Runtime state for an arena.
    /// This type is owned by Arena module - not shared.
    /// </summary>
    public sealed class ArenaState
    {
        /// <summary>
        /// Arena ID.
        /// </summary>
        public string ArenaId { get; init; } = string.Empty;

        /// <summary>
        /// Current arena state.
        /// </summary>
        public ArenaStateType State { get; set; } = ArenaStateType.Inactive;

        /// <summary>
        /// Current match mode.
        /// </summary>
        public ArenaMatchMode MatchMode { get; set; } = ArenaMatchMode.Duel;

        /// <summary>
        /// Current rule profile ID.
        /// </summary>
        public string RuleProfileId { get; set; } = "default";

        /// <summary>
        /// Current zone settings ID.
        /// </summary>
        public string ZoneSettingsId { get; set; } = "default";

        /// <summary>
        /// Players currently in the arena.
        /// </summary>
        public HashSet<string> Players { get; set; } = new();

        /// <summary>
        /// Players by team.
        /// </summary>
        public Dictionary<int, HashSet<string>> TeamPlayers { get; set; } = new()
        {
            [1] = new HashSet<string>(),
            [2] = new HashSet<string>()
        };

        /// <summary>
        /// Current spectators.
        /// </summary>
        public HashSet<string> Spectators { get; set; } = new();

        /// <summary>
        /// Player scores.
        /// </summary>
        public Dictionary<string, int> PlayerScores { get; set; } = new();

        /// <summary>
        /// Team scores.
        /// </summary>
        public Dictionary<int, int> TeamScores { get; set; } = new()
        {
            [1] = 0,
            [2] = 0
        };

        /// <summary>
        /// Player kills.
        /// </summary>
        public Dictionary<string, int> PlayerKills { get; set; } = new();

        /// <summary>
        /// Player deaths.
        /// </summary>
        public Dictionary<string, int> PlayerDeaths { get; set; } = new();

        /// <summary>
        /// Current match start time.
        /// </summary>
        public DateTime? MatchStartTime { get; set; }

        /// <summary>
        /// Match duration in seconds.
        /// </summary>
        public float MatchDurationSeconds { get; set; }

        /// <summary>
        /// Current wave number (for wave modes).
        /// </summary>
        public int CurrentWave { get; set; }

        /// <summary>
        /// Is PvP currently enabled.
        /// </summary>
        public bool IsPvpEnabled { get; set; } = true;

        /// <summary>
        /// Is friendly fire enabled.
        /// </summary>
        public bool IsFriendlyFireEnabled { get; set; }

        /// <summary>
        /// Active boss entities.
        /// </summary>
        public List<string> ActiveBosses { get; set; } = new();

        /// <summary>
        /// Spawned enemy entities.
        /// </summary>
        public List<string> SpawnedEnemies { get; set; } = new();

        /// <summary>
        /// Last error message.
        /// </summary>
        public string? LastError { get; set; }

        /// <summary>
        /// Whether the arena is enabled.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Get player count.
        /// </summary>
        public int PlayerCount => Players.Count;

        /// <summary>
        /// Get spectator count.
        /// </summary>
        public int SpectatorCount => Spectators.Count;

        /// <summary>
        /// Get total participant count.
        /// </summary>
        public int ParticipantCount => Players.Count + Spectators.Count;

        /// <summary>
        /// Get match elapsed time.
        /// </summary>
        public TimeSpan? MatchElapsedTime => MatchStartTime.HasValue 
            ? DateTime.UtcNow - MatchStartTime.Value 
            : null;

        /// <summary>
        /// Get remaining match time.
        /// </summary>
        public TimeSpan? MatchRemainingTime
        {
            get
            {
                if (!MatchStartTime.HasValue || MatchDurationSeconds <= 0)
                    return null;

                var elapsed = DateTime.UtcNow - MatchStartTime.Value;
                var remaining = TimeSpan.FromSeconds(MatchDurationSeconds) - elapsed;
                return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
            }
        }
    }

    /// <summary>
    /// Arena state types.
    /// </summary>
    public enum ArenaStateType
    {
        Inactive,
        Waiting,          // Waiting for players
        Countdown,        // Match starting soon
        Active,           // Match in progress
        Paused,           // Match paused
        Ending,           // Match ending
        Maintenance       // Under maintenance
    }
}
