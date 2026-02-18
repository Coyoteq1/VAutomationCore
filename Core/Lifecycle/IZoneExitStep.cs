namespace VAutomationCore.Core.Lifecycle
{
    /// <summary>
    /// Ordered step executed during zone exit.
    /// </summary>
    public interface IZoneExitStep
    {
        string Name { get; }
        int Order { get; }
        void Execute(IZoneLifecycleContext context);
    }
}
