using System;
using System.Collections.Generic;
using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using ProjectM.Shared;
using Stunlock.Core;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using VAutomationCore.Core;
using VAutomationCore.Core.Services;
using VAutomationCore.Core.Logging;

namespace VAutomationCore.Core.Patches
{
    /// <summary>
    /// Critical ECS patches for heavy modding needs.
    /// Provides hooks into core game systems that mods frequently need to modify or monitor.
    /// </summary>
    [HarmonyPatch]
    internal static class ECSModPatches
    {
        private static readonly CoreLogger Log = new("ECSModPatches");

        #region Player System Patches

        /// <summary>
        /// Patch PlayerSystem to monitor player connections and disconnections.
        /// Critical for mods that need to track player presence and handle player-specific data.
        /// </summary>
        [HarmonyPatch(typeof(PlayerSystem), nameof(PlayerSystem.OnUpdate))]
        [HarmonyPostfix]
        private static void PlayerSystemOnUpdatePostfix(PlayerSystem __instance)
        {
            try
            {
                if (!ServerReadySystem.Instance.IsServerReady) return;

                // Get newly connected players
                var connectedQuery = __instance.__OnPlayerConnectedQuery.ToEntityArray(Allocator.Temp);
                foreach (var playerEntity in connectedQuery)
                {
                    if (!playerEntity.Has<PlayerCharacter>()) continue;
                    
                    var userEntity = playerEntity.Read<PlayerCharacter>().UserEntity;
                    if (!userEntity.Has<User>()) continue;
                    
                    var user = userEntity.Read<User>();
                    Log.LogInfo($"Player connected: {user.CharacterName} (PlatformId: {user.PlatformId})");
                    
                    // Trigger player connected event for other mods
                    TriggerPlayerConnectedEvent(playerEntity, userEntity);
                }
                connectedQuery.Dispose();

                // Get disconnected players
                var disconnectedQuery = __instance.__OnPlayerDisconnectedQuery.ToEntityArray(Allocator.Temp);
                foreach (var playerEntity in disconnectedQuery)
                {
                    if (!playerEntity.Has<PlayerCharacter>()) continue;
                    
                    var userEntity = playerEntity.Read<PlayerCharacter>().UserEntity;
                    if (!userEntity.Has<User>()) continue;
                    
                    var user = userEntity.Read<User>();
                    Log.LogInfo($"Player disconnected: {user.CharacterName} (PlatformId: {user.PlatformId})");
                    
                    // Trigger player disconnected event for other mods
                    TriggerPlayerDisconnectedEvent(playerEntity, userEntity);
                }
                disconnectedQuery.Dispose();
            }
            catch (Exception ex)
            {
                Log.LogError($"Error in PlayerSystem patch: {ex.Message}");
            }
        }

        #endregion

        #region Inventory System Patches

        /// <summary>
        /// Patch InventorySystem to monitor all inventory changes.
        /// Essential for mods that need to track item movements, crafting, or inventory modifications.
        /// </summary>
        [HarmonyPatch(typeof(InventorySystem), nameof(InventorySystem.OnUpdate))]
        [HarmonyPostfix]
        private static void InventorySystemOnUpdatePostfix(InventorySystem __instance)
        {
            try
            {
                if (!ServerReadySystem.Instance.IsServerReady) return;

                var query = __instance.__InventoryChangeEventQuery.ToEntityArray(Allocator.Temp);
                foreach (var entity in query)
                {
                    if (!entity.Has<InventoryChangeEvent>()) continue;
                    
                    var evt = entity.Read<InventoryChangeEvent>();
                    var itemPrefab = evt.ItemEntity.Read<PrefabGUID>();
                    
                    Log.LogInfo($"Inventory change: {evt.Owner} - Item {itemPrefab} - Operation {evt.Operation}");
                    
                    // Trigger inventory change event for other mods
                    TriggerInventoryChangeEvent(evt.Owner, evt.ItemEntity, itemPrefab, evt.Operation);
                }
                query.Dispose();
            }
            catch (Exception ex)
            {
                Log.LogError($"Error in InventorySystem patch: {ex.Message}");
            }
        }

        #endregion

        #region Ability System Patches

