using System.Collections.Generic;
using VAutomationCore.Core.Api;
using VAutomationCore.Core.Services;

namespace VAutomationCore.Core.Flows
{
    /// <summary>
    /// Visibility and stealth flows for managing player detection, stealth mechanics, and visual gameplay.
    /// Handles line-of-sight, visibility ranges, detection events, and stealth state management.
    /// </summary>
    public static class VisibilityAndStealthFlows
    {
        /// <summary>
        /// Register all visibility and stealth flows with the FlowService.
        /// </summary>
        public static void RegisterVisibilityAndStealthFlows()
        {
            // Stealth state flows
            FlowService.RegisterFlow("stealth_enter", new[]
            {
                new FlowStep("set_stealth_state", "@player", true),
                new FlowStep("modify_visibility", "@player", "@stealth_visibility"),
                new FlowStep("apply_stealth_buff", "@player", "@stealth_buff"),
                new FlowStep("sendmessagetouser", "@player", "Entered stealth mode"),
                new FlowStep("progress_achievement", "@player", "stealth_entries", 1)
            });

            FlowService.RegisterFlow("stealth_exit", new[]
            {
                new FlowStep("set_stealth_state", "@player", false),
                new FlowStep("restore_visibility", "@player", "@normal_visibility"),
                new FlowStep("remove_stealth_buff", "@player"),
                new FlowStep("sendmessagetouser", "@player", "Exited stealth mode"),
                new FlowStep("progress_achievement", "@player", "stealth_exits", 1)
            });

            FlowService.RegisterFlow("stealth_broken", new[]
            {
                new FlowStep("set_stealth_state", "@player", false),
                new FlowStep("restore_visibility", "@player", "@normal_visibility"),
                new FlowStep("remove_stealth_buff", "@player"),
                new FlowStep("sendmessagetouser", "@player", "Stealth broken by @detection_source"),
                new FlowStep("progress_achievement", "@player", "stealth_broken", 1)
            });

            // Detection flows
            FlowService.RegisterFlow("detection_player_detected", new[]
            {
                new FlowStep("trigger_detection", "@detector", "@detected_player", "@detection_type"),
                new FlowStep("break_stealth", "@detected_player"),
                new FlowStep("sendmessagetouser", "@detected_player", "Detected by @detector_name!"),
                new FlowStep("sendmessagetouser", "@detector", "You detected @detected_player_name"),
                new FlowStep("progress_achievement", "@detector", "players_detected", 1)
            });

            FlowService.RegisterFlow("detection_area_sweep", new[]
            {
                new FlowStep("perform_area_detection", "@detector", "@area_center", "@detection_range"),
                new FlowStep("check_line_of_sight", "@detector", "@all_targets"),
                new FlowStep("trigger_detections", "@detector", "@detected_entities"),
                new FlowStep("sendmessagetouser", "@detector", "Area sweep complete: @detection_count entities detected"),
                new FlowStep("progress_achievement", "@detector", "area_sweeps", 1)
            });

            FlowService.RegisterFlow("detection_perimeter_check", new[]
            {
                new FlowStep("set_detection_perimeter", "@detector", "@perimeter_center", "@perimeter_radius"),
                new FlowStep("monitor_perimeter_crossings", "@detector", "@perimeter_duration"),
                new FlowStep("trigger_perimeter_alerts", "@detector", "@crossing_entities"),
                new FlowStep("sendmessagetouser", "@detector", "Perimeter monitoring active"),
                new FlowStep("progress_achievement", "@detector", "perimeter_checks", 1)
            });

            // Visibility modification flows
            FlowService.RegisterFlow("visibility_reduce", new[]
            {
                new FlowStep("modify_visibility", "@target", "@reduced_visibility"),
                new FlowStep("apply_visibility_buff", "@target", "@reduction_buff"),
                new FlowStep("sendmessagetouser", "@target", "Visibility reduced to @visibility_level"),
                new FlowStep("progress_achievement", "@target", "visibility_reduced", 1)
            });

            FlowService.RegisterFlow("visibility_increase", new[]
            {
                new FlowStep("modify_visibility", "@target", "@increased_visibility"),
                new FlowStep("apply_visibility_buff", "@target", "@increase_buff"),
                new FlowStep("sendmessagetouser", "@target", "Visibility increased to @visibility_level"),
                new FlowStep("progress_achievement", "@target", "visibility_increased", 1)
            });

            FlowService.RegisterFlow("visibility_restore", new[]
            {
                new FlowStep("restore_visibility", "@target", "@normal_visibility"),
                new FlowStep("remove_visibility_buffs", "@target"),
                new FlowStep("sendmessagetouser", "@target", "Visibility restored to normal"),
                new FlowStep("progress_achievement", "@target", "visibility_restored", 1)
            });

            // Line of sight flows
            FlowService.RegisterFlow("los_check", new[]
            {
                new FlowStep("check_line_of_sight", "@observer", "@target"),
                new FlowStep("sendmessagetouser", "@observer", "Line of sight to @target_name: @has_los"),
                new FlowStep("progress_achievement", "@observer", "los_checks", 1)
            });

            FlowService.RegisterFlow("los_block", new[]
            {
                new FlowStep("block_line_of_sight", "@observer", "@target", "@duration"),
                new FlowStep("apply_los_block_buff", "@observer", "@block_buff"),
                new FlowStep("sendmessagetouser", "@observer", "Line of sight to @target_name blocked for @duration seconds"),
                new FlowStep("progress_achievement", "@observer", "los_blocks", 1)
            });

            FlowService.RegisterFlow("los_clear", new[]
            {
                new FlowStep("clear_line_of_sight", "@observer", "@target"),
                new FlowStep("remove_los_block_buff", "@observer"),
                new FlowStep("sendmessagetouser", "@observer", "Line of sight to @target_name cleared"),
                new FlowStep("progress_achievement", "@observer", "los_clears", 1)
            });

            // Detection range flows
            FlowService.RegisterFlow("detection_range_increase", new[]
            {
                new FlowStep("set_detection_range", "@entity", "@increased_range"),
                new FlowStep("apply_range_buff", "@entity", "@range_buff"),
                new FlowStep("sendmessagetouser", "@entity", "Detection range increased to @range_value"),
                new FlowStep("progress_achievement", "@entity", "range_increases", 1)
            });

            FlowService.RegisterFlow("detection_range_decrease", new[]
            {
                new FlowStep("set_detection_range", "@entity", "@decreased_range"),
                new FlowStep("remove_range_buff", "@entity"),
                new FlowStep("sendmessagetouser", "@entity", "Detection range decreased to @range_value"),
                new FlowStep("progress_achievement", "@entity", "range_decreases", 1)
            });

            FlowService.RegisterFlow("detection_range_reset", new[]
            {
                new FlowStep("reset_detection_range", "@entity"),
                new FlowStep("remove_all_range_buffs", "@entity"),
                new FlowStep("sendmessagetouser", "@entity", "Detection range reset to normal"),
                new FlowStep("progress_achievement", "@entity", "range_resets", 1)
            });

            // Admin visibility flows
            FlowService.RegisterFlow("visibility_admin_global", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("set_global_visibility", "@visibility_level"),
                new FlowStep("apply_global_visibility_buff", "@all_players", "@admin_buff"),
                new FlowStep("sendmessagetoall", "Admin set global visibility to @visibility_level"),
                new FlowStep("sendmessagetouser", "@requesting_admin", "Global visibility set successfully")
            });

