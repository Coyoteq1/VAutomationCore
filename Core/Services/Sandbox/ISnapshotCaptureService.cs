using System;
using Unity.Entities;

namespace VAuto.Core.Services
{
    internal interface ISnapshotCaptureService
    {
        string BuildSnapshotId(ulong platformId, string characterName, DateTime capturedUtc);
        SandboxProgressionSnapshot CaptureProgressionSnapshot(Entity character);
        BaselineRow[] BuildBaselineRows(
            SandboxProgressionSnapshot snapshot,
            string playerKey,
            string characterName,
            ulong platformId,
            string zoneId,
            string snapshotId,
            DateTime capturedUtc);
        ZoneEntityEntry[] CaptureZoneEntityMap(string zoneId);
    }
}
