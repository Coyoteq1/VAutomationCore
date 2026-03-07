using System.Collections.Generic;
using VAutomationCore.Core.Api;
using VAutomationCore.Core.Services;

namespace VAutomationCore.Core.Flows
{
    /// <summary>
    /// Game objects flows for managing world objects, spawning, despawning, movement, and manipulation.
    /// Handles spawn/despawn, move/teleport, attach/detach, prefab instantiation, and object ownership.
    /// </summary>
    public static class GameObjectsFlows
    {
        /// <summary>
        /// Register all game objects flows with the FlowService.
        /// </summary>
        public static void RegisterGameObjectsFlows()
        {
            // Spawn flows
            FlowService.RegisterFlow("spawn_object", new[]
            {
                new FlowStep("spawn_entity", "@prefab_guid", "@position", "@rotation"),
                new FlowStep("set_object_owner", "@spawned_entity", "@player"),
                new FlowStep("sendmessagetouser", "@player", "Object spawned: @object_name"),
                new FlowStep("progress_achievement", "@player", "objects_spawned", 1)
            });

            FlowService.RegisterFlow("spawn_prefab_at_location", new[]
            {
                new FlowStep("spawn_entity", "@prefab_guid", "@location", "@rotation"),
                new FlowStep("set_object_position", "@spawned_entity", "@location"),
                new FlowStep("sendmessagetouser", "@player", "Prefab spawned at @location_name"),
                new FlowStep("progress_achievement", "@player", "prefabs_spawned", 1)
            });

            FlowService.RegisterFlow("spawn_multiple_objects", new[]
            {
                new FlowStep("spawn_entities_batch", "@prefabs", "@positions", "@rotations"),
                new FlowStep("set_batch_ownership", "@spawned_entities", "@player"),
                new FlowStep("sendmessagetouser", "@player", "Spawned @spawn_count objects"),
                new FlowStep("progress_achievement", "@player", "batch_spawns", 1)
            });

            // Despawn flows
            FlowService.RegisterFlow("despawn_object", new[]
            {
                new FlowStep("despawn_entity", "@target_entity"),
                new FlowStep("cleanup_object_references", "@target_entity"),
                new FlowStep("sendmessagetouser", "@player", "Object despawned: @object_name"),
                new FlowStep("progress_achievement", "@player", "objects_despawned", 1)
            });

            FlowService.RegisterFlow("despawn_area_objects", new[]
            {
                new FlowStep("despawn_entities_in_area", "@center", "@radius"),
                new FlowStep("cleanup_area_references", "@center", "@radius"),
                new FlowStep("sendmessagetouser", "@player", "Despawned @despawn_count objects in area"),
                new FlowStep("progress_achievement", "@player", "area_despawns", 1)
            });

            FlowService.RegisterFlow("despawn_all_owned", new[]
            {
                new FlowStep("despawn_player_entities", "@player"),
                new FlowStep("cleanup_player_references", "@player"),
                new FlowStep("sendmessagetouser", "@player", "Despawned all your objects"),
                new FlowStep("progress_achievement", "@player", "all_despawned", 1)
            });

            // Movement flows
            FlowService.RegisterFlow("move_object", new[]
            {
                new FlowStep("move_entity_to", "@target_entity", "@new_position"),
                new FlowStep("update_object_position", "@target_entity", "@new_position"),
                new FlowStep("sendmessagetouser", "@player", "Object moved to @new_position"),
                new FlowStep("progress_achievement", "@player", "objects_moved", 1)
            });

            FlowService.RegisterFlow("teleport_object", new[]
            {
                new FlowStep("teleport_entity", "@target_entity", "@destination"),
                new FlowStep("spawn_teleport_fx", "@destination", "@teleport_fx"),
                new FlowStep("update_entity_position", "@target_entity", "@destination"),
                new FlowStep("sendmessagetouser", "@player", "Object teleported to @destination_name"),
                new FlowStep("progress_achievement", "@player", "objects_teleported", 1)
            });

            FlowService.RegisterFlow("move_object_to_player", new[]
            {
                new FlowStep("move_entity_to_player", "@target_entity", "@player"),
                new FlowStep("update_entity_position", "@target_entity", "@player_position"),
                new FlowStep("sendmessagetouser", "@player", "Object moved to your location"),
                new FlowStep("progress_achievement", "@player", "objects_moved_to_player", 1)
            });

            // Attach/Detach flows
            FlowService.RegisterFlow("attach_to_entity", new[]
            {
                new FlowStep("attach_entity", "@object_entity", "@target_entity", "@attachment_point"),
                new FlowStep("set_attachment_offset", "@object_entity", "@offset"),
                new FlowStep("sendmessagetouser", "@player", "Object attached to @target_entity_name"),
                new FlowStep("progress_achievement", "@player", "objects_attached", 1)
            });

            FlowService.RegisterFlow("detach_from_entity", new[]
            {
                new FlowStep("detach_entity", "@object_entity", "@target_entity"),
                new FlowStep("restore_entity_position", "@object_entity", "@original_position"),
                new FlowStep("sendmessagetouser", "@player", "Object detached from @target_entity_name"),
                new FlowStep("progress_achievement", "@player", "objects_detached", 1)
            });

            FlowService.RegisterFlow("detach_all_from_entity", new[]
            {
                new FlowStep("detach_all_entities", "@target_entity"),
                new FlowStep("restore_all_positions", "@target_entity"),
                new FlowStep("sendmessagetouser", "@player", "All objects detached from @target_entity_name"),
                new FlowStep("progress_achievement", "@player", "all_detached", 1)
            });

            // Prefab instantiation flows
            FlowService.RegisterFlow("instantiate_prefab", new[]
            {
                new FlowStep("create_prefab_instance", "@prefab_guid", "@position", "@rotation"),
                new FlowStep("apply_prefab_modifications", "@instance", "@modifications"),
                new FlowStep("sendmessagetouser", "@player", "Prefab instantiated: @prefab_name"),
                new FlowStep("progress_achievement", "@player", "prefabs_instantiated", 1)
            });

            FlowService.RegisterFlow("instantiate_prefab_with_data", new[]
            {
                new FlowStep("create_prefab_instance", "@prefab_guid", "@position", "@rotation"),
                new FlowStep("apply_prefab_data", "@instance", "@prefab_data"),
                new FlowStep("sendmessagetouser", "@player", "Prefab with data instantiated: @prefab_name"),
                new FlowStep("progress_achievement", "@player", "data_prefabs_instantiated", 1)
            });

            FlowService.RegisterFlow("instantiate_prefab_variant", new[]
            {
                new FlowStep("select_prefab_variant", "@base_prefab", "@variant_criteria"),
                new FlowStep("create_variant_instance", "@selected_variant", "@position", "@rotation"),
                new FlowStep("apply_variant_modifications", "@instance", "@variant_mods"),
                new FlowStep("sendmessagetouser", "@player", "Prefab variant instantiated: @variant_name"),
                new FlowStep("progress_achievement", "@player", "variant_prefabs_instantiated", 1)
            });

            // Object ownership flows
            FlowService.RegisterFlow("set_object_owner", new[]
            {
                new FlowStep("transfer_ownership", "@target_entity", "@new_owner"),
                new FlowStep("update_owner_permissions", "@target_entity", "@new_owner"),
                new FlowStep("sendmessagetouser", "@new_owner", "You now own: @object_name"),
                new FlowStep("sendmessagetouser", "@old_owner", "You no longer own: @object_name"),
                new FlowStep("progress_achievement", "@new_owner", "objects_owned", 1)
            });

            FlowService.RegisterFlow("clear_object_owner", new[]
            {
                new FlowStep("clear_ownership", "@target_entity"),
                new FlowStep("remove_owner_permissions", "@target_entity"),
                new FlowStep("sendmessagetouser", "@player", "Ownership cleared for: @object_name"),
                new FlowStep("progress_achievement", "@player", "ownership_cleared", 1)
            });

            FlowService.RegisterFlow("transfer_object_ownership", new[]
            {
                new FlowStep("transfer_ownership", "@target_entity", "@new_owner"),
                new FlowStep("update_owner_permissions", "@target_entity", "@new_owner"),
                new FlowStep("sendmessagetouser", "@new_owner", "Ownership transferred: @object_name"),
                new FlowStep("sendmessagetouser", "@old_owner", "You transferred: @object_name"),
                new FlowStep("progress_achievement", "@new_owner", "ownership_transfers", 1)
            });

            // Object state flows
            FlowService.RegisterFlow("activate_object", new[]
            {
                new FlowStep("activate_entity", "@target_entity"),
                new FlowStep("apply_activation_effects", "@target_entity", "@activation_effects"),
                new FlowStep("sendmessagetouser", "@player", "Object activated: @object_name"),
                new FlowStep("progress_achievement", "@player", "objects_activated", 1)
            });

            FlowService.RegisterFlow("deactivate_object", new[]
            {
                new FlowStep("deactivate_entity", "@target_entity"),
                new FlowStep("remove_activation_effects", "@target_entity"),
                new FlowStep("sendmessagetouser", "@player", "Object deactivated: @object_name"),
                new FlowStep("progress_achievement", "@player", "objects_deactivated", 1)
            });

            FlowService.RegisterFlow("reset_object", new[]
            {
                new FlowStep("reset_entity_state", "@target_entity"),
                new FlowStep("restore_default_properties", "@target_entity"),
                new FlowStep("sendmessagetouser", "@player", "Object reset: @object_name"),
                new FlowStep("progress_achievement", "@player", "objects_reset", 1)
            });

            // Admin object flows
            FlowService.RegisterFlow("admin_spawn_object", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("spawn_entity", "@admin_prefab", "@admin_position", "@admin_rotation"),
                new FlowStep("set_admin_ownership", "@spawned_entity", "@admin_owner"),
                new FlowStep("sendmessagetouser", "@requesting_admin", "Admin object spawned: @object_name"),
                new FlowStep("sendmessagetoall", "Admin spawned: @object_name")
            });

