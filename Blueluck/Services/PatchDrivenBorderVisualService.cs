using System;
using System.Collections.Generic;
using System.Linq;
using ProjectM;
using Stunlock.Core;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using VAuto.Core;
using VAutomationCore.Abstractions;
using VAutomationCore.Core;

namespace Blueluck.Services
{
    internal static class PatchDrivenBorderVisualService
    {
        private static readonly Dictionary<Entity, ActiveBorderVisualState> _active = new();
        private static readonly List<Entity> _toRemove = new();
        private static readonly Dictionary<Entity, string> _lastSkipReasonByPlayer = new();
        private static int _tickId;

        private struct ActiveBorderVisualState
        {
            public int ZoneHash;
            public int Tier;
            public PrefabGUID BuffGuid;
            public int LastSeenTick;
        }

        public static void ProcessTick(EntityManager em)
        {
            if (em == default || Plugin.ZoneConfig?.IsInitialized != true)
            {
                CleanupMissingPlayers(em);
                return;
            }

            _tickId++;

            try
            {
                var query = em.CreateEntityQuery(ComponentType.ReadOnly<PlayerCharacter>());
                var players = query.ToEntityArray(Unity.Collections.Allocator.Temp);
                try
                {
                    foreach (var player in players)
                    {
                        ProcessPlayer(em, player);
                    }
                }
                finally
                {
                    players.Dispose();
                }
            }
            catch (Exception ex)
            {
                Plugin.LogDebug($"[BorderVisual] Patch-driven tick skipped: {ex.Message}");
            }

            CleanupMissingPlayers(em);
        }

        public static void Cleanup()
        {
            try
            {
                foreach (var pair in _active.ToArray())
                {
                    if (pair.Value.BuffGuid != PrefabGUID.Empty)
                    {
                        Buffs.RemoveBuff(pair.Key, pair.Value.BuffGuid);
                    }
                }
            }
            catch
            {
                // ignored
            }

            _active.Clear();
            _toRemove.Clear();
            _lastSkipReasonByPlayer.Clear();
            _tickId = 0;
        }

        private static void ProcessPlayer(EntityManager em, Entity player)
        {
            if (player == Entity.Null || !em.Exists(player))
            {
                return;
            }

            var zoneHash = Plugin.ZoneTransition?.GetPlayerZone(player) ?? 0;
            if (zoneHash == 0 || Plugin.ZoneConfig?.TryGetZoneByHash(zoneHash, out var zone) != true || zone == null)
            {
                EnsureRemoved(player, "no resolved zone");
                return;
            }

            var cfg = zone.BorderVisual;
            if (cfg == null || cfg.IntensityMax <= 0)
            {
                EnsureRemoved(player, $"zone {zoneHash} border visual disabled");
                return;
            }

            if (!TryGetBestPosition(em, player, out var pos))
            {
                EnsureRemoved(player, $"zone {zoneHash} player position unresolved");
                return;
            }

            var center = zone.GetCenterFloat3();
            var distFromCenter = math.distance(pos, center);
            var edgeRadius = zone.ExitRadius > 0f ? zone.ExitRadius : zone.EntryRadius;
            if (distFromCenter > edgeRadius)
            {
                EnsureRemoved(player, $"player outside zone {zoneHash}: dist={distFromCenter:0.00} radius={edgeRadius:0.00}");
                return;
            }

            var activationRange = math.max(0.01f, cfg.Range);
            var distToEdge = edgeRadius - distFromCenter;
            if (distToEdge > activationRange)
            {
                EnsureRemoved(player, $"inside zone {zoneHash} but outside border range: edgeDist={distToEdge:0.00} activeRange={activationRange:0.00}");
                return;
            }

            var tier = ComputeTier(distToEdge, activationRange, math.max(1, cfg.IntensityMax));
            if (!TryResolveBorderBuffGuid(cfg, tier, out var buffGuid))
            {
                EnsureRemoved(player, $"zone {zoneHash} border buff unresolved for tier {tier}");
                return;
            }

            var hasState = _active.TryGetValue(player, out var state);
            if (hasState)
            {
                state.LastSeenTick = _tickId;
            }

            if (hasState && state.ZoneHash == zoneHash && state.Tier == tier && state.BuffGuid == buffGuid)
            {
                _active[player] = state;
                _lastSkipReasonByPlayer.Remove(player);
                return;
            }

            if (hasState && state.BuffGuid != PrefabGUID.Empty && cfg.RemoveOnExit)
            {
                Buffs.RemoveBuff(player, state.BuffGuid);
                Plugin.LogInfo($"[BorderVisual] Removed prior border buff from player={player.Index} zone={state.ZoneHash} oldTier={state.Tier} oldBuff={state.BuffGuid.GuidHash}");
            }

            if (!TryResolveUserEntity(em, player, out var userEntity))
            {
                LogSkip(player, $"zone {zoneHash} user entity unresolved");
                _active.Remove(player);
                return;
            }

            Buffs.AddBuff(userEntity, player, buffGuid, duration: -1f, immortal: false);
            Plugin.LogInfo($"[BorderVisual] Applied border buff to player={player.Index} zone={zoneHash} tier={tier} buff={buffGuid.GuidHash} distToEdge={distToEdge:0.00}");

            _active[player] = new ActiveBorderVisualState
            {
                ZoneHash = zoneHash,
                Tier = tier,
                BuffGuid = buffGuid,
                LastSeenTick = _tickId
            };
            _lastSkipReasonByPlayer.Remove(player);
        }

