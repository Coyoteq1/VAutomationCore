using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Logging;
using Blueluck.Models;
using ProjectM;
using Stunlock.Core;
using Unity.Entities;
using Unity.Mathematics;
using VAuto.Core;
using VAuto.Services.Interfaces;
using VAutomationCore.Core;
using VAutomationCore.Core.Api;
using VAutomationCore.Core.Services;
using VAutomationCore.Services;

namespace Blueluck.Services
{
    /// <summary>
    /// Service for managing and executing resolved arena/boss flows.
    /// </summary>
    public class FlowRegistryService : IService
    {
        private static readonly ManualLogSource _log = Logger.CreateLogSource("Blueluck.FlowRegistry");
        private const string BridgeModId = "Blueluck";
        private const string BossSpawnTopic = "boss.spawn";
        private const string BloodyBossSpawnTopic = "bloodyboss.spawn";

        public bool IsInitialized { get; private set; }
        public ManualLogSource Log => _log;

        private readonly Dictionary<string, GameplayFlowDefinitionConfig> _flowDefinitions = new(StringComparer.OrdinalIgnoreCase);
        private PrefabToGuidService? _prefabToGuid;
        private readonly Dictionary<int, List<Entity>> _borderFxByZone = new();
        private readonly Dictionary<int, List<Entity>> _bossesByZone = new();
        private Action<string, object>? _bossSpawnMessageHandler;

        public static class FlowActions
        {
            public const string SetPvp = "zone.setpvp";
            public const string SendMessage = "zone.sendmessage";
            public const string SpawnBoss = "zone.spawnboss";
            public const string RemoveBoss = "zone.removeboss";
            public const string ApplyBorderFx = "zone.applyborderfx";
            public const string RemoveBorderFx = "zone.removeborderfx";
            public const string ApplyZoneBuff = "zone.applyzonebuff";
            public const string RemoveZoneBuff = "zone.removezonebuff";
            public const string EnableCoop = "zone.enablecoop";
            public const string DisableCoop = "zone.disablecoop";
            public const string TriggerCoopEvent = "zone.triggercoop";
            public const string ArenaApplyRuleProfile = "arena.applyruleprofile";
            public const string ArenaRestoreSafeCombatState = "arena.restoresafecombatstate";
            public const string ArenaApplyLoadoutState = "arena.applyloadoutstate";
            public const string ArenaRestoreLoadoutState = "arena.restoreloadoutstate";
            public const string ArenaApplyProgressionGate = "arena.applyprogressiongate";
            public const string ArenaRestoreProgressionState = "arena.restoreprogressionstate";
            public const string ArenaCaptureSnapshot = "arena.captureplayersnapshot";
            public const string ArenaRestoreSnapshot = "arena.restoreplayersnapshot";
            public const string ArenaApplyZoneVisuals = "arena.applyzonevisuals";
            public const string ArenaClearZoneVisuals = "arena.clearzonevisuals";
            public const string BossCreateEncounterGroup = "boss.createencountergroup";
            public const string BossPrepareEncounterState = "boss.prepareencounterstate";
            public const string BossSpawnEncounter = "boss.spawnencounter";
            public const string BossApplyEncounterVisuals = "boss.applyencountervisuals";
            public const string BossCleanupEncounterGroup = "boss.cleanupencountergroup";
            public const string BossUnwindEncounterState = "boss.unwindencounterstate";
            public const string BossRestoreEncounterOverrides = "boss.restoreencounteroverrides";
        }

        public void Initialize()
        {
            _prefabToGuid = Plugin.PrefabToGuid;
            RegisterFlowActionAliases();
            RegisterCrossModBossBridge();
            IsInitialized = true;
            _log.LogInfo("[FlowRegistry] Initialized.");
        }

        public void Cleanup()
        {
            UnregisterCrossModBossBridge();
            CleanupAllSpawnedEntities();
            _flowDefinitions.Clear();
            IsInitialized = false;
            _log.LogInfo("[FlowRegistry] Cleaned up.");
        }

        public void SetConfiguredFlows(IEnumerable<GameplayFlowDefinitionConfig> flowDefinitions)
        {
            _flowDefinitions.Clear();
            foreach (var definition in flowDefinitions ?? Array.Empty<GameplayFlowDefinitionConfig>())
            {
                if (definition == null || string.IsNullOrWhiteSpace(definition.FlowId))
                {
                    continue;
                }

                definition.Actions ??= Array.Empty<FlowAction>();
                _flowDefinitions[definition.FlowId] = definition;
            }

            _log.LogInfo($"[FlowRegistry] Registered {_flowDefinitions.Count} configured flows.");
        }

        public void Reload()
        {
            Plugin.ZoneConfig?.Reload();
        }

        public bool FlowExists(string flowId)
        {
            return !string.IsNullOrWhiteSpace(flowId) && _flowDefinitions.ContainsKey(flowId);
        }

        public bool TryGetFlowDefinition(string flowId, out GameplayFlowDefinitionConfig definition)
        {
            return _flowDefinitions.TryGetValue(flowId, out definition!);
        }

        public IReadOnlyCollection<string> GetFlowIds()
        {
            return _flowDefinitions.Keys.ToArray();
        }

