using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using ProjectM;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using VAuto.Services.Interfaces;
using Blueluck.Models;
using VAutomationCore.Core.Lifecycle;

namespace Blueluck.Services
{
    /// <summary>
    /// Service for handling zone enter/exit transitions.
    /// </summary>
    public class ZoneTransitionService : IService
    {
        private static readonly ManualLogSource _log = Logger.CreateLogSource("Blueluck.ZoneTransition");
        
        public bool IsInitialized { get; private set; }
        public ManualLogSource Log => _log;

        // Track players currently in zones - thread-safe for ECS systems
        private readonly ConcurrentDictionary<Entity, int> _playersInZones = new();
        // Track occupancy counts per zone hash so we can apply/remove zone-level effects once - thread-safe
        private readonly ConcurrentDictionary<int, int> _zoneOccupancy = new();

        private static ZoneConfigService? ZoneConfig => Plugin.ZoneConfig?.IsInitialized == true ? Plugin.ZoneConfig : null;
        private static FlowRegistryService? FlowRegistry => Plugin.FlowRegistry?.IsInitialized == true ? Plugin.FlowRegistry : null;
        private static GameSessionManager? GameSessions => Plugin.GameSessions?.IsInitialized == true ? Plugin.GameSessions : null;

        public void Initialize()
        {
            IsInitialized = true;
            _log.LogInfo("[ZoneTransition] Initialized.");
        }

        public void Cleanup()
        {
            _playersInZones.Clear();
            _zoneOccupancy.Clear();
            IsInitialized = false;
            _log.LogInfo("[ZoneTransition] Cleaned up.");
        }

