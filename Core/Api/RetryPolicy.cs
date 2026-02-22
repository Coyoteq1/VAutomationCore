using System;

namespace VAutomationCore.Core.Api
{
    /// <summary>
    /// Defines retry behavior for transient-failure operations.
    /// </summary>
    public sealed class RetryPolicy
    {
        /// <summary>
        /// Default policy: 3 attempts, exponential backoff, jitter enabled.
        /// </summary>
        public static RetryPolicy Default => new();

        /// <summary>
        /// Total attempts including the first attempt.
        /// </summary>
        public int MaxAttempts { get; set; } = 3;

        /// <summary>
        /// Delay used for the second attempt before backoff is applied.
        /// </summary>
        public TimeSpan InitialDelay { get; set; } = TimeSpan.FromMilliseconds(150);

        /// <summary>
        /// Backoff multiplier between attempts.
        /// </summary>
        public double BackoffMultiplier { get; set; } = 2d;

        /// <summary>
        /// Upper bound for computed delay.
        /// </summary>
        public TimeSpan MaxDelay { get; set; } = TimeSpan.FromSeconds(2);

        /// <summary>
        /// Adds up to 20% random jitter to reduce thundering herd effects.
        /// </summary>
        public bool UseJitter { get; set; } = true;

        /// <summary>
        /// Optional filter controlling which exceptions are retryable.
        /// </summary>
        public Func<Exception, bool>? ShouldRetryException { get; set; }

        public void Validate()
        {
            if (MaxAttempts < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(MaxAttempts), "MaxAttempts must be at least 1.");
            }

            if (InitialDelay < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(InitialDelay), "InitialDelay cannot be negative.");
            }

            if (BackoffMultiplier < 1d)
            {
                throw new ArgumentOutOfRangeException(nameof(BackoffMultiplier), "BackoffMultiplier must be >= 1.");
            }

            if (MaxDelay < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(MaxDelay), "MaxDelay cannot be negative.");
            }
        }

        internal TimeSpan GetDelay(int attempt)
        {
            if (attempt <= 1 || InitialDelay == TimeSpan.Zero)
            {
                return TimeSpan.Zero;
            }

            var exponent = attempt - 2;
            var rawMs = InitialDelay.TotalMilliseconds * Math.Pow(BackoffMultiplier, exponent);
            var clampedMs = Math.Min(rawMs, MaxDelay.TotalMilliseconds);

            if (UseJitter && clampedMs > 0d)
            {
                // Small random spread keeps retries from synchronizing.
                var jitterMs = Random.Shared.NextDouble() * (clampedMs * 0.2d);
                clampedMs = Math.Max(0d, clampedMs - jitterMs);
            }

            return TimeSpan.FromMilliseconds(clampedMs);
        }
    }
}
