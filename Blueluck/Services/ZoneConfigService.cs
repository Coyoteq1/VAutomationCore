using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using BepInEx;
using BepInEx.Logging;
using Blueluck.Models;
using Il2CppInterop.Runtime;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using VAuto.Services.Interfaces;
using VAutomationCore.Core.ECS.Components;

namespace Blueluck.Services
{
    /// <summary>
    /// Service for loading and managing resolved zone configurations.
    /// </summary>
    public class ZoneConfigService : IService
    {
        private static readonly ManualLogSource _log = Logger.CreateLogSource("Blueluck.ZoneConfig");
        private const int FxPresetPoolSize = 400;
        private static readonly MethodInfo? SetComponentDataGeneric = typeof(EntityManager).GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(m => m.Name == "SetComponentData" && m.IsGenericMethodDefinition && m.GetParameters().Length == 2 && m.GetParameters()[0].ParameterType == typeof(Entity));

        public bool IsInitialized { get; private set; }
        public ManualLogSource Log => _log;

        private ZonesConfig _config = new();
        private readonly Dictionary<int, ZoneDefinition> _zonesByHash = new();
        private readonly Dictionary<int, ZoneDefinition> _retiredZonesByHash = new();
        private readonly List<string> _watchedConfigPaths = new();
        private readonly List<GameplayRegistrationDiagnostics> _registrationDiagnostics = new();
        private string _configDirectory = string.Empty;
        private string _gameplayConfigDirectory = string.Empty;
        private string _buffsNumberedPath = string.Empty;
        private DateTime _lastConfigCheck = DateTime.MinValue;

        public void Initialize()
        {
            _configDirectory = Path.Combine(Paths.ConfigPath, "Blueluck");
            _gameplayConfigDirectory = Path.Combine(_configDirectory, "gameplay");
            Directory.CreateDirectory(_configDirectory);
            Directory.CreateDirectory(_gameplayConfigDirectory);
            _buffsNumberedPath = ResolveBuffsNumberedPath();

            LoadConfig();
            IsInitialized = true;
            _log.LogInfo($"[ZoneConfig] Initialized with {_zonesByHash.Count} zones.");
        }

        public void SpawnZoneEntitiesIfReady()
        {
            if (!IsInitialized)
            {
                _log.LogWarning("[ZoneConfig] Cannot spawn zones - not initialized.");
                return;
            }

            SpawnZoneEntities();
        }

        public void Cleanup()
        {
            CleanupZoneEntities();
            _zonesByHash.Clear();
            _retiredZonesByHash.Clear();
            _registrationDiagnostics.Clear();
            IsInitialized = false;
            _log.LogInfo("[ZoneConfig] Cleaned up.");
        }

        public IReadOnlyList<GameplayRegistrationDiagnostics> GetRegistrationDiagnostics()
        {
            return _registrationDiagnostics.ToArray();
        }

        public IReadOnlyList<ZoneDefinition> GetZones()
        {
            var zones = new List<ZoneDefinition>();
            foreach (var zone in _zonesByHash.Values)
            {
                if (zone.Enabled)
                {
                    zones.Add(zone);
                }
            }

            return zones;
        }

        public bool TryGetZoneByHash(int hash, out ZoneDefinition zone)
        {
            return _zonesByHash.TryGetValue(hash, out zone!) || _retiredZonesByHash.TryGetValue(hash, out zone!);
        }

        public bool IsActiveZoneHash(int hash)
        {
            return _zonesByHash.ContainsKey(hash);
        }

        public void ReleaseRetiredZone(int hash)
        {
            if (hash == 0 || _zonesByHash.ContainsKey(hash))
            {
                return;
            }

            if (_retiredZonesByHash.Remove(hash))
            {
                _log.LogInfo($"[ZoneConfig] Released retired zone definition hash={hash}.");
            }
        }

        public ZoneDetectionConfig GetDetectionConfig()
        {
            return _config.Detection ?? new ZoneDetectionConfig();
        }

        public void Reload()
        {
            CleanupZoneEntities();
            LoadConfig();
            Plugin.GamePresets?.InvalidateAll();
            SpawnZoneEntities();
            _log.LogInfo("[ZoneConfig] Configuration reloaded.");
        }