        /// <summary>
        /// Called when a player enters a zone.
        /// </summary>
        public void OnZoneEnter(Entity player, ZoneDefinition zone)
        {
            try
            {
                _log.LogInfo($"[ZoneTransition] Player {player.Index} entering zone: {zone.Name} ({zone.Type})");

                // Track player in zone
                _playersInZones[player] = zone.Hash;
                _zoneOccupancy[zone.Hash] = _zoneOccupancy.TryGetValue(zone.Hash, out var count) ? count + 1 : 1;
                var sessionEnabled = GameSessions?.IsSessionEnabledZone(zone.Hash) == true;

                var flowRegistry = FlowRegistry;
                if (flowRegistry != null)
                {
                    flowRegistry.ExecuteFlows(zone.ResolvedEntryFlows, player, zone.Name, zone.Hash);
                }

                if (sessionEnabled && TryGetPlatformId(player, out var platformId))
                {
                    GameSessions?.OnPlayerJoin(player, zone.Hash, platformId);
                }

                // Zone message (player-scoped; "broadcast" is treated as a label, not a server-wide broadcast)
                if (zone.OnEnter != null)
                {
                    var message = !string.IsNullOrEmpty(zone.OnEnter.Message) 
                        ? zone.OnEnter.Message 
                        : zone.OnEnter.Broadcast;
                    if (!string.IsNullOrEmpty(message))
                    {
                        if (flowRegistry != null)
                        {
                            flowRegistry.SendMessage(player, message, zone.Hash);
                        }
                        else
                        {
                            _log.LogInfo($"[ZoneTransition] Message: {message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.LogError($"[ZoneTransition] Error on zone enter: {ex.Message}");
            }
        }

        /// <summary>
        /// Called when a player exits a zone.
        /// </summary>
        public void OnZoneExit(Entity player, ZoneDefinition zone)
        {
            try
            {
                _log.LogInfo($"[ZoneTransition] Player {player.Index} exiting zone: {zone.Name} ({zone.Type})");

                // Remove from tracking
                _playersInZones.TryRemove(player, out _);
                if (_zoneOccupancy.TryGetValue(zone.Hash, out var count))
                {
                    var newCount = count - 1;
                    if (newCount <= 0) 
                        _zoneOccupancy.TryRemove(zone.Hash, out _);
                    else
                        _zoneOccupancy[zone.Hash] = newCount;
                }

                var sessionEnabled = GameSessions?.IsSessionEnabledZone(zone.Hash) == true;
                if (sessionEnabled && TryGetPlatformId(player, out var platformId))
                {
                    GameSessions?.OnPlayerLeave(zone.Hash, platformId);
                }

                var flowRegistry = FlowRegistry;
                if (flowRegistry != null)
                {
                    flowRegistry.ExecuteFlows(zone.ResolvedExitFlows, player, zone.Name, zone.Hash);
                }

                // Zone message (player-scoped)
                if (zone.OnExit != null)
                {
                    var message = !string.IsNullOrEmpty(zone.OnExit.Message) 
                        ? zone.OnExit.Message 
                        : zone.OnExit.Broadcast;
                    if (!string.IsNullOrEmpty(message))
                    {
                        if (flowRegistry != null)
                        {
                            flowRegistry.SendMessage(player, message, zone.Hash);
                        }
                        else
                        {
                            _log.LogInfo($"[ZoneTransition] Message: {message}");
                        }
                    }
                }

                if (!_zoneOccupancy.ContainsKey(zone.Hash))
                {
                    ZoneConfig?.ReleaseRetiredZone(zone.Hash);
                }
            }
            catch (Exception ex)
            {
                _log.LogError($"[ZoneTransition] Error on zone exit: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the current zone hash for a player.
        /// </summary>
        public int GetPlayerZone(Entity player)
        {
            if (_playersInZones.TryGetValue(player, out var hash) && hash != 0)
            {
                return hash;
            }

            return ResolveZoneHashByPosition(player);
        }

        /// <summary>
        /// Checks if a player is in a specific zone type.
        /// </summary>
        public bool IsPlayerInZoneType(Entity player, string zoneType)
        {
            if (!_playersInZones.TryGetValue(player, out var hash))
                return false;

            if (ZoneConfig?.TryGetZoneByHash(hash, out var zone) == true)
                return zone.Type == zoneType;

            return false;
        }

        public List<Entity> GetPlayersInZone(int zoneHash)
        {
            var players = new List<Entity>();
            foreach (var pair in _playersInZones)
            {
                if (pair.Value == zoneHash)
                {
                    players.Add(pair.Key);
                }
            }

            if (players.Count > 0)
            {
                return players;
            }

            try
            {
                var em = VAutomationCore.Core.UnifiedCore.EntityManager;
                if (em == default)
                {
                    return players;
                }

                var query = em.CreateEntityQuery(ComponentType.ReadOnly<PlayerCharacter>());
                var playerEntities = query.ToEntityArray(Unity.Collections.Allocator.Temp);
                try
                {
                    foreach (var player in playerEntities)
                    {
                        if (ResolveZoneHashByPosition(player) == zoneHash)
                        {
                            players.Add(player);
                        }
                    }
                }
                finally
                {
                    playerEntities.Dispose();
                }
            }
            catch
            {
                // ignored
            }

            return players;
        }

        private int ResolveZoneHashByPosition(Entity player)
        {
            try
            {
                var em = VAutomationCore.Core.UnifiedCore.EntityManager;
                if (em == default || player == Entity.Null || !em.Exists(player) || ZoneConfig == null)
                {
                    return 0;
                }

                if (!TryGetBestPosition(em, player, out var position))
                {
                    return 0;
                }

                var currentHash = _playersInZones.TryGetValue(player, out var cachedHash) ? cachedHash : 0;
                var zones = ZoneConfig.GetZones()
                    .OrderByDescending(z => z.Priority)
                    .ThenByDescending(z => z.EntryRadiusSq)
                    .ThenBy(z => z.Hash);

                foreach (var zone in zones)
                {
                    var distSq = math.distancesq(position, zone.GetCenterFloat3());
                    var inside = currentHash == zone.Hash
                        ? distSq <= zone.ExitRadiusSq
                        : distSq <= zone.EntryRadiusSq;
                    if (inside)
                    {
                        _playersInZones[player] = zone.Hash;
                        return zone.Hash;
                    }
                }
            }
            catch
            {
                // ignored
            }

            return 0;
        }

        private static bool TryGetBestPosition(EntityManager em, Entity entity, out float3 position)
        {
            position = default;

            try
            {
                if (em.HasComponent<LocalTransform>(entity))
                {
                    position = em.GetComponentData<LocalTransform>(entity).Position;
                    return true;
                }

                if (em.HasComponent<Translation>(entity))
                {
                    position = em.GetComponentData<Translation>(entity).Value;
                    return true;
                }

                if (em.HasComponent<LastTranslation>(entity))
                {
                    position = em.GetComponentData<LastTranslation>(entity).Value;
                    return true;
                }

                if (em.HasComponent<SpawnTransform>(entity))
                {
                    position = em.GetComponentData<SpawnTransform>(entity).Position;
                    return true;
                }

                if (em.HasComponent<LocalToWorld>(entity))
                {
                    position = em.GetComponentData<LocalToWorld>(entity).Position;
                    return true;
                }
            }
            catch
            {
                // ignored
            }

            return false;
        }

        private static bool TryGetPlatformId(Entity characterEntity, out ulong platformId)
        {
            platformId = 0;

            try
            {
                var em = VAutomationCore.Core.UnifiedCore.EntityManager;
                if (em == default || characterEntity == Entity.Null || !em.Exists(characterEntity) || !em.HasComponent<PlayerCharacter>(characterEntity))
                {
                    return false;
                }

                var playerCharacter = em.GetComponentData<PlayerCharacter>(characterEntity);
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
    }
}
