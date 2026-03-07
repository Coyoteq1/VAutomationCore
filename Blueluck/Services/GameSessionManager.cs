using System;
using System.Collections.Concurrent;
using System.Linq;
using BepInEx.Logging;
using Blueluck.Models;
using ProjectM.Network;
using Stunlock.Core;
using Unity.Entities;
using VAuto.Core;
using VAuto.Services.Interfaces;
using VAutomationCore.Services;

namespace Blueluck.Services
{
    public sealed class GameSessionManager : IService
    {
        private static readonly ManualLogSource _log = Logger.CreateLogSource("Blueluck.GameSession");
        private readonly ConcurrentDictionary<int, GameSession> _sessionsByZone = new();

        private static FlowRegistryService? FlowRegistry => Plugin.FlowRegistry?.IsInitialized == true ? Plugin.FlowRegistry : null;
        private static ZoneConfigService? ZoneConfig => Plugin.ZoneConfig?.IsInitialized == true ? Plugin.ZoneConfig : null;
        private static GamePresetService? Presets => Plugin.GamePresets?.IsInitialized == true ? Plugin.GamePresets : null;
        private static ReadyLobbyService? Lobby => Plugin.ReadyLobbies?.IsInitialized == true ? Plugin.ReadyLobbies : null;
        private static SessionTimerService? Timers => Plugin.SessionTimers?.IsInitialized == true ? Plugin.SessionTimers : null;
        private static ZonePrepService? Prep => Plugin.ZonePrep?.IsInitialized == true ? Plugin.ZonePrep : null;
        private static ZoneTransitionService? ZoneTransition => Plugin.ZoneTransition?.IsInitialized == true ? Plugin.ZoneTransition : null;
        private static SessionOutcomeService? Outcomes => Plugin.SessionOutcomes?.IsInitialized == true ? Plugin.SessionOutcomes : null;

        public bool IsInitialized { get; private set; }
        public ManualLogSource Log => _log;

        public void Initialize()
        {
            IsInitialized = true;
            _log.LogInfo("[GameSession] Initialized.");
        }

        public void Cleanup()
        {
            foreach (var session in _sessionsByZone.Values)
            {
                CancelTimers(session);
            }

            _sessionsByZone.Clear();
            IsInitialized = false;
            _log.LogInfo("[GameSession] Cleaned up.");
        }

        public bool IsSessionEnabledZone(int zoneHash)
        {
            if (ZoneConfig?.TryGetZoneByHash(zoneHash, out var zone) != true || zone == null)
            {
                return false;
            }

            var definition = Presets?.Resolve(zone);
            return definition?.IsValid == true && definition.Session.Enabled;
        }

        public GameSession GetOrCreateSession(int zoneHash)
        {
            if (_sessionsByZone.TryGetValue(zoneHash, out var existing))
            {
                return existing;
            }

            if (ZoneConfig?.TryGetZoneByHash(zoneHash, out var zone) != true || zone == null)
            {
                throw new InvalidOperationException($"Zone {zoneHash} not found.");
            }

            var definition = Presets?.Resolve(zone) ?? new EffectiveSessionDefinition();
            var session = new GameSession
            {
                ZoneHash = zone.Hash,
                ZoneName = zone.Name,
                ZoneType = zone.Type,
                Definition = definition
            };

            _sessionsByZone[zoneHash] = session;
            _log.LogInfo($"[GameSession] Created session for zone {zoneHash} ({zone.Name}).");
            return session;
        }

        public bool TryGetSession(int zoneHash, out GameSession session)
        {
            return _sessionsByZone.TryGetValue(zoneHash, out session!);
        }

