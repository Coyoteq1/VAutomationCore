using System;
using System.Collections.Generic;
using ProjectM;
using ProjectM.Network;
using ProjectM.Shared;
using Stunlock.Core;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using VAutomationCore.Core.Logging;

namespace VAutomationCore.Core.Services
{
    /// <summary>
    /// Shared runtime actions:
    /// buff application/removal, teleport, and player-visible system messaging.
    /// </summary>
    public static class GameActionService
    {
        private static readonly CoreLogger Log = new("GameActions");
        private static readonly PrefabGUID TeleportBuffGuid = new(150521246);
        public const string EventPlayerDetect = "PlayerDetect";
        public const string EventPlayerExit = "PlayerExit";
        public const string EventPlayerEnter = "PlayerEnter";
        public const string EventPlayerPostEnter = "PlayerPostEnter";
        public delegate bool GameAction(params object[] args);
        private sealed class EventActionBinding
        {
            public string ActionName { get; }
            public Func<object[], object[]>? ArgTransform { get; }

            public EventActionBinding(string actionName, Func<object[], object[]>? argTransform)
            {
                ActionName = actionName;
                ArgTransform = argTransform;
            }
        }

        private static readonly object EventLock = new();
        private static readonly object StateRestoreLock = new();
        private static readonly Dictionary<string, List<EventActionBinding>> _eventBindings =
            new(StringComparer.OrdinalIgnoreCase);
        
        /// <summary>
        /// Tracks applied buffs per player for restoration on exit.
        /// Key: platformId, Value: List of buff GUIDs applied during zone.
        /// </summary>
        private static readonly Dictionary<ulong, List<PrefabGUID>> _appliedBuffsByPlayer =
            new();
        
        private static readonly Dictionary<string, GameAction> _actions = new(StringComparer.OrdinalIgnoreCase)
        {
            ["ApplyBuff"] = args =>
                TryResolveBuffArgs(args, out var target, out var guid, out var duration) &&
                TryApplyBuff(target, guid, duration),
            ["CleanBuff"] = args =>
                TryResolveBuffArgs(args, out var target, out var guid, out var duration) &&
                TryApplyCleanBuff(target, guid, duration),
            ["RemoveBuff"] = args =>
                TryGetArg(args, 0, out Entity target) &&
                TryGetArg(args, 1, out PrefabGUID guid) &&
                TryRemoveBuff(target, guid),
            ["Teleport"] = args =>
                TryGetArg(args, 0, out Entity target) &&
                TryGetArg(args, 1, out float3 position) &&
                TryTeleport(target, position),
            ["SetPosition"] = args =>
                TryGetArg(args, 0, out Entity target) &&
                TryGetArg(args, 1, out float3 position) &&
                TrySetEntityPosition(target, position),
            ["SendMessageToAll"] = args =>
                TryGetArg(args, 0, out string message) &&
                TrySendSystemMessageToAll(message),
            ["SendMessageToPlatform"] = args =>
                TryGetArg(args, 0, out ulong platformId) &&
                TryGetArg(args, 1, out string message) &&
                TrySendSystemMessageToPlatformId(platformId, message),
            ["SendMessageToUser"] = args =>
                TryGetArg(args, 0, out Entity userEntity) &&
                TryGetArg(args, 1, out string message) &&
                TrySendSystemMessageToUserEntity(userEntity, message)
        };

        public static bool InvokeAction(string actionName, params object[] args)
        {
            if (string.IsNullOrWhiteSpace(actionName))
            {
                return false;
            }

            if (!_actions.TryGetValue(actionName, out var action))
            {
                Log.Debug($"Action '{actionName}' not registered.");
                return false;
            }

            try
            {
                return action.Invoke(args);
            }
            catch (Exception ex)
            {
                Log.Warning($"Action '{actionName}' failed: {ex.Message}");
                return false;
            }
        }

        public static IReadOnlyCollection<string> GetRegisteredActionNames()
        {
            return _actions.Keys;
        }

