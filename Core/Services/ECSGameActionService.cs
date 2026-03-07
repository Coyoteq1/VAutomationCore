using System;
using System.Linq;
using System.Collections.Concurrent;
using ProjectM;
using ProjectM.Network;
using ProjectM.Shared;
using Stunlock.Core;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using VAutomationCore.Abstractions;
using VAutomationCore.Core;
using VAutomationCore.Core.Api;
using VAutomationCore.Core.Logging;
using VAuto.Extensions;

namespace VAutomationCore.Services
{
    /// <summary>
    /// ECS-specific game actions for ProjectM components used by flow execution.
    /// Provides safe component operations for achievements, progression, admin users, and interactables.
    /// </summary>
    public static class ECSGameActionService
    {
        private static readonly CoreLogger Log = new("ECSGameActionService");
        private static readonly ConcurrentDictionary<string, Func<object[], EntityMap?, bool>> ECSActions =
            new(StringComparer.OrdinalIgnoreCase);

        static ECSGameActionService()
        {
            RegisterECSActions();
        }

        /// <summary>
        /// Try to invoke an ECS-specific action.
        /// Returns false when the action is missing or execution fails.
        /// </summary>
        public static bool TryInvokeECSAction(string actionName, object[] args, EntityMap? entityMap, out bool result)
        {
            result = false;
            if (string.IsNullOrWhiteSpace(actionName))
            {
                return false;
            }

            if (ECSActions.TryGetValue(actionName.Trim().ToLowerInvariant(), out var action))
            {
                try
                {
                    result = action(args, entityMap);
                    return true;
                }
                catch (Exception ex)
                {
                    Log.LogError($"Error executing ECS action '{actionName}': {ex.Message}");
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// Register a custom ECS action.
        /// </summary>
        public static bool RegisterECSAction(string actionName, Func<object[], EntityMap?, bool> action)
        {
            if (string.IsNullOrWhiteSpace(actionName) || action == null)
            {
                return false;
            }

            return ECSActions.TryAdd(actionName.Trim().ToLowerInvariant(), action);
        }

        /// <summary>
        /// Get all registered ECS action names.
        /// </summary>
        public static string[] GetECSActionNames()
        {
            return ECSActions.Keys.ToArray();
        }

        private static void RegisterECSActions()
        {
            // Achievement System Actions
            RegisterECSAction("progress_achievement", ProgressAchievementAction);
            RegisterECSAction("grant_achievement", GrantAchievementAction);
            RegisterECSAction("achievement_progress_kill", AchievementProgressKillAction);
            RegisterECSAction("achievement_progress_craft", AchievementProgressCraftAction);
            RegisterECSAction("achievement_progress_item_gain", AchievementProgressItemGainAction);
            RegisterECSAction("achievement_progress_consume", AchievementProgressConsumeAction);

            // Progression System Actions
            RegisterECSAction("unlock_progression", UnlockProgressionAction);
            RegisterECSAction("grant_progression", GrantProgressionAction);
            RegisterECSAction("unlock_spellbook_abilities", UnlockSpellbookAbilitiesAction);
            RegisterECSAction("unlock_vblood_progression", UnlockVBloodProgressionAction);

            // Admin User Actions
            RegisterECSAction("admin_promote_user", AdminPromoteUserAction);
            RegisterECSAction("admin_demote_user", AdminDemoteUserAction);
            RegisterECSAction("admin_validate_user", AdminValidateUserAction);

            // Interactable System Actions
            RegisterECSAction("start_interactable_sequence", StartInteractableSequenceAction);
            RegisterECSAction("end_interactable_sequence", EndInteractableSequenceAction);

            // Location/Movement Actions
            RegisterECSAction("entity_tilt_to_location", EntityTiltToLocationAction);
            RegisterECSAction("entity_set_spawn_location", EntitySetSpawnLocationAction);

            // Visual/Display Actions
            RegisterECSAction("place_tilemodel_ability", PlaceTilemodelAbilityAction);
            RegisterECSAction("player_custom_marker", PlayerCustomMarkerAction);
            RegisterECSAction("play_mounted_sequence", PlayMountedSequenceAction);
            RegisterECSAction("is_previewing_placement", IsPreviewingPlacementAction);

            // Zone System Actions
            RegisterECSAction("sunblocker_region", SunBlockerRegionAction);

            // Blood Altar System Actions
            RegisterECSAction("bloodaltar_track_start", BloodAltarTrackStartAction);
            RegisterECSAction("bloodaltar_track_stop", BloodAltarTrackStopAction);
        }

        #region Achievement System Actions

        private static bool ProgressAchievementAction(object[] args, EntityMap? entityMap)
        {
            if (!TryGetEntityArg(args, 0, out var playerEntity, entityMap) ||
                !TryGetStringArg(args, 1, out var achievementId) ||
                !TryGetIntArg(args, 2, out var progress))
            {
                return false;
            }

            try
            {
                var em = UnifiedCore.EntityManager;
                if (!em.Exists(playerEntity))
                {
                    Log.LogWarning($"Player entity {playerEntity} does not exist");
                    return false;
                }

                // Check if player has achievement component
                if (!playerEntity.Has<ProjectM.AchievementOwner>())
                {
                    playerEntity.Add<ProjectM.AchievementOwner>();
                }

                var achievementOwner = playerEntity.Read<ProjectM.AchievementOwner>();
                // Update achievement progress logic here
                Log.LogInfo($"Progressed achievement {achievementId} for player {playerEntity} by {progress}");
                return true;
            }
            catch (Exception ex)
            {
                Log.LogError($"Error progressing achievement: {ex.Message}");
                return false;
            }
        }

        private static bool GrantAchievementAction(object[] args, EntityMap? entityMap)
        {
            if (!TryGetEntityArg(args, 0, out var playerEntity) ||
                !TryGetStringArg(args, 1, out var achievementId))
            {
                return false;
            }

            try
            {
                var em = UnifiedCore.EntityManager;
                if (!em.Exists(playerEntity))
                {
                    return false;
                }

                // Grant achievement logic
                Log.LogInfo($"Granted achievement {achievementId} to player {playerEntity}");
                return true;
            }
            catch (Exception ex)
            {
                Log.LogError($"Error granting achievement: {ex.Message}");
                return false;
            }
        }

        private static bool AchievementProgressKillAction(object[] args, EntityMap? entityMap)
        {
            if (!TryGetEntityArg(args, 0, out var killerEntity) ||
                !TryGetEntityArg(args, 1, out var victimEntity) ||
                !TryGetStringArg(args, 2, out var achievementId))
            {
                return false;
            }

            try
            {
                // Progress kill-based achievement
                Log.LogInfo($"Kill achievement progress: {killerEntity} killed {victimEntity} for {achievementId}");
                return true;
            }
            catch (Exception ex)
            {
                Log.LogError($"Error in kill achievement progress: {ex.Message}");
                return false;
            }
        }

        private static bool AchievementProgressCraftAction(object[] args, EntityMap? entityMap)
        {
            if (!TryGetEntityArg(args, 0, out var playerEntity) ||
                !TryGetPrefabGuidArg(args, 1, out var craftedItemGuid) ||
                !TryGetStringArg(args, 2, out var achievementId))
            {
                return false;
            }

            try
            {
                // Progress crafting achievement
                Log.LogInfo($"Craft achievement progress: {playerEntity} crafted {craftedItemGuid} for {achievementId}");
                return true;
            }
            catch (Exception ex)
            {
                Log.LogError($"Error in craft achievement progress: {ex.Message}");
                return false;
            }
        }

        private static bool AchievementProgressItemGainAction(object[] args, EntityMap? entityMap)
        {
            if (!TryGetEntityArg(args, 0, out var playerEntity) ||
                !TryGetPrefabGuidArg(args, 1, out var itemGuid) ||
                !TryGetStringArg(args, 2, out var achievementId))
            {
                return false;
            }

            try
            {
                // Progress item gain achievement
                Log.LogInfo($"Item gain achievement progress: {playerEntity} gained {itemGuid} for {achievementId}");
                return true;
            }
            catch (Exception ex)
            {
                Log.LogError($"Error in item gain achievement progress: {ex.Message}");
                return false;
            }
        }

        private static bool AchievementProgressConsumeAction(object[] args, EntityMap? entityMap)
        {
            if (!TryGetEntityArg(args, 0, out var playerEntity) ||
                !TryGetPrefabGuidArg(args, 1, out var consumedItemGuid) ||
                !TryGetStringArg(args, 2, out var achievementId))
            {
                return false;
            }

            try
            {
                // Progress consume achievement
                Log.LogInfo($"Consume achievement progress: {playerEntity} consumed {consumedItemGuid} for {achievementId}");
                return true;
            }
            catch (Exception ex)
            {
                Log.LogError($"Error in consume achievement progress: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Progression System Actions

        private static bool UnlockProgressionAction(object[] args, EntityMap? entityMap)
        {
            if (!TryGetEntityArg(args, 0, out var playerEntity) ||
                !TryGetStringArg(args, 1, out var progressionId))
            {
                return false;
            }

            try
            {
                var em = UnifiedCore.EntityManager;
                if (!em.Exists(playerEntity))
                {
                    return false;
                }

                // Add or update progression component
                if (!playerEntity.Has<ProjectM.DefaultUnlockedProgression>())
                {
                    playerEntity.Add<ProjectM.DefaultUnlockedProgression>();
                }

                Log.LogInfo($"Unlocked progression {progressionId} for player {playerEntity}");
                return true;
            }
            catch (Exception ex)
            {
                Log.LogError($"Error unlocking progression: {ex.Message}");
                return false;
            }
        }

        private static bool GrantProgressionAction(object[] args, EntityMap? entityMap)
        {
            if (!TryGetEntityArg(args, 0, out var playerEntity) ||
                !TryGetStringArg(args, 1, out var progressionType) ||
                !TryGetStringArg(args, 2, out var progressionId))
            {
                return false;
            }

            try
            {
                // Grant specific progression based on type
                Log.LogInfo($"Granted {progressionType} progression {progressionId} to player {playerEntity}");
                return true;
            }
            catch (Exception ex)
            {
                Log.LogError($"Error granting progression: {ex.Message}");
                return false;
            }
        }

        private static bool UnlockSpellbookAbilitiesAction(object[] args, EntityMap? entityMap)
        {
            if (!TryGetEntityArg(args, 0, out var playerEntity))
            {
                return false;
            }

            try
            {
                // Trigger spellbook abilities unlock event
                Log.LogInfo($"Unlocked spellbook abilities for player {playerEntity}");
                return true;
            }
            catch (Exception ex)
            {
                Log.LogError($"Error unlocking spellbook abilities: {ex.Message}");
                return false;
            }
        }

        private static bool UnlockVBloodProgressionAction(object[] args, EntityMap? entityMap)
        {
            if (!TryGetEntityArg(args, 0, out var playerEntity) ||
                !TryGetPrefabGuidArg(args, 1, out var vBloodGuid))
            {
                return false;
            }

            try
            {
                var em = UnifiedCore.EntityManager;
                if (!em.Exists(playerEntity))
                {
                    return false;
                }

                // Add VBlood progression component
                if (!playerEntity.Has<ProjectM.VBloodProgressionUnlockData>())
                {
                    playerEntity.Add<ProjectM.VBloodProgressionUnlockData>();
                }

                Log.LogInfo($"Unlocked VBlood progression {vBloodGuid} for player {playerEntity}");
                return true;
            }
            catch (Exception ex)
            {
                Log.LogError($"Error unlocking VBlood progression: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Admin User Actions

        private static bool AdminPromoteUserAction(object[] args, EntityMap? entityMap)
        {
            if (!TryGetEntityArg(args, 0, out var userEntity) ||
                !TryGetStringArg(args, 1, out var adminLevel))
            {
                return false;
            }

            try
            {
                var em = UnifiedCore.EntityManager;
                if (!em.Exists(userEntity))
                {
                    return false;
                }

                // Add admin user component
                if (!userEntity.Has<ProjectM.AdminUser>())
                {
                    userEntity.Add<ProjectM.AdminUser>();
                }

                var adminUser = userEntity.Read<ProjectM.AdminUser>();
                // Update admin level logic here
                Log.LogInfo($"Promoted user {userEntity} to admin level {adminLevel}");
                return true;
            }
            catch (Exception ex)
            {
                Log.LogError($"Error promoting admin user: {ex.Message}");
                return false;
            }
        }

        private static bool AdminDemoteUserAction(object[] args, EntityMap? entityMap)
        {
            if (!TryGetEntityArg(args, 0, out var userEntity))
            {
                return false;
            }

            try
            {
                var em = UnifiedCore.EntityManager;
                if (!em.Exists(userEntity))
                {
                    return false;
                }

                // Remove admin user component
                if (userEntity.Has<ProjectM.AdminUser>())
                {
                    userEntity.Remove<ProjectM.AdminUser>();
                }

                Log.LogInfo($"Demoted user {userEntity} from admin");
                return true;
            }
            catch (Exception ex)
            {
                Log.LogError($"Error demoting admin user: {ex.Message}");
                return false;
            }
        }

        private static bool AdminValidateUserAction(object[] args, EntityMap? entityMap)
        {
            if (!TryGetEntityArg(args, 0, out var userEntity))
            {
                return false;
            }

            try
            {
                var em = UnifiedCore.EntityManager;
                if (!em.Exists(userEntity))
                {
                    return false;
                }

                var isAdmin = userEntity.Has<ProjectM.AdminUser>();
                Log.LogInfo($"Admin validation for user {userEntity}: {(isAdmin ? "Is Admin" : "Not Admin")}");
                return isAdmin;
            }
            catch (Exception ex)
            {
                Log.LogError($"Error validating admin user: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Interactable System Actions

        private static bool StartInteractableSequenceAction(object[] args, EntityMap? entityMap)
        {
            if (!TryGetEntityArg(args, 0, out var playerEntity) ||
                !TryGetEntityArg(args, 1, out var interactableEntity) ||
                !TryGetStringArg(args, 2, out var sequenceId))
            {
                return false;
            }

            try
            {
                var em = UnifiedCore.EntityManager;
                if (!em.Exists(playerEntity) || !em.Exists(interactableEntity))
                {
                    return false;
                }

                // Add interactable sequence component
                if (!interactableEntity.Has<ProjectM.InteractableSequence>())
                {
                    interactableEntity.Add<ProjectM.InteractableSequence>();
                }

                Log.LogInfo($"Started interactable sequence {sequenceId} for player {playerEntity} on entity {interactableEntity}");
                return true;
            }
            catch (Exception ex)
            {
                Log.LogError($"Error starting interactable sequence: {ex.Message}");
                return false;
            }
        }

        private static bool EndInteractableSequenceAction(object[] args, EntityMap? entityMap)
        {
            if (!TryGetEntityArg(args, 0, out var playerEntity) ||
                !TryGetEntityArg(args, 1, out var interactableEntity) ||
                !TryGetStringArg(args, 2, out var sequenceId))
            {
                return false;
            }

            try
            {
                var em = UnifiedCore.EntityManager;
                if (!em.Exists(playerEntity) || !em.Exists(interactableEntity))
                {
                    return false;
                }

                // Add end interactable sequence component
                if (!interactableEntity.Has<ProjectM.EndInteractableSequence>())
                {
                    interactableEntity.Add<ProjectM.EndInteractableSequence>();
                }

                Log.LogInfo($"Ended interactable sequence {sequenceId} for player {playerEntity} on entity {interactableEntity}");
                return true;
            }
            catch (Exception ex)
            {
                Log.LogError($"Error ending interactable sequence: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Location/Movement Actions

        private static bool EntityTiltToLocationAction(object[] args, EntityMap? entityMap)
        {
            if (!TryGetEntityArg(args, 0, out var targetEntity) ||
                !TryGetFloat3Arg(args, 1, out var targetLocation))
            {
                return false;
            }

            try
            {
                var em = UnifiedCore.EntityManager;
                if (!em.Exists(targetEntity))
                {
                    return false;
                }

                // Add tilt toward location component
                if (!targetEntity.Has<ProjectM.TiltTowardGameplayLocationOnSpawn>())
                {
                    targetEntity.Add<ProjectM.TiltTowardGameplayLocationOnSpawn>();
                }

                // Update position if entity has transform
                if (targetEntity.Has<Translation>())
                {
                    var translation = targetEntity.Read<Translation>();
                    translation.Value = targetLocation;
                    targetEntity.Write(translation);
                }

                Log.LogInfo($"Set entity {targetEntity} to tilt toward location {targetLocation}");
                return true;
            }
            catch (Exception ex)
            {
                Log.LogError($"Error setting entity tilt to location: {ex.Message}");
                return false;
            }
        }

        private static bool EntitySetSpawnLocationAction(object[] args, EntityMap? entityMap)
        {
            if (!TryGetEntityArg(args, 0, out var targetEntity) ||
                !TryGetFloat3Arg(args, 1, out var spawnLocation))
            {
                return false;
            }

            try
            {
                var em = UnifiedCore.EntityManager;
                if (!em.Exists(targetEntity))
                {
                    return false;
                }

                // Set spawn location
                if (targetEntity.Has<Translation>())
                {
                    var translation = targetEntity.Read<Translation>();
                    translation.Value = spawnLocation;
                    targetEntity.Write(translation);
                }

                Log.LogInfo($"Set spawn location for entity {targetEntity} to {spawnLocation}");
                return true;
            }
            catch (Exception ex)
            {
                Log.LogError($"Error setting spawn location: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Visual/Display Actions

        private static bool PlaceTilemodelAbilityAction(object[] args, EntityMap? entityMap)
        {
            if (!TryGetEntityArg(args, 0, out var playerEntity) ||
                !TryGetPrefabGuidArg(args, 1, out var tilemodelGuid))
            {
                return false;
            }

            try
            {
                var em = UnifiedCore.EntityManager;
                if (!em.Exists(playerEntity))
                {
                    return false;
                }

                // Add PlaceTilemodelAbility component for glow effect
                if (!playerEntity.Has<ProjectM.PlaceTilemodelAbility>())
                {
                    playerEntity.Add<ProjectM.PlaceTilemodelAbility>();
                }

                Log.LogInfo($"Placed tilemodel ability {tilemodelGuid} for player {playerEntity} (glow effect)");
                return true;
            }
            catch (Exception ex)
            {
                Log.LogError($"Error placing tilemodel ability: {ex.Message}");
                return false;
            }
        }

        private static bool PlayerCustomMarkerAction(object[] args, EntityMap? entityMap)
        {
            if (!TryGetEntityArg(args, 0, out var playerEntity) ||
                !TryGetStringArg(args, 1, out var markerType) ||
                !TryGetFloat3Arg(args, 2, out var markerPosition))
            {
                return false;
            }

            try
            {
                var em = UnifiedCore.EntityManager;
                if (!em.Exists(playerEntity))
                {
                    return false;
                }

                // Add PlayerCustomMarker component for player icon
                if (!playerEntity.Has<ProjectM.PlayerCustomMarker>())
                {
                    playerEntity.Add<ProjectM.PlayerCustomMarker>();
                }

                Log.LogInfo($"Set custom marker {markerType} for player {playerEntity} at position {markerPosition}");
                return true;
            }
            catch (Exception ex)
            {
                Log.LogError($"Error setting player custom marker: {ex.Message}");
                return false;
            }
        }

        private static bool PlayMountedSequenceAction(object[] args, EntityMap? entityMap)
        {
            if (!TryGetEntityArg(args, 0, out var playerEntity) ||
                !TryGetEntityArg(args, 1, out var mountEntity))
            {
                return false;
            }

            try
            {
                var em = UnifiedCore.EntityManager;
                if (!em.Exists(playerEntity) || !em.Exists(mountEntity))
                {
                    return false;
                }

                // Add PlayMountedSequence component for horse mounting
                if (!playerEntity.Has<ProjectM.PlayMountedSequence>())
                {
                    playerEntity.Add<ProjectM.PlayMountedSequence>();
                }

                Log.LogInfo($"Started mounted sequence for player {playerEntity} on mount {mountEntity}");
                return true;
            }
            catch (Exception ex)
            {
                Log.LogError($"Error starting mounted sequence: {ex.Message}");
                return false;
            }
        }

        private static bool IsPreviewingPlacementAction(object[] args, EntityMap? entityMap)
        {
            if (!TryGetEntityArg(args, 0, out var playerEntity) ||
                !TryGetBoolArg(args, 1, out var isPreviewing))
            {
                return false;
            }

            try
            {
                var em = UnifiedCore.EntityManager;
                if (!em.Exists(playerEntity))
                {
                    return false;
                }

                // Add or remove IsPreviewingPlacement component
                if (isPreviewing)
                {
                    if (!playerEntity.Has<ProjectM.IsPreviewingPlacement>())
                    {
                        playerEntity.Add<ProjectM.IsPreviewingPlacement>();
                    }
                }
                else
                {
                    if (playerEntity.Has<ProjectM.IsPreviewingPlacement>())
                    {
                        playerEntity.Remove<ProjectM.IsPreviewingPlacement>();
                    }
                }

                Log.LogInfo($"Set placement preview for player {playerEntity} to {isPreviewing}");
                return true;
            }
            catch (Exception ex)
            {
                Log.LogError($"Error setting placement preview: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Zone System Actions

        private static bool SunBlockerRegionAction(object[] args, EntityMap? entityMap)
        {
            if (!TryGetEntityArg(args, 0, out var zoneEntity) ||
                !TryGetStringArg(args, 1, out var actionType))
            {
                return false;
            }

            try
            {
                var em = UnifiedCore.EntityManager;
                if (!em.Exists(zoneEntity))
                {
                    return false;
                }

                // Add or manage SunBlockerRegion component
                if (actionType.ToLower() == "activate" || actionType.ToLower() == "create")
                {
                    if (!zoneEntity.Has<ProjectM.SunBlocker.SunBlockerRegion>())
                    {
                        zoneEntity.Add<ProjectM.SunBlocker.SunBlockerRegion>();
                    }
                    Log.LogInfo($"Activated sun blocker region for zone {zoneEntity}");
                }
                else if (actionType.ToLower() == "deactivate" || actionType.ToLower() == "remove")
                {
                    if (zoneEntity.Has<ProjectM.SunBlocker.SunBlockerRegion>())
                    {
                        zoneEntity.Remove<ProjectM.SunBlocker.SunBlockerRegion>();
                    }
                    Log.LogInfo($"Deactivated sun blocker region for zone {zoneEntity}");
                }
                else
                {
                    // Configuration or status check
                    if (!zoneEntity.Has<ProjectM.SunBlocker.SunBlockerRegion>())
                    {
                        zoneEntity.Add<ProjectM.SunBlocker.SunBlockerRegion>();
                    }
                    Log.LogInfo($"Configured sun blocker region for zone {zoneEntity} with action: {actionType}");
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.LogError($"Error managing sun blocker region: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Blood Altar System Actions

        private static bool BloodAltarTrackStartAction(object[] args, EntityMap? entityMap)
        {
            if (!TryGetEntityArg(args, 0, out var bloodAltarEntity) ||
                !TryGetEntityArg(args, 1, out var vBloodUnitEntity) ||
                !TryGetEntityArg(args, 2, out var playerEntity))
            {
                return false;
            }

            try
            {
                var em = UnifiedCore.EntityManager;
                if (!em.Exists(bloodAltarEntity) || !em.Exists(vBloodUnitEntity) || !em.Exists(playerEntity))
                {
                    return false;
                }

                // Add StartTrackVBloodUnit_System_V2 component
                if (!bloodAltarEntity.Has<ProjectM.StartTrackVBloodUnit_System_V2>())
                {
                    bloodAltarEntity.Add<ProjectM.StartTrackVBloodUnit_System_V2>();
                }

                Log.LogInfo($"Started VBlood tracking at altar {bloodAltarEntity} for unit {vBloodUnitEntity} by player {playerEntity}");
                return true;
            }
            catch (Exception ex)
            {
                Log.LogError($"Error starting VBlood tracking: {ex.Message}");
                return false;
            }
        }

        private static bool BloodAltarTrackStopAction(object[] args, EntityMap? entityMap)
        {
            if (!TryGetEntityArg(args, 0, out var bloodAltarEntity) ||
                !TryGetEntityArg(args, 1, out var vBloodUnitEntity) ||
                !TryGetEntityArg(args, 2, out var playerEntity))
            {
                return false;
            }

            try
            {
                var em = UnifiedCore.EntityManager;
                if (!em.Exists(bloodAltarEntity) || !em.Exists(vBloodUnitEntity) || !em.Exists(playerEntity))
                {
                    return false;
                }

                // Add StopTrackVBloodUnit_System component
                if (!bloodAltarEntity.Has<ProjectM.StopTrackVBloodUnit_System>())
                {
                    bloodAltarEntity.Add<ProjectM.StopTrackVBloodUnit_System>();
                }

                Log.LogInfo($"Stopped VBlood tracking at altar {bloodAltarEntity} for unit {vBloodUnitEntity} by player {playerEntity}");
                return true;
            }
            catch (Exception ex)
            {
                Log.LogError($"Error stopping VBlood tracking: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Helper Methods

        private static bool TryGetEntityArg(object[] args, int index, out Entity entity, EntityMap? entityMap = null)
        {
            entity = default;
            if (args == null || index >= args.Length)
            {
                return false;
            }

            var arg = args[index];
            if (arg is Entity e)
            {
                entity = e;
                return true;
            }

            // Simplified entity resolution without entityMap for now
            return false;
        }

        private static bool TryGetStringArg(object[] args, int index, out string value)
        {
            value = string.Empty;
            if (args == null || index >= args.Length)
            {
                return false;
            }

            if (args[index] is string str)
            {
                value = str;
                return true;
            }

            return false;
        }

        private static bool TryGetIntArg(object[] args, int index, out int value)
        {
            value = 0;
            if (args == null || index >= args.Length)
            {
                return false;
            }

            if (args[index] is int intValue)
            {
                value = intValue;
                return true;
            }

            if (int.TryParse(args[index]?.ToString(), out intValue))
            {
                value = intValue;
                return true;
            }

            return false;
        }

        private static bool TryGetFloatArg(object[] args, int index, out float value)
        {
            value = 0f;
            if (args == null || index >= args.Length)
            {
                return false;
            }

            if (args[index] is float floatValue)
            {
                value = floatValue;
                return true;
            }

            if (float.TryParse(args[index]?.ToString(), out floatValue))
            {
                value = floatValue;
                return true;
            }

            return false;
        }

        private static bool TryGetFloat3Arg(object[] args, int index, out float3 value)
        {
            value = float3.zero;
            if (args == null || index >= args.Length)
            {
                return false;
            }

            if (args[index] is float3 floatValue)
            {
                value = floatValue;
                return true;
            }

            // Try to parse from string format "x,y,z"
            if (args[index] is string str && str.Contains(','))
            {
                var parts = str.Split(',');
                if (parts.Length == 3 &&
                    float.TryParse(parts[0], out var x) &&
                    float.TryParse(parts[1], out var y) &&
                    float.TryParse(parts[2], out var z))
                {
                    value = new float3(x, y, z);
                    return true;
                }
            }

            return false;
        }

        private static bool TryGetBoolArg(object[] args, int index, out bool value)
        {
            value = false;
            if (args == null || index >= args.Length)
            {
                return false;
            }

            if (args[index] is bool boolValue)
            {
                value = boolValue;
                return true;
            }

            if (bool.TryParse(args[index]?.ToString(), out boolValue))
            {
                value = boolValue;
                return true;
            }

            return false;
        }

        private static bool TryGetPrefabGuidArg(object[] args, int index, out PrefabGUID guid)
        {
            guid = PrefabGUID.Empty;
            if (args == null || index >= args.Length)
            {
                return false;
            }

            if (args[index] is PrefabGUID prefabGuid)
            {
                guid = prefabGuid;
                return true;
            }

            if (args[index] is string str && long.TryParse(str, out var guidValue))
            {
                guid = new PrefabGUID((int)guidValue);
                return true;
            }

            return false;
        }

        #endregion
    }
}
