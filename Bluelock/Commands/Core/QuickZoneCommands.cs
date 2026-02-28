using Unity.Entities;
using VampireCommandFramework;
using VAuto.Zone.Services;

namespace VAuto.Zone.Commands
{
    [CommandGroup("zone", "z")]
    public static class QuickZoneCommands
    {
        /// <summary>
        /// Backward compatible alias - use .zone enter instead.
        /// </summary>
        [Command("enter", shortHand: "en", description: "Enter zone immediately. Empty zone uses default.", adminOnly: true)]
        public static void Enter(ChatCommandContext ctx, string zoneId = "")
        {
            var character = ctx.Event?.SenderCharacterEntity ?? Entity.Null;
            if (character == Entity.Null)
            {
                ctx.Reply("<color=#FF0000>Error: Could not resolve your character entity.</color>");
                return;
            }

            var normalizedInput = ResolveFlowAlias(zoneId);
            var resolvedZoneId = string.IsNullOrWhiteSpace(normalizedInput)
                ? ZoneConfigService.GetDefaultZoneId()
                : normalizedInput;

            if (string.IsNullOrWhiteSpace(resolvedZoneId))
            {
                ctx.Reply("<color=#FF0000>Error: No zone provided and no default zone set. Use .z default [name].</color>");
                return;
            }

            if (!Plugin.ForcePlayerEnterZone(character, resolvedZoneId))
            {
                ctx.Reply($"<color=#FF0000>Error: Failed to enter zone '{resolvedZoneId}'.</color>");
                return;
            }

            ctx.Reply($"<color=#00FF00>Entered zone '{resolvedZoneId}'.</color>");
        }

        [Command("exit", shortHand: "ex", description: "Exit current zone immediately.", adminOnly: true)]
        public static void Exit(ChatCommandContext ctx, string flowOrZone = "")
        {
            var character = ctx.Event?.SenderCharacterEntity ?? Entity.Null;
            if (character == Entity.Null)
            {
                ctx.Reply("<color=#FF0000>Error: Could not resolve your character entity.</color>");
                return;
            }

            var expectedZoneId = ResolveFlowAlias(flowOrZone);
            if (!string.IsNullOrWhiteSpace(expectedZoneId))
            {
                var state = VAutomationCore.Services.ZoneEventBridge.GetPlayerZoneState(character);
                var currentZoneId = state?.CurrentZoneId ?? string.Empty;
                if (!string.Equals(currentZoneId, expectedZoneId, System.StringComparison.OrdinalIgnoreCase))
                {
                    ctx.Reply($"<color=#FF0000>Error: You are in zone '{(string.IsNullOrWhiteSpace(currentZoneId) ? "none" : currentZoneId)}', not '{expectedZoneId}'.</color>");
                    return;
                }
            }

            if (!Plugin.ForcePlayerExitZone(character))
            {
                ctx.Reply("<color=#FF0000>Error: You are not in an active zone or exit failed.</color>");
                return;
            }

            ctx.Reply("<color=#00FF00>Exited zone.</color>");
        }

        internal static string ResolveFlowAlias(string flowOrZone)
        {
            var token = (flowOrZone ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(token))
            {
                return string.Empty;
            }

            if (token.Equals("flow1", System.StringComparison.OrdinalIgnoreCase))
            {
                return "1";
            }

            if (token.Equals("flow2", System.StringComparison.OrdinalIgnoreCase))
            {
                return "2";
            }

            return token;
        }
    }
}