        public GameSession OnPlayerJoin(Entity player, int zoneHash, ulong steamId)
        {
            var session = GetOrCreateSession(zoneHash);
            var playerSession = session.Players.FirstOrDefault(x => x.SteamId == steamId);
            if (playerSession != null)
            {
                playerSession.Player = player;
                return session;
            }

            var canParticipate = CanParticipateNow(session);
            playerSession = new PlayerSession
            {
                Player = player,
                SteamId = steamId,
                IsParticipant = canParticipate,
                JoinedLate = session.State is GameSessionState.Countdown or GameSessionState.InProgress,
                WasLateJoinRejected = !canParticipate,
                IsAlive = true
            };

            session.Players.Add(playerSession);
            SendMessageToPlayer(player, canParticipate ? "Joined game lobby." : "Joined zone as observer for the current round.");

            var joinFlows = playerSession.JoinedLate && canParticipate
                ? session.Definition?.Lifecycle.LateJoinFlows
                : session.Definition?.Lifecycle.LobbyOpenFlows;
            if (joinFlows?.Length > 0)
            {
                Prep?.ExecuteFlows(session, joinFlows, player);
            }

            if (session.ReadyTimeoutEventId == null && session.Definition?.Session.ReadyTimeoutSeconds > 0)
            {
                session.ReadyTimeoutEventId = Timers?.Schedule(
                    session,
                    $"ready-timeout-{zoneHash}",
                    TimeSpan.FromSeconds(session.Definition.Session.ReadyTimeoutSeconds),
                    () => OnReadyTimeout(zoneHash));
            }

            ReevaluateSession(session);
            return session;
        }

        public void OnPlayerLeave(int zoneHash, ulong steamId)
        {
            if (!_sessionsByZone.TryGetValue(zoneHash, out var session))
            {
                return;
            }

            var playerSession = session.Players.FirstOrDefault(x => x.SteamId == steamId);
            if (playerSession == null)
            {
                return;
            }

            session.Players.Remove(playerSession);
            if (session.Players.Count == 0 && session.Definition?.Session.ResetOnEmpty == true)
            {
                AbortSession(zoneHash);
                return;
            }

            ReevaluateSession(session);
        }

        public bool SetPlayerReady(Entity player, ulong steamId, int zoneHash, bool ready)
        {
            if (!_sessionsByZone.TryGetValue(zoneHash, out var session))
            {
                return false;
            }

            if (session.State is GameSessionState.InProgress or GameSessionState.Ending or GameSessionState.Ended)
            {
                return false;
            }

            var ok = Lobby?.SetReady(session, player, steamId, ready) == true;
            if (!ok)
            {
                return false;
            }

            BroadcastToSession(session, $"Ready: {session.ReadyCount}/{session.ParticipantCount}");
            ReevaluateSession(session);
            return true;
        }

        public bool ForceStart(int zoneHash)
        {
            if (!_sessionsByZone.TryGetValue(zoneHash, out var session) || session.Definition == null)
            {
                return false;
            }

            if (session.ParticipantCount < session.Definition.Session.MinPlayers)
            {
                return false;
            }

            foreach (var player in session.Players.Where(x => x.IsParticipant))
            {
                player.IsReady = true;
            }

            StartCountdown(session);
            return true;
        }

        public bool Start(int zoneHash)
        {
            if (!IsSessionEnabledZone(zoneHash))
            {
                return false;
            }

            if (ZoneTransition?.GetPlayersInZone(zoneHash) is not { Count: > 0 } playersInZone)
            {
                return false;
            }

            foreach (var player in playersInZone)
            {
                if (!TryGetPlatformId(player, out var steamId))
                {
                    continue;
                }

                OnPlayerJoin(player, zoneHash, steamId);
            }

            return ForceStart(zoneHash);
        }

        public void EndGame(int zoneHash)
        {
            if (_sessionsByZone.TryGetValue(zoneHash, out var session))
            {
                EndSession(session);
            }
        }

        public void ResetToWaiting(int zoneHash)
        {
            if (_sessionsByZone.TryGetValue(zoneHash, out var session))
            {
                ResetSession(session);
            }
        }

        public void MarkPlayerDead(int zoneHash, Entity player)
        {
            if (!_sessionsByZone.TryGetValue(zoneHash, out var session))
            {
                return;
            }

            var current = session.Players.FirstOrDefault(x => x.Player == player);
            if (current == null)
            {
                return;
            }

            current.IsAlive = false;
            current.DeathTime = DateTime.UtcNow;
            EvaluateSessionOutcome(session);
        }

