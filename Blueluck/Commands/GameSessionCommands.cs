using System;
using System.Globalization;
using System.Linq;
using Blueluck.Models;
using Unity.Entities;
using Unity.Mathematics;
using VampireCommandFramework;
using VAutomationCore.Services;

namespace Blueluck.Commands
{
    [CommandGroup("game", "g")]
    public static class GameSessionCommands
    {
        [Command("ready", adminOnly: false)]
        public static void Ready(ChatCommandContext ctx)
        {
            ExecuteReady(ctx, true);
        }

        [Command("unready", adminOnly: false)]
        public static void Unready(ChatCommandContext ctx)
        {
            ExecuteReady(ctx, false);
        }

        [Command("lobby", adminOnly: false)]
        public static void Lobby(ChatCommandContext ctx)
        {
            if (!TryGetCurrentSession(ctx, out var session, out _))
            {
                ctx.Reply("Not in a session-enabled zone.");
                return;
            }

            ctx.Reply($"[Game] {session.ZoneName} [{session.State}] ready {session.ReadyCount}/{session.ParticipantCount}, round {session.RoundNumber}");
        }

        [Command("status", adminOnly: false)]
        public static void Status(ChatCommandContext ctx)
        {
            Lobby(ctx);
        }

        [Command("forcestart", adminOnly: true)]
        public static void ForceStart(ChatCommandContext ctx)
        {
            if (!TryGetCurrentZoneHash(ctx, out var zoneHash))
            {
                ctx.Reply("Not in a session-enabled zone.");
                return;
            }

            var ok = Plugin.GameSessions?.ForceStart(zoneHash) == true;
            ctx.Reply(ok ? "[Game] Force start triggered." : "[Game] Force start failed.");
        }

        [Command("start", adminOnly: true)]
        public static void Start(ChatCommandContext ctx)
        {
            if (!TryGetCurrentZoneHash(ctx, out var zoneHash))
            {
                ctx.Reply("Not in a session-enabled zone.");
                return;
            }

            var ok = Plugin.GameSessions?.Start(zoneHash) == true;
            ctx.Reply(ok ? "[Game] Session start triggered." : "[Game] Session start failed.");
        }

        [Command("end", adminOnly: true)]
        public static void End(ChatCommandContext ctx)
        {
            if (!TryGetCurrentSession(ctx, out _, out var zoneHash))
            {
                ctx.Reply("Not in an active session.");
                return;
            }

            Plugin.GameSessions?.EndGame(zoneHash);
            ctx.Reply("[Game] End requested.");
        }

        [Command("reset", adminOnly: true)]
        public static void Reset(ChatCommandContext ctx)
        {
            if (!TryGetCurrentSession(ctx, out var session, out var zoneHash))
            {
                ctx.Reply("Not in an active session.");
                return;
            }

            Plugin.ZonePrep?.ClearSessionRuntime(session.SessionId);
            Plugin.GameSessions?.ResetToWaiting(zoneHash);
            ctx.Reply("[Game] Reset complete.");
        }

        [Command("reload", adminOnly: true)]
        public static void Reload(ChatCommandContext ctx)
        {
            Plugin.FlowRegistry?.Reload();
            Plugin.GamePresets?.InvalidateAll();
            ctx.Reply("[Game] Config reload requested.");
        }

        [Command("debug", adminOnly: true)]
        public static void Debug(ChatCommandContext ctx)
        {
            if (!TryGetCurrentSession(ctx, out var session, out _))
            {
                ctx.Reply("Not in an active session.");
                return;
            }

            ctx.Reply($"[Game] Debug: zone={session.ZoneHash} state={session.State} players={session.Players.Count} participants={session.ParticipantCount}");
        }

        [Command("stun", adminOnly: true)]
        public static void Stun(ChatCommandContext ctx, float durationSeconds)
        {
            ctx.Reply($"[Game] Stun command reserved for session flows. Duration={durationSeconds.ToString(CultureInfo.InvariantCulture)}");
        }

        [Command("unstun", adminOnly: true)]
        public static void Unstun(ChatCommandContext ctx)
        {
            ctx.Reply("[Game] Unstun command reserved for session flows.");
        }

