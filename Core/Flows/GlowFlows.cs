using System.Collections.Generic;
using VAutomationCore.Core.Api;
using VAutomationCore.Core.Services;

namespace VAutomationCore.Core.Flows
{
    /// <summary>
    /// Glow flows for managing visual glow effects, highlights, outlines, and FX visibility logic.
    /// Handles glow effects, highlight outlines, player markers, placement preview glow, and interaction glow.
    /// </summary>
    public static class GlowFlows
    {
        /// <summary>
        /// Register all glow flows with the FlowService.
        /// </summary>
        public static void RegisterGlowFlows()
        {
            // Glow effect flows
            FlowService.RegisterFlow("glow_start", new[]
            {
                new FlowStep("apply_glow_effect", "@target_entity", "@glow_type", "@glow_color", "@glow_intensity"),
                new FlowStep("spawn_glow_fx", "@target_entity", "@glow_fx"),
                new FlowStep("sendmessagetouser", "@player", "Glow effect started: @glow_type"),
                new FlowStep("progress_achievement", "@player", "glow_effects_started", 1)
            });

            FlowService.RegisterFlow("glow_stop", new[]
            {
                new FlowStep("remove_glow_effect", "@target_entity"),
                new FlowStep("stop_glow_fx", "@target_entity"),
                new FlowStep("sendmessagetouser", "@player", "Glow effect stopped"),
                new FlowStep("progress_achievement", "@player", "glow_effects_stopped", 1)
            });

            FlowService.RegisterFlow("glow_pulse", new[]
            {
                new FlowStep("apply_pulsing_glow", "@target_entity", "@pulse_color", "@pulse_speed"),
                new FlowStep("spawn_pulse_fx", "@target_entity", "@pulse_fx"),
                new FlowStep("sendmessagetouser", "@player", "Pulsing glow applied"),
                new FlowStep("progress_achievement", "@player", "pulsing_glows", 1)
            });

            FlowService.RegisterFlow("glow_fade", new[]
            {
                new FlowStep("apply_fading_glow", "@target_entity", "@fade_color", "@fade_duration"),
                new FlowStep("spawn_fade_fx", "@target_entity", "@fade_fx"),
                new FlowStep("sendmessagetouser", "@player", "Fading glow applied"),
                new FlowStep("progress_achievement", "@player", "fading_glows", 1)
            });

            FlowService.RegisterFlow("glow_rainbow", new[]
            {
                new FlowStep("apply_rainbow_glow", "@target_entity", "@rainbow_speed"),
                new FlowStep("spawn_rainbow_fx", "@target_entity", "@rainbow_fx"),
                new FlowStep("sendmessagetouser", "@player", "Rainbow glow applied"),
                new FlowStep("progress_achievement", "@player", "rainbow_glows", 1)
            });

            // Highlight outline flows
            FlowService.RegisterFlow("highlight_entity", new[]
            {
                new FlowStep("apply_highlight_outline", "@target_entity", "@outline_color", "@outline_thickness"),
                new FlowStep("spawn_highlight_fx", "@target_entity", "@highlight_fx"),
                new FlowStep("sendmessagetouser", "@player", "Entity highlighted: @entity_name"),
                new FlowStep("progress_achievement", "@player", "entities_highlighted", 1)
            });

            FlowService.RegisterFlow("highlight_remove", new[]
            {
                new FlowStep("remove_highlight_outline", "@target_entity"),
                new FlowStep("stop_highlight_fx", "@target_entity"),
                new FlowStep("sendmessagetouser", "@player", "Highlight removed from @entity_name"),
                new FlowStep("progress_achievement", "@player", "highlights_removed", 1)
            });

            FlowService.RegisterFlow("highlight_group", new[]
            {
                new FlowStep("apply_group_highlight", "@target_entities", "@group_color", "@group_thickness"),
                new FlowStep("spawn_group_fx", "@target_entities", "@group_fx"),
                new FlowStep("sendmessagetouser", "@player", "Group highlighted: @group_count entities"),
                new FlowStep("progress_achievement", "@player", "group_highlights", 1)
            });

            FlowService.RegisterFlow("highlight_clear_all", new[]
            {
                new FlowStep("clear_all_highlights", "@all_entities"),
                new FlowStep("stop_all_highlight_fx", "@all_entities"),
                new FlowStep("sendmessagetouser", "@player", "All highlights cleared"),
                new FlowStep("progress_achievement", "@player", "all_highlights_cleared", 1)
            });

            // Player marker flows
            FlowService.RegisterFlow("player_marker_set", new[]
            {
                new FlowStep("apply_player_marker", "@player", "@marker_type", "@marker_color"),
                new FlowStep("spawn_marker_icon", "@player", "@marker_icon"),
                new FlowStep("set_marker_visibility", "@player", "@visibility_range"),
                new FlowStep("sendmessagetouser", "@player", "Player marker set: @marker_type"),
                new FlowStep("progress_achievement", "@player", "player_markers_set", 1)
            });

            FlowService.RegisterFlow("player_marker_remove", new[]
            {
                new FlowStep("remove_player_marker", "@player"),
                new FlowStep("hide_marker_icon", "@player"),
                new FlowStep("sendmessagetouser", "@player", "Player marker removed"),
                new FlowStep("progress_achievement", "@player", "player_markers_removed", 1)
            });

            FlowService.RegisterFlow("player_marker_update", new[]
            {
                new FlowStep("update_player_marker", "@player", "@new_marker_type", "@new_marker_color"),
                new FlowStep("update_marker_icon", "@player", "@new_marker_icon"),
                new FlowStep("sendmessagetouser", "@player", "Player marker updated: @new_marker_type"),
                new FlowStep("progress_achievement", "@player", "player_markers_updated", 1)
            });

            FlowService.RegisterFlow("player_marker_visibility", new[]
            {
                new FlowStep("set_marker_visibility", "@player", "@visibility_range"),
                new FlowStep("update_marker_display", "@player", "@display_mode"),
                new FlowStep("sendmessagetouser", "@player", "Marker visibility set to @visibility_range"),
                new FlowStep("progress_achievement", "@player", "marker_visibility_set", 1)
            });

            // Placement preview glow flows
            FlowService.RegisterFlow("preview_glow_enable", new[]
            {
                new FlowStep("enable_placement_preview", "@player"),
                new FlowStep("apply_preview_glow", "@player", "@preview_color", "@preview_intensity"),
                new FlowStep("spawn_preview_fx", "@player", "@preview_fx"),
                new FlowStep("sendmessagetouser", "@player", "Placement preview glow enabled"),
                new FlowStep("progress_achievement", "@player", "preview_glows_enabled", 1)
            });

            FlowService.RegisterFlow("preview_glow_disable", new[]
            {
                new FlowStep("disable_placement_preview", "@player"),
                new FlowStep("remove_preview_glow", "@player"),
                new FlowStep("stop_preview_fx", "@player"),
                new FlowStep("sendmessagetouser", "@player", "Placement preview glow disabled"),
                new FlowStep("progress_achievement", "@player", "preview_glows_disabled", 1)
            });

            FlowService.RegisterFlow("preview_glow_update", new[]
            {
                new FlowStep("update_preview_glow", "@player", "@new_preview_color", "@new_preview_intensity"),
                new FlowStep("update_preview_fx", "@player", "@new_preview_fx"),
                new FlowStep("sendmessagetouser", "@player", "Placement preview glow updated"),
                new FlowStep("progress_achievement", "@player", "preview_glows_updated", 1)
            });

            // Interaction glow flows
            FlowService.RegisterFlow("interaction_glow_start", new[]
            {
                new FlowStep("detect_interactable_entity", "@player", "@interaction_range"),
                new FlowStep("apply_interaction_glow", "@interactable_entity", "@interaction_color"),
                new FlowStep("spawn_interaction_fx", "@interactable_entity", "@interaction_fx"),
                new FlowStep("sendmessagetouser", "@player", "Interaction glow: @interactable_name"),
                new FlowStep("progress_achievement", "@player", "interaction_glows", 1)
            });

            FlowService.RegisterFlow("interaction_glow_stop", new[]
            {
                new FlowStep("remove_interaction_glow", "@interactable_entity"),
                new FlowStep("stop_interaction_fx", "@interactable_entity"),
                new FlowStep("sendmessagetouser", "@player", "Interaction glow stopped: @interactable_name"),
                new FlowStep("progress_achievement", "@player", "interaction_glows_stopped", 1)
            });

            FlowService.RegisterFlow("interaction_glow_range", new[]
            {
                new FlowStep("set_interaction_range", "@player", "@glow_range"),
                new FlowStep("update_range_indicators", "@player", "@range_visualization"),
                new FlowStep("sendmessagetouser", "@player", "Interaction glow range set to @glow_range"),
                new FlowStep("progress_achievement", "@player", "interaction_ranges_set", 1)
            });

            // Admin glow flows
            FlowService.RegisterFlow("glow_admin_set_all", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("apply_glow_to_all", "@all_players", "@admin_glow_type", "@admin_glow_color"),
                new FlowStep("spawn_admin_glow_fx", "@all_players", "@admin_glow_fx"),
                new FlowStep("sendmessagetouser", "@requesting_admin", "Admin glow applied to all players"),
                new FlowStep("sendmessagetoall", "Admin glow effect applied: @admin_glow_type")
            });

