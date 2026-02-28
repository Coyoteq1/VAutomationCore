using System;
using System.Collections.Generic;
using System.Linq;
using VAuto.Zone.Core;

namespace VAuto.Zone.Services
{
    public static class GlowService
    {
        public static int[] GetValidatedGlowBuffHashes()
        {
            var configured = ParseGuidList(Plugin.GlowSystemDefaultBuffGuidsValue);
            if (configured.Length > 0)
            {
                return configured;
            }

            return new[] { 4345235, 54252435, 65245252 };
        }

        private static int[] ParseGuidList(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return Array.Empty<int>();
            }

            var values = new List<int>();
            var tokens = raw.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var token in tokens)
            {
                if (int.TryParse(token.Trim(), out var hash) && hash != 0)
                {
                    values.Add(hash);
                }
            }

            return values.Distinct().ToArray();
        }

        public static bool TryResolve(string token, out int guidHash)
        {
            guidHash = 0;
            if (string.IsNullOrWhiteSpace(token))
            {
                return false;
            }

            var lookup = token.Trim();
            if (int.TryParse(lookup, out var numeric) && numeric != 0)
            {
                guidHash = numeric;
                return true;
            }

            if (PrefabReferenceCatalog.TryResolve(lookup, out var guid))
            {
                guidHash = guid.GuidHash;
                return true;
            }

            return false;
        }
    }
}
