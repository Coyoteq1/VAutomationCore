using System.Collections.Generic;
using VAutomationCore.Core.Api;
using VAutomationCore.Core.Services;

namespace VAutomationCore.Core.Flows
{
    /// <summary>
    /// Visual effects and game objects flows for managing particle systems, visual effects, and spawned entities.
    /// Handles FX creation, object spawning, visual feedback, and environmental effects.
    /// </summary>
    public static class FXAndGameObjectsFlows
    {
        /// <summary>
        /// Register all visual effects and game object flows with the FlowService.
        /// </summary>
        public static void RegisterFXAndGameObjectsFlows()
        {
            // Visual effect flows
            FlowService.RegisterFlow("fx_play", new[]
            {
                new FlowStep("spawn_visual_effect", "@player", "@fx_guid", "@position"),
                new FlowStep("sendmessagetouser", "@player", "Visual effect played: @fx_name"),
                new FlowStep("progress_achievement", "@player", "fx_played", 1)
            });

            FlowService.RegisterFlow("fx_play_loop", new[]
            {
                new FlowStep("spawn_visual_effect_loop", "@player", "@fx_guid", "@position"),
                new FlowStep("sendmessagetouser", "@player", "Looping visual effect: @fx_name"),
                new FlowStep("progress_achievement", "@player", "fx_loops", 1)
            });

            FlowService.RegisterFlow("fx_stop", new[]
            {
                new FlowStep("stop_visual_effect", "@fx_entity"),
                new FlowStep("sendmessagetouser", "@player", "Visual effect stopped: @fx_name"),
                new FlowStep("progress_achievement", "@player", "fx_stopped", 1)
            });

            FlowService.RegisterFlow("fx_stop_all", new[]
            {
                new FlowStep("stop_all_visual_effects", "@player"),
                new FlowStep("sendmessagetouser", "@player", "All visual effects stopped"),
                new FlowStep("progress_achievement", "@player", "fx_stopped_all", 1)
            });

            // Particle system flows
            FlowService.RegisterFlow("particle_spawn", new[]
            {
                new FlowStep("spawn_particle_system", "@player", "@particle_guid", "@position"),
                new FlowStep("sendmessagetouser", "@player", "Particle system spawned: @particle_name"),
                new FlowStep("progress_achievement", "@player", "particles_spawned", 1)
            });

            FlowService.RegisterFlow("particle_stop", new[]
            {
                new FlowStep("stop_particle_system", "@particle_entity"),
                new FlowStep("sendmessagetouser", "@player", "Particle system stopped: @particle_name"),
                new FlowStep("progress_achievement", "@player", "particles_stopped", 1)
            });

            // Game object spawning flows
            FlowService.RegisterFlow("object_spawn", new[]
            {
                new FlowStep("spawn_game_object", "@player", "@object_guid", "@position"),
                new FlowStep("sendmessagetouser", "@player", "Object spawned: @object_name"),
                new FlowStep("progress_achievement", "@player", "objects_spawned", 1)
            });

            FlowService.RegisterFlow("object_spawn_with_rotation", new[]
            {
                new FlowStep("spawn_game_object", "@player", "@object_guid", "@position", "@rotation"),
                new FlowStep("sendmessagetouser", "@player", "Object spawned with rotation: @object_name"),
                new FlowStep("progress_achievement", "@player", "objects_spawned", 1)
            });

            FlowService.RegisterFlow("object_despawn", new[]
            {
                new FlowStep("despawn_game_object", "@object_entity"),
                new FlowStep("sendmessagetouser", "@player", "Object despawned: @object_name"),
                new FlowStep("progress_ambient", "@player", "objects_despawned", 1)
            });

            // Environmental effect flows
            FlowService.RegisterFlow("environment_weather_change", new[]
            {
                new FlowStep("change_weather", "@zone", "@weather_type"),
                new FlowStep("sendmessagetoall", "Weather changed to: @weather_name"),
                new FlowStep("progress_achievement", "@player", "weather_changes", 1)
            });

            FlowService.RegisterFlow("environment_time_change", new[]
            {
                new FlowStep("change_time", "@zone", "@time_of_day"),
                new FlowStep("sendmessagetoall", "Time changed to: @time_name"),
                new FlowStep("progress_achievement", "@player", "time_changes", 1)
            });

            FlowService.RegisterFlow("environment_lighting", new[]
            {
                new FlowStep("modify_lighting", "@zone", "@lighting_params"),
                new FlowStep("sendmessagetoall", "Lighting modified in zone: @zone_name"),
                new FlowStep("progress_achievement", "@player", "lighting_changes", 1)
            });

            // Decal and decoration flows
            FlowService.RegisterFlow("decal_place", new[]
            {
                new FlowStep("place_decal", "@player", "@decal_guid", "@position", "@rotation"),
                new FlowStep("sendmessagetouser", "@player", "Decal placed: @decal_name"),
                new FlowStep("progress_achievement", "@player", "decals_placed", 1)
            });

            FlowService.RegisterFlow("decal_remove", new[]
            {
                new FlowStep("remove_decal", "@decal_entity"),
                new FlowStep("sendmessagetouser", "@player", "Decal removed: @decal_name"),
                new FlowStep("progress_achievement", "@player", "decals_removed", 1)
            });

            FlowService.RegisterFlow("decal_clear_all", new[]
            {
                new FlowStep("clear_all_decals", "@zone"),
                new FlowStep("sendmessagetoall", "All decals cleared in zone: @zone_name"),
                new FlowStep("progress_achievement", "@player", "decals_cleared", 1)
            });

            // Sound effect flows
            FlowService.RegisterFlow("sound_play", new[]
            {
                new FlowStep("play_sound", "@player", "@sound_guid", "@position"),
                new FlowStep("sendmessagetouser", "@player", "Sound played: @sound_name"),
                new FlowStep("progress_achievement", "@player", "sounds_played", 1)
            });

            FlowService.RegisterFlow("sound_play_loop", new[]
            {
                new FlowStep("play_sound_loop", "@player", "@sound_guid", "@position"),
                new FlowStep("sendmessagetouser", "@player", "Looping sound: @sound_name"),
                new FlowStep("progress_achievement", "@player", "sound_loops", 1)
            });

            FlowService.RegisterFlow("sound_stop", new[]
            {
                new FlowStep("stop_sound", "@sound_entity"),
                new FlowStep("sendmessagetouser", "@player", "Sound stopped: @sound_name"),
                new FlowStep("progress_achievement", "@player", "sounds_stopped", 1)
            });

            // Admin visual effect flows
            FlowService.RegisterFlow("fx_admin_play_all", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("spawn_visual_effect", "@all_players", "@fx_guid", "@player_position"),
                new FlowStep("sendmessagetoall", "Admin played visual effect: @fx_name"),
                new FlowStep("progress_achievement", "@all_players", "fx_admin_played", 1)
            });

