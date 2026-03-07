using System.Collections.Generic;
using BepInEx.Logging;
using Blueluck.Models;
using Unity.Entities;
using VAuto.Services.Interfaces;

namespace Blueluck.Services
{
    public sealed class ZonePrepService : IService
    {
        private static readonly ManualLogSource _log = Logger.CreateLogSource("Blueluck.ZonePrep");
        private readonly Dictionary<string, List<Entity>> _runtimeEntitiesByGroup = new();

        public bool IsInitialized { get; private set; }
        public ManualLogSource Log => _log;

        public void Initialize()
        {
            IsInitialized = true;
        }

        public void Cleanup()
        {
            _runtimeEntitiesByGroup.Clear();
            IsInitialized = false;
        }

        public void ExecuteFlows(GameSession session, string[] flows, Entity player)
        {
            if (Plugin.FlowRegistry?.IsInitialized == true)
            {
                Plugin.FlowRegistry.ExecuteFlows(flows, player, session.ZoneName, session.ZoneHash);
            }
        }

        public void ClearSessionRuntime(string sessionId)
        {
            var keys = new List<string>();
            foreach (var key in _runtimeEntitiesByGroup.Keys)
            {
                if (key.StartsWith(sessionId + ":", System.StringComparison.Ordinal))
                {
                    keys.Add(key);
                }
            }

            foreach (var key in keys)
            {
                _runtimeEntitiesByGroup.Remove(key);
            }
        }
    }
}
