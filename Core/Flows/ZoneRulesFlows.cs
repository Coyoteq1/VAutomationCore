using System.Collections.Generic;
using VAutomationCore.Core.Api;
using VAutomationCore.Core.Services;

namespace VAutomationCore.Core.Flows
{
    /// <summary>
    /// Zone rules flows for managing zone rule systems, rule enforcement, and rule management.
    /// Handles rule creation, modification, enforcement, disabling, and administrative rule control.
    /// </summary>
    public static class ZoneRulesFlows
    {
        /// <summary>
        /// Register all zone rules flows with the FlowService.
        /// </summary>
        public static void RegisterZoneRulesFlows()
        {
            // Rule management flows
            FlowService.RegisterFlow("zone_rule_create", new[]
            {
                new FlowStep("create_zone_rule", "@zone", "@rule_type", "@rule_parameters"),
                new FlowStep("apply_rule_enforcement", "@zone", "@rule_type", "@enforcement_level"),
                new FlowStep("spawn_rule_indicators", "@zone", "@rule_visualization"),
                new FlowStep("sendmessagetoall", "Zone rule created: @rule_type in @zone_name"),
                new FlowStep("progress_achievement", "@all_players", "zone_rules_created", 1)
            });

            FlowService.RegisterFlow("zone_rule_modify", new[]
            {
                new FlowStep("modify_zone_rule", "@zone", "@rule_type", "@new_rule_parameters"),
                new FlowStep("update_rule_enforcement", "@zone", "@rule_type", "@new_enforcement_level"),
                new FlowStep("update_rule_indicators", "@zone", "@new_rule_visualization"),
                new FlowStep("sendmessagetoall", "Zone rule modified: @rule_type in @zone_name"),
                new FlowStep("progress_achievement", "@all_players", "zone_rules_modified", 1)
            });

            FlowService.RegisterFlow("zone_rule_delete", new[]
            {
                new FlowStep("delete_zone_rule", "@zone", "@rule_type"),
                new FlowStep("remove_rule_enforcement", "@zone", "@rule_type"),
                new FlowStep("clear_rule_indicators", "@zone", "@rule_type"),
                new FlowStep("sendmessagetoall", "Zone rule deleted: @rule_type in @zone_name"),
                new FlowStep("progress_achievement", "@all_players", "zone_rules_deleted", 1)
            });

            // Rule enforcement flows
            FlowService.RegisterFlow("zone_rule_enforce", new[]
            {
                new FlowStep("enforce_zone_rules", "@zone", "@rule_types"),
                new FlowStep("apply_enforcement_effects", "@zone", "@enforcement_effects"),
                new FlowStep("monitor_rule_compliance", "@zone", "@rule_types"),
                new FlowStep("sendmessagetoall", "Zone rules enforced in @zone_name: @enforced_rules"),
                new FlowStep("progress_achievement", "@all_players", "zone_rules_enforced", 1)
            });

            FlowService.RegisterFlow("zone_rule_relax", new[]
            {
                new FlowStep("relax_zone_rules", "@zone", "@rule_types"),
                new FlowStep("reduce_enforcement_effects", "@zone", "@relaxed_effects"),
                new FlowStep("update_rule_compliance", "@zone", "@rule_types"),
                new FlowStep("sendmessagetoall", "Zone rules relaxed in @zone_name: @relaxed_rules"),
                new FlowStep("progress_achievement", "@all_players", "zone_rules_relaxed", 1)
            });

            // Rule status flows
            FlowService.RegisterFlow("zone_rule_status", new[]
            {
                new FlowStep("check_zone_rules", "@zone"),
                new FlowStep("check_rule_enforcement", "@zone"),
                new FlowStep("check_rule_compliance", "@zone"),
                new FlowStep("sendmessagetouser", "@player", "Zone rules status: @rule_status_info"),
                new FlowStep("progress_achievement", "@player", "zone_rule_checks", 1)
            });

            FlowService.RegisterFlow("zone_rule_list", new[]
            {
                new FlowStep("list_zone_rules", "@zone"),
                new FlowStep("format_rule_list", "@zone_rules"),
                new FlowStep("sendmessagetouser", "@player", "Zone rules in @zone_name: @rule_list"),
                new FlowStep("progress_achievement", "@player", "zone_rule_lists", 1)
            });

            // Rule category flows
            FlowService.RegisterFlow("zone_rule_pvp_disable", new[]
            {
                new FlowStep("disable_pvp_rules", "@zone"),
                new FlowStep("apply_pvp_safety", "@zone", "@pvp_safety_level"),
                new FlowStep("spawn_pvp_indicators", "@zone", "@pvp_disabled_visuals"),
                new FlowStep("sendmessagetoall", "PVP rules disabled in @zone_name"),
                new FlowStep("progress_achievement", "@all_players", "pvp_rules_disabled", 1)
            });

            FlowService.RegisterFlow("zone_rule_pvp_enable", new[]
            {
                new FlowStep("enable_pvp_rules", "@zone"),
                new FlowStep("remove_pvp_safety", "@zone"),
                new FlowStep("clear_pvp_indicators", "@zone"),
                new FlowStep("sendmessagetoall", "PVP rules enabled in @zone_name"),
                new FlowStep("progress_achievement", "@all_players", "pvp_rules_enabled", 1)
            });

            FlowService.RegisterFlow("zone_rule_building_disable", new[]
            {
                new FlowStep("disable_building_rules", "@zone"),
                new FlowStep("apply_building_freedom", "@zone", "@building_freedom_level"),
                new FlowStep("spawn_building_indicators", "@zone", "@building_freedom_visuals"),
                new FlowStep("sendmessagetoall", "Building rules disabled in @zone_name"),
                new FlowStep("progress_achievement", "@all_players", "building_rules_disabled", 1)
            });

            FlowService.RegisterFlow("zone_rule_building_enable", new[]
            {
                new FlowStep("enable_building_rules", "@zone"),
                new FlowStep("remove_building_freedom", "@zone"),
                new FlowStep("clear_building_indicators", "@zone"),
                new FlowStep("sendmessagetoall", "Building rules enabled in @zone_name"),
                new FlowStep("progress_achievement", "@all_players", "building_rules_enabled", 1)
            });

            FlowService.RegisterFlow("zone_rule_combat_disable", new[]
            {
                new FlowStep("disable_combat_rules", "@zone"),
                new FlowStep("apply_combat_peace", "@zone", "@combat_peace_level"),
                new FlowStep("spawn_combat_indicators", "@zone", "@combat_peace_visuals"),
                new FlowStep("sendmessagetoall", "Combat rules disabled in @zone_name"),
                new FlowStep("progress_achievement", "@all_players", "combat_rules_disabled", 1)
            });

            FlowService.RegisterFlow("zone_rule_combat_enable", new[]
            {
                new FlowStep("enable_combat_rules", "@zone"),
                new FlowStep("remove_combat_peace", "@zone"),
                new FlowStep("clear_combat_indicators", "@zone"),
                new FlowStep("sendmessagetoall", "Combat rules enabled in @zone_name"),
                new FlowStep("progress_achievement", "@all_players", "combat_rules_enabled", 1)
            });

            // Rule override flows
            FlowService.RegisterFlow("zone_rule_override_all", new[]
            {
                new FlowStep("override_all_zone_rules", "@zone", "@override_type"),
                new FlowStep("apply_override_effects", "@zone", "@override_effects"),
                new FlowStep("spawn_override_indicators", "@zone", "@override_visualization"),
                new FlowStep("sendmessagetoall", "All zone rules overridden in @zone_name: @override_type"),
                new FlowStep("progress_achievement", "@all_players", "zone_rules_overridden", 1)
            });

            FlowService.RegisterFlow("zone_rule_restore_all", new[]
            {
                new FlowStep("restore_all_zone_rules", "@zone"),
                new FlowStep("remove_override_effects", "@zone"),
                new FlowStep("clear_override_indicators", "@zone"),
                new FlowStep("sendmessagetoall", "All zone rules restored in @zone_name"),
                new FlowStep("progress_achievement", "@all_players", "zone_rules_restored", 1)
            });

            // Rule permission flows
            FlowService.RegisterFlow("zone_rule_permission_grant", new[]
            {
                new FlowStep("grant_rule_permission", "@player", "@zone", "@rule_permission_level"),
                new FlowStep("apply_permission_effects", "@player", "@zone", "@permission_effects"),
                new FlowStep("sendmessagetouser", "@player", "Rule permission granted: @permission_level in @zone_name"),
                new FlowStep("progress_achievement", "@player", "rule_permissions_granted", 1)
            });

            FlowService.RegisterFlow("zone_rule_permission_revoke", new[]
            {
                new FlowStep("revoke_rule_permission", "@player", "@zone"),
                new FlowStep("remove_permission_effects", "@player", "@zone"),
                new FlowStep("sendmessagetouser", "@player", "Rule permission revoked in @zone_name"),
                new FlowStep("progress_achievement", "@player", "rule_permissions_revoked", 1)
            });

            // Rule exception flows
            FlowService.RegisterFlow("zone_rule_exception_add", new[]
            {
                new FlowStep("add_rule_exception", "@zone", "@rule_type", "@exception_target", "@exception_params"),
                new FlowStep("apply_exception_effects", "@zone", "@exception_effects"),
                new FlowStep("spawn_exception_indicators", "@zone", "@exception_visualization"),
                new FlowStep("sendmessagetoall", "Rule exception added in @zone_name: @rule_type for @exception_target"),
                new FlowStep("progress_achievement", "@all_players", "rule_exceptions_added", 1)
            });

            FlowService.RegisterFlow("zone_rule_exception_remove", new[]
            {
                new FlowStep("remove_rule_exception", "@zone", "@rule_type", "@exception_target"),
                new FlowStep("remove_exception_effects", "@zone", "@exception_effects"),
                new FlowStep("clear_exception_indicators", "@zone", "@exception_target"),
                new FlowStep("sendmessagetoall", "Rule exception removed in @zone_name: @rule_type for @exception_target"),
                new FlowStep("progress_achievement", "@all_players", "rule_exceptions_removed", 1)
            });

            // Admin rule control flows
            FlowService.RegisterFlow("zone_rule_admin_disable_all", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("disable_all_zone_rules", "@all_zones"),
                new FlowStep("apply_admin_rule_effects", "@all_zones", "@admin_disable_effects"),
                new FlowStep("spawn_admin_indicators", "@all_zones", "@admin_visualization"),
                new FlowStep("sendmessagetouser", "@requesting_admin", "Admin disabled all zone rules"),
                new FlowStep("sendmessagetoall", "Admin: All zone rules disabled globally")
            });

