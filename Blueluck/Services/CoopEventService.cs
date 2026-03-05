using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using ProjectM;
using Stunlock.Core;
using Unity.Collections;
using Unity.Entities;
using VAuto.Services.Interfaces;
using VAutomationCore.Services;

namespace Blueluck.Services
{
    /// <summary>
    /// Automatic co-op event system that triggers co-op mode based on:
    /// - Player count threshold in a zone
    /// - Time-based intervals
    /// - Manual trigger via commands
    /// </summary>
    public sealed class CoopEventService : IService
    {
        private static readonly ManualLogSource _log = Logger.CreateLogSource("Blueluck.CoopEvent");

        public bool IsInitialized { get; private set; }
        public ManualLogSource Log => _log;

        private BossCoopService? _bossCoop;
        private PrefabToGuidService? _prefabToGuid;
        private bool _enabled;
        private int _minPlayersForAutoCoop;
        private int _cooldownSeconds;
        private int _checkIntervalMs;
        private DateTime _lastEventTime = DateTime.MinValue;
        private int _lastZoneHash;
        private bool _debugMode;

        // Track active co-op events by zone
        private readonly Dictionary<int, CoopEventState> _activeEvents = new();

        public sealed class CoopEventState
        {
            public int PlayerCount;
            public DateTime StartTime;
            public bool CoopEnabled;
        }

        public void Initialize()
        {
            IsInitialized = true;
            _bossCoop = Plugin.BossCoop;
            _prefabToGuid = Plugin.PrefabToGuid;

            LoadConfig();

            _log.LogInfo($"[CoopEvent] Initialized. AutoCoop: {_enabled}, MinPlayers: {_minPlayersForAutoCoop}, Cooldown: {_cooldownSeconds}s");
        }

        private void LoadConfig()
        {
            var config = Plugin.Instance?.Config;
            if (config == null)
            {
                _enabled = true;
                _minPlayersForAutoCoop = 2;
                _cooldownSeconds = 300; // 5 minutes
                _checkIntervalMs = 5000; // 5 seconds
                _debugMode = false;
                return;
            }

            _enabled = config.Bind("CoopEvents", "Enabled", true, "Enable automatic co-op events").Value;
            _minPlayersForAutoCoop = config.Bind("CoopEvents", "MinPlayers", 2, "Minimum players in zone to trigger auto co-op").Value;
            _cooldownSeconds = config.Bind("CoopEvents", "CooldownSeconds", 300, "Cooldown between auto co-op events (seconds)").Value;
            _checkIntervalMs = config.Bind("CoopEvents", "CheckIntervalMs", 5000, "Check interval for auto co-op (milliseconds)").Value;
            _debugMode = config.Bind("CoopEvents", "DebugMode", false, "Enable debug logging for co-op events").Value;
        }

        public void Cleanup()
        {
            _activeEvents.Clear();
            _lastEventTime = DateTime.MinValue;
            IsInitialized = false;
            _log.LogInfo("[CoopEvent] Cleaned up.");
        }

        /// <summary>
        /// Check if auto co-op should trigger for a zone based on player count.
        /// </summary>
        public void CheckAutoCoop(int zoneHash, IEnumerable<Entity> playersInZone)
        {
            if (!IsInitialized || !_enabled || _bossCoop == null)
            {
                return;
            }

            if (playersInZone == null)
            {
                return;
            }

            var players = playersInZone.ToList();
            var playerCount = players.Count;

            // Check if we meet the minimum player threshold
            if (playerCount < _minPlayersForAutoCoop)
            {
                if (_debugMode && _activeEvents.TryGetValue(zoneHash, out var existingState) && existingState.CoopEnabled)
                {
                    _log.LogDebug($"[CoopEvent] Zone {zoneHash}: Not enough players ({playerCount} < {_minPlayersForAutoCoop}), disabling co-op");
                }
                
                // Disable co-op if we dropped below threshold
                if (_activeEvents.TryGetValue(zoneHash, out var state) && state.CoopEnabled)
                {
                    DisableCoopForZone(zoneHash);
                }
                return;
            }

            // Check cooldown
            var now = DateTime.UtcNow;
            if ((now - _lastEventTime).TotalSeconds < _cooldownSeconds && _lastZoneHash != zoneHash)
            {
                if (_debugMode)
                {
                    _log.LogDebug($"[CoopEvent] Zone {zoneHash}: Cooldown active, skipping");
                }
                return;
            }

            // Check if co-op is already active for this zone
            if (_activeEvents.TryGetValue(zoneHash, out var activeState) && activeState.CoopEnabled)
            {
                if (_debugMode)
                {
                    _log.LogDebug($"[CoopEvent] Zone {zoneHash}: Co-op already active with {playerCount} players");
                }
                
                // Update player count
                activeState.PlayerCount = playerCount;
                return;
            }

            // Trigger auto co-op!
            TriggerAutoCoop(zoneHash, players);
        }

        private void TriggerAutoCoop(int zoneHash, List<Entity> players)
        {
            if (_bossCoop == null)
            {
                return;
            }

            var now = DateTime.UtcNow;
            _lastEventTime = now;
            _lastZoneHash = zoneHash;

            // Enable co-op for all players in the zone
            foreach (var player in players)
            {
                _bossCoop.OnBossZoneEnter(player, zoneHash, forceJoinClan: false, shuffleClan: false);
            }

            // Track the event
            _activeEvents[zoneHash] = new CoopEventState
            {
                PlayerCount = players.Count,
                StartTime = now,
                CoopEnabled = true
            };

            var msg = $"🤝 AUTO CO-OP: {players.Count} players teamed up!";
            BroadcastMessage(msg);

            _log.LogInfo($"[CoopEvent] Auto co-op triggered in zone {zoneHash} with {players.Count} players");
        }