        public void CheckForChanges()
        {
            if (_watchedConfigPaths.Count == 0)
            {
                return;
            }

            var newestWrite = DateTime.MinValue;
            foreach (var path in _watchedConfigPaths)
            {
                if (!File.Exists(path))
                {
                    continue;
                }

                var writeTime = File.GetLastWriteTimeUtc(path);
                if (writeTime > newestWrite)
                {
                    newestWrite = writeTime;
                }
            }

            if (newestWrite > _lastConfigCheck)
            {
                Reload();
                _lastConfigCheck = newestWrite;
            }
        }

        private void LoadConfig()
        {
            var previousZones = new Dictionary<int, ZoneDefinition>(_zonesByHash);
            _registrationDiagnostics.Clear();

            try
            {
                var arenaResult = ArenaGameplayRegistration.Register(_gameplayConfigDirectory);
                var bossResult = BossGameplayRegistration.Register(_gameplayConfigDirectory);

                _registrationDiagnostics.Add(arenaResult.Diagnostics);
                _registrationDiagnostics.Add(bossResult.Diagnostics);

                var allFlows = arenaResult.Flows.Concat(bossResult.Flows).ToArray();
                Plugin.FlowRegistry?.SetConfiguredFlows(allFlows);

                _config = new ZonesConfig
                {
                    Detection = arenaResult.Settings.Detection ?? bossResult.Settings.Detection ?? new ZoneDetectionConfig(),
                    FxPresetList = BuildDefaultFxPresetList(),
                    Zones = arenaResult.Zones.Concat(bossResult.Zones).ToArray()
                };

                NormalizeFxPresets();

                _zonesByHash.Clear();
                foreach (var zone in _config.Zones)
                {
                    if (zone.Enabled && zone.Hash != 0)
                    {
                        _zonesByHash[zone.Hash] = zone;
                    }
                }

                UpdateRetiredZones(previousZones);
                UpdateWatchedPaths();
                LogDiagnostics();
                Plugin.GameplayRegistration?.Refresh();
                Plugin.GamePresets?.InvalidateAll();
                _log.LogInfo($"[ZoneConfig] Loaded {_zonesByHash.Count} resolved zones.");
            }
            catch (Exception ex)
            {
                _log.LogError($"[ZoneConfig] Failed to load config: {ex.Message}");
                _config ??= new ZonesConfig();
                _zonesByHash.Clear();
                foreach (var pair in previousZones)
                {
                    _zonesByHash[pair.Key] = pair.Value;
                }

                _log.LogWarning($"[ZoneConfig] Preserved {previousZones.Count} previously loaded zones after config load failure.");
            }
        }

        private void UpdateWatchedPaths()
        {
            _watchedConfigPaths.Clear();

            var fileNames = new[]
            {
                "arena.settings.json",
                "arena.zones.json",
                "arena.rules.json",
                "arena_flows.config.json",
                "arena_presets.config.json",
                "boss.settings.json",
                "boss.zones.json",
                "boss.rules.json",
                "boss_flows.config.json",
                "boss_presets.config.json",
                "buffs_numbered.txt"
            };

            foreach (var fileName in fileNames)
            {
                if (string.Equals(fileName, "buffs_numbered.txt", StringComparison.OrdinalIgnoreCase))
                {
                    _watchedConfigPaths.Add(_buffsNumberedPath);
                    continue;
                }

                _watchedConfigPaths.Add(Path.Combine(_gameplayConfigDirectory, fileName));
            }
        }

        private void LogDiagnostics()
        {
            foreach (var diagnostics in _registrationDiagnostics)
            {
                foreach (var warning in diagnostics.Warnings)
                {
                    _log.LogWarning($"[{diagnostics.GameplayType}] {warning}");
                }

                foreach (var preset in diagnostics.IgnoredPresets)
                {
                    _log.LogWarning($"[{diagnostics.GameplayType}] Ignored preset: {preset}");
                }

                foreach (var flow in diagnostics.DroppedFlows)
                {
                    _log.LogWarning($"[{diagnostics.GameplayType}] Dropped flow: {flow}");
                }

                foreach (var zone in diagnostics.InvalidZones)
                {
                    _log.LogWarning($"[{diagnostics.GameplayType}] Invalid zone: {zone}");
                }
            }
        }

        private void UpdateRetiredZones(Dictionary<int, ZoneDefinition> previousZones)
        {
            foreach (var activeHash in _zonesByHash.Keys)
            {
                _retiredZonesByHash.Remove(activeHash);
            }

            foreach (var pair in previousZones)
            {
                if (!_zonesByHash.ContainsKey(pair.Key))
                {
                    _retiredZonesByHash[pair.Key] = pair.Value;
                }
            }
        }

