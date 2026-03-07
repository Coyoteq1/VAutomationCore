using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using BepInEx.Logging;
using Blueluck.Models;
using VAuto.Services.Interfaces;

namespace Blueluck.Services
{
    public sealed class SessionTimerService : IService
    {
        private static readonly ManualLogSource _log = Logger.CreateLogSource("Blueluck.SessionTimer");
        private readonly ConcurrentDictionary<string, ScheduledEvent> _timers = new();
        private readonly object _sync = new();

        private sealed class ScheduledEvent
        {
            public string EventId { get; init; } = string.Empty;
            public DateTime DueAtUtc { get; init; }
            public Action Callback { get; init; } = static () => { };
        }

        public bool IsInitialized { get; private set; }
        public ManualLogSource Log => _log;

        public void Initialize()
        {
            IsInitialized = true;
        }

        public void Cleanup()
        {
            _timers.Clear();
            IsInitialized = false;
        }

        public string? Schedule(GameSession session, string name, TimeSpan delay, Action callback)
        {
            if (!IsInitialized)
            {
                _log.LogWarning($"[SessionTimer] Schedule skipped: timer service not initialized. session={session?.SessionId ?? "<null>"} name={name}");
                return null;
            }

            var eventId = $"{session.SessionId}:{name}:{Guid.NewGuid():N}";
            var scheduled = new ScheduledEvent
            {
                EventId = eventId,
                DueAtUtc = DateTime.UtcNow + delay,
                Callback = callback
            };

            if (!_timers.TryAdd(eventId, scheduled))
            {
                _log.LogWarning($"[SessionTimer] Schedule failed: duplicate eventId={eventId}");
                return null;
            }

            _log.LogInfo($"[SessionTimer] Scheduled event name={name} eventId={eventId} dueAtUtc={scheduled.DueAtUtc:O}");
            return eventId;
        }

        public void Cancel(string? eventId)
        {
            if (!string.IsNullOrWhiteSpace(eventId))
            {
                if (_timers.TryRemove(eventId, out _))
                {
                    _log.LogInfo($"[SessionTimer] Cancelled event {eventId}");
                }
            }
        }

        public void ProcessTick()
        {
            if (!IsInitialized || _timers.IsEmpty)
            {
                return;
            }

            List<ScheduledEvent>? ready = null;
            var now = DateTime.UtcNow;

            lock (_sync)
            {
                foreach (var pair in _timers)
                {
                    if (pair.Value.DueAtUtc > now)
                    {
                        continue;
                    }

                    if (!_timers.TryRemove(pair.Key, out var timer))
                    {
                        continue;
                    }

                    ready ??= new List<ScheduledEvent>();
                    ready.Add(timer);
                }
            }

            if (ready == null)
            {
                return;
            }

            foreach (var timer in ready)
            {
                try
                {
                    _log.LogInfo($"[SessionTimer] Firing event {timer.EventId}");
                    timer.Callback();
                }
                catch (Exception ex)
                {
                    _log.LogError($"[SessionTimer] Scheduled callback failed: {ex.Message}");
                }
            }
        }
    }
}
