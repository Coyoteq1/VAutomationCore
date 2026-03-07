using System;
using System.Collections.Generic;
using System.Linq;
using VAutomationCore.Core.Flows;
using VAutomationCore.Core.Logging;

namespace VAutomationCore.Core.Api
{
    /// <summary>
    /// Centralized flow registry system for managing all gameplay-domain flows.
    /// Provides flow registration, discovery, and management capabilities for the new flow architecture.
    /// </summary>
    public static class FlowRegistrySystem
    {
        private static readonly CoreLogger Log = new("FlowRegistrySystem");
        
        // Flow domain categories
        private static readonly Dictionary<FlowDomain, List<FlowInfo>> _flowsByDomain = new();
        private static readonly Dictionary<string, FlowInfo> _flowsByName = new();
        private static readonly Dictionary<string, FlowDomain> _flowDomains = new();
        private static bool _isInitialized = false;

        /// <summary>
        /// Flow domain enumeration for categorizing flows by gameplay domain.
        /// </summary>
        public enum FlowDomain
        {
            GameObjects,
            Glow,
            VBlood,
            Abilities,
            FXAndGameObjects,
            EquipmentAndKits,
            Zone,
            Arena,
            PlacementRestriction,
            CastleBuilding,
            CastleTerritory,
            SpawnTag,
            VisibilityAndStealth
        }

        /// <summary>
        /// Flow information structure containing metadata about each flow.
        /// </summary>
        public class FlowInfo
        {
            public string Name { get; set; }
            public FlowDomain Domain { get; set; }
            public string Description { get; set; }
            public string[] Parameters { get; set; }
            public bool IsAdminOnly { get; set; }
            public bool RequiresPermission { get; set; }
            public DateTime RegisteredAt { get; set; }
            public string[] Tags { get; set; }

            public FlowInfo(string name, FlowDomain domain, string description, string[] parameters = null, bool isAdminOnly = false, bool requiresPermission = false, string[] tags = null)
            {
                Name = name;
                Domain = domain;
                Description = description;
                Parameters = parameters ?? Array.Empty<string>();
                IsAdminOnly = isAdminOnly;
                RequiresPermission = requiresPermission;
                RegisteredAt = DateTime.UtcNow;
                Tags = tags ?? Array.Empty<string>();
            }
        }