        /// <summary>
        /// Patch AbilitySystem to monitor ability casts and cooldowns.
        /// Critical for mods that need to modify ability behavior, track usage, or implement custom abilities.
        /// </summary>
        [HarmonyPatch(typeof(AbilitySystem), nameof(AbilitySystem.OnUpdate))]
        [HarmonyPostfix]
        private static void AbilitySystemOnUpdatePostfix(AbilitySystem __instance)
        {
            try
            {
                if (!ServerReadySystem.Instance.IsServerReady) return;

                // Monitor ability casts
                var castQuery = __instance.__AbilityCastEventQuery.ToEntityArray(Allocator.Temp);
                foreach (var entity in castQuery)
                {
                    if (!entity.Has<AbilityCastEvent>()) continue;
                    
                    var castEvent = entity.Read<AbilityCastEvent>();
                    var abilityPrefab = castEvent.Ability.Read<PrefabGUID>();
                    
                    Log.LogInfo($"Ability cast: {castEvent.Caster} - Ability {abilityPrefab}");
                    
                    // Trigger ability cast event for other mods
                    TriggerAbilityCastEvent(castEvent.Caster, castEvent.Ability, abilityPrefab);
                }
                castQuery.Dispose();

                // Monitor ability cooldown changes
                var cooldownQuery = __instance.__AbilityCooldownModifyEventQuery.ToEntityArray(Allocator.Temp);
                foreach (var entity in cooldownQuery)
                {
                    if (!entity.Has<AbilityCooldownModifyEvent>()) continue;
                    
                    var cooldownEvent = entity.Read<AbilityCooldownModifyEvent>();
                    
                    Log.LogInfo($"Ability cooldown modified: {cooldownEvent.Owner} - CooldownType: {cooldownEvent.CooldownType}");
                    
                    // Trigger cooldown modify event for other mods
                    TriggerAbilityCooldownEvent(cooldownEvent.Owner, cooldownEvent.CooldownType);
                }
                cooldownQuery.Dispose();
            }
            catch (Exception ex)
            {
                Log.LogError($"Error in AbilitySystem patch: {ex.Message}");
            }
        }

        #endregion

        #region Buff System Patches

        /// <summary>
        /// Patch BuffSystem to monitor all buff applications and removals.
        /// Essential for mods that need to track buff states, modify buff effects, or implement custom buff logic.
        /// </summary>
        [HarmonyPatch(typeof(BuffSystem), nameof(BuffSystem.OnUpdate))]
        [HarmonyPostfix]
        private static void BuffSystemOnUpdatePostfix(BuffSystem __instance)
        {
            try
            {
                if (!ServerReadySystem.Instance.IsServerReady) return;

                // Monitor buff applications
                var applyQuery = __instance.__ApplyBuffEventQuery.ToEntityArray(Allocator.Temp);
                foreach (var entity in applyQuery)
                {
                    if (!entity.Has<ApplyBuffEvent>()) continue;
                    
                    var applyEvent = entity.Read<ApplyBuffEvent>();
                    var buffPrefab = applyEvent.BuffPrefab.Read<PrefabGUID>();
                    
                    Log.LogInfo($"Buff applied: {applyEvent.Target} - Buff {buffPrefab} from {applyEvent.Sender}");
                    
                    // Trigger buff apply event for other mods
                    TriggerBuffApplyEvent(applyEvent.Target, applyEvent.Sender, buffPrefab);
                }
                applyQuery.Dispose();

                // Monitor buff removals
                var removeQuery = __instance.__RemoveBuffEventQuery.ToEntityArray(Allocator.Temp);
                foreach (var entity in removeQuery)
                {
                    if (!entity.Has<RemoveBuffEvent>()) continue;
                    
                    var removeEvent = entity.Read<RemoveBuffEvent>();
                    var buffPrefab = removeEvent.BuffPrefab.Read<PrefabGUID>();
                    
                    Log.LogInfo($"Buff removed: {removeEvent.Target} - Buff {buffPrefab} from {removeEvent.Sender}");
                    
                    // Trigger buff remove event for other mods
                    TriggerBuffRemoveEvent(removeEvent.Target, removeEvent.Sender, buffPrefab);
                }
                removeQuery.Dispose();
            }
            catch (Exception ex)
            {
                Log.LogError($"Error in BuffSystem patch: {ex.Message}");
            }
        }

        #endregion

        #region Combat System Patches

