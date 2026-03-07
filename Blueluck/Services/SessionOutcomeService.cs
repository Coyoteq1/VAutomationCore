using System;
using System.Linq;
using BepInEx.Logging;
using Blueluck.Models;
using VAuto.Services.Interfaces;

namespace Blueluck.Services
{
    public sealed class SessionOutcomeService : IService
    {
        private static readonly ManualLogSource _log = Logger.CreateLogSource("Blueluck.SessionOutcome");

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

        public bool ShouldEndArena(GameSession session)
        {
            var objective = session.Definition?.Objective.ObjectiveType ?? string.Empty;
            if (string.Equals(objective, "last_player_standing", StringComparison.OrdinalIgnoreCase))
            {
                return session.AliveParticipantCount <= 1 && session.ParticipantCount > 0;
            }

            return false;
        }

        public bool ShouldEndBoss(GameSession session)
        {
            return string.Equals(session.Definition?.Objective.ObjectiveType, "boss_defeat", StringComparison.OrdinalIgnoreCase)
                && session.Players.Any(x => x.IsParticipant);
        }
    }
}
