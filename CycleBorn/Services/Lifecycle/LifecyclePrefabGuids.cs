using Stunlock.Core;

namespace VAuto.Core.Lifecycle
{
    /// <summary>
    /// Lifecycle-owned prefab GUID constants.
    /// Keep only values needed by lifecycle flows (snapshot/enter/exit/unlock/workstation hooks).
    /// </summary>
    public static class LifecyclePrefabGuids
    {
        // Player ability/inventory lifecycle
        public static readonly PrefabGUID AbilityGroupSlot = new(-633717863);
        public static readonly PrefabGUID ExternalInventory = new(1183666186);

        // Player state/buff lifecycle
        public static readonly PrefabGUID ImprisonedBuff = new(1603329680);

        // Territory/workstation lifecycle hooks
        public static readonly PrefabGUID TMHiddenObject = new(162560418);
        public static readonly PrefabGUID TMWorkstationWaypointCastle = new(-148794951);
    }
}