            FlowService.RegisterFlow("glow_admin_clear_all", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("remove_all_glow_effects", "@all_players"),
                new FlowStep("stop_all_glow_fx", "@all_players"),
                new FlowStep("sendmessagetouser", "@requesting_admin", "Admin cleared all glow effects"),
                new FlowStep("sendmessagetoall", "Admin cleared all glow effects")
            });

            FlowService.RegisterFlow("glow_admin_entity", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("apply_admin_glow", "@target_entity", "@admin_glow_type", "@admin_glow_color"),
                new FlowStep("spawn_admin_glow_fx", "@target_entity", "@admin_glow_fx"),
                new FlowStep("sendmessagetouser", "@requesting_admin", "Admin glow applied to @entity_name"),
                new FlowStep("sendmessagetoall", "Admin glow: @admin_glow_type on @entity_name")
            });

            // Glow status flows
            FlowService.RegisterFlow("glow_status", new[]
            {
                new FlowStep("check_entity_glow", "@target_entity"),
                new FlowStep("check_glow_parameters", "@target_entity"),
                new FlowStep("sendmessagetouser", "@player", "Glow status: @glow_info"),
                new FlowStep("progress_achievement", "@player", "glow_status_checks", 1)
            });

            FlowService.RegisterFlow("glow_list_active", new[]
            {
                new FlowStep("get_active_glows", "@area"),
                new FlowStep("format_glow_list", "@active_glows"),
                new FlowStep("sendmessagetouser", "@player", "Active glows in area: @glow_count"),
                new FlowStep("progress_achievement", "@player", "glow_lists", 1)
            });

            // Glow customization flows
            FlowService.RegisterFlow("glow_customize", new[]
            {
                new FlowStep("customize_glow_effect", "@target_entity", "@custom_glow_params"),
                new FlowStep("apply_custom_glow", "@target_entity", "@custom_glow"),
                new FlowStep("spawn_custom_fx", "@target_entity", "@custom_fx"),
                new FlowStep("sendmessagetouser", "@player", "Custom glow applied: @custom_glow_name"),
                new FlowStep("progress_achievement", "@player", "custom_glows", 1)
            });

            FlowService.RegisterFlow("glow_preset_save", new[]
            {
                new FlowStep("save_glow_preset", "@target_entity", "@preset_name", "@glow_parameters"),
                new FlowStep("store_preset_data", "@preset_name", "@preset_data"),
                new FlowStep("sendmessagetouser", "@player", "Glow preset saved: @preset_name"),
                new FlowStep("progress_achievement", "@player", "glow_presets_saved", 1)
            });

            FlowService.RegisterFlow("glow_preset_load", new[]
            {
                new FlowStep("load_glow_preset", "@target_entity", "@preset_name"),
                new FlowStep("apply_preset_glow", "@target_entity", "@preset_glow"),
                new FlowStep("spawn_preset_fx", "@target_entity", "@preset_fx"),
                new FlowStep("sendmessagetouser", "@player", "Glow preset loaded: @preset_name"),
                new FlowStep("progress_achievement", "@player", "glow_presets_loaded", 1)
            });

            // Glow condition flows
            FlowService.RegisterFlow("glow_conditional", new[]
            {
                new FlowStep("check_glow_condition", "@target_entity", "@condition_type"),
                new FlowStep("apply_conditional_glow", "@target_entity", "@conditional_glow"),
                new FlowStep("spawn_condition_fx", "@target_entity", "@condition_fx"),
                new FlowStep("sendmessagetouser", "@player", "Conditional glow applied: @condition_result"),
                new FlowStep("progress_achievement", "@player", "conditional_glows", 1)
            });

            FlowService.RegisterFlow("glow_health_based", new[]
            {
                new FlowStep("check_entity_health", "@target_entity"),
                new FlowStep("apply_health_glow", "@target_entity", "@health_color", "@health_threshold"),
                new FlowStep("update_health_glow", "@target_entity", "@current_health"),
                new FlowStep("sendmessagetouser", "@player", "Health-based glow: @health_status"),
                new FlowStep("progress_achievement", "@player", "health_glows", 1)
            });

            FlowService.RegisterFlow("glow_distance_based", new[]
            {
                new FlowStep("check_entity_distance", "@target_entity", "@reference_point"),
                new FlowStep("apply_distance_glow", "@target_entity", "@distance_color", "@distance_threshold"),
                new FlowStep("update_distance_glow", "@target_entity", "@current_distance"),
                new FlowStep("sendmessagetouser", "@player", "Distance-based glow: @distance_status"),
                new FlowStep("progress_achievement", "@player", "distance_glows", 1)
            });

            // Glow animation flows
            FlowService.RegisterFlow("glow_animate", new[]
            {
                new FlowStep("start_glow_animation", "@target_entity", "@animation_type", "@animation_duration"),
                new FlowStep("spawn_animation_fx", "@target_entity", "@animation_fx"),
                new FlowStep("sendmessagetouser", "@player", "Glow animation started: @animation_type"),
                new FlowStep("progress_achievement", "@player", "glow_animations", 1)
            });

            FlowService.RegisterFlow("glow_animate_stop", new[]
            {
                new FlowStep("stop_glow_animation", "@target_entity"),
                new FlowStep("stop_animation_fx", "@target_entity"),
                new FlowStep("sendmessagetouser", "@player", "Glow animation stopped"),
                new FlowStep("progress_achievement", "@player", "glow_animations_stopped", 1)
            });

            // Glow synchronization flows
            FlowService.RegisterFlow("glow_sync_group", new[]
            {
                new FlowStep("sync_group_glows", "@target_entities", "@sync_pattern"),
                new FlowStep("apply_sync_glow", "@target_entities", "@sync_glow"),
                new FlowStep("spawn_sync_fx", "@target_entities", "@sync_fx"),
                new FlowStep("sendmessagetouser", "@player", "Group glow synchronized: @sync_count entities"),
                new FlowStep("progress_achievement", "@player", "sync_glows", 1)
            });

            FlowService.RegisterFlow("glow_sync_stop", new[]
            {
                new FlowStep("stop_group_sync", "@target_entities"),
                new FlowStep("remove_sync_glow", "@target_entities"),
                new FlowStep("stop_sync_fx", "@target_entities"),
                new FlowStep("sendmessagetouser", "@player", "Group glow synchronization stopped"),
                new FlowStep("progress_achievement", "@player", "sync_stopped", 1)
            });
        }

        /// <summary>
        /// Get all glow flow names for registration.
        /// </summary>
        public static string[] GetGlowFlowNames()
        {
            return new[]
            {
                "glow_start", "glow_stop", "glow_pulse", "glow_fade", "glow_rainbow",
                "highlight_entity", "highlight_remove", "highlight_group", "highlight_clear_all",
                "player_marker_set", "player_marker_remove", "player_marker_update", "player_marker_visibility",
                "preview_glow_enable", "preview_glow_disable", "preview_glow_update",
                "interaction_glow_start", "interaction_glow_stop", "interaction_glow_range",
                "glow_admin_set_all", "glow_admin_clear_all", "glow_admin_entity",
                "glow_status", "glow_list_active",
                "glow_customize", "glow_preset_save", "glow_preset_load",
                "glow_conditional", "glow_health_based", "glow_distance_based",
                "glow_animate", "glow_animate_stop",
                "glow_sync_group", "glow_sync_stop"
            };
        }
    }
}
