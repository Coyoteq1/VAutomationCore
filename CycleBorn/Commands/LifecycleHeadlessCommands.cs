using VampireCommandFramework;
using VLifecycle.Services.Lifecycle;

namespace VLifecycle.Commands
{
    [CommandGroup("lifecycle", "lc")]
    public static class LifecycleHeadlessCommands
    {
        [Command("help", shortHand: "h", description: "Show lifecycle commands", adminOnly: false)]
        public static void Help(ChatCommandContext ctx)
        {
            ctx.Reply("[Lifecycle] Commands: .lifecycle status | .lifecycle config | .lifecycle respawn <on|off|status>");
        }

        [Command("status", shortHand: "s", description: "Show lifecycle module status", adminOnly: false)]
        public static void Status(ChatCommandContext ctx)
        {
            ctx.Reply("[Lifecycle] Module status");
            ctx.Reply($"Enabled: {(Plugin.IsEnabled ? "Yes" : "No")}");
            ctx.Reply($"ZoneTriggersLifecycle: {(Plugin.ZoneTriggersLifecycle ? "Yes" : "No")}");
            ctx.Reply($"SendLifecycleEvents: {(Plugin.SendLifecycleEvents ? "Yes" : "No")}");
        }

        [Command("config", shortHand: "c", description: "Show lifecycle config summary", adminOnly: true)]
        public static void Config(ChatCommandContext ctx)
        {
            ctx.Reply("[Lifecycle] Config");
            ctx.Reply($"SaveInventory: {Plugin.SaveInventory}");
            ctx.Reply($"RestoreInventory: {Plugin.RestoreInventory}");
            ctx.Reply($"SaveBuffs: {Plugin.SaveBuffs}");
            ctx.Reply($"RestoreBuffs: {Plugin.RestoreBuffs}");
            ctx.Reply($"SaveEquipment: {Plugin.SaveEquipment}");
            ctx.Reply($"SaveBlood: {Plugin.SaveBlood}");
            ctx.Reply($"SaveSpells: {Plugin.SaveSpells}");
            ctx.Reply($"SandboxEnabled: {Plugin.SandboxEnabled}");
        }

        [Command("respawn", shortHand: "rs", description: "Manage respawn prevention", adminOnly: true)]
        public static void Respawn(ChatCommandContext ctx, string mode = "status")
        {
            switch ((mode ?? "status").Trim().ToLowerInvariant())
            {
                case "on":
                case "enable":
                case "start":
                    RespawnPreventionService.PreventRespawns();
                    ctx.Reply($"[Lifecycle] Respawn prevention ON (refCount={RespawnPreventionService.RefCount})");
                    break;
                case "off":
                case "disable":
                case "stop":
                    RespawnPreventionService.AllowRespawns();
                    ctx.Reply($"[Lifecycle] Respawn prevention {(RespawnPreventionService.IsEnabled ? "ON" : "OFF")} (refCount={RespawnPreventionService.RefCount})");
                    break;
                case "reset":
                    RespawnPreventionService.Reset();
                    ctx.Reply("[Lifecycle] Respawn prevention reset (OFF)");
                    break;
                default:
                    ctx.Reply($"[Lifecycle] Respawn prevention: {(RespawnPreventionService.IsEnabled ? "ON" : "OFF")} (refCount={RespawnPreventionService.RefCount})");
                    break;
            }
        }
    }
}