        private static void CleanupMissingPlayers(EntityManager em)
        {
            _toRemove.Clear();
            foreach (var kvp in _active)
            {
                var exists = em != default && kvp.Key != Entity.Null && em.Exists(kvp.Key);
                if (!exists || kvp.Value.LastSeenTick != _tickId)
                {
                    _toRemove.Add(kvp.Key);
                }
            }

            foreach (var player in _toRemove)
            {
                if (_active.TryGetValue(player, out var state) && state.BuffGuid != PrefabGUID.Empty)
                {
                    Buffs.RemoveBuff(player, state.BuffGuid);
                    Plugin.LogInfo($"[BorderVisual] Removed border buff from player={player.Index} zone={state.ZoneHash} reason=cleanup");
                }

                _active.Remove(player);
                _lastSkipReasonByPlayer.Remove(player);
            }
        }

        private static void EnsureRemoved(Entity player, string reason)
        {
            if (!_active.TryGetValue(player, out var state))
            {
                LogSkip(player, reason);
                return;
            }

            if (state.BuffGuid != PrefabGUID.Empty)
            {
                Buffs.RemoveBuff(player, state.BuffGuid);
                Plugin.LogInfo($"[BorderVisual] Removed border buff from player={player.Index} zone={state.ZoneHash} reason={reason}");
            }

            _active.Remove(player);
            LogSkip(player, reason);
        }

        private static int ComputeTier(float distToEdge, float range, int intensityMax)
        {
            var frac = math.clamp(distToEdge / range, 0f, 0.999999f);
            var tier = intensityMax - (int)math.floor(frac * intensityMax);
            if (tier < 1) tier = 1;
            if (tier > intensityMax) tier = intensityMax;
            return tier;
        }

        private static bool TryResolveUserEntity(EntityManager em, Entity player, out Entity userEntity)
        {
            userEntity = Entity.Null;

            try
            {
                if (em == default || player == Entity.Null || !em.Exists(player) || !em.HasComponent<PlayerCharacter>(player))
                {
                    return false;
                }

                var playerCharacter = em.GetComponentData<PlayerCharacter>(player);
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

        private static bool TryResolveBorderBuffGuid(Models.BorderVisualConfig cfg, int tier, out PrefabGUID guid)
        {
            guid = PrefabGUID.Empty;

            try
            {
                if (cfg.BuffPrefabs != null && cfg.BuffPrefabs.Length >= tier)
                {
                    var token = cfg.BuffPrefabs[tier - 1]?.Trim();
                    if (!string.IsNullOrWhiteSpace(token) && TryResolvePrefabGuid(token, out guid))
                    {
                        return guid != PrefabGUID.Empty;
                    }
                }

                var effect = cfg.Effect?.Trim();
                if (string.IsNullOrWhiteSpace(effect))
                {
                    return false;
                }

                var prefabToken = effect switch
                {
                    "megara_visual" => "_megaraVisual",
                    "solarus_visual" => "_solarusVisual",
                    "manticore_visual" => "_manticoreVisual",
                    "monster_visual" => "_monsterVisual",
                    "dracula_visual" => "_draculaVisual",
                    _ => effect
                };

                return TryResolvePrefabGuid(prefabToken, out guid) && guid != PrefabGUID.Empty;
            }
            catch
            {
                return false;
            }
        }

        private static bool TryResolvePrefabGuid(string prefabName, out PrefabGUID guid)
        {
            guid = PrefabGUID.Empty;
            if (string.IsNullOrWhiteSpace(prefabName))
            {
                return false;
            }

            var token = prefabName.Trim();
            if (Plugin.PrefabToGuid?.IsInitialized == true && Plugin.PrefabToGuid.TryGetGuid(token, out guid))
            {
                return guid.GuidHash != 0;
            }

            return PrefabGuidConverter.TryGetGuid(token, out guid) && guid.GuidHash != 0;
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

        private static void LogSkip(Entity player, string reason)
        {
            if (_lastSkipReasonByPlayer.TryGetValue(player, out var lastReason) && string.Equals(lastReason, reason, StringComparison.Ordinal))
            {
                return;
            }

            _lastSkipReasonByPlayer[player] = reason;
            Plugin.LogInfo($"[BorderVisual] Skipped player={player.Index}: {reason}");
        }
    }
}