        public static void RegisterEventAction(string eventName, string actionName, Func<object[], object[]>? argTransform = null)
        {
            if (string.IsNullOrWhiteSpace(eventName) || string.IsNullOrWhiteSpace(actionName))
            {
                return;
            }

            lock (EventLock)
            {
                if (!_eventBindings.TryGetValue(eventName, out var bindings))
                {
                    bindings = new List<EventActionBinding>();
                    _eventBindings[eventName] = bindings;
                }

                bindings.Add(new EventActionBinding(actionName, argTransform));
            }
        }

        public static void ClearEventActions(string eventName)
        {
            if (string.IsNullOrWhiteSpace(eventName))
            {
                return;
            }

            lock (EventLock)
            {
                _eventBindings.Remove(eventName);
            }
        }

        public static IReadOnlyCollection<string> GetRegisteredEventNames()
        {
            lock (EventLock)
            {
                return new List<string>(_eventBindings.Keys);
            }
        }

        public static int GetRegisteredEventActionCount(string eventName)
        {
            if (string.IsNullOrWhiteSpace(eventName))
            {
                return 0;
            }

            lock (EventLock)
            {
                return _eventBindings.TryGetValue(eventName, out var bindings) ? bindings.Count : 0;
            }
        }

        public static int TriggerEvent(string eventName, params object[] args)
        {
            if (string.IsNullOrWhiteSpace(eventName))
            {
                return 0;
            }

            List<EventActionBinding>? bindings;
            lock (EventLock)
            {
                if (!_eventBindings.TryGetValue(eventName, out bindings) || bindings.Count == 0)
                {
                    return 0;
                }

                bindings = new List<EventActionBinding>(bindings);
            }

            // Validate entities in args before processing
            if (!ValidateEventArgs(args))
            {
                Log.Debug($"Event '{eventName}' skipped: invalid entity in args.");
                return 0;
            }

            var fired = 0;
            foreach (var binding in bindings)
            {
                var actionArgs = args;
                if (binding.ArgTransform != null)
                {
                    try
                    {
                        actionArgs = binding.ArgTransform(args);
                    }
                    catch (Exception ex)
                    {
                        Log.Warning($"ArgTransform for action '{binding.ActionName}' in event '{eventName}' failed: {ex.Message}");
                        continue;
                    }
                }

                if (InvokeAction(binding.ActionName, actionArgs))
                {
                    fired++;
                }
            }

            return fired;
        }

        public static int TriggerLifecycleFlow(params object[] args)
        {
            var fired = 0;
            fired += TriggerEvent(EventPlayerDetect, args);
            fired += TriggerEvent(EventPlayerExit, args);
            fired += TriggerEvent(EventPlayerEnter, args);
            fired += TriggerEvent(EventPlayerPostEnter, args);
            return fired;
        }

        /// <summary>
        /// Captures the list of buffs applied to a player (for restoration on exit).
        /// Call on zone entry to snapshot applied buffs.
        /// </summary>
        public static void CaptureBuffSnapshot(ulong platformId, Entity targetEntity)
        {
            if (platformId == 0)
            {
                return;
            }

            try
            {
                var em = UnifiedCore.EntityManager;
                if (!IsEntityValid(em, targetEntity))
                {
                    return;
                }

                lock (StateRestoreLock)
                {
                    if (!_appliedBuffsByPlayer.ContainsKey(platformId))
                    {
                        _appliedBuffsByPlayer[platformId] = new List<PrefabGUID>();
                    }
                }

                Log.Debug($"Buff snapshot captured for platform {platformId}");
            }
            catch (Exception ex)
            {
                Log.Warning($"Failed to capture buff snapshot for platform {platformId}: {ex.Message}");
            }
        }

