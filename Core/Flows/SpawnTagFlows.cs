using System.Collections.Generic;
using VAutomationCore.Core.Api;
using VAutomationCore.Core.Services;

namespace VAutomationCore.Core.Flows
{
    /// <summary>
    /// Spawn tag flows for managing spawn tags, tag-based spawning, and dynamic spawn systems.
    /// Handles spawn tag management, tag validation, dynamic spawning, and spawn system mechanics.
    /// </summary>
    public static class SpawnTagFlows
    {
        /// <summary>
        /// Register all spawn tag flows with the FlowService.
        /// </summary>
        public static void RegisterSpawnTagFlows()
        {
            // Spawn tag management flows
            FlowService.RegisterFlow("spawntag_add", new[]
            {
                new FlowStep("add_spawn_tag", "@entity", "@tag"),
                new FlowStep("validate_spawn_tag", "@tag"),
                new FlowStep("sendmessagetouser", "@player", "Spawn tag added: @tag to @entity_name"),
                new FlowStep("progress_achievement", "@player", "spawn_tags_added", 1)
            });

            FlowService.RegisterFlow("spawntag_remove", new[]
            {
                new FlowStep("remove_spawn_tag", "@entity", "@tag"),
                new FlowStep("sendmessagetouser", "@player", "Spawn tag removed: @tag from @entity_name"),
                new FlowStep("progress_achievement", "@player", "spawn_tags_removed", 1)
            });

            FlowService.RegisterFlow("spawntag_clear_all", new[]
            {
                new FlowStep("clear_spawn_tags", "@entity"),
                new FlowStep("sendmessagetouser", "@player", "All spawn tags cleared from @entity_name"),
                new FlowStep("progress_achievement", "@player", "spawn_tags_cleared", 1)
            });

            FlowService.RegisterFlow("spawntag_check", new[]
            {
                new FlowStep("check_spawn_tag", "@entity", "@tag"),
                new FlowStep("sendmessagetouser", "@player", "Spawn tag check: @entity_name has @tag: @has_tag"),
                new FlowStep("progress_achievement", "@player", "spawn_tag_checks", 1)
            });

            // Tag-based spawning flows
            FlowService.RegisterFlow("spawntag_spawn_single", new[]
            {
                new FlowStep("validate_spawn_tag", "@tag"),
                new FlowStep("spawn_with_tag", "@tag", "@position", "@rotation"),
                new FlowStep("spawn_spawn_fx", "@position", "@spawn_fx"),
                new FlowStep("sendmessagetouser", "@player", "Spawned entity with tag @tag at @position"),
                new FlowStep("progress_achievement", "@player", "tag_spawns", 1)
            });

            FlowService.RegisterFlow("spawntag_spawn_multiple", new[]
            {
                new FlowStep("validate_spawn_tag", "@tag"),
                new FlowStep("spawn_multiple_with_tag", "@tag", "@center_position", "@count", "@spread_radius"),
                new FlowStep("spawn_batch_fx", "@center_position", "@batch_fx"),
                new FlowStep("sendmessagetouser", "@player", "Spawned @count entities with tag @tag"),
                new FlowStep("progress_achievement", "@player", "batch_spawns", 1)
            });

            FlowService.RegisterFlow("spawntag_spawn_area", new[]
            {
                new FlowStep("validate_spawn_tag", "@tag"),
                new FlowStep("spawn_area_with_tag", "@tag", "@area_center", "@area_radius", "@density"),
                new FlowStep("spawn_area_fx", "@area_center", "@area_fx"),
                new FlowStep("sendmessagetouser", "@player", "Spawned area with tag @tag: @spawn_count entities"),
                new FlowStep("progress_achievement", "@player", "area_spawns", 1)
            });

            FlowService.RegisterFlow("spawntag_spawn_wave", new[]
            {
                new FlowStep("validate_spawn_tag", "@tag"),
                new FlowStep("spawn_wave_with_tag", "@tag", "@spawn_points", "@wave_delay"),
                new FlowStep("spawn_wave_fx", "@spawn_points", "@wave_fx"),
                new FlowStep("sendmessagetouser", "@player", "Spawned wave with tag @tag: @wave_count entities"),
                new FlowStep("progress_achievement", "@player", "wave_spawns", 1)
            });

            // Dynamic spawn flows
            FlowService.RegisterFlow("spawntag_dynamic_spawn", new[]
            {
                new FlowStep("check_spawn_conditions", "@spawn_location"),
                new FlowStep("select_spawn_tag", "@spawn_location", "@criteria"),
                new FlowStep("spawn_with_tag", "@selected_tag", "@spawn_location", "@spawn_rotation"),
                new FlowStep("sendmessagetouser", "@player", "Dynamic spawn: @selected_tag at @spawn_location"),
                new FlowStep("progress_achievement", "@player", "dynamic_spawns", 1)
            });

            FlowService.RegisterFlow("spawntag_adaptive_spawn", new[]
            {
                new FlowStep("analyze_spawn_environment", "@spawn_location"),
                new FlowStep("select_adaptive_tag", "@spawn_location", "@environment_analysis"),
                new FlowStep("spawn_with_tag", "@adaptive_tag", "@spawn_location", "@adaptive_rotation"),
                new FlowStep("sendmessagetouser", "@player", "Adaptive spawn: @adaptive_tag at @spawn_location"),
                new FlowStep("progress_achievement", "@player", "adaptive_spawns", 1)
            });

            // Spawn tag validation flows
            FlowService.RegisterFlow("spawntag_validate", new[]
            {
                new FlowStep("validate_spawn_tag", "@tag"),
                new FlowStep("check_tag_permissions", "@tag", "@player"),
                new FlowStep("sendmessagetouser", "@player", "Spawn tag validation: @tag - @validation_result"),
                new FlowStep("progress_achievement", "@player", "tag_validations", 1)
            });

            FlowService.RegisterFlow("spawntag_validate_all", new[]
            {
                new FlowStep("validate_all_spawn_tags", "@entity"),
                new FlowStep("check_all_tag_permissions", "@entity", "@player"),
                new FlowStep("sendmessagetouser", "@player", "All spawn tags validated for @entity_name"),
                new FlowStep("progress_achievement", "@player", "all_tag_validations", 1)
            });

            // Spawn tag search flows
            FlowService.RegisterFlow("spawntag_find_entities", new[]
            {
                new FlowStep("find_entities_with_tag", "@tag"),
                new FlowStep("format_entity_list", "@found_entities"),
                new FlowStep("sendmessagetouser", "@player", "Found @entity_count entities with tag @tag"),
                new FlowStep("progress_achievement", "@player", "entity_searches", 1)
            });

            FlowService.RegisterFlow("spawntag_find_nearby", new[]
            {
                new FlowStep("find_entities_with_tag_nearby", "@tag", "@center", "@radius"),
                new FlowStep("format_nearby_list", "@nearby_entities"),
                new FlowStep("sendmessagetouser", "@player", "Found @nearby_count nearby entities with tag @tag"),
                new FlowStep("progress_achievement", "@player", "nearby_searches", 1)
            });

            // Spawn tag statistics flows
            FlowService.RegisterFlow("spawntag_statistics", new[]
            {
                new FlowStep("get_spawn_tag_statistics"),
                new FlowStep("format_statistics", "@tag_stats"),
                new FlowStep("sendmessagetouser", "@player", "Spawn tag statistics: @stats_info"),
                new FlowStep("progress_achievement", "@player", "statistics_checks", 1)
            });

            FlowService.RegisterFlow("spawntag_top_tags", new[]
            {
                new FlowStep("get_top_spawn_tags", "@limit"),
                new FlowStep("format_top_tags", "@top_tag_list"),
                new FlowStep("sendmessagetouser", "@player", "Top @limit spawn tags: @top_tags"),
                new FlowStep("progress_achievement", "@player", "top_tag_checks", 1)
            });

            // Admin spawn tag flows
            FlowService.RegisterFlow("spawntag_admin_spawn", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("admin_spawn_with_tag", "@admin_tag", "@target_position", "@admin_count"),
                new FlowStep("spawn_admin_fx", "@target_position", "@admin_fx"),
                new FlowStep("sendmessagetouser", "@requesting_admin", "Admin spawned @admin_count entities with tag @admin_tag"),
                new FlowStep("sendmessagetoall", "Admin spawned entities: @admin_tag")
            });

