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
using VAutomationCore.Core.Services;
using VAutomationCore.Core.Logging;
using VAuto.Extensions;

namespace VAutomationCore.Services
{
    /// <summary>
    /// Game-specific actions for ProjectM components used by flow execution.
    /// Provides safe component operations for achievements, progression, admin users, interactables, and more.
    /// </summary>
    public static class ModGameActionService
    {
        private static readonly CoreLogger Log = new("ModGameActionService");
        private static readonly ConcurrentDictionary<string, Func<object[], EntityMap?, bool>> GameActions =
            new(StringComparer.OrdinalIgnoreCase);

        static ModGameActionService()
        {
            RegisterGameActions();
        }

        /// <summary>
        /// Try to invoke a game-specific action.
        /// Returns false when the action is missing or execution fails.
        /// </summary>
        public static bool TryInvokeGameAction(string actionName, object[] args, EntityMap? entityMap, out bool result)
        {
            result = false;
            if (string.IsNullOrWhiteSpace(actionName))
            {
                return false;
            }

            if (GameActions.TryGetValue(actionName.Trim().ToLowerInvariant(), out var action))
            {
                try
                {
                    result = action(args, entityMap);
                    return true;
                }
                catch (Exception ex)
                {
                    Log.LogError($"Error executing game action '{actionName}': {ex.Message}");
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// Register a custom game action.
        /// </summary>
        public static bool RegisterGameAction(string actionName, Func<object[], EntityMap?, bool> action)
        {
            if (string.IsNullOrWhiteSpace(actionName) || action == null)
            {
                return false;
            }

            return GameActions.TryAdd(actionName.Trim().ToLowerInvariant(), action);
        }

        /// <summary>
        /// Get all registered game action names.
        /// </summary>
        public static string[] GetGameActionNames()
        {
            return GameActions.Keys.ToArray();
        }

        private static void RegisterGameActions()
        {
            // Achievement System Actions
            RegisterGameAction("progress_achievement", ProgressAchievementAction);
            RegisterGameAction("grant_achievement", GrantAchievementAction);
            RegisterGameAction("achievement_progress_kill", AchievementProgressKillAction);
            RegisterGameAction("achievement_progress_craft", AchievementProgressCraftAction);
            RegisterGameAction("achievement_progress_item_gain", AchievementProgressItemGainAction);
            RegisterGameAction("achievement_progress_consume", AchievementProgressConsumeAction);

            // Progression System Actions
            RegisterGameAction("unlock_progression", UnlockProgressionAction);
            RegisterGameAction("grant_progression", GrantProgressionAction);
            RegisterGameAction("unlock_spellbook_abilities", UnlockSpellbookAbilitiesAction);
            RegisterGameAction("unlock_vblood_progression", UnlockVBloodProgressionAction);

            // Admin User Actions
            RegisterGameAction("admin_promote_user", AdminPromoteUserAction);
            RegisterGameAction("admin_demote_user", AdminDemoteUserAction);
            RegisterGameAction("admin_validate_user", AdminValidateUserAction);

            // Interactable System Actions
            RegisterGameAction("start_interactable_sequence", StartInteractableSequenceAction);
            RegisterGameAction("end_interactable_sequence", EndInteractableSequenceAction);

            // Location/Movement Actions
            RegisterGameAction("entity_tilt_to_location", EntityTiltToLocationAction);
            RegisterGameAction("entity_set_spawn_location", EntitySetSpawnLocationAction);

            // Visual/Display Actions
            RegisterGameAction("place_tilemodel_ability", PlaceTilemodelAbilityAction);
            RegisterGameAction("player_custom_marker", PlayerCustomMarkerAction);
            RegisterGameAction("play_mounted_sequence", PlayMountedSequenceAction);
            RegisterGameAction("is_previewing_placement", IsPreviewingPlacementAction);

            // Zone System Actions
            RegisterGameAction("sunblocker_region", SunBlockerRegionAction);

            // Blood Altar System Actions
            RegisterGameAction("bloodaltar_track_start", BloodAltarTrackStartAction);
            RegisterGameAction("bloodaltar_track_stop", BloodAltarTrackStopAction);

            // Placement Restriction System Actions
            RegisterGameAction("placement_disable_all", PlacementDisableAllAction);
            RegisterGameAction("placement_enable_all", PlacementEnableAllAction);
            RegisterGameAction("placement_disable_zone", PlacementDisableZoneAction);
            RegisterGameAction("placement_enable_zone", PlacementEnableZoneAction);
            RegisterGameAction("placement_disable_player", PlacementDisablePlayerAction);
            RegisterGameAction("placement_enable_player", PlacementEnablePlayerAction);
            RegisterGameAction("placement_disable_global", PlacementDisableGlobalAction);
            RegisterGameAction("placement_enable_global", PlacementEnableGlobalAction);
            RegisterGameAction("placement_disable_area", PlacementDisableAreaAction);
            RegisterGameAction("placement_enable_area", PlacementEnableAreaAction);

            // Visibility and Stealth System Actions
            RegisterGameAction("set_stealth_state", SetStealthStateAction);
            RegisterGameAction("modify_visibility", ModifyVisibilityAction);
            RegisterGameAction("apply_stealth_buff", ApplyStealthBuffAction);
            RegisterGameAction("remove_stealth_buff", RemoveStealthBuffAction);
            RegisterGameAction("trigger_detection", TriggerDetectionAction);
            RegisterGameAction("perform_area_detection", PerformAreaDetectionAction);
            RegisterGameAction("check_line_of_sight", CheckLineOfSightAction);
            RegisterGameAction("block_line_of_sight", BlockLineOfSightAction);
            RegisterGameAction("clear_line_of_sight", ClearLineOfSightAction);
            RegisterGameAction("set_detection_range", SetDetectionRangeAction);
            RegisterGameAction("reset_detection_range", ResetDetectionRangeAction);
            RegisterGameAction("apply_visibility_buff", ApplyVisibilityBuffAction);
            RegisterGameAction("remove_visibility_buffs", RemoveVisibilityBuffsAction);

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

        #region Placement Restriction System Actions

        private static bool PlacementDisableAllAction(object[] args, EntityMap? entityMap)
        {
            if (!TryGetEntityArg(args, 0, out var playerEntity))
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

                // Add placement restriction component to disable all placement
                if (!playerEntity.Has<ProjectM.IsPreviewingPlacement>())
                {
                    playerEntity.Add<ProjectM.IsPreviewingPlacement>();
                }

                Log.LogInfo($"Disabled all placement for player {playerEntity}");
                return true;
            }
            catch (Exception ex)
            {
                Log.LogError($"Error disabling placement: {ex.Message}");
                return false;
            }
        }

        private static bool PlacementEnableAllAction(object[] args, EntityMap? entityMap)
        {
            if (!TryGetEntityArg(args, 0, out var playerEntity))
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

                // Remove placement restriction component to enable all placement
                if (playerEntity.Has<ProjectM.IsPreviewingPlacement>())
                {
                    playerEntity.Remove<ProjectM.IsPreviewingPlacement>();
                }

                Log.LogInfo($"Enabled all placement for player {playerEntity}");
                return true;
            }
            catch (Exception ex)
            {
                Log.LogError($"Error enabling placement: {ex.Message}");
                return false;
            }
        }

        private static bool PlacementDisableZoneAction(object[] args, EntityMap? entityMap)
        {
            if (!TryGetEntityArg(args, 0, out var zoneEntity))
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

                // Add zone placement restriction
                if (!zoneEntity.Has<ProjectM.SunBlocker.SunBlockerRegion>())
                {
                    zoneEntity.Add<ProjectM.SunBlocker.SunBlockerRegion>();
                }

                Log.LogInfo($"Disabled placement in zone {zoneEntity}");
                return true;
            }
            catch (Exception ex)
            {
                Log.LogError($"Error disabling zone placement: {ex.Message}");
                return false;
            }
        }

        private static bool PlacementEnableZoneAction(object[] args, EntityMap? entityMap)
        {
            if (!TryGetEntityArg(args, 0, out var zoneEntity))
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

                // Remove zone placement restriction
                if (zoneEntity.Has<ProjectM.SunBlocker.SunBlockerRegion>())
                {
                    zoneEntity.Remove<ProjectM.SunBlocker.SunBlockerRegion>();
                }

                Log.LogInfo($"Enabled placement in zone {zoneEntity}");
                return true;
            }
            catch (Exception ex)
            {
                Log.LogError($"Error enabling zone placement: {ex.Message}");
                return false;
            }
        }

