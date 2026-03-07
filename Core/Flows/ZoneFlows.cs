using System.Collections.Generic;
using VAutomationCore.Core.Api;
using VAutomationCore.Core.Services;

namespace VAutomationCore.Core.Flows
{
    /// <summary>
    /// Zone flows for managing zone lifecycle, entry/exit, restrictions, environment, security, and monitoring.
    /// Organized with 14 logical groups for better maintainability.
    /// </summary>
    public static class ZoneFlows
    {
        /// <summary>
        /// Register all zone flows with the FlowService.
        /// </summary>
        public static void RegisterZoneFlows()
        {
            // ========================================
            // GROUP 1: ZONE LIFECYCLE
            // ========================================
            
            // Zone ready events - notify when zones are ready/initialized
            FlowService.RegisterFlow("zone_ready", new[]
            {
                new FlowStep("check_zone_ready", "@zone"),
                new FlowStep("trigger_zone_ready_events", "@zone"),
                new FlowStep("sendmessagetoall", "Zone ready: @zone_name"),
                new FlowStep("progress_achievement", "@all_players", "zones_ready", 1)
            });

            FlowService.RegisterFlow("zone_ready_all", new[]
            {
                new FlowStep("check_all_zones_ready", "@zone_type"),
                new FlowStep("trigger_all_ready_events", "@zone_type"),
                new FlowStep("sendmessagetoall", "All @zone_type zones ready"),
                new FlowStep("progress_achievement", "@all_players", "all_zones_ready", 1)
            });

            // Zone preset flows - save/load zone configurations
            FlowService.RegisterFlow("zone_preset_save", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("save_zone_preset", "@zone", "@preset_name", "@preset_params"),
                new FlowStep("store_preset_data", "@preset_name", "@preset_data"),
                new FlowStep("sendmessagetouser", "@requesting_admin", "Zone preset saved: @preset_name"),
                new FlowStep("progress_achievement", "@requesting_admin", "zone_presets_saved", 1)
            });

            FlowService.RegisterFlow("zone_preset_load", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("load_zone_preset", "@zone", "@preset_name"),
                new FlowStep("apply_preset_config", "@zone", "@preset_data"),
                new FlowStep("sendmessagetouser", "@requesting_admin", "Zone preset loaded: @preset_name"),
                new FlowStep("sendmessagetoall", "Zone @zone_name loaded preset: @preset_name"),
                new FlowStep("progress_achievement", "@requesting_admin", "zone_presets_loaded", 1)
            });