        /// <summary>
        /// Initialize the flow registry system with all gameplay-domain flows.
        /// </summary>
        public static void Initialize()
        {
            if (_isInitialized)
            {
                Log.LogWarning("FlowRegistrySystem already initialized");
                return;
            }

            try
            {
                Log.LogInfo("Initializing FlowRegistrySystem...");

                // Register all flow domains
                RegisterGameObjectsFlows();
                RegisterGlowFlows();
                RegisterVBloodFlows();
                RegisterAbilitiesFlows();
                RegisterFXAndGameObjectsFlows();
                RegisterEquipmentAndKitsFlows();
                RegisterZoneFlows();
                RegisterZoneRulesFlows();
                RegisterArenaFlows();
                RegisterPlacementRestrictionFlows();
                RegisterCastleBuildingFlows();
                RegisterCastleTerritoryFlows();
                RegisterSpawnTagFlows();
                RegisterVisibilityAndStealthFlows();

                _isInitialized = true;
                Log.LogInfo($"FlowRegistrySystem initialized with {_flowsByName.Count} flows across {_flowsByDomain.Count} domains");
            }
            catch (Exception ex)
            {
                Log.LogError($"Failed to initialize FlowRegistrySystem: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Register GameObjects flows.
        /// </summary>
        private static void RegisterGameObjectsFlows()
        {
            var domain = FlowDomain.GameObjects;
            var flows = new List<FlowInfo>
            {
                new FlowInfo("spawn_object", domain, "Spawn a game object at specified position", new[] { "prefab_guid", "position", "rotation" }, false, false, new[] { "spawn", "creation" }),
                new FlowInfo("spawn_prefab_at_location", domain, "Spawn a prefab at a specific location", new[] { "prefab_guid", "location", "rotation" }, false, false, new[] { "spawn", "prefab" }),
                new FlowInfo("spawn_multiple_objects", domain, "Spawn multiple objects at once", new[] { "prefabs", "positions", "rotations" }, false, false, new[] { "spawn", "batch" }),
                new FlowInfo("despawn_object", domain, "Despawn a specific game object", new[] { "target_entity" }, false, false, new[] { "despawn", "cleanup" }),
                new FlowInfo("despawn_area_objects", domain, "Despawn all objects in an area", new[] { "center", "radius" }, false, false, new[] { "despawn", "area" }),
                new FlowInfo("despawn_all_owned", domain, "Despawn all objects owned by a player", new[] { "player" }, false, false, new[] { "despawn", "ownership" }),
                new FlowInfo("move_object", domain, "Move an object to a new position", new[] { "target_entity", "new_position" }, false, false, new[] { "movement", "position" }),
                new FlowInfo("teleport_object", domain, "Teleport an object with effects", new[] { "target_entity", "destination" }, false, false, new[] { "teleport", "movement" }),
                new FlowInfo("move_object_to_player", domain, "Move object to player's location", new[] { "target_entity", "player" }, false, false, new[] { "movement", "player" }),
                new FlowInfo("attach_to_entity", domain, "Attach one entity to another", new[] { "object_entity", "target_entity", "attachment_point" }, false, false, new[] { "attach", "relationship" }),
                new FlowInfo("detach_from_entity", domain, "Detach an entity from its parent", new[] { "object_entity", "target_entity" }, false, false, new[] { "detach", "relationship" }),
                new FlowInfo("detach_all_from_entity", domain, "Detach all entities from a target", new[] { "target_entity" }, false, false, new[] { "detach", "batch" }),
                new FlowInfo("instantiate_prefab", domain, "Instantiate a prefab with modifications", new[] { "prefab_guid", "position", "rotation" }, false, false, new[] { "prefab", "instantiation" }),
                new FlowInfo("set_object_owner", domain, "Transfer ownership of an object", new[] { "target_entity", "new_owner" }, false, false, new[] { "ownership", "transfer" }),
                new FlowInfo("clear_object_owner", domain, "Clear ownership of an object", new[] { "target_entity" }, false, false, new[] { "ownership", "clear" }),
                new FlowInfo("transfer_object_ownership", domain, "Transfer object ownership between players", new[] { "target_entity", "new_owner" }, false, false, new[] { "ownership", "transfer" }),
                new FlowInfo("activate_object", domain, "Activate a game object", new[] { "target_entity" }, false, false, new[] { "activation", "state" }),
                new FlowInfo("deactivate_object", domain, "Deactivate a game object", new[] { "target_entity" }, false, false, new[] { "deactivation", "state" }),
                new FlowInfo("reset_object", domain, "Reset object to default state", new[] { "target_entity" }, false, false, new[] { "reset", "state" }),
                new FlowInfo("admin_spawn_object", domain, "Admin: spawn an object", new[] { "admin_prefab", "admin_position", "admin_rotation" }, true, false, new[] { "admin", "spawn" }),
                new FlowInfo("admin_despawn_all", domain, "Admin: despawn all objects", new[] { "all_entities" }, true, false, new[] { "admin", "despawn" }),
                new FlowInfo("admin_move_all", domain, "Admin: move all objects", new[] { "all_entities", "admin_destination" }, true, false, new[] { "admin", "movement" }),
                new FlowInfo("object_query_nearby", domain, "Query for nearby objects", new[] { "center", "radius", "query_type" }, false, false, new[] { "query", "search" }),
                new FlowInfo("object_query_owned", domain, "Query objects owned by a player", new[] { "player" }, false, false, new[] { "query", "ownership" }),
                new FlowInfo("object_query_by_type", domain, "Query objects by type", new[] { "entity_type", "search_area" }, false, false, new[] { "query", "type" }),
                new FlowInfo("persist_object", domain, "Persist an object's state", new[] { "target_entity" }, false, false, new[] { "persistence", "save" }),
                new FlowInfo("restore_object", domain, "Restore an object's persisted state", new[] { "target_entity" }, false, false, new[] { "persistence", "restore" }),
                new FlowInfo("cleanup_object", domain, "Clean up an object", new[] { "target_entity" }, false, false, new[] { "cleanup", "maintenance" }),
                new FlowInfo("cleanup_area", domain, "Clean up all objects in an area", new[] { "center", "radius" }, false, false, new[] { "cleanup", "area" })
            };

            RegisterFlows(domain, flows);
        }

        /// <summary>
        /// Register Glow flows.
        /// </summary>
        private static void RegisterGlowFlows()
        {
            var domain = FlowDomain.Glow;
            var flows = new List<FlowInfo>
            {
                new FlowInfo("glow_start", domain, "Start a glow effect on an entity", new[] { "target_entity", "glow_type", "glow_color", "glow_intensity" }, false, false, new[] { "glow", "visual", "effect" }),
                new FlowInfo("glow_stop", domain, "Stop a glow effect", new[] { "target_entity" }, false, false, new[] { "glow", "visual", "stop" }),
                new FlowInfo("glow_pulse", domain, "Apply a pulsing glow effect", new[] { "target_entity", "pulse_color", "pulse_speed" }, false, false, new[] { "glow", "pulse", "animation" }),
                new FlowInfo("glow_fade", domain, "Apply a fading glow effect", new[] { "target_entity", "fade_color", "fade_duration" }, false, false, new[] { "glow", "fade", "animation" }),
                new FlowInfo("glow_rainbow", domain, "Apply a rainbow glow effect", new[] { "target_entity", "rainbow_speed" }, false, false, new[] { "glow", "rainbow", "animation" }),
                new FlowInfo("highlight_entity", domain, "Highlight an entity with outline", new[] { "target_entity", "outline_color", "outline_thickness" }, false, false, new[] { "highlight", "outline", "visual" }),
                new FlowInfo("highlight_remove", domain, "Remove highlight from an entity", new[] { "target_entity" }, false, false, new[] { "highlight", "remove", "visual" }),
                new FlowInfo("highlight_group", domain, "Highlight a group of entities", new[] { "target_entities", "group_color", "group_thickness" }, false, false, new[] { "highlight", "group", "visual" }),
                new FlowInfo("highlight_clear_all", domain, "Clear all highlights", new[] { "all_entities" }, false, false, new[] { "highlight", "clear", "batch" }),
                new FlowInfo("player_marker_set", domain, "Set a player marker", new[] { "player", "marker_type", "marker_color" }, false, false, new[] { "marker", "player", "visual" }),
                new FlowInfo("player_marker_remove", domain, "Remove a player marker", new[] { "player" }, false, false, new[] { "marker", "remove", "player" }),
                new FlowInfo("player_marker_update", domain, "Update a player marker", new[] { "player", "new_marker_type", "new_marker_color" }, false, false, new[] { "marker", "update", "player" }),
                new FlowInfo("player_marker_visibility", domain, "Set player marker visibility", new[] { "player", "visibility_range" }, false, false, new[] { "marker", "visibility", "player" }),
                new FlowInfo("preview_glow_enable", domain, "Enable placement preview glow", new[] { "player", "preview_color", "preview_intensity" }, false, false, new[] { "preview", "placement", "glow" }),
                new FlowInfo("preview_glow_disable", domain, "Disable placement preview glow", new[] { "player" }, false, false, new[] { "preview", "placement", "disable" }),
                new FlowInfo("preview_glow_update", domain, "Update placement preview glow", new[] { "player", "new_preview_color", "new_preview_intensity" }, false, false, new[] { "preview", "placement", "update" }),
                new FlowInfo("interaction_glow_start", domain, "Start interaction glow", new[] { "player", "interaction_range" }, false, false, new[] { "interaction", "glow", "proximity" }),
                new FlowInfo("interaction_glow_stop", domain, "Stop interaction glow", new[] { "interactable_entity" }, false, false, new[] { "interaction", "glow", "stop" }),
                new FlowInfo("interaction_glow_range", domain, "Set interaction glow range", new[] { "player", "glow_range" }, false, false, new[] { "interaction", "range", "glow" }),
                new FlowInfo("glow_admin_set_all", domain, "Admin: set glow for all players", new[] { "all_players", "admin_glow_type", "admin_glow_color" }, true, false, new[] { "admin", "glow", "batch" }),
                new FlowInfo("glow_admin_clear_all", domain, "Admin: clear all glow effects", new[] { "all_players" }, true, false, new[] { "admin", "glow", "clear" }),
                new FlowInfo("glow_admin_entity", domain, "Admin: apply glow to specific entity", new[] { "target_entity", "admin_glow_type", "admin_glow_color" }, true, false, new[] { "admin", "glow", "entity" }),
                new FlowInfo("glow_status", domain, "Check glow status of an entity", new[] { "target_entity" }, false, false, new[] { "glow", "status", "query" }),
                new FlowInfo("glow_list_active", domain, "List all active glows in an area", new[] { "area" }, false, false, new[] { "glow", "list", "query" }),
                new FlowInfo("glow_customize", domain, "Apply custom glow effect", new[] { "target_entity", "custom_glow_params" }, false, false, new[] { "glow", "custom", "visual" }),
                new FlowInfo("glow_preset_save", domain, "Save a glow preset", new[] { "target_entity", "preset_name", "glow_parameters" }, false, false, new[] { "glow", "preset", "save" }),
                new FlowInfo("glow_preset_load", domain, "Load a glow preset", new[] { "target_entity", "preset_name" }, false, false, new[] { "glow", "preset", "load" }),
                new FlowInfo("glow_conditional", domain, "Apply conditional glow based on conditions", new[] { "target_entity", "condition_type" }, false, false, new[] { "glow", "conditional", "logic" }),
                new FlowInfo("glow_health_based", domain, "Apply health-based glow", new[] { "target_entity", "health_color", "health_threshold" }, false, false, new[] { "glow", "health", "conditional" }),
                new FlowInfo("glow_distance_based", domain, "Apply distance-based glow", new[] { "target_entity", "distance_color", "distance_threshold" }, false, false, new[] { "glow", "distance", "conditional" }),
                new FlowInfo("glow_animate", domain, "Start glow animation", new[] { "target_entity", "animation_type", "animation_duration" }, false, false, new[] { "glow", "animation", "visual" }),
                new FlowInfo("glow_animate_stop", domain, "Stop glow animation", new[] { "target_entity" }, false, false, new[] { "glow", "animation", "stop" }),
                new FlowInfo("glow_sync_group", domain, "Synchronize glow effects for a group", new[] { "target_entities", "sync_pattern" }, false, false, new[] { "glow", "sync", "group" }),
                new FlowInfo("glow_sync_stop", domain, "Stop group glow synchronization", new[] { "target_entities" }, false, false, new[] { "glow", "sync", "stop" })
            };

            RegisterFlows(domain, flows);
        }

        /// <summary>
        /// Register VBlood flows.
        /// </summary>
        private static void RegisterVBloodFlows()
        {
            var domain = FlowDomain.VBlood;
            var flows = new List<FlowInfo>
            {
                new FlowInfo("vblood_track_start", domain, "Start tracking a VBlood unit", new[] { "blood_altar", "vblood_unit", "player" }, false, false, new[] { "vblood", "tracking", "boss" }),
                new FlowInfo("vblood_track_stop", domain, "Stop tracking a VBlood unit", new[] { "blood_altar", "vblood_unit", "player" }, false, false, new[] { "vblood", "tracking", "stop" }),
                new FlowInfo("vblood_track_success", domain, "Handle successful VBlood tracking", new[] { "blood_altar", "vblood_unit", "player" }, false, false, new[] { "vblood", "success", "reward" }),
                new FlowInfo("vblood_track_failed", domain, "Handle failed VBlood tracking", new[] { "blood_altar", "vblood_unit", "player", "failure_reason" }, false, false, new[] { "vblood", "failure", "tracking" }),
                new FlowInfo("vblood_track_timeout", domain, "Handle VBlood tracking timeout", new[] { "blood_altar", "vblood_unit", "player" }, false, false, new[] { "vblood", "timeout", "tracking" }),
                new FlowInfo("blood_altar_activate", domain, "Activate a blood altar", new[] { "blood_altar", "activating_player" }, false, false, new[] { "altar", "activation", "vblood" }),
                new FlowInfo("blood_altar_reset", domain, "Reset a blood altar", new[] { "blood_altar", "resetting_player" }, false, false, new[] { "altar", "reset", "vblood" }),
                new FlowInfo("vblood_admin_track_all", domain, "Admin: track all VBlood units", new[] { "requesting_admin" }, true, false, new[] { "admin", "vblood", "tracking" }),
                new FlowInfo("vblood_admin_stop_all", domain, "Admin: stop all VBlood tracking", new[] { "requesting_admin" }, true, false, new[] { "admin", "vblood", "tracking" }),
                new FlowInfo("vblood_reward_claim", domain, "Claim VBlood rewards", new[] { "player", "vblood_type", "reward_tier" }, false, false, new[] { "vblood", "reward", "claim" }),
                new FlowInfo("vblood_progression_unlock", domain, "Unlock VBlood progression", new[] { "player", "vblood_guid", "progression_level" }, false, false, new[] { "vblood", "progression", "unlock" }),
                new FlowInfo("vblood_admin_reward", domain, "Admin: grant VBlood rewards", new[] { "target_player", "vblood_type", "reward_amount" }, true, false, new[] { "admin", "vblood", "reward" }),
                new FlowInfo("vblood_status", domain, "Check VBlood tracking status", new[] { "player", "vblood_unit" }, false, false, new[] { "vblood", "status", "query" }),
                new FlowInfo("vblood_list_active", domain, "List all active VBlood tracking", new[] { "requesting_player" }, false, false, new[] { "vblood", "list", "query" })
            };

            RegisterFlows(domain, flows);
        }

        /// <summary>
        /// Register Abilities flows.
        /// </summary>
        private static void RegisterAbilitiesFlows()
        {
            var domain = FlowDomain.Abilities;
            var flows = new List<FlowInfo>
            {
                new FlowInfo("ability_cast_start", domain, "Start casting an ability", new[] { "player", "ability", "target" }, false, false, new[] { "ability", "cast", "start" }),
                new FlowInfo("ability_cast_success", domain, "Handle successful ability cast", new[] { "player", "ability", "target" }, false, false, new[] { "ability", "cast", "success" }),
                new FlowInfo("ability_cast_failed", domain, "Handle failed ability cast", new[] { "player", "ability", "target", "failure_reason" }, false, false, new[] { "ability", "cast", "failure" }),
                new FlowInfo("ability_cast_end", domain, "End ability casting", new[] { "player", "ability", "target" }, false, false, new[] { "ability", "cast", "end" }),
                new FlowInfo("ability_cooldown_start", domain, "Start ability cooldown", new[] { "player", "ability", "cooldown_duration" }, false, false, new[] { "ability", "cooldown", "start" }),
                new FlowInfo("ability_cooldown_end", domain, "End ability cooldown", new[] { "player", "ability" }, false, false, new[] { "ability", "cooldown", "end" }),
                new FlowInfo("ability_cooldown_reset", domain, "Reset ability cooldown", new[] { "player", "ability" }, false, false, new[] { "ability", "cooldown", "reset" }),
                new FlowInfo("ability_cooldown_reduce", domain, "Reduce ability cooldown", new[] { "player", "ability", "reduction_amount" }, false, false, new[] { "ability", "cooldown", "reduce" }),
                new FlowInfo("ability_slot_assign", domain, "Assign ability to slot", new[] { "player", "ability", "slot_index" }, false, false, new[] { "ability", "slot", "assign" }),
                new FlowInfo("ability_slot_remove", domain, "Remove ability from slot", new[] { "player", "slot_index" }, false, false, new[] { "ability", "slot", "remove" }),
                new FlowInfo("ability_slot_swap", domain, "Swap abilities between slots", new[] { "player", "slot1", "slot2" }, false, false, new[] { "ability", "slot", "swap" }),
                new FlowInfo("ability_slot_clear", domain, "Clear all ability slots", new[] { "player" }, false, false, new[] { "ability", "slot", "clear" }),
                new FlowInfo("ability_learn", domain, "Learn a new ability", new[] { "player", "ability", "learning_method" }, false, false, new[] { "ability", "learn", "unlock" }),
                new FlowInfo("ability_forget", domain, "Forget an ability", new[] { "player", "ability" }, false, false, new[] { "ability", "forget", "remove" }),
                new FlowInfo("unlock_spellbook_ability", domain, "Unlock spellbook ability", new[] { "player", "spellbook_type", "ability" }, false, false, new[] { "spellbook", "unlock", "ability" }),
                new FlowInfo("ability_admin_grant", domain, "Admin: grant ability to player", new[] { "target_player", "ability", "grant_method" }, true, false, new[] { "admin", "ability", "grant" }),
                new FlowInfo("ability_admin_remove", domain, "Admin: remove ability from player", new[] { "target_player", "ability" }, true, false, new[] { "admin", "ability", "remove" }),
                new FlowInfo("ability_admin_reset_all", domain, "Admin: reset all abilities for player", new[] { "target_player" }, true, false, new[] { "admin", "ability", "reset" }),
                new FlowInfo("ability_status", domain, "Check ability status", new[] { "player", "ability" }, false, false, new[] { "ability", "status", "query" }),
                new FlowInfo("ability_list", domain, "List all abilities for player", new[] { "player" }, false, false, new[] { "ability", "list", "query" }),
                new FlowInfo("ability_validate", domain, "Validate ability requirements", new[] { "player", "ability" }, false, false, new[] { "ability", "validate", "check" }),
                new FlowInfo("ability_upgrade", domain, "Upgrade an ability", new[] { "player", "ability", "upgrade_materials" }, false, false, new[] { "ability", "upgrade", "enhance" }),
                new FlowInfo("ability_enchant", domain, "Enchant an ability", new[] { "player", "ability", "enchantment_type" }, false, false, new[] { "ability", "enchant", "enhance" })
            };

            RegisterFlows(domain, flows);
        }

        /// <summary>
        /// Register FX and Game Objects flows.
        /// </summary>
        private static void RegisterFXAndGameObjectsFlows()
        {
            var domain = FlowDomain.FXAndGameObjects;
            var flows = new List<FlowInfo>
            {
                new FlowInfo("fx_play", domain, "Play a visual effect", new[] { "player", "fx_guid", "position" }, false, false, new[] { "fx", "visual", "play" }),
                new FlowInfo("fx_play_loop", domain, "Play a looping visual effect", new[] { "player", "fx_guid", "position" }, false, false, new[] { "fx", "visual", "loop" }),
                new FlowInfo("fx_stop", domain, "Stop a visual effect", new[] { "fx_entity" }, false, false, new[] { "fx", "visual", "stop" }),
                new FlowInfo("fx_stop_all", domain, "Stop all visual effects", new[] { "player" }, false, false, new[] { "fx", "visual", "stop", "batch" }),
                new FlowInfo("particle_spawn", domain, "Spawn a particle system", new[] { "player", "particle_guid", "position" }, false, false, new[] { "particle", "spawn", "fx" }),
                new FlowInfo("particle_stop", domain, "Stop a particle system", new[] { "particle_entity" }, false, false, new[] { "particle", "stop", "fx" }),
                new FlowInfo("object_spawn", domain, "Spawn a visual game object", new[] { "player", "object_guid", "position" }, false, false, new[] { "object", "spawn", "visual" }),
                new FlowInfo("object_spawn_with_rotation", domain, "Spawn object with rotation", new[] { "player", "object_guid", "position", "rotation" }, false, false, new[] { "object", "spawn", "rotation" }),
                new FlowInfo("object_despawn", domain, "Despawn a visual game object", new[] { "object_entity" }, false, false, new[] { "object", "despawn", "visual" }),
                new FlowInfo("environment_weather_change", domain, "Change weather in a zone", new[] { "zone", "weather_type" }, false, false, new[] { "environment", "weather", "zone" }),
                new FlowInfo("environment_time_change", domain, "Change time of day in a zone", new[] { "zone", "time_of_day" }, false, false, new[] { "environment", "time", "zone" }),
                new FlowInfo("environment_lighting", domain, "Modify lighting in a zone", new[] { "zone", "lighting_params" }, false, false, new[] { "environment", "lighting", "zone" }),
                new FlowInfo("decal_place", domain, "Place a decal", new[] { "player", "decal_guid", "position", "rotation" }, false, false, new[] { "decal", "place", "visual" }),
                new FlowInfo("decal_remove", domain, "Remove a decal", new[] { "decal_entity" }, false, false, new[] { "decal", "remove", "visual" }),
                new FlowInfo("decal_clear_all", domain, "Clear all decals in a zone", new[] { "zone" }, false, false, new[] { "decal", "clear", "zone" }),
                new FlowInfo("sound_play", domain, "Play a sound effect", new[] { "player", "sound_guid", "position" }, false, false, new[] { "sound", "audio", "play" }),
                new FlowInfo("sound_play_loop", domain, "Play a looping sound", new[] { "player", "sound_guid", "position" }, false, false, new[] { "sound", "audio", "loop" }),
                new FlowInfo("sound_stop", domain, "Stop a sound", new[] { "sound_entity" }, false, false, new[] { "sound", "audio", "stop" }),
                new FlowInfo("fx_admin_play_all", domain, "Admin: play FX for all players", new[] { "requesting_admin", "all_players", "fx_guid" }, true, false, new[] { "admin", "fx", "batch" }),
                new FlowInfo("fx_admin_stop_all", domain, "Admin: stop all FX", new[] { "requesting_admin", "all_players" }, true, false, new[] { "admin", "fx", "stop", "batch" }),
                new FlowInfo("fx_status", domain, "Check visual effects status", new[] { "player" }, false, false, new[] { "fx", "status", "query" }),
                new FlowInfo("fx_cleanup", domain, "Clean up visual effects in a zone", new[] { "zone" }, false, false, new[] { "fx", "cleanup", "zone" })
            };

            RegisterFlows(domain, flows);
        }

        /// <summary>
        /// Register Equipment and Kits flows.
        /// </summary>
        private static void RegisterEquipmentAndKitsFlows()
        {
            var domain = FlowDomain.EquipmentAndKits;
            var flows = new List<FlowInfo>
            {
                new FlowInfo("equipment_equip", domain, "Equip an item to a slot", new[] { "player", "item", "slot" }, false, false, new[] { "equipment", "equip", "slot" }),
                new FlowInfo("equipment_unequip", domain, "Unequip an item from a slot", new[] { "player", "slot" }, false, false, new[] { "equipment", "unequip", "slot" }),
                new FlowInfo("equipment_swap", domain, "Swap equipment between slots", new[] { "player", "slot1", "slot2" }, false, false, new[] { "equipment", "swap", "slot" }),
                new FlowInfo("equipment_clear_all", domain, "Clear all equipment slots", new[] { "player" }, false, false, new[] { "equipment", "clear", "batch" }),
                new FlowInfo("inventory_add", domain, "Add item to inventory", new[] { "player", "item", "quantity" }, false, false, new[] { "inventory", "add", "item" }),
                new FlowInfo("inventory_remove", domain, "Remove item from inventory", new[] { "player", "item", "quantity" }, false, false, new[] { "inventory", "remove", "item" }),
                new FlowInfo("inventory_transfer", domain, "Transfer item between players", new[] { "source_player", "target_player", "item", "quantity" }, false, false, new[] { "inventory", "transfer", "item" }),
                new FlowInfo("inventory_clear", domain, "Clear player inventory", new[] { "player" }, false, false, new[] { "inventory", "clear", "batch" }),
                new FlowInfo("kit_create", domain, "Create an equipment kit", new[] { "player", "kit_name", "kit_items" }, false, false, new[] { "kit", "create", "equipment" }),
                new FlowInfo("kit_save", domain, "Save current equipment as kit", new[] { "player", "kit_name", "current_equipment" }, false, false, new[] { "kit", "save", "equipment" }),
                new FlowInfo("kit_load", domain, "Load an equipment kit", new[] { "player", "kit_name" }, false, false, new[] { "kit", "load", "equipment" }),
                new FlowInfo("kit_delete", domain, "Delete an equipment kit", new[] { "player", "kit_name" }, false, false, new[] { "kit", "delete", "equipment" }),
                new FlowInfo("kit_distribute", domain, "Distribute kit to multiple players", new[] { "target_players", "kit_name" }, false, false, new[] { "kit", "distribute", "batch" }),
                new FlowInfo("kit_distribute_party", domain, "Distribute kit to party members", new[] { "player", "kit_name" }, false, false, new[] { "kit", "distribute", "party" }),
                new FlowInfo("preset_save", domain, "Save equipment preset", new[] { "player", "preset_name" }, false, false, new[] { "preset", "save", "equipment" }),
                new FlowInfo("preset_load", domain, "Load equipment preset", new[] { "player", "preset_name" }, false, false, new[] { "preset", "load", "equipment" }),
                new FlowInfo("preset_delete", domain, "Delete equipment preset", new[] { "player", "preset_name" }, false, false, new[] { "preset", "delete", "equipment" }),
                new FlowInfo("durability_repair", domain, "Repair item durability", new[] { "player", "item", "repair_amount" }, false, false, new[] { "durability", "repair", "item" }),
                new FlowInfo("durability_damage", domain, "Damage item durability", new[] { "player", "item", "damage_amount" }, false, false, new[] { "durability", "damage", "item" }),
                new FlowInfo("durability_break", domain, "Break an item", new[] { "player", "item" }, false, false, new[] { "durability", "break", "item" }),
                new FlowInfo("equipment_admin_give", domain, "Admin: give equipment to player", new[] { "requesting_admin", "target_player", "item", "quantity" }, true, false, new[] { "admin", "equipment", "give" }),
                new FlowInfo("equipment_admin_remove", domain, "Admin: remove equipment from player", new[] { "requesting_admin", "target_player", "item", "quantity" }, true, false, new[] { "admin", "equipment", "remove" }),
                new FlowInfo("equipment_admin_clear", domain, "Admin: clear all equipment", new[] { "requesting_admin", "target_player" }, true, false, new[] { "admin", "equipment", "clear" }),
                new FlowInfo("equipment_status", domain, "Check equipment status", new[] { "player" }, false, false, new[] { "equipment", "status", "query" }),
                new FlowInfo("inventory_status", domain, "Check inventory status", new[] { "player" }, false, false, new[] { "inventory", "status", "query" }),
                new FlowInfo("equipment_upgrade", domain, "Upgrade equipment", new[] { "player", "item", "upgrade_materials" }, false, false, new[] { "equipment", "upgrade", "enhance" }),
                new FlowInfo("equipment_enchant", domain, "Enchant equipment", new[] { "player", "item", "enchantment" }, false, false, new[] { "equipment", "enchant", "enhance" })
            };

            RegisterFlows(domain, flows);
        }

        /// <summary>
        /// Register Zone flows.
        /// </summary>
        private static void RegisterZoneFlows()
        {
            var domain = FlowDomain.Zone;
            var flows = new List<FlowInfo>
            {
                new FlowInfo("sunblocker_region", domain, "Create a sunblocker region", new[] { "zone", "region_params" }, false, false, new[] { "sunblocker", "region", "zone" }),
                new FlowInfo("sunblocker_activate", domain, "Activate a sunblocker region", new[] { "zone" }, false, false, new[] { "sunblocker", "activate", "zone" }),
                new FlowInfo("sunblocker_deactivate", domain, "Deactivate a sunblocker region", new[] { "zone" }, false, false, new[] { "sunblocker", "deactivate", "zone" }),
                new FlowInfo("sunblocker_remove", domain, "Remove a sunblocker region", new[] { "zone" }, false, false, new[] { "sunblocker", "remove", "zone" }),
                new FlowInfo("zone_restrict_enable", domain, "Enable zone restrictions", new[] { "zone", "restriction_type" }, false, false, new[] { "zone", "restrict", "enable" }),
                new FlowInfo("zone_restrict_disable", domain, "Disable zone restrictions", new[] { "zone" }, false, false, new[] { "zone", "restrict", "disable" }),
                new FlowInfo("zone_restrict_modify", domain, "Modify zone restrictions", new[] { "zone", "new_restriction_params" }, false, false, new[] { "zone", "restrict", "modify" }),
                new FlowInfo("zone_enter", domain, "Handle zone entry", new[] { "player", "zone" }, false, false, new[] { "zone", "enter", "transition" }),
                new FlowInfo("zone_exit", domain, "Handle zone exit", new[] { "player", "zone" }, false, false, new[] { "zone", "exit", "transition" }),
                new FlowInfo("zone_transition", domain, "Handle zone transition", new[] { "player", "from_zone", "to_zone" }, false, false, new[] { "zone", "transition", "change" }),
                new FlowInfo("zone_rule_add", domain, "Add a zone rule", new[] { "zone", "rule_type", "rule_params" }, false, false, new[] { "zone", "rule", "add" }),
                new FlowInfo("zone_rule_remove", domain, "Remove a zone rule", new[] { "zone", "rule_type" }, false, false, new[] { "zone", "rule", "remove" }),
                new FlowInfo("zone_rule_update", domain, "Update a zone rule", new[] { "zone", "rule_type", "new_rule_params" }, false, false, new[] { "zone", "rule", "update" }),
                new FlowInfo("zone_rule_check", domain, "Check zone rules", new[] { "zone", "rule_type" }, false, false, new[] { "zone", "rule", "check" }),
                new FlowInfo("zone_admin_create", domain, "Admin: create a zone", new[] { "requesting_admin", "zone_params", "admin_zone_type" }, true, false, new[] { "admin", "zone", "create" }),
                new FlowInfo("zone_admin_delete", domain, "Admin: delete a zone", new[] { "requesting_admin", "target_zone" }, true, false, new[] { "admin", "zone", "delete" }),
                new FlowInfo("zone_admin_modify", domain, "Admin: modify a zone", new[] { "requesting_admin", "target_zone", "admin_modifications" }, true, false, new[] { "admin", "zone", "modify" }),
                new FlowInfo("zone_admin_lock", domain, "Admin: lock a zone", new[] { "requesting_admin", "target_zone", "lock_type" }, true, false, new[] { "admin", "zone", "lock" }),
                new FlowInfo("zone_admin_unlock", domain, "Admin: unlock a zone", new[] { "requesting_admin", "target_zone" }, true, false, new[] { "admin", "zone", "unlock" }),
                new FlowInfo("zone_status", domain, "Check zone status", new[] { "zone" }, false, false, new[] { "zone", "status", "query" }),
                new FlowInfo("zone_list", domain, "List all zones", new[] { "zone_type" }, false, false, new[] { "zone", "list", "query" }),
                new FlowInfo("zone_player_status", domain, "Check player's zone status", new[] { "player" }, false, false, new[] { "zone", "player", "status" }),
                new FlowInfo("zone_environment_set", domain, "Set zone environment", new[] { "zone", "environment_params" }, false, false, new[] { "zone", "environment", "set" }),
                new FlowInfo("zone_weather_control", domain, "Control zone weather", new[] { "zone", "weather_type" }, false, false, new[] { "zone", "weather", "control" }),
                new FlowInfo("zone_time_control", domain, "Control zone time", new[] { "zone", "time_of_day" }, false, false, new[] { "zone", "time", "control" }),
                new FlowInfo("zone_security_enable", domain, "Enable zone security", new[] { "zone", "security_level" }, false, false, new[] { "zone", "security", "enable" }),
                new FlowInfo("zone_security_disable", domain, "Disable zone security", new[] { "zone" }, false, false, new[] { "zone", "security", "disable" }),
                new FlowInfo("zone_permission_grant", domain, "Grant zone permission", new[] { "player", "zone", "permission_level" }, false, false, new[] { "zone", "permission", "grant" }),
                new FlowInfo("zone_permission_revoke", domain, "Revoke zone permission", new[] { "player", "zone" }, false, false, new[] { "zone", "permission", "revoke" }),
                new FlowInfo("zone_event_trigger", domain, "Trigger zone event", new[] { "zone", "event_type", "event_params" }, false, false, new[] { "zone", "event", "trigger" }),
                new FlowInfo("zone_event_cancel", domain, "Cancel zone event", new[] { "zone", "event_type" }, false, false, new[] { "zone", "event", "cancel" }),
                new FlowInfo("zone_monitor_start", domain, "Start zone monitoring", new[] { "zone", "monitor_params" }, false, false, new[] { "zone", "monitor", "start" }),
                new FlowInfo("zone_monitor_stop", domain, "Stop zone monitoring", new[] { "zone" }, false, false, new[] { "zone", "monitor", "stop" }),
                new FlowInfo("zone_monitor_report", domain, "Generate zone monitoring report", new[] { "zone", "report_params" }, false, false, new[] { "zone", "monitor", "report" })
            };

            RegisterFlows(domain, flows);
        }

        /// <summary>
        /// Register Zone Rules flows.
        /// </summary>
        private static void RegisterZoneRulesFlows()
        {
            var domain = FlowDomain.Zone;
            var flows = new List<FlowInfo>
            {
                new FlowInfo("zone_rule_create", domain, "Create a zone rule", new[] { "zone", "rule_type", "rule_parameters" }, false, false, new[] { "zone", "rule", "create" }),
                new FlowInfo("zone_rule_modify", domain, "Modify a zone rule", new[] { "zone", "rule_type", "new_rule_parameters" }, false, false, new[] { "zone", "rule", "modify" }),
                new FlowInfo("zone_rule_delete", domain, "Delete a zone rule", new[] { "zone", "rule_type" }, false, false, new[] { "zone", "rule", "delete" }),
                new FlowInfo("zone_rule_enforce", domain, "Enforce zone rules", new[] { "zone", "rule_types", "enforcement_effects", "enforced_rules" }, false, false, new[] { "zone", "rule", "enforce" }),
                new FlowInfo("zone_rule_relax", domain, "Relax zone rules", new[] { "zone", "rule_types", "relaxed_effects", "relaxed_rules" }, false, false, new[] { "zone", "rule", "relax" }),
                new FlowInfo("zone_rule_status", domain, "Check zone rules status", new[] { "zone", "rule_status_info" }, false, false, new[] { "zone", "rule", "status", "query" }),
                new FlowInfo("zone_rule_list", domain, "List all zone rules", new[] { "zone", "rule_list" }, false, false, new[] { "zone", "rule", "list", "query" }),
                new FlowInfo("zone_rule_pvp_disable", domain, "Disable PVP rules in zone", new[] { "zone", "pvp_safety_level", "pvp_disabled_visuals" }, false, false, new[] { "zone", "pvp", "disable", "rule" }),
                new FlowInfo("zone_rule_pvp_enable", domain, "Enable PVP rules in zone", new[] { "zone" }, false, false, new[] { "zone", "pvp", "enable", "rule" }),
                new FlowInfo("zone_rule_building_disable", domain, "Disable building rules in zone", new[] { "zone", "building_freedom_level", "building_freedom_visuals" }, false, false, new[] { "zone", "building", "disable", "rule" }),
                new FlowInfo("zone_rule_building_enable", domain, "Enable building rules in zone", new[] { "zone" }, false, false, new[] { "zone", "building", "enable", "rule" }),
                new FlowInfo("zone_rule_combat_disable", domain, "Disable combat rules in zone", new[] { "zone", "combat_peace_level", "combat_peace_visuals" }, false, false, new[] { "zone", "combat", "disable", "rule" }),
                new FlowInfo("zone_rule_combat_enable", domain, "Enable combat rules in zone", new[] { "zone" }, false, false, new[] { "zone", "combat", "enable", "rule" }),
                new FlowInfo("zone_rule_override_all", domain, "Override all zone rules", new[] { "zone", "override_type", "override_effects", "override_visualization" }, false, false, new[] { "zone", "rule", "override", "all" }),
                new FlowInfo("zone_rule_restore_all", domain, "Restore all zone rules", new[] { "zone", "override_effects", "override_visualization" }, false, false, new[] { "zone", "rule", "restore", "all" }),
                new FlowInfo("zone_rule_permission_grant", domain, "Grant rule permission to player", new[] { "player", "zone", "rule_permission_level", "permission_effects" }, false, false, new[] { "zone", "rule", "permission", "grant" }),
                new FlowInfo("zone_rule_permission_revoke", domain, "Revoke rule permission from player", new[] { "player", "zone", "permission_effects" }, false, false, new[] { "zone", "rule", "permission", "revoke" }),
                new FlowInfo("zone_rule_exception_add", domain, "Add rule exception", new[] { "zone", "rule_type", "exception_target", "exception_params", "exception_effects", "exception_visualization" }, false, false, new[] { "zone", "rule", "exception", "add" }),
                new FlowInfo("zone_rule_exception_remove", domain, "Remove rule exception", new[] { "zone", "rule_type", "exception_target", "exception_effects", "exception_target" }, false, false, new[] { "zone", "rule", "exception", "remove" }),
                new FlowInfo("zone_rule_admin_disable_all", domain, "Admin: disable all zone rules globally", new[] { "requesting_admin", "all_zones", "admin_disable_effects", "admin_visualization" }, true, false, new[] { "admin", "zone", "rule", "disable", "all", "global" }),
                new FlowInfo("zone_rule_admin_enable_all", domain, "Admin: enable all zone rules globally", new[] { "requesting_admin", "all_zones", "admin_visualization" }, true, false, new[] { "admin", "zone", "rule", "enable", "all", "global" }),
                new FlowInfo("zone_rule_admin_category", domain, "Admin: modify rule category globally", new[] { "requesting_admin", "all_zones", "rule_category", "admin_action", "category_effects", "category_visualization" }, true, false, new[] { "admin", "zone", "rule", "category", "global" }),
                new FlowInfo("zone_rule_admin_custom", domain, "Admin: create custom rule globally", new[] { "requesting_admin", "target_zones", "rule_definition", "custom_parameters", "custom_enforcement", "custom_visualization" }, true, false, new[] { "admin", "zone", "rule", "custom", "global" }),
                new FlowInfo("zone_rule_monitor_start", domain, "Start zone rule monitoring", new[] { "zone", "monitoring_params", "monitoring_effects", "monitoring_visualization" }, false, false, new[] { "zone", "rule", "monitor", "start" }),
                new FlowInfo("zone_rule_monitor_stop", domain, "Stop zone rule monitoring", new[] { "zone", "monitoring_effects", "monitoring_visualization" }, false, false, new[] { "zone", "rule", "monitor", "stop" }),
                new FlowInfo("zone_rule_monitor_report", domain, "Generate zone rule monitoring report", new[] { "zone", "report_params", "compliance_data", "report_data" }, false, false, new[] { "zone", "rule", "monitor", "report" }),
                new FlowInfo("zone_rule_history_log", domain, "Log zone rule change", new[] { "zone", "rule_type", "change_type", "change_details", "rule_history_data" }, false, false, new[] { "zone", "rule", "history", "log" }),
                new FlowInfo("zone_rule_history_view", domain, "View zone rule history", new[] { "zone", "rule_type", "history_params", "history_data" }, false, false, new[] { "zone", "rule", "history", "view" }),
                new FlowInfo("zone_rule_history_clear", domain, "Clear zone rule history", new[] { "zone", "rule_type", "rule_history_data" }, false, false, new[] { "zone", "rule", "history", "clear" }),
                new FlowInfo("zone_rule_template_save", domain, "Save zone rule template", new[] { "template_name", "rule_definition", "template_parameters", "template_data" }, false, false, new[] { "zone", "rule", "template", "save" }),
                new FlowInfo("zone_rule_template_load", domain, "Load zone rule template", new[] { "template_name", "target_zone", "template_rules", "template_visualization" }, false, false, new[] { "zone", "rule", "template", "load" }),
                new FlowInfo("zone_rule_template_delete", domain, "Delete zone rule template", new[] { "template_name", "template_data" }, false, false, new[] { "zone", "rule", "template", "delete" }),
                new FlowInfo("zone_rule_validate", domain, "Validate zone rule", new[] { "zone", "rule_type", "validation_params", "validation_result" }, false, false, new[] { "zone", "rule", "validate", "check" }),
                new FlowInfo("zone_rule_test", domain, "Test zone rule behavior", new[] { "zone", "rule_type", "test_scenario", "test_conditions", "test_results", "test_result" }, false, false, new[] { "zone", "rule", "test", "behavior" })
            };

            RegisterFlows(domain, flows);
        }

        /// <summary>
        /// Register Arena flows.
        /// </summary>
        private static void RegisterArenaFlows()
        {
            var domain = FlowDomain.Arena;
            var flows = new List<FlowInfo>
            {
                new FlowInfo("arena_enable", domain, "Enable an arena and its bound zone rules", new[] { "arena_id", "zone", "enable_reason" }, true, false, new[] { "arena", "enable", "zone" }),
                new FlowInfo("arena_disable", domain, "Disable an arena and suspend its zone rules", new[] { "arena_id", "zone", "disable_reason" }, true, false, new[] { "arena", "disable", "zone" }),
                new FlowInfo("arena_create", domain, "Create an arena definition", new[] { "arena_id", "zone", "center", "radius", "rules_profile" }, true, false, new[] { "arena", "create", "zone" }),
                new FlowInfo("arena_delete", domain, "Delete an arena definition", new[] { "arena_id", "zone" }, true, false, new[] { "arena", "delete", "zone" }),
                new FlowInfo("arena_bind_zone", domain, "Bind an arena to a zone", new[] { "arena_id", "zone", "rules_profile" }, true, false, new[] { "arena", "bind", "zone" }),
                new FlowInfo("arena_unbind_zone", domain, "Unbind an arena from a zone", new[] { "arena_id", "zone" }, true, false, new[] { "arena", "unbind", "zone" }),
                new FlowInfo("arena_match_start", domain, "Start an arena match", new[] { "arena_id", "participants", "match_mode" }, true, false, new[] { "arena", "match", "start" }),
                new FlowInfo("arena_match_stop", domain, "Stop an arena match", new[] { "arena_id", "stop_reason" }, true, false, new[] { "arena", "match", "stop" }),
                new FlowInfo("arena_match_reset", domain, "Reset an arena match", new[] { "arena_id", "reset_mode" }, true, false, new[] { "arena", "match", "reset" }),
                new FlowInfo("arena_player_join", domain, "Join a player to an arena", new[] { "arena_id", "player", "team" }, false, false, new[] { "arena", "player", "join" }),
                new FlowInfo("arena_player_leave", domain, "Remove a player from an arena", new[] { "arena_id", "player", "leave_reason" }, false, false, new[] { "arena", "player", "leave" }),
                new FlowInfo("arena_spectator_enable", domain, "Enable spectator mode for an arena", new[] { "arena_id", "player", "view_mode" }, true, false, new[] { "arena", "spectator", "enable" }),
                new FlowInfo("arena_spectator_disable", domain, "Disable spectator mode for an arena", new[] { "arena_id", "player" }, true, false, new[] { "arena", "spectator", "disable" }),
                new FlowInfo("arena_status", domain, "Get arena status", new[] { "arena_id" }, false, false, new[] { "arena", "status", "query" }),
                new FlowInfo("arena_list", domain, "List arenas", new[] { "arena_state" }, false, false, new[] { "arena", "list", "query" })
            };

            RegisterFlows(domain, flows);
        }

        /// <summary>
        /// Register Placement Restriction flows.
        /// </summary>
        private static void RegisterPlacementRestrictionFlows()
        {
            var domain = FlowDomain.PlacementRestriction;
            var flows = new List<FlowInfo>
            {
                new FlowInfo("placement_restriction_critical", domain, "Critical placement restriction sequence", new[] { "player", "fx_guid", "position" }, false, false, new[] { "placement", "restriction", "critical" }),
                new FlowInfo("placement_emergency_override", domain, "Emergency placement override", new[] { "requesting_admin", "target_player", "emergency_fx" }, true, false, new[] { "placement", "emergency", "admin" }),
                new FlowInfo("placement_zone_restriction", domain, "Zone-based placement restriction", new[] { "zone", "zone_fx_guid" }, false, false, new[] { "placement", "zone", "restriction" }),
                new FlowInfo("placement_player_restriction", domain, "Player-specific placement restriction", new[] { "player", "player_fx_guid", "player_position" }, false, false, new[] { "placement", "player", "restriction" }),
                new FlowInfo("placement_global_restriction", domain, "Global placement restriction", new[] { "global_fx_guid", "positions" }, false, false, new[] { "placement", "global", "restriction" }),
                new FlowInfo("placement_temporary_restriction", domain, "Temporary placement restriction", new[] { "player", "temp_fx_guid", "position", "temp_duration" }, false, false, new[] { "placement", "temporary", "restriction" }),
                new FlowInfo("placement_restriction_with_validation", domain, "Placement restriction with validation", new[] { "player", "area", "validation_fx", "area_center" }, false, false, new[] { "placement", "validation", "restriction" }),
                new FlowInfo("placement_safe_restriction", domain, "Safe placement restriction with backup", new[] { "player", "safe_fx_guid", "position", "spawned_item" }, false, false, new[] { "placement", "safe", "restriction" }),
                new FlowInfo("placement_restriction_recovery", domain, "Placement restriction failure recovery", new[] { "player", "recovery_fx_guid", "position", "failure_reason" }, false, false, new[] { "placement", "recovery", "restriction" }),
                new FlowInfo("placement_admin_control", domain, "Admin placement control", new[] { "requesting_admin", "target_players", "admin_fx_guid" }, true, false, new[] { "placement", "admin", "control" }),
                new FlowInfo("placement_restriction_status", domain, "Check placement restriction status", new[] { "player" }, false, false, new[] { "placement", "status", "query" }),
                new FlowInfo("placement_restriction_cleanup", domain, "Placement restriction cleanup", new[] { "all_players" }, false, false, new[] { "placement", "cleanup", "batch" })
            };

            RegisterFlows(domain, flows);
        }

        /// <summary>
        /// Register Castle Building flows.
        /// </summary>
        private static void RegisterCastleBuildingFlows()
        {
            var domain = FlowDomain.CastleBuilding;
            var flows = new List<FlowInfo>
            {
                new FlowInfo("castle_attachment_add", domain, "Add building attachment", new[] { "building", "attachment", "buff" }, false, false, new[] { "castle", "building", "attachment" }),
                new FlowInfo("castle_attachment_remove", domain, "Remove building attachment", new[] { "building", "attachment", "buff" }, false, false, new[] { "castle", "building", "attachment" }),
                new FlowInfo("castle_attachment_clear_all", domain, "Clear all building attachments", new[] { "building", "buff" }, false, false, new[] { "castle", "building", "attachment" }),
                new FlowInfo("castle_attachment_upgrade", domain, "Upgrade building attachment", new[] { "building", "attachment", "upgrade_materials", "new_buff" }, false, false, new[] { "castle", "building", "attachment", "upgrade" }),
                new FlowInfo("castle_buff_apply", domain, "Apply building buff", new[] { "building", "buff", "source", "buff_effects" }, false, false, new[] { "castle", "building", "buff" }),
                new FlowInfo("castle_buff_remove", domain, "Remove building buff", new[] { "building", "buff" }, false, false, new[] { "castle", "building", "buff" }),
                new FlowInfo("castle_buff_update", domain, "Update building buff", new[] { "building", "buff", "new_value" }, false, false, new[] { "castle", "building", "buff" }),
                new FlowInfo("castle_buff_clear_all", domain, "Clear all building buffs", new[] { "building", "buff_effects" }, false, false, new[] { "castle", "building", "buff" }),
                new FlowInfo("castle_construct", domain, "Construct a castle", new[] { "player", "building_type", "position", "default_attachments", "construction_buffs" }, false, false, new[] { "castle", "construct", "building" }),
                new FlowInfo("castle_deconstruct", domain, "Deconstruct a castle", new[] { "building", "attachments", "buffs" }, false, false, new[] { "castle", "deconstruct", "building" }),
                new FlowInfo("castle_repair", domain, "Repair a castle", new[] { "building", "repair_materials", "health_amount" }, false, false, new[] { "castle", "repair", "building" }),
                new FlowInfo("castle_upgrade_tier", domain, "Upgrade castle tier", new[] { "castle", "new_tier", "upgrade_materials", "tier_attachments", "tier_buffs" }, false, false, new[] { "castle", "upgrade", "tier" }),
                new FlowInfo("castle_upgrade_defense", domain, "Upgrade castle defense", new[] { "castle", "defense_attachments", "defense_buffs", "defense_bonus" }, false, false, new[] { "castle", "upgrade", "defense" }),
                new FlowInfo("castle_upgrade_production", domain, "Upgrade castle production", new[] { "castle", "production_attachments", "production_buffs", "production_bonus" }, false, false, new[] { "castle", "upgrade", "production" }),
                new FlowInfo("castle_manage_permissions", domain, "Manage castle permissions", new[] { "castle", "player", "permission_level", "permission_buffs" }, false, false, new[] { "castle", "permissions", "manage" }),
                new FlowInfo("castle_claim", domain, "Claim a castle", new[] { "castle", "player", "claim_type", "ownership_buffs", "castle_name" }, false, false, new[] { "castle", "claim", "ownership" }),
                new FlowInfo("castle_abandon", domain, "Abandon a castle", new[] { "castle", "ownership_buffs", "castle_name" }, false, false, new[] { "castle", "abandon", "ownership" }),
                new FlowInfo("castle_admin_build", domain, "Admin: instant castle build", new[] { "requesting_admin", "target_location", "castle_type", "admin_buffs" }, true, false, new[] { "admin", "castle", "build" }),
                new FlowInfo("castle_admin_destroy", domain, "Admin: instant castle destroy", new[] { "requesting_admin", "target_castle", "target_location" }, true, false, new[] { "admin", "castle", "destroy" }),
                new FlowInfo("castle_admin_upgrade_all", domain, "Admin: upgrade all castles", new[] { "requesting_admin", "all_castles", "target_tier", "admin_upgrades" }, true, false, new[] { "admin", "castle", "upgrade", "batch" }),
                new FlowInfo("castle_status", domain, "Check castle status", new[] { "castle", "attachments", "buffs" }, false, false, new[] { "castle", "status", "query" }),
                new FlowInfo("castle_list", domain, "List all castles", new[] { "castles" }, false, false, new[] { "castle", "list", "query" }),
                new FlowInfo("castle_defense_activate", domain, "Activate castle defenses", new[] { "castle", "defense_buffs", "defense_fx" }, false, false, new[] { "castle", "defense", "activate" }),
                new FlowInfo("castle_defense_deactivate", domain, "Deactivate castle defenses", new[] { "castle", "defense_buffs" }, false, false, new[] { "castle", "defense", "deactivate" }),
                new FlowInfo("castle_production_start", domain, "Start castle production", new[] { "castle", "production_type", "production_buffs", "production_fx" }, false, false, new[] { "castle", "production", "start" }),
                new FlowInfo("castle_production_stop", domain, "Stop castle production", new[] { "castle", "production_buffs", "production_fx" }, false, false, new[] { "castle", "production", "stop" })
            };

            RegisterFlows(domain, flows);
        }

        /// <summary>
        /// Register Castle Territory flows.
        /// </summary>
        private static void RegisterCastleTerritoryFlows()
        {
            var domain = FlowDomain.CastleTerritory;
            var flows = new List<FlowInfo>
            {
                new FlowInfo("territory_draw", domain, "Draw a castle territory", new[] { "castle", "center", "radius", "territory_fx", "territory_color" }, false, false, new[] { "territory", "draw", "castle" }),
                new FlowInfo("territory_update", domain, "Update a castle territory", new[] { "castle", "new_center", "new_radius", "new_visualization", "new_boundary" }, false, false, new[] { "territory", "update", "castle" }),
                new FlowInfo("territory_clear", domain, "Clear a castle territory", new[] { "castle", "territory_visualization", "territory_boundary" }, false, false, new[] { "territory", "clear", "castle" }),
                new FlowInfo("territory_expand", domain, "Expand a castle territory", new[] { "castle", "expansion_radius", "expanded_visualization", "expansion_fx" }, false, false, new[] { "territory", "expand", "castle" }),
                new FlowInfo("territory_shrink", domain, "Shrink a castle territory", new[] { "castle", "shrink_radius", "shrunk_visualization", "shrink_fx" }, false, false, new[] { "territory", "shrink", "castle" }),
                new FlowInfo("territory_claim", domain, "Claim a territory", new[] { "castle", "center", "default_radius", "player", "ownership_buffs", "player_color_fx" }, false, false, new[] { "territory", "claim", "ownership" }),
                new FlowInfo("territory_abandon", domain, "Abandon a territory", new[] { "castle", "ownership_buffs", "territory_visualization", "castle" }, false, false, new[] { "territory", "abandon", "ownership" }),
                new FlowInfo("territory_transfer", domain, "Transfer territory ownership", new[] { "castle", "new_owner", "new_owner_fx", "transfer_buffs", "old_owner" }, false, false, new[] { "territory", "transfer", "ownership" }),
                new FlowInfo("territory_boundary_create", domain, "Create territory boundary", new[] { "castle", "boundary_type", "boundary_fx", "collision_type" }, false, false, new[] { "territory", "boundary", "create" }),
                new FlowInfo("territory_boundary_remove", domain, "Remove territory boundary", new[] { "castle", "boundary_fx", "boundary_collision" }, false, false, new[] { "territory", "boundary", "remove" }),
                new FlowInfo("territory_boundary_update", domain, "Update territory boundary", new[] { "castle", "new_boundary_type", "new_boundary_fx", "new_collision_type" }, false, false, new[] { "territory", "boundary", "update" }),
                new FlowInfo("territory_visualization_set", domain, "Set territory visualization", new[] { "castle", "visualization_fx", "territory_color", "marker_type", "ui_elements" }, false, false, new[] { "territory", "visualization", "set" }),
                new FlowInfo("territory_visualization_update", domain, "Update territory visualization", new[] { "castle", "new_visualization_fx", "new_color", "new_marker_type" }, false, false, new[] { "territory", "visualization", "update" }),
                new FlowInfo("territory_visualization_hide", domain, "Hide territory visualization", new[] { "castle", "territory_markers", "territory_ui" }, false, false, new[] { "territory", "visualization", "hide" }),
                new FlowInfo("territory_visualization_show", domain, "Show territory visualization", new[] { "castle", "territory_markers", "territory_ui" }, false, false, new[] { "territory", "visualization", "show" }),
                new FlowInfo("territory_conflict_check", domain, "Check territory conflicts", new[] { "castle", "conflicts", "conflict_list" }, false, false, new[] { "territory", "conflict", "check" }),
                new FlowInfo("territory_conflict_resolve", domain, "Resolve territory conflicts", new[] { "castle", "conflicting_castle", "resolution_type", "post_conflict_visualization" }, false, false, new[] { "territory", "conflict", "resolve" }),
                new FlowInfo("territory_admin_draw", domain, "Admin: draw territory", new[] { "requesting_admin", "target_location", "admin_radius", "admin_visualization" }, true, false, new[] { "admin", "territory", "draw" }),
                new FlowInfo("territory_admin_clear_all", domain, "Admin: clear all territories", new[] { "requesting_admin", "all_territories", "all_territory_visualizations" }, true, false, new[] { "admin", "territory", "clear", "batch" }),
                new FlowInfo("territory_admin_resize", domain, "Admin: resize territory", new[] { "requesting_admin", "target_territory", "admin_radius", "admin_visualization" }, true, false, new[] { "admin", "territory", "resize" }),
                new FlowInfo("territory_status", domain, "Check territory status", new[] { "castle", "territory_boundaries", "territory_visualization" }, false, false, new[] { "territory", "status", "query" }),
                new FlowInfo("territory_list", domain, "List all territories", new[] { "territories" }, false, false, new[] { "territory", "list", "query" }),
                new FlowInfo("territory_point_check", domain, "Check if point is in territories", new[] { "point", "containing_territories", "territory_names" }, false, false, new[] { "territory", "point", "check" }),
                new FlowInfo("territory_player_check", domain, "Check if player is in territories", new[] { "player", "territory_status", "territory_names" }, false, false, new[] { "territory", "player", "check" })
            };

            RegisterFlows(domain, flows);
        }

        /// <summary>
        /// Register Spawn Tag flows.
        /// </summary>
        private static void RegisterSpawnTagFlows()
        {
            var domain = FlowDomain.SpawnTag;
            var flows = new List<FlowInfo>
            {
                new FlowInfo("spawntag_add", domain, "Add spawn tag to entity", new[] { "entity", "tag" }, false, false, new[] { "spawn", "tag", "add" }),
                new FlowInfo("spawntag_remove", domain, "Remove spawn tag from entity", new[] { "entity", "tag" }, false, false, new[] { "spawn", "tag", "remove" }),
                new FlowInfo("spawntag_clear_all", domain, "Clear all spawn tags from entity", new[] { "entity" }, false, false, new[] { "spawn", "tag", "clear" }),
                new FlowInfo("spawntag_check", domain, "Check if entity has spawn tag", new[] { "entity", "tag" }, false, false, new[] { "spawn", "tag", "check" }),
                new FlowInfo("spawntag_spawn_single", domain, "Spawn single entity with tag", new[] { "tag", "position", "rotation", "spawn_fx" }, false, false, new[] { "spawn", "tag", "single" }),
                new FlowInfo("spawntag_spawn_multiple", domain, "Spawn multiple entities with tag", new[] { "tag", "center_position", "count", "spread_radius", "batch_fx" }, false, false, new[] { "spawn", "tag", "multiple", "batch" }),
                new FlowInfo("spawntag_spawn_area", domain, "Spawn entities in area with tag", new[] { "tag", "area_center", "area_radius", "density", "area_fx" }, false, false, new[] { "spawn", "tag", "area" }),
                new FlowInfo("spawntag_spawn_wave", domain, "Spawn wave of entities with tag", new[] { "tag", "spawn_points", "wave_delay", "wave_fx" }, false, false, new[] { "spawn", "tag", "wave" }),
                new FlowInfo("spawntag_dynamic_spawn", domain, "Dynamic spawn with tag", new[] { "spawn_location", "spawn_criteria", "selected_tag", "spawn_rotation" }, false, false, new[] { "spawn", "tag", "dynamic" }),
                new FlowInfo("spawntag_adaptive_spawn", domain, "Adaptive spawn with tag", new[] { "spawn_location", "environment_analysis", "adaptive_tag", "adaptive_rotation" }, false, false, new[] { "spawn", "tag", "adaptive" }),
                new FlowInfo("spawntag_validate", domain, "Validate spawn tag", new[] { "tag", "player", "validation_result" }, false, false, new[] { "spawn", "tag", "validate" }),
                new FlowInfo("spawntag_validate_all", domain, "Validate all spawn tags", new[] { "entity", "player", "tag_permissions" }, false, false, new[] { "spawn", "tag", "validate", "all" }),
                new FlowInfo("spawntag_find_entities", domain, "Find entities with tag", new[] { "tag", "found_entities", "entity_count" }, false, false, new[] { "spawn", "tag", "find", "query" }),
                new FlowInfo("spawntag_find_nearby", domain, "Find nearby entities with tag", new[] { "tag", "center", "radius", "nearby_count" }, false, false, new[] { "spawn", "tag", "find", "nearby" }),
                new FlowInfo("spawntag_statistics", domain, "Get spawn tag statistics", new[] { "tag_stats", "stats_info" }, false, false, new[] { "spawn", "tag", "statistics", "query" }),
                new FlowInfo("spawntag_top_tags", domain, "Get top spawn tags", new[] { "limit", "top_tags" }, false, false, new[] { "spawn", "tag", "top", "statistics" }),
                new FlowInfo("spawntag_admin_spawn", domain, "Admin: spawn with tag", new[] { "requesting_admin", "admin_tag", "target_position", "admin_count", "admin_fx" }, true, false, new[] { "admin", "spawn", "tag" }),
                new FlowInfo("spawntag_admin_clear_all", domain, "Admin: clear all spawn tags", new[] { "requesting_admin", "all_entities" }, true, false, new[] { "admin", "spawn", "tag", "clear", "batch" }),
                new FlowInfo("spawntag_admin_mass_spawn", domain, "Admin: mass spawn with tag", new[] { "requesting_admin", "admin_tag", "spawn_positions", "mass_fx", "spawn_count" }, true, false, new[] { "admin", "spawn", "tag", "mass" }),
                new FlowInfo("spawntag_batch_add", domain, "Batch add spawn tags", new[] { "entities", "tags", "tag_count", "entity_count" }, false, false, new[] { "spawn", "tag", "batch", "add" }),
                new FlowInfo("spawntag_batch_remove", domain, "Batch remove spawn tags", new[] { "entities", "tags", "tag_count", "entity_count" }, false, false, new[] { "spawn", "tag", "batch", "remove" }),
                new FlowInfo("spawntag_replace", domain, "Replace spawn tag", new[] { "entity", "old_tag", "new_tag" }, false, false, new[] { "spawn", "tag", "replace" }),
                new FlowInfo("spawntag_filter_entities", domain, "Filter entities by tags", new[] { "all_entities", "include_tags", "exclude_tags", "filtered_count" }, false, false, new[] { "spawn", "tag", "filter" }),
                new FlowInfo("spawntag_filter_spawn", domain, "Filter spawn by tags", new[] { "spawn_pool", "allowed_tags", "selected_tag", "spawn_position" }, false, false, new[] { "spawn", "tag", "filter", "spawn" }),
                new FlowInfo("spawntag_on_spawn", domain, "Handle spawn tag events", new[] { "spawned_entity", "spawn_tags", "spawn_effects" }, false, false, new[] { "spawn", "tag", "event" }),
                new FlowInfo("spawntag_on_despawn", domain, "Handle despawn tag events", new[] { "despawned_entity", "spawn_tags", "spawn_effects" }, false, false, new[] { "spawn", "tag", "event", "despawn" })
            };

            RegisterFlows(domain, flows);
        }

        /// <summary>
        /// Register Visibility and Stealth flows.
        /// </summary>
        private static void RegisterVisibilityAndStealthFlows()
        {
            var domain = FlowDomain.VisibilityAndStealth;
            var flows = new List<FlowInfo>
            {
                new FlowInfo("stealth_enter", domain, "Enter stealth mode", new[] { "player", "stealth_visibility", "stealth_buff" }, false, false, new[] { "stealth", "enter", "visibility" }),
                new FlowInfo("stealth_exit", domain, "Exit stealth mode", new[] { "player", "normal_visibility", "stealth_buff" }, false, false, new[] { "stealth", "exit", "visibility" }),
                new FlowInfo("stealth_broken", domain, "Handle stealth being broken", new[] { "player", "normal_visibility", "stealth_buff", "detection_source" }, false, false, new[] { "stealth", "broken", "detection" }),
                new FlowInfo("detection_player_detected", domain, "Handle player detection", new[] { "detector", "detected_player", "detection_type" }, false, false, new[] { "detection", "player", "stealth" }),
                new FlowInfo("detection_area_sweep", domain, "Perform area detection sweep", new[] { "detector", "area_center", "detection_range", "detection_count" }, false, false, new[] { "detection", "area", "sweep" }),
                new FlowInfo("detection_perimeter_check", domain, "Check perimeter for crossings", new[] { "detector", "perimeter_center", "perimeter_radius", "perimeter_duration", "crossing_entities" }, false, false, new[] { "detection", "perimeter", "check" }),
                new FlowInfo("visibility_reduce", domain, "Reduce visibility level", new[] { "target", "reduced_visibility", "reduction_buff" }, false, false, new[] { "visibility", "reduce", "stealth" }),
                new FlowInfo("visibility_increase", domain, "Increase visibility level", new[] { "target", "increased_visibility", "increase_buff" }, false, false, new[] { "visibility", "increase", "stealth" }),
                new FlowInfo("visibility_restore", domain, "Restore visibility to normal", new[] { "target", "normal_visibility", "visibility_buffs" }, false, false, new[] { "visibility", "restore", "stealth" }),
                new FlowInfo("los_check", domain, "Check line of sight", new[] { "observer", "target", "has_los" }, false, false, new[] { "los", "line_of_sight", "check" }),
                new FlowInfo("los_block", domain, "Block line of sight", new[] { "observer", "target", "duration", "los_block_buff" }, false, false, new[] { "los", "block", "line_of_sight" }),
                new FlowInfo("los_clear", domain, "Clear line of sight block", new[] { "observer", "target", "los_block_buff" }, false, false, new[] { "los", "clear", "line_of_sight" }),
                new FlowInfo("detection_range_increase", domain, "Increase detection range", new[] { "entity", "increased_range", "range_buff", "range_value" }, false, false, new[] { "detection", "range", "increase" }),
                new FlowInfo("detection_range_decrease", domain, "Decrease detection range", new[] { "entity", "decreased_range", "range_value" }, false, false, new[] { "detection", "range", "decrease" }),
                new FlowInfo("detection_range_reset", domain, "Reset detection range to normal", new[] { "entity", "range_value" }, false, false, new[] { "detection", "range", "reset" }),
                new FlowInfo("visibility_admin_global", domain, "Admin: set global visibility", new[] { "requesting_admin", "visibility_level", "admin_buff", "all_players" }, true, false, new[] { "admin", "visibility", "global" }),
                new FlowInfo("visibility_admin_player", domain, "Admin: set player visibility", new[] { "requesting_admin", "target_player", "visibility_level", "admin_buff" }, true, false, new[] { "admin", "visibility", "player" }),
                new FlowInfo("stealth_equip_bonus", domain, "Apply stealth equipment bonus", new[] { "player", "stealth_item", "item_bonus", "enhanced_stealth" }, false, false, new[] { "stealth", "equipment", "bonus" }),
                new FlowInfo("stealth_unequip_penalty", domain, "Remove stealth equipment penalty", new[] { "player", "stealth_item", "item_bonus", "normal_stealth" }, false, false, new[] { "stealth", "equipment", "penalty" }),
                new FlowInfo("visibility_weather_affect", domain, "Weather affects visibility", new[] { "zone", "weather_type", "weather_buff", "all_players" }, false, false, new[] { "visibility", "weather", "environment" }),
                new FlowInfo("visibility_time_affect", domain, "Time affects visibility", new[] { "zone", "time_of_day", "time_buff", "all_players" }, false, false, new[] { "visibility", "time", "environment" }),
                new FlowInfo("stealth_status", domain, "Check stealth status", new[] { "player", "stealth_info" }, false, false, new[] { "stealth", "status", "query" }),
                new FlowInfo("visibility_status", domain, "Check visibility status", new[] { "player", "visibility_info" }, false, false, new[] { "visibility", "status", "query" })
            };

            RegisterFlows(domain, flows);
        }

        /// <summary>
        /// Register flows for a specific domain.
        /// </summary>
        private static void RegisterFlows(FlowDomain domain, List<FlowInfo> flows)
        {
            if (!_flowsByDomain.ContainsKey(domain))
            {
                _flowsByDomain[domain] = new List<FlowInfo>();
            }

            foreach (var flow in flows)
            {
                _flowsByDomain[domain].Add(flow);
                _flowsByName[flow.Name] = flow;
                _flowDomains[flow.Name] = domain;
            }

            Log.LogInfo($"Registered {flows.Count} flows for domain: {domain}");
        }

        /// <summary>
        /// Get all flows for a specific domain.
        /// </summary>
        public static List<FlowInfo> GetFlowsByDomain(FlowDomain domain)
        {
            return _flowsByDomain.TryGetValue(domain, out var flows) ? flows : new List<FlowInfo>();
        }

        /// <summary>
        /// Get flow information by name.
        /// </summary>
        public static FlowInfo GetFlowByName(string flowName)
        {
            return _flowsByName.TryGetValue(flowName, out var flow) ? flow : null;
        }

        /// <summary>
        /// Get the domain for a specific flow.
        /// </summary>
        public static FlowDomain GetFlowDomain(string flowName)
        {
            return _flowDomains.TryGetValue(flowName, out var domain) ? domain : FlowDomain.GameObjects; // Default fallback
        }

        /// <summary>
        /// Get all registered flows.
        /// </summary>
        public static Dictionary<string, FlowInfo> GetAllFlows()
        {
            return new Dictionary<string, FlowInfo>(_flowsByName);
        }

        /// <summary>
        /// Search flows by tag.
        /// </summary>
        public static List<FlowInfo> SearchFlowsByTag(string tag)
        {
            return _flowsByName.Values.Where(f => f.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase)).ToList();
        }

        /// <summary>
        /// Search flows by domain and tag.
        /// </summary>
        public static List<FlowInfo> SearchFlowsByDomainAndTag(FlowDomain domain, string tag)
        {
            return _flowsByDomain.TryGetValue(domain, out var flows) 
                ? flows.Where(f => f.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase)).ToList()
                : new List<FlowInfo>();
        }

        /// <summary>
        /// Get flows that require admin permissions.
        /// </summary>
        public static List<FlowInfo> GetAdminFlows()
        {
            return _flowsByName.Values.Where(f => f.IsAdminOnly).ToList();
        }

        /// <summary>
        /// Get flows that require special permissions.
        /// </summary>
        public static List<FlowInfo> GetPermissionRequiredFlows()
        {
            return _flowsByName.Values.Where(f => f.RequiresPermission).ToList();
        }

        /// <summary>
        /// Get flow statistics.
        /// </summary>
        public static Dictionary<FlowDomain, int> GetFlowStatistics()
        {
            return _flowsByDomain.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Count);
        }

