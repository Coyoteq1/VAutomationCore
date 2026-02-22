using System;
using System.Collections.Generic;
using Unity.Entities;
using VAutomationCore.Core.Services;

namespace VAutomationCore.Core.Api
{
    /// <summary>
    /// Alias map for runtime entities used by flow execution.
    /// </summary>
    public sealed class EntityMap
    {
        private readonly Dictionary<string, Entity> _map = new(StringComparer.OrdinalIgnoreCase);

        public int Count => _map.Count;

        public bool Map(string alias, Entity entity, bool replace = true)
        {
            if (string.IsNullOrWhiteSpace(alias))
            {
                return false;
            }

            var key = alias.Trim();
            if (_map.ContainsKey(key))
            {
                if (!replace)
                {
                    return false;
                }

                _map[key] = entity;
                return true;
            }

            _map.Add(key, entity);
            return true;
        }

        public bool TryGet(string alias, out Entity entity)
        {
            entity = Entity.Null;
            if (string.IsNullOrWhiteSpace(alias))
            {
                return false;
            }

            return _map.TryGetValue(alias.Trim(), out entity);
        }

        public bool Remove(string alias)
        {
            if (string.IsNullOrWhiteSpace(alias))
            {
                return false;
            }

            return _map.Remove(alias.Trim());
        }

        public void Clear()
        {
            _map.Clear();
        }

        public IReadOnlyDictionary<string, Entity> Snapshot()
        {
            return new Dictionary<string, Entity>(_map, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Convenience mapping from platform ID to User entity.
        /// </summary>
        public bool TryMapUserByPlatformId(string alias, ulong platformId, bool replace = true)
        {
            if (!GameActionService.TryFindUserEntityByPlatformId(platformId, out var userEntity))
            {
                return false;
            }

            return Map(alias, userEntity, replace);
        }
    }
}
