using System.Collections.Generic;
using VAutomationCore.Core.Api;
using VAutomationCore.Core.Services;

namespace VAutomationCore.Core.Flows
{
    /// <summary>
    /// Placement restriction flows for managing building placement rules and restrictions.
    /// Critical for proper map spawning mechanics: disable placement → spawn FX/buff → enable placement.
    /// </summary>
    public static class PlacementRestrictionFlows
    {
        /// <summary>
        /// Register all placement restriction flows with the FlowService.
        /// </summary>
        public static void RegisterPlacementRestrictionFlows()
        {
            // Core placement restriction flow - MOST IMPORTANT
            FlowService.RegisterFlow("placement_restriction_critical", new[]
            {
                new FlowStep("placement_disable_all", "@player"),
                new FlowStep("spawn_placement_fx", "@player", "@fx_guid", "@position"),
                new FlowStep("apply_placement_buff", "@player", "@buff_guid"),
                new FlowStep("wait_for_fx_complete", "@player", "@fx_duration"),
                new FlowStep("placement_enable_all", "@player"),
                new FlowStep("sendmessagetouser", "@player", "Placement restriction completed: @item_name spawned"),
                new FlowStep("progress_achievement", "@player", "placements_restricted", 1)
            });

            // Emergency placement override
            FlowService.RegisterFlow("placement_emergency_override", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("placement_disable_all", "@target_player"),
                new FlowStep("spawn_emergency_fx", "@target_player", "@emergency_fx", "@position"),
                new FlowStep("apply_emergency_buff", "@target_player", "@emergency_buff"),
                new FlowStep("placement_enable_all", "@target_player"),
                new FlowStep("sendmessagetouser", "@target_player", "Emergency placement override applied"),
                new FlowStep("sendmessagetouser", "@requesting_admin", "Emergency override completed for @target_player_name")
            });

            // Zone-based placement restriction
            FlowService.RegisterFlow("placement_zone_restriction", new[]
            {
                new FlowStep("placement_disable_zone", "@zone"),
                new FlowStep("spawn_zone_fx", "@zone", "@zone_fx_guid"),
                new FlowStep("apply_zone_buff", "@zone", "@zone_buff_guid"),
                new FlowStep("wait_for_zone_fx", "@zone", "@fx_duration"),
                new FlowStep("placement_enable_zone", "@zone"),
                new FlowStep("sendmessagetoall", "Zone placement restriction completed: @zone_name"),
                new FlowStep("progress_achievement", "@all_players", "zone_restrictions", 1)
            });

            // Individual player placement restriction
            FlowService.RegisterFlow("placement_player_restriction", new[]
            {
                new FlowStep("placement_disable_player", "@player"),
                new FlowStep("spawn_player_fx", "@player", "@player_fx_guid", "@player_position"),
                new FlowStep("apply_player_buff", "@player", "@player_buff_guid"),
                new FlowStep("wait_for_player_fx", "@player", "@fx_duration"),
                new FlowStep("placement_enable_player", "@player"),
                new FlowStep("sendmessagetouser", "@player", "Personal placement restriction completed"),
                new FlowStep("progress_achievement", "@player", "personal_restrictions", 1)
            });

            // Global placement restriction
            FlowService.RegisterFlow("placement_global_restriction", new[]
            {
                new FlowStep("placement_disable_global"),
                new FlowStep("spawn_global_fx", "@global_fx_guid", "@positions"),
                new FlowStep("apply_global_buff", "@all_players", "@global_buff_guid"),
                new FlowStep("wait_for_global_fx", "@global_fx_duration"),
                new FlowStep("placement_enable_global"),
                new FlowStep("sendmessagetoall", "Global placement restriction completed"),
                new FlowStep("progress_achievement", "@all_players", "global_restrictions", 1)
            });

            // Temporary placement restriction
            FlowService.RegisterFlow("placement_temporary_restriction", new[]
            {
                new FlowStep("placement_disable_all", "@player"),
                new FlowStep("spawn_temp_fx", "@player", "@temp_fx_guid", "@position"),
                new FlowStep("apply_temp_buff", "@player", "@temp_buff_guid"),
                new FlowStep("wait_for_temp_fx", "@player", "@temp_duration"),
                new FlowStep("placement_enable_all", "@player"),
                new FlowStep("sendmessagetouser", "@player", "Temporary placement restriction completed"),
                new FlowStep("progress_achievement", "@player", "temp_restrictions", 1)
            });

