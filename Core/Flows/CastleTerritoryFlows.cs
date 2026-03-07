using System.Collections.Generic;
using VAutomationCore.Core.Api;
using VAutomationCore.Core.Services;

namespace VAutomationCore.Core.Flows
{
    /// <summary>
    /// Castle territory flows for managing territory drawing, boundaries, and visualization.
    /// Handles territory creation, expansion, visualization, and territory management mechanics.
    /// </summary>
    public static class CastleTerritoryFlows
    {
        /// <summary>
        /// Register all castle territory flows with the FlowService.
        /// </summary>
        public static void RegisterCastleTerritoryFlows()
        {
            // Territory drawing flows
            FlowService.RegisterFlow("territory_draw", new[]
            {
                new FlowStep("draw_castle_territory", "@castle", "@center", "@radius"),
                new FlowStep("set_territory_visualization", "@castle", "@territory_fx", "@territory_color"),
                new FlowStep("spawn_territory_boundary", "@castle", "@boundary_type"),
                new FlowStep("sendmessagetouser", "@player", "Territory drawn for @castle_name: radius @radius"),
                new FlowStep("progress_achievement", "@player", "territories_drawn", 1)
            });

            FlowService.RegisterFlow("territory_update", new[]
            {
                new FlowStep("update_castle_territory", "@castle", "@new_center", "@new_radius"),
                new FlowStep("update_territory_visualization", "@castle", "@new_visualization"),
                new FlowStep("update_territory_boundary", "@castle", "@new_boundary"),
                new FlowStep("sendmessagetouser", "@player", "Territory updated for @castle_name"),
                new FlowStep("progress_achievement", "@player", "territories_updated", 1)
            });

            FlowService.RegisterFlow("territory_clear", new[]
            {
                new FlowStep("clear_castle_territory", "@castle"),
                new FlowStep("remove_territory_visualization", "@castle"),
                new FlowStep("clear_territory_boundary", "@castle"),
                new FlowStep("sendmessagetouser", "@player", "Territory cleared for @castle_name"),
                new FlowStep("progress_achievement", "@player", "territories_cleared", 1)
            });

            FlowService.RegisterFlow("territory_expand", new[]
            {
                new FlowStep("expand_castle_territory", "@castle", "@expansion_radius"),
                new FlowStep("update_territory_visualization", "@castle", "@expanded_visualization"),
                new FlowStep("spawn_expansion_fx", "@castle", "@expansion_fx"),
                new FlowStep("sendmessagetouser", "@player", "Territory expanded for @castle_name to @new_radius"),
                new FlowStep("progress_achievement", "@player", "territories_expanded", 1)
            });

            FlowService.RegisterFlow("territory_shrink", new[]
            {
                new FlowStep("shrink_castle_territory", "@castle", "@shrink_radius"),
                new FlowStep("update_territory_visualization", "@castle", "@shrunk_visualization"),
                new FlowStep("spawn_shrink_fx", "@castle", "@shrink_fx"),
                new FlowStep("sendmessagetouser", "@player", "Territory shrunk for @castle_name to @new_radius"),
                new FlowStep("progress_achievement", "@player", "territories_shrunk", 1)
            });

            // Territory management flows
            FlowService.RegisterFlow("territory_claim", new[]
            {
                new FlowStep("draw_castle_territory", "@castle", "@center", "@default_radius"),
                new FlowStep("set_territory_ownership", "@castle", "@player"),
                new FlowStep("apply_territory_buffs", "@castle", "@ownership_buffs"),
                new FlowStep("set_territory_visualization", "@castle", "@player_color_fx"),
                new FlowStep("sendmessagetouser", "@player", "Territory claimed for @castle_name"),
                new FlowStep("progress_achievement", "@player", "territories_claimed", 1)
            });

            FlowService.RegisterFlow("territory_abandon", new[]
            {
                new FlowStep("remove_territory_ownership", "@castle"),
                new FlowStep("remove_territory_buffs", "@castle"),
                new FlowStep("clear_territory_visualization", "@castle"),
                new FlowStep("clear_castle_territory", "@castle"),
                new FlowStep("sendmessagetouser", "@player", "Territory abandoned for @castle_name"),
                new FlowStep("progress_achievement", "@player", "territories_abandoned", 1)
            });

            FlowService.RegisterFlow("territory_transfer", new[]
            {
                new FlowStep("remove_territory_ownership", "@castle"),
                new FlowStep("set_territory_ownership", "@castle", "@new_owner"),
                new FlowStep("update_territory_visualization", "@castle", "@new_owner_fx"),
                new FlowStep("apply_transfer_buffs", "@castle", "@transfer_buffs"),
                new FlowStep("sendmessagetouser", "@new_owner", "Territory transferred: @castle_name"),
                new FlowStep("sendmessagetouser", "@old_owner", "Territory transferred to @new_owner_name"),
                new FlowStep("progress_achievement", "@new_owner", "territories_received", 1)
            });

            // Territory boundary flows
            FlowService.RegisterFlow("territory_boundary_create", new[]
            {
                new FlowStep("create_territory_boundary", "@castle", "@boundary_type"),
                new FlowStep("spawn_boundary_fx", "@castle", "@boundary_fx"),
                new FlowStep("set_boundary_collision", "@castle", "@collision_type"),
                new FlowStep("sendmessagetouser", "@player", "Boundary created for @castle_name"),
                new FlowStep("progress_achievement", "@player", "boundaries_created", 1)
            });

            FlowService.RegisterFlow("territory_boundary_remove", new[]
            {
                new FlowStep("remove_territory_boundary", "@castle"),
                new FlowStep("stop_boundary_fx", "@castle"),
                new FlowStep("clear_boundary_collision", "@castle"),
                new FlowStep("sendmessagetouser", "@player", "Boundary removed for @castle_name"),
                new FlowStep("progress_achievement", "@player", "boundaries_removed", 1)
            });

            FlowService.RegisterFlow("territory_boundary_update", new[]
            {
                new FlowStep("update_territory_boundary", "@castle", "@new_boundary_type"),
                new FlowStep("update_boundary_fx", "@castle", "@new_boundary_fx"),
                new FlowStep("update_boundary_collision", "@castle", "@new_collision_type"),
                new FlowStep("sendmessagetouser", "@player", "Boundary updated for @castle_name"),
                new FlowStep("progress_achievement", "@player", "boundaries_updated", 1)
            });

            // Territory visualization flows
            FlowService.RegisterFlow("territory_visualization_set", new[]
            {
                new FlowStep("set_territory_visualization", "@castle", "@visualization_fx", "@territory_color"),
                new FlowStep("spawn_territory_markers", "@castle", "@marker_type"),
                new FlowStep("update_territory_ui", "@castle", "@ui_elements"),
                new FlowStep("sendmessagetouser", "@player", "Territory visualization set for @castle_name"),
                new FlowStep("progress_achievement", "@player", "visualizations_set", 1)
            });

            FlowService.RegisterFlow("territory_visualization_update", new[]
            {
                new FlowStep("update_territory_visualization", "@castle", "@new_visualization_fx", "@new_color"),
                new FlowStep("update_territory_markers", "@castle", "@new_marker_type"),
                new FlowStep("refresh_territory_ui", "@castle"),
                new FlowStep("sendmessagetouser", "@player", "Territory visualization updated for @castle_name"),
                new FlowStep("progress_achievement", "@player", "visualizations_updated", 1)
            });

            FlowService.RegisterFlow("territory_visualization_hide", new[]
            {
                new FlowStep("hide_territory_visualization", "@castle"),
                new FlowStep("hide_territory_markers", "@castle"),
                new FlowStep("hide_territory_ui", "@castle"),
                new FlowStep("sendmessagetouser", "@player", "Territory visualization hidden for @castle_name"),
                new FlowStep("progress_achievement", "@player", "visualizations_hidden", 1)
            });

            FlowService.RegisterFlow("territory_visualization_show", new[]
            {
                new FlowStep("show_territory_visualization", "@castle"),
                new FlowStep("show_territory_markers", "@castle"),
                new FlowStep("show_territory_ui", "@castle"),
                new FlowStep("sendmessagetouser", "@player", "Terrory visualization shown for @castle_name"),
                new FlowStep("progress_achievement", "@player", "visualizations_shown", 1)
            });

            // Territory conflict flows
            FlowService.RegisterFlow("territory_conflict_check", new[]
            {
                new FlowStep("check_territory_overlaps", "@castle"),
                new FlowStep("identify_conflicting_territories", "@castle", "@conflicts"),
                new FlowStep("notify_territory_conflicts", "@castle", "@conflict_list"),
                new FlowStep("sendmessagetouser", "@player", "Territory conflicts detected: @conflict_count"),
                new FlowStep("progress_achievement", "@player", "conflict_checks", 1)
            });

            FlowService.RegisterFlow("territory_conflict_resolve", new[]
            {
                new FlowStep("resolve_territory_conflict", "@castle", "@conflicting_castle", "@resolution_type"),
                new FlowStep("adjust_territory_boundaries", "@castle", "@conflicting_castle"),
                new FlowStep("update_territory_visualization", "@castle", "@post_conflict_visualization"),
                new FlowStep("sendmessagetouser", "@player", "Territory conflict resolved: @resolution_type"),
                new FlowStep("progress_achievement", "@player", "conflicts_resolved", 1)
            });

            // Territory admin flows
            FlowService.RegisterFlow("territory_admin_draw", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("admin_draw_territory", "@target_location", "@admin_radius"),
                new FlowStep("set_admin_territory_visualization", "@new_territory", "@admin_visualization"),
                new FlowStep("sendmessagetouser", "@requesting_admin", "Admin territory drawn at @target_location"),
                new FlowStep("sendmessagetoall", "Admin territory created: radius @admin_radius")
            });

