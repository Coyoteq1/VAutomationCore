using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ProjectM;
using ProjectM.Gameplay.Systems;
using ProjectM.Network;
using Stunlock.Core;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using VAutomationCore.Core;

namespace VAuto.Core.Services
{
    public static class DebugEventBridge
    {
        private static readonly string[] ProgressionKeywords = { "Research", "VBlood", "Achievement", "Unlock", "Tech", "Recipe", "Progress" };
        private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true, WriteIndented = true };
        private static readonly ISnapshotCaptureService SnapshotCaptureService = new SnapshotCaptureService();
        private static readonly ISnapshotDiffService SnapshotDiffService = new SnapshotDiffService();
        private static readonly ISnapshotPersistenceService SnapshotPersistenceService = new SnapshotPersistenceService();
        private static readonly IProgressionRestoreService ProgressionRestoreService = new ProgressionRestoreService();

        private static bool _enabled = true;
        private static bool _persistSnapshots = true;
        private static string _snapshotPath = string.Empty;
        private static bool _verboseLogs;
        private const string BaselineCsvFileName = "sandbox_progression_baseline.csv.gz";
        private const string DeltaCsvFileName = "sandbox_progression_delta.csv.gz";
        private const string ProgressionJournalJsonlFileName = "sandbox_progression_journal.jsonl";
        private const string LegacyJsonFileName = "sandbox_progression_snapshots.json";
        private const string BlueLockAssemblyName = "BlueLock";

        private static readonly HashSet<ulong> _unlockAppliedThisSession = new();
        private static readonly object _snapshotFileLock = new();
        private static readonly object _stateLock = new();
        private static bool _snapshotsLoaded;

