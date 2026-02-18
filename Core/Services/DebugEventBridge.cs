using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using ProjectM;
using ProjectM.Gameplay.Systems;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;
using VAutomationCore.Core;

namespace VAuto.Core.Services
{
    public static class DebugEventBridge
    {
        private static readonly string[] ProgressionKeywords = { "Research", "VBlood", "Achievement", "Unlock", "Tech", "Recipe", "Progress" };
        private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true, WriteIndented = true };

        private static bool _enabled = true;
        private static bool _persistSnapshots = true;
        private static string _snapshotPath = string.Empty;
        private static bool _verboseLogs;

        private static readonly HashSet<ulong> _unlockAppliedThisSession = new();
        private static readonly Dictionary<ulong, SandboxProgressionSnapshot> _activeSnapshots = new();
        private static readonly Dictionary<ulong, SandboxProgressionSnapshot> _persistedSnapshots = new();
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

            EnsureSnapshotsLoaded();
            LogInfo($"Configured sandbox progression: enabled={_enabled}, persist={_persistSnapshots}, path='{_snapshotPath}'.");
        }

        public static void OnPlayerEnterZone(Entity character) => OnPlayerEnterZone(character, true);
        public static void OnPlayerIsInZone(Entity character) => OnPlayerIsInZone(character, true);
        public static void OnPlayerExitZone(Entity character) => OnPlayerExitZone(character, true);

        public static void OnPlayerEnterZone(Entity character, bool enableUnlock)
        {
            if (!_enabled || !enableUnlock)
            {
                return;
            }

            EnsureSnapshotsLoaded();

            if (!TryGetPlatformId(character, out var platformId))
            {
                return;
            }

            lock (_stateLock)
            {
                if (_unlockAppliedThisSession.Contains(platformId))
                {
                    return;
                }

                if (!_activeSnapshots.ContainsKey(platformId))
                {
                    var snapshot = CaptureProgressionSnapshot(character);
                    _activeSnapshots[platformId] = snapshot;
                    _persistedSnapshots[platformId] = snapshot;
                    PersistSnapshotsToDisk();
                }
            }

            ApplyFullUnlock(character);

            lock (_stateLock)
            {
                _unlockAppliedThisSession.Add(platformId);
            }
        }

        public static void OnPlayerIsInZone(Entity character, bool enableUnlock)
        {
            // Unlock intentionally runs on enter only.
        }

        public static void OnPlayerExitZone(Entity character, bool enableUnlock)
        {
            if (!TryGetPlatformId(character, out var platformId))
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

            SandboxProgressionSnapshot? snapshot;
            lock (_stateLock)
            {
                if (!_activeSnapshots.TryGetValue(platformId, out snapshot))
                {
                    return;
                }
            }

            if (snapshot == null)
            {
                return;
            }

            if (!RestoreProgressionSnapshot(character, snapshot))
            {
                LogWarning($"Restore failed for platformId={platformId}.");
                return;
            }

            lock (_stateLock)
            {
                _activeSnapshots.Remove(platformId);
                _persistedSnapshots.Remove(platformId);
                _unlockAppliedThisSession.Remove(platformId);
            }

            PersistSnapshotsToDisk();
        }

        private static bool TryGetPlatformId(Entity character, out ulong platformId)
        {
            platformId = 0;

            try
            {
                var em = UnifiedCore.EntityManager;
                if (em == default || !em.Exists(character))
                {
                    return false;
                }

                if (!em.HasComponent<PlayerCharacter>(character))
                {
                    return false;
                }

                var playerCharacter = em.GetComponentData<PlayerCharacter>(character);
                if (!em.Exists(playerCharacter.UserEntity) || !em.HasComponent<User>(playerCharacter.UserEntity))
                {
                    return false;
                }

                platformId = em.GetComponentData<User>(playerCharacter.UserEntity).PlatformId;
                if (platformId == 0)
                {
                    LogWarning("Sandbox progression skipped: platformId == 0");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                LogWarning($"TryGetPlatformId failed: {ex.Message}");
                return false;
            }
        }

        private static bool TryGetUserEntity(Entity character, out Entity userEntity)
        {
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

                userEntity = playerCharacter.UserEntity;
                return true;
            }
            catch
            {
                return false;
            }
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

        private static SandboxProgressionSnapshot CaptureProgressionSnapshot(Entity character)
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

        private static bool RestoreProgressionSnapshot(Entity character, SandboxProgressionSnapshot snapshot)
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

                lock (_stateLock)
                {
                    _activeSnapshots.Clear();
                    _persistedSnapshots.Clear();
                }

                if (!_persistSnapshots || string.IsNullOrWhiteSpace(_snapshotPath))
                {
                    _snapshotsLoaded = true;
                    return;
                }

                try
                {
                    if (!File.Exists(_snapshotPath))
                    {
                        _snapshotsLoaded = true;
                        return;
                    }

                    var json = File.ReadAllText(_snapshotPath);
                    var envelope = JsonSerializer.Deserialize<SnapshotEnvelope>(json, JsonOpts);
                    if (envelope?.Players != null)
                    {
                        lock (_stateLock)
                        {
                            foreach (var pair in envelope.Players)
                            {
                                if (!ulong.TryParse(pair.Key, out var platformId) || platformId == 0 || pair.Value == null)
                                {
                                    continue;
                                }

                                _persistedSnapshots[platformId] = pair.Value;
                                _activeSnapshots[platformId] = pair.Value;
                            }
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
            if (!_persistSnapshots || string.IsNullOrWhiteSpace(_snapshotPath))
            {
                return;
            }

            Dictionary<ulong, SandboxProgressionSnapshot> snapshotCopy;
            lock (_stateLock)
            {
                snapshotCopy = new Dictionary<ulong, SandboxProgressionSnapshot>(_activeSnapshots);
            }

            lock (_snapshotFileLock)
            {
                try
                {
                    var directory = Path.GetDirectoryName(_snapshotPath);
                    if (!string.IsNullOrWhiteSpace(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    if (snapshotCopy.Count == 0)
                    {
                        if (File.Exists(_snapshotPath))
                        {
                            File.Delete(_snapshotPath);
                        }
                        return;
                    }

                    var envelope = new SnapshotEnvelope
                    {
                        Version = 1,
                        Players = snapshotCopy.ToDictionary(pair => pair.Key.ToString(), pair => pair.Value, StringComparer.Ordinal)
                    };

                    var tempPath = _snapshotPath + ".tmp";
                    File.WriteAllText(tempPath, JsonSerializer.Serialize(envelope, JsonOpts));
                    File.Copy(tempPath, _snapshotPath, true);
                    File.Delete(tempPath);

                    lock (_stateLock)
                    {
                        _persistedSnapshots.Clear();
                        foreach (var pair in snapshotCopy)
                        {
                            _persistedSnapshots[pair.Key] = pair.Value;
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogWarning($"Snapshot persist failed: {ex.Message}");
                }
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
}
