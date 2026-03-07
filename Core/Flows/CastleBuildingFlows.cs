using System.Collections.Generic;
using VAutomationCore.Core.Api;
using VAutomationCore.Core.Services;

namespace VAutomationCore.Core.Flows
{
    /// <summary>
    /// Castle building flows for managing building attachments, buffs, and castle mechanics.
    /// Handles building construction, attachment management, castle enhancements, and building interactions.
    /// </summary>
    public static class CastleBuildingFlows
    {
        /// <summary>
        /// Register all castle building flows with the FlowService.
        /// </summary>
        public static void RegisterCastleBuildingFlows()
        {
            // Building attachment flows
            FlowService.RegisterFlow("castle_attachment_add", new[]
            {
                new FlowStep("add_building_attachment", "@building", "@attachment"),
                new FlowStep("apply_attachment_buff", "@building", "@attachment", "@buff"),
                new FlowStep("sendmessagetouser", "@player", "Attachment added: @attachment_name to @building_name"),
                new FlowStep("progress_achievement", "@player", "attachments_added", 1)
            });

            FlowService.RegisterFlow("castle_attachment_remove", new[]
            {
                new FlowStep("remove_building_attachment", "@building", "@attachment"),
                new FlowStep("remove_attachment_buff", "@building", "@buff"),
                new FlowStep("sendmessagetouser", "@player", "Attachment removed: @attachment_name from @building_name"),
                new FlowStep("progress_achievement", "@player", "attachments_removed", 1)
            });

            FlowService.RegisterFlow("castle_attachment_clear_all", new[]
            {
                new FlowStep("clear_building_attachments", "@building"),
                new FlowStep("remove_all_attachment_buffs", "@building"),
                new FlowStep("sendmessagetouser", "@player", "All attachments cleared from @building_name"),
                new FlowStep("progress_achievement", "@player", "attachments_cleared", 1)
            });

            FlowService.RegisterFlow("castle_attachment_upgrade", new[]
            {
                new FlowStep("upgrade_building_attachment", "@building", "@attachment", "@upgrade_materials"),
                new FlowStep("update_attachment_buff", "@building", "@new_buff"),
                new FlowStep("sendmessagetouser", "@player", "Attachment upgraded: @attachment_name"),
                new FlowStep("progress_achievement", "@player", "attachments_upgraded", 1)
            });

            // Building buff flows
            FlowService.RegisterFlow("castle_buff_apply", new[]
            {
                new FlowStep("apply_building_buff", "@building", "@buff", "@source"),
                new FlowStep("update_building_stats", "@building", "@buff_effects"),
                new FlowStep("sendmessagetouser", "@player", "Buff applied: @buff_name to @building_name"),
                new FlowStep("progress_achievement", "@player", "building_buffs_applied", 1)
            });

            FlowService.RegisterFlow("castle_buff_remove", new[]
            {
                new FlowStep("remove_building_buff", "@building", "@buff"),
                new FlowStep("restore_building_stats", "@building"),
                new FlowStep("sendmessagetouser", "@player", "Buff removed: @buff_name from @building_name"),
                new FlowStep("progress_achievement", "@player", "building_buffs_removed", 1)
            });

            FlowService.RegisterFlow("castle_buff_update", new[]
            {
                new FlowStep("update_building_buff", "@building", "@buff", "@new_value"),
                new FlowStep("refresh_building_stats", "@building"),
                new FlowStep("sendmessagetouser", "@player", "Buff updated: @buff_name to @new_value"),
                new FlowStep("progress_achievement", "@player", "building_buffs_updated", 1)
            });

            FlowService.RegisterFlow("castle_buff_clear_all", new[]
            {
                new FlowStep("clear_building_buffs", "@building"),
                new FlowStep("reset_building_stats", "@building"),
                new FlowStep("sendmessagetouser", "@player", "All buffs cleared from @building_name"),
                new FlowStep("progress_achievement", "@player", "building_buffs_cleared", 1)
            });

            // Castle construction flows
            FlowService.RegisterFlow("castle_construct", new[]
            {
                new FlowStep("place_building_foundation", "@player", "@building_type", "@position"),
                new FlowStep("add_building_attachments", "@building", "@default_attachments"),
                new FlowStep("apply_initial_buffs", "@building", "@construction_buffs"),
                new FlowStep("sendmessagetouser", "@player", "Castle constructed: @building_name"),
                new FlowStep("progress_achievement", "@player", "castles_constructed", 1)
            });

            FlowService.RegisterFlow("castle_deconstruct", new[]
            {
                new FlowStep("remove_building_attachments", "@building"),
                new FlowStep("clear_building_buffs", "@building"),
                new FlowStep("remove_building_foundation", "@building"),
                new FlowStep("sendmessagetouser", "@player", "Castle deconstructed: @building_name"),
                new FlowStep("progress_achievement", "@player", "castles_deconstructed", 1)
            });

            FlowService.RegisterFlow("castle_repair", new[]
            {
                new FlowStep("repair_building_foundation", "@building", "@repair_materials"),
                new FlowStep("restore_building_health", "@building", "@health_amount"),
                new FlowStep("reactivate_building_buffs", "@building"),
                new FlowStep("sendmessagetouser", "@player", "Castle repaired: @building_name"),
                new FlowStep("progress_achievement", "@player", "castles_repaired", 1)
            });

            // Castle upgrade flows
            FlowService.RegisterFlow("castle_upgrade_tier", new[]
            {
                new FlowStep("upgrade_castle_tier", "@castle", "@new_tier", "@upgrade_materials"),
                new FlowStep("add_tier_attachments", "@castle", "@tier_attachments"),
                new FlowStep("apply_tier_buffs", "@castle", "@tier_buffs"),
                new FlowStep("sendmessagetouser", "@player", "Castle upgraded to tier @new_tier"),
                new FlowStep("progress_achievement", "@player", "castle_upgrades", 1)
            });

            FlowService.RegisterFlow("castle_upgrade_defense", new[]
            {
                new FlowStep("add_defense_attachments", "@castle", "@defense_attachments"),
                new FlowStep("apply_defense_buffs", "@castle", "@defense_buffs"),
                new FlowStep("increase_defense_rating", "@castle", "@defense_bonus"),
                new FlowStep("sendmessagetouser", "@player", "Castle defense upgraded: @defense_rating"),
                new FlowStep("progress_achievement", "@player", "defense_upgrades", 1)
            });

            FlowService.RegisterFlow("castle_upgrade_production", new[]
            {
                new FlowStep("add_production_attachments", "@castle", "@production_attachments"),
                new FlowStep("apply_production_buffs", "@castle", "@production_buffs"),
                new FlowStep("increase_production_rate", "@castle", "@production_bonus"),
                new FlowStep("sendmessagetouser", "@player", "Castle production upgraded: @production_rate"),
                new FlowStep("progress_achievement", "@player", "production_upgrades", 1)
            });

            // Castle management flows
            FlowService.RegisterFlow("castle_manage_permissions", new[]
            {
                new FlowStep("set_castle_permissions", "@castle", "@player", "@permission_level"),
                new FlowStep("apply_permission_buffs", "@castle", "@permission_buffs"),
                new FlowStep("sendmessagetouser", "@player", "Castle permissions set: @permission_level"),
                new FlowStep("progress_achievement", "@player", "permissions_set", 1)
            });

            FlowService.RegisterFlow("castle_claim", new[]
            {
                new FlowStep("claim_castle", "@castle", "@player", "@claim_type"),
                new FlowStep("apply_ownership_buffs", "@castle", "@ownership_buffs"),
                new FlowStep("set_castle_name", "@castle", "@castle_name"),
                new FlowStep("sendmessagetouser", "@player", "Castle claimed: @castle_name"),
                new FlowStep("progress_achievement", "@player", "castles_claimed", 1)
            });

            FlowService.RegisterFlow("castle_abandon", new[]
            {
                new FlowStep("remove_ownership_buffs", "@castle"),
                new FlowStep("clear_castle_permissions", "@castle"),
                new FlowStep("reset_castle_name", "@castle"),
                new FlowStep("sendmessagetouser", "@player", "Castle abandoned: @castle_name"),
                new FlowStep("progress_achievement", "@player", "castles_abandoned", 1)
            });

            // Admin castle flows
            FlowService.RegisterFlow("castle_admin_build", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("instant_build_castle", "@target_location", "@castle_type"),
                new FlowStep("apply_admin_buffs", "@castle", "@admin_buffs"),
                new FlowStep("sendmessagetouser", "@requesting_admin", "Admin castle built at @target_location"),
                new FlowStep("sendmessagetoall", "Admin castle constructed: @castle_name")
            });