        /// <summary>
        /// Validate flow registry integrity.
        /// </summary>
        public static bool ValidateRegistry()
        {
            var issues = new List<string>();

            // Check for duplicate flow names
            var domainDuplicates = _flowsByDomain
                .SelectMany(kvp => kvp.Value.Select(flow => flow.Name))
                .GroupBy(name => name, StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key);

            foreach (var duplicate in domainDuplicates)
            {
                issues.Add($"Duplicate flow name: {duplicate}");
            }

            // Check for orphaned domain references
            foreach (var flow in _flowsByName.Values)
            {
                if (!_flowsByDomain.ContainsKey(flow.Domain))
                {
                    issues.Add($"Flow {flow.Name} references unknown domain {flow.Domain}");
                }
            }

            if (issues.Count > 0)
            {
                Log.LogError($"Flow registry validation failed: {string.Join(", ", issues)}");
                return false;
            }

            Log.LogInfo("Flow registry validation passed");
            return true;
        }

        /// <summary>
        /// Get a formatted summary of all flows.
        /// </summary>
        public static string GetRegistrySummary()
        {
            var summary = new System.Text.StringBuilder();
            summary.AppendLine("Flow Registry Summary:");
            summary.AppendLine($"Total Flows: {_flowsByName.Count}");
            summary.AppendLine($"Total Domains: {_flowsByDomain.Count}");
            summary.AppendLine();

            foreach (var domain in _flowsByDomain.Keys)
            {
                var flows = _flowsByDomain[domain];
                summary.AppendLine($"{domain}: {flows.Count} flows");
                
                var adminFlows = flows.Count(f => f.IsAdminOnly);
                var permissionFlows = flows.Count(f => f.RequiresPermission);
                
                if (adminFlows > 0 || permissionFlows > 0)
                {
                    summary.AppendLine($"  - Admin flows: {adminFlows}");
                    summary.AppendLine($"  - Permission flows: {permissionFlows}");
                }
            }

            return summary.ToString();
        }
    }
}