        public void HandleEntityDeath(Entity player)
        {
            foreach (var pair in _sessionsByZone)
            {
                var session = pair.Value;
                if (session.State != GameSessionState.InProgress)
                {
                    continue;
                }

                if (session.Players.Any(x => x.Player == player))
                {
                    MarkPlayerDead(pair.Key, player);
                    return;
                }
            }
        }

        public string GetSessionStatus(int zoneHash)
        {
            return _sessionsByZone.TryGetValue(zoneHash, out var session)
                ? $"[{session.State}] ready {session.ReadyCount}/{session.ParticipantCount}, round {session.RoundNumber}"
                : "No active session";
        }

        private void ReevaluateSession(GameSession session)
        {
            if (session.Definition == null || !session.Definition.IsValid || !session.Definition.Session.Enabled)
            {
                return;
            }

            if (session.State == GameSessionState.Countdown)
            {
                if (session.ParticipantCount < session.Definition.Session.MinPlayers || Lobby?.AreAllParticipantsReady(session) != true)
                {
                    CancelCountdown(session);
                }

                return;
            }

            if (session.State != GameSessionState.Waiting && session.State != GameSessionState.Ready)
            {
                return;
            }

            if (Lobby?.AreAllParticipantsReady(session) == true)
            {
                session.State = GameSessionState.Ready;
                if (session.Definition.Session.AutoStartWhenReady)
                {
                    StartCountdown(session);
                }
            }
            else
            {
                session.State = GameSessionState.Waiting;
            }
        }

        private void StartCountdown(GameSession session)
        {
            if (session.State == GameSessionState.Countdown || session.Definition == null)
            {
                return;
            }

            session.State = GameSessionState.Countdown;
            session.IsAdmissionLocked = string.Equals(session.ZoneType, "ArenaZone", StringComparison.OrdinalIgnoreCase);
            session.CountdownStartedAt = DateTime.UtcNow;

            foreach (var player in session.Players.Where(x => x.IsParticipant).Select(x => x.Player))
            {
                if (session.Definition.Lifecycle.PrepareFlows.Length > 0)
                {
                    Prep?.ExecuteFlows(session, session.Definition.Lifecycle.PrepareFlows, player);
                }

                if (session.Definition.Lifecycle.CountdownFlows.Length > 0)
                {
                    Prep?.ExecuteFlows(session, session.Definition.Lifecycle.CountdownFlows, player);
                }

                ApplyCountdownFreeze(session, player);
            }

            BroadcastToSession(session, $"Match starts in {session.Definition.Session.CountdownSeconds}s");
            Timers?.Cancel(session.CountdownEventId);
            session.CountdownEventId = null;
            session.CountdownEventId = Timers?.Schedule(
                session,
                $"countdown-{session.ZoneHash}",
                TimeSpan.FromSeconds(session.Definition.Session.CountdownSeconds),
                () => StartGame(session.ZoneHash));
        }

        private void CancelCountdown(GameSession session)
        {
            Timers?.Cancel(session.CountdownEventId);
            session.CountdownEventId = null;
            if (session.State == GameSessionState.Countdown)
            {
                foreach (var player in session.Players.Where(x => x.IsParticipant).Select(x => x.Player))
                {
                    RemoveCountdownFreeze(session, player);
                }

                session.State = GameSessionState.Waiting;
                session.IsAdmissionLocked = false;
                BroadcastToSession(session, "Countdown cancelled.");
            }
        }

        private void StartGame(int zoneHash)
        {
            if (!_sessionsByZone.TryGetValue(zoneHash, out var session) || session.Definition == null)
            {
                return;
            }

            session.State = GameSessionState.InProgress;
            session.StartedAt = DateTime.UtcNow;
            session.IsAdmissionLocked = string.Equals(session.ZoneType, "ArenaZone", StringComparison.OrdinalIgnoreCase);

            foreach (var player in session.Players.Where(x => x.IsParticipant).Select(x => x.Player))
            {
                RemoveCountdownFreeze(session, player);

                if (session.Definition.Lifecycle.StartFlows.Length > 0)
                {
                    Prep?.ExecuteFlows(session, session.Definition.Lifecycle.StartFlows, player);
                }
            }

            if (session.Definition.Session.MatchDurationSeconds > 0)
            {
                session.MatchDurationEventId = Timers?.Schedule(
                    session,
                    $"match-duration-{zoneHash}",
                    TimeSpan.FromSeconds(session.Definition.Session.MatchDurationSeconds),
                    () => EndSession(session));
            }

            BroadcastToSession(session, "Match started.");
        }

