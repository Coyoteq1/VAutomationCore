using System;
using System.Threading;

namespace VAutomationCore.Core.TrapLifecycle
{
    /// <summary>
    /// Shared resolver used by lifecycle consumers to evaluate trap policy overrides.
    /// </summary>
    public static class TrapPolicyResolver
    {
        private static readonly object _lock = new object();
        private static ITrapLifecyclePolicy _policy;
        private static string _owner;

        public static void RegisterPolicy(ITrapLifecyclePolicy policy, string owner = null)
        {
            if (policy == null) return;

            lock (_lock)
            {
                _policy = policy;
                _owner = string.IsNullOrWhiteSpace(owner) ? "unknown" : owner;
            }
        }

        public static void UnregisterPolicy(string owner = null)
        {
            lock (_lock)
            {
                if (_policy == null)
                {
                    return;
                }

                if (!string.IsNullOrWhiteSpace(owner) &&
                    !string.Equals(_owner, owner, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                _policy = null;
                _owner = null;
            }
        }

        public static bool AreOverridesEnabled()
        {
            // Use volatile read to avoid lock overhead on every check
            var policy = Volatile.Read(ref _policy);
            return policy != null && policy.IsEnabled;
        }

        public static TrapLifecycleDecision EvaluateEnter(TrapLifecycleContext ctx)
        {
            var policy = GetPolicy();
            if (policy == null || !policy.IsEnabled)
            {
                return TrapLifecycleDecision.None("Trap policy not enabled");
            }

            try
            {
                return policy.OnBeforeLifecycleEnter(ctx);
            }
            catch (Exception ex)
            {
                return TrapLifecycleDecision.None($"Trap policy enter error: {ex.Message}");
            }
        }

        public static TrapLifecycleDecision EvaluateExit(TrapLifecycleContext ctx)
        {
            var policy = GetPolicy();
            if (policy == null || !policy.IsEnabled)
            {
                return TrapLifecycleDecision.None("Trap policy not enabled");
            }

            try
            {
                return policy.OnBeforeLifecycleExit(ctx);
            }
            catch (Exception ex)
            {
                return TrapLifecycleDecision.None($"Trap policy exit error: {ex.Message}");
            }
        }

        private static ITrapLifecyclePolicy GetPolicy()
        {
            // Use volatile read for thread-safe access without lock overhead
            return Volatile.Read(ref _policy);
        }
    }
}