        public void ExecuteFlow(string flowId, Entity player, string zoneId, int zoneHash)
        {
            if (string.IsNullOrWhiteSpace(flowId) || player == Entity.Null)
            {
                _log.LogWarning($"[FlowRegistry] ExecuteFlow skipped: invalid flowId/player. flowId='{flowId ?? "<null>"}' player={player.Index} zoneHash={zoneHash}");
                return;
            }

            if (!_flowDefinitions.TryGetValue(flowId, out var definition))
            {
                _log.LogWarning($"[FlowRegistry] Flow not found: {flowId}");
                return;
            }

            if (definition.Actions == null || definition.Actions.Length == 0)
            {
                _log.LogWarning($"[FlowRegistry] Flow '{flowId}' has no actions. zone={zoneId} hash={zoneHash} player={player.Index}");
                return;
            }

            _log.LogInfo($"[FlowRegistry] Executing flow '{flowId}' with {definition.Actions.Length} action(s). zone={zoneId} hash={zoneHash} player={player.Index}");
            foreach (var action in definition.Actions)
            {
                ExecuteAction(action, player, zoneId, zoneHash);
            }
        }

        public void ExecuteFlows(IEnumerable<string> flowIds, Entity player, string zoneId, int zoneHash)
        {
            var resolved = (flowIds ?? Array.Empty<string>()).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
            if (resolved.Length == 0)
            {
                _log.LogWarning($"[FlowRegistry] ExecuteFlows skipped: no flow ids resolved. zone={zoneId} hash={zoneHash} player={player.Index}");
                return;
            }

            _log.LogInfo($"[FlowRegistry] Executing {resolved.Length} flow id(s): {string.Join(", ", resolved)}. zone={zoneId} hash={zoneHash} player={player.Index}");
            foreach (var flowId in resolved)
            {
                ExecuteFlow(flowId, player, zoneId, zoneHash);
            }
        }

        public void SetPvp(Entity player, bool enabled, int zoneHash = 0)
        {
            if (player == Entity.Null)
            {
                return;
            }

            HandleSetPvp(new FlowAction { Action = FlowActions.SetPvp, Value = enabled }, player, zoneHash);
        }

        public void SendMessage(Entity player, string message, int zoneHash = 0)
        {
            if (player == Entity.Null || string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            HandleSendMessage(new FlowAction { Action = FlowActions.SendMessage, Message = message }, player, zoneHash);
        }

        public void ApplyBorderFx(Entity player, int zoneHash, string vfxPrefab, float radius, int segments)
        {
            if (player == Entity.Null)
            {
                return;
            }

            HandleApplyBorderFx(new FlowAction
            {
                Action = FlowActions.ApplyBorderFx,
                VfxPrefab = vfxPrefab,
                Radius = radius,
                Segments = segments
            }, player, zoneHash);
        }

        public void RemoveBorderFx(int zoneHash)
        {
            RemoveBorderFxInternal(zoneHash);
        }

        public bool EnsureBosses(Entity player, int zoneHash, string bossPrefab, int quantity = 1, bool randomInZone = true, float3? position = null)
        {
            return EnsureBosses(zoneHash, bossPrefab, quantity, randomInZone, position, player);
        }

        public bool EnsureBosses(int zoneHash, string bossPrefab, int quantity = 1, bool randomInZone = true, float3? position = null)
        {
            return EnsureBosses(zoneHash, bossPrefab, quantity, randomInZone, position, Entity.Null);
        }

        private bool EnsureBosses(int zoneHash, string bossPrefab, int quantity, bool randomInZone, float3? position, Entity player)
        {
            if (zoneHash == 0 || string.IsNullOrWhiteSpace(bossPrefab))
            {
                return false;
            }

            PruneDeadBossEntities(zoneHash);
            if (_bossesByZone.TryGetValue(zoneHash, out var existing) && existing.Count > 0)
            {
                return false;
            }

            var action = new FlowAction
            {
                Action = FlowActions.SpawnBoss,
                Prefab = bossPrefab,
                Quantity = quantity,
                RandomInZone = randomInZone
            };
            if (position.HasValue)
            {
                action.Position = new[] { position.Value.x, position.Value.y, position.Value.z };
            }

            HandleSpawnBoss(action, player, zoneHash);
            return true;
        }

        public void RemoveBosses(int zoneHash)
        {
            RemoveBossesInternal(zoneHash);
        }

        public bool TryGetFlow(string flowId, out List<FlowAction> actions)
        {
            actions = new List<FlowAction>();
            if (!_flowDefinitions.TryGetValue(flowId, out var definition))
            {
                return false;
            }

            actions.AddRange(definition.Actions);
            return true;
        }

        private void RegisterFlowActionAliases()
        {
            try
            {
                var aliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    [FlowActions.SetPvp] = "SetPvp",
                    [FlowActions.SendMessage] = "SendMessage",
                    [FlowActions.SpawnBoss] = "SpawnBoss",
                    [FlowActions.RemoveBoss] = "RemoveBoss",
                    [FlowActions.ApplyBorderFx] = "ApplyBorderFx",
                    [FlowActions.RemoveBorderFx] = "RemoveBorderFx",
                    [FlowActions.ApplyZoneBuff] = "ApplyZoneBuff",
                    [FlowActions.RemoveZoneBuff] = "RemoveZoneBuff",
                    [FlowActions.EnableCoop] = "EnableCoop",
                    [FlowActions.DisableCoop] = "DisableCoop",
                    [FlowActions.TriggerCoopEvent] = "TriggerCoopEvent",
                    [FlowActions.ArenaApplyRuleProfile] = "ArenaApplyRuleProfile",
                    [FlowActions.ArenaRestoreSafeCombatState] = "ArenaRestoreSafeCombatState",
                    [FlowActions.ArenaApplyLoadoutState] = "ArenaApplyLoadoutState",
                    [FlowActions.ArenaRestoreLoadoutState] = "ArenaRestoreLoadoutState",
                    [FlowActions.ArenaApplyProgressionGate] = "ArenaApplyProgressionGate",
                    [FlowActions.ArenaRestoreProgressionState] = "ArenaRestoreProgressionState",
                    [FlowActions.ArenaCaptureSnapshot] = "ArenaCaptureSnapshot",
                    [FlowActions.ArenaRestoreSnapshot] = "ArenaRestoreSnapshot",
                    [FlowActions.ArenaApplyZoneVisuals] = "ArenaApplyZoneVisuals",
                    [FlowActions.ArenaClearZoneVisuals] = "ArenaClearZoneVisuals",
                    [FlowActions.BossCreateEncounterGroup] = "BossCreateEncounterGroup",
                    [FlowActions.BossPrepareEncounterState] = "BossPrepareEncounterState",
                    [FlowActions.BossSpawnEncounter] = "BossSpawnEncounter",
                    [FlowActions.BossApplyEncounterVisuals] = "BossApplyEncounterVisuals",
                    [FlowActions.BossCleanupEncounterGroup] = "BossCleanupEncounterGroup",
                    [FlowActions.BossUnwindEncounterState] = "BossUnwindEncounterState",
                    [FlowActions.BossRestoreEncounterOverrides] = "BossRestoreEncounterOverrides"
                };

                foreach (var alias in aliases)
                {
                    FlowService.RegisterActionAlias(alias.Key, alias.Value, replace: true);
                }
            }
            catch (Exception ex)
            {
                _log.LogWarning($"[FlowRegistry] Failed to register action aliases: {ex.Message}");
            }
        }