        /// <summary>
        /// Patch CombatSystem to monitor all combat events including damage and healing.
        /// Critical for combat mods, damage modifiers, and custom combat mechanics.
        /// </summary>
        [HarmonyPatch(typeof(CombatSystem), nameof(CombatSystem.OnUpdate))]
        [HarmonyPostfix]
        private static void CombatSystemOnUpdatePostfix(CombatSystem __instance)
        {
            try
            {
                if (!ServerReadySystem.Instance.IsServerReady) return;

                // Monitor damage events
                var damageQuery = __instance.__DealDamageEventQuery.ToEntityArray(Allocator.Temp);
                foreach (var entity in damageQuery)
                {
                    if (!entity.Has<DealDamageEvent>()) continue;
                    
                    var damageEvent = entity.Read<DealDamageEvent>();
                    
                    Log.LogInfo($"Damage dealt: {damageEvent.Target} from {damageEvent.Source} - Amount: {damageEvent.Amount}");
                    
                    // Trigger damage event for other mods
                    TriggerDamageEvent(damageEvent.Source, damageEvent.Target, damageEvent.Amount);
                }
                damageQuery.Dispose();

                // Monitor healing events
                var healQuery = __instance.__HealEventQuery.ToEntityArray(Allocator.Temp);
                foreach (var entity in healQuery)
                {
                    if (!entity.Has<HealEvent>()) continue;
                    
                    var healEvent = entity.Read<HealEvent>();
                    
                    Log.LogInfo($"Healing: {healEvent.Target} from {healEvent.Source} - Amount: {healEvent.Amount}");
                    
                    // Trigger heal event for other mods
                    TriggerHealEvent(healEvent.Source, healEvent.Target, healEvent.Amount);
                }
                healQuery.Dispose();
            }
            catch (Exception ex)
            {
                Log.LogError($"Error in CombatSystem patch: {ex.Message}");
            }
        }

        #endregion

        #region Death System Patches

        /// <summary>
        /// Patch DeathSystem to monitor all death events.
        /// Essential for mods that need to handle death mechanics, respawn logic, or death penalties.
        /// </summary>
        [HarmonyPatch(typeof(DeathSystem), nameof(DeathSystem.OnUpdate))]
        [HarmonyPostfix]
        private static void DeathSystemOnUpdatePostfix(DeathSystem __instance)
        {
            try
            {
                if (!ServerReadySystem.Instance.IsServerReady) return;

                var query = __instance.__DeathEventQuery.ToEntityArray(Allocator.Temp);
                foreach (var entity in query)
                {
                    if (!entity.Has<DeathEvent>()) continue;
                    
                    var deathEvent = entity.Read<DeathEvent>();
                    
                    Log.LogInfo($"Death event: {deathEvent.Dead} - Source: {deathEvent.Source} - Reason: {deathEvent.Reason}");
                    
                    // Trigger death event for other mods
                    TriggerDeathEvent(deathEvent.Dead, deathEvent.Source, deathEvent.Reason);
                }
                query.Dispose();
            }
            catch (Exception ex)
            {
                Log.LogError($"Error in DeathSystem patch: {ex.Message}");
            }
        }

        #endregion

        #region Level System Patches

        /// <summary>
        /// Patch LevelSystem to monitor level changes and experience gains.
        /// Critical for progression mods, custom leveling systems, and experience modifications.
        /// </summary>
        [HarmonyPatch(typeof(LevelSystem), nameof(LevelSystem.OnUpdate))]
        [HarmonyPostfix]
        private static void LevelSystemOnUpdatePostfix(LevelSystem __instance)
        {
            try
            {
                if (!ServerReadySystem.Instance.IsServerReady) return;

                var query = __instance.__LevelUpEventQuery.ToEntityArray(Allocator.Temp);
                foreach (var entity in query)
                {
                    if (!entity.Has<LevelUpEvent>()) continue;
                    
                    var levelUpEvent = entity.Read<LevelUpEvent>();
                    
                    Log.LogInfo($"Level up: {levelUpEvent.Character} - New Level: {levelUpEvent.NewLevel}");
                    
                    // Trigger level up event for other mods
                    TriggerLevelUpEvent(levelUpEvent.Character, levelUpEvent.NewLevel);
                }
                query.Dispose();
            }
            catch (Exception ex)
            {
                Log.LogError($"Error in LevelSystem patch: {ex.Message}");
            }
        }