        [Command("tpall", adminOnly: true)]
        public static void TpAllZone(ChatCommandContext ctx, string targetZoneHash)
        {
            if (!TryGetCurrentSession(ctx, out var session, out _))
            {
                ctx.Reply("Not in an active session.");
                return;
            }

            if (!int.TryParse(targetZoneHash, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedZoneHash)
                || Plugin.ZoneConfig?.TryGetZoneByHash(parsedZoneHash, out var targetZone) != true
                || targetZone == null)
            {
                ctx.Reply("Target zone not found.");
                return;
            }

            var participants = session.Players.Where(x => x.IsParticipant).Select(x => x.Player).ToArray();
            if (participants.Length == 0)
            {
                ctx.Reply("No active participants to teleport.");
                return;
            }

            var destination = targetZone.GetCenterFloat3();
            foreach (var participant in participants)
            {
                GameActionService.InvokeAction("setposition", new object[] { participant, destination });
            }

            Plugin.Logger.LogInfo($"[Game] Admin {ctx.User.CharacterName} teleported {participants.Length} session players from zone {session.ZoneHash} to zone {parsedZoneHash}.");
            ctx.Reply($"[Game] Teleported {participants.Length} participant(s) to zone {parsedZoneHash}.");
        }

        [Command("tpall", adminOnly: true)]
        public static void TpAllPosition(ChatCommandContext ctx, float x, float y, float z)
        {
            if (!TryGetCurrentSession(ctx, out var session, out _))
            {
                ctx.Reply("Not in an active session.");
                return;
            }

            var participants = session.Players.Where(x => x.IsParticipant).Select(x => x.Player).ToArray();
            if (participants.Length == 0)
            {
                ctx.Reply("No active participants to teleport.");
                return;
            }

            var destination = new float3(x, y, z);
            foreach (var participant in participants)
            {
                GameActionService.InvokeAction("setposition", new object[] { participant, destination });
            }

            Plugin.Logger.LogInfo($"[Game] Admin {ctx.User.CharacterName} teleported {participants.Length} session players from zone {session.ZoneHash} to position ({x}, {y}, {z}).");
            ctx.Reply($"[Game] Teleported {participants.Length} participant(s) to ({x}, {y}, {z}).");
        }

        private static void ExecuteReady(ChatCommandContext ctx, bool ready)
        {
            if (!TryGetCurrentSession(ctx, out _, out var zoneHash) || ctx.Event == null)
            {
                ctx.Reply("Not in a session-enabled zone.");
                return;
            }

            var player = ctx.Event.SenderCharacterEntity;
            var steamId = ctx.User.PlatformId;
            var ok = Plugin.GameSessions?.SetPlayerReady(player, steamId, zoneHash, ready) == true;
            ctx.Reply(ok ? $"[Game] {(ready ? "Ready" : "Unready")}." : "[Game] Could not update ready state.");
        }

        private static bool TryGetCurrentSession(ChatCommandContext ctx, out GameSession session, out int zoneHash)
        {
            session = null!;
            zoneHash = 0;

            if (ctx.Event == null || Plugin.ZoneTransition?.IsInitialized != true || Plugin.GameSessions?.IsInitialized != true)
            {
                return false;
            }

            Entity player = ctx.Event.SenderCharacterEntity;
            zoneHash = Plugin.ZoneTransition.GetPlayerZone(player);
            return zoneHash != 0 && Plugin.GameSessions.TryGetSession(zoneHash, out session);
        }

        private static bool TryGetCurrentZoneHash(ChatCommandContext ctx, out int zoneHash)
        {
            zoneHash = 0;

            if (ctx.Event == null || Plugin.ZoneTransition?.IsInitialized != true || Plugin.GameSessions?.IsInitialized != true)
            {
                return false;
            }

            var player = ctx.Event.SenderCharacterEntity;
            zoneHash = Plugin.ZoneTransition.GetPlayerZone(player);
            return zoneHash != 0 && Plugin.GameSessions.IsSessionEnabledZone(zoneHash);
        }
    }
}
