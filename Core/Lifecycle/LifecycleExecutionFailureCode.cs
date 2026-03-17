namespace VAutomationCore.Core.Lifecycle
{
    public enum LifecycleExecutionFailureCode
    {
        None = 0,
        MissingFlow = 1,
        ValidationFailed = 2,
        DependencyUnavailable = 3,
        RuntimeActionFailure = 4,
        Unknown = 100
    }
}
