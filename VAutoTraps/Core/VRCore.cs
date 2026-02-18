using Unity.Entities;
using VAutomationCore.Core;

namespace VAuto.Core
{
    /// <summary>
    /// Compatibility shim for legacy modules expecting VRCore.
    /// </summary>
    public static class VRCore
    {
        public static World ServerWorld => UnifiedCore.Server;
        public static EntityManager EntityManager => UnifiedCore.EntityManager;
    }
}
