using System;

namespace VAutomationCore.Core.Api
{
    /// <summary>
    /// Represents the outcome of an operation that can fail.
    /// </summary>
    public readonly struct OperationResult
    {
        public bool Success { get; }
        public string? ErrorMessage { get; }
        public string? ErrorCode { get; }
        public Exception? Exception { get; }

        private OperationResult(bool success, string? errorMessage, string? errorCode, Exception? exception)
        {
            Success = success;
            ErrorMessage = errorMessage;
            ErrorCode = errorCode;
            Exception = exception;
        }

        public static OperationResult Ok()
        {
            return new OperationResult(true, null, null, null);
        }

        public static OperationResult Fail(string errorMessage, string? errorCode = null, Exception? exception = null)
        {
            var message = string.IsNullOrWhiteSpace(errorMessage) ? "Operation failed." : errorMessage;
            return new OperationResult(false, message, errorCode, exception);
        }

        public static OperationResult FromException(Exception exception, string? errorCode = null)
        {
            if (exception == null)
            {
                return Fail("Unhandled exception.");
            }

            return Fail(exception.Message, errorCode, exception);
        }

        public override string ToString()
        {
            if (Success)
            {
                return "Success";
            }

            if (!string.IsNullOrWhiteSpace(ErrorCode))
            {
                return $"{ErrorCode}: {ErrorMessage}";
            }

            return ErrorMessage ?? "Failed";
        }

        public static implicit operator bool(OperationResult result)
        {
            return result.Success;
        }
    }

    /// <summary>
    /// Represents the outcome of an operation that returns a value.
    /// </summary>
    public readonly struct OperationResult<T>
    {
        public bool Success { get; }
        public T Value { get; }
        public string? ErrorMessage { get; }
        public string? ErrorCode { get; }
        public Exception? Exception { get; }

        private OperationResult(bool success, T value, string? errorMessage, string? errorCode, Exception? exception)
        {
            Success = success;
            Value = value;
            ErrorMessage = errorMessage;
            ErrorCode = errorCode;
            Exception = exception;
        }

        public static OperationResult<T> Ok(T value)
        {
            return new OperationResult<T>(true, value, null, null, null);
        }

        public static OperationResult<T> Fail(string errorMessage, string? errorCode = null, Exception? exception = null)
        {
            var message = string.IsNullOrWhiteSpace(errorMessage) ? "Operation failed." : errorMessage;
            return new OperationResult<T>(false, default!, message, errorCode, exception);
        }

        public static OperationResult<T> FromException(Exception exception, string? errorCode = null)
        {
            if (exception == null)
            {
                return Fail("Unhandled exception.");
            }

            return Fail(exception.Message, errorCode, exception);
        }

        public OperationResult ToResult()
        {
            return Success
                ? OperationResult.Ok()
                : OperationResult.Fail(ErrorMessage ?? "Operation failed.", ErrorCode, Exception);
        }

        public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<OperationResult<T>, TResult> onFailure)
        {
            if (onSuccess == null)
            {
                throw new ArgumentNullException(nameof(onSuccess));
            }

            if (onFailure == null)
            {
                throw new ArgumentNullException(nameof(onFailure));
            }

            return Success ? onSuccess(Value) : onFailure(this);
        }

        public override string ToString()
        {
            return ToResult().ToString();
        }

        public static implicit operator bool(OperationResult<T> result)
        {
            return result.Success;
        }
    }
}
