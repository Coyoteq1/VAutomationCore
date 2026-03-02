using System;
using System.Collections.Generic;
using Unity.Entities;
using VAutomationCore.Core.Events;
using VAutomationCore.Models;

namespace VAutomationCore.Services
{
    /// <summary>
    /// Event bridge for zone-related events.
    /// </summary>
    public static class ZoneEventBridge
    {
        private static readonly object Sync = new();
        private static readonly Dictionary<Entity, PlayerZoneState> PlayerStates = new();
        private static bool _initialized;

        public static event Action<Entity, string>? OnPlayerEntered;
        public static event Action<Entity, string>? OnPlayerExited;

        public static void Initialize()
        {
            lock (Sync)
            {
                _initialized = true;
            }
        }

        /// <summary>
        /// Publishes a player entered zone event.
        /// </summary>
        public static void PublishPlayerEntered(Entity player, string zoneId)
        {
            EnsureInitialized();
            var normalizedZoneId = zoneId ?? string.Empty;

            lock (Sync)
            {
                if (!PlayerStates.TryGetValue(player, out var state))
                {
                    state = new PlayerZoneState();
                    PlayerStates[player] = state;
                }

                state.PreviousZoneId = state.CurrentZoneId ?? string.Empty;
                state.CurrentZoneId = normalizedZoneId;
                state.WasInZone = true;
                state.IsInAnyZone = !string.IsNullOrWhiteSpace(state.CurrentZoneId);
                state.LastUpdateTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                state.EnteredAt = DateTime.UtcNow;
            }

            InvokeSafely(OnPlayerEntered, player, normalizedZoneId);
            TypedEventBus.Publish(new PlayerEnteredZoneEvent
            {
                Player = player,
                ZoneId = normalizedZoneId
            });
        }

        /// <summary>
        /// Publishes a player exited zone event.
        /// </summary>
        public static void PublishPlayerExited(Entity player, string zoneId)
        {
            EnsureInitialized();
            var normalizedZoneId = zoneId ?? string.Empty;

            lock (Sync)
            {
                if (!PlayerStates.TryGetValue(player, out var state))
                {
                    state = new PlayerZoneState();
                    PlayerStates[player] = state;
                }

                state.PreviousZoneId = state.CurrentZoneId ?? normalizedZoneId;
                state.CurrentZoneId = string.Empty;
                state.IsInAnyZone = false;
                state.LastUpdateTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                state.ExitedAt = DateTime.UtcNow;
            }

            InvokeSafely(OnPlayerExited, player, normalizedZoneId);
            TypedEventBus.Publish(new PlayerExitedZoneEvent
            {
                Player = player,
                ZoneId = normalizedZoneId
            });
        }

        /// <summary>
        /// Gets the current zone state for a player.
        /// </summary>
        public static PlayerZoneState GetPlayerZoneState(Entity player)
        {
            EnsureInitialized();

            lock (Sync)
            {
                if (PlayerStates.TryGetValue(player, out var state))
                {
                    return state;
                }
            }

            return new PlayerZoneState();
        }

        /// <summary>
        /// Updates the zone state for a player.
        /// </summary>
        public static void UpdatePlayerZoneState(Entity player, PlayerZoneState state)
        {
            if (state == null)
            {
                return;
            }

            EnsureInitialized();

            lock (Sync)
            {
                state.LastUpdateTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                PlayerStates[player] = state;
            }
        }

        /// <summary>
        /// Removes the zone state for a player.
        /// </summary>
        public static void RemovePlayerZoneState(Entity player)
        {
            EnsureInitialized();

            lock (Sync)
            {
                PlayerStates.Remove(player);
            }
        }

<<<<<<< Updated upstream
=======
        public static void RemovePlayerState(Entity player)
        {
            RemovePlayerZoneState(player);
        }

        public static Core.ECS.Components.EcsPlayerZoneState GetPlayerZoneComponentState(Entity player)
        {
            var modelState = GetPlayerZoneState(player);
            return ZoneStateMapper.ToComponent(modelState, Core.ECS.ZoneHashUtility.GetZoneHash);
        }

        public static void UpdateFromComponentState(Entity player, Core.ECS.Components.EcsPlayerZoneState state)
        {
            var model = ZoneStateMapper.ToModel(state, Core.ECS.ZoneHashUtility.GetZoneId);
            UpdatePlayerZoneState(player, model);
        }

>>>>>>> Stashed changes
        private static void EnsureInitialized()
        {
            if (_initialized)
            {
                return;
            }

            Initialize();
        }

        private static void InvokeSafely(Action<Entity, string>? handlers, Entity player, string zoneId)
        {
            if (handlers == null)
            {
                return;
            }

            foreach (var handler in handlers.GetInvocationList())
            {
                try
                {
                    ((Action<Entity, string>)handler).Invoke(player, zoneId);
                }
                catch
                {
                    // One subscriber should never block zone transition processing.
                }
            }
        }
    }
}