            FlowService.RegisterFlow("admin_despawn_all", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("despawn_all_entities", "@all_entities"),
                new FlowStep("cleanup_all_references", "@all_entities"),
                new FlowStep("sendmessagetouser", "@requesting_admin", "Admin despawned all objects"),
                new FlowStep("sendmessagetoall", "Admin cleared all objects")
            });

            FlowService.RegisterFlow("admin_move_all", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("move_all_entities", "@all_entities", "@admin_destination"),
                new FlowStep("update_all_positions", "@all_entities", "@admin_destination"),
                new FlowStep("sendmessagetouser", "@requesting_admin", "Admin moved all objects"),
                new FlowStep("sendmessagetoall", "Admin moved all objects to new location")
            });

            // Object query flows
            FlowService.RegisterFlow("object_query_nearby", new[]
            {
                new FlowStep("query_nearby_entities", "@center", "@radius", "@query_type"),
                new FlowStep("format_query_results", "@query_results"),
                new FlowStep("sendmessagetouser", "@player", "Found @result_count nearby objects"),
                new FlowStep("progress_achievement", "@player", "object_queries", 1)
            });

            FlowService.RegisterFlow("object_query_owned", new[]
            {
                new FlowStep("query_owned_entities", "@player"),
                new FlowStep("format_owned_results", "@owned_entities"),
                new FlowStep("sendmessagetouser", "@player", "You own @owned_count objects"),
                new FlowStep("progress_achievement", "@player", "ownership_checks", 1)
            });

            FlowService.RegisterFlow("object_query_by_type", new[]
            {
                new FlowStep("query_entities_by_type", "@entity_type", "@search_area"),
                new FlowStep("format_type_results", "@type_results"),
                new FlowStep("sendmessagetouser", "@player", "Found @result_count objects of type @entity_type"),
                new FlowStep("progress_achievement", "@player", "type_queries", 1)
            });

            // Object persistence flows
            FlowService.RegisterFlow("persist_object", new[]
            {
                new FlowStep("save_object_state", "@target_entity"),
                new FlowStep("store_persistence_data", "@target_entity", "@persistence_data"),
                new FlowStep("sendmessagetouser", "@player", "Object persisted: @object_name"),
                new FlowStep("progress_achievement", "@player", "objects_persisted", 1)
            });

            FlowService.RegisterFlow("restore_object", new[]
            {
                new FlowStep("load_object_state", "@target_entity"),
                new FlowStep("restore_persistence_data", "@target_entity", "@persistence_data"),
                new FlowStep("sendmessagetouser", "@player", "Object restored: @object_name"),
                new FlowStep("progress_achievement", "@player", "objects_restored", 1)
            });

            // Object cleanup flows
            FlowService.RegisterFlow("cleanup_object", new[]
            {
                new FlowStep("cleanup_entity_references", "@target_entity"),
                new FlowStep("remove_object_data", "@target_entity"),
                new FlowStep("sendmessagetouser", "@player", "Object cleaned up: @object_name"),
                new FlowStep("progress_achievement", "@player", "objects_cleaned", 1)
            });

            FlowService.RegisterFlow("cleanup_area", new[]
            {
                new FlowStep("cleanup_area_entities", "@center", "@radius"),
                new FlowStep("remove_area_data", "@center", "@radius"),
                new FlowStep("sendmessagetouser", "@player", "Area cleaned up: @cleanup_count objects"),
                new FlowStep("progress_achievement", "@player", "area_cleanups", 1)
            });
        }

        /// <summary>
        /// Get all game objects flow names for registration.
        /// </summary>
        public static string[] GetGameObjectsFlowNames()
        {
            return new[]
            {
                "spawn_object", "spawn_prefab_at_location", "spawn_multiple_objects",
                "despawn_object", "despawn_area_objects", "despawn_all_owned",
                "move_object", "teleport_object", "move_object_to_player",
                "attach_to_entity", "detach_from_entity", "detach_all_from_entity",
                "instantiate_prefab", "instantiate_prefab_with_data", "instantiate_prefab_variant",
                "set_object_owner", "clear_object_owner", "transfer_object_ownership",
                "activate_object", "deactivate_object", "reset_object",
                "admin_spawn_object", "admin_despawn_all", "admin_move_all",
                "object_query_nearby", "object_query_owned", "object_query_by_type",
                "persist_object", "restore_object",
                "cleanup_object", "cleanup_area"
            };
        }
    }
}
