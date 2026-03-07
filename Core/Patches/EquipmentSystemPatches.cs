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
    /// Harmony patches for equipment and inventory systems.
    /// Minimal version that follows existing working patterns.
    /// </summary>
    [HarmonyPatch]
    internal static class EquipmentSystemPatches
    {
        /// <summary>
        /// Patch ReactToInventoryChangedSystem to track equipment changes for kit snapshots.
        /// This follows the exact same pattern as the existing InventoryPatch.
        /// </summary>
        [HarmonyPatch(typeof(ReactToInventoryChangedSystem), nameof(ReactToInventoryChangedSystem.OnUpdate))]
        [HarmonyPostfix]
        private static void ReactToInventoryChangedSystemPostfix(ReactToInventoryChangedSystem __instance)
        {
            try
            {
                // Reuse the current ECS inventory query exposed by this system.
                var query = __instance.__query_2096870026_0.ToEntityArray(Allocator.Temp);
                
                foreach (var entity in query)
                {
                    if (!entity.Has<InventoryChangedEvent>()) continue;
                    var evt = entity.Read<InventoryChangedEvent>();

                    // Only process player character changes
                    if (evt.Owner.Has<PlayerCharacter>())
                    {
                        var playerEntity = evt.Owner;
                        
                        // Handle equipment changes for kit snapshots
                        HandleEquipmentChangeForKits(playerEntity, entity);
                        
                        // Update ability slots when equipment changes
                        AbilitySlotUIService.RefreshAbilitySlots(playerEntity);
                    }
                }
                
                query.Dispose();
            }
            catch (Exception e)
            {
                UnifiedCore.LogError($"Error in EquipmentSystemPatches: {e.Message}");
            }
        }

        /// <summary>
        /// Handles equipment changes for kit snapshot functionality.
        /// </summary>
        private static void HandleEquipmentChangeForKits(Entity playerEntity, Entity itemEntity)
        {
            try
            {
                // Simple equipment change tracking
                UnifiedCore.LogError($"Equipment change detected for player {playerEntity}");
                
                // Here you could:
                // 1. Update kit snapshots
                // 2. Trigger equipment-based ability updates  
                // 3. Sync with external inventory systems
                // 4. Emit custom events for kit management
            }
            catch (System.Exception ex)
            {
                UnifiedCore.LogError($"Error in HandleEquipmentChangeForKits: {ex.Message}");
            }
        }
    }
}