        private void RegisterCrossModBossBridge()
        {
            if (_bossSpawnMessageHandler != null)
            {
                return;
            }

            _bossSpawnMessageHandler = HandleBossSpawnMessage;

            try
            {
                ModCommunicationService.Instance.Subscribe(BridgeModId, BossSpawnTopic, _bossSpawnMessageHandler);
                ModCommunicationService.Instance.Subscribe(BridgeModId, BloodyBossSpawnTopic, _bossSpawnMessageHandler);
                _log.LogInfo($"[FlowRegistry] Cross-mod boss bridge subscribed: {BridgeModId}.{BossSpawnTopic}, {BridgeModId}.{BloodyBossSpawnTopic}");
            }
            catch (Exception ex)
            {
                _log.LogWarning($"[FlowRegistry] Failed to register cross-mod boss bridge: {ex.Message}");
                _bossSpawnMessageHandler = null;
            }
        }

        private void UnregisterCrossModBossBridge()
        {
            if (_bossSpawnMessageHandler == null)
            {
                return;
            }

            try
            {
                ModCommunicationService.Instance.Unsubscribe(BridgeModId, BossSpawnTopic, _bossSpawnMessageHandler);
                ModCommunicationService.Instance.Unsubscribe(BridgeModId, BloodyBossSpawnTopic, _bossSpawnMessageHandler);
            }
            catch (Exception ex)
            {
                _log.LogDebug($"[FlowRegistry] Cross-mod boss bridge unsubscribe skipped: {ex.Message}");
            }
            finally
            {
                _bossSpawnMessageHandler = null;
            }
        }

        private void HandleBossSpawnMessage(string fromMod, object payload)
        {
            try
            {
                if (!TryBuildBossSpawnRequest(payload, out var request, out var error))
                {
                    _log.LogWarning($"[FlowRegistry] Rejected boss spawn request from '{fromMod}': {error}");
                    return;
                }

                var ok = EnsureBosses(
                    request.ZoneHash,
                    request.Prefab,
                    request.Quantity,
                    request.RandomInZone,
                    request.Position);

                if (!ok)
                {
                    _log.LogWarning($"[FlowRegistry] Boss spawn request from '{fromMod}' did not execute. zoneHash={request.ZoneHash} prefab={request.Prefab}");
                    return;
                }

                _log.LogInfo($"[FlowRegistry] Boss spawn request accepted from '{fromMod}'. zoneHash={request.ZoneHash} prefab={request.Prefab} qty={request.Quantity} random={request.RandomInZone}");
            }
            catch (Exception ex)
            {
                _log.LogError($"[FlowRegistry] Boss spawn bridge failed for '{fromMod}': {ex.Message}");
            }
        }

        private bool TryBuildBossSpawnRequest(object payload, out ExternalBossSpawnRequest request, out string error)
        {
            request = default;
            error = string.Empty;

            if (payload == null)
            {
                error = "payload is null";
                return false;
            }

            if (!TryReadString(payload, new[] { "prefab", "bossPrefab", "boss", "unit" }, out var prefab) ||
                string.IsNullOrWhiteSpace(prefab))
            {
                error = "missing prefab/bossPrefab";
                return false;
            }

            var hasPosition = TryReadFloat3(payload, new[] { "position", "spawnPosition", "pos", "location" }, out var position);
            var hasRandomInZone = TryReadBool(payload, new[] { "randomInZone", "random", "useRandomSpawn" }, out var randomInZone);
            var quantity = TryReadInt(payload, new[] { "quantity", "qty", "count" }, out var parsedQuantity) && parsedQuantity > 0
                ? parsedQuantity
                : 1;

            var player = ResolveRequestedPlayer(payload);
            var zoneHash = ResolveRequestedZoneHash(payload, player);
            if (zoneHash == 0)
            {
                error = "missing valid zoneHash/zoneName and no player zone fallback was resolved";
                return false;
            }

            request = new ExternalBossSpawnRequest(
                Prefab: prefab.Trim(),
                ZoneHash: zoneHash,
                Quantity: quantity,
                RandomInZone: hasRandomInZone ? randomInZone : !hasPosition,
                Position: hasPosition ? position : null);
            return true;
        }

        private Entity ResolveRequestedPlayer(object payload)
        {
            if (TryReadULong(payload, new[] { "platformId", "playerPlatformId", "steamId", "playerSteamId" }, out var platformId) &&
                TryResolvePlayerEntity(platformId, out var player))
            {
                return player;
            }

            return Entity.Null;
        }