        /// <summary>
        /// Tracks a buff as applied for a player (for state restoration on exit).
        /// </summary>
        public static void TrackAppliedBuff(ulong platformId, PrefabGUID buffGuid)
        {
            if (platformId == 0 || buffGuid == PrefabGUID.Empty)
            {
                return;
            }

            lock (StateRestoreLock)
            {
                if (!_appliedBuffsByPlayer.TryGetValue(platformId, out var buffs))
                {
                    buffs = new List<PrefabGUID>();
                    _appliedBuffsByPlayer[platformId] = buffs;
                }

                if (!buffs.Contains(buffGuid))
                {
                    buffs.Add(buffGuid);
                }
            }
        }

        /// <summary>
        /// Restores player state by removing tracked buffs on zone exit.
        /// </summary>
        public static bool RestorePlayerStateOnExit(ulong platformId, Entity targetEntity)
        {
            if (platformId == 0)
            {
                return false;
            }

            try
            {
                var em = UnifiedCore.EntityManager;
                if (!IsEntityValid(em, targetEntity))
                {
                    return false;
                }

                List<PrefabGUID> buffsToRemove = null;
                lock (StateRestoreLock)
                {
                    if (_appliedBuffsByPlayer.TryGetValue(platformId, out var buffs))
                    {
                        buffsToRemove = new List<PrefabGUID>(buffs);
                        _appliedBuffsByPlayer.Remove(platformId);
                    }
                }

                if (buffsToRemove != null && buffsToRemove.Count > 0)
                {
                    var removedCount = 0;
                    foreach (var buffGuid in buffsToRemove)
                    {
                        if (TryRemoveBuff(targetEntity, buffGuid))
                        {
                            removedCount++;
                        }
                    }

                    Log.Info($"Player {platformId} exited zone: restored state by removing {removedCount}/{buffsToRemove.Count} buffs");
                    return true;
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.Warning($"Failed to restore state for player {platformId}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Clears all tracked state for a player (e.g., on disconnect).
        /// </summary>
        public static void ClearPlayerTrackedState(ulong platformId)
        {
            if (platformId == 0)
            {
                return;
            }

            lock (StateRestoreLock)
            {
                _appliedBuffsByPlayer.Remove(platformId);
            }

            Log.Debug($"Cleared tracked state for player {platformId}");
        }

        /// <summary>
        /// Gets count of tracked buffs for a player.
        /// </summary>
        public static int GetTrackedBuffCount(ulong platformId)
        {
            if (platformId == 0)
            {
                return 0;
            }

            lock (StateRestoreLock)
            {
                return _appliedBuffsByPlayer.TryGetValue(platformId, out var buffs) ? buffs.Count : 0;
            }
        }

        public static bool TryApplyBuff(Entity targetEntity, PrefabGUID buffGuid, out Entity buffEntity, float duration = 0f)
        {
            buffEntity = Entity.Null;

            var em = UnifiedCore.EntityManager;
            if (!IsEntityValid(em, targetEntity) || buffGuid == PrefabGUID.Empty)
            {
                return false;
            }

            if (!TryApplyBuffViaDebugEvents(em, targetEntity, buffGuid, out buffEntity))
            {
                return false;
            }

            ApplyBuffDuration(em, buffEntity, duration);
            return true;
        }

        public static bool TryApplyBuff(Entity targetEntity, PrefabGUID buffGuid, float duration = 0f)
        {
            return TryApplyBuff(targetEntity, buffGuid, out _, duration);
        }

        public static bool TryApplyCleanBuff(Entity targetEntity, PrefabGUID buffGuid, out Entity buffEntity, float duration = -1f)
        {
            buffEntity = Entity.Null;

            var em = UnifiedCore.EntityManager;
            if (!IsEntityValid(em, targetEntity) || buffGuid == PrefabGUID.Empty)
            {
                return false;
            }

            if (!TryApplyBuffViaDebugEvents(em, targetEntity, buffGuid, out buffEntity))
            {
                return false;
            }

            ApplyBuffDuration(em, buffEntity, duration);
            SanitizeBuffEntity(em, buffEntity, targetEntity);
            return true;
        }

        public static bool TryApplyCleanBuff(Entity targetEntity, PrefabGUID buffGuid, float duration = -1f)
        {
            return TryApplyCleanBuff(targetEntity, buffGuid, out _, duration);
        }

        public static bool TryRemoveBuff(Entity targetEntity, PrefabGUID buffGuid)
        {
            var em = UnifiedCore.EntityManager;
            if (!IsEntityValid(em, targetEntity) || buffGuid == PrefabGUID.Empty)
            {
                return false;
            }

            if (!BuffUtility.TryGetBuff(em, targetEntity, buffGuid, out var buffEntity))
            {
                return false;
            }

            DestroyUtility.Destroy(em, buffEntity, DestroyDebugReason.TryRemoveBuff);
            return true;
        }

        public static bool HasBuff(Entity targetEntity, PrefabGUID buffGuid)
        {
            var em = UnifiedCore.EntityManager;
            if (!IsEntityValid(em, targetEntity) || buffGuid == PrefabGUID.Empty)
            {
                return false;
            }

            return BuffUtility.HasBuff(em, targetEntity, buffGuid);
        }

        public static bool TryTeleport(Entity targetEntity, float3 targetPosition)
        {
            var em = UnifiedCore.EntityManager;
            if (!IsEntityValid(em, targetEntity))
            {
                return false;
            }

            if (TryApplyBuff(targetEntity, TeleportBuffGuid, out var teleportBuffEntity, 0f) &&
                teleportBuffEntity != Entity.Null &&
                em.HasComponent<TeleportBuff>(teleportBuffEntity))
            {
                var teleportBuff = em.GetComponentData<TeleportBuff>(teleportBuffEntity);
                teleportBuff.EndPosition = targetPosition;
                em.SetComponentData(teleportBuffEntity, teleportBuff);
                return true;
            }

            return TrySetEntityPosition(targetEntity, targetPosition);
        }

        public static bool TrySetEntityPosition(Entity targetEntity, float3 targetPosition)
        {
            var em = UnifiedCore.EntityManager;
            if (!IsEntityValid(em, targetEntity))
            {
                return false;
            }

            var wroteAny = false;

            if (em.HasComponent<SpawnTransform>(targetEntity))
            {
                var spawnTransform = em.GetComponentData<SpawnTransform>(targetEntity);
                spawnTransform.Position = targetPosition;
                em.SetComponentData(targetEntity, spawnTransform);
                wroteAny = true;
            }

            if (em.HasComponent<Height>(targetEntity))
            {
                var height = em.GetComponentData<Height>(targetEntity);
                height.LastPosition = targetPosition;
                em.SetComponentData(targetEntity, height);
                wroteAny = true;
            }

            if (em.HasComponent<LocalTransform>(targetEntity))
            {
                var localTransform = em.GetComponentData<LocalTransform>(targetEntity);
                localTransform.Position = targetPosition;
                em.SetComponentData(targetEntity, localTransform);
                wroteAny = true;
            }

            if (em.HasComponent<Translation>(targetEntity))
            {
                var translation = em.GetComponentData<Translation>(targetEntity);
                translation.Value = targetPosition;
                em.SetComponentData(targetEntity, translation);
                wroteAny = true;
            }

            if (em.HasComponent<LastTranslation>(targetEntity))
            {
                var lastTranslation = em.GetComponentData<LastTranslation>(targetEntity);
                lastTranslation.Value = targetPosition;
                em.SetComponentData(targetEntity, lastTranslation);
                wroteAny = true;
            }

            return wroteAny;
        }

        public static bool TrySendSystemMessageToAll(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return false;
            }

            try
            {
                var em = UnifiedCore.EntityManager;
                var msg = new FixedString512Bytes(message);
                ServerChatUtils.SendSystemMessageToAllClients(em, ref msg);
                return true;
            }
            catch (Exception ex)
            {
                Log.Warning($"Failed to send system message to all: {ex.Message}");
                return false;
            }
        }

        public static bool TrySendSystemMessageToPlatformId(ulong platformId, string message)
        {
            if (platformId == 0 || string.IsNullOrWhiteSpace(message))
            {
                return false;
            }

            if (!TryFindUserEntityByPlatformId(platformId, out var userEntity))
            {
                return false;
            }

            return TrySendSystemMessageToUserEntity(userEntity, message);
        }

        public static bool TrySendSystemMessageToUserEntity(Entity userEntity, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return false;
            }

            try
            {
                var em = UnifiedCore.EntityManager;
                if (!IsEntityValid(em, userEntity) || !em.HasComponent<User>(userEntity))
                {
                    return false;
                }

                var user = em.GetComponentData<User>(userEntity);
                var msg = new FixedString512Bytes(message);
                ServerChatUtils.SendSystemMessageToClient(em, user, ref msg);
                return true;
            }
            catch (Exception ex)
            {
                Log.Warning($"Failed to send system message to user entity {userEntity.Index}: {ex.Message}");
                return false;
            }
        }

        public static bool TryFindUserEntityByPlatformId(ulong platformId, out Entity userEntity)
        {
            userEntity = Entity.Null;
            if (platformId == 0)
            {
                return false;
            }

            try
            {
                var em = UnifiedCore.EntityManager;
                var query = em.CreateEntityQuery(ComponentType.ReadOnly<User>());
                var users = query.ToEntityArray(Allocator.Temp);
                try
                {
                    foreach (var candidate in users)
                    {
                        if (!IsEntityValid(em, candidate) || !em.HasComponent<User>(candidate))
                        {
                            continue;
                        }

                        var user = em.GetComponentData<User>(candidate);
                        if (user.PlatformId == platformId)
                        {
                            userEntity = candidate;
                            return true;
                        }
                    }
                }
                finally
                {
                    users.Dispose();
                    query.Dispose();
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"Failed to find user by platform ID {platformId}: {ex.Message}");
                return false;
            }

            return false;
        }

        private static bool TryApplyBuffViaDebugEvents(EntityManager em, Entity targetEntity, PrefabGUID buffGuid, out Entity buffEntity)
        {
            buffEntity = Entity.Null;

            try
            {
                var server = UnifiedCore.Server;
                var debugEvents = server.GetExistingSystemManaged<DebugEventsSystem>();
                if (debugEvents == null)
                {
                    Log.Debug($"DebugEventsSystem not available for buff {buffGuid.GuidHash}.");
                    return false;
                }

                var userEntity = targetEntity;
                if (em.HasComponent<PlayerCharacter>(targetEntity))
                {
                    var playerCharacter = em.GetComponentData<PlayerCharacter>(targetEntity);
                    if (IsEntityValid(em, playerCharacter.UserEntity))
                    {
                        userEntity = playerCharacter.UserEntity;
                    }
                }

                var applyEvent = new ApplyBuffDebugEvent { BuffPrefabGUID = buffGuid };
                var fromCharacter = new FromCharacter
                {
                    Character = targetEntity,
                    User = userEntity
                };

                debugEvents.ApplyBuff(fromCharacter, applyEvent);
                return BuffUtility.TryGetBuff(em, targetEntity, buffGuid, out buffEntity);
            }
            catch (Exception ex)
            {
                Log.Warning($"Failed to apply buff {buffGuid.GuidHash} to entity {targetEntity.Index}: {ex.Message}");
                return false;
            }
        }

        private static void ApplyBuffDuration(EntityManager em, Entity buffEntity, float duration)
        {
            if (buffEntity == Entity.Null || !em.Exists(buffEntity))
            {
                return;
            }

            try
            {
                if (duration > 0f && em.HasComponent<LifeTime>(buffEntity))
                {
                    var lifeTime = em.GetComponentData<LifeTime>(buffEntity);
                    lifeTime.Duration = duration;
                    lifeTime.EndAction = LifeTimeEndAction.Destroy;
                    em.SetComponentData(buffEntity, lifeTime);
                    return;
                }

                if (duration <= -1f && em.HasComponent<LifeTime>(buffEntity))
                {
                    var lifeTime = em.GetComponentData<LifeTime>(buffEntity);
                    lifeTime.EndAction = LifeTimeEndAction.None;
                    em.SetComponentData(buffEntity, lifeTime);
                }
            }
            catch
            {
                // Ignore duration tuning failures.
            }
        }

        private static void SanitizeBuffEntity(EntityManager em, Entity buffEntity, Entity targetEntity)
        {
            try
            {
                if (!em.Exists(buffEntity))
                {
                    return;
                }

                if (em.HasComponent<EntityOwner>(buffEntity))
                {
                    em.SetComponentData(buffEntity, new EntityOwner { Owner = targetEntity });
                }

                var removeComponents = new ComponentType[]
                {
                    ComponentType.ReadWrite<CreateGameplayEventsOnSpawn>(),
                    ComponentType.ReadWrite<GameplayEventListeners>(),
                    ComponentType.ReadWrite<RemoveBuffOnGameplayEvent>(),
                    ComponentType.ReadWrite<RemoveBuffOnGameplayEventEntry>(),
                    ComponentType.ReadWrite<DealDamageOnGameplayEvent>(),
                    ComponentType.ReadWrite<ModifyMovementSpeedBuff>(),
                    ComponentType.ReadWrite<HealOnGameplayEvent>(),
                    ComponentType.ReadWrite<DestroyOnGameplayEvent>()
                };

                foreach (var type in removeComponents)
                {
                    if (em.HasComponent(buffEntity, type))
                        em.RemoveComponent(buffEntity, type);
                }
            }
            catch
            {
                // Keep buff even if sanitization partially fails.
            }
        }

        private static bool TryResolveBuffArgs(object[] args, out Entity targetEntity, out PrefabGUID buffGuid, out float duration)
        {
            targetEntity = Entity.Null;
            buffGuid = PrefabGUID.Empty;
            duration = 0f;
            return TryGetArg(args, 0, out targetEntity) &&
                   TryGetArg(args, 1, out buffGuid) &&
                   TryGetOptionalFloatArg(args, 2, out duration);
        }

        private static bool TryGetOptionalFloatArg(object[] args, int index, out float value)
        {
            value = 0f;
            if (args == null || index >= args.Length || args[index] == null)
            {
                return true;
            }

            return TryGetArg(args, index, out value);
        }

        private static bool TryGetArg<T>(object[] args, int index, out T value)
        {
            value = default;
            if (args == null || index < 0 || index >= args.Length)
            {
                return false;
            }

            var arg = args[index];
            if (arg is T typed)
            {
                value = typed;
                return true;
            }

            try
            {
                if (typeof(T) == typeof(PrefabGUID))
                {
                    if (arg is int guidInt)
                    {
                        value = (T)(object)new PrefabGUID(guidInt);
                        return true;
                    }

                    if (arg is long guidLong)
                    {
                        value = (T)(object)new PrefabGUID((int)guidLong);
                        return true;
                    }
                }

                if (typeof(T) == typeof(float))
                {
                    if (arg is int intValue)
                    {
                        value = (T)(object)(float)intValue;
                        return true;
                    }

                    if (arg is double doubleValue)
                    {
                        value = (T)(object)(float)doubleValue;
                        return true;
                    }
                }

                if (typeof(T) == typeof(ulong))
                {
                    if (arg is long longValue && longValue >= 0)
                    {
                        value = (T)(object)(ulong)longValue;
                        return true;
                    }

                    if (arg is int intValue && intValue >= 0)
                    {
                        value = (T)(object)(ulong)intValue;
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        private static bool IsEntityValid(EntityManager em, Entity entity)
        {
            return entity != Entity.Null && em != default && em.Exists(entity);
        }

        /// <summary>
        /// Validates that any Entity arguments in the event args are still valid.
        /// </summary>
        private static bool ValidateEventArgs(object[] args)
        {
            if (args == null || args.Length == 0)
            {
                return true;
            }

            try
            {
                var em = UnifiedCore.EntityManager;
                foreach (var arg in args)
                {
                    if (arg is Entity entity && entity != Entity.Null)
                    {
                        if (!IsEntityValid(em, entity))
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}
