using Unity.Entities;

namespace VAuto.Core.Services
{
    public interface IProgressionRestoreService
    {
        void TryApplyDeltaEntityCleanup(SandboxDeltaSnapshot? deltaSnapshot, string zoneId);
        bool RestoreProgressionSnapshot(Entity character, SandboxProgressionSnapshot snapshot);
        void ValidateDeltaAfterRestore(SandboxDeltaSnapshot? deltaSnapshot, string zoneId);
    }
}
