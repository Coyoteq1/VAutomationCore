using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace VAutomationCore.Core.Api
{
    /// <summary>
    /// Castle-related shared state API.
    /// This is a lightweight cross-module registry for castle ownership/flags.
    /// </summary>
    public static class CastleApi
    {
        public sealed class CastleState
        {
            public string CastleId { get; init; } = string.Empty;
            public ulong OwnerSubjectId { get; set; }
            public bool PvpEnabled { get; set; }
            public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
        }

        private static readonly ConcurrentDictionary<string, CastleState> Castles =
            new(StringComparer.OrdinalIgnoreCase);

        public static bool SetOwner(string castleId, ulong ownerSubjectId)
        {
            if (string.IsNullOrWhiteSpace(castleId))
            {
                return false;
            }

            var id = castleId.Trim();
            Castles.AddOrUpdate(
                id,
                _ => new CastleState
                {
                    CastleId = id,
                    OwnerSubjectId = ownerSubjectId,
                    UpdatedAtUtc = DateTime.UtcNow
                },
                (_, state) =>
                {
                    state.OwnerSubjectId = ownerSubjectId;
                    state.UpdatedAtUtc = DateTime.UtcNow;
                    return state;
                });
            return true;
        }

        public static bool SetPvpEnabled(string castleId, bool enabled)
        {
            if (string.IsNullOrWhiteSpace(castleId))
            {
                return false;
            }

            var id = castleId.Trim();
            Castles.AddOrUpdate(
                id,
                _ => new CastleState
                {
                    CastleId = id,
                    PvpEnabled = enabled,
                    UpdatedAtUtc = DateTime.UtcNow
                },
                (_, state) =>
                {
                    state.PvpEnabled = enabled;
                    state.UpdatedAtUtc = DateTime.UtcNow;
                    return state;
                });
            return true;
        }

        public static bool TryGetState(string castleId, out CastleState state)
        {
            state = null!;
            if (string.IsNullOrWhiteSpace(castleId))
            {
                return false;
            }

            return Castles.TryGetValue(castleId.Trim(), out state);
        }

        public static bool Remove(string castleId)
        {
            if (string.IsNullOrWhiteSpace(castleId))
            {
                return false;
            }

            return Castles.TryRemove(castleId.Trim(), out _);
        }

        public static IReadOnlyCollection<CastleState> Snapshot()
        {
            return Castles.Values.OrderBy(x => x.CastleId, StringComparer.OrdinalIgnoreCase).ToArray();
        }
    }
}
