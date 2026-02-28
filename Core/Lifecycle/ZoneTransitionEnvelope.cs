using System;
using Unity.Entities;

namespace VAutomationCore.Core.Lifecycle
{
    public readonly struct ZoneTransitionEnvelope
    {
        public Entity Player { get; }
        public int OldZoneHash { get; }
        public int NewZoneHash { get; }
        public string OldZoneId { get; }
        public string NewZoneId { get; }
        public DateTime OccurredUtc { get; }
        public string Source { get; }
        public EntityManager EntityManager { get; }

        public ZoneTransitionEnvelope(
            Entity player,
            int oldZoneHash,
            int newZoneHash,
            string oldZoneId,
            string newZoneId,
            DateTime occurredUtc,
            string source,
            EntityManager entityManager)
        {
            Player = player;
            OldZoneHash = oldZoneHash;
            NewZoneHash = newZoneHash;
            OldZoneId = oldZoneId ?? string.Empty;
            NewZoneId = newZoneId ?? string.Empty;
            OccurredUtc = occurredUtc;
            Source = source ?? string.Empty;
            EntityManager = entityManager;
        }
    }
}