            FlowService.RegisterFlow("zone_rule_admin_enable_all", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("enable_all_zone_rules", "@all_zones"),
                new FlowStep("remove_admin_rule_effects", "@all_zones"),
                new FlowStep("clear_admin_indicators", "@all_zones"),
                new FlowStep("sendmessagetouser", "@requesting_admin", "Admin enabled all zone rules"),
                new FlowStep("sendmessagetoall", "Admin: All zone rules enabled globally")
            });

            FlowService.RegisterFlow("zone_rule_admin_category", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("admin_category_rules", "@all_zones", "@rule_category", "@admin_action"),
                new FlowStep("apply_category_effects", "@all_zones", "@category_effects"),
                new FlowStep("spawn_category_indicators", "@all_zones", "@category_visualization"),
                new FlowStep("sendmessagetouser", "@requesting_admin", "Admin modified @rule_category rules in all zones"),
                new FlowStep("sendmessagetoall", "Admin: @rule_category rules @admin_action globally")
            });

            FlowService.RegisterFlow("zone_rule_admin_custom", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("create_custom_rule", "@target_zones", "@rule_definition", "@custom_parameters"),
                new FlowStep("apply_custom_enforcement", "@target_zones", "@custom_enforcement"),
                new FlowStep("spawn_custom_indicators", "@target_zones", "@custom_visualization"),
                new FlowStep("sendmessagetouser", "@requesting_admin", "Admin created custom rule in @target_zones_count zones"),
                new FlowStep("sendmessagetoall", "Admin: Custom rule created globally")
            });

            // Rule monitoring flows
            FlowService.RegisterFlow("zone_rule_monitor_start", new[]
            {
                new FlowStep("start_rule_monitoring", "@zone", "@monitoring_params"),
                new FlowStep("apply_monitoring_effects", "@zone", "@monitoring_effects"),
                new FlowStep("spawn_monitoring_indicators", "@zone", "@monitoring_visualization"),
                new FlowStep("sendmessagetouser", "@player", "Rule monitoring started in @zone_name"),
                new FlowStep("progress_achievement", "@player", "rule_monitoring_started", 1)
            });

            FlowService.RegisterFlow("zone_rule_monitor_stop", new[]
            {
                new FlowStep("stop_rule_monitoring", "@zone"),
                new FlowStep("remove_monitoring_effects", "@zone"),
                new FlowStep("clear_monitoring_indicators", "@zone"),
                new FlowStep("sendmessagetouser", "@player", "Rule monitoring stopped in @zone_name"),
                new FlowStep("progress_achievement", "@player", "rule_monitoring_stopped", 1)
            });

            FlowService.RegisterFlow("zone_rule_monitor_report", new[]
            {
                new FlowStep("generate_rule_report", "@zone", "@report_params"),
                new FlowStep("analyze_rule_compliance", "@zone", "@compliance_data"),
                new FlowStep("format_rule_report", "@zone", "@report_data"),
                new FlowStep("sendmessagetouser", "@player", "Rule monitoring report: @zone_report"),
                new FlowStep("progress_achievement", "@player", "rule_reports", 1)
            });

            // Rule history flows
            FlowService.RegisterFlow("zone_rule_history_log", new[]
            {
                new FlowStep("log_rule_change", "@zone", "@rule_type", "@change_type", "@change_details"),
                new FlowStep("store_rule_history", "@zone", "@rule_history_data"),
                new FlowStep("sendmessagetouser", "@player", "Rule change logged: @change_type for @rule_type in @zone_name"),
                new FlowStep("progress_achievement", "@player", "rule_history_logged", 1)
            });

            FlowService.RegisterFlow("zone_rule_history_view", new[]
            {
                new FlowStep("view_rule_history", "@zone", "@rule_type", "@history_params"),
                new FlowStep("format_history_report", "@zone", "@history_data"),
                new FlowStep("sendmessagetouser", "@player", "Rule history for @rule_type in @zone_name: @history_report"),
                new FlowStep("progress_achievement", "@player", "rule_history_viewed", 1)
            });

            FlowService.RegisterFlow("zone_rule_history_clear", new[]
            {
                new FlowStep("clear_rule_history", "@zone", "@rule_type"),
                new FlowStep("remove_history_data", "@zone", "@rule_type"),
                new FlowStep("sendmessagetouser", "@player", "Rule history cleared for @rule_type in @zone_name"),
                new FlowStep("progress_achievement", "@player", "rule_history_cleared", 1)
            });

            // Rule template flows
            FlowService.RegisterFlow("zone_rule_template_save", new[]
            {
                new FlowStep("save_rule_template", "@template_name", "@rule_definition", "@template_parameters"),
                new FlowStep("store_template_data", "@template_name", "@template_data"),
                new FlowStep("sendmessagetouser", "@player", "Rule template saved: @template_name"),
                new FlowStep("progress_achievement", "@player", "rule_templates_saved", 1)
            });

            FlowService.RegisterFlow("zone_rule_template_load", new[]
            {
                new FlowStep("load_rule_template", "@template_name", "@target_zone"),
                new FlowStep("apply_template_rules", "@target_zone", "@template_rules"),
                new FlowStep("spawn_template_indicators", "@target_zone", "@template_visualization"),
                new FlowStep("sendmessagetouser", "@player", "Rule template loaded: @template_name in @zone_name"),
                new FlowStep("progress_achievement", "@player", "rule_templates_loaded", 1)
            });

            FlowService.RegisterFlow("zone_rule_template_delete", new[]
            {
                new FlowStep("delete_rule_template", "@template_name"),
                new FlowStep("remove_template_data", "@template_name"),
                new FlowStep("sendmessagetouser", "@player", "Rule template deleted: @template_name"),
                new FlowStep("progress_achievement", "@player", "rule_templates_deleted", 1)
            });

            // Rule validation flows
            FlowService.RegisterFlow("zone_rule_validate", new[]
            {
                new FlowStep("validate_zone_rule", "@zone", "@rule_type", "@validation_params"),
                new FlowStep("check_rule_conflicts", "@zone", "@rule_type"),
                new FlowStep("validate_rule_parameters", "@zone", "@rule_type"),
                new FlowStep("sendmessagetouser", "@player", "Rule validation result: @validation_result"),
                new FlowStep("progress_achievement", "@player", "rule_validations", 1)
            });

            FlowService.RegisterFlow("zone_rule_test", new[]
            {
                new FlowStep("test_zone_rule", "@zone", "@rule_type", "@test_scenario"),
                new FlowStep("simulate_rule_behavior", "@zone", "@rule_type", "@test_conditions"),
                new FlowStep("analyze_test_results", "@zone", "@rule_type", "@test_results"),
                new FlowStep("sendmessagetouser", "@player", "Rule test result: @test_result"),
                new FlowStep("progress_achievement", "@player", "rule_tests", 1)
            });
        }

        /// <summary>
        /// Get all zone rules flow names for registration.
        /// </summary>
        public static string[] GetZoneRulesFlowNames()
        {
            return new[]
            {
                "zone_rule_create", "zone_rule_modify", "zone_rule_delete",
                "zone_rule_enforce", "zone_rule_relax",
                "zone_rule_status", "zone_rule_list",
                "zone_rule_pvp_disable", "zone_rule_pvp_enable",
                "zone_rule_building_disable", "zone_rule_building_enable",
                "zone_rule_combat_disable", "zone_rule_combat_enable",
                "zone_rule_override_all", "zone_rule_restore_all",
                "zone_rule_permission_grant", "zone_rule_permission_revoke",
                "zone_rule_exception_add", "zone_rule_exception_remove",
                "zone_rule_admin_disable_all", "zone_rule_admin_enable_all", "zone_rule_admin_category", "zone_rule_admin_custom",
                "zone_rule_monitor_start", "zone_rule_monitor_stop", "zone_rule_monitor_report",
                "zone_rule_history_log", "zone_rule_history_view", "zone_rule_history_clear",
                "zone_rule_template_save", "zone_rule_template_load", "zone_rule_template_delete",
                "zone_rule_validate", "zone_rule_test"
            };
        }
    }
}
