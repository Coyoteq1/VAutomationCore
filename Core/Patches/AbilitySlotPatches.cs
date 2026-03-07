using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using Stunlock.Network;
using Unity.Collections;
using Unity.Entities;
using VAutomationCore.Core;
using VAutomationCore.Core.Services;
using VAutomationCore.Abstractions;

namespace VAutomationCore.Core.Patches
{
    /// <summary>
    /// Harmony patches for ability slot management systems.
    /// Minimal version that follows existing working patterns.
    /// </summary>
    [HarmonyPatch]
    internal static class AbilitySlotPatches
    {
        /// <summary>
        /// Patch AbilityRunScriptsSystem to detect ability changes and update UI.
        /// This follows the same pattern as existing patches.
        /// </summary>
        [HarmonyPatch(typeof(AbilityRunScriptsSystem), nameof(AbilityRunScriptsSystem.OnUpdate))]
        [HarmonyPostfix]
        private static void AbilityRunScriptsSystemPostfix(AbilityRunScriptsSystem __instance)
        {
            try
            {
                // Follow the existing ECS query pattern used by the active patch set.
                var castStarted = __instance._OnCastStartedQuery.ToEntityArray(Allocator.Temp);
                
                foreach (var entity in castStarted)
                {
                    if (!entity.Has<AbilityCastStartedEvent>()) continue;
                    
                    var castEvent = entity.Read<AbilityCastStartedEvent>();
                    var caster = castEvent.Caster;
                    
                    // Only process player characters
                    if (caster.Has<PlayerCharacter>())
                    {
                        // Refresh ability slots when player casts abilities
                        AbilitySlotUIService.RefreshAbilitySlots(caster);
                    }
                }
                
                castStarted.Dispose();
            }
            catch (Exception e)
            {
                UnifiedCore.LogError($"Error in AbilitySlotPatches: {e.Message}");
            }
        }
    }
}
