namespace VAuto.Core.Services
{
    using VAutomationCore.Core.Services;

    /// <summary>
    /// Service for broadcasting announcements to players.
    /// Simple, regular system for admin messages and trap notifications.
    /// </summary>
    public static class AnnouncementService
    {
        private static bool _initialized = false;
        
        /// <summary>
        /// Notification types for announcements.
        /// </summary>
        public enum NotifyType
        {
            Info,
            Warning,
            Error,
            TrapTriggered,
            KillStreak,
            Achievement
        }
        
        public static void Initialize()
        {
            if (_initialized) return;
            _initialized = true;
            VLifecycle.Plugin.Log.LogInfo("[AnnouncementService] Initialized");
        }
        
        /// <summary>
        /// Broadcasts a message to all players (global announcement).
        /// </summary>
        public static void Broadcast(string message, NotifyType type = NotifyType.Info)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            VLifecycle.Plugin.Log.LogInfo($"[Announcement][BROADCAST][{type}] {message}");
            if (!GameActionService.TrySendSystemMessageToAll(message))
            {
                VLifecycle.Plugin.Log.LogWarning("[Announcement] Broadcast transport failed; message was not sent to clients.");
            }
        }
        
        /// <summary>
        /// Sends a message to a specific player.
        /// </summary>
        public static void SendTo(ulong platformId, string message, NotifyType type = NotifyType.Info)
        {
            if (platformId == 0 || string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            VLifecycle.Plugin.Log.LogInfo($"[Announcement][To:{platformId}][{type}] {message}");
            if (!GameActionService.TrySendSystemMessageToPlatformId(platformId, message))
            {
                VLifecycle.Plugin.Log.LogWarning($"[Announcement] Direct message transport failed for platformId={platformId}.");
            }
        }
        
        /// <summary>
        /// Broadcasts a trap trigger event.
        /// </summary>
        public static void BroadcastTrapTrigger(string playerName, string trapOwnerName, bool isContainerTrap)
        {
            var trapType = isContainerTrap ? "Container Trap" : "Waypoint Trap";
            var message = $"[TRAP] {playerName} triggered {trapOwnerName}'s {trapType}!";
            VLifecycle.Plugin.Log.LogInfo($"[Announcement] {message}");
            
            // Broadcast to nearby players only (not global)
            // TODO: Implement area-based broadcasting
        }
        
        /// <summary>
        /// Notifies trap owner about trigger.
        /// </summary>
        public static void NotifyTrapOwner(string ownerName, ulong ownerPlatformId, string intruderName, string location)
        {
            var notifyMessage = $"[TRAP] {intruderName} triggered your trap at {location}!";
            VLifecycle.Plugin.Log.LogInfo($"[Announcement] Owner notification to {ownerName} ({ownerPlatformId}): {notifyMessage}");
            
            // Send private notification to trap owner
            SendTo(ownerPlatformId, notifyMessage, NotifyType.TrapTriggered);
        }
        
        /// <summary>
        /// Announces a kill streak milestone.
        /// </summary>
        public static void AnnounceKillStreak(string playerName, int streak)
        {
            var message = $"[KILLSTREAK] {playerName} has {streak} consecutive kills!";
            VLifecycle.Plugin.Log.LogInfo($"[Announcement] {message}");
            
            // Broadcast milestone achievements
            if (streak >= 10)
            {
                Broadcast(message + " 🔥", NotifyType.Achievement);
            }
            else
            {
                Broadcast(message, NotifyType.KillStreak);
            }
        }
        
        /// <summary>
        /// Announces chest spawn for a player.
        /// </summary>
        public static void AnnounceChestSpawn(string playerName, int waypointIndex, string waypointName)
        {
            var message = $"[CHEST] Containers spawned for {playerName} at {waypointName}!";
            VLifecycle.Plugin.Log.LogInfo($"[Announcement] {message}");
            
            // Notify the player privately
            // TODO: Get player platformId and send private message
        }
    }
}
