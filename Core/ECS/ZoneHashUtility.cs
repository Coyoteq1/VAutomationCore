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
        private static bool _initialized;

        public static void Initialize()
        {
            if (_initialized) return;
            _initialized = true;
        }

        public static int GetZoneHash(string zoneId)
        {
            if (string.IsNullOrEmpty(zoneId)) return 0;
            if (ZoneIdToHash.TryGetValue(zoneId, out var hash)) return hash;

            hash = ComputeStableHash(zoneId);
            if (hash == 0)
            {
                hash = 1;
            }

            var normalizedZoneId = zoneId.Trim();
            while (HashToZoneId.TryGetValue(hash, out var existing) &&
                   !string.Equals(existing, normalizedZoneId, System.StringComparison.Ordinal))
            {
                // Resolve collisions deterministically by re-hashing with a numeric salt.
                hash = ComputeStableHash(normalizedZoneId + "#" + hash);
                if (hash == 0)
                {
                    hash = 1;
                }
            }

            ZoneIdToHash[normalizedZoneId] = hash;
            HashToZoneId[hash] = normalizedZoneId;
            return hash;
        }

        public static string GetZoneId(int hash)
        {
            return hash == 0 ? string.Empty : HashToZoneId.GetValueOrDefault(hash, string.Empty);
        }

        public static void CacheZoneCenter(int hash, float3 center)
        {
            if (hash != 0) HashToCenter[hash] = center;
        }

        public static float3 GetZoneCenter(int hash)
        {
            return HashToCenter.GetValueOrDefault(hash, float3.zero);
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
