using System;
using System.Collections.Concurrent;
using System.Linq;
using BepInEx.Logging;
using Blueluck.Models;
using VAuto.Services.Interfaces;

namespace Blueluck.Services
{
    public sealed class GamePresetService : IService
    {
        private static readonly ManualLogSource _log = Logger.CreateLogSource("Blueluck.GamePreset");
        private readonly ConcurrentDictionary<int, EffectiveSessionDefinition> _cache = new();

        public bool IsInitialized { get; private set; }
        public ManualLogSource Log => _log;

        public void Initialize()
        {
            IsInitialized = true;
            _log.LogInfo("[GamePreset] Initialized.");
        }

        public void Cleanup()
        {
            _cache.Clear();
            IsInitialized = false;
            _log.LogInfo("[GamePreset] Cleaned up.");
        }

        public void InvalidateAll()
        {
            _cache.Clear();
        }

        public EffectiveSessionDefinition Resolve(ZoneDefinition zone)
        {
            return _cache.GetOrAdd(zone.Hash, _ => BuildDefinition(zone));
        }

        private static EffectiveSessionDefinition BuildDefinition(ZoneDefinition zone)
        {
            var definition = new EffectiveSessionDefinition();
            var basePreset = zone.ResolvedPresets?
                .FirstOrDefault(p => p.Session != null || p.SessionLifecycle != null || p.Objective != null)
                ?? new GameplayPresetConfig();

            definition.SourcePreset = basePreset;
            definition.Session = MergeSession(basePreset.Session, zone.Session);
            definition.Lifecycle = MergeLifecycle(basePreset.SessionLifecycle, zone.SessionLifecycle);
            definition.Objective = MergeObjective(basePreset.Objective, zone.Objective);

            if (!definition.Session.Enabled)
            {
                return definition;
            }

            if (string.Equals(definition.Objective.ObjectiveType, "last_player_standing", StringComparison.OrdinalIgnoreCase)
                && zone is ArenaZoneConfig arena
                && arena.RespawnEnabled)
            {
                definition.IsValid = false;
                definition.InvalidReason = "last_player_standing requires RespawnEnabled = false";
            }

            return definition;
        }

        private static GameSessionConfig MergeSession(GameSessionConfig? preset, GameSessionConfig? zone)
        {
            var merged = new GameSessionConfig();
            ApplySession(merged, preset);
            ApplySession(merged, zone);
            return merged;
        }

        private static SessionLifecycleConfig MergeLifecycle(SessionLifecycleConfig? preset, SessionLifecycleConfig? zone)
        {
            var merged = new SessionLifecycleConfig();
            ApplyLifecycle(merged, preset);
            ApplyLifecycle(merged, zone);
            return merged;
        }

        private static GameObjectiveConfig MergeObjective(GameObjectiveConfig? preset, GameObjectiveConfig? zone)
        {
            var merged = new GameObjectiveConfig();
            ApplyObjective(merged, preset);
            ApplyObjective(merged, zone);
            return merged;
        }

        private static void ApplySession(GameSessionConfig target, GameSessionConfig? source)
        {
            if (source == null) return;
            target.Enabled = source.Enabled;
            target.MinPlayers = source.MinPlayers;
            target.MaxPlayers = source.MaxPlayers;
            target.CountdownSeconds = source.CountdownSeconds;
            target.ReadyTimeoutSeconds = source.ReadyTimeoutSeconds;
            target.MatchDurationSeconds = source.MatchDurationSeconds;
            target.AutoStartWhenReady = source.AutoStartWhenReady;
            target.RequireAllPresentReady = source.RequireAllPresentReady;
            target.AllowLateJoin = source.AllowLateJoin;
            target.LateJoinGraceSeconds = source.LateJoinGraceSeconds;
            target.PostMatchResetDelaySeconds = source.PostMatchResetDelaySeconds;
            target.FreezeDuringCountdown = source.FreezeDuringCountdown;
            target.CountdownFreezeBuffPrefab = source.CountdownFreezeBuffPrefab;
            target.ResetOnEmpty = source.ResetOnEmpty;
        }

        private static void ApplyLifecycle(SessionLifecycleConfig target, SessionLifecycleConfig? source)
        {
            if (source == null) return;
            target.PrepareFlows = source.PrepareFlows?.ToArray() ?? Array.Empty<string>();
            target.LobbyOpenFlows = source.LobbyOpenFlows?.ToArray() ?? Array.Empty<string>();
            target.PlayerReadyFlows = source.PlayerReadyFlows?.ToArray() ?? Array.Empty<string>();
            target.PlayerUnreadyFlows = source.PlayerUnreadyFlows?.ToArray() ?? Array.Empty<string>();
            target.CountdownFlows = source.CountdownFlows?.ToArray() ?? Array.Empty<string>();
            target.StartFlows = source.StartFlows?.ToArray() ?? Array.Empty<string>();
            target.LateJoinFlows = source.LateJoinFlows?.ToArray() ?? Array.Empty<string>();
            target.VictoryFlows = source.VictoryFlows?.ToArray() ?? Array.Empty<string>();
            target.DefeatFlows = source.DefeatFlows?.ToArray() ?? Array.Empty<string>();
            target.EndFlows = source.EndFlows?.ToArray() ?? Array.Empty<string>();
            target.ResetFlows = source.ResetFlows?.ToArray() ?? Array.Empty<string>();
            target.TickFlows = source.TickFlows?.ToArray() ?? Array.Empty<string>();
        }

        private static void ApplyObjective(GameObjectiveConfig target, GameObjectiveConfig? source)
        {
            if (source == null) return;
            target.ObjectiveType = source.ObjectiveType;
            target.EndMatchOnObjective = source.EndMatchOnObjective;
            target.TreatTimeoutAsDraw = source.TreatTimeoutAsDraw;
        }
    }
}
