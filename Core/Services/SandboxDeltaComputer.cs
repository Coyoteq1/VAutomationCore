using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace VAuto.Core.Services
{
    internal static class SandboxDeltaComputer
    {
        public static IEnumerable<DeltaRow> ComputeComponentDelta(IEnumerable<BaselineRow> pre, IEnumerable<BaselineRow> post)
        {
            var preMap = (pre ?? Array.Empty<BaselineRow>())
                .Where(row => row != null && !string.IsNullOrWhiteSpace(row.AssemblyQualifiedType))
                .ToDictionary(
                    row => row.AssemblyQualifiedType,
                    row => row,
                    StringComparer.Ordinal);

            var postMap = (post ?? Array.Empty<BaselineRow>())
                .Where(row => row != null && !string.IsNullOrWhiteSpace(row.AssemblyQualifiedType))
                .ToDictionary(
                    row => row.AssemblyQualifiedType,
                    row => row,
                    StringComparer.Ordinal);

            var allKeys = new HashSet<string>(preMap.Keys, StringComparer.Ordinal);
            allKeys.UnionWith(postMap.Keys);

            var deltaRows = new List<DeltaRow>();
            foreach (var key in allKeys)
            {
                preMap.TryGetValue(key, out var before);
                postMap.TryGetValue(key, out var after);

                var beforePayload = before?.PayloadBase64 ?? string.Empty;
                var afterPayload = after?.PayloadBase64 ?? string.Empty;
                var beforeExisted = before?.Existed ?? false;
                var afterExisted = after?.Existed ?? false;

                if (beforeExisted == afterExisted &&
                    string.Equals(beforePayload, afterPayload, StringComparison.Ordinal))
                {
                    continue;
                }

                deltaRows.Add(new DeltaRow
                {
                    RowType = "component_changed",
                    Operation = "changed",
                    ComponentType = after?.ComponentType ?? before?.ComponentType ?? key,
                    BeforePayloadBase64 = beforePayload,
                    AfterPayloadBase64 = afterPayload
                });
            }

            return deltaRows;
        }

        public static IEnumerable<DeltaRow> ExtractOpenedTech(IEnumerable<BaselineRow> preComponents, IEnumerable<BaselineRow> postComponents)
        {
            var preGuids = ExtractGuidSet(preComponents);
            var postGuids = ExtractGuidSet(postComponents);
            postGuids.ExceptWith(preGuids);

            return postGuids.Select(guid => new DeltaRow
            {
                RowType = "tech_opened",
                Operation = "opened",
                TechGuid = guid,
                TechName = $"GUID:{guid.ToString(CultureInfo.InvariantCulture)}"
            });
        }

        public static IEnumerable<DeltaRow> ComputeEntityDelta(IEnumerable<ZoneEntityEntry> preZoneEntities, IEnumerable<ZoneEntityEntry> postZoneEntities)
        {
            var preMap = BuildEntityMap(preZoneEntities);
            var postMap = BuildEntityMap(postZoneEntities);

            var results = new List<DeltaRow>();

            foreach (var pair in postMap)
            {
                if (!preMap.TryGetValue(pair.Key, out var before))
                {
                    var after = pair.Value;
                    results.Add(new DeltaRow
                    {
                        RowType = "entity_created",
                        Operation = "created",
                        EntityIndex = after.EntityIndex,
                        EntityVersion = after.EntityVersion,
                        PrefabGuid = after.PrefabGuidHash,
                        PrefabName = after.PrefabName,
                        PosX = after.PosX,
                        PosY = after.PosY,
                        PosZ = after.PosZ
                    });

                    continue;
                }

                var changedPrefab = before.PrefabGuidHash != pair.Value.PrefabGuidHash;
                if (!changedPrefab)
                {
                    continue;
                }

                var beforePayload = Convert.ToBase64String(Encoding.UTF8.GetBytes(before.PrefabGuidHash.ToString(CultureInfo.InvariantCulture)));
                var afterPayload = Convert.ToBase64String(Encoding.UTF8.GetBytes(pair.Value.PrefabGuidHash.ToString(CultureInfo.InvariantCulture)));

                results.Add(new DeltaRow
                {
                    RowType = "entity_prefab_changed",
                    Operation = "prefab_changed",
                    BeforePayloadBase64 = beforePayload,
                    AfterPayloadBase64 = afterPayload,
                    EntityIndex = pair.Value.EntityIndex,
                    EntityVersion = pair.Value.EntityVersion,
                    PrefabGuid = pair.Value.PrefabGuidHash,
                    PrefabName = pair.Value.PrefabName,
                    PosX = pair.Value.PosX,
                    PosY = pair.Value.PosY,
                    PosZ = pair.Value.PosZ
                });
            }

            foreach (var pair in preMap)
            {
                if (postMap.ContainsKey(pair.Key))
                {
                    continue;
                }

                var removed = pair.Value;
                results.Add(new DeltaRow
                {
                    RowType = "entity_removed",
                    Operation = "removed",
                    EntityIndex = removed.EntityIndex,
                    EntityVersion = removed.EntityVersion,
                    PrefabGuid = removed.PrefabGuidHash,
                    PrefabName = removed.PrefabName,
                    PosX = removed.PosX,
                    PosY = removed.PosY,
                    PosZ = removed.PosZ
                });
            }

            return results;
        }

        private static Dictionary<string, ZoneEntityEntry> BuildEntityMap(IEnumerable<ZoneEntityEntry> entries)
        {
            var map = new Dictionary<string, ZoneEntityEntry>(StringComparer.Ordinal);
            foreach (var entry in entries ?? Array.Empty<ZoneEntityEntry>())
            {
                var key = BuildEntityKey(entry.EntityIndex, entry.EntityVersion);
                map[key] = entry;
            }

            return map;
        }

        private static string BuildEntityKey(int index, int version)
            => $"{index.ToString(CultureInfo.InvariantCulture)}:{version.ToString(CultureInfo.InvariantCulture)}";

        private static HashSet<long> ExtractGuidSet(IEnumerable<BaselineRow> rows)
        {
            var guids = new HashSet<long>();
            foreach (var row in rows ?? Array.Empty<BaselineRow>())
            {
                foreach (var guid in ExtractGuidsFromPayload(row?.PayloadBase64))
                {
                    guids.Add(guid);
                }
            }

            return guids;
        }

        private static IEnumerable<long> ExtractGuidsFromPayload(string? payloadBase64)
        {
            if (string.IsNullOrWhiteSpace(payloadBase64))
            {
                return Array.Empty<long>();
            }

            try
            {
                var bytes = Convert.FromBase64String(payloadBase64);
                using var document = JsonDocument.Parse(bytes);
                var guids = new List<long>();
                CollectGuids(document.RootElement, currentProperty: string.Empty, guids);
                return guids;
            }
            catch
            {
                // Ignore invalid payloads.
                return Array.Empty<long>();
            }
        }

        private static void CollectGuids(JsonElement element, string currentProperty, ICollection<long> guids)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    foreach (var prop in element.EnumerateObject())
                    {
                        CollectGuids(prop.Value, prop.Name, guids);
                    }
                    break;
                case JsonValueKind.Array:
                    foreach (var item in element.EnumerateArray())
                    {
                        CollectGuids(item, currentProperty, guids);
                    }
                    break;
                case JsonValueKind.Number:
                    var name = currentProperty ?? string.Empty;
                    if (name.IndexOf("guid", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        name.IndexOf("prefab", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        name.IndexOf("tech", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        name.IndexOf("unlock", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        if (element.TryGetInt64(out var asLong))
                        {
                            guids.Add(asLong);
                        }
                    }
                    break;
            }
        }
    }
}
