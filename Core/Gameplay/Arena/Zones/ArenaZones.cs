using System;
using System.Collections.Generic;
using System.Linq;
using VAutomationCore.Core.Gameplay;
using VAutomationCore.Core.Gameplay.Arena.Data;
using VAutomationCore.Core.Gameplay.Shared.Contracts;
using VAutomationCore.Core.Gameplay.Shared.Types;

namespace VAutomationCore.Core.Gameplay.Arena.Zones
{
    /// <summary>
    /// Arena zone management.
    /// This class is owned by the Arena module.
    /// </summary>
    public static class ArenaZones
    {
        private static readonly Dictionary<string, ArenaState> _arenaStates = new(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, ArenaZoneDefinition> _zoneDefinitions = new(StringComparer.OrdinalIgnoreCase);
        private static bool _isInitialized = false;

        /// <summary>
        /// Initialize arena zones.
        /// </summary>
        public static void Initialize()
        {
            _arenaStates.Clear();
            _zoneDefinitions.Clear();

            // Register default zone definitions
            foreach (var zone in ArenaZoneDefaults.GetDefaults())
            {
                RegisterZone(zone);
            }

            _isInitialized = true;
        }

        /// <summary>
        /// Register an arena zone definition.
        /// </summary>
        public static bool RegisterZone(ArenaZoneDefinition definition)
        {
            if (definition == null || string.IsNullOrWhiteSpace(definition.ZoneId))
            {
                return false;
            }

            var zoneId = definition.ZoneId.Trim();
            _zoneDefinitions[zoneId] = definition;

            // Also register with the shared zone registry
            ZoneRegistry.Register(definition.ToZoneDefinition(), replace: true);

            // Initialize arena state
            if (!_arenaStates.ContainsKey(zoneId))
            {
                _arenaStates[zoneId] = new ArenaState
                {
                    ArenaId = zoneId,
                    RuleProfileId = definition.DefaultRuleProfileId,
                    ZoneSettingsId = definition.DefaultZoneSettingsId,
                    MatchMode = definition.DefaultMatchMode,
                    IsEnabled = definition.Enabled
                };
            }

            return true;
        }

        /// <summary>
        /// Get arena zone definition.
        /// </summary>
        public static ArenaZoneDefinition? GetZone(string zoneId)
        {
            return _zoneDefinitions.TryGetValue(zoneId, out var zone) ? zone : null;
        }

        /// <summary>
        /// Get all arena zone definitions.
        /// </summary>
        public static IReadOnlyList<ArenaZoneDefinition> GetAllZones()
        {
            return _zoneDefinitions.Values.ToList();
        }

        /// <summary>
        /// Get arena runtime state.
        /// </summary>
        public static ArenaState? GetState(string arenaId)
        {
            return _arenaStates.TryGetValue(arenaId, out var state) ? state : null;
        }

        /// <summary>
        /// Get all arena states.
        /// </summary>
        public static IReadOnlyCollection<ArenaState> GetAllStates()
        {
            return _arenaStates.Values;
        }

        /// <summary>
        /// Enable an arena.
        /// </summary>
        public static bool EnableArena(string arenaId)
        {
            if (!_arenaStates.TryGetValue(arenaId, out var state))
            {
                return false;
            }

            state.IsEnabled = true;
            state.State = ArenaStateType.Waiting;
            ZoneRegistry.EnableZone(arenaId);
            return true;
        }

        /// <summary>
        /// Disable an arena.
        /// </summary>
        public static bool DisableArena(string arenaId)
        {
            if (!_arenaStates.TryGetValue(arenaId, out var state))
            {
                return false;
            }

            state.IsEnabled = false;
            state.State = ArenaStateType.Inactive;
            ZoneRegistry.DisableZone(arenaId);
            return true;
        }

        /// <summary>
        /// Create a new arena.
        /// </summary>
        public static bool CreateArena(ArenaZoneDefinition definition)
        {
            return RegisterZone(definition);
        }

        /// <summary>
        /// Delete an arena.
        /// </summary>
        public static bool DeleteArena(string arenaId)
        {
            if (!_zoneDefinitions.ContainsKey(arenaId))
            {
                return false;
            }

            _zoneDefinitions.Remove(arenaId);
            _arenaStates.Remove(arenaId);
            return true;
        }

        /// <summary>
        /// Start a match in an arena.
        /// </summary>
        public static bool StartMatch(string arenaId, ArenaMatchMode mode)
        {
            if (!_arenaStates.TryGetValue(arenaId, out var state))
            {
                return false;
            }

            if (!state.IsEnabled)
            {
                return false;
            }

            state.MatchMode = mode;
            state.State = ArenaStateType.Countdown;
            state.MatchStartTime = DateTime.UtcNow;
            state.CurrentWave = 1;

            return true;
        }

        /// <summary>
        /// Stop a match in an arena.
        /// </summary>
        public static bool StopMatch(string arenaId, string reason)
        {
            if (!_arenaStates.TryGetValue(arenaId, out var state))
            {
                return false;
            }

            state.State = ArenaStateType.Ending;
            state.LastError = reason;
            return true;
        }

        /// <summary>
        /// Add player to arena.
        /// </summary>
        public static bool AddPlayer(string arenaId, string playerId, int team)
        {
            if (!_arenaStates.TryGetValue(arenaId, out var state))
            {
                return false;
            }

            state.Players.Add(playerId);

            if (team > 0)
            {
                if (!state.TeamPlayers.ContainsKey(team))
                {
                    state.TeamPlayers[team] = new HashSet<string>();
                }
                state.TeamPlayers[team].Add(playerId);
            }

            return true;
        }

        /// <summary>
        /// Remove player from arena.
        /// </summary>
        public static bool RemovePlayer(string arenaId, string playerId)
        {
            if (!_arenaStates.TryGetValue(arenaId, out var state))
            {
                return false;
            }

            state.Players.Remove(playerId);
            state.Spectators.Remove(playerId);

            foreach (var team in state.TeamPlayers.Values)
            {
                team.Remove(playerId);
            }

            return true;
        }

        /// <summary>
        /// Get arena status.
        /// </summary>
        public static Shared.Contracts.ZoneStatus GetStatus(string arenaId)
        {
            if (!_arenaStates.TryGetValue(arenaId, out var state))
            {
                return new Shared.Contracts.ZoneStatus
                {
                    ZoneId = arenaId,
                    IsEnabled = false
                };
            }

            return new Shared.Contracts.ZoneStatus
            {
                ZoneId = arenaId,
                ZoneType = "arena",
                OwnerType = GameplayType.Arena,
                IsEnabled = state.IsEnabled,
                PlayerCount = state.PlayerCount,
                MatchState = state.State.ToString(),
                CreatedAt = DateTime.UtcNow,
                LastMatchAt = state.MatchStartTime
            };
        }

        /// <summary>
        /// List all arena IDs.
        /// </summary>
        public static IReadOnlyList<string> ListArenas()
        {
            return _zoneDefinitions.Keys.ToList();
        }

        /// <summary>
        /// Validate arena configurations.
        /// </summary>
        public static IReadOnlyList<string> Validate()
        {
            var errors = new List<string>();

            foreach (var zone in _zoneDefinitions.Values)
            {
                if (zone.MaxPlayers < zone.MinPlayers)
                {
                    errors.Add($"Arena '{zone.ZoneId}': MaxPlayers < MinPlayers");
                }

                if (zone.Radius <= 0)
                {
                    errors.Add($"Arena '{zone.ZoneId}': Invalid radius");
                }
            }

            return errors;
        }

        /// <summary>
        /// Check if initialized.
        /// </summary>
        public static bool IsInitialized => _isInitialized;
    }
}
