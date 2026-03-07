using System;
using System.Collections.Generic;
using VAutomationCore.Core.Gameplay.Shared.Contracts;

namespace VAutomationCore.Core.Gameplay.Arena.Data
{
    /// <summary>
    /// Arena-specific rule profile.
    /// This type is owned by Arena module - not shared.
    /// </summary>
    [RuleProfile("ArenaRuleProfile", "Arena Rule Profile", "Defines rules for arena matches")]
    public sealed class ArenaRuleProfile : IRuleProfile
    {
        public string ProfileId { get; init; } = "default";
        public string DisplayName { get; init; } = "Default Arena Rules";
        public string Description { get; init; } = "Standard arena match rules";
        
        public bool PvpEnabled { get; init; } = true;
        public bool FriendlyFireEnabled { get; init; }
        public bool MountsAllowed { get; init; }
        public bool SpectatorsAllowed { get; init; } = true;

        /// <summary>
        /// Maximum time for a match in seconds.
        /// </summary>
        public float MatchDurationSeconds { get; init; } = 300f;

        /// <summary>
        /// Minimum players to start a match.
        /// </summary>
        public int MinPlayersToStart { get; init; } = 2;

        /// <summary>
        /// Maximum players per team.
        /// </summary>
        public int MaxPlayersPerTeam { get; init; } = 5;

        /// <summary>
        /// Whether to auto-balance teams.
        /// </summary>
        public bool AutoBalanceTeams { get; init; } = true;

        /// <summary>
        /// Score limit for match end.
        /// </summary>
        public int ScoreLimit { get; init; } = 100;

        /// <summary>
        /// Points awarded per kill.
        /// </summary>
        public int PointsPerKill { get; init; } = 10;

        /// <summary>
        /// Points awarded per death (to opponent).
        /// </summary>
        public int PointsPerDeath { get; init; } = 5;

        /// <summary>
        /// Whether resurrection is allowed.
        /// </summary>
        public bool ResurrectionAllowed { get; init; } = true;

        /// <summary>
        /// Resurrection wait time in seconds.
        /// </summary>
        public float ResurrectionWaitSeconds { get; init; } = 5f;

        /// <summary>
        /// Arena difficulty level (1-10).
        /// </summary>
        public int DifficultyLevel { get; init; } = 1;

        /// <summary>
        /// Whether to enable friendly fire indicators.
        /// </summary>
        public bool ShowFriendlyFireIndicator { get; init; } = true;

        public IReadOnlyDictionary<string, string> GetRules()
        {
            return new Dictionary<string, string>
            {
                ["PvpEnabled"] = PvpEnabled.ToString(),
                ["FriendlyFireEnabled"] = FriendlyFireEnabled.ToString(),
                ["MountsAllowed"] = MountsAllowed.ToString(),
                ["SpectatorsAllowed"] = SpectatorsAllowed.ToString(),
                ["MatchDurationSeconds"] = MatchDurationSeconds.ToString(),
                ["MinPlayersToStart"] = MinPlayersToStart.ToString(),
                ["MaxPlayersPerTeam"] = MaxPlayersPerTeam.ToString(),
                ["AutoBalanceTeams"] = AutoBalanceTeams.ToString(),
                ["ScoreLimit"] = ScoreLimit.ToString(),
                ["PointsPerKill"] = PointsPerKill.ToString(),
                ["PointsPerDeath"] = PointsPerDeath.ToString(),
                ["ResurrectionAllowed"] = ResurrectionAllowed.ToString(),
                ["ResurrectionWaitSeconds"] = ResurrectionWaitSeconds.ToString(),
                ["DifficultyLevel"] = DifficultyLevel.ToString(),
                ["ShowFriendlyFireIndicator"] = ShowFriendlyFireIndicator.ToString()
            };
        }

        public IReadOnlyList<string> Validate()
        {
            var errors = new List<string>();

            if (MatchDurationSeconds <= 0)
            {
                errors.Add("MatchDurationSeconds must be positive");
            }

            if (MinPlayersToStart < 1)
            {
                errors.Add("MinPlayersToStart must be at least 1");
            }

            if (MaxPlayersPerTeam < MinPlayersToStart)
            {
                errors.Add("MaxPlayersPerTeam cannot be less than MinPlayersToStart");
            }

            if (DifficultyLevel < 1 || DifficultyLevel > 10)
            {
                errors.Add("DifficultyLevel must be between 1 and 10");
            }

            return errors;
        }
    }

    /// <summary>
    /// Match mode for arena.
    /// </summary>
    public enum ArenaMatchMode
    {
        Duel,           // 1v1
        TeamDuel,       // 2v2, 3v3, etc.
        FreeForAll,     // Everyone vs Everyone
        CaptureTheFlag, // CTF variant
        KingOfTheHill,  // KOTH variant
        Survival,       // Last player standing
        WaveDefense,    // Survive waves of enemies
        BossRush        // Defeat bosses
    }
}
