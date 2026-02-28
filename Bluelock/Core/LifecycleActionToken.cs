using System;
using System.Collections.Generic;

namespace VAuto.Zone.Core
{
    public static class LifecycleActionToken
    {
        private static readonly Dictionary<string, string> CanonicalActionMap = new(StringComparer.Ordinal)
        {
            ["store"] = "store",
            ["snapshot"] = "snapshot",
            ["blood"] = "blood",
            ["message"] = "message",
            ["kitapply"] = "kit_apply",
            ["kit"] = "kit",
            ["abilities"] = "abilities",
            ["ability"] = "ability",
            ["glow"] = "glow",
            ["teleport"] = "teleport",
            ["templates"] = "templates",
            ["integration"] = "integration",
            ["announce"] = "announce",
            ["restore"] = "restore",
            ["capturereturnposition"] = "capture_return_position",
            ["snapshotsave"] = "snapshot_save",
            ["setblood"] = "set_blood",
            ["zoneentermessage"] = "zone_enter_message",
            ["applykit"] = "apply_kit",
            ["teleportenter"] = "teleport_enter",
            ["applytemplates"] = "apply_templates",
            ["applytemplate"] = "apply_template",
            ["applyabilities"] = "apply_abilities",
            ["glowspawn"] = "glow_spawn",
            ["bossenter"] = "boss_enter",
            ["integrationeventsenter"] = "integration_events_enter",
            ["announceenter"] = "announce_enter",
            ["zoneexitmessage"] = "zone_exit_message",
            ["restorekitsnapshot"] = "restore_kit_snapshot",
            ["restoreabilities"] = "restore_abilities",
            ["bossexit"] = "boss_exit",
            ["teleportreturn"] = "teleport_return",
            ["glowreset"] = "glow_reset",
            ["integrationeventsexit"] = "integration_events_exit",
            ["announceexit"] = "announce_exit",
            ["playertag"] = "player_tag",
            ["cleartemplate"] = "clear_template"
        };

        public static bool TryParse(string rawToken, out string action, out string parameter)
        {
            action = string.Empty;
            parameter = string.Empty;

            if (string.IsNullOrWhiteSpace(rawToken))
            {
                return false;
            }

            var token = rawToken.Trim();
            var separator = token.IndexOf(':');
            if (separator < 0)
            {
                action = NormalizeActionToken(token);
                return action.Length > 0;
            }

            action = NormalizeActionToken(token[..separator]);
            parameter = token[(separator + 1)..].Trim();
            return action.Length > 0;
        }

        private static string NormalizeActionToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return string.Empty;
            }

            var compact = token.Trim().ToLowerInvariant().Replace("_", string.Empty).Replace("-", string.Empty);
            if (compact.Length == 0)
            {
                return string.Empty;
            }

            return CanonicalActionMap.TryGetValue(compact, out var canonical)
                ? canonical
                : token.Trim().ToLowerInvariant().Replace('-', '_');
        }
    }
}