            FlowService.RegisterFlow("castle_admin_destroy", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("instant_destroy_castle", "@target_castle"),
                new FlowStep("cleanup_castle_remains", "@target_location"),
                new FlowStep("sendmessagetouser", "@requesting_admin", "Admin castle destroyed: @castle_name"),
                new FlowStep("sendmessagetoall", "Admin castle destroyed: @castle_name")
            });

            FlowService.RegisterFlow("castle_admin_upgrade_all", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("upgrade_all_castles", "@all_castles", "@target_tier"),
                new FlowStep("apply_admin_upgrades", "@all_castles", "@admin_upgrades"),
                new FlowStep("sendmessagetouser", "@requesting_admin", "Admin upgraded all castles to tier @target_tier"),
                new FlowStep("sendmessagetoall", "Admin upgraded all castles to tier @target_tier")
            });

            // Castle status flows
            FlowService.RegisterFlow("castle_status", new[]
            {
                new FlowStep("check_castle_status", "@castle"),
                new FlowStep("check_building_attachments", "@castle"),
                new FlowStep("check_building_buffs", "@castle"),
                new FlowStep("sendmessagetouser", "@player", "Castle status: @castle_info"),
                new FlowStep("progress_achievement", "@player", "castle_checks", 1)
            });

            FlowService.RegisterFlow("castle_list", new[]
            {
                new FlowStep("get_player_castles", "@player"),
                new FlowStep("format_castle_list", "@castles"),
                new FlowStep("sendmessagetouser", "@player", "Your castles: @castle_list"),
                new FlowStep("progress_achievement", "@player", "castle_lists", 1)
            });

            // Castle defense flows
            FlowService.RegisterFlow("castle_defense_activate", new[]
            {
                new FlowStep("activate_castle_defenses", "@castle"),
                new FlowStep("apply_defense_buffs", "@castle", "@defense_buffs"),
                new FlowStep("spawn_defense_fx", "@castle", "@defense_fx"),
                new FlowStep("sendmessagetouser", "@player", "Castle defenses activated"),
                new FlowStep("progress_achievement", "@player", "defenses_activated", 1)
            });

            FlowService.RegisterFlow("castle_defense_deactivate", new[]
            {
                new FlowStep("deactivate_castle_defenses", "@castle"),
                new FlowStep("remove_defense_buffs", "@castle"),
                new FlowStep("stop_defense_fx", "@castle"),
                new FlowStep("sendmessagetouser", "@player", "Castle defenses deactivated"),
                new FlowStep("progress_achievement", "@player", "defenses_deactivated", 1)
            });

            // Castle production flows
            FlowService.RegisterFlow("castle_production_start", new[]
            {
                new FlowStep("start_castle_production", "@castle", "@production_type"),
                new FlowStep("apply_production_buffs", "@castle", "@production_buffs"),
                new FlowStep("spawn_production_fx", "@castle", "@production_fx"),
                new FlowStep("sendmessagetouser", "@player", "Castle production started: @production_type"),
                new FlowStep("progress_achievement", "@player", "production_started", 1)
            });

            FlowService.RegisterFlow("castle_production_stop", new[]
            {
                new FlowStep("stop_castle_production", "@castle"),
                new FlowStep("remove_production_buffs", "@castle"),
                new FlowStep("stop_production_fx", "@castle"),
                new FlowStep("sendmessagetouser", "@player", "Castle production stopped"),
                new FlowStep("progress_achievement", "@player", "production_stopped", 1)
            });
        }

        /// <summary>
        /// Get all castle building flow names for registration.
        /// </summary>
        public static string[] GetCastleBuildingFlowNames()
        {
            return new[]
            {
                "castle_attachment_add", "castle_attachment_remove", "castle_attachment_clear_all", "castle_attachment_upgrade",
                "castle_buff_apply", "castle_buff_remove", "castle_buff_update", "castle_buff_clear_all",
                "castle_construct", "castle_deconstruct", "castle_repair",
                "castle_upgrade_tier", "castle_upgrade_defense", "castle_upgrade_production",
                "castle_manage_permissions", "castle_claim", "castle_abandon",
                "castle_admin_build", "castle_admin_destroy", "castle_admin_upgrade_all",
                "castle_status", "castle_list",
                "castle_defense_activate", "castle_defense_deactivate",
                "castle_production_start", "castle_production_stop"
            };
        }
    }
}
