using VAutomationCore.Core.Logging;

namespace VAutomationCore.Core.Services
{
    /// <summary>
    /// Centralized input blocking control for ZUI interactions.
    /// Thread-safe static class to coordinate input blocking between client and server approaches.
    /// </summary>
    public static class ZUIInputBlocker
    {
        private static int _shouldBlock = 0;
        private static readonly CoreLogger _logger = new CoreLogger("ZUIInputBlocker");
        
        /// <summary>
        /// Whether player inputs should currently be blocked.
        /// </summary>
        public static bool ShouldBlock => Interlocked.CompareExchange(ref _shouldBlock, 1, 1) == 1;
        
        /// <summary>
        /// Momentarily block all game inputs. Call this when ZUI interaction starts.
        /// Uses a brief pulse - caller should call UnblockAfterDelay() if persistent blocking needed.
        /// </summary>
        public static void BlockMomentarily()
        {
            Interlocked.Exchange(ref _shouldBlock, 1);
            _logger.LogDebug($"[ZUIInputBlocker] BlockMomentarily called - blocking: {ShouldBlock}");
        }
        
        /// <summary>
        /// Unblock all game inputs. Call this when ZUI interaction ends.
        /// </summary>
        public static void Unblock()
        {
            Interlocked.Exchange(ref _shouldBlock, 0);
            _logger.LogDebug($"[ZUIInputBlocker] Unblock called - blocking: {ShouldBlock}");
        }
        
        /// <summary>
        /// Force set blocking state (for advanced control).
        /// </summary>
        public static void SetBlocking(bool block)
        {
            Interlocked.Exchange(ref _shouldBlock, block ? 1 : 0);
            _logger.LogDebug($"[ZUIInputBlocker] SetBlocking({block}) - blocking: {ShouldBlock}");
        }
    }
}