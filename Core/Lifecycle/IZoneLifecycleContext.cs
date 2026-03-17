using Unity.Entities;

namespace VAutomationCore.Core.Lifecycle
{
    /// <summary>
    /// Shared context passed to zone lifecycle steps.
    /// </summary>
    public interface IZoneLifecycleContext
    {
        Entity Player { get; }
        string ZoneId { get; }
        EntityManager EntityManager { get; }
    }
}
