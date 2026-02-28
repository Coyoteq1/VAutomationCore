using System;
using System.Collections.Generic;
using ProjectM;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using VAutomationCore.Core.ECS;
using VAutomationCore.Core.ECS.Components;

namespace VAuto.Zone.Systems
{
    public partial class ZoneDetectionSystem : SystemBase
    {
        private EntityQuery _playerQuery;
        private EntityQuery _zoneQuery;
        private int _updateCounter;
        private float _nextDetectionTickTime;

        private struct ZoneCandidate
        {
            public Entity Entity;
            public ZoneComponent Zone;
        }

        public override void OnCreate()
        {
            _playerQuery = GetEntityQuery(
                ComponentType.ReadOnly<LocalToWorld>(),
                ComponentType.ReadOnly<PlayerCharacter>());
            _zoneQuery = GetEntityQuery(ComponentType.ReadOnly<ZoneComponent>());
            RequireForUpdate<ZoneComponent>();
        }

        public override void OnUpdate()
        {
            var now = UnityEngine.Time.realtimeSinceStartup;
            if (now < _nextDetectionTickTime)
            {
                return;
            }
            _nextDetectionTickTime = now + Plugin.EcsDetectionTickSecondsValue;

            var em = EntityManager;
            var players = _playerQuery.ToEntityArray(Allocator.Temp);
            var zones = _zoneQuery.ToEntityArray(Allocator.Temp);

            try
            {
                var sortedZones = new List<ZoneCandidate>(zones.Length);
                for (var i = 0; i < zones.Length; i++)
                {
                    var zoneEntity = zones[i];
                    sortedZones.Add(new ZoneCandidate
                    {
                        Entity = zoneEntity,
                        Zone = em.GetComponentData<ZoneComponent>(zoneEntity)
                    });
                }

                sortedZones.Sort((a, b) => ZoneDetectionOrdering.Compare(a.Zone, b.Zone));

                var opCount = players.Length * sortedZones.Count;
                _updateCounter++;

                var threshold = Plugin.ZoneDetectionOpsWarningThresholdValue;
                if (Plugin.ZoneDetectionDebug && _updateCounter % 50 == 0)
                {
                    Plugin.Logger.LogInfo($"[BlueLock][ECS] ZoneDetection players={players.Length} zones={sortedZones.Count} ops~={opCount} tick={Plugin.EcsDetectionTickSecondsValue:F2}s");
                }

                if (threshold > 0 && opCount > threshold)
                {
                    Plugin.Logger.LogWarning($"[BlueLock][ECS] ZoneDetection workload high: ops~={opCount} (threshold={threshold}).");
                }

                foreach (var player in players)
                {
                    var pos = em.GetComponentData<LocalToWorld>(player).Position;
                    var state = em.HasComponent<EcsPlayerZoneState>(player)
                        ? em.GetComponentData<EcsPlayerZoneState>(player)
                        : new EcsPlayerZoneState { CurrentZoneHash = 0 };

                    var newZone = 0;

                    for (var i = 0; i < sortedZones.Count; i++)
                    {
                        var zone = sortedZones[i].Zone;
                        var distSq = math.distancesq(pos, zone.Center);

                        var inside = state.CurrentZoneHash == zone.ZoneHash
                            ? distSq <= zone.ExitRadiusSq
                            : distSq <= zone.EntryRadiusSq;

                        if (inside)
                        {
                            newZone = zone.ZoneHash;
                            break;
                        }
                    }

                    if (newZone != state.CurrentZoneHash)
                    {
                        var oldZoneHash = state.CurrentZoneHash;
                        EmitZoneTransition(em, player, oldZoneHash, newZone);
                        state.CurrentZoneHash = newZone;

                        if (em.HasComponent<EcsPlayerZoneState>(player))
                        {
                            em.SetComponentData(player, state);
                        }
                        else
                        {
                            em.AddComponentData(player, state);
                        }

                        if (Plugin.ZoneDetectionDebug)
                        {
                            var correlationId = $"{player.Index}:{DateTime.UtcNow.Ticks}";
                            Plugin.Logger.LogInfo($"[ZoneTransition][detect] cid={correlationId} player={player.Index} oldHash={oldZoneHash} newHash={newZone}");
                        }
                    }
                }
            }
            finally
            {
                players.Dispose();
                zones.Dispose();
            }
        }

        private static void EmitZoneTransition(EntityManager em, Entity player, int oldZone, int newZone)
        {
            var e = em.CreateEntity();
            em.AddComponentData(e, new ZoneTransitionEvent
            {
                Player = player,
                OldZoneHash = oldZone,
                NewZoneHash = newZone
            });
        }
    }
}
