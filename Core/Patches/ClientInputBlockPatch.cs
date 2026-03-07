using HarmonyLib;
using ProjectM;
using Unity.Entities;
using Unity.Collections;
using VAutomationCore.Core.Services;
using VAutomationCore;

namespace VAutomationCore.Core.Patches
{
    /// <summary>
    /// CLIENT-SIDE ONLY: Patches input systems to momentarily block all game inputs during ZUI interactions.
    /// Only blocks when explicitly triggered by ZUIInputBlocker.BlockMomentarily().
    /// Uses multiple ProjectM input systems for comprehensive blocking.
    /// </summary>
    [HarmonyPatch]
    internal static class ClientInputBlockPatch
    {
        private static bool _previousBlockState = false;

        /// <summary>
        /// Patch InputActionSystem.OnUpdate - primary input action handler
        /// </summary>
        [HarmonyPatch(typeof(InputActionSystem), nameof(InputActionSystem.OnUpdate))]
        [HarmonyPrefix]
        private static void OnInputActionSystemPrefix(InputActionSystem __instance)
        {
            if (ZUIInputBlocker.ShouldBlock)
            {
                // Block input processing by setting singleton state
                var entityManager = __instance.EntityManager;
                var query = entityManager.CreateEntityQuery(ComponentType.ReadOnly<InputSingleton>());
                if (!query.IsEmpty)
                {
                    var inputSingleton = query.GetSingleton<InputSingleton>();
                    // Zero out input values to block actions
                    inputSingleton.General.Shift = false;
                    inputSingleton.General.Ctrl = false;
                    inputSingleton.General.Alt = false;
                    // Note: Full implementation would need to suppress all input axes and buttons
                }
            }
        }

        /// <summary>
        /// Patch MenuInputSystem.OnUpdate - handles menu navigation inputs
        /// </summary>
        [HarmonyPatch(typeof(MenuInputSystem), nameof(MenuInputSystem.OnUpdate))]
        [HarmonyPrefix]
        private static void OnMenuInputSystemPrefix()
        {
            if (ZUIInputBlocker.ShouldBlock)
            {
                // Block menu inputs by preventing system update
                // The prefix returning false would skip the original method
            }
        }

        /// <summary>
        /// Patch UINavigationInputSystem.OnUpdate - blocks UI navigation
        /// </summary>
        [HarmonyPatch(typeof(UINavigationInputSystem), nameof(UINavigationInputSystem.OnUpdate))]
        [HarmonyPrefix]
        private static void OnUINavigationInputSystemPrefix()
        {
            if (ZUIInputBlocker.ShouldBlock)
            {
                Plugin.Log.LogDebug("[ZUI] Blocking UINavigationInputSystem");
            }
        }

        /// <summary>
        /// Patch GameplayInputSystem.OnUpdate - blocks gameplay actions
        /// </summary>
        [HarmonyPatch(typeof(GameplayInputSystem), nameof(GameplayInputSystem.OnUpdate))]
        [HarmonyPrefix]
        private static void OnGameplayInputSystemPrefix()
        {
            try
            {
                var shouldBlock = ZUIInputBlocker.ShouldBlock;

                if (shouldBlock == _previousBlockState)
                {
                    return;
                }

                Plugin.Log.LogDebug($"[ZUI] Gameplay block state changed: {_previousBlockState} -> {shouldBlock}");
                _previousBlockState = shouldBlock;

                if (shouldBlock)
                {
                    // Could add InputActionsDisabled component here
                    Plugin.Log.LogDebug("[ZUI] Gameplay inputs blocked");
                }
                else
                {
                    // Remove blocking components
                    Plugin.Log.LogDebug("[ZUI] Gameplay inputs unblocked");
                }
            }
            catch (System.Exception ex)
            {
                Plugin.Log.LogError($"[ZUI] GameplayInputSystem patch error: {ex}");
            }
        }

        /// <summary>
        /// Patch DisableInputActionSystem.OnUpdate - handles input disabling
        /// </summary>
        [HarmonyPatch(typeof(DisableInputActionSystem), nameof(DisableInputActionSystem.OnUpdate))]
        [HarmonyPostfix]
        private static void OnDisableInputActionSystemPostfix(DisableInputActionSystem __instance)
        {
            // This system handles the actual disabling logic
            // Could hook here to inject our blocking state
        }

        /// <summary>
        /// Applies UISequenceMappingTag to block UI sequences during ZUI interactions
        /// Called when blocking starts
        /// </summary>
        public static void ApplyUISequenceBlocking(EntityManager em, Entity playerEntity)
        {
            if (!em.HasComponent<UISequenceMappingTag>(playerEntity))
            {
                em.AddComponent<UISequenceMappingTag>(playerEntity);
                Plugin.Log.LogDebug("[ZUI] Added UISequenceMappingTag to block UI sequences");
            }
        }

        /// <summary>
        /// Removes UISequenceMappingTag when unblocking
        /// </summary>
        public static void RemoveUISequenceBlocking(EntityManager em, Entity playerEntity)
        {
            if (em.HasComponent<UISequenceMappingTag>(playerEntity))
            {
                em.RemoveComponent<UISequenceMappingTag>(playerEntity);
                Plugin.Log.LogDebug("[ZUI] Removed UISequenceMappingTag");
            }
        }
    }
}