        private string ResolveBuffsNumberedPath()
        {
            var configDataCandidate = Path.Combine(Paths.ConfigPath, "Blueluck", "Data", "buffs_numbered.txt");
            if (File.Exists(configDataCandidate))
            {
                return configDataCandidate;
            }

            var pluginDataCandidate = Path.Combine(Paths.PluginPath, "Blueluck", "Data", "buffs_numbered.txt");
            if (File.Exists(pluginDataCandidate))
            {
                return pluginDataCandidate;
            }

            var assemblyConfigCandidate = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty, "config", "buffs_numbered.txt");
            if (File.Exists(assemblyConfigCandidate))
            {
                return assemblyConfigCandidate;
            }

            var pluginConfigCandidate = Path.Combine(Paths.PluginPath, "Blueluck", "config", "buffs_numbered.txt");
            if (File.Exists(pluginConfigCandidate))
            {
                return pluginConfigCandidate;
            }

            return Path.Combine(Paths.ConfigPath, "Blueluck", "buffs_numbered.txt");
        }

        private static int ComputePreAssignedFxPresetIndex(int zoneHash, int poolSize)
        {
            if (poolSize <= 0)
            {
                return 1;
            }

            var value = zoneHash == 0 ? 1 : Math.Abs(zoneHash);
            return ((value - 1) % poolSize) + 1;
        }

        private int[] BuildDefaultFxPresetList()
        {
            var result = new List<int>(FxPresetPoolSize);
            try
            {
                if (File.Exists(_buffsNumberedPath))
                {
                    foreach (var line in File.ReadLines(_buffsNumberedPath))
                    {
                        if (result.Count >= FxPresetPoolSize)
                        {
                            break;
                        }

                        var match = Regex.Match(line, @"^\s*\d+\.\s*(-?\d+)\s*$");
                        if (!match.Success || !int.TryParse(match.Groups[1].Value, out var value))
                        {
                            continue;
                        }

                        result.Add(value);
                    }
                }
            }
            catch (Exception ex)
            {
                _log.LogWarning($"[ZoneConfig] Failed reading FX source '{_buffsNumberedPath}': {ex.Message}");
            }

            while (result.Count < FxPresetPoolSize)
            {
                result.Add(0);
            }

            return result.Take(FxPresetPoolSize).ToArray();
        }

        private void NormalizeFxPresets()
        {
            _config ??= new ZonesConfig();
            _config.Zones ??= Array.Empty<ZoneDefinition>();

            if (_config.FxPresetList == null || _config.FxPresetList.Length != FxPresetPoolSize)
            {
                _config.FxPresetList = BuildDefaultFxPresetList();
            }

            foreach (var zone in _config.Zones)
            {
                if (zone == null)
                {
                    continue;
                }

                var normalizedEntry = zone.EntryRadius > 0.001f ? zone.EntryRadius : 50f;
                zone.EntryRadius = normalizedEntry;

                var normalizedExit = zone.ExitRadius > 0.001f ? zone.ExitRadius : normalizedEntry;
                if (normalizedExit < normalizedEntry)
                {
                    normalizedExit = normalizedEntry;
                }

                zone.ExitRadius = normalizedExit;

                if (zone.FxPresetIndex <= 0)
                {
                    zone.FxPresetIndex = ComputePreAssignedFxPresetIndex(zone.Hash, _config.FxPresetList.Length);
                }

                if (zone.FxPresetIndex > _config.FxPresetList.Length)
                {
                    zone.FxPresetIndex = _config.FxPresetList.Length;
                }

                var presetGuid = _config.FxPresetList[zone.FxPresetIndex - 1];
                zone.BorderVisual ??= new BorderVisualConfig();

                var needsDefaultPrefab = zone.BorderVisual.BuffPrefabs == null || zone.BorderVisual.BuffPrefabs.Length == 0;
                if (string.IsNullOrWhiteSpace(zone.BorderVisual.Effect) && needsDefaultPrefab)
                {
                    zone.BorderVisual.Effect = "custom";
                }

                if (needsDefaultPrefab)
                {
                    zone.BorderVisual.BuffPrefabs = new[] { presetGuid.ToString() };
                }

                var availableTiers = zone.BorderVisual.BuffPrefabs?.Length ?? 0;
                if (zone.BorderVisual.IntensityMax <= 0)
                {
                    zone.BorderVisual.IntensityMax = Math.Max(1, availableTiers);
                }
                else if (availableTiers > 0 && zone.BorderVisual.IntensityMax > availableTiers)
                {
                    zone.BorderVisual.IntensityMax = availableTiers;
                }
            }
        }

