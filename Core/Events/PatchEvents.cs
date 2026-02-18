using ProjectM;
using Stunlock.Core;
using Unity.Entities;
using Unity.Mathematics;

namespace VAutomationCore.Core.Events
{
    public sealed class BuffInitializedEvent
    {
        public Entity Owner { get; init; }
        public Entity Source { get; init; }
        public PrefabGUID BuffGuid { get; init; }
        public float Duration { get; init; }
        public bool IsExtended { get; init; }
    }

    public sealed class BuffDestroyedEvent
    {
        public Entity Owner { get; init; }
        public PrefabGUID BuffGuid { get; init; }
    }

    public sealed class UnitSpawnedEvent
    {
        public Entity Spawner { get; init; }
        public Entity SpawnedUnit { get; init; }
        public PrefabGUID PrefabGuid { get; init; }
        public float3 Position { get; init; }
        public int Level { get; init; }
        public bool IsNightSpawn { get; init; }
    }

    public sealed class SpawnTravelBuffAppliedEvent
    {
        public Entity Unit { get; init; }
        public PrefabGUID PrefabGuid { get; init; }
        public float3 Position { get; init; }
        public bool IsMoving { get; init; }
    }

    public sealed class DeathOccurredEvent
    {
        public Entity Killer { get; init; }
        public Entity Victim { get; init; }
        public StatChangeReason Reason { get; init; }
        public bool IsPlayerKill { get; init; }
        public bool IsVBlood { get; init; }
    }

    public sealed class ServerStartedEvent
    {
    }

    public sealed class WorldReadyEvent
    {
    }

    public sealed class WorldInitializedEvent
    {
    }
}