            FlowService.RegisterFlow("visibility_admin_player", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("set_player_visibility", "@target_player", "@visibility_level"),
                new FlowStep("apply_visibility_buff", "@target_player", "@admin_buff"),
                new FlowStep("sendmessagetouser", "@target_player", "Admin set your visibility to @visibility_level"),
                new FlowStep("sendmessagetouser", "@requesting_admin", "Visibility set for @target_player_name")
            });

            // Stealth equipment flows
            FlowService.RegisterFlow("stealth_equip_bonus", new[]
            {
                new FlowStep("equip_stealth_item", "@player", "@stealth_item"),
                new FlowStep("apply_stealth_bonus", "@player", "@item_bonus"),
                new FlowStep("modify_visibility", "@player", "@enhanced_stealth"),
                new FlowStep("sendmessagetouser", "@player", "Stealth bonus from @item_name equipped"),
                new FlowStep("progress_achievement", "@player", "stealth_equipment", 1)
            });

            FlowService.RegisterFlow("stealth_unequip_penalty", new[]
            {
                new FlowStep("unequip_stealth_item", "@player", "@stealth_item"),
                new FlowStep("remove_stealth_bonus", "@player", "@item_bonus"),
                new FlowStep("restore_visibility", "@player", "@normal_stealth"),
                new FlowStep("sendmessagetouser", "@player", "Stealth bonus from @item_name removed"),
                new FlowStep("progress_achievement", "@player", "stealth_equipment", 1)
            });

            // Environmental visibility flows
            FlowService.RegisterFlow("visibility_weather_affect", new[]
            {
                new FlowStep("modify_weather_visibility", "@zone", "@weather_type"),
                new FlowStep("apply_weather_buff", "@all_players", "@weather_buff"),
                new FlowStep("sendmessagetoall", "Weather @weather_name affects visibility"),
                new FlowStep("progress_achievement", "@all_players", "weather_visibility", 1)
            });

            FlowService.RegisterFlow("visibility_time_affect", new[]
            {
                new FlowStep("modify_time_visibility", "@zone", "@time_of_day"),
                new FlowStep("apply_time_buff", "@all_players", "@time_buff"),
                new FlowStep("sendmessagetoall", "Time @time_name affects visibility"),
                new FlowStep("progress_achievement", "@all_players", "time_visibility", 1)
            });

            // Stealth status flows
            FlowService.RegisterFlow("stealth_status", new[]
            {
                new FlowStep("check_stealth_state", "@player"),
                new FlowStep("check_visibility_level", "@player"),
                new FlowStep("sendmessagetouser", "@player", "Stealth status: @stealth_info"),
                new FlowStep("progress_achievement", "@player", "stealth_checks", 1)
            });

            FlowService.RegisterFlow("visibility_status", new[]
            {
                new FlowStep("check_visibility_state", "@player"),
                new FlowStep("check_detection_range", "@player"),
                new FlowStep("sendmessagetouser", "@player", "Visibility status: @visibility_info"),
                new FlowStep("progress_achievement", "@player", "visibility_checks", 1)
            });
        }

        /// <summary>
        /// Get all visibility and stealth flow names for registration.
        /// </summary>
        public static string[] GetVisibilityAndStealthFlowNames()
        {
            return new[]
            {
                "stealth_enter", "stealth_exit", "stealth_broken",
                "detection_player_detected", "detection_area_sweep", "detection_perimeter_check",
                "visibility_reduce", "visibility_increase", "visibility_restore",
                "los_check", "los_block", "los_clear",
                "detection_range_increase", "detection_range_decrease", "detection_range_reset",
                "visibility_admin_global", "visibility_admin_player",
                "stealth_equip_bonus", "stealth_unequip_penalty",
                "visibility_weather_affect", "visibility_time_affect",
                "stealth_status", "visibility_status"
            };
        }
    }
}