        private int ResolveRequestedZoneHash(object payload, Entity player)
        {
            if (TryReadInt(payload, new[] { "zoneHash", "zone", "hash" }, out var zoneHash) &&
                zoneHash != 0 &&
                Plugin.ZoneConfig?.TryGetZoneByHash(zoneHash, out _) == true)
            {
                return zoneHash;
            }

            if (TryReadString(payload, new[] { "zoneName", "zoneId", "name" }, out var zoneName) &&
                TryResolveZoneHashByName(zoneName, out zoneHash))
            {
                return zoneHash;
            }

            return player != Entity.Null && Plugin.ZoneTransition != null
                ? Plugin.ZoneTransition.GetPlayerZone(player)
                : 0;
        }

        private bool TryResolveZoneHashByName(string? zoneName, out int zoneHash)
        {
            zoneHash = 0;
            if (string.IsNullOrWhiteSpace(zoneName) || Plugin.ZoneConfig == null)
            {
                return false;
            }

            var normalized = zoneName.Trim();
            var match = Plugin.ZoneConfig
                .GetZones()
                .FirstOrDefault(zone => string.Equals(zone.Name, normalized, StringComparison.OrdinalIgnoreCase));
            if (match == null)
            {
                return false;
            }

            zoneHash = match.Hash;
            return zoneHash != 0;
        }

