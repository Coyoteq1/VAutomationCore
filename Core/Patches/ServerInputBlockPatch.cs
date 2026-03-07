using HarmonyLib;
using ProjectM;
using Stunlock.Core;
using Unity.Entities;
using Unity.Collections;
using VAutomationCore.Core.Services;
using VAutomationCore;

namespace VAutomationCore.Core.Patches
{
    /// <summary>
    /// SERVER-SIDE: Patches ability casting systems to block actions when ZUIInputBlocker is active.
    /// This provides server-side enforcement as a backup to client-side input blocking.
    /// Note: Server cannot fully block client inputs but can prevent server-processed actions.
    /// </summary>
    [HarmonyPatch]
    internal static class ServerInputBlockPatch
    {
        private static bool _wasBlocking = false;
        
        // PrefabGUID for a stun/freeze buff to apply when blocking
        // This would need to be set to an actual stun buff in the game
        private static readonly PrefabGUID StunBuffGuid = new PrefabGUID(-1751363526); // Example: freeze stun
        
        [HarmonyPatch(typeof(AbilityRunScriptsSystem), nameof(AbilityRunScriptsSystem.OnUpdate))]
        [HarmonyPrefix]
        private static void OnAbilityCastPrefix(AbilityRunScriptsSystem __instance)
        {
            try
            {
                // Quick exit if not blocking - no need to process
                if (!ZUIInputBlocker.ShouldBlock)
                {
                    _wasBlocking = false;
                    return;
                }
                
                // State changed to blocking
                if (!_wasBlocking)
                {
                    _wasBlocking = true;
                    Plugin.Log.LogDebug("[ZUI] Server blocking active - intercepting abilities");
                }
                
                // Get all entities with PlayerCharacter that are trying to cast abilities
                var entityManager = __instance.EntityManager;
                
                // Query for entities that have ability casting components
                // This is a simplified approach - actual implementation may need
                // to intercept the ability cast more directly
                var playerQuery = entityManager.CreateEntityQuery(
                    ComponentType.ReadOnly<PlayerCharacter>()
                );
                
                if (playerQuery.IsEmpty)
                {
                    return;
                }
                
                var playerEntities = playerQuery.ToEntityArray(Allocator.Temp);
                
                foreach (var playerEntity in playerEntities)
                {
                    // Check if player has active cast and is trying to use ability
                    if (entityManager.HasComponent<CastAbility>(playerEntity))
                    {
                        var castAbility = entityManager.GetComponentData<CastAbility>(playerEntity);
                        

                        // This prevents the ability from executing on server
                        Plu                        // If there's an active cast, cancel it by removing the componentgin.Log.LogDebug($"[ZUI] Server cancelling ability cast for player entity");
                        entityManager.RemoveComponent<CastAbility>(playerEntity);
                    }
                }
                
                playerEntities.Dispose();
            }
            catch (System.Exception ex)
            {
                Plugin.Log.LogError($"[ZUI] Server input blocking error: {ex}");
            }
        }
        
        /// <summary>
        /// Alternative approach: Patch system that processes input commands.
        /// This intercepts before abilities are even queued.
        /// Note: ServerBootstrapSystem.OnUpdate is handled by ServerBootstrapSystemPatch.cs
        /// </summary>
    }
}