            FlowService.RegisterFlow("zone_preset_delete", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("delete_zone_preset", "@preset_name"),
                new FlowStep("remove_preset_data", "@preset_name"),
                new FlowStep("sendmessagetouser", "@requesting_admin", "Zone preset deleted: @preset_name"),
                new FlowStep("progress_achievement", "@requesting_admin", "zone_presets_deleted", 1)
            });

            FlowService.RegisterFlow("zone_preset_list", new[]
            {
                new FlowStep("list_zone_presets", "@preset_filter"),
                new FlowStep("format_preset_list", "@presets"),
                new FlowStep("sendmessagetouser", "@player", "Zone presets: @preset_list")
            });

            // Zone creation/deletion (admin)
            FlowService.RegisterFlow("zone_create", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("create_zone", "@zone_params", "@zone_type"),
                new FlowStep("apply_zone_defaults", "@zone", "@default_settings"),
                new FlowStep("sendmessagetouser", "@requesting_admin", "Zone created: @zone_name"),
                new FlowStep("sendmessagetoall", "New zone created: @zone_name")
            });

            FlowService.RegisterFlow("zone_delete", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("delete_zone", "@zone"),
                new FlowStep("cleanup_zone_data", "@zone"),
                new FlowStep("sendmessagetouser", "@requesting_admin", "Zone deleted: @zone_name"),
                new FlowStep("sendmessagetoall", "Zone removed: @zone_name")
            });

            FlowService.RegisterFlow("zone_rename", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("rename_zone", "@zone", "@new_zone_name"),
                new FlowStep("update_zone_indicators", "@zone"),
                new FlowStep("sendmessagetouser", "@requesting_admin", "Zone renamed to: @new_zone_name"),
                new FlowStep("sendmessagetoall", "Zone renamed: @zone_name -> @new_zone_name")
            });

            // ========================================
            // GROUP 2: ZONE ENTRY/EXIT/TRANSITION
            // ========================================

            FlowService.RegisterFlow("zone_enter", new[]
            {
                new FlowStep("detect_zone_entry", "@player", "@zone"),
                new FlowStep("apply_zone_entry_effects", "@player", "@zone"),
                new FlowStep("trigger_zone_entry_events", "@player", "@zone"),
                new FlowStep("sendmessagetouser", "@player", "Entered zone: @zone_name"),
                new FlowStep("progress_achievement", "@player", "zone_entries", 1)
            });

            FlowService.RegisterFlow("zone_exit", new[]
            {
                new FlowStep("detect_zone_exit", "@player", "@zone"),
                new FlowStep("apply_zone_exit_effects", "@player", "@zone"),
                new FlowStep("trigger_zone_exit_events", "@player", "@zone"),
                new FlowStep("sendmessagetouser", "@player", "Exited zone: @zone_name"),
                new FlowStep("progress_achievement", "@player", "zone_exits", 1)
            });

            FlowService.RegisterFlow("zone_transition", new[]
            {
                new FlowStep("detect_zone_transition", "@player", "@from_zone", "@to_zone"),
                new FlowStep("apply_transition_effects", "@player", "@transition_params"),
                new FlowStep("trigger_transition_events", "@player", "@from_zone", "@to_zone"),
                new FlowStep("sendmessagetouser", "@player", "Transitioned from @from_zone_name to @to_zone_name"),
                new FlowStep("progress_achievement", "@player", "zone_transitions", 1)
            });

            // ========================================
            // GROUP 3: ZONE RULES
            // ========================================

            FlowService.RegisterFlow("zone_rule_add", new[]
            {
                new FlowStep("add_zone_rule", "@zone", "@rule_type", "@rule_params"),
                new FlowStep("apply_rule_effects", "@zone", "@rule_effects"),
                new FlowStep("sendmessagetoall", "Zone rule added to @zone_name: @rule_type"),
                new FlowStep("progress_achievement", "@all_players", "zone_rules_added", 1)
            });

            FlowService.RegisterFlow("zone_rule_remove", new[]
            {
                new FlowStep("remove_zone_rule", "@zone", "@rule_type"),
                new FlowStep("remove_rule_effects", "@zone", "@rule_effects"),
                new FlowStep("sendmessagetoall", "Zone rule removed from @zone_name: @rule_type"),
                new FlowStep("progress_achievement", "@all_players", "zone_rules_removed", 1)
            });

            FlowService.RegisterFlow("zone_rule_update", new[]
            {
                new FlowStep("update_zone_rule", "@zone", "@rule_type", "@new_rule_params"),
                new FlowStep("update_rule_effects", "@zone", "@new_effects"),
                new FlowStep("sendmessagetoall", "Zone rule updated in @zone_name: @rule_type"),
                new FlowStep("progress_achievement", "@all_players", "zone_rules_updated", 1)
            });

            FlowService.RegisterFlow("zone_rule_check", new[]
            {
                new FlowStep("check_zone_rules", "@zone", "@rule_type"),
                new FlowStep("validate_rule_compliance", "@zone", "@rule_type"),
                new FlowStep("sendmessagetouser", "@player", "Zone rules checked: @rule_check_result"),
                new FlowStep("progress_achievement", "@player", "zone_rule_checks", 1)
            });

            // Zone rules - PVP/Building/Combat toggles
            FlowService.RegisterFlow("zone_rule_pvp_enable", new[]
            {
                new FlowStep("enable_pvp_rules", "@zone"),
                new FlowStep("remove_pvp_safety", "@zone"),
                new FlowStep("clear_pvp_indicators", "@zone"),
                new FlowStep("sendmessagetoall", "PVP rules enabled in @zone_name"),
                new FlowStep("progress_achievement", "@all_players", "pvp_rules_enabled", 1)
            });

            FlowService.RegisterFlow("zone_rule_pvp_disable", new[]
            {
                new FlowStep("disable_pvp_rules", "@zone"),
                new FlowStep("apply_pvp_safety", "@zone", "@pvp_safety_level"),
                new FlowStep("spawn_pvp_indicators", "@zone", "@pvp_disabled_visuals"),
                new FlowStep("sendmessagetoall", "PVP rules disabled in @zone_name"),
                new FlowStep("progress_achievement", "@all_players", "pvp_rules_disabled", 1)
            });

            FlowService.RegisterFlow("zone_rule_building_enable", new[]
            {
                new FlowStep("enable_building_rules", "@zone"),
                new FlowStep("remove_building_freedom", "@zone"),
                new FlowStep("clear_building_indicators", "@zone"),
                new FlowStep("sendmessagetoall", "Building rules enabled in @zone_name"),
                new FlowStep("progress_achievement", "@all_players", "building_rules_enabled", 1)
            });

            FlowService.RegisterFlow("zone_rule_building_disable", new[]
            {
                new FlowStep("disable_building_rules", "@zone"),
                new FlowStep("apply_building_freedom", "@zone", "@building_freedom_level"),
                new FlowStep("spawn_building_indicators", "@zone", "@building_freedom_visuals"),
                new FlowStep("sendmessagetoall", "Building rules disabled in @zone_name"),
                new FlowStep("progress_achievement", "@all_players", "building_rules_disabled", 1)
            });

            FlowService.RegisterFlow("zone_rule_combat_enable", new[]
            {
                new FlowStep("enable_combat_rules", "@zone"),
                new FlowStep("remove_combat_peace", "@zone"),
                new FlowStep("clear_combat_indicators", "@zone"),
                new FlowStep("sendmessagetoall", "Combat rules enabled in @zone_name"),
                new FlowStep("progress_achievement", "@all_players", "combat_rules_enabled", 1)
            });

            FlowService.RegisterFlow("zone_rule_combat_disable", new[]
            {
                new FlowStep("disable_combat_rules", "@zone"),
                new FlowStep("apply_combat_peace", "@zone", "@combat_peace_level"),
                new FlowStep("spawn_combat_indicators", "@zone", "@combat_peace_visuals"),
                new FlowStep("sendmessagetoall", "Combat rules disabled in @zone_name"),
                new FlowStep("progress_achievement", "@all_players", "combat_rules_disabled", 1)
            });

            // Zone rules override
            FlowService.RegisterFlow("zone_rule_override", new[]
            {
                new FlowStep("override_all_zone_rules", "@zone", "@override_type"),
                new FlowStep("apply_override_effects", "@zone", "@override_effects"),
                new FlowStep("spawn_override_indicators", "@zone", "@override_visualization"),
                new FlowStep("sendmessagetoall", "Zone rules overridden in @zone_name: @override_type"),
                new FlowStep("progress_achievement", "@all_players", "zone_rules_overridden", 1)
            });

            FlowService.RegisterFlow("zone_rule_restore", new[]
            {
                new FlowStep("restore_all_zone_rules", "@zone"),
                new FlowStep("remove_override_effects", "@zone"),
                new FlowStep("clear_override_indicators", "@zone"),
                new FlowStep("sendmessagetoall", "Zone rules restored in @zone_name"),
                new FlowStep("progress_achievement", "@all_players", "zone_rules_restored", 1)
            });

            // ========================================
            // GROUP 4: ZONE RESTRICTIONS
            // ========================================

            FlowService.RegisterFlow("zone_restrict_enable", new[]
            {
                new FlowStep("enable_zone_restriction", "@zone", "@restriction_type"),
                new FlowStep("apply_restriction_effects", "@zone", "@restriction_effects"),
                new FlowStep("spawn_restriction_markers", "@zone", "@marker_type"),
                new FlowStep("sendmessagetoall", "Zone restriction enabled: @zone_name"),
                new FlowStep("progress_achievement", "@all_players", "zone_restrictions", 1)
            });

            FlowService.RegisterFlow("zone_restrict_disable", new[]
            {
                new FlowStep("disable_zone_restriction", "@zone"),
                new FlowStep("remove_restriction_effects", "@zone"),
                new FlowStep("clear_restriction_markers", "@zone"),
                new FlowStep("sendmessagetoall", "Zone restriction disabled: @zone_name"),
                new FlowStep("progress_achievement", "@all_players", "zone_restrictions_lifted", 1)
            });

            FlowService.RegisterFlow("zone_restrict_modify", new[]
            {
                new FlowStep("modify_zone_restriction", "@zone", "@new_restriction_params"),
                new FlowStep("update_restriction_effects", "@zone", "@new_effects"),
                new FlowStep("update_restriction_markers", "@zone", "@new_markers"),
                new FlowStep("sendmessagetoall", "Zone restriction modified: @zone_name"),
                new FlowStep("progress_achievement", "@all_players", "zone_modifications", 1)
            });

            // ========================================
            // GROUP 5: SUNBLOCKER REGIONS
            // ========================================

            FlowService.RegisterFlow("sunblocker_region", new[]
            {
                new FlowStep("create_sunblocker_region", "@zone", "@region_params"),
                new FlowStep("apply_sunblocker_effects", "@zone", "@sunblocker_type"),
                new FlowStep("sendmessagetoall", "Sunblocker region created in zone: @zone_name"),
                new FlowStep("progress_achievement", "@all_players", "sunblocker_regions", 1)
            });

            FlowService.RegisterFlow("sunblocker_activate", new[]
            {
                new FlowStep("activate_sunblocker_region", "@zone"),
                new FlowStep("apply_sunblocker_visuals", "@zone", "@visual_effects"),
                new FlowStep("sendmessagetoall", "Sunblocker activated in zone: @zone_name"),
                new FlowStep("progress_achievement", "@all_players", "sunblocker_activated", 1)
            });

            FlowService.RegisterFlow("sunblocker_deactivate", new[]
            {
                new FlowStep("deactivate_sunblocker_region", "@zone"),
                new FlowStep("remove_sunblocker_visuals", "@zone"),
                new FlowStep("sendmessagetoall", "Sunblocker deactivated in zone: @zone_name"),
                new FlowStep("progress_achievement", "@all_players", "sunblocker_deactivated", 1)
            });

            FlowService.RegisterFlow("sunblocker_remove", new[]
            {
                new FlowStep("remove_sunblocker_region", "@zone"),
                new FlowStep("cleanup_sunblocker_data", "@zone"),
                new FlowStep("sendmessagetoall", "Sunblocker region removed from zone: @zone_name"),
                new FlowStep("progress_achievement", "@all_players", "sunblocker_removed", 1)
            });

            // ========================================
            // GROUP 6: ZONE ENVIRONMENT
            // ========================================

            FlowService.RegisterFlow("zone_environment_set", new[]
            {
                new FlowStep("set_zone_environment", "@zone", "@environment_params"),
                new FlowStep("apply_environment_effects", "@zone", "@environment_effects"),
                new FlowStep("sendmessagetoall", "Zone environment set: @zone_name"),
                new FlowStep("progress_achievement", "@all_players", "zone_environments", 1)
            });

            FlowService.RegisterFlow("zone_weather_control", new[]
            {
                new FlowStep("control_zone_weather", "@zone", "@weather_type"),
                new FlowStep("apply_weather_effects", "@zone", "@weather_effects"),
                new FlowStep("sendmessagetoall", "Zone weather controlled: @zone_name - @weather_type"),
                new FlowStep("progress_achievement", "@all_players", "weather_controls", 1)
            });

            FlowService.RegisterFlow("zone_time_control", new[]
            {
                new FlowStep("control_zone_time", "@zone", "@time_of_day"),
                new FlowStep("apply_time_effects", "@zone", "@time_effects"),
                new FlowStep("sendmessagetoall", "Zone time controlled: @zone_name - @time_of_day"),
                new FlowStep("progress_achievement", "@all_players", "time_controls", 1)
            });

            // ========================================
            // GROUP 7: ZONE SECURITY
            // ========================================

            FlowService.RegisterFlow("zone_security_enable", new[]
            {
                new FlowStep("enable_zone_security", "@zone", "@security_level"),
                new FlowStep("apply_security_effects", "@zone", "@security_effects"),
                new FlowStep("sendmessagetoall", "Zone security enabled: @zone_name - Level @security_level"),
                new FlowStep("progress_achievement", "@all_players", "security_enabled", 1)
            });

            FlowService.RegisterFlow("zone_security_disable", new[]
            {
                new FlowStep("disable_zone_security", "@zone"),
                new FlowStep("remove_security_effects", "@zone"),
                new FlowStep("sendmessagetoall", "Zone security disabled: @zone_name"),
                new FlowStep("progress_achievement", "@all_players", "security_disabled", 1)
            });

            FlowService.RegisterFlow("zone_permission_grant", new[]
            {
                new FlowStep("grant_zone_permission", "@player", "@zone", "@permission_level"),
                new FlowStep("apply_permission_effects", "@player", "@zone", "@permission_effects"),
                new FlowStep("sendmessagetouser", "@player", "Zone permission granted: @zone_name - @permission_level"),
                new FlowStep("progress_achievement", "@player", "permissions_granted", 1)
            });

            FlowService.RegisterFlow("zone_permission_revoke", new[]
            {
                new FlowStep("revoke_zone_permission", "@player", "@zone"),
                new FlowStep("remove_permission_effects", "@player", "@zone"),
                new FlowStep("sendmessagetouser", "@player", "Zone permission revoked: @zone_name"),
                new FlowStep("progress_achievement", "@player", "permissions_revoked", 1)
            });

            // ========================================
            // GROUP 8: ZONE STATUS & LIST
            // ========================================

            FlowService.RegisterFlow("zone_status", new[]
            {
                new FlowStep("check_zone_status", "@zone"),
                new FlowStep("check_zone_rules", "@zone"),
                new FlowStep("check_zone_restrictions", "@zone"),
                new FlowStep("sendmessagetouser", "@player", "Zone status: @zone_info"),
                new FlowStep("progress_achievement", "@player", "zone_status_checks", 1)
            });

            FlowService.RegisterFlow("zone_list", new[]
            {
                new FlowStep("get_all_zones", "@zone_type"),
                new FlowStep("format_zone_list", "@zones"),
                new FlowStep("sendmessagetouser", "@player", "Available zones: @zone_list"),
                new FlowStep("progress_achievement", "@player", "zone_lists", 1)
            });

            FlowService.RegisterFlow("zone_player_status", new[]
            {
                new FlowStep("check_player_zone_status", "@player"),
                new FlowStep("check_player_zone_permissions", "@player"),
                new FlowStep("sendmessagetouser", "@player", "Your zone status: @zone_status_info"),
                new FlowStep("progress_achievement", "@player", "player_zone_checks", 1)
            });

            // ========================================
            // GROUP 9: ZONE EVENTS
            // ========================================

            FlowService.RegisterFlow("zone_event_trigger", new[]
            {
                new FlowStep("trigger_zone_event", "@zone", "@event_type", "@event_params"),
                new FlowStep("apply_event_effects", "@zone", "@event_effects"),
                new FlowStep("sendmessagetoall", "Zone event triggered: @zone_name - @event_type"),
                new FlowStep("progress_achievement", "@all_players", "zone_events", 1)
            });

            FlowService.RegisterFlow("zone_event_cancel", new[]
            {
                new FlowStep("cancel_zone_event", "@zone", "@event_type"),
                new FlowStep("remove_event_effects", "@zone", "@event_effects"),
                new FlowStep("sendmessagetoall", "Zone event cancelled: @zone_name - @event_type"),
                new FlowStep("progress_achievement", "@all_players", "events_cancelled", 1)
            });

            // ========================================
            // GROUP 10: ZONE MONITORING
            // ========================================

            FlowService.RegisterFlow("zone_monitor_start", new[]
            {
                new FlowStep("start_zone_monitoring", "@zone", "@monitor_params"),
                new FlowStep("apply_monitoring_effects", "@zone", "@monitoring_effects"),
                new FlowStep("sendmessagetouser", "@player", "Zone monitoring started: @zone_name"),
                new FlowStep("progress_achievement", "@player", "monitoring_started", 1)
            });

            FlowService.RegisterFlow("zone_monitor_stop", new[]
            {
                new FlowStep("stop_zone_monitoring", "@zone"),
                new FlowStep("remove_monitoring_effects", "@zone"),
                new FlowStep("sendmessagetouser", "@player", "Zone monitoring stopped: @zone_name"),
                new FlowStep("progress_achievement", "@player", "monitoring_stopped", 1)
            });

            FlowService.RegisterFlow("zone_monitor_report", new[]
            {
                new FlowStep("generate_zone_report", "@zone", "@report_params"),
                new FlowStep("format_zone_report", "@zone", "@report_data"),
                new FlowStep("sendmessagetouser", "@player", "Zone report: @zone_report"),
                new FlowStep("progress_achievement", "@player", "zone_reports", 1)
            });

            // ========================================
            // GROUP 11: ADMIN ZONE CONTROL
            // ========================================

            FlowService.RegisterFlow("zone_admin_create", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("create_admin_zone", "@zone_params", "@admin_zone_type"),
                new FlowStep("apply_admin_zone_effects", "@new_zone", "@admin_effects"),
                new FlowStep("sendmessagetouser", "@requesting_admin", "Admin zone created: @zone_name"),
                new FlowStep("sendmessagetoall", "Admin created zone: @zone_name")
            });

            FlowService.RegisterFlow("zone_admin_delete", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("delete_admin_zone", "@target_zone"),
                new FlowStep("cleanup_admin_zone_data", "@target_zone"),
                new FlowStep("sendmessagetouser", "@requesting_admin", "Admin zone deleted: @zone_name"),
                new FlowStep("sendmessagetoall", "Admin deleted zone: @zone_name")
            });

            FlowService.RegisterFlow("zone_admin_modify", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("modify_admin_zone", "@target_zone", "@admin_modifications"),
                new FlowStep("apply_admin_modifications", "@target_zone", "@admin_effects"),
                new FlowStep("sendmessagetouser", "@requesting_admin", "Admin zone modified: @zone_name"),
                new FlowStep("sendmessagetoall", "Admin modified zone: @zone_name")
            });

            FlowService.RegisterFlow("zone_admin_lock", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("lock_admin_zone", "@target_zone", "@lock_type"),
                new FlowStep("apply_lock_effects", "@target_zone", "@lock_effects"),
                new FlowStep("sendmessagetouser", "@requesting_admin", "Admin zone locked: @zone_name"),
                new FlowStep("sendmessagetoall", "Admin locked zone: @zone_name")
            });

            FlowService.RegisterFlow("zone_admin_unlock", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("unlock_admin_zone", "@target_zone"),
                new FlowStep("remove_lock_effects", "@target_zone"),
                new FlowStep("sendmessagetouser", "@requesting_admin", "Admin zone unlocked: @zone_name"),
                new FlowStep("sendmessagetoall", "Admin unlocked zone: @zone_name")
            });

            // ========================================
            // GROUP 12: ZONE CONFIG/SETTINGS
            // ========================================

            FlowService.RegisterFlow("zone_settings_load", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("load_zone_settings", "@zone", "@settings_file"),
                new FlowStep("apply_zone_settings", "@zone", "@settings_data"),
                new FlowStep("sendmessagetouser", "@requesting_admin", "Zone settings loaded: @zone_name"),
                new FlowStep("progress_achievement", "@requesting_admin", "zone_settings_loaded", 1)
            });

            FlowService.RegisterFlow("zone_settings_save", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("capture_zone_settings", "@zone"),
                new FlowStep("serialize_zone_settings", "@zone", "@settings_data"),
                new FlowStep("save_zone_settings", "@zone", "@settings_file"),
                new FlowStep("sendmessagetouser", "@requesting_admin", "Zone settings saved: @zone_name"),
                new FlowStep("progress_achievement", "@requesting_admin", "zone_settings_saved", 1)
            });

            FlowService.RegisterFlow("zone_settings_reset", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("reset_zone_settings", "@zone"),
                new FlowStep("apply_default_settings", "@zone"),
                new FlowStep("sendmessagetouser", "@requesting_admin", "Zone settings reset: @zone_name"),
                new FlowStep("sendmessagetoall", "Zone settings reset: @zone_name")
            });

            // ========================================
            // GROUP 13: ZONE SCHEDULER
            // ========================================

            FlowService.RegisterFlow("zone_scheduler_add", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("create_scheduled_task", "@zone", "@task_name", "@schedule_time", "@task_params"),
                new FlowStep("register_scheduled_task", "@zone", "@task_name"),
                new FlowStep("sendmessagetouser", "@requesting_admin", "Scheduled task added: @task_name"),
                new FlowStep("progress_achievement", "@requesting_admin", "scheduler_tasks_added", 1)
            });

            FlowService.RegisterFlow("zone_scheduler_remove", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("unregister_scheduled_task", "@zone", "@task_name"),
                new FlowStep("cleanup_scheduled_task", "@task_name"),
                new FlowStep("sendmessagetouser", "@requesting_admin", "Scheduled task removed: @task_name"),
                new FlowStep("progress_achievement", "@requesting_admin", "scheduler_tasks_removed", 1)
            });

            FlowService.RegisterFlow("zone_scheduler_list", new[]
            {
                new FlowStep("list_scheduled_tasks", "@zone"),
                new FlowStep("format_task_list", "@tasks"),
                new FlowStep("sendmessagetouser", "@player", "Scheduled tasks: @task_list")
            });

            FlowService.RegisterFlow("zone_scheduler_enable", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("enable_scheduled_task", "@zone", "@task_name"),
                new FlowStep("sendmessagetouser", "@requesting_admin", "Scheduled task enabled: @task_name")
            });

            FlowService.RegisterFlow("zone_scheduler_disable", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("disable_scheduled_task", "@zone", "@task_name"),
                new FlowStep("sendmessagetouser", "@requesting_admin", "Scheduled task disabled: @task_name")
            });

            // ========================================
            // GROUP 14: ZONE REPEATING TASKS
            // ========================================

            FlowService.RegisterFlow("zone_repeating_enable", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("create_repeating_task", "@zone", "@task_name", "@interval", "@repeat_params"),
                new FlowStep("start_repeating_task", "@zone", "@task_name"),
                new FlowStep("sendmessagetouser", "@requesting_admin", "Repeating task enabled: @task_name"),
                new FlowStep("progress_achievement", "@requesting_admin", "repeating_tasks_enabled", 1)
            });

            FlowService.RegisterFlow("zone_repeating_disable", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("stop_repeating_task", "@zone", "@task_name"),
                new FlowStep("sendmessagetouser", "@requesting_admin", "Repeating task disabled: @task_name"),
                new FlowStep("progress_achievement", "@requesting_admin", "repeating_tasks_disabled", 1)
            });

            FlowService.RegisterFlow("zone_repeating_status", new[]
            {
                new FlowStep("get_repeating_task_status", "@zone", "@task_name"),
                new FlowStep("format_repeating_status", "@task_status"),
                new FlowStep("sendmessagetouser", "@player", "Repeating task status: @task_status")
            });

            FlowService.RegisterFlow("zone_repeating_pause", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("pause_repeating_task", "@zone", "@task_name"),
                new FlowStep("sendmessagetouser", "@requesting_admin", "Repeating task paused: @task_name")
            });

            FlowService.RegisterFlow("zone_repeating_resume", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("resume_repeating_task", "@zone", "@task_name"),
                new FlowStep("sendmessagetouser", "@requesting_admin", "Repeating task resumed: @task_name")
            });
        }

        /// <summary>
        /// Get all zone flow names for registration.
        /// </summary>
        public static string[] GetZoneFlowNames()
        {
            return new[]
            {
                // Group 1: Zone Lifecycle
                "zone_ready", "zone_ready_all",
                "zone_preset_save", "zone_preset_load", "zone_preset_delete", "zone_preset_list",
                "zone_create", "zone_delete", "zone_rename",
                // Group 2: Zone Entry/Exit/Transition
                "zone_enter", "zone_exit", "zone_transition",
                // Group 3: Zone Rules
                "zone_rule_add", "zone_rule_remove", "zone_rule_update", "zone_rule_check",
                "zone_rule_pvp_enable", "zone_rule_pvp_disable",
                "zone_rule_building_enable", "zone_rule_building_disable",
                "zone_rule_combat_enable", "zone_rule_combat_disable",
                "zone_rule_override", "zone_rule_restore",
                // Group 4: Zone Restrictions
                "zone_restrict_enable", "zone_restrict_disable", "zone_restrict_modify",
                // Group 5: Sunblocker Regions
                "sunblocker_region", "sunblocker_activate", "sunblocker_deactivate", "sunblocker_remove",
                // Group 6: Zone Environment
                "zone_environment_set", "zone_weather_control", "zone_time_control",
                // Group 7: Zone Security
                "zone_security_enable", "zone_security_disable", "zone_permission_grant", "zone_permission_revoke",
                // Group 8: Zone Status & List
                "zone_status", "zone_list", "zone_player_status",
                // Group 9: Zone Events
                "zone_event_trigger", "zone_event_cancel",
                // Group 10: Zone Monitoring
                "zone_monitor_start", "zone_monitor_stop", "zone_monitor_report",
                // Group 11: Admin Zone Control
                "zone_admin_create", "zone_admin_delete", "zone_admin_modify", "zone_admin_lock", "zone_admin_unlock",
                // Group 12: Zone Config/Settings
                "zone_settings_load", "zone_settings_save", "zone_settings_reset",
                // Group 13: Zone Scheduler
                "zone_scheduler_add", "zone_scheduler_remove", "zone_scheduler_list", "zone_scheduler_enable", "zone_scheduler_disable",
                // Group 14: Zone Repeating Tasks
                "zone_repeating_enable", "zone_repeating_disable", "zone_repeating_status", "zone_repeating_pause", "zone_repeating_resume"
            };
        }
    }
}