            // Placement restriction with validation
            FlowService.RegisterFlow("placement_restriction_with_validation", new[]
            {
                new FlowStep("validate_placement_area", "@player", "@area"),
                new FlowStep("placement_disable_area", "@player", "@area"),
                new FlowStep("spawn_validation_fx", "@player", "@validation_fx", "@area_center"),
                new FlowStep("apply_validation_buff", "@player", "@validation_buff"),
                new FlowStep("wait_for_validation", "@player", "@validation_duration"),
                new FlowStep("placement_enable_area", "@player", "@area"),
                new FlowStep("sendmessagetouser", "@player", "Validated placement restriction completed in @area_name"),
                new FlowStep("progress_achievement", "@player", "validated_restrictions", 1)
            });

            // Safe placement restriction (with rollback)
            FlowService.RegisterFlow("placement_safe_restriction", new[]
            {
                new FlowStep("backup_placement_state", "@player"),
                new FlowStep("placement_disable_all", "@player"),
                new FlowStep("spawn_safe_fx", "@player", "@safe_fx_guid", "@position"),
                new FlowStep("apply_safe_buff", "@player", "@safe_buff_guid"),
                new FlowStep("validate_safe_spawn", "@player", "@spawned_item"),
                new FlowStep("wait_for_safe_fx", "@player", "@safe_fx_duration"),
                new FlowStep("placement_enable_all", "@player"),
                new FlowStep("restore_placement_state", "@player"),
                new FlowStep("sendmessagetouser", "@player", "Safe placement restriction completed"),
                new FlowStep("progress_achievement", "@player", "safe_restrictions", 1)
            });

            // Placement restriction failure recovery
            FlowService.RegisterFlow("placement_restriction_recovery", new[]
            {
                new FlowStep("detect_placement_failure", "@player", "@failure_reason"),
                new FlowStep("placement_disable_all", "@player"),
                new FlowStep("spawn_recovery_fx", "@player", "@recovery_fx_guid", "@position"),
                new FlowStep("apply_recovery_buff", "@player", "@recovery_buff_guid"),
                new FlowStep("clear_failed_placement", "@player", "@failed_position"),
                new FlowStep("wait_for_recovery_fx", "@player", "@recovery_duration"),
                new FlowStep("placement_enable_all", "@player"),
                new FlowStep("sendmessagetouser", "@player", "Placement failure recovered: @failure_reason"),
                new FlowStep("progress_achievement", "@player", "placement_recoveries", 1)
            });

            // Admin placement restriction control
            FlowService.RegisterFlow("placement_admin_control", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("placement_disable_all", "@target_players"),
                new FlowStep("spawn_admin_fx", "@target_players", "@admin_fx_guid"),
                new FlowStep("apply_admin_buff", "@target_players", "@admin_buff_guid"),
                new FlowStep("wait_for_admin_fx", "@target_players", "@admin_fx_duration"),
                new FlowStep("placement_enable_all", "@target_players"),
                new FlowStep("sendmessagetouser", "@requesting_admin", "Admin placement control completed"),
                new FlowStep("sendmessagetoall", "Admin placement control applied to all players")
            });

            // Placement restriction status check
            FlowService.RegisterFlow("placement_restriction_status", new[]
            {
                new FlowStep("check_placement_status", "@player"),
                new FlowStep("sendmessagetouser", "@player", "Placement status: @status_info"),
                new FlowStep("progress_achievement", "@player", "placement_checks", 1)
            });

            // Placement restriction cleanup
            FlowService.RegisterFlow("placement_restriction_cleanup", new[]
            {
                new FlowStep("placement_disable_all", "@all_players"),
                new FlowStep("cleanup_placement_fx", "@all_players"),
                new FlowStep("cleanup_placement_buffs", "@all_players"),
                new FlowStep("placement_enable_all", "@all_players"),
                new FlowStep("sendmessagetoall", "Placement restriction cleanup completed"),
                new FlowStep("progress_achievement", "@all_players", "placement_cleanups", 1)
            });
        }

        /// <summary>
        /// Get all placement restriction flow names for registration.
        /// </summary>
        public static string[] GetPlacementRestrictionFlowNames()
        {
            return new[]
            {
                "placement_restriction_critical", "placement_emergency_override", "placement_zone_restriction",
                "placement_player_restriction", "placement_global_restriction", "placement_temporary_restriction",
                "placement_restriction_with_validation", "placement_safe_restriction", "placement_restriction_recovery",
                "placement_admin_control", "placement_restriction_status", "placement_restriction_cleanup"
            };
        }
    }
}