        private static bool TryResolvePlayerEntity(ulong platformId, out Entity player)
        {
            player = Entity.Null;
            if (platformId == 0)
            {
                return false;
            }

            try
            {
                var em = UnifiedCore.EntityManager;
                if (em == default)
                {
                    return false;
                }

                var query = em.CreateEntityQuery(ComponentType.ReadOnly<PlayerCharacter>());
                var players = query.ToEntityArray(Unity.Collections.Allocator.Temp);
                try
                {
                    for (var i = 0; i < players.Length; i++)
                    {
                        var candidate = players[i];
                        if (!em.Exists(candidate) || !em.HasComponent<PlayerCharacter>(candidate))
                        {
                            continue;
                        }

                        var playerCharacter = em.GetComponentData<PlayerCharacter>(candidate);
                        if (playerCharacter.UserEntity == Entity.Null ||
                            !em.Exists(playerCharacter.UserEntity) ||
                            !em.HasComponent<User>(playerCharacter.UserEntity))
                        {
                            continue;
                        }

                        var user = em.GetComponentData<User>(playerCharacter.UserEntity);
                        if (user.PlatformId != platformId)
                        {
                            continue;
                        }

                        player = candidate;
                        return true;
                    }
                }
                finally
                {
                    players.Dispose();
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        private static bool TryReadString(object payload, string[] names, out string value)
        {
            value = string.Empty;
            if (!TryGetPayloadValue(payload, names, out var raw) || raw == null)
            {
                return false;
            }

            value = raw.ToString() ?? string.Empty;
            return !string.IsNullOrWhiteSpace(value);
        }

        private static bool TryReadInt(object payload, string[] names, out int value)
        {
            value = 0;
            if (!TryGetPayloadValue(payload, names, out var raw) || raw == null)
            {
                return false;
            }

            try
            {
                value = raw switch
                {
                    int i => i,
                    long l => checked((int)l),
                    short s => s,
                    byte b => b,
                    uint ui => checked((int)ui),
                    _ => Convert.ToInt32(raw)
                };
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool TryReadULong(object payload, string[] names, out ulong value)
        {
            value = 0;
            if (!TryGetPayloadValue(payload, names, out var raw) || raw == null)
            {
                return false;
            }

            try
            {
                value = raw switch
                {
                    ulong ul => ul,
                    long l when l >= 0 => (ulong)l,
                    uint ui => ui,
                    int i when i >= 0 => (ulong)i,
                    _ => Convert.ToUInt64(raw)
                };
                return value != 0;
            }
            catch
            {
                return false;
            }
        }

        private static bool TryReadBool(object payload, string[] names, out bool value)
        {
            value = false;
            if (!TryGetPayloadValue(payload, names, out var raw) || raw == null)
            {
                return false;
            }

            try
            {
                value = raw switch
                {
                    bool b => b,
                    string s when bool.TryParse(s, out var parsed) => parsed,
                    _ => Convert.ToBoolean(raw)
                };
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool TryReadFloat3(object payload, string[] names, out float3 value)
        {
            value = default;
            if (!TryGetPayloadValue(payload, names, out var raw) || raw == null)
            {
                return TryReadFloat3FromComponents(payload, out value);
            }

            switch (raw)
            {
                case float3 f3:
                    value = f3;
                    return true;
                case float[] arr when arr.Length >= 3:
                    value = new float3(arr[0], arr[1], arr[2]);
                    return true;
                case double[] arr when arr.Length >= 3:
                    value = new float3((float)arr[0], (float)arr[1], (float)arr[2]);
                    return true;
                case int[] arr when arr.Length >= 3:
                    value = new float3(arr[0], arr[1], arr[2]);
                    return true;
            }

            if (TryGetPayloadValue(raw, new[] { "x" }, out var xRaw) &&
                TryGetPayloadValue(raw, new[] { "y" }, out var yRaw) &&
                TryGetPayloadValue(raw, new[] { "z" }, out var zRaw) &&
                TryConvertToFloat(xRaw, out var x) &&
                TryConvertToFloat(yRaw, out var y) &&
                TryConvertToFloat(zRaw, out var z))
            {
                value = new float3(x, y, z);
                return true;
            }

            return TryReadFloat3FromComponents(payload, out value);
        }

        private static bool TryReadFloat3FromComponents(object payload, out float3 value)
        {
            value = default;
            return TryGetPayloadValue(payload, new[] { "x", "posX", "spawnX" }, out var xRaw) &&
                   TryGetPayloadValue(payload, new[] { "y", "posY", "spawnY" }, out var yRaw) &&
                   TryGetPayloadValue(payload, new[] { "z", "posZ", "spawnZ" }, out var zRaw) &&
                   TryConvertToFloat(xRaw, out var x) &&
                   TryConvertToFloat(yRaw, out var y) &&
                   TryConvertToFloat(zRaw, out var z) &&
                   AssignFloat3(x, y, z, out value);
        }

        private static bool AssignFloat3(float x, float y, float z, out float3 value)
        {
            value = new float3(x, y, z);
            return true;
        }

        private static bool TryConvertToFloat(object? raw, out float value)
        {
            value = 0f;
            if (raw == null)
            {
                return false;
            }

            try
            {
                value = raw switch
                {
                    float f => f,
                    double d => (float)d,
                    int i => i,
                    long l => l,
                    _ => Convert.ToSingle(raw)
                };
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool TryGetPayloadValue(object payload, string[] names, out object? value)
        {
            value = null;
            if (payload == null || names == null || names.Length == 0)
            {
                return false;
            }

            if (payload is IReadOnlyDictionary<string, object> readOnlyDictionary)
            {
                foreach (var name in names)
                {
                    var match = readOnlyDictionary.FirstOrDefault(pair => string.Equals(pair.Key, name, StringComparison.OrdinalIgnoreCase));
                    if (!string.IsNullOrEmpty(match.Key))
                    {
                        value = match.Value;
                        return true;
                    }
                }
            }

            if (payload is IDictionary<string, object> dictionary)
            {
                foreach (var name in names)
                {
                    var match = dictionary.FirstOrDefault(pair => string.Equals(pair.Key, name, StringComparison.OrdinalIgnoreCase));
                    if (!string.IsNullOrEmpty(match.Key))
                    {
                        value = match.Value;
                        return true;
                    }
                }
            }

            var property = payload
                .GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .FirstOrDefault(p => p.CanRead && names.Any(name => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase)));
            if (property == null)
            {
                return false;
            }

            value = property.GetValue(payload);
            return true;
        }

        private readonly record struct ExternalBossSpawnRequest(
            string Prefab,
            int ZoneHash,
            int Quantity,
            bool RandomInZone,
            float3? Position);

        private void ExecuteAction(FlowAction action, Entity player, string zoneId, int zoneHash)
        {
            try
            {
                if (action == null || string.IsNullOrWhiteSpace(action.Action))
                {
                    _log.LogWarning($"[FlowRegistry] ExecuteAction skipped: action null/empty. zone={zoneId} hash={zoneHash} player={player.Index}");
                    return;
                }

                _log.LogInfo($"[FlowRegistry] Action '{action.Action}' executing. zone={zoneId} hash={zoneHash} player={player.Index}");
                switch (action.Action)
                {
                    case FlowActions.SetPvp:
                        HandleSetPvp(action, player, zoneHash);
                        break;
                    case FlowActions.SendMessage:
                        HandleSendMessage(action, player, zoneHash);
                        break;
                    case FlowActions.SpawnBoss:
                        HandleSpawnBoss(action, player, zoneHash);
                        break;
                    case FlowActions.RemoveBoss:
                        HandleRemoveBoss(action, player, zoneHash);
                        break;
                    case FlowActions.ApplyBorderFx:
                        HandleApplyBorderFx(action, player, zoneHash);
                        break;
                    case FlowActions.RemoveBorderFx:
                        HandleRemoveBorderFx(zoneHash);
                        break;
                    case FlowActions.ApplyZoneBuff:
                        HandleApplyZoneBuff(action, player);
                        break;
                    case FlowActions.RemoveZoneBuff:
                        HandleRemoveZoneBuff(action, player);
                        break;
                    case FlowActions.EnableCoop:
                    case FlowActions.TriggerCoopEvent:
                    case FlowActions.BossCreateEncounterGroup:
                        HandleCreateEncounterGroup(player, zoneHash);
                        break;
                    case FlowActions.DisableCoop:
                    case FlowActions.BossCleanupEncounterGroup:
                        HandleCleanupEncounterGroup(player, zoneHash);
                        break;
                    case FlowActions.ArenaApplyRuleProfile:
                        HandleArenaApplyRuleProfile(player, zoneHash);
                        break;
                    case FlowActions.ArenaRestoreSafeCombatState:
                        SetPvp(player, false, zoneHash);
                        break;
                    case FlowActions.ArenaApplyLoadoutState:
                        HandleArenaApplyLoadoutState(player, zoneHash);
                        break;
                    case FlowActions.ArenaRestoreLoadoutState:
                        HandleArenaRestoreLoadoutState(player, zoneHash);
                        break;
                    case FlowActions.ArenaApplyProgressionGate:
                    case FlowActions.ArenaCaptureSnapshot:
                        HandleArenaSaveProgress(player, zoneHash);
                        break;
                    case FlowActions.ArenaRestoreProgressionState:
                    case FlowActions.ArenaRestoreSnapshot:
                        HandleArenaRestoreProgress(player, zoneHash);
                        break;
                    case FlowActions.ArenaApplyZoneVisuals:
                    case FlowActions.BossApplyEncounterVisuals:
                        HandleZoneVisuals(player, zoneHash, remove: false);
                        break;
                    case FlowActions.ArenaClearZoneVisuals:
                        HandleZoneVisuals(player, zoneHash, remove: true);
                        break;
                    case FlowActions.BossPrepareEncounterState:
                        HandleBossPrepareEncounterState(player, zoneHash);
                        break;
                    case FlowActions.BossSpawnEncounter:
                        HandleBossSpawnEncounter(player, zoneHash);
                        break;
                    case FlowActions.BossUnwindEncounterState:
                        HandleBossUnwindEncounterState(zoneHash);
                        break;
                    case FlowActions.BossRestoreEncounterOverrides:
                        HandleBossRestoreEncounterOverrides(player, zoneHash);
                        break;
                    default:
                        _log.LogWarning($"[FlowRegistry] Unknown action: {action.Action}");
                        break;
                }
            }
            catch (Exception ex)
            {
                _log.LogError($"[FlowRegistry] Error executing action '{action.Action}' in zone={zoneId} hash={zoneHash} player={player.Index}: {ex.Message}");
            }
        }

        private void HandleSetPvp(FlowAction action, Entity player, int zoneHash)
        {
            var enabled = action.Value is bool b && b;
            if (!TryResolvePvpBuffGuid(out var pvpBuffGuid))
            {
                _log.LogWarning($"[FlowRegistry] SetPvp skipped: PvP buff unresolved for player={player.Index} zoneHash={zoneHash} enabled={enabled}");
                return;
            }

            if (!TryResolveUserEntity(player, out var userEntity))
            {
                _log.LogWarning($"[FlowRegistry] SetPvp skipped: user unresolved for player={player.Index} zoneHash={zoneHash} enabled={enabled}");
                return;
            }

            if (enabled)
            {
                var ok = GameActionService.InvokeAction("applybuff", new object[] { userEntity, player, pvpBuffGuid, -1f });
                if (!ok)
                {
                    _log.LogWarning($"[FlowRegistry] SetPvp apply failed for player={player.Index} zoneHash={zoneHash} buff={pvpBuffGuid.GuidHash}");
                    return;
                }

                _log.LogInfo($"[FlowRegistry] SetPvp applied for player={player.Index} zoneHash={zoneHash} buff={pvpBuffGuid.GuidHash}");
            }
            else
            {
                var ok = GameActionService.InvokeAction("removebuff", new object[] { player, pvpBuffGuid });
                if (!ok)
                {
                    _log.LogWarning($"[FlowRegistry] SetPvp remove failed for player={player.Index} zoneHash={zoneHash} buff={pvpBuffGuid.GuidHash}");
                    return;
                }

                _log.LogInfo($"[FlowRegistry] SetPvp removed for player={player.Index} zoneHash={zoneHash} buff={pvpBuffGuid.GuidHash}");
            }
        }

        private void HandleSendMessage(FlowAction action, Entity player, int zoneHash)
        {
            var message = action.Message ?? action.Value?.ToString();
            if (string.IsNullOrWhiteSpace(message) || !TryResolveUserEntity(player, out var userEntity))
            {
                _log.LogWarning($"[FlowRegistry] SendMessage skipped: message/user unresolved for player={player.Index} zoneHash={zoneHash}");
                return;
            }

            GameActionService.InvokeAction("sendmessagetouser", new object[] { userEntity, message });
        }

        private void HandleSpawnBoss(FlowAction action, Entity player, int zoneHash)
        {
            var bossPrefab = action.Prefab?.ToString();
            if (string.IsNullOrWhiteSpace(bossPrefab) || !TryResolvePrefabGuid(bossPrefab, out var bossGuid))
            {
                _log.LogWarning($"[FlowRegistry] SpawnBoss skipped: prefab unresolved '{bossPrefab}' zoneHash={zoneHash}");
                return;
            }

            if (!UnifiedCore.TryGetPrefabEntity(bossGuid, out var prefabEntity) || prefabEntity == Entity.Null)
            {
                _log.LogWarning($"[FlowRegistry] SpawnBoss skipped: prefab entity missing for '{bossPrefab}' guid={bossGuid.GuidHash} zoneHash={zoneHash}");
                return;
            }

            ZoneDefinition? zone = null;
            var zoneFound = Plugin.ZoneConfig?.TryGetZoneByHash(zoneHash, out zone) == true && zone != null;
            var quantity = action.Quantity > 0 ? action.Quantity : 1;
            for (var i = 0; i < quantity; i++)
            {
                var spawnPos = float3.zero;
                if (zoneFound)
                {
                    if (action.RandomInZone)
                    {
                        spawnPos = zone!.GetRandomPositionInside();
                    }
                    else if (action.Position != null && action.Position.Length >= 3)
                    {
                        spawnPos = new float3(action.Position[0], action.Position[1], action.Position[2]);
                    }
                    else
                    {
                        spawnPos = zone!.GetCenterFloat3();
                    }
                }

                var spawned = UnifiedCore.EntityManager.Instantiate(prefabEntity);
                GameActionService.InvokeAction("setposition", new object[] { spawned, spawnPos });
                if (!_bossesByZone.TryGetValue(zoneHash, out var list))
                {
                    list = new List<Entity>();
                    _bossesByZone[zoneHash] = list;
                }

                list.Add(spawned);
            }
        }

        private void HandleRemoveBoss(FlowAction action, Entity player, int zoneHash)
        {
            RemoveBossesInternal(zoneHash);
        }

        private void HandleApplyBorderFx(FlowAction action, Entity player, int zoneHash)
        {
            var vfxPrefab = action.VfxPrefab?.ToString();
            if (string.IsNullOrWhiteSpace(vfxPrefab) || !TryResolvePrefabGuid(vfxPrefab, out var vfxGuid))
            {
                _log.LogWarning($"[FlowRegistry] ApplyBorderFx skipped: VFX unresolved '{vfxPrefab}' zoneHash={zoneHash}");
                return;
            }

            if (!UnifiedCore.TryGetPrefabEntity(vfxGuid, out var prefabEntity) || prefabEntity == Entity.Null)
            {
                _log.LogWarning($"[FlowRegistry] ApplyBorderFx skipped: prefab entity missing for '{vfxPrefab}' guid={vfxGuid.GuidHash} zoneHash={zoneHash}");
                return;
            }

            if (Plugin.ZoneConfig?.TryGetZoneByHash(zoneHash, out var zone) != true || zone == null)
            {
                _log.LogWarning($"[FlowRegistry] ApplyBorderFx skipped: zone {zoneHash} not found.");
                return;
            }

            var radius = action.Radius > 0 ? action.Radius : zone.EntryRadius;
            var segments = action.Segments > 0 ? action.Segments : 24;
            RemoveBorderFxInternal(zoneHash);

            var center = zone.GetCenterFloat3();
            var spawnedList = new List<Entity>(segments);
            for (var i = 0; i < segments; i++)
            {
                var angle = i * math.PI * 2f / segments;
                var pos = new float3(center.x + math.cos(angle) * radius, center.y, center.z + math.sin(angle) * radius);
                var spawned = UnifiedCore.EntityManager.Instantiate(prefabEntity);
                GameActionService.InvokeAction("setposition", new object[] { spawned, pos });
                spawnedList.Add(spawned);
            }

            _borderFxByZone[zoneHash] = spawnedList;
        }

        private void HandleRemoveBorderFx(int zoneHash)
        {
            RemoveBorderFxInternal(zoneHash);
        }

        private void HandleApplyZoneBuff(FlowAction action, Entity player)
        {
            var buffPrefab = action.BuffPrefab ?? action.Value?.ToString();
            if (string.IsNullOrWhiteSpace(buffPrefab) || !TryResolveUserEntity(player, out var userEntity) || !TryResolvePrefabGuid(buffPrefab, out var buffGuid))
            {
                _log.LogWarning($"[FlowRegistry] ApplyZoneBuff skipped: buff/user unresolved for player={player.Index} buff='{buffPrefab ?? "<null>"}'");
                return;
            }

            var duration = action.Duration == 0f ? -1f : action.Duration;
            GameActionService.InvokeAction("applybuff", new object[] { userEntity, player, buffGuid, duration });
        }

        private void HandleRemoveZoneBuff(FlowAction action, Entity player)
        {
            var buffPrefab = action.BuffPrefab ?? action.Value?.ToString();
            if (string.IsNullOrWhiteSpace(buffPrefab) || !TryResolvePrefabGuid(buffPrefab, out var buffGuid))
            {
                _log.LogWarning($"[FlowRegistry] RemoveZoneBuff skipped: buff unresolved '{buffPrefab ?? "<null>"}' player={player.Index}");
                return;
            }

            GameActionService.InvokeAction("removebuff", new object[] { player, buffGuid });
        }

        private void HandleArenaApplyRuleProfile(Entity player, int zoneHash)
        {
            if (Plugin.ZoneConfig?.TryGetZoneByHash(zoneHash, out var zone) != true || zone is not ArenaZoneConfig arena)
            {
                return;
            }

            SetPvp(player, arena.PvpEnabled, zoneHash);
        }

        private void HandleArenaApplyLoadoutState(Entity player, int zoneHash)
        {
            if (Plugin.ZoneConfig?.TryGetZoneByHash(zoneHash, out var zone) != true)
            {
                return;
            }

            if (Plugin.Kits?.IsInitialized == true && !string.IsNullOrWhiteSpace(zone.KitOnEnter))
            {
                Plugin.Kits.ApplyKit(player, zone.KitOnEnter);
            }

            if (Plugin.Abilities?.IsInitialized == true)
            {
                var bossDefault = zone is BossZoneConfig bossCfg && bossCfg.AbilitySets != null && bossCfg.AbilitySets.Length > 0
                    ? bossCfg.AbilitySets[0]
                    : null;
                var setName = !string.IsNullOrWhiteSpace(bossDefault) ? bossDefault : zone.AbilitySet;
                if (!string.IsNullOrWhiteSpace(setName))
                {
                    Plugin.Abilities.ApplySet(player, setName);
                }
            }
        }

        private void HandleArenaRestoreLoadoutState(Entity player, int zoneHash)
        {
            if (Plugin.ZoneConfig?.TryGetZoneByHash(zoneHash, out var zone) == true &&
                Plugin.Kits?.IsInitialized == true &&
                !string.IsNullOrWhiteSpace(zone.KitOnExit))
            {
                Plugin.Kits.ApplyKit(player, zone.KitOnExit);
            }

            Plugin.Abilities?.ClearAbilities(player);
        }

        private void HandleArenaSaveProgress(Entity player, int zoneHash)
        {
            if (Plugin.ZoneConfig?.TryGetZoneByHash(zoneHash, out var zone) == true && zone is ArenaZoneConfig arena && arena.SaveProgress)
            {
                Plugin.Progress?.SaveProgress(player);
            }
        }

        private void HandleArenaRestoreProgress(Entity player, int zoneHash)
        {
            if (Plugin.ZoneConfig?.TryGetZoneByHash(zoneHash, out var zone) == true && zone is ArenaZoneConfig arena && arena.RestoreOnExit)
            {
                Plugin.Progress?.RestoreProgress(player, clearAfter: true);
            }
        }

        private void HandleZoneVisuals(Entity player, int zoneHash, bool remove)
        {
            if (Plugin.ZoneConfig?.TryGetZoneByHash(zoneHash, out var zone) != true || zone?.BorderVisual == null)
            {
                return;
            }

            var buffPrefab = zone.BorderVisual.BuffPrefabs?.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(buffPrefab))
            {
                return;
            }

            if (remove)
            {
                HandleRemoveZoneBuff(new FlowAction { Action = FlowActions.RemoveZoneBuff, BuffPrefab = buffPrefab }, player);
            }
            else
            {
                HandleApplyZoneBuff(new FlowAction { Action = FlowActions.ApplyZoneBuff, BuffPrefab = buffPrefab, Duration = -1f }, player);
            }
        }

        private void HandleCreateEncounterGroup(Entity player, int zoneHash)
        {
            if (Plugin.BossCoop?.IsInitialized != true || Plugin.ZoneTransition == null)
            {
                return;
            }

            foreach (var member in Plugin.ZoneTransition.GetPlayersInZone(zoneHash))
            {
                Plugin.BossCoop.OnBossZoneEnter(member, zoneHash, forceJoinClan: false, shuffleClan: false);
            }
        }

        private void HandleCleanupEncounterGroup(Entity player, int zoneHash)
        {
            Plugin.BossCoop?.OnBossZoneExit(player, zoneHash);
        }

        private void HandleBossPrepareEncounterState(Entity player, int zoneHash)
        {
            if (Plugin.ZoneConfig?.TryGetZoneByHash(zoneHash, out var zone) == true && zone is BossZoneConfig bossZone && !bossZone.NoProgress)
            {
                Plugin.Progress?.SaveProgress(player);
            }
        }

        private void HandleBossSpawnEncounter(Entity player, int zoneHash)
        {
            if (Plugin.ZoneConfig?.TryGetZoneByHash(zoneHash, out var zone) != true || zone is not BossZoneConfig bossZone)
            {
                return;
            }

            EnsureBosses(player, zoneHash, bossZone.BossPrefab, bossZone.BossQuantity, bossZone.RandomSpawn);
        }

        private void HandleBossUnwindEncounterState(int zoneHash)
        {
            if (Plugin.ZoneTransition == null || Plugin.ZoneTransition.GetPlayersInZone(zoneHash).Count > 0)
            {
                return;
            }

            RemoveBosses(zoneHash);
            RemoveBorderFx(zoneHash);
        }

        private void HandleBossRestoreEncounterOverrides(Entity player, int zoneHash)
        {
            if (Plugin.ZoneConfig?.TryGetZoneByHash(zoneHash, out var zone) == true && zone is BossZoneConfig bossZone && !bossZone.NoProgress)
            {
                Plugin.Progress?.RestoreProgress(player, clearAfter: true);
            }

            Plugin.BossCoop?.OnBossZoneExit(player, zoneHash);
        }

        private bool TryResolvePrefabGuid(string prefabName, out PrefabGUID guid)
        {
            guid = default;
            if (string.IsNullOrWhiteSpace(prefabName))
            {
                return false;
            }

            _prefabToGuid ??= Plugin.PrefabToGuid;
            var token = prefabName.Trim();
            if (_prefabToGuid?.IsInitialized == true && _prefabToGuid.TryGetGuid(token, out guid))
            {
                return guid.GuidHash != 0;
            }

            return PrefabGuidConverter.TryGetGuid(token, out guid) && guid.GuidHash != 0;
        }

        private static bool TryResolvePvpBuffGuid(out PrefabGUID guid)
        {
            guid = PrefabGUID.Empty;

            if (Plugin.PrefabToGuid?.IsInitialized == true && Plugin.PrefabToGuid.TryGetGuid("Buff_PvP_Enabled", out guid))
            {
                return guid.GuidHash != 0;
            }

            return PrefabGuidConverter.TryGetGuid("Buff_PvP_Enabled", out guid) && guid.GuidHash != 0;
        }

        private static bool TryResolveUserEntity(Entity player, out Entity userEntity)
        {
            userEntity = Entity.Null;

            try
            {
                var em = UnifiedCore.EntityManager;
                if (em == default || player == Entity.Null || !em.Exists(player) || !em.HasComponent<PlayerCharacter>(player))
                {
                    return false;
                }

                var pc = em.GetComponentData<PlayerCharacter>(player);
                if (pc.UserEntity == Entity.Null || !em.Exists(pc.UserEntity))
                {
                    return false;
                }

                userEntity = pc.UserEntity;
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void RemoveBorderFxInternal(int zoneHash)
        {
            if (!_borderFxByZone.TryGetValue(zoneHash, out var entities) || entities.Count == 0)
            {
                return;
            }

            TryDestroyEntities(entities);
            _borderFxByZone.Remove(zoneHash);
        }

        private void RemoveBossesInternal(int zoneHash)
        {
            if (_bossesByZone.TryGetValue(zoneHash, out var entities) && entities.Count > 0)
            {
                TryDestroyEntities(entities);
                _bossesByZone.Remove(zoneHash);
            }
        }

        private void PruneDeadBossEntities(int zoneHash)
        {
            if (!_bossesByZone.TryGetValue(zoneHash, out var entities) || entities.Count == 0)
            {
                return;
            }

            try
            {
                var em = UnifiedCore.EntityManager;
                if (em == default)
                {
                    return;
                }

                entities.RemoveAll(entity => entity == Entity.Null || !em.Exists(entity));
                if (entities.Count == 0)
                {
                    _bossesByZone.Remove(zoneHash);
                }
            }
            catch
            {
                // ignored
            }
        }

        private void CleanupAllSpawnedEntities()
        {
            foreach (var zoneHash in _borderFxByZone.Keys.ToArray())
            {
                RemoveBorderFxInternal(zoneHash);
            }

            foreach (var zoneHash in _bossesByZone.Keys.ToArray())
            {
                RemoveBossesInternal(zoneHash);
            }
        }

        private static void TryDestroyEntities(IEnumerable<Entity> entities)
        {
            try
            {
                var em = UnifiedCore.EntityManager;
                if (em == default)
                {
                    return;
                }

                foreach (var entity in entities)
                {
                    if (entity != Entity.Null && em.Exists(entity))
                    {
                        em.DestroyEntity(entity);
                    }
                }
            }
            catch
            {
                // ignored
            }
        }
    }
}
