using System;
using System.Threading;
using System.Threading.Tasks;
using VAutomationCore.Core.Logging;

namespace VAutomationCore.Core.Api
{
    /// <summary>
    /// Safe execution helpers with consistent error handling and retry support.
    /// </summary>
    public static class CoreExecution
    {
        public static OperationResult Run(Action action, string operationName = "operation", CoreLogger? logger = null)
        {
            if (action == null)
            {
                return OperationResult.Fail("Action cannot be null.", "invalid_action");
            }

            try
            {
                action();
                return OperationResult.Ok();
            }
            catch (Exception ex)
            {
                logger?.Exception(ex, operationName);
                return OperationResult.FromException(ex, "execution_failed");
            }
        }

        public static OperationResult<T> Run<T>(Func<T> action, string operationName = "operation", CoreLogger? logger = null)
        {
            if (action == null)
            {
                return OperationResult<T>.Fail("Action cannot be null.", "invalid_action");
            }

            try
            {
                return OperationResult<T>.Ok(action());
            }
            catch (Exception ex)
            {
                logger?.Exception(ex, operationName);
                return OperationResult<T>.FromException(ex, "execution_failed");
            }
        }

        public static OperationResult RunWithRetry(
            Action action,
            RetryPolicy? retryPolicy = null,
            string operationName = "operation",
            CoreLogger? logger = null)
        {
            if (action == null)
            {
                return OperationResult.Fail("Action cannot be null.", "invalid_action");
            }

            var policy = retryPolicy ?? RetryPolicy.Default;
            policy.Validate();

            Exception? lastException = null;
            for (var attempt = 1; attempt <= policy.MaxAttempts; attempt++)
            {
                try
                {
                    action();
                    return OperationResult.Ok();
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    var retryable = attempt < policy.MaxAttempts &&
                                    (policy.ShouldRetryException == null || policy.ShouldRetryException(ex));

                    if (!retryable)
                    {
                        break;
                    }

                    logger?.Warning($"Retry {attempt}/{policy.MaxAttempts} failed for '{operationName}': {ex.Message}");
                    var delay = policy.GetDelay(attempt + 1);
                    if (delay > TimeSpan.Zero)
                    {
                        Thread.Sleep(delay);
                    }
                }
            }

            if (lastException == null)
            {
                return OperationResult.Fail("Operation failed.", "execution_failed");
            }

            logger?.Exception(lastException, operationName);
            return OperationResult.FromException(lastException, "execution_failed");
        }

        public static OperationResult<T> RunWithRetry<T>(
            Func<T> action,
            RetryPolicy? retryPolicy = null,
            string operationName = "operation",
            CoreLogger? logger = null)
        {
            if (action == null)
            {
                return OperationResult<T>.Fail("Action cannot be null.", "invalid_action");
            }

            var policy = retryPolicy ?? RetryPolicy.Default;
            policy.Validate();

            Exception? lastException = null;
            for (var attempt = 1; attempt <= policy.MaxAttempts; attempt++)
            {
                try
                {
                    return OperationResult<T>.Ok(action());
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    var retryable = attempt < policy.MaxAttempts &&
                                    (policy.ShouldRetryException == null || policy.ShouldRetryException(ex));

                    if (!retryable)
                    {
                        break;
                    }

                    logger?.Warning($"Retry {attempt}/{policy.MaxAttempts} failed for '{operationName}': {ex.Message}");
                    var delay = policy.GetDelay(attempt + 1);
                    if (delay > TimeSpan.Zero)
                    {
                        Thread.Sleep(delay);
                    }
                }
            }

            if (lastException == null)
            {
                return OperationResult<T>.Fail("Operation failed.", "execution_failed");
            }

            logger?.Exception(lastException, operationName);
            return OperationResult<T>.FromException(lastException, "execution_failed");
        }

        public static async Task<OperationResult> RunAsync(
            Func<CancellationToken, Task> action,
            CancellationToken cancellationToken = default,
            string operationName = "operation",
            CoreLogger? logger = null)
        {
            if (action == null)
            {
                return OperationResult.Fail("Action cannot be null.", "invalid_action");
            }

            try
            {
                await action(cancellationToken).ConfigureAwait(false);
                return OperationResult.Ok();
            }
            catch (Exception ex)
            {
                logger?.Exception(ex, operationName);
                return OperationResult.FromException(ex, "execution_failed");
            }
        }

        public static async Task<OperationResult<T>> RunAsync<T>(
            Func<CancellationToken, Task<T>> action,
            CancellationToken cancellationToken = default,
            string operationName = "operation",
            CoreLogger? logger = null)
        {
            if (action == null)
            {
                return OperationResult<T>.Fail("Action cannot be null.", "invalid_action");
            }

            try
            {
                var value = await action(cancellationToken).ConfigureAwait(false);
                return OperationResult<T>.Ok(value);
            }
            catch (Exception ex)
            {
                logger?.Exception(ex, operationName);
                return OperationResult<T>.FromException(ex, "execution_failed");
            }
        }
    }
}
