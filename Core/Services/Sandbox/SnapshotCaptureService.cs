using System;
using Unity.Entities;

namespace VAuto.Core.Services
{
    internal sealed class SnapshotCaptureService : ISnapshotCaptureService
    {
        public string BuildSnapshotId(ulong platformId, string characterName, DateTime capturedUtc)
            => DebugEventBridge.BuildSnapshotIdCore(platformId, characterName, capturedUtc);

        public SandboxProgressionSnapshot CaptureProgressionSnapshot(Entity character)
            => DebugEventBridge.CaptureProgressionSnapshotCore(character);

        public BaselineRow[] BuildBaselineRows(
            SandboxProgressionSnapshot snapshot,
            string playerKey,
            string characterName,
            ulong platformId,
            string zoneId,
            string snapshotId,
            DateTime capturedUtc)
            => DebugEventBridge.BuildBaselineRowsCore(snapshot, playerKey, characterName, platformId, zoneId, snapshotId, capturedUtc);

        public ZoneEntityEntry[] CaptureZoneEntityMap(string zoneId)
            => DebugEventBridge.CaptureZoneEntityMapCore(zoneId);
    }
}
