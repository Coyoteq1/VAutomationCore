using Unity.Collections;
using VAutomationCore.Core;
using VAutomationCore.Core.Logging;
using VAutomationCore.Core.Services;

namespace VLifecycle.Services.Lifecycle
{
    public static class AnnouncementService
    {
        private static bool _initialized;

        public static void Initialize(CoreLogger logger)
        {
            if (_initialized) return;
            _initialized = true;
            logger.Info("[AnnouncementService] Initialized");
        }

        public static void Broadcast(string message, string channel = "info")
        {
            if (string.IsNullOrWhiteSpace(message)) return;
            var formatted = FormatMessage(message, channel);
            _ = new FixedString512Bytes(formatted);
            _ = GameActionService.TrySendSystemMessageToAll(formatted);
            Plugin.Log.LogInfo($"[Announcement][{channel}] {message}");
        }

        public static void ZoneEnter(string zoneId, string playerName)
        {
            Broadcast($"<b>{playerName}</b> entered <color=#4FC3F7>{zoneId}</color>", "zone");
        }

        public static void ZoneExit(string zoneId, string playerName)
        {
            Broadcast($"<b>{playerName}</b> exited <color=#90A4AE>{zoneId}</color>", "zone");
        }

        public static void TrapTriggered(string trapId, string playerName)
        {
            var message = $"<b>{playerName}</b> triggered trap <color=#FF8A65>{trapId}</color>";
            Broadcast($"----TRAP---- {message} ----TRAP----", "trap");
        }

        public static void PveBossCoopCall(string zoneId, string playerName)
        {
            var message = $"<b>{playerName}</b> joined event <color=#BA68C8>{zoneId}</color> against high-level bosses and wants co-op.";
            Broadcast($"----PVE---- {message} ----PVE----", "pve");
        }

        public static void PvpFightCall(string zoneId, string playerName)
        {
            var message = $"<b>{playerName}</b> entered <color=#29B6F6>{zoneId}</color> and is looking for PvP.";
            Broadcast($"----PVP---- {message} ----PVP----", "pvp");
        }

        private static string FormatMessage(string message, string channel)
        {
            var prefix = channel switch
            {
                "zone" => "<color=#4DD0E1>[ZONE]</color>",
                "trap" => "<color=#EF5350>[TRAP]</color>",
                "pve" => "<color=#AB47BC>[PVE]</color>",
                "pvp" => "<color=#29B6F6>[PVP]</color>",
                "admin" => "<color=#FFD54F>[ADMIN]</color>",
                _ => "<color=#81C784>[INFO]</color>"
            };

            return $"{prefix} <color=#ECEFF1>{message}</color>";
        }
    }
}
