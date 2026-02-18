namespace VAutomationCore.Core.Lifecycle
{
    /// <summary>
    /// Ordered step executed during zone enter.
    /// </summary>
    public interface IZoneEnterStep
    {
        string Name { get; }
        int Order { get; }
        void Execute(IZoneLifecycleContext context);
    }
}
