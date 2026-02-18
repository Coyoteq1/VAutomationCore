using System;
using System.Collections.Generic;

namespace VLifecycle.Services.Lifecycle
{
    public enum ScoreEventType
    {
        Kill,
        Death,
        ZoneEnter,
        ZoneExit,
        CoopEventJoin,
        CoopBossKill,
        TrapTrigger,
        TrapKillOwner,
        PvpEngage
    }

    public static class ScoreService
    {
        private static readonly object _lock = new();
        private static readonly Dictionary<ulong, float> _scores = new();
        private static readonly Dictionary<ulong, int> _killsWithoutDeath = new();

        private static readonly Dictionary<ScoreEventType, float> _basePoints = new()
        {
            [ScoreEventType.Kill] = 5f,
            [ScoreEventType.Death] = 0f,
            [ScoreEventType.ZoneEnter] = 0f,
            [ScoreEventType.ZoneExit] = 0f,
            [ScoreEventType.CoopEventJoin] = 3f,
            [ScoreEventType.CoopBossKill] = 10f,
            [ScoreEventType.TrapTrigger] = 1f,
            [ScoreEventType.TrapKillOwner] = 5f,
            [ScoreEventType.PvpEngage] = 4f
        };

        public static void AddEvent(ulong playerId, ScoreEventType eventType, bool inZone = false)
        {
            if (playerId == 0) return;

            lock (_lock)
            {
                var delta = _basePoints.TryGetValue(eventType, out var points) ? points : 0f;

                if (eventType == ScoreEventType.Kill || eventType == ScoreEventType.TrapKillOwner)
                {
                    delta += inZone ? 1f : 2f;
                }

                if (eventType == ScoreEventType.Kill)
                {
                    _killsWithoutDeath.TryGetValue(playerId, out var streak);
                    streak++;
                    _killsWithoutDeath[playerId] = streak;
                    if (streak % 3 == 0)
                    {
                        delta += 10f;
                    }
                }
                else if (eventType == ScoreEventType.Death)
                {
                    _killsWithoutDeath[playerId] = 0;
                }

                _scores.TryGetValue(playerId, out var current);
                _scores[playerId] = current + delta;
            }
        }

        public static void OnCoopEventJoin(ulong playerId, string zoneId)
        {
            AddEvent(playerId, ScoreEventType.CoopEventJoin, inZone: true);
        }

        public static void OnTrapKillOwner(ulong trapOwnerId, ulong victimId, bool inZone = false)
        {
            AddEvent(trapOwnerId, ScoreEventType.TrapKillOwner, inZone);
            AddEvent(victimId, ScoreEventType.Death);
        }

        public static float GetScore(ulong playerId)
        {
            lock (_lock)
            {
                return _scores.TryGetValue(playerId, out var score) ? score : 0f;
            }
        }
    }
}