            FlowService.RegisterFlow("territory_admin_clear_all", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("clear_all_territories", "@all_territories"),
                new FlowStep("remove_all_territory_visualizations", "@all_territories"),
                new FlowStep("sendmessagetouser", "@requesting_admin", "Admin cleared all territories"),
                new FlowStep("sendmessagetoall", "Admin cleared all territories")
            });

            FlowService.RegisterFlow("territory_admin_resize", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("admin_resize_territory", "@target_territory", "@admin_radius"),
                new FlowStep("update_admin_territory_visualization", "@target_territory", "@admin_visualization"),
                new FlowStep("sendmessagetouser", "@requesting_admin", "Admin resized territory to @admin_radius"),
                new FlowStep("sendmessagetoall", "Admin resized territory: @territory_name")
            });

            // Territory status flows
            FlowService.RegisterFlow("territory_status", new[]
            {
                new FlowStep("check_territory_status", "@castle"),
                new FlowStep("check_territory_boundaries", "@castle"),
                new FlowStep("check_territory_visualization", "@castle"),
                new FlowStep("sendmessagetouser", "@player", "Territory status: @territory_info"),
                new FlowStep("progress_achievement", "@player", "territory_checks", 1)
            });

            FlowService.RegisterFlow("territory_list", new[]
            {
                new FlowStep("get_player_territories", "@player"),
                new FlowStep("format_territory_list", "@territories"),
                new FlowStep("sendmessagetouser", "@player", "Your territories: @territory_list"),
                new FlowStep("progress_achievement", "@player", "territory_lists", 1)
            });

            // Territory point check flows
            FlowService.RegisterFlow("territory_point_check", new[]
            {
                new FlowStep("check_point_in_territories", "@point"),
                new FlowStep("get_containing_territories", "@point", "@containing_territories"),
                new FlowStep("sendmessagetouser", "@player", "Point @point is in territories: @territory_names"),
                new FlowStep("progress_achievement", "@player", "point_checks", 1)
            });

            FlowService.RegisterFlow("territory_player_check", new[]
            {
                new FlowStep("check_player_in_territories", "@player"),
                new FlowStep("get_player_territory_status", "@player", "@territory_status"),
                new FlowStep("sendmessagetouser", "@player", "You are in territories: @territory_names"),
                new FlowStep("progress_achievement", "@player", "territory_entries", 1)
            });
        }

        /// <summary>
        /// Get all castle territory flow names for registration.
        /// </summary>
        public static string[] GetCastleTerritoryFlowNames()
        {
            return new[]
            {
                "territory_draw", "territory_update", "territory_clear", "territory_expand", "territory_shrink",
                "territory_claim", "territory_abandon", "territory_transfer",
                "territory_boundary_create", "territory_boundary_remove", "territory_boundary_update",
                "territory_visualization_set", "territory_visualization_update", "territory_visualization_hide", "territory_visualization_show",
                "territory_conflict_check", "territory_conflict_resolve",
                "territory_admin_draw", "territory_admin_clear_all", "territory_admin_resize",
                "territory_status", "territory_list", "territory_point_check", "territory_player_check"
            };
        }
    }
}