        private static readonly MethodInfo? GetComponentDataGeneric = typeof(EntityManager).GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(m => m.Name == "GetComponentData" && m.IsGenericMethodDefinition && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(Entity));
        private static readonly MethodInfo? SetComponentDataGeneric = typeof(EntityManager).GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(m => m.Name == "SetComponentData" && m.IsGenericMethodDefinition && m.GetParameters().Length == 2 && m.GetParameters()[0].ParameterType == typeof(Entity));
        private static readonly MethodInfo? HasComponentGeneric = typeof(EntityManager).GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(m => m.Name == "HasComponent" && m.IsGenericMethodDefinition && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(Entity));
        private static readonly MethodInfo? AddComponentGeneric = typeof(EntityManager).GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(m => m.Name == "AddComponent" && m.IsGenericMethodDefinition && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(Entity));
        private static readonly MethodInfo? RemoveComponentGeneric = typeof(EntityManager).GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(m => m.Name == "RemoveComponent" && m.IsGenericMethodDefinition && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(Entity));
        private static readonly MethodInfo[] GetComponentTypesMethods = typeof(EntityManager).GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.Name == "GetComponentTypes" && !m.IsGenericMethod)
            .ToArray();

        public static void ConfigureSandboxProgression(bool enabled, bool persistSnapshots, string snapshotPath, bool verboseLogs)
        {
            _enabled = enabled;
            _persistSnapshots = persistSnapshots;
            _snapshotPath = snapshotPath ?? string.Empty;
            _verboseLogs = verboseLogs;
            _snapshotsLoaded = false;

            EnsureSnapshotsLoaded();
            LogInfo($"Configured sandbox progression: enabled={_enabled}, persist={_persistSnapshots}, path='{_snapshotPath}'.");
        }

        public static void OnZoneEnterStart(Entity character, string zoneId) => OnZoneEnterStart(character, zoneId, true);
        public static void OnPlayerEnter(Entity character, string zoneId) => OnPlayerEnter(character, zoneId, true);
        public static void OnPlayerExit(Entity character, string zoneId) => OnPlayerExit(character, zoneId, true);
        public static void OnPlayerIsInZone(Entity character, string zoneId) => OnPlayerIsInZone(character, zoneId, true);
        public static void OnPlayerEnterZone(Entity character) => OnPlayerEnterZone(character, true);
        public static void OnPlayerIsInZone(Entity character) => OnPlayerIsInZone(character, true);
        public static void OnPlayerExitZone(Entity character) => OnPlayerExitZone(character, true);
        public static void FlushSnapshotsToDisk() => PersistSnapshotsToDisk();

        public static void OnPlayerEnter(Entity character, string zoneId, bool enableUnlock)
        {
            OnZoneEnterStart(character, zoneId, enableUnlock);
            OnPlayerEnterZone(character, enableUnlock);
        }

        public static void OnPlayerExit(Entity character, string zoneId, bool enableUnlock)
        {
            OnPlayerExitZone(character, enableUnlock);
        }

        public static void OnPlayerIsInZone(Entity character, string zoneId, bool enableUnlock)
        {
            OnPlayerIsInZone(character, enableUnlock);
        }

        public static void OnZoneEnterStart(Entity character, string zoneId, bool enableUnlock)
        {
            if (!_enabled || !enableUnlock)
            {
                return;
            }

            EnsureSnapshotsLoaded();

            if (!TryResolvePlayerIdentity(character, out var platformId, out var characterName, out _))
            {
                return;
            }

            var playerKey = SandboxSnapshotStore.GetPreferredPlayerKey(characterName, platformId);
            var capturedUtc = DateTime.UtcNow;
            var snapshotId = SnapshotCaptureService.BuildSnapshotId(platformId, characterName, capturedUtc);
            var preSnapshot = SnapshotCaptureService.CaptureProgressionSnapshot(character);
            var baselineRows = SnapshotCaptureService.BuildBaselineRows(preSnapshot, playerKey, characterName, platformId, zoneId, snapshotId, capturedUtc);
            var preZoneEntities = SnapshotCaptureService.CaptureZoneEntityMap(zoneId);

            var pending = new SandboxPendingContext
            {
                PlayerKey = playerKey,
                CharacterName = characterName,
                PlatformId = platformId,
                ZoneId = zoneId ?? string.Empty,
                SnapshotId = snapshotId,
                CapturedUtc = capturedUtc,
                ComponentRows = baselineRows,
                PreEnterZoneEntities = preZoneEntities
            };

            SandboxSnapshotStore.UpsertPendingContext(pending);
            LogDebug($"Captured sandbox baseline for key='{playerKey}', zone='{zoneId}', components={baselineRows.Length}, entities={preZoneEntities.Length}.");
        }

        public static void OnPlayerEnterZone(Entity character, bool enableUnlock)
        {
            if (!_enabled || !enableUnlock)
            {
                return;
            }

            EnsureSnapshotsLoaded();

            if (!TryResolvePlayerIdentity(character, out var platformId, out var characterName, out _))
            {
                return;
            }

            lock (_stateLock)
            {
                if (_unlockAppliedThisSession.Contains(platformId))
                {
                    return;
                }
            }

            if (!SandboxSnapshotStore.TryTakePendingContext(characterName, platformId, out var playerKey, out var pending) || pending == null)
            {
                var fallbackCapturedUtc = DateTime.UtcNow;
                var fallbackSnapshotId = SnapshotCaptureService.BuildSnapshotId(platformId, characterName, fallbackCapturedUtc);
                playerKey = SandboxSnapshotStore.GetPreferredPlayerKey(characterName, platformId);
                var fallbackBaselineSnapshot = SnapshotCaptureService.CaptureProgressionSnapshot(character);
                pending = new SandboxPendingContext
                {
                    PlayerKey = playerKey,
                    CharacterName = characterName,
                    PlatformId = platformId,
                    ZoneId = string.Empty,
                    SnapshotId = fallbackSnapshotId,
                    CapturedUtc = fallbackCapturedUtc,
                    ComponentRows = SnapshotCaptureService.BuildBaselineRows(fallbackBaselineSnapshot, playerKey, characterName, platformId, string.Empty, fallbackSnapshotId, fallbackCapturedUtc),
                    PreEnterZoneEntities = Array.Empty<ZoneEntityEntry>()
                };
            }

            ApplyFullUnlock(character);

            var finalizedUtc = DateTime.UtcNow;
            var postSnapshot = SnapshotCaptureService.CaptureProgressionSnapshot(character);
            var postRows = SnapshotCaptureService.BuildBaselineRows(postSnapshot, playerKey, characterName, platformId, pending.ZoneId, pending.SnapshotId, finalizedUtc);

            var deltaRows = new List<DeltaRow>();
            deltaRows.AddRange(SnapshotDiffService.ComputeComponentDelta(pending.ComponentRows, postRows));
            deltaRows.AddRange(SnapshotDiffService.ExtractOpenedTech(pending.ComponentRows, postRows));
            deltaRows.AddRange(SnapshotDiffService.ComputeEntityDelta(pending.PreEnterZoneEntities, SnapshotCaptureService.CaptureZoneEntityMap(pending.ZoneId)));
            StampDeltaRows(deltaRows, playerKey, characterName, platformId, pending.ZoneId, pending.SnapshotId, finalizedUtc);

            var baselineSnapshot = new SandboxBaselineSnapshot
            {
                PlayerKey = playerKey,
                CharacterName = characterName,
                PlatformId = platformId,
                ZoneId = pending.ZoneId,
                SnapshotId = pending.SnapshotId,
                CapturedUtc = pending.CapturedUtc,
                Rows = pending.ComponentRows
            };

            var deltaSnapshot = new SandboxDeltaSnapshot
            {
                PlayerKey = playerKey,
                CharacterName = characterName,
                PlatformId = platformId,
                ZoneId = pending.ZoneId,
                SnapshotId = pending.SnapshotId,
                CapturedUtc = finalizedUtc,
                Rows = deltaRows.ToArray()
            };

            SandboxSnapshotStore.PutActiveSnapshots(playerKey, baselineSnapshot, deltaSnapshot);
            SandboxSnapshotStore.MarkDirty();

            // Scope 1 journal: append deterministic progression mutations in JSONL format.
            // Keep CSV snapshots unchanged for fallback compatibility.
            TryAppendProgressionJournal(playerKey, pending, postRows, finalizedUtc);

            lock (_stateLock)
            {
                _unlockAppliedThisSession.Add(platformId);
            }

            LogDebug($"Finalized sandbox baseline+delta for key='{playerKey}', zone='{pending.ZoneId}', deltaRows={deltaRows.Count}.");
        }

        public static void OnPlayerIsInZone(Entity character, bool enableUnlock)
        {
            // Unlock intentionally runs on enter only.
        }

        public static void OnPlayerExitZone(Entity character, bool enableUnlock)
        {
            if (!TryResolvePlayerIdentity(character, out var platformId, out var characterName, out _))
            {
                return;
            }

            if (!enableUnlock)
            {
                lock (_stateLock)
                {
                    _unlockAppliedThisSession.Remove(platformId);
                }
                return;
            }

            if (!SandboxSnapshotStore.TryGetActiveSnapshots(characterName, platformId, out var playerKey, out var baseline, out var delta) || baseline == null)
            {
                lock (_stateLock)
                {
                    _unlockAppliedThisSession.Remove(platformId);
                }
                LogDebug($"No active sandbox baseline found for character='{characterName}', platformId={platformId}.");
                return;
            }

            ProgressionRestoreService.TryApplyDeltaEntityCleanup(delta, baseline.ZoneId);
            var restoreSnapshot = BuildSnapshotFromBaselineRows(baseline.Rows, baseline.PlatformId, baseline.CapturedUtc);

            if (!ProgressionRestoreService.RestoreProgressionSnapshot(character, restoreSnapshot))
            {
                LogWarning($"Restore failed for playerKey='{playerKey}', platformId={platformId}.");
            }

            ProgressionRestoreService.ValidateDeltaAfterRestore(delta, baseline.ZoneId);
            SandboxSnapshotStore.RemoveActiveSnapshots(playerKey);
            SandboxSnapshotStore.MarkDirty();

            lock (_stateLock)
            {
                _unlockAppliedThisSession.Remove(platformId);
            }

            FlushSnapshotsToDisk();
        }

        private static bool TryGetPlatformId(Entity character, out ulong platformId)
        {
            return TryResolvePlayerIdentity(character, out platformId, out _, out _);
        }

        private static bool TryGetUserEntity(Entity character, out Entity userEntity)
        {
            return TryResolvePlayerIdentity(character, out _, out _, out userEntity);
        }

        private static bool TryResolvePlayerIdentity(Entity character, out ulong platformId, out string characterName, out Entity userEntity)
        {
            platformId = 0;
            characterName = string.Empty;
            userEntity = Entity.Null;

            try
            {
                var em = UnifiedCore.EntityManager;
                if (em == default || !em.Exists(character) || !em.HasComponent<PlayerCharacter>(character))
                {
                    return false;
                }

                var playerCharacter = em.GetComponentData<PlayerCharacter>(character);
                if (!em.Exists(playerCharacter.UserEntity) || !em.HasComponent<User>(playerCharacter.UserEntity))
                {
                    return false;
                }

                var user = em.GetComponentData<User>(playerCharacter.UserEntity);
                platformId = user.PlatformId;
                if (platformId == 0)
                {
                    LogWarning("Sandbox progression skipped: platformId == 0.");
                    return false;
                }

                userEntity = playerCharacter.UserEntity;
                characterName = NormalizeCharacterName(user.CharacterName.ToString(), platformId);
                return true;
            }
            catch (Exception ex)
            {
                LogWarning($"TryResolvePlayerIdentity failed: {ex.Message}");
                return false;
            }
        }

        private static string NormalizeCharacterName(string? rawName, ulong platformId)
        {
            var normalized = (rawName ?? string.Empty).Trim();
            if (normalized.Length == 0)
            {
                return platformId.ToString(CultureInfo.InvariantCulture);
            }

            return normalized;
        }

        private static void ApplyFullUnlock(Entity character)
        {
            try
            {
                if (!TryGetUserEntity(character, out var userEntity))
                {
                    return;
                }

                var debugSystem = UnifiedCore.Server.GetExistingSystemManaged<DebugEventsSystem>();
                if (debugSystem == null)
                {
                    LogWarning("ApplyFullUnlock skipped: DebugEventsSystem unavailable.");
                    return;
                }

                var fromCharacter = new FromCharacter { User = userEntity, Character = character };
                var type = debugSystem.GetType();

                var researchOk = TryInvokeUnlock(type, debugSystem, new[] { "UnlockAllResearch", "TriggerUnlockAllResearch" }, fromCharacter, userEntity);
                var vbloodOk = TryInvokeUnlock(type, debugSystem, new[] { "UnlockAllVBloods", "TriggerUnlockAllVBlood" }, fromCharacter, userEntity);
                var achievementOk = TryInvokeUnlock(type, debugSystem, new[] { "CompleteAllAchievements", "TriggerCompleteAllAchievements" }, fromCharacter, userEntity);

                if (!researchOk || !vbloodOk || !achievementOk)
                {
                    LogWarning($"ApplyFullUnlock partial failure: research={researchOk}, vblood={vbloodOk}, achievements={achievementOk}.");
                }
                else
                {
                    LogDebug("ApplyFullUnlock succeeded.");
                }
            }
            catch (Exception ex)
            {
                LogWarning($"ApplyFullUnlock failed: {ex.Message}");
            }
        }

        private static bool TryInvokeUnlock(Type systemType, object instance, string[] methodNames, FromCharacter fromCharacter, Entity userEntity)
        {
            foreach (var methodName in methodNames)
            {
                foreach (var method in systemType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(m => m.Name == methodName))
                {
                    var parameters = method.GetParameters();
                    if (parameters.Length != 1)
                    {
                        continue;
                    }

                    try
                    {
                        if (parameters[0].ParameterType == typeof(FromCharacter))
                        {
                            method.Invoke(instance, new object[] { fromCharacter });
                            return true;
                        }

                        if (parameters[0].ParameterType == typeof(Entity))
                        {
                            method.Invoke(instance, new object[] { userEntity });
                            return true;
                        }
                    }
                    catch
                    {
                        // Try next overload.
                    }
                }
            }

            return false;
        }

        internal static SandboxProgressionSnapshot CaptureProgressionSnapshotCore(Entity character)
        {
            var snapshot = new SandboxProgressionSnapshot
            {
                CapturedUtc = DateTime.UtcNow
            };

            if (!TryGetPlatformId(character, out var platformId))
            {
                return snapshot;
            }

            snapshot.PlatformId = platformId;

            if (!TryGetUserEntity(character, out var userEntity))
            {
                return snapshot;
            }

            var em = UnifiedCore.EntityManager;
            if (em == default || !em.Exists(userEntity))
            {
                return snapshot;
            }

            if (!TryGetComponentTypeEnumerable(em, userEntity, out var componentTypes, out var disposable))
            {
                return snapshot;
            }

            try
            {
                foreach (var componentTypeObj in componentTypes)
                {
                    if (!TryResolveManagedType(componentTypeObj, out var managedType) || managedType == null)
                    {
                        continue;
                    }

                    if (!IsProgressionComponentType(managedType))
                    {
                        continue;
                    }

                    if (!TryHasComponent(em, userEntity, managedType))
                    {
                        continue;
                    }

                    if (!TryGetComponentData(em, userEntity, managedType, out var componentValue, out _))
                    {
                        continue;
                    }

                    var typeKey = managedType.AssemblyQualifiedName ?? managedType.FullName ?? managedType.Name;
                    if (string.IsNullOrWhiteSpace(typeKey))
                    {
                        continue;
                    }

                    string payload;
                    try
                    {
                        payload = JsonSerializer.Serialize(componentValue, managedType, JsonOpts);
                    }
                    catch
                    {
                        continue;
                    }

                    snapshot.Components[typeKey] = new SnapshotComponentState
                    {
                        Existed = true,
                        AssemblyQualifiedType = typeKey,
                        JsonPayload = payload
                    };
                }
            }
            finally
            {
                disposable?.Dispose();
            }

            return snapshot;
        }

        internal static bool RestoreProgressionSnapshotCore(Entity character, SandboxProgressionSnapshot snapshot)
        {
            if (!TryGetUserEntity(character, out var userEntity))
            {
                return false;
            }

            var em = UnifiedCore.EntityManager;
            if (em == default || !em.Exists(userEntity))
            {
                return false;
            }

            var ok = true;
            var snapshotTypes = new HashSet<string>(snapshot.Components.Keys, StringComparer.Ordinal);

            if (TryGetComponentTypeEnumerable(em, userEntity, out var currentTypes, out var currentTypesDisposable))
            {
                try
                {
                    foreach (var componentTypeObj in currentTypes)
                    {
                        if (!TryResolveManagedType(componentTypeObj, out var managedType) || managedType == null)
                        {
                            continue;
                        }

                        if (!IsProgressionComponentType(managedType))
                        {
                            continue;
                        }

                        var typeKey = managedType.AssemblyQualifiedName ?? managedType.FullName ?? managedType.Name;
                        if (snapshotTypes.Contains(typeKey))
                        {
                            continue;
                        }

                        if (TryHasComponent(em, userEntity, managedType) && !TryRemoveComponent(em, userEntity, managedType, out _))
                        {
                            ok = false;
                        }
                    }
                }
                finally
                {
                    currentTypesDisposable?.Dispose();
                }
            }

            foreach (var kv in snapshot.Components)
            {
                var state = kv.Value;
                if (state == null)
                {
                    continue;
                }

                var resolvedType = ResolveTypeByName(state.AssemblyQualifiedType);
                if (resolvedType == null || !IsProgressionComponentType(resolvedType))
                {
                    continue;
                }

                try
                {
                    if (!state.Existed)
                    {
                        if (TryHasComponent(em, userEntity, resolvedType))
                        {
                            ok &= TryRemoveComponent(em, userEntity, resolvedType, out _);
                        }
                        continue;
                    }

                    if (!TryHasComponent(em, userEntity, resolvedType) && !TryAddComponent(em, userEntity, resolvedType, out _))
                    {
                        ok = false;
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(state.JsonPayload))
                    {
                        ok = false;
                        continue;
                    }

                    var deserialized = JsonSerializer.Deserialize(state.JsonPayload, resolvedType, JsonOpts);
                    if (deserialized == null || !TrySetComponentData(em, userEntity, resolvedType, deserialized, out _))
                    {
                        ok = false;
                    }
                }
                catch
                {
                    ok = false;
                }
            }

            return ok;
        }

        internal static string BuildSnapshotIdCore(ulong platformId, string characterName, DateTime capturedUtc)
        {
            var safeName = (characterName ?? string.Empty).Trim();
            if (safeName.Length == 0)
            {
                safeName = platformId.ToString(CultureInfo.InvariantCulture);
            }

            safeName = safeName.Replace("|", "_", StringComparison.Ordinal);
            return $"{capturedUtc:yyyyMMddHHmmssfff}_{safeName}_{platformId.ToString(CultureInfo.InvariantCulture)}";
        }

        internal static BaselineRow[] BuildBaselineRowsCore(
            SandboxProgressionSnapshot snapshot,
            string playerKey,
            string characterName,
            ulong platformId,
            string zoneId,
            string snapshotId,
            DateTime capturedUtc)
        {
            var rows = new List<BaselineRow>(snapshot.Components.Count);
            foreach (var pair in snapshot.Components)
            {
                var state = pair.Value;
                if (state == null)
                {
                    continue;
                }

                var assemblyQualifiedType = !string.IsNullOrWhiteSpace(state.AssemblyQualifiedType)
                    ? state.AssemblyQualifiedType
                    : pair.Key;
                if (string.IsNullOrWhiteSpace(assemblyQualifiedType))
                {
                    continue;
                }

                var payload = state.JsonPayload ?? string.Empty;
                rows.Add(new BaselineRow
                {
                    Version = 1,
                    SnapshotId = snapshotId,
                    PlayerKey = playerKey,
                    CharacterName = characterName,
                    PlatformId = platformId,
                    ZoneId = zoneId ?? string.Empty,
                    CapturedUtc = capturedUtc,
                    RowType = "component",
                    ComponentType = ResolveComponentTypeName(assemblyQualifiedType),
                    AssemblyQualifiedType = assemblyQualifiedType,
                    Existed = state.Existed,
                    PayloadBase64 = EncodePayload(payload),
                    PayloadHash = ComputePayloadHash(payload)
                });
            }

            return rows.ToArray();
        }

        private static SandboxProgressionSnapshot BuildSnapshotFromBaselineRows(IEnumerable<BaselineRow> rows, ulong platformId, DateTime capturedUtc)
        {
            var snapshot = new SandboxProgressionSnapshot
            {
                PlatformId = platformId,
                CapturedUtc = capturedUtc
            };

            foreach (var row in rows ?? Array.Empty<BaselineRow>())
            {
                if (row == null)
                {
                    continue;
                }

                var typeName = !string.IsNullOrWhiteSpace(row.AssemblyQualifiedType)
                    ? row.AssemblyQualifiedType
                    : row.ComponentType;
                if (string.IsNullOrWhiteSpace(typeName))
                {
                    continue;
                }

                snapshot.Components[typeName] = new SnapshotComponentState
                {
                    Existed = row.Existed,
                    AssemblyQualifiedType = typeName,
                    JsonPayload = DecodePayload(row.PayloadBase64)
                };
            }

            return snapshot;
        }

        private static string ResolveComponentTypeName(string assemblyQualifiedType)
        {
            try
            {
                var resolved = ResolveTypeByName(assemblyQualifiedType);
                if (resolved != null)
                {
                    return resolved.Name;
                }
            }
            catch
            {
                // Fall through to lightweight parser.
            }

            var shortName = assemblyQualifiedType.Split(',')[0];
            var lastDot = shortName.LastIndexOf('.');
            return lastDot >= 0 && lastDot + 1 < shortName.Length
                ? shortName.Substring(lastDot + 1)
                : shortName;
        }

        private static string EncodePayload(string payload)
        {
            var bytes = Encoding.UTF8.GetBytes(payload ?? string.Empty);
            return Convert.ToBase64String(bytes);
        }

        private static string DecodePayload(string payloadBase64)
        {
            if (string.IsNullOrWhiteSpace(payloadBase64))
            {
                return string.Empty;
            }

            try
            {
                return Encoding.UTF8.GetString(Convert.FromBase64String(payloadBase64));
            }
            catch
            {
                return string.Empty;
            }
        }

        private static string ComputePayloadHash(string payload)
        {
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(payload ?? string.Empty));
            return Convert.ToHexString(hash);
        }

        private static void StampDeltaRows(
            IEnumerable<DeltaRow> rows,
            string playerKey,
            string characterName,
            ulong platformId,
            string zoneId,
            string snapshotId,
            DateTime capturedUtc)
        {
            foreach (var row in rows ?? Array.Empty<DeltaRow>())
            {
                if (row == null)
                {
                    continue;
                }

                row.Version = 1;
                row.SnapshotId = snapshotId;
                row.PlayerKey = playerKey;
                row.CharacterName = characterName;
                row.PlatformId = platformId;
                row.ZoneId = zoneId ?? string.Empty;
                row.CapturedUtc = capturedUtc;
            }
        }

        internal static ZoneEntityEntry[] CaptureZoneEntityMapCore(string zoneId)
        {
            if (string.IsNullOrWhiteSpace(zoneId))
            {
                return Array.Empty<ZoneEntityEntry>();
            }

            var contains = ResolveZoneContainsPredicate(zoneId);
            if (contains == null)
            {
                return Array.Empty<ZoneEntityEntry>();
            }

            try
            {
                var em = UnifiedCore.EntityManager;
                if (em == default)
                {
                    return Array.Empty<ZoneEntityEntry>();
                }

                var query = em.CreateEntityQuery(ComponentType.ReadOnly<PrefabGUID>());
                var entities = query.ToEntityArray(Allocator.Temp);
                try
                {
                    var results = new List<ZoneEntityEntry>(Math.Min(entities.Length, 2048));

                    for (var i = 0; i < entities.Length; i++)
                    {
                        var entity = entities[i];
                        if (!TryGetBestPosition(em, entity, out var pos))
                        {
                            continue;
                        }

                        if (!contains(pos.x, pos.z))
                        {
                            continue;
                        }

                        var prefab = em.GetComponentData<PrefabGUID>(entity);
                        results.Add(new ZoneEntityEntry
                        {
                            EntityIndex = entity.Index,
                            EntityVersion = entity.Version,
                            PrefabGuidHash = prefab.GuidHash,
                            PrefabName = ResolvePrefabName(prefab),
                            PosX = pos.x,
                            PosY = pos.y,
                            PosZ = pos.z
                        });
                    }

                    return results.ToArray();
                }
                finally
                {
                    if (entities.IsCreated)
                    {
                        entities.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                LogWarning($"CaptureZoneEntityMap failed for zone '{zoneId}': {ex.Message}");
                return Array.Empty<ZoneEntityEntry>();
            }
        }

        private static bool TryGetBestPosition(EntityManager em, Entity entity, out float3 position)
        {
            position = default;

            try
            {
                if (em.HasComponent<LocalTransform>(entity))
                {
                    position = em.GetComponentData<LocalTransform>(entity).Position;
                    return true;
                }

                if (em.HasComponent<Translation>(entity))
                {
                    position = em.GetComponentData<Translation>(entity).Value;
                    return true;
                }

                if (em.HasComponent<LastTranslation>(entity))
                {
                    position = em.GetComponentData<LastTranslation>(entity).Value;
                    return true;
                }

                if (em.HasComponent<SpawnTransform>(entity))
                {
                    position = em.GetComponentData<SpawnTransform>(entity).Position;
                    return true;
                }
            }
            catch
            {
                // ignored
            }

            return false;
        }

        private static Func<float, float, bool>? ResolveZoneContainsPredicate(string zoneId)
        {
            if (string.IsNullOrWhiteSpace(zoneId))
            {
                return null;
            }

            try
            {
                var zoneConfigType = Type.GetType($"VAuto.Zone.Services.ZoneConfigService, {BlueLockAssemblyName}", throwOnError: false)
                                     ?? Type.GetType("VAuto.Zone.Services.ZoneConfigService", throwOnError: false);
                if (zoneConfigType == null)
                {
                    return null;
                }

                var getZoneById = zoneConfigType.GetMethod(
                    "GetZoneById",
                    BindingFlags.Public | BindingFlags.Static,
                    null,
                    new[] { typeof(string) },
                    null);
                if (getZoneById == null)
                {
                    return null;
                }

                var zone = getZoneById.Invoke(null, new object[] { zoneId });
                if (zone == null)
                {
                    return null;
                }

                var isInside = zone.GetType().GetMethod(
                    "IsInside",
                    BindingFlags.Public | BindingFlags.Instance,
                    null,
                    new[] { typeof(float), typeof(float) },
                    null);
                if (isInside == null)
                {
                    return null;
                }

                return (x, z) =>
                {
                    try
                    {
                        return isInside.Invoke(zone, new object[] { x, z }) is bool inside && inside;
                    }
                    catch
                    {
                        return false;
                    }
                };
            }
            catch
            {
                return null;
            }
        }

        private static string ResolvePrefabName(PrefabGUID prefabGuid)
        {
            if (prefabGuid.GuidHash == 0)
            {
                return string.Empty;
            }

            return $"Prefab_{prefabGuid.GuidHash.ToString(CultureInfo.InvariantCulture)}";
        }

        internal static void TryApplyDeltaEntityCleanupCore(SandboxDeltaSnapshot? deltaSnapshot, string zoneId)
        {
            if (deltaSnapshot?.Rows == null || deltaSnapshot.Rows.Length == 0)
            {
                return;
            }

            var contains = ResolveZoneContainsPredicate(zoneId);

            var em = UnifiedCore.EntityManager;
            if (em == default)
            {
                return;
            }

            var removedStrict = 0;
            var removedForce = 0;

            // Pass 1 (strict): original behavior with zone/prefab guards.
            foreach (var row in deltaSnapshot.Rows)
            {
                if (row == null || !string.Equals(row.RowType, "entity_created", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var entity = new Entity { Index = row.EntityIndex, Version = row.EntityVersion };
                if (!em.Exists(entity))
                {
                    continue;
                }

                if (contains == null || !TryGetBestPosition(em, entity, out var position) || !contains(position.x, position.z))
                {
                    continue;
                }

                if (row.PrefabGuid != 0 && em.HasComponent<PrefabGUID>(entity))
                {
                    var currentPrefab = em.GetComponentData<PrefabGUID>(entity).GuidHash;
                    if (currentPrefab != (int)row.PrefabGuid)
                    {
                        continue;
                    }
                }

                try
                {
                    em.DestroyEntity(entity);
                    removedStrict++;
                }
                catch (Exception ex)
                {
                    LogWarning($"Delta cleanup failed for entity {entity.Index}:{entity.Version}: {ex.Message}");
                }
            }

            // Pass 2 (force fallback): if entity still exists, destroy by captured entity id
            // even when zone/prefab checks are inconclusive (common after transforms/versions drift).
            foreach (var row in deltaSnapshot.Rows)
            {
                if (row == null || !string.Equals(row.RowType, "entity_created", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var entity = new Entity { Index = row.EntityIndex, Version = row.EntityVersion };
                if (!em.Exists(entity))
                {
                    continue;
                }

                if (row.PrefabGuid != 0 && em.HasComponent<PrefabGUID>(entity))
                {
                    var currentPrefab = em.GetComponentData<PrefabGUID>(entity).GuidHash;
                    if (currentPrefab != (int)row.PrefabGuid)
                    {
                        continue;
                    }
                }

                try
                {
                    em.DestroyEntity(entity);
                    removedForce++;
                }
                catch (Exception ex)
                {
                    LogWarning($"Delta force-cleanup failed for entity {entity.Index}:{entity.Version}: {ex.Message}");
                }
            }

            var removed = removedStrict + removedForce;
            if (removed > 0)
            {
                LogDebug($"Delta cleanup removed {removed} created entities for zone '{zoneId}' (strict={removedStrict}, force={removedForce}).");
            }
        }

        internal static void ValidateDeltaAfterRestoreCore(SandboxDeltaSnapshot? deltaSnapshot, string zoneId)
        {
            if (deltaSnapshot?.Rows == null || deltaSnapshot.Rows.Length == 0)
            {
                return;
            }

            var em = UnifiedCore.EntityManager;
            if (em == default)
            {
                return;
            }

            var unresolvedCreates = 0;
            foreach (var row in deltaSnapshot.Rows)
            {
                if (row == null || !string.Equals(row.RowType, "entity_created", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var entity = new Entity { Index = row.EntityIndex, Version = row.EntityVersion };
                if (em.Exists(entity))
                {
                    unresolvedCreates++;
                }
            }

            if (unresolvedCreates > 0)
            {
                LogWarning($"Delta validation found {unresolvedCreates} unresolved created entities for zone '{zoneId}'.");
            }
        }

        private static bool TryGetComponentTypeEnumerable(EntityManager em, Entity userEntity, out IEnumerable componentTypes, out IDisposable? disposable)
        {
            componentTypes = Array.Empty<object>();
            disposable = null;

            foreach (var method in GetComponentTypesMethods)
            {
                var parameters = method.GetParameters();
                if (parameters.Length != 2 || parameters[0].ParameterType != typeof(Entity))
                {
                    continue;
                }

                if (!TryGetAllocatorArgument(parameters[1].ParameterType, out var allocatorArgument))
                {
                    continue;
                }

                try
                {
                    var result = method.Invoke(em, new[] { (object)userEntity, allocatorArgument });
                    if (TryConvertToEnumerable(result, out componentTypes))
                    {
                        disposable = result as IDisposable;
                        return true;
                    }
                }
                catch
                {
                    // Try next overload.
                }
            }

            return false;
        }

        private static bool TryGetAllocatorArgument(Type allocatorParamType, out object allocatorArgument)
        {
            allocatorArgument = default!;

            if (allocatorParamType == typeof(Allocator))
            {
                allocatorArgument = Allocator.Temp;
                return true;
            }

            if (allocatorParamType.IsEnum)
            {
                allocatorArgument = Enum.ToObject(allocatorParamType, (int)Allocator.Temp);
                return true;
            }

            return false;
        }

        private static bool TryConvertToEnumerable(object? value, out IEnumerable enumerable)
        {
            if (value is IEnumerable directEnumerable)
            {
                enumerable = directEnumerable;
                return true;
            }

            enumerable = Array.Empty<object>();
            return false;
        }

        private static bool TryResolveManagedType(object componentTypeObj, out Type? managedType)
        {
            managedType = null;
            if (componentTypeObj == null)
            {
                return false;
            }

            var componentType = componentTypeObj.GetType();
            var getManagedType = componentType.GetMethod("GetManagedType", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);
            if (getManagedType != null)
            {
                try
                {
                    var raw = getManagedType.Invoke(componentTypeObj, null);
                    if (TryCoerceToManagedType(raw, out managedType))
                    {
                        return true;
                    }
                }
                catch
                {
                    // Continue to fallback probes.
                }
            }

            foreach (var propertyName in new[] { "ManagedType", "Type" })
            {
                var property = componentType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (property == null)
                {
                    continue;
                }

                try
                {
                    var raw = property.GetValue(componentTypeObj);
                    if (TryCoerceToManagedType(raw, out managedType))
                    {
                        return true;
                    }
                }
                catch
                {
                    // Try next property.
                }
            }

            return false;
        }

        private static bool TryCoerceToManagedType(object? rawType, out Type? managedType)
        {
            managedType = null;
            if (rawType == null)
            {
                return false;
            }

            if (rawType is Type directType)
            {
                managedType = directType;
                return true;
            }

            var raw = rawType.GetType();
            var aqn = raw.GetProperty("AssemblyQualifiedName", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(rawType) as string;
            managedType = ResolveTypeByName(aqn);
            if (managedType != null)
            {
                return true;
            }

            var fullName = raw.GetProperty("FullName", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(rawType) as string;
            managedType = ResolveTypeByName(fullName);
            return managedType != null;
        }

        private static bool IsProgressionComponentType(Type managedType)
        {
            var name = managedType.FullName ?? managedType.Name;
            return ProgressionKeywords.Any(keyword => name.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private static Type? ResolveTypeByName(string? typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
            {
                return null;
            }

            var resolved = Type.GetType(typeName, false);
            if (resolved != null)
            {
                return resolved;
            }

            var shortName = typeName.Split(',')[0].Trim();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                resolved = assembly.GetType(shortName, false);
                if (resolved != null)
                {
                    return resolved;
                }
            }

            return null;
        }

        private static bool TryGetComponentData(EntityManager em, Entity entity, Type componentType, out object? value, out string error)
        {
            value = null;
            error = string.Empty;

            if (GetComponentDataGeneric == null)
            {
                error = "GetComponentData<T> method unavailable";
                return false;
            }

            try
            {
                value = GetComponentDataGeneric.MakeGenericMethod(componentType).Invoke(em, new object[] { entity });
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        private static bool TrySetComponentData(EntityManager em, Entity entity, Type componentType, object value, out string error)
        {
            error = string.Empty;

            if (SetComponentDataGeneric == null)
            {
                error = "SetComponentData<T> method unavailable";
                return false;
            }

            try
            {
                SetComponentDataGeneric.MakeGenericMethod(componentType).Invoke(em, new[] { (object)entity, value });
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        private static bool TryHasComponent(EntityManager em, Entity entity, Type componentType)
        {
            if (HasComponentGeneric == null)
            {
                return false;
            }

            try
            {
                var result = HasComponentGeneric.MakeGenericMethod(componentType).Invoke(em, new object[] { entity });
                return result is bool has && has;
            }
            catch
            {
                return false;
            }
        }

        private static bool TryAddComponent(EntityManager em, Entity entity, Type componentType, out string error)
        {
            error = string.Empty;

            if (AddComponentGeneric == null)
            {
                error = "AddComponent<T> method unavailable";
                return false;
            }

            try
            {
                AddComponentGeneric.MakeGenericMethod(componentType).Invoke(em, new object[] { entity });
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        private static bool TryRemoveComponent(EntityManager em, Entity entity, Type componentType, out string error)
        {
            error = string.Empty;

            if (RemoveComponentGeneric == null)
            {
                error = "RemoveComponent<T> method unavailable";
                return false;
            }

            try
            {
                RemoveComponentGeneric.MakeGenericMethod(componentType).Invoke(em, new object[] { entity });
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        private static void EnsureSnapshotsLoaded()
        {
            if (_snapshotsLoaded)
            {
                return;
            }

            lock (_snapshotFileLock)
            {
                if (_snapshotsLoaded)
                {
                    return;
                }

                SandboxSnapshotStore.ClearAll();

                if (!_persistSnapshots)
                {
                    _snapshotsLoaded = true;
                    return;
                }

                var snapshotDirectory = ResolveSnapshotDirectory();
                if (string.IsNullOrWhiteSpace(snapshotDirectory))
                {
                    _snapshotsLoaded = true;
                    return;
                }

                var baselinePath = Path.Combine(snapshotDirectory, BaselineCsvFileName);
                var deltaPath = Path.Combine(snapshotDirectory, DeltaCsvFileName);
                var legacyPath = ResolveLegacyJsonPath(snapshotDirectory);

                try
                {
                    if (File.Exists(baselinePath) || File.Exists(deltaPath))
                    {
                        var baselineRows = SnapshotPersistenceService.ReadBaseline(baselinePath);
                        var deltaRows = SnapshotPersistenceService.ReadDelta(deltaPath);
                        var baselines = BuildBaselineSnapshotsFromRows(baselineRows);
                        var deltas = BuildDeltaSnapshotsFromRows(deltaRows);
                        SandboxSnapshotStore.ImportActiveSnapshots(baselines, deltas, markDirty: false);
                        LogDebug($"Loaded sandbox snapshots from CSV: baselines={baselines.Length}, deltas={deltas.Length}.");
                        _snapshotsLoaded = true;
                        return;
                    }

                    if (File.Exists(legacyPath))
                    {
                        var json = File.ReadAllText(legacyPath);
                        var envelope = JsonSerializer.Deserialize<SnapshotEnvelope>(json, JsonOpts);
                        var migrated = ConvertLegacyEnvelopeToBaselines(envelope);
                        if (migrated.Length > 0)
                        {
                            SandboxSnapshotStore.ImportActiveSnapshots(migrated, Array.Empty<SandboxDeltaSnapshot>(), markDirty: true);
                            LogInfo($"Migrated {migrated.Length} legacy sandbox snapshots to in-memory baseline format.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogWarning($"Snapshot file load failed: {ex.Message}");
                }

                _snapshotsLoaded = true;
            }
        }

        private static void PersistSnapshotsToDisk()
        {
            if (!_persistSnapshots || !SandboxSnapshotStore.IsDirty)
            {
                return;
            }

            var snapshotDirectory = ResolveSnapshotDirectory();
            if (string.IsNullOrWhiteSpace(snapshotDirectory))
            {
                return;
            }

            var baselinePath = Path.Combine(snapshotDirectory, BaselineCsvFileName);
            var deltaPath = Path.Combine(snapshotDirectory, DeltaCsvFileName);

            lock (_snapshotFileLock)
            {
                if (!SandboxSnapshotStore.IsDirty)
                {
                    return;
                }

                try
                {
                    Directory.CreateDirectory(snapshotDirectory);

                    var baselineRows = SandboxSnapshotStore.GetActiveBaselines()
                        .SelectMany(snapshot => snapshot.Rows ?? Array.Empty<BaselineRow>())
                        .ToArray();
                    var deltaRows = SandboxSnapshotStore.GetActiveDeltas()
                        .SelectMany(snapshot => snapshot.Rows ?? Array.Empty<DeltaRow>())
                        .ToArray();

                    if (baselineRows.Length == 0)
                    {
                        DeleteIfExists(baselinePath);
                    }
                    else
                    {
                        SnapshotPersistenceService.WriteBaseline(baselinePath, baselineRows);
                    }

                    if (deltaRows.Length == 0)
                    {
                        DeleteIfExists(deltaPath);
                    }
                    else
                    {
                        SnapshotPersistenceService.WriteDelta(deltaPath, deltaRows);
                    }

                    SandboxSnapshotStore.MarkClean();
                }
                catch (Exception ex)
                {
                    LogWarning($"Snapshot persist failed: {ex.Message}");
                }
            }
        }

        private static void TryAppendProgressionJournal(string playerKey, SandboxPendingContext pending, BaselineRow[] postRows, DateTime capturedUtc)
        {
            if (!_persistSnapshots)
            {
                return;
            }

            var snapshotDirectory = ResolveSnapshotDirectory();
            if (string.IsNullOrWhiteSpace(snapshotDirectory))
            {
                return;
            }

            try
            {
                Directory.CreateDirectory(snapshotDirectory);
                var journalPath = Path.Combine(snapshotDirectory, ProgressionJournalJsonlFileName);

                var preRows = pending?.ComponentRows ?? Array.Empty<BaselineRow>();
                postRows ??= Array.Empty<BaselineRow>();

                var preByType = preRows
                    .Where(r => r != null && string.Equals(r.RowType, "component", StringComparison.OrdinalIgnoreCase))
                    .GroupBy(r => r.AssemblyQualifiedType ?? string.Empty, StringComparer.Ordinal)
                    .ToDictionary(g => g.Key, g => g.Last(), StringComparer.Ordinal);

                var postByType = postRows
                    .Where(r => r != null && string.Equals(r.RowType, "component", StringComparison.OrdinalIgnoreCase))
                    .GroupBy(r => r.AssemblyQualifiedType ?? string.Empty, StringComparer.Ordinal)
                    .ToDictionary(g => g.Key, g => g.Last(), StringComparer.Ordinal);

                var keys = new HashSet<string>(preByType.Keys, StringComparer.Ordinal);
                keys.UnionWith(postByType.Keys);

                using var stream = new FileStream(journalPath, FileMode.Append, FileAccess.Write, FileShare.Read);
                using var writer = new StreamWriter(stream, Encoding.UTF8);

                foreach (var typeKey in keys)
                {
                    preByType.TryGetValue(typeKey, out var beforeRow);
                    postByType.TryGetValue(typeKey, out var afterRow);

                    var beforePayload = DecodePayload(beforeRow?.PayloadBase64 ?? string.Empty);
                    var afterPayload = DecodePayload(afterRow?.PayloadBase64 ?? string.Empty);

                    if (string.Equals(beforePayload, afterPayload, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    var op = beforeRow == null
                        ? "add"
                        : afterRow == null
                            ? "remove"
                            : "modify";

                    var evt = new ProgressionJournalEvent
                    {
                        Version = 1,
                        SnapshotId = pending?.SnapshotId ?? string.Empty,
                        PlayerKey = playerKey ?? string.Empty,
                        CharacterName = pending?.CharacterName ?? string.Empty,
                        PlatformId = pending?.PlatformId ?? 0,
                        ZoneId = pending?.ZoneId ?? string.Empty,
                        CapturedUtc = capturedUtc,
                        Operation = op,
                        ComponentType = ResolveComponentTypeName(typeKey),
                        AssemblyQualifiedType = typeKey,
                        BeforeJson = beforePayload,
                        AfterJson = afterPayload
                    };

                    writer.WriteLine(JsonSerializer.Serialize(evt, JsonOpts));
                }
            }
            catch (Exception ex)
            {
                LogWarning($"Progression journal append failed: {ex.Message}");
            }
        }

        private static string ResolveSnapshotDirectory()
        {
            if (string.IsNullOrWhiteSpace(_snapshotPath))
            {
                return string.Empty;
            }

            var normalized = _snapshotPath.Trim();
            if (Path.HasExtension(normalized))
            {
                return Path.GetDirectoryName(normalized) ?? string.Empty;
            }

            return normalized;
        }

        private static string ResolveLegacyJsonPath(string snapshotDirectory)
        {
            if (!string.IsNullOrWhiteSpace(_snapshotPath) &&
                string.Equals(Path.GetExtension(_snapshotPath), ".json", StringComparison.OrdinalIgnoreCase))
            {
                return _snapshotPath;
            }

            return Path.Combine(snapshotDirectory, LegacyJsonFileName);
        }

        private static SandboxBaselineSnapshot[] BuildBaselineSnapshotsFromRows(IEnumerable<BaselineRow> rows)
        {
            return (rows ?? Array.Empty<BaselineRow>())
                .Where(row => row != null)
                .GroupBy(
                    row => ResolvePlayerKey(row.PlayerKey, row.CharacterName, row.PlatformId),
                    StringComparer.Ordinal)
                .Select(group =>
                {
                    var ordered = group.OrderBy(row => row.CapturedUtc).ToArray();
                    var first = ordered[0];
                    return new SandboxBaselineSnapshot
                    {
                        PlayerKey = group.Key,
                        CharacterName = first.CharacterName,
                        PlatformId = first.PlatformId,
                        ZoneId = first.ZoneId,
                        SnapshotId = first.SnapshotId,
                        CapturedUtc = first.CapturedUtc,
                        Rows = ordered
                    };
                })
                .ToArray();
        }

        private static SandboxDeltaSnapshot[] BuildDeltaSnapshotsFromRows(IEnumerable<DeltaRow> rows)
        {
            return (rows ?? Array.Empty<DeltaRow>())
                .Where(row => row != null)
                .GroupBy(
                    row => ResolvePlayerKey(row.PlayerKey, row.CharacterName, row.PlatformId),
                    StringComparer.Ordinal)
                .Select(group =>
                {
                    var ordered = group.OrderBy(row => row.CapturedUtc).ToArray();
                    var first = ordered[0];
                    return new SandboxDeltaSnapshot
                    {
                        PlayerKey = group.Key,
                        CharacterName = first.CharacterName,
                        PlatformId = first.PlatformId,
                        ZoneId = first.ZoneId,
                        SnapshotId = first.SnapshotId,
                        CapturedUtc = first.CapturedUtc,
                        Rows = ordered
                    };
                })
                .ToArray();
        }

        private static string ResolvePlayerKey(string? key, string characterName, ulong platformId)
        {
            if (!string.IsNullOrWhiteSpace(key))
            {
                return key;
            }

            return SandboxSnapshotStore.GetPreferredPlayerKey(characterName, platformId);
        }

        private static SandboxBaselineSnapshot[] ConvertLegacyEnvelopeToBaselines(SnapshotEnvelope? envelope)
        {
            if (envelope?.Players == null || envelope.Players.Count == 0)
            {
                return Array.Empty<SandboxBaselineSnapshot>();
            }

            var snapshots = new List<SandboxBaselineSnapshot>();
            foreach (var pair in envelope.Players)
            {
                if (pair.Value == null)
                {
                    continue;
                }

                var platformId = pair.Value.PlatformId;
                if (platformId == 0 && !ulong.TryParse(pair.Key, NumberStyles.Integer, CultureInfo.InvariantCulture, out platformId))
                {
                    continue;
                }

                var capturedUtc = pair.Value.CapturedUtc == default ? DateTime.UtcNow : pair.Value.CapturedUtc;
                var characterName = NormalizeCharacterName(string.Empty, platformId);
                var playerKey = SandboxSnapshotStore.GetPreferredPlayerKey(characterName, platformId);
                var snapshotId = BuildSnapshotIdCore(platformId, characterName, capturedUtc);
                var rows = BuildBaselineRowsCore(pair.Value, playerKey, characterName, platformId, string.Empty, snapshotId, capturedUtc);
                snapshots.Add(new SandboxBaselineSnapshot
                {
                    PlayerKey = playerKey,
                    CharacterName = characterName,
                    PlatformId = platformId,
                    ZoneId = string.Empty,
                    SnapshotId = snapshotId,
                    CapturedUtc = capturedUtc,
                    Rows = rows
                });
            }

            return snapshots.ToArray();
        }

        private static void DeleteIfExists(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch (Exception ex)
            {
                LogWarning($"Failed deleting snapshot file '{path}': {ex.Message}");
            }
        }

        private static void LogInfo(string message)
        {
            try
            {
                VAutomationCore.Plugin.Log.LogInfo($"[DebugEventBridge] {message}");
            }
            catch
            {
                // ignored
            }
        }

        private static void LogWarning(string message)
        {
            try
            {
                VAutomationCore.Plugin.Log.LogWarning($"[DebugEventBridge] {message}");
            }
            catch
            {
                // ignored
            }
        }

        private static void LogDebug(string message)
        {
            if (_verboseLogs)
            {
                LogInfo(message);
            }
        }

        private sealed class SnapshotEnvelope
        {
            public int Version { get; set; } = 1;
            public Dictionary<string, SandboxProgressionSnapshot> Players { get; set; } = new(StringComparer.Ordinal);
        }
    }

    internal sealed class SandboxProgressionSnapshot
    {
        public ulong PlatformId { get; set; }
        public DateTime CapturedUtc { get; set; }
        public Dictionary<string, SnapshotComponentState> Components { get; set; } = new(StringComparer.Ordinal);
    }

    internal sealed class SnapshotComponentState
    {
        public bool Existed { get; set; }
        public string AssemblyQualifiedType { get; set; } = string.Empty;
        public string JsonPayload { get; set; } = string.Empty;
    }

    internal sealed class ProgressionJournalEvent
    {
        public int Version { get; set; } = 1;
        public string SnapshotId { get; set; } = string.Empty;
        public string PlayerKey { get; set; } = string.Empty;
        public string CharacterName { get; set; } = string.Empty;
        public ulong PlatformId { get; set; }
        public string ZoneId { get; set; } = string.Empty;
        public DateTime CapturedUtc { get; set; }
        public string Operation { get; set; } = string.Empty;
        public string ComponentType { get; set; } = string.Empty;
        public string AssemblyQualifiedType { get; set; } = string.Empty;
        public string BeforeJson { get; set; } = string.Empty;
        public string AfterJson { get; set; } = string.Empty;
    }
}
