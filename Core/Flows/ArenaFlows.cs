using VAutomationCore.Core.Api;
using VAutomationCore.Core.Services;

namespace VAutomationCore.Core.Flows
{
    /// <summary>
    /// Arena flows for managing arena lifecycle, zone linkage, match state, spectator control, PvP and PvE/boss content.
    /// </summary>
    public static class ArenaFlows
    {
        /// <summary>
        /// Register all arena flows with the FlowService.
        /// </summary>
        public static void RegisterArenaFlows()
        {
            // ========================================
            // ARENA MANAGEMENT FLOWS
            // ========================================

            FlowService.RegisterFlow("arena_enable", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("enable_arena", "@arena_id"),
                new FlowStep("enable_zone_for_arena", "@zone"),
                new FlowStep("apply_arena_rules_profile", "@arena_id", "@rules_profile"),
                new FlowStep("sendmessagetoall", "Arena enabled: @arena_name")
            });

            FlowService.RegisterFlow("arena_disable", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("disable_arena", "@arena_id"),
                new FlowStep("disable_zone_for_arena", "@zone"),
                new FlowStep("clear_arena_match_state", "@arena_id"),
                new FlowStep("sendmessagetoall", "Arena disabled: @arena_name")
            });

            FlowService.RegisterFlow("arena_create", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("create_arena", "@arena_id", "@center", "@radius"),
                new FlowStep("bind_arena_zone", "@arena_id", "@zone"),
                new FlowStep("apply_arena_rules_profile", "@arena_id", "@rules_profile"),
                new FlowStep("sendmessagetouser", "@requesting_admin", "Arena created: @arena_name")
            });

            FlowService.RegisterFlow("arena_delete", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("disable_arena", "@arena_id"),
                new FlowStep("unbind_arena_zone", "@arena_id", "@zone"),
                new FlowStep("delete_arena", "@arena_id"),
                new FlowStep("sendmessagetouser", "@requesting_admin", "Arena deleted: @arena_name")
            });

            FlowService.RegisterFlow("arena_bind_zone", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("bind_arena_zone", "@arena_id", "@zone"),
                new FlowStep("apply_arena_rules_profile", "@arena_id", "@rules_profile"),
                new FlowStep("sendmessagetouser", "@requesting_admin", "Arena zone bound: @arena_name -> @zone_name")
            });

            FlowService.RegisterFlow("arena_unbind_zone", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("clear_arena_rules_profile", "@arena_id"),
                new FlowStep("unbind_arena_zone", "@arena_id", "@zone"),
                new FlowStep("sendmessagetouser", "@requesting_admin", "Arena zone unbound: @arena_name")
            });

            // ========================================
            // ARENA MATCH FLOWS
            // ========================================

            FlowService.RegisterFlow("arena_match_start", new[]
            {
                new FlowStep("ensure_arena_enabled", "@arena_id"),
                new FlowStep("arena_assign_participants", "@arena_id", "@participants"),
                new FlowStep("arena_start_match", "@arena_id", "@match_mode"),
                new FlowStep("sendmessagetoall", "Arena match started in @arena_name")
            });

            FlowService.RegisterFlow("arena_match_stop", new[]
            {
                new FlowStep("arena_stop_match", "@arena_id", "@stop_reason"),
                new FlowStep("arena_clear_match_effects", "@arena_id"),
                new FlowStep("sendmessagetoall", "Arena match stopped in @arena_name")
            });

            FlowService.RegisterFlow("arena_match_reset", new[]
            {
                new FlowStep("arena_stop_match", "@arena_id", "@reset_mode"),
                new FlowStep("arena_reset_state", "@arena_id"),
                new FlowStep("arena_restore_zone_defaults", "@arena_id"),
                new FlowStep("sendmessagetoall", "Arena reset in @arena_name")
            });

            // ========================================
            // ARENA PLAYER FLOWS
            // ========================================

            FlowService.RegisterFlow("arena_player_join", new[]
            {
                new FlowStep("arena_add_player", "@arena_id", "@player", "@team"),
                new FlowStep("teleport_entity", "@player", "@arena_spawn_position"),
                new FlowStep("sendmessagetouser", "@player", "Joined arena: @arena_name")
            });

            FlowService.RegisterFlow("arena_player_leave", new[]
            {
                new FlowStep("arena_remove_player", "@arena_id", "@player", "@leave_reason"),
                new FlowStep("teleport_entity", "@player", "@arena_exit_position"),
                new FlowStep("sendmessagetouser", "@player", "Left arena: @arena_name")
            });

            FlowService.RegisterFlow("arena_spectator_enable", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("arena_enable_spectator", "@arena_id", "@player", "@view_mode"),
                new FlowStep("sendmessagetouser", "@player", "Spectator mode enabled for @arena_name")
            });

            FlowService.RegisterFlow("arena_spectator_disable", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("arena_disable_spectator", "@arena_id", "@player"),
                new FlowStep("sendmessagetouser", "@player", "Spectator mode disabled for @arena_name")
            });

            // ========================================
            // ARENA STATUS FLOWS
            // ========================================

            FlowService.RegisterFlow("arena_status", new[]
            {
                new FlowStep("arena_get_status", "@arena_id"),
                new FlowStep("sendmessagetouser", "@player", "Arena status: @arena_status_info")
            });

            FlowService.RegisterFlow("arena_list", new[]
            {
                new FlowStep("arena_list_all", "@arena_state"),
                new FlowStep("sendmessagetouser", "@player", "Arenas: @arena_list")
            });

            // ========================================
            // ARENA PVP FLOWS
            // ========================================

            FlowService.RegisterFlow("arena_pvp_enable", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("enable_arena_pvp", "@arena_id"),
                new FlowStep("apply_pvp_rules", "@arena_id"),
                new FlowStep("spawn_pvp_indicators", "@arena_id"),
                new FlowStep("sendmessagetoall", "PvP enabled in arena: @arena_name")
            });

            FlowService.RegisterFlow("arena_pvp_disable", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("disable_arena_pvp", "@arena_id"),
                new FlowStep("apply_pvp_safety", "@arena_id"),
                new FlowStep("clear_pvp_indicators", "@arena_id"),
                new FlowStep("sendmessagetoall", "PvP disabled in arena: @arena_name")
            });

            FlowService.RegisterFlow("arena_pvp_toggle", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("toggle_arena_pvp", "@arena_id"),
                new FlowStep("update_pvp_state", "@arena_id"),
                new FlowStep("sendmessagetoall", "PvP toggled in arena: @arena_name")
            });

            FlowService.RegisterFlow("arena_friendly_fire_enable", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("enable_friendly_fire", "@arena_id"),
                new FlowStep("sendmessagetoall", "Friendly fire enabled in arena: @arena_name")
            });

            FlowService.RegisterFlow("arena_friendly_fire_disable", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("disable_friendly_fire", "@arena_id"),
                new FlowStep("sendmessagetoall", "Friendly fire disabled in arena: @arena_name")
            });

            // ========================================
            // ARENA BOSS/PVE FLOWS
            // ========================================

            FlowService.RegisterFlow("arena_boss_spawn", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("spawn_boss", "@arena_id", "@boss_type", "@spawn_position"),
                new FlowStep("apply_boss_buffs", "@arena_id", "@boss_entity"),
                new FlowStep("sendmessagetoall", "Boss @boss_name spawned in arena: @arena_name"),
                new FlowStep("progress_achievement", "@all_players", "bosses_spawned", 1)
            });

            FlowService.RegisterFlow("arena_boss_defeat", new[]
            {
                new FlowStep("track_boss_health", "@arena_id", "@boss_entity"),
                new FlowStep("on_boss_defeated", "@arena_id", "@boss_entity"),
                new FlowStep("distribute_rewards", "@arena_id", "@participants"),
                new FlowStep("sendmessagetoall", "Boss defeated in arena: @arena_name!"),
                new FlowStep("progress_achievement", "@all_players", "bosses_defeated", 1)
            });

            FlowService.RegisterFlow("arena_boss_escape", new[]
            {
                new FlowStep("on_boss_escaped", "@arena_id", "@boss_entity"),
                new FlowStep("apply_escape_penalty", "@arena_id"),
                new FlowStep("sendmessagetoall", "Boss escaped from arena: @arena_name")
            });

            FlowService.RegisterFlow("arena_wave_start", new[]
            {
                new FlowStep("start_wave", "@arena_id", "@wave_number"),
                new FlowStep("spawn_wave_enemies", "@arena_id", "@wave_number"),
                new FlowStep("sendmessagetoall", "Wave @wave_number started in arena: @arena_name")
            });

            FlowService.RegisterFlow("arena_wave_complete", new[]
            {
                new FlowStep("complete_wave", "@arena_id", "@wave_number"),
                new FlowStep("distribute_wave_rewards", "@arena_id", "@participants"),
                new FlowStep("sendmessagetoall", "Wave @wave_number completed in arena: @arena_name"),
                new FlowStep("progress_achievement", "@all_players", "waves_completed", 1)
            });

            FlowService.RegisterFlow("arena_wave_skip", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("skip_wave", "@arena_id", "@current_wave"),
                new FlowStep("spawn_wave_enemies", "@arena_id", "@next_wave"),
                new FlowStep("sendmessagetoall", "Wave skipped in arena: @arena_name")
            });

            FlowService.RegisterFlow("arena_difficulty_set", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("set_arena_difficulty", "@arena_id", "@difficulty_level"),
                new FlowStep("apply_difficulty_modifiers", "@arena_id", "@difficulty_modifiers"),
                new FlowStep("sendmessagetoall", "Arena difficulty set to @difficulty_name: @arena_name")
            });

            FlowService.RegisterFlow("arena_enemy_spawn", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("spawn_enemy", "@arena_id", "@enemy_type", "@spawn_position"),
                new FlowStep("apply_enemy_modifiers", "@arena_id", "@enemy_entity"),
                new FlowStep("sendmessagetouser", "@requesting_admin", "Enemy @enemy_name spawned in arena: @arena_name")
            });

            FlowService.RegisterFlow("arena_enemy_clear", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("clear_all_enemies", "@arena_id"),
                new FlowStep("sendmessagetoall", "All enemies cleared from arena: @arena_name")
            });
        }
    }
}

