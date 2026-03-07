using System;
using System.Linq;
using BepInEx.Logging;
using Blueluck.Models;
using Unity.Entities;
using VAuto.Services.Interfaces;

namespace Blueluck.Services
{
    public sealed class ReadyLobbyService : IService
    {
        private static readonly ManualLogSource _log = Logger.CreateLogSource("Blueluck.ReadyLobby");
        private static FlowRegistryService? FlowRegistry => Plugin.FlowRegistry?.IsInitialized == true ? Plugin.FlowRegistry : null;

        public bool IsInitialized { get; private set; }
        public ManualLogSource Log => _log;

        public void Initialize()
        {
            IsInitialized = true;
        }

        public void Cleanup()
        {
            IsInitialized = false;
        }

        public bool SetReady(GameSession session, Entity player, ulong steamId, bool ready)
        {
            var playerSession = session.Players.FirstOrDefault(x => x.SteamId == steamId);
            if (playerSession == null)
            {
                return false;
            }

            playerSession.IsReady = ready;
            var flows = ready
                ? session.Definition?.Lifecycle.PlayerReadyFlows
                : session.Definition?.Lifecycle.PlayerUnreadyFlows;

            if (flows != null && FlowRegistry != null)
            {
                FlowRegistry.ExecuteFlows(flows, player, session.ZoneName, session.ZoneHash);
            }

            return true;
        }

        public bool AreAllParticipantsReady(GameSession session)
        {
            var definition = session.Definition;
            if (definition == null)
            {
                return false;
            }

            var participants = session.Players.Where(x => x.IsParticipant).ToArray();
            if (participants.Length < definition.Session.MinPlayers)
            {
                return false;
            }

            return !definition.Session.RequireAllPresentReady || participants.All(x => x.IsReady);
        }
    }
}
