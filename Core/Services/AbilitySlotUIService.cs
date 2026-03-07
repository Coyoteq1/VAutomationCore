using System.Collections.Generic;
using ProjectM;
using ProjectM.Network;
using Stunlock.Core;
using Unity.Collections;
using Unity.Entities;
using VAutomationCore.Core;
using VAutomationCore.Core.Data.DataType;
using VAutomationCore.Abstractions;

namespace VAutomationCore.Core.Services
{
    /// <summary>
    /// Service for managing and updating ability slot UI.
    /// Provides methods to refresh, update, and synchronize ability slots with the game's UI system.
    /// </summary>
    public static class AbilitySlotUIService
    {
        /// <summary>
        /// Callback delegate for ability slot UI events.
        /// </summary>
        public delegate void AbilitySlotUIEvent(Entity playerEntity, int slotIndex, PrefabGUID abilityPrefab);

        /// <summary>
        /// Event fired when ability slots are updated.
        /// </summary>
        public static event AbilitySlotUIEvent OnAbilitySlotUpdated;

        /// <summary>
        /// Event fired when ability slots are refreshed.
        /// </summary>
        public static event AbilitySlotUIEvent OnAbilitySlotsRefreshed;

        /// <summary>
        /// Updates a specific ability slot for a player using the Abilities abstraction.
        /// </summary>
        /// <param name="playerEntity">The player entity</param>
        /// <param name="slotIndex">The slot index to update</param>
        /// <param name="abilityPrefab">The ability prefab to place in the slot</param>
        /// <returns>True if the slot was updated successfully, false otherwise</returns>
        public static bool UpdateAbilitySlot(Entity playerEntity, int slotIndex, PrefabGUID abilityPrefab)
        {
            try
            {
                var em = UnifiedCore.EntityManager;
                if (!em.Exists(playerEntity) || abilityPrefab == PrefabGUID.Empty || slotIndex < 0)
                {
                    return false;
                }

                // Check if player has the ability
                if (!Abilities.HasAbility(playerEntity, abilityPrefab))
                {
                    // Grant the ability if they don't have it
                    if (!Abilities.GrantAbility(playerEntity, playerEntity, abilityPrefab))
                    {
                        return false;
                    }
                }

                // For now, we'll just ensure the ability is granted
                // In a full implementation, this would manage specific slot assignments
                                
                // Fire event
                OnAbilitySlotUpdated?.Invoke(playerEntity, slotIndex, abilityPrefab);
                
                return true;
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Refreshes all ability slots for a player by re-syncing with their current abilities.
        /// </summary>
        /// <param name="playerEntity">The player entity</param>
        /// <returns>True if slots were refreshed successfully, false otherwise</returns>
        public static bool RefreshAbilitySlots(Entity playerEntity)
        {
            try
            {
                var em = UnifiedCore.EntityManager;
                if (!em.Exists(playerEntity))
                {
                    return false;
                }

                // Get all current abilities
                var currentAbilities = Abilities.GetAbilities(playerEntity);

                // For now, we'll just ensure all abilities are properly granted
                // In a full implementation, this would rebuild the slot assignments
                foreach (var abilityPrefab in currentAbilities)
                {
                    Abilities.GrantAbility(playerEntity, playerEntity, abilityPrefab);
                }

                // Fire refresh event
                OnAbilitySlotsRefreshed?.Invoke(playerEntity, -1, PrefabGUID.Empty);

                return true;
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Gets abilities for a player using the Abilities abstraction.
        /// </summary>
        /// <param name="playerEntity">The player entity</param>
        /// <returns>List of ability prefab GUIDs</returns>
        public static List<PrefabGUID> GetPlayerAbilities(Entity playerEntity)
        {
            try
            {
                return Abilities.GetAbilities(playerEntity);
            }
            catch
            {
                return new List<PrefabGUID>();
            }
        }

        /// <summary>
        /// Checks if a player has a specific ability.
        /// </summary>
        /// <param name="playerEntity">The player entity</param>
        /// <param name="abilityPrefab">The ability prefab to check</param>
        /// <returns>True if the player has the ability, false otherwise</returns>
        public static bool HasAbility(Entity playerEntity, PrefabGUID abilityPrefab)
        {
            try
            {
                return Abilities.HasAbility(playerEntity, abilityPrefab);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Grants an ability to a player.
        /// </summary>
        /// <param name="playerEntity">The player entity</param>
        /// <param name="abilityPrefab">The ability prefab to grant</param>
        /// <returns>True if the ability was granted successfully, false otherwise</returns>
        public static bool GrantAbility(Entity playerEntity, PrefabGUID abilityPrefab)
        {
            try
            {
                return Abilities.GrantAbility(playerEntity, playerEntity, abilityPrefab);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Removes an ability from a player.
        /// </summary>
        /// <param name="playerEntity">The player entity</param>
        /// <param name="abilityPrefab">The ability prefab to remove</param>
        /// <returns>True if the ability was removed successfully, false otherwise</returns>
        public static bool RemoveAbility(Entity playerEntity, PrefabGUID abilityPrefab)
        {
            try
            {
                return Abilities.RemoveAbility(playerEntity, abilityPrefab);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Clears all abilities for a player.
        /// </summary>
        /// <param name="playerEntity">The player entity</param>
        /// <returns>Number of abilities cleared</returns>
        public static int ClearAllAbilities(Entity playerEntity)
        {
            var clearedCount = 0;
            
            try
            {
                var currentAbilities = Abilities.GetAbilities(playerEntity);
                
                foreach (var abilityPrefab in currentAbilities)
                {
                    if (Abilities.RemoveAbility(playerEntity, abilityPrefab))
                    {
                        clearedCount++;
                    }
                }
            }
            catch
            {
                // Return count even on error
            }

            return clearedCount;
        }

        /// <summary>
        /// Updates ability slots using a kit configuration.
        /// </summary>
        /// <param name="playerEntity">The player entity</param>
        /// <param name="abilities">Dictionary of ability slot names to ability prefab names</param>
        /// <returns>True if abilities were updated successfully, false otherwise</returns>
        public static bool UpdateAbilitiesFromKit(Entity playerEntity, Dictionary<string, string> abilities)
        {
            try
            {
                var successCount = 0;
                
                foreach (var (slotName, abilityName) in abilities)
                {
                    if (!string.IsNullOrEmpty(abilityName))
                    {
                        var prefabGuid = Prefabs.GetPrefabGuid(abilityName);
                        if (prefabGuid != PrefabGUID.Empty)
                        {
                            if (GrantAbility(playerEntity, prefabGuid))
                            {
                                successCount++;
                            }
                        }
                    }
                }

                return successCount > 0;
            }
            catch
            {
                return false;
            }
        }
    }
}
