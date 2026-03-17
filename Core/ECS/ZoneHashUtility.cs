using System.Collections.Generic;
using System.Text;
using Unity.Mathematics;

namespace VAutomationCore.Core.ECS
{
    public static class ZoneHashUtility
    {
        private static readonly Dictionary<string, int> ZoneIdToHash = new();
        private static readonly Dictionary<int, string> HashToZoneId = new();
        private static readonly Dictionary<int, float3> HashToCenter = new();
        private static readonly object Sync = new();
        private static bool _initialized;

        public static void Initialize()
        {
            if (_initialized) return;
            _initialized = true;
        }

        public static int GetZoneHash(string zoneId)
        {
            if (string.IsNullOrEmpty(zoneId)) return 0;
            lock (Sync)
            {
                var normalizedZoneId = zoneId.Trim();
                if (ZoneIdToHash.TryGetValue(normalizedZoneId, out var existingHash))
                {
                    return existingHash;
                }

                var hash = ComputeStableHash(zoneId);
                if (hash == 0)
                {
                    hash = 1;
                }

                // Resolve collisions deterministically by re-hashing with a numeric salt.
                // Added maxRetries to prevent infinite loop on persistent hash collisions.
                const int maxRetries = 10;
                int retries = 0;
                while (retries < maxRetries && HashToZoneId.TryGetValue(hash, out var existing) &&
                       !string.Equals(existing, normalizedZoneId, System.StringComparison.Ordinal))
                {
                    hash = ComputeStableHash(normalizedZoneId + "#" + hash);
                    if (hash == 0)
                    {
                        hash = 1;
                    }
                    retries++;
                }

                // If we hit the retry limit, fail safely by returning a unique hash based on the original
                if (retries >= maxRetries && HashToZoneId.ContainsKey(hash))
                {
                    // Use timestamp-based fallback to ensure uniqueness
                    hash = ComputeStableHash(normalizedZoneId + "_" + DateTime.UtcNow.Ticks);
                    if (hash == 0) hash = 1;
                }

                ZoneIdToHash[normalizedZoneId] = hash;
                HashToZoneId[hash] = normalizedZoneId;
                return hash;
            }
        }

        public static string GetZoneId(int hash)
        {
            if (hash == 0)
            {
                return string.Empty;
            }

            lock (Sync)
            {
                return HashToZoneId.TryGetValue(hash, out var zoneId) ? zoneId : string.Empty;
            }
        }

        public static void CacheZoneCenter(int hash, float3 center)
        {
            if (hash == 0)
            {
                return;
            }

            lock (Sync)
            {
                HashToCenter[hash] = center;
            }
        }

        public static float3 GetZoneCenter(int hash)
        {
            lock (Sync)
            {
                return HashToCenter.GetValueOrDefault(hash, float3.zero);
            }
        }

        public static bool AreZonesSameLocation(int hashA, int hashB)
        {
            if (hashA == 0 || hashB == 0) return false;
            var centerA = GetZoneCenter(hashA);
            var centerB = GetZoneCenter(hashB);
            return math.distancesq(centerA, centerB) < 0.01f;
        }

        private static int ComputeStableHash(string value)
        {
            const uint offsetBasis = 2166136261;
            const uint prime = 16777619;

            var bytes = Encoding.UTF8.GetBytes((value ?? string.Empty).Trim().ToLowerInvariant());
            uint hash = offsetBasis;
            for (var i = 0; i < bytes.Length; i++)
            {
                hash ^= bytes[i];
                hash *= prime;
            }

            return unchecked((int)hash);
        }
    }
}
