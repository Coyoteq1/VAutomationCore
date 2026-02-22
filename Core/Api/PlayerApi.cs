using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;

namespace VAutomationCore.Core.Api
{
    /// <summary>
    /// Player/session APIs shared across modules.
    /// Uses subject id (platform id or console subject id) as the stable key.
    /// </summary>
    public static class PlayerApi
    {
        private static readonly ConcurrentDictionary<ulong, EntityMap> PlayerMaps = new();

        public static EntityMap GetOrCreateEntityMap(ulong subjectId)
        {
            return PlayerMaps.GetOrAdd(subjectId, _ => new EntityMap());
        }

        public static bool TryGetEntityMap(ulong subjectId, out EntityMap map)
        {
            return PlayerMaps.TryGetValue(subjectId, out map!);
        }

        public static bool RemoveEntityMap(ulong subjectId)
        {
            return PlayerMaps.TryRemove(subjectId, out _);
        }

        public static IReadOnlyCollection<ulong> GetActiveSubjects()
        {
            return PlayerMaps.Keys.ToArray();
        }

        public static bool TryMapEntity(ulong subjectId, string alias, Entity entity, bool replace = true)
        {
            if (string.IsNullOrWhiteSpace(alias) || entity == Entity.Null)
            {
                return false;
            }

            var map = GetOrCreateEntityMap(subjectId);
            return map.Map(alias.Trim(), entity, replace);
        }

        public static bool TryResolveEntity(ulong subjectId, string alias, out Entity entity)
        {
            entity = Entity.Null;
            if (string.IsNullOrWhiteSpace(alias))
            {
                return false;
            }

            if (!TryGetEntityMap(subjectId, out var map))
            {
                return false;
            }

            return map.TryGet(alias.Trim(), out entity);
        }
    }
}