            FlowService.RegisterFlow("spawntag_admin_clear_all", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("clear_all_spawn_tags", "@all_entities"),
                new FlowStep("sendmessagetouser", "@requesting_admin", "Admin cleared all spawn tags"),
                new FlowStep("sendmessagetoall", "Admin cleared all spawn tags")
            });

            FlowService.RegisterFlow("spawntag_admin_mass_spawn", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("admin_mass_spawn_with_tag", "@admin_tag", "@spawn_positions"),
                new FlowStep("spawn_mass_fx", "@spawn_positions", "@mass_fx"),
                new FlowStep("sendmessagetouser", "@requesting_admin", "Admin mass spawned @spawn_count entities with tag @admin_tag"),
                new FlowStep("sendmessagetoall", "Admin mass spawn: @admin_tag")
            });

            // Spawn tag management flows
            FlowService.RegisterFlow("spawntag_batch_add", new[]
            {
                new FlowStep("add_spawn_tags_batch", "@entities", "@tags"),
                new FlowStep("validate_batch_tags", "@tags"),
                new FlowStep("sendmessagetouser", "@player", "Added @tag_count spawn tags to @entity_count entities"),
                new FlowStep("progress_achievement", "@player", "batch_tag_adds", 1)
            });

            FlowService.RegisterFlow("spawntag_batch_remove", new[]
            {
                new FlowStep("remove_spawn_tags_batch", "@entities", "@tags"),
                new FlowStep("sendmessagetouser", "@player", "Removed @tag_count spawn tags from @entity_count entities"),
                new FlowStep("progress_achievement", "@player", "batch_tag_removes", 1)
            });

            FlowService.RegisterFlow("spawntag_replace", new[]
            {
                new FlowStep("replace_spawn_tag", "@entity", "@old_tag", "@new_tag"),
                new FlowStep("validate_new_tag", "@new_tag"),
                new FlowStep("sendmessagetouser", "@player", "Replaced spawn tag @old_tag with @new_tag on @entity_name"),
                new FlowStep("progress_achievement", "@player", "tag_replacements", 1)
            });

            // Spawn tag filtering flows
            FlowService.RegisterFlow("spawntag_filter_entities", new[]
            {
                new FlowStep("filter_entities_by_tags", "@all_entities", "@include_tags", "@exclude_tags"),
                new FlowStep("format_filtered_list", "@filtered_entities"),
                new FlowStep("sendmessagetouser", "@player", "Filtered entities: @filtered_count entities match criteria"),
                new FlowStep("progress_achievement", "@player", "entity_filters", 1)
            });

            FlowService.RegisterFlow("spawntag_filter_spawn", new[]
            {
                new FlowStep("filter_spawn_by_tags", "@spawn_pool", "@allowed_tags"),
                new FlowStep("select_random_tag", "@allowed_tags"),
                new FlowStep("spawn_with_tag", "@selected_tag", "@spawn_position", "@spawn_rotation"),
                new FlowStep("sendmessagetouser", "@player", "Filtered spawn: @selected_tag at @spawn_position"),
                new FlowStep("progress_achievement", "@player", "filtered_spawns", 1)
            });

            // Spawn tag event flows
            FlowService.RegisterFlow("spawntag_on_spawn", new[]
            {
                new FlowStep("check_entity_spawn_tags", "@spawned_entity"),
                new FlowStep("trigger_spawn_events", "@spawned_entity", "@spawn_tags"),
                new FlowStep("apply_spawn_effects", "@spawned_entity", "@spawn_effects"),
                new FlowStep("sendmessagetouser", "@player", "Entity spawned with tags: @spawn_tags"),
                new FlowStep("progress_achievement", "@player", "spawn_events", 1)
            });

            FlowService.RegisterFlow("spawntag_on_despawn", new[]
            {
                new FlowStep("check_entity_spawn_tags", "@despawned_entity"),
                new FlowStep("trigger_despawn_events", "@despawned_entity", "@spawn_tags"),
                new FlowStep("cleanup_spawn_effects", "@despawned_entity", "@spawn_effects"),
                new FlowStep("sendmessagetouser", "@player", "Entity despawned with tags: @spawn_tags"),
                new FlowStep("progress_achievement", "@player", "despawn_events", 1)
            });
        }

        /// <summary>
        /// Get all spawn tag flow names for registration.
        /// </summary>
        public static string[] GetSpawnTagFlowNames()
        {
            return new[]
            {
                "spawntag_add", "spawntag_remove", "spawntag_clear_all", "spawntag_check",
                "spawntag_spawn_single", "spawntag_spawn_multiple", "spawntag_spawn_area", "spawntag_spawn_wave",
                "spawntag_dynamic_spawn", "spawntag_adaptive_spawn",
                "spawntag_validate", "spawntag_validate_all",
                "spawntag_find_entities", "spawntag_find_nearby",
                "spawntag_statistics", "spawntag_top_tags",
                "spawntag_admin_spawn", "spawntag_admin_clear_all", "spawntag_admin_mass_spawn",
                "spawntag_batch_add", "spawntag_batch_remove", "spawntag_replace",
                "spawntag_filter_entities", "spawntag_filter_spawn",
                "spawntag_on_spawn", "spawntag_on_despawn"
            };
        }
    }
}