        #endregion

        #region Zone System Patches

        /// <summary>
        /// Patch ZoneSystem to monitor zone entries and exits.
        /// Essential for zone-based mods, regional restrictions, and zone-specific mechanics.
        /// </summary>
        [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.OnUpdate))]
        [HarmonyPostfix]
        private static void ZoneSystemOnUpdatePostfix(ZoneSystem __instance)
        {
            try
            {
                if (!ServerReadySystem.Instance.IsServerReady) return;

                // Monitor zone entries
                var entryQuery = __instance.__ZoneEntryEventQuery.ToEntityArray(Allocator.Temp);
                foreach (var entity in entryQuery)
                {
                    if (!entity.Has<ZoneEntryEvent>()) continue;
                    
                    var entryEvent = entity.Read<ZoneEntryEvent>();
                    
                    Log.LogInfo($"Zone entry: {entryEvent.Player} - Zone: {entryEvent.ZoneId}");
                    
                    // Trigger zone entry event for other mods
                    TriggerZoneEntryEvent(entryEvent.Player, entryEvent.ZoneId);
                }
                entryQuery.Dispose();

                // Monitor zone exits
                var exitQuery = __instance.__ZoneExitEventQuery.ToEntityArray(Allocator.Temp);
                foreach (var entity in exitQuery)
                {
                    if (!entity.Has<ZoneExitEvent>()) continue;
                    
                    var exitEvent = entity.Read<ZoneExitEvent>();
                    
                    Log.LogInfo($"Zone exit: {exitEvent.Player} - Zone: {exitEvent.ZoneId}");
                    
                    // Trigger zone exit event for other mods
                    TriggerZoneExitEvent(exitEvent.Player, exitEvent.ZoneId);
                }
                exitQuery.Dispose();
            }
            catch (Exception ex)
            {
                Log.LogError($"Error in ZoneSystem patch: {ex.Message}");
            }
        }

        #endregion

        #region Event Trigger Methods

        private static void TriggerPlayerConnectedEvent(Entity playerEntity, Entity userEntity)
        {
            // This can be used by other mods to hook into player connections
            // Mod developers can subscribe to these events or extend this functionality
        }

        private static void TriggerPlayerDisconnectedEvent(Entity playerEntity, Entity userEntity)
        {
            // This can be used by other mods to hook into player disconnections
        }

        private static void TriggerInventoryChangeEvent(Entity owner, Entity itemEntity, PrefabGUID itemPrefab, InventoryOperation operation)
        {
            // This can be used by other mods to hook into inventory changes
        }

        private static void TriggerAbilityCastEvent(Entity caster, Entity ability, PrefabGUID abilityPrefab)
        {
            // This can be used by other mods to hook into ability casts
        }

        private static void TriggerAbilityCooldownEvent(Entity owner, CooldownType cooldownType)
        {
            // This can be used by other mods to hook into ability cooldowns
        }

        private static void TriggerBuffApplyEvent(Entity target, Entity sender, PrefabGUID buffPrefab)
        {
            // This can be used by other mods to hook into buff applications
        }

        private static void TriggerBuffRemoveEvent(Entity target, Entity sender, PrefabGUID buffPrefab)
        {
            // This can be used by other mods to hook into buff removals
        }

        private static void TriggerDamageEvent(Entity source, Entity target, float amount)
        {
            // This can be used by other mods to hook into damage events
        }

        private static void TriggerHealEvent(Entity source, Entity target, float amount)
        {
            // This can be used by other mods to hook into healing events
        }

        private static void TriggerDeathEvent(Entity dead, Entity source, DeathReason reason)
        {
            // This can be used by other mods to hook into death events
        }

        private static void TriggerLevelUpEvent(Entity character, int newLevel)
        {
            // This can be used by other mods to hook into level up events
        }

        private static void TriggerZoneEntryEvent(Entity player, string zoneId)
        {
            // This can be used by other mods to hook into zone entries
        }

        private static void TriggerZoneExitEvent(Entity player, string zoneId)
        {
            // This can be used by other mods to hook into zone exits
        }

        private static void TriggerVBloodTrackStartEvent(Entity bloodAltar, Entity vBloodUnit, Entity player)
        {
            // This can be used by other mods to hook into VBlood tracking start
            // Essential for VBlood hunt mods, tracking systems, and blood altar modifications
        }

        private static void TriggerVBloodTrackStopEvent(Entity bloodAltar, Entity vBloodUnit, Entity player, StopReason reason)
        {
            // This can be used by other mods to hook into VBlood tracking stop
            // Critical for VBlood hunt completion, failure handling, and blood altar mechanics
        }

        #endregion

        #region Blood Altar System Patches

        /// <summary>
        /// Patch BloodAltarSystem to monitor VBlood unit tracking.
        /// Critical for mods that need to track VBlood hunts, altar interactions, or modify blood altar mechanics.
        /// </summary>
        [HarmonyPatch(typeof(BloodAltarSystem), nameof(BloodAltarSystem.OnUpdate))]
        [HarmonyPostfix]
        private static void BloodAltarSystemOnUpdatePostfix(BloodAltarSystem __instance)
        {
            try
            {
                if (!ServerReadySystem.Instance.IsServerReady) return;

                // Monitor VBlood unit tracking start
                var startTrackQuery = __instance.__StartTrackVBloodUnit_System_V2Query.ToEntityArray(Allocator.Temp);
                foreach (var entity in startTrackQuery)
                {
                    if (!entity.Has<StartTrackVBloodUnit_System_V2>()) continue;
                    
                    var trackEvent = entity.Read<StartTrackVBloodUnit_System_V2>();
                    var bloodAltar = trackEvent.BloodAltar;
                    var vBloodUnit = trackEvent.VBloodUnit;
                    
                    Log.LogInfo($"Started tracking VBlood unit: {vBloodUnit} at altar {bloodAltar}");
                    
                    // Trigger VBlood tracking start event for other mods
                    TriggerVBloodTrackStartEvent(bloodAltar, vBloodUnit, trackEvent.Player);
                }
                startTrackQuery.Dispose();

                // Monitor VBlood unit tracking stop
                var stopTrackQuery = __instance.__StopTrackVBloodUnit_SystemQuery.ToEntityArray(Allocator.Temp);
                foreach (var entity in stopTrackQuery)
                {
                    if (!entity.Has<StopTrackVBloodUnit_System>()) continue;
                    
                    var stopEvent = entity.Read<StopTrackVBloodUnit_System>();
                    var bloodAltar = stopEvent.BloodAltar;
                    var vBloodUnit = stopEvent.VBloodUnit;
                    
                    Log.LogInfo($"Stopped tracking VBlood unit: {vBloodUnit} at altar {bloodAltar}");
                    
                    // Trigger VBlood tracking stop event for other mods
                    TriggerVBloodTrackStopEvent(bloodAltar, vBloodUnit, stopEvent.Player, stopEvent.Reason);
                }
                stopTrackQuery.Dispose();
            }
            catch (Exception ex)
            {
                Log.LogError($"Error in BloodAltarSystem patch: {ex.Message}");
            }
        }

        #endregion

        #region Helper Methods for Mods

        /// <summary>
        /// Helper method for mods to safely add components to entities.
        /// </summary>
        public static bool SafeAddComponent<T>(Entity entity) where T : struct
        {
            try
            {
                var em = UnifiedCore.EntityManager;
                if (!em.Exists(entity)) return false;
                
                if (!entity.Has<T>())
                {
                    entity.Add<T>();
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.LogError($"Error adding component {typeof(T).Name} to entity {entity}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Helper method for mods to safely remove components from entities.
        /// </summary>
        public static bool SafeRemoveComponent<T>(Entity entity) where T : struct
        {
            try
            {
                var em = UnifiedCore.EntityManager;
                if (!em.Exists(entity)) return false;
                
                if (entity.Has<T>())
                {
                    entity.Remove<T>();
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.LogError($"Error removing component {typeof(T).Name} from entity {entity}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Helper method for mods to safely read component data.
        /// </summary>
        public static T SafeReadComponent<T>(Entity entity) where T : struct
        {
            try
            {
                var em = UnifiedCore.EntityManager;
                if (!em.Exists(entity) || !entity.Has<T>()) return default;
                
                return entity.Read<T>();
            }
            catch (Exception ex)
            {
                Log.LogError($"Error reading component {typeof(T).Name} from entity {entity}: {ex.Message}");
                return default;
            }
        }

        #endregion
    }
}