        private void SpawnZoneEntities()
        {
            try
            {
                var world = World.DefaultGameObjectInjectionWorld;
                if (world == null || !world.IsCreated)
                {
                    _log.LogWarning("[ZoneConfig] World not ready - cannot spawn zone entities.");
                    return;
                }

                var em = world.EntityManager;
                var spawnedCount = 0;
                var zoneType = Il2CppType.Of<ZoneComponent>(throwOnFailure: false);
                var localToWorldType = Il2CppType.Of<LocalToWorld>(throwOnFailure: false);

                if (zoneType == null || localToWorldType == null)
                {
                    _log.LogWarning("[ZoneConfig] Zone entity spawn skipped: required IL2CPP component types unavailable.");
                    return;
                }

                var zoneComponentType = new ComponentType(zoneType, ComponentType.AccessMode.ReadWrite);
                var localToWorldComponentType = new ComponentType(localToWorldType, ComponentType.AccessMode.ReadWrite);

                foreach (var zone in _zonesByHash.Values)
                {
                    if (!zone.Enabled || zone.Hash == 0)
                    {
                        continue;
                    }

                    var zoneEntity = em.CreateEntity(zoneComponentType, localToWorldComponentType);
                    var zoneComponent = new ZoneComponent
                    {
                        ZoneHash = zone.Hash,
                        Priority = zone.Priority,
                        Center = zone.GetCenterFloat3(),
                        EntryRadius = zone.EntryRadius,
                        ExitRadius = zone.ExitRadius,
                        EntryRadiusSq = zone.EntryRadius * zone.EntryRadius,
                        ExitRadiusSq = zone.ExitRadius * zone.ExitRadius
                    };

                    WriteComponentData(em, zoneEntity, zoneComponent);
                    WriteComponentData(em, zoneEntity, new LocalToWorld { Value = Float4x4Translate(zoneComponent.Center) });
                    spawnedCount++;
                }

                _log.LogInfo($"[ZoneConfig] Spawned {spawnedCount} zone entities for detection.");
            }
            catch (Exception ex)
            {
                _log.LogError($"[ZoneConfig] Failed to spawn zone entities: {ex.Message}");
            }
        }

        private void CleanupZoneEntities()
        {
            try
            {
                var world = World.DefaultGameObjectInjectionWorld;
                if (world == null || !world.IsCreated)
                {
                    return;
                }

                var em = world.EntityManager;
                var zoneType = Il2CppType.Of<ZoneComponent>(throwOnFailure: false);
                if (zoneType == null)
                {
                    return;
                }

                var query = em.CreateEntityQuery(new ComponentType(zoneType, ComponentType.AccessMode.ReadOnly));
                if (query.CalculateEntityCount() == 0)
                {
                    return;
                }

                var entities = query.ToEntityArray(Unity.Collections.Allocator.Temp);
                try
                {
                    foreach (var entity in entities)
                    {
                        if (em.Exists(entity))
                        {
                            em.DestroyEntity(entity);
                        }
                    }
                }
                finally
                {
                    entities.Dispose();
                }
            }
            catch (Exception ex)
            {
                _log.LogWarning($"[ZoneConfig] Failed to cleanup zone entities: {ex.Message}");
            }
        }

        private static float4x4 Float4x4Translate(float3 position)
        {
            return new float4x4(
                1, 0, 0, 0,
                0, 1, 0, 0,
                0, 0, 1, 0,
                position.x, position.y, position.z, 1);
        }

        private static void WriteComponentData<T>(EntityManager em, Entity entity, T value) where T : struct
        {
            try
            {
                if (em.HasComponent<T>(entity))
                {
                    em.SetComponentData(entity, value);
                    return;
                }

                if (SetComponentDataGeneric != null)
                {
                    SetComponentDataGeneric.MakeGenericMethod(typeof(T)).Invoke(em, new object[] { entity, value });
                }
            }
            catch (Exception ex)
            {
                _log.LogWarning($"[ZoneConfig] Failed to write component {typeof(T).Name}: {ex.Message}");
            }
        }
    }
}
