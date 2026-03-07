using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using Il2CppInterop.Runtime;
using Unity.Entities;
using VAuto.Services.Interfaces;
using VAutomationCore.Core.ECS.Components;

namespace Blueluck.Services
{
    public sealed class GameplayRegistrationService : IService
    {
        private static readonly ManualLogSource _log = Logger.CreateLogSource("Blueluck.GameplayRegistrationSnapshot");

        private readonly List<int> _registeredZoneHashes = new();
        private readonly List<string> _registeredFlowIds = new();
        private readonly List<int> _ecsZoneHashes = new();

        public bool IsInitialized { get; private set; }
        public ManualLogSource Log => _log;

        public void Initialize()
        {
            IsInitialized = true;
            Refresh();
            _log.LogInfo("[GameplayRegistrationSnapshot] Initialized.");
        }

        public void Cleanup()
        {
            _registeredZoneHashes.Clear();
            _registeredFlowIds.Clear();
            _ecsZoneHashes.Clear();
            IsInitialized = false;
            _log.LogInfo("[GameplayRegistrationSnapshot] Cleaned up.");
        }

        public void Refresh()
        {
            _registeredZoneHashes.Clear();
            _registeredFlowIds.Clear();
            _ecsZoneHashes.Clear();

            if (Plugin.ZoneConfig?.IsInitialized == true)
            {
                _registeredZoneHashes.AddRange(Plugin.ZoneConfig.GetZones().Select(x => x.Hash).OrderBy(x => x));
            }

            if (Plugin.FlowRegistry?.IsInitialized == true)
            {
                _registeredFlowIds.AddRange(Plugin.FlowRegistry.GetFlowIds().OrderBy(x => x, StringComparer.OrdinalIgnoreCase));
            }

            RefreshEcsZoneHashes();
        }

        public IReadOnlyList<int> GetRegisteredZoneHashes() => _registeredZoneHashes.ToArray();

        public IReadOnlyList<string> GetRegisteredFlowIds() => _registeredFlowIds.ToArray();

        public IReadOnlyList<int> GetEcsZoneHashes() => _ecsZoneHashes.ToArray();

        private void RefreshEcsZoneHashes()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null || !world.IsCreated)
            {
                return;
            }

            try
            {
                var em = world.EntityManager;
                var zoneType = Il2CppType.Of<ZoneComponent>(throwOnFailure: false);
                if (zoneType == null)
                {
                    return;
                }

                var query = em.CreateEntityQuery(new ComponentType(zoneType, ComponentType.AccessMode.ReadOnly));
                var zones = query.ToComponentDataArray<ZoneComponent>(Unity.Collections.Allocator.Temp);
                try
                {
                    foreach (var zone in zones)
                    {
                        if (!_ecsZoneHashes.Contains(zone.ZoneHash))
                        {
                            _ecsZoneHashes.Add(zone.ZoneHash);
                        }
                    }
                }
                finally
                {
                    zones.Dispose();
                }
            }
            catch (Exception ex)
            {
                _log.LogDebug($"[GameplayRegistrationSnapshot] ECS zone snapshot refresh skipped: {ex.Message}");
            }
        }
    }
}