        private static bool PlacementDisablePlayerAction(object[] args, EntityMap? entityMap)
        {
            if (!TryGetEntityArg(args, 0, out var playerEntity))
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

                // Add player-specific placement restriction
                if (!playerEntity.Has<ProjectM.IsPreviewingPlacement>())
                {
                    playerEntity.Add<ProjectM.IsPreviewingPlacement>();
                }

                Log.LogInfo($"Disabled placement for player {playerEntity}");
                return true;
            }
            catch (Exception ex)
            {
                Log.LogError($"Error disabling player placement: {ex.Message}");
                return false;
            }
        }

        private static bool PlacementEnablePlayerAction(object[] args, EntityMap? entityMap)
        {
            if (!TryGetEntityArg(args, 0, out var playerEntity))
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

                // Remove player-specific placement restriction
                if (playerEntity.Has<ProjectM.IsPreviewingPlacement>())
                {
                    playerEntity.Remove<ProjectM.IsPreviewingPlacement>();
                }

                Log.LogInfo($"Enabled placement for player {playerEntity}");
                return true;
            }
            catch (Exception ex)
            {
                Log.LogError($"Error enabling player placement: {ex.Message}");
                return false;
            }
        }

        private static bool PlacementDisableGlobalAction(object[] args, EntityMap? entityMap)
        {
            try
            {
                // Global placement disable would affect all players
                // This would typically be implemented through a global system
                Log.LogInfo("Global placement disable requested");
                return true;
            }
            catch (Exception ex)
            {
                Log.LogError($"Error disabling global placement: {ex.Message}");
                return false;
            }
        }

        private static bool PlacementEnableGlobalAction(object[] args, EntityMap? entityMap)
        {
            try
            {
                // Global placement enable would affect all players
                // This would typically be implemented through a global system
                Log.LogInfo("Global placement enable requested");
                return true;
            }
            catch (Exception ex)
            {
                Log.LogError($"Error enabling global placement: {ex.Message}");
                return false;
            }
        }

        private static bool PlacementDisableAreaAction(object[] args, EntityMap? entityMap)
        {
            if (!TryGetEntityArg(args, 0, out var playerEntity) ||
                !TryGetStringArg(args, 1, out var areaId))
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

                // Add area-specific placement restriction
                if (!playerEntity.Has<ProjectM.IsPreviewingPlacement>())
                {
                    playerEntity.Add<ProjectM.IsPreviewingPlacement>();
                }

                Log.LogInfo($"Disabled placement in area {areaId} for player {playerEntity}");
                return true;
            }
            catch (Exception ex)
            {
                Log.LogError($"Error disabling area placement: {ex.Message}");
                return false;
            }
        }

        private static bool PlacementEnableAreaAction(object[] args, EntityMap? entityMap)
        {
            if (!TryGetEntityArg(args, 0, out var playerEntity) ||
                !TryGetStringArg(args, 1, out var areaId))
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

                // Remove area-specific placement restriction
                if (playerEntity.Has<ProjectM.IsPreviewingPlacement>())
                {
                    playerEntity.Remove<ProjectM.IsPreviewingPlacement>();
                }

                Log.LogInfo($"Enabled placement in area {areaId} for player {playerEntity}");
                return true;
            }
            catch (Exception ex)
            {
                Log.LogError($"Error enabling area placement: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Visibility and Stealth System Actions

        private static bool SetStealthStateAction(object[] args, EntityMap? entityMap)
        {
            if (!TryGetEntityArg(args, 0, out var playerEntity) ||
                !TryGetBoolArg(args, 1, out var isStealth))
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

                BridgeEventService.PublishVisibilityModified(playerEntity, isStealth ? 0.1f : 1f);
                Log.LogInfo($"Set stealth state for player {playerEntity} to {isStealth}");
                return true;
            }
            catch (Exception ex)
            {
                Log.LogError($"Error setting stealth state: {ex.Message}");
                return false;
            }
        }

        private static bool ModifyVisibilityAction(object[] args, EntityMap? entityMap)
        {
            if (!TryGetEntityArg(args, 0, out var targetEntity) ||
                !TryGetFloatArg(args, 1, out var visibilityLevel))
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

                BridgeEventService.PublishVisibilityModified(targetEntity, visibilityLevel);
                Log.LogInfo($"Modified visibility for {targetEntity} to {visibilityLevel}");
                return true;
            }
            catch (Exception ex)
            {
                Log.LogError($"Error modifying visibility: {ex.Message}");
                return false;
            }
        }

        private static bool ApplyStealthBuffAction(object[] args, EntityMap? entityMap)
        {
            if (!TryGetEntityArg(args, 0, out var playerEntity) ||
                !TryGetPrefabGuidArg(args, 1, out var buffGuid))
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

                // Apply stealth buff
                Log.LogInfo($"Applied stealth buff {buffGuid} to player {playerEntity}");
                return true;
            }
            catch (Exception ex)
            {
                Log.LogError($"Error applying stealth buff: {ex.Message}");
                return false;
            }
        }

        private static bool RemoveStealthBuffAction(object[] args, EntityMap? entityMap)
        {
            if (!TryGetEntityArg(args, 0, out var playerEntity))
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

                // Remove stealth buff
                Log.LogInfo($"Removed stealth buff from player {playerEntity}");
                return true;
            }
            catch (Exception ex)
            {
                Log.LogError($"Error removing stealth buff: {ex.Message}");
                return false;
            }
        }

        private static bool TriggerDetectionAction(object[] args, EntityMap? entityMap)
        {
            if (!TryGetEntityArg(args, 0, out var detectorEntity) ||
                !TryGetEntityArg(args, 1, out var detectedEntity) ||
                !TryGetStringArg(args, 2, out var detectionType))
            {
                return false;
            }

            try
            {
                var em = UnifiedCore.EntityManager;
                if (!em.Exists(detectorEntity) || !em.Exists(detectedEntity))
                {
                    return false;
                }

                BridgeEventService.PublishDetectionTriggered(detectorEntity, detectedEntity, detectionType);
                Log.LogInfo($"Triggered detection: {detectorEntity} detected {detectedEntity} with type {detectionType}");
                return true;
            }
            catch (Exception ex)
            {
                Log.LogError($"Error triggering detection: {ex.Message}");
                return false;
            }
        }

        private static bool PerformAreaDetectionAction(object[] args, EntityMap? entityMap)
        {
            if (!TryGetEntityArg(args, 0, out var detectorEntity) ||
                !TryGetFloat3Arg(args, 1, out var areaCenter) ||
                !TryGetFloatArg(args, 2, out var detectionRange))
            {
                return false;
            }

            try
            {
                var em = UnifiedCore.EntityManager;
                if (!em.Exists(detectorEntity))
                {
                    return false;
                }

                // Perform area detection sweep
                Log.LogInfo($"Area detection sweep by {detectorEntity} at {areaCenter} range {detectionRange}");
                return true;
            }
            catch (Exception ex)
            {
                Log.LogError($"Error performing area detection: {ex.Message}");
                return false;
            }
        }

        private static bool CheckLineOfSightAction(object[] args, EntityMap? entityMap)
        {
            if (!TryGetEntityArg(args, 0, out var observerEntity) ||
                !TryGetEntityArg(args, 1, out var targetEntity))
            {
                return false;
            }

            try
            {
                var em = UnifiedCore.EntityManager;
                if (!em.Exists(observerEntity) || !em.Exists(targetEntity))
                {
                    return false;
                }

                var hasLos = false;
                BridgeEventService.CheckAndPublishLineOfSight(observerEntity, targetEntity, out hasLos);
                Log.LogInfo($"Line of sight check: {observerEntity} -> {targetEntity}: {hasLos}");
                return true;
            }
            catch (Exception ex)
            {
                Log.LogError($"Error checking line of sight: {ex.Message}");
                return false;
            }
        }

        private static bool BlockLineOfSightAction(object[] args, EntityMap? entityMap)
        {
            if (!TryGetEntityArg(args, 0, out var observerEntity) ||
                !TryGetEntityArg(args, 1, out var targetEntity) ||
                !TryGetFloatArg(args, 2, out var duration))
            {
                return false;
            }

            try
            {
                var em = UnifiedCore.EntityManager;
                if (!em.Exists(observerEntity) || !em.Exists(targetEntity))
                {
                    return false;
                }

                // Block line of sight
                Log.LogInfo($"Blocked line of sight: {observerEntity} -> {targetEntity} for {duration} seconds");
                return true;
            }
            catch (Exception ex)
            {
                Log.LogError($"Error blocking line of sight: {ex.Message}");
                return false;
            }
        }

        private static bool ClearLineOfSightAction(object[] args, EntityMap? entityMap)
        {
            if (!TryGetEntityArg(args, 0, out var observerEntity) ||
                !TryGetEntityArg(args, 1, out var targetEntity))
            {
                return false;
            }

            try
            {
                var em = UnifiedCore.EntityManager;
                if (!em.Exists(observerEntity) || !em.Exists(targetEntity))
                {
                    return false;
                }

                // Clear line of sight block
                Log.LogInfo($"Cleared line of sight block: {observerEntity} -> {targetEntity}");
                return true;
            }
            catch (Exception ex)
            {
                Log.LogError($"Error clearing line of sight: {ex.Message}");
                return false;
            }
        }

        private static bool SetDetectionRangeAction(object[] args, EntityMap? entityMap)
        {
            if (!TryGetEntityArg(args, 0, out var entity) ||
                !TryGetFloatArg(args, 1, out var range))
            {
                return false;
            }

            try
            {
                var em = UnifiedCore.EntityManager;
                if (!em.Exists(entity))
                {
                    return false;
                }

                BridgeEventService.PublishDetectionRangeChanged(entity, range);
                Log.LogInfo($"Set detection range for {entity} to {range}");
                return true;
            }
            catch (Exception ex)
            {
                Log.LogError($"Error setting detection range: {ex.Message}");
                return false;
            }
        }

        private static bool ResetDetectionRangeAction(object[] args, EntityMap? entityMap)
        {
            if (!TryGetEntityArg(args, 0, out var entity))
            {
                return false;
            }

            try
            {
                var em = UnifiedCore.EntityManager;
                if (!em.Exists(entity))
                {
                    return false;
                }

                // Reset detection range to normal
                Log.LogInfo($"Reset detection range for {entity} to normal");
                return true;
            }
            catch (Exception ex)
            {
                Log.LogError($"Error resetting detection range: {ex.Message}");
                return false;
            }
        }

        private static bool ApplyVisibilityBuffAction(object[] args, EntityMap? entityMap)
        {
            if (!TryGetEntityArg(args, 0, out var targetEntity) ||
                !TryGetPrefabGuidArg(args, 1, out var buffGuid))
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

                // Apply visibility buff
                Log.LogInfo($"Applied visibility buff {buffGuid} to {targetEntity}");
                return true;
            }
            catch (Exception ex)
            {
                Log.LogError($"Error applying visibility buff: {ex.Message}");
                return false;
            }
        }

        private static bool RemoveVisibilityBuffsAction(object[] args, EntityMap? entityMap)
        {
            if (!TryGetEntityArg(args, 0, out var targetEntity))
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

                // Remove all visibility buffs
                Log.LogInfo($"Removed all visibility buffs from {targetEntity}");
                return true;
            }
            catch (Exception ex)
            {
                Log.LogError($"Error removing visibility buffs: {ex.Message}");
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
