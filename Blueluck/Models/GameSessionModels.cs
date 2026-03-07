using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;

namespace Blueluck.Models
{
    public enum GameSessionState
    {
        Waiting,
        Ready,
        Countdown,
        InProgress,
        Ending,
        Ended
    }

    public sealed class PlayerSession
    {
        public Entity Player { get; set; }
        public ulong SteamId { get; set; }
        public bool IsReady { get; set; }
        public bool IsAlive { get; set; } = true;
        public bool IsParticipant { get; set; } = true;
        public bool JoinedLate { get; set; }
        public bool WasLateJoinRejected { get; set; }
        public DateTime JoinTime { get; set; } = DateTime.UtcNow;
        public DateTime? DeathTime { get; set; }
    }

    public sealed class EffectiveSessionDefinition
    {
        public GameplayPresetConfig? SourcePreset { get; set; }
        public GameSessionConfig Session { get; set; } = new();
        public SessionLifecycleConfig Lifecycle { get; set; } = new();
        public GameObjectiveConfig Objective { get; set; } = new();
        public bool IsValid { get; set; } = true;
        public string? InvalidReason { get; set; }
    }

    public sealed class GameSession
    {
        public string SessionId { get; set; } = Guid.NewGuid().ToString("N");
        public int ZoneHash { get; set; }
        public string ZoneName { get; set; } = string.Empty;
        public string ZoneType { get; set; } = string.Empty;
        public GameSessionState State { get; set; } = GameSessionState.Waiting;
        public List<PlayerSession> Players { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? StartedAt { get; set; }
        public DateTime? CountdownStartedAt { get; set; }
        public int RoundNumber { get; set; } = 1;
        public EffectiveSessionDefinition? Definition { get; set; }
        public bool IsAdmissionLocked { get; set; }
        public string? ReadyTimeoutEventId { get; set; }
        public string? CountdownEventId { get; set; }
        public string? MatchDurationEventId { get; set; }
        public string? ResetEventId { get; set; }

        public int ReadyCount => Players.Count(x => x.IsParticipant && x.IsReady);
        public int ParticipantCount => Players.Count(x => x.IsParticipant);
        public int AliveParticipantCount => Players.Count(x => x.IsParticipant && x.IsAlive);
    }
}
