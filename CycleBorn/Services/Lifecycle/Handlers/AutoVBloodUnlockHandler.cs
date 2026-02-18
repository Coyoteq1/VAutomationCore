using System;
using Unity.Entities;
using VAuto.Core.Lifecycle;

namespace VAuto.Core.Lifecycle.Handlers
{
    /// <summary>
    /// Result enum for VBlood unlock operations.
    /// </summary>
    public enum UnlockResult
    {
        Success,
        AlreadyUnlocked,
        Failed,
        ConditionsNotMet
    }

    /// <summary>
    /// Handles automatic VBlood boss unlocks.
    /// Implements existing LifecycleActionHandler interface.
    /// </summary>
    public class AutoVBloodUnlockHandler : LifecycleActionHandler
    {
        private const string LogSource = "AutoVBloodUnlockHandler";
        
        /// <summary>
        /// Cooldown in seconds between unlock operations.
        /// </summary>
        public float CooldownSeconds { get; set; } = 60f;
        
        /// <summary>
        /// Whether to force unlock regardless of conditions.
        /// </summary>
        public bool ForceUnlockOverride { get; set; } = false;
        
        /// <summary>
        /// Priority for unlock requests (lower = higher priority).
        /// </summary>
        public int UnlockPriority { get; set; } = 0;
        
        private float _lastUnlockTime;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public AutoVBloodUnlockHandler()
        {
        }

        /// <summary>
        /// Executes the VBlood unlock action for the given context.
        /// </summary>
        /// <param name="action">The lifecycle action containing unlock parameters.</param>
        /// <param name="context">The lifecycle context containing player entity information.</param>
        /// <returns>True if unlock was successful.</returns>
        public bool Execute(LifecycleAction action, LifecycleContext context)
        {
            if (!string.Equals(action.Type, "vbloodunlock", StringComparison.OrdinalIgnoreCase))
            {
                VLifecycle.Plugin.Log?.LogDebug($"[{LogSource}] Ignoring action type: {action.Type}");
                return false;
            }

            var em = VAutomationCore.Core.UnifiedCore.EntityManager;
            var user = context.UserEntity;

            if (user == Entity.Null)
            {
                VLifecycle.Plugin.Log?.LogWarning($"[{LogSource}] User entity is null");
                return false;
            }

            try
            {
                // Check cooldown
                if (CooldownSeconds > 0 && (float)(DateTime.UtcNow - _lastUnlockTime).TotalSeconds < CooldownSeconds)
                {
                    VLifecycle.Plugin.Log?.LogDebug($"[{LogSource}] Unlock cooldown active");
                    return false;
                }

                // Check conditions or use force override
                if (!ForceUnlockOverride && !AreUnlockConditionsMet(user, em))
                {
                    VLifecycle.Plugin.Log?.LogDebug($"[{LogSource}] Unlock conditions not met");
                    return false;
                }

                // Perform unlock
                var result = UnlockVBloods(user, em);
                
                if (result == UnlockResult.Success)
                {
                    _lastUnlockTime = DateTime.UtcNow;
                    VLifecycle.Plugin.Log?.LogInfo($"[{LogSource}] ✅ VBlood unlock completed successfully");
                }
                
                return result == UnlockResult.Success;
            }
            catch (Exception ex)
            {
                VLifecycle.Plugin.Log?.LogError($"[{LogSource}] Exception: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Checks if unlock conditions are met for the player.
        /// </summary>
        private bool AreUnlockConditionsMet(Entity user, EntityManager em)
        {
            // Check if player is in valid zone for unlocks
            // This is a placeholder - actual implementation would check zone membership
            return true;
        }

        /// <summary>
        /// Unlocks VBlood content for the player using SandboxUnlockUtility.
        /// </summary>
        private UnlockResult UnlockVBloods(Entity user, EntityManager em)
        {
            try
            {
                // Route unlock to DebugEventBridge for proper backup/restore support
                VAuto.Core.Services.DebugEventBridge.OnPlayerIsInZone(user);
                
                VLifecycle.Plugin.Log?.LogInfo($"[{LogSource}] VBlood unlocks applied via DebugEventBridge");
                return UnlockResult.Success;
            }
            catch (Exception ex)
            {
                VLifecycle.Plugin.Log?.LogError($"[{LogSource}] Unlock failed: {ex.Message}");
                return UnlockResult.Failed;
            }
        }

        /// <summary>
        /// Creates a VBlood unlock lifecycle action.
        /// </summary>
        public static LifecycleAction CreateVBloodUnlockAction(bool forceOverride = false, int priority = 0)
        {
            return new LifecycleAction
            {
                Type = "VBloodUnlock",
                ConfigId = forceOverride ? "force" : null
            };
        }
    }
}