        private void EndSession(GameSession session)
        {
            if (session.State is GameSessionState.Ending or GameSessionState.Ended)
            {
                return;
            }

            session.State = GameSessionState.Ending;
            CancelTimers(session);

            foreach (var player in session.Players.Where(x => x.IsParticipant).Select(x => x.Player))
            {
                RemoveCountdownFreeze(session, player);

                if (session.Definition?.Lifecycle.EndFlows.Length > 0)
                {
                    Prep?.ExecuteFlows(session, session.Definition.Lifecycle.EndFlows, player);
                }
            }

            session.State = GameSessionState.Ended;
            session.ResetEventId = Timers?.Schedule(
                session,
                $"reset-{session.ZoneHash}",
                TimeSpan.FromSeconds(session.Definition?.Session.PostMatchResetDelaySeconds ?? 1),
                () => ResetSession(session));
        }

        private void ResetSession(GameSession session)
        {
            CancelTimers(session);

            foreach (var player in session.Players)
            {
                player.IsReady = false;
                player.IsAlive = true;
                player.DeathTime = null;
            }

            foreach (var player in session.Players.Where(x => x.IsParticipant).Select(x => x.Player))
            {
                RemoveCountdownFreeze(session, player);

                if (session.Definition?.Lifecycle.ResetFlows.Length > 0)
                {
                    Prep?.ExecuteFlows(session, session.Definition.Lifecycle.ResetFlows, player);
                }
            }

            Prep?.ClearSessionRuntime(session.SessionId);
            session.State = GameSessionState.Waiting;
            session.IsAdmissionLocked = false;
            session.CountdownStartedAt = null;
            session.StartedAt = null;
            session.RoundNumber++;
        }

        private void AbortSession(int zoneHash)
        {
            if (_sessionsByZone.TryRemove(zoneHash, out var session))
            {
                CancelTimers(session);
                Prep?.ClearSessionRuntime(session.SessionId);
            }
        }

        private void OnReadyTimeout(int zoneHash)
        {
            if (!_sessionsByZone.TryGetValue(zoneHash, out var session))
            {
                return;
            }

            foreach (var player in session.Players)
            {
                player.IsReady = false;
            }

            session.ReadyTimeoutEventId = null;
            BroadcastToSession(session, "Lobby ready timeout expired.");
            ReevaluateSession(session);
        }

        private void EvaluateSessionOutcome(GameSession session)
        {
            if (session.State != GameSessionState.InProgress || session.Definition == null)
            {
                return;
            }

            if (string.Equals(session.ZoneType, "ArenaZone", StringComparison.OrdinalIgnoreCase)
                && Outcomes?.ShouldEndArena(session) == true)
            {
                ApplyOutcomeFlows(session);
                if (session.Definition.Objective.EndMatchOnObjective)
                {
                    EndSession(session);
                }
            }
        }

        private void ApplyOutcomeFlows(GameSession session)
        {
            if (session.Definition == null)
            {
                return;
            }

            var winner = session.Players.FirstOrDefault(x => x.IsParticipant && x.IsAlive);
            foreach (var player in session.Players.Where(x => x.IsParticipant))
            {
                var flows = winner != null && player.SteamId == winner.SteamId
                    ? session.Definition.Lifecycle.VictoryFlows
                    : session.Definition.Lifecycle.DefeatFlows;
                if (flows.Length > 0)
                {
                    Prep?.ExecuteFlows(session, flows, player.Player);
                }
            }
        }

