using Unity.Entities;

namespace VAuto.Core.Services
{
    internal sealed class ProgressionRestoreService : IProgressionRestoreService
    {
        public void TryApplyDeltaEntityCleanup(SandboxDeltaSnapshot? deltaSnapshot, string zoneId)
            => DebugEventBridge.TryApplyDeltaEntityCleanupCore(deltaSnapshot, zoneId);

        public bool RestoreProgressionSnapshot(Entity character, SandboxProgressionSnapshot snapshot)
            => DebugEventBridge.RestoreProgressionSnapshotCore(character, snapshot);

        public void ValidateDeltaAfterRestore(SandboxDeltaSnapshot? deltaSnapshot, string zoneId)
            => DebugEventBridge.ValidateDeltaAfterRestoreCore(deltaSnapshot, zoneId);
    }
}
