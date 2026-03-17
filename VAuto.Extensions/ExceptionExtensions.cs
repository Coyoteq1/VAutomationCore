using System;
using System.Collections.Generic;
using System.Linq;

namespace VAuto.Extensions
{
    /// <summary>
    /// Generic Exception extension methods
    /// </summary>
    public static class ExceptionExtensions
    {
        /// <summary>
        /// Get full exception message with inner exceptions
        /// </summary>
        public static string GetFullMessage(this Exception ex)
        {
            var messages = new List<string>();
            var current = ex;

            while (current != null)
            {
                messages.Add(current.Message);
                current = current.InnerException;
            }

            return string.Join(" -> ", messages);
        }

        /// <summary>
        /// Get full stack trace including inner exceptions
        /// </summary>
        public static string GetFullStackTrace(this Exception ex)
        {
            var traces = new List<string>();
            var current = ex;

            while (current != null)
            {
                if (!string.IsNullOrEmpty(current.StackTrace))
                    traces.Add(current.StackTrace);
                current = current.InnerException;
            }

            return string.Join(Environment.NewLine + "--- Inner Exception ---" + Environment.NewLine, traces);
        }

        /// <summary>
        /// Get all inner exceptions
        /// </summary>
        public static IEnumerable<Exception> GetInnerExceptions(this Exception ex)
        {
            var current = ex.InnerException;
            while (current != null)
            {
                yield return current;
                current = current.InnerException;
            }
        }

        /// <summary>
        /// Check if exception or any inner exception is of specific type
        /// </summary>
        public static bool Contains<T>(this Exception ex) where T : Exception
        {
            if (ex is T)
                return true;

            foreach (var inner in ex.GetInnerExceptions())
            {
                if (inner is T)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Get deepest inner exception
        /// </summary>
        public static Exception GetInnerMost(this Exception ex)
        {
            var current = ex;
            while (current.InnerException != null)
            {
                current = current.InnerException;
            }
            return current;
        }

        /// <summary>
        /// Format exception as user-friendly string
        /// </summary>
        public static string ToUserString(this Exception ex)
        {
            return $"[{ex.GetType().Name}] {ex.GetFullMessage()}";
        }

        /// <summary>
        /// Log-friendly format
        /// </summary>
        public static string ToLogString(this Exception ex, bool includeStackTrace = false)
        {
            var result = $"Exception: {ex.GetType().Name}{Environment.NewLine}" +
                        $"Message: {ex.Message}{Environment.NewLine}" +
                        $"Source: {ex.Source}";

            if (includeStackTrace && !string.IsNullOrEmpty(ex.StackTrace))
            {
                result += $"{Environment.NewLine}StackTrace: {ex.StackTrace}";
            }

            var inners = ex.GetInnerExceptions().ToList();
            if (inners.Any())
            {
                result += $"{Environment.NewLine}Inner Exceptions:";
                foreach (var inner in inners)
                {
                    result += $"{Environment.NewLine}  - [{inner.GetType().Name}] {inner.Message}";
                }
            }

            return result;
        }

        /// <summary>
        /// Try execute action and return result or exception
        /// </summary>
        public static Exception? TryExecute(Action action)
        {
            try
            {
                action();
                return null;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        /// <summary>
        /// Try execute func and return result or exception
        /// </summary>
        public static Exception? TryExecute<T>(Func<T> func, out T? result)
        {
            try
            {
                result = func();
                return null;
            }
            catch (Exception ex)
            {
                result = default;
                return ex;
            }
        }
    }
}