            FlowService.RegisterFlow("fx_admin_stop_all", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("stop_all_visual_effects", "@all_players"),
                new FlowStep("sendmessagetoall", "Admin stopped all visual effects"),
                new FlowStep("progress_achievement", "@all_players", "fx_admin_stopped", 1)
            });

            // Visual effect status flows
            FlowService.RegisterFlow("fx_status", new[]
            {
                new FlowStep("check_visual_effects", "@player"),
                new FlowStep("sendmessagetouser", "@player", "Active effects: @active_effects_count"),
                new FlowStep("progress_achievement", "@player", "fx_checks", 1)
            });

            // Visual effect cleanup flows
            FlowService.RegisterFlow("fx_cleanup", new[]
            {
                new FlowStep("cleanup_visual_effects", "@zone"),
                new FlowStep("sendmessagetoall", "Visual effects cleaned up in zone: @zone_name"),
                new FlowStep("progress_achievement", "@all_players", "fx_cleanup", 1)
            });
        }

        /// <summary>
        /// Get all visual effects and game object flow names for registration.
        /// </summary>
        public static string[] GetFXAndGameObjectsFlowNames()
        {
            return new[]
            {
                "fx_play", "fx_play_loop", "fx_stop", "fx_stop_all",
                "particle_spawn", "particle_stop",
                "object_spawn", "object_spawn_with_rotation", "object_despawn",
                "environment_weather_change", "environment_time_change", "environment_lighting",
                "decal_place", "decal_remove", "decal_clear_all",
                "sound_play", "sound_play_loop", "sound_stop",
                "fx_admin_play_all", "fx_admin_stop_all", "fx_status", "fx_cleanup"
            };
        }
    }
}