        private bool CanParticipateNow(GameSession session)
        {
            if (session.Definition == null)
            {
                return false;
            }

            if (session.State is GameSessionState.Waiting or GameSessionState.Ready)
            {
                return true;
            }

            if (string.Equals(session.ZoneType, "BossZone", StringComparison.OrdinalIgnoreCase)
                && session.Definition.Session.AllowLateJoin
                && session.StartedAt.HasValue)
            {
                return DateTime.UtcNow - session.StartedAt.Value
                    <= TimeSpan.FromSeconds(session.Definition.Session.LateJoinGraceSeconds);
            }

            return false;
        }

        private void CancelTimers(GameSession session)
        {
            Timers?.Cancel(session.ReadyTimeoutEventId);
            Timers?.Cancel(session.CountdownEventId);
            Timers?.Cancel(session.MatchDurationEventId);
            Timers?.Cancel(session.ResetEventId);
            session.ReadyTimeoutEventId = null;
            session.CountdownEventId = null;
            session.MatchDurationEventId = null;
            session.ResetEventId = null;
        }

        private void BroadcastToSession(GameSession session, string message)
        {
            if (FlowRegistry == null)
            {
                return;
            }

            foreach (var player in session.Players.Select(x => x.Player))
            {
                FlowRegistry.SendMessage(player, message, session.ZoneHash);
            }
        }

        private void SendMessageToPlayer(Entity player, string message)
        {
            FlowRegistry?.SendMessage(player, message);
        }

        private static bool TryGetPlatformId(Entity characterEntity, out ulong platformId)
        {
            platformId = 0;

            try
            {
                var em = VAutomationCore.Core.UnifiedCore.EntityManager;
                if (em == default || characterEntity == Entity.Null || !em.Exists(characterEntity) || !em.HasComponent<ProjectM.PlayerCharacter>(characterEntity))
                {
                    return false;
                }

                var playerCharacter = em.GetComponentData<ProjectM.PlayerCharacter>(characterEntity);
                if (playerCharacter.UserEntity == Entity.Null || !em.Exists(playerCharacter.UserEntity) || !em.HasComponent<User>(playerCharacter.UserEntity))
                {
                    return false;
                }

                platformId = em.GetComponentData<User>(playerCharacter.UserEntity).PlatformId;
                return platformId != 0;
            }
            catch
            {
                return false;
            }
        }

        private static void ApplyCountdownFreeze(GameSession session, Entity player)
        {
            var freezePrefab = session.Definition?.Session.CountdownFreezeBuffPrefab;
            if (session.Definition?.Session.FreezeDuringCountdown != true || string.IsNullOrWhiteSpace(freezePrefab))
            {
                return;
            }

            if (!TryResolveUserEntity(player, out var userEntity) || !PrefabGuidConverter.TryGetGuid(freezePrefab, out var buffGuid) || buffGuid == PrefabGUID.Empty)
            {
                return;
            }

            GameActionService.InvokeAction("applybuff", new object[] { userEntity, player, buffGuid, -1f });
        }

        private static void RemoveCountdownFreeze(GameSession session, Entity player)
        {
            var freezePrefab = session.Definition?.Session.CountdownFreezeBuffPrefab;
            if (string.IsNullOrWhiteSpace(freezePrefab) || !PrefabGuidConverter.TryGetGuid(freezePrefab, out var buffGuid) || buffGuid == PrefabGUID.Empty)
            {
                return;
            }

            GameActionService.InvokeAction("removebuff", new object[] { player, buffGuid });
        }

        private static bool TryResolveUserEntity(Entity characterEntity, out Entity userEntity)
        {
            userEntity = Entity.Null;

            try
            {
                var em = VAutomationCore.Core.UnifiedCore.EntityManager;
                if (em == default || characterEntity == Entity.Null || !em.Exists(characterEntity) || !em.HasComponent<ProjectM.PlayerCharacter>(characterEntity))
                {
                    return false;
                }

                var playerCharacter = em.GetComponentData<ProjectM.PlayerCharacter>(characterEntity);
                if (playerCharacter.UserEntity == Entity.Null || !em.Exists(playerCharacter.UserEntity))
                {
                    return false;
                }

                userEntity = playerCharacter.UserEntity;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
