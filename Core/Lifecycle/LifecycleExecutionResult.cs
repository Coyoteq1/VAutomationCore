namespace VAutomationCore.Core.Lifecycle
{
    public readonly struct LifecycleExecutionResult
    {
        public bool Success { get; }
        public LifecycleExecutionFailureCode FailureCode { get; }
        public string Message { get; }

        private LifecycleExecutionResult(bool success, LifecycleExecutionFailureCode failureCode, string message)
        {
            Success = success;
            FailureCode = failureCode;
            Message = message ?? string.Empty;
        }

        public static LifecycleExecutionResult Ok(string message = "")
        {
            return new LifecycleExecutionResult(true, LifecycleExecutionFailureCode.None, message);
        }

        public static LifecycleExecutionResult Fail(LifecycleExecutionFailureCode code, string message)
        {
            return new LifecycleExecutionResult(false, code, message);
        }
    }
}
