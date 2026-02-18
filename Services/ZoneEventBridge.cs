using System;
using System.Collections.Generic;
using Unity.Entities;
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

        public static event Action<Entity, string>? OnPlayerEntered;
        public static event Action<Entity, string>? OnPlayerExited;

        public static void Initialize()
        {
            // optional hook for future init work
        }

        /// <summary>
        /// Publishes a player entered zone event.
        /// </summary>
        public static void PublishPlayerEntered(Entity player, string zoneId)
        {
            lock (Sync)
            {
                if (!PlayerStates.TryGetValue(player, out var state))
                {
                    state = new PlayerZoneState();
                    PlayerStates[player] = state;
                }

                state.PreviousZoneId = state.CurrentZoneId ?? string.Empty;
                state.CurrentZoneId = zoneId ?? string.Empty;
                state.WasInZone = true;
                state.IsInAnyZone = !string.IsNullOrWhiteSpace(state.CurrentZoneId);
                state.LastUpdateTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                state.EnteredAt = DateTime.UtcNow;
            }

            OnPlayerEntered?.Invoke(player, zoneId);
        }

        /// <summary>
        /// Publishes a player exited zone event.
        /// </summary>
        public static void PublishPlayerExited(Entity player, string zoneId)
        {
            lock (Sync)
            {
                if (!PlayerStates.TryGetValue(player, out var state))
                {
                    state = new PlayerZoneState();
                    PlayerStates[player] = state;
                }

                state.PreviousZoneId = state.CurrentZoneId ?? zoneId ?? string.Empty;
                state.CurrentZoneId = string.Empty;
                state.IsInAnyZone = false;
                state.LastUpdateTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                state.ExitedAt = DateTime.UtcNow;
            }

            OnPlayerExited?.Invoke(player, zoneId);
        }

        /// <summary>
        /// Gets the current zone state for a player.
        /// </summary>
        public static PlayerZoneState GetPlayerZoneState(Entity player)
        {
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
            lock (Sync)
            {
                PlayerStates.Remove(player);
            }
        }
    }
}