        private void DisableCoopForZone(int zoneHash)
        {
            if (_bossCoop == null || !_activeEvents.TryGetValue(zoneHash, out var state))
            {
                return;
            }

            state.CoopEnabled = false;
            _log.LogInfo($"[CoopEvent] Auto co-op disabled in zone {zoneHash} (player count dropped)");
        }

        /// <summary>
        /// Manually trigger a co-op event for players in a zone.
        /// </summary>
        public void TriggerCoopEvent(int zoneHash, IEnumerable<Entity> players, bool forceJoinClan = false, bool shuffleClan = false)
        {
            if (!IsInitialized || _bossCoop == null || players == null)
            {
                return;
            }

            var playerList = players.ToList();
            if (playerList.Count == 0)
            {
                _log.LogWarning("[CoopEvent] No players provided for co-op event");
                return;
            }

            // Enable co-op for all players
            foreach (var player in playerList)
            {
                _bossCoop.OnBossZoneEnter(player, zoneHash, forceJoinClan, shuffleClan);
            }

            // Track the event
            _activeEvents[zoneHash] = new CoopEventState
            {
                PlayerCount = playerList.Count,
                StartTime = DateTime.UtcNow,
                CoopEnabled = true
            };

            var clanMsg = forceJoinClan ? " and joined the same clan!" : "!";
            var msg = $"🤝 CO-OP EVENT: {playerList.Count} players teamed up{clanMsg}";
            BroadcastMessage(msg);

            _log.LogInfo($"[CoopEvent] Co-op event triggered in zone {zoneHash} with {playerList.Count} players (clan: {forceJoinClan})");
        }

        /// <summary>
        /// End a co-op event for a specific zone.
        /// </summary>
        public void EndCoopEvent(int zoneHash, Entity? exitingPlayer = null)
        {
            if (!IsInitialized || _bossCoop == null)
            {
                return;
            }

            if (exitingPlayer.HasValue)
            {
                _bossCoop.OnBossZoneExit(exitingPlayer.Value, zoneHash);
            }

            if (_activeEvents.TryGetValue(zoneHash, out var state))
            {
                state.CoopEnabled = false;
                BroadcastMessage("💔 CO-OP EVENT ENDED");
                _log.LogInfo($"[CoopEvent] Co-op event ended in zone {zoneHash}");
            }
        }

        /// <summary>
        /// Get current co-op event status for all zones.
        /// </summary>
        public Dictionary<int, CoopEventState> GetActiveEvents()
        {
            return new Dictionary<int, CoopEventState>(_activeEvents);
        }

        /// <summary>
        /// Check if a specific zone has an active co-op event.
        /// </summary>
        public bool HasActiveCoop(int zoneHash)
        {
            return _activeEvents.TryGetValue(zoneHash, out var state) && state.CoopEnabled;
        }

        /// <summary>
        /// Reset cooldown to allow immediate re-trigger.
        /// </summary>
        public void ResetCooldown()
        {
            _lastEventTime = DateTime.MinValue;
            _log.LogInfo("[CoopEvent] Cooldown reset");
        }

        private void BroadcastMessage(string message)
        {
            try
            {
                if (!VAuto.Core.Chat.ChatService.TryBroadcastSystemMessage(message, out var error))
                {
                    _log.LogWarning($"[CoopEvent] Failed to broadcast message: {error}");
                }
            }
            catch (Exception ex)
            {
                _log.LogWarning($"[CoopEvent] Failed to broadcast message: {ex.Message}");
            }
        }

        /// <summary>
        /// Force enable co-op for all online players (global co-op event).
        /// </summary>
        public void EnableGlobalCoop(bool forceJoinClan = false)
        {
            if (!IsInitialized || _bossCoop == null)
            {
                return;
            }

            var em = VAutomationCore.Core.UnifiedCore.EntityManager;
            if (em == default)
            {
                _log.LogWarning("[CoopEvent] EntityManager not available");
                return;
            }

            var query = em.CreateEntityQuery(ComponentType.ReadOnly<PlayerCharacter>());
            var playerEntities = query.ToEntityArray(Allocator.Temp);
            var playerList = new List<Entity>(playerEntities.Length);
            for (int i = 0; i < playerEntities.Length; i++)
            {
                playerList.Add(playerEntities[i]);
            }
            playerEntities.Dispose();

            if (playerList.Count == 0)
            {
                _log.LogWarning("[CoopEvent] No players online for global co-op");
                return;
            }

            var globalZoneHash = "global_coop".GetHashCode();
            
            foreach (var player in playerList)
            {
                _bossCoop.OnBossZoneEnter(player, globalZoneHash, forceJoinClan, false);
            }

            _activeEvents[globalZoneHash] = new CoopEventState
            {
                PlayerCount = playerList.Count,
                StartTime = DateTime.UtcNow,
                CoopEnabled = true
            };

            var msg = forceJoinClan 
                ? $"🤝 GLOBAL CO-OP: All {playerList.Count} players united in one clan!" 
                : $"🤝 GLOBAL CO-OP: All {playerList.Count} players are now cooperating!";
            BroadcastMessage(msg);

            _log.LogInfo($"[CoopEvent] Global co-op enabled with {playerList.Count} players (clan: {forceJoinClan})");
        }

        /// <summary>
        /// Disable global co-op for all players.
        /// </summary>
        public void DisableGlobalCoop()
        {
            if (!IsInitialized || _bossCoop == null)
            {
                return;
            }

            var globalZoneHash = "global_coop".GetHashCode();
            EndCoopEvent(globalZoneHash);
            
            BroadcastMessage("💔 GLOBAL CO-OP ENDED");
            _log.LogInfo("[CoopEvent] Global co-op disabled");
        }
    }
}
