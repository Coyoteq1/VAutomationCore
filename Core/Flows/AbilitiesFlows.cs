using System.Collections.Generic;
using VAutomationCore.Core.Api;
using VAutomationCore.Core.Services;

namespace VAutomationCore.Core.Flows
{
    /// <summary>
    /// Ability system flows for managing ability casts, cooldowns, and ability modifications.
    /// Handles ability slot management, cooldown tracking, and custom ability mechanics.
    /// </summary>
    public static class AbilitiesFlows
    {
        /// <summary>
        /// Register all ability-related flows with the FlowService.
        /// </summary>
        public static void RegisterAbilitiesFlows()
        {
            // Ability casting flows
            FlowService.RegisterFlow("ability_cast_start", new[]
            {
                new FlowStep("ability_cast", "@player", "@ability", "@target"),
                new FlowStep("sendmessagetouser", "@player", "Casting ability: @ability_name"),
                new FlowStep("progress_achievement", "@player", "abilities_cast", 1)
            });

            FlowService.RegisterFlow("ability_cast_success", new[]
            {
                new FlowStep("ability_cast", "@player", "@ability", "@target"),
                new FlowStep("sendmessagetouser", "@player", "Ability cast successfully: @ability_name"),
                new FlowStep("progress_achievement", "@player", "successful_casts", 1),
                new FlowStep("start_cooldown", "@player", "@ability", "@cooldown_time")
            });

            FlowService.RegisterFlow("ability_cast_failed", new[]
            {
                new FlowStep("sendmessagetouser", "@player", "Ability cast failed: @ability_name - @failure_reason"),
                new FlowStep("progress_achievement", "@player", "failed_casts", 1)
            });

            // Ability cooldown flows
            FlowService.RegisterFlow("ability_cooldown_start", new[]
            {
                new FlowStep("start_cooldown", "@player", "@ability", "@duration"),
                new FlowStep("sendmessagetouser", "@player", "Cooldown started for @ability_name: @duration seconds"),
                new FlowStep("progress_achievement", "@player", "cooldowns_started", 1)
            });

            FlowService.RegisterFlow("ability_cooldown_end", new[]
            {
                new FlowStep("sendmessagetouser", "@player", "Cooldown ended: @ability_name"),
                new FlowStep("progress_achievement", "@player", "cooldowns_ended", 1)
            });

            FlowService.RegisterFlow("ability_cooldown_reduce", new[]
            {
                new FlowStep("modify_cooldown", "@player", "@ability", "@reduction"),
                new FlowStep("sendmessagetouser", "@player", "Cooldown reduced for @ability_name by @reduction%"),
                new FlowStep("progress_achievement", "@player", "cooldowns_reduced", 1)
            });

            // Ability slot management flows
            FlowService.RegisterFlow("ability_slot_assign", new[]
            {
                new FlowStep("assign_ability_slot", "@player", "@slot_index", "@ability"),
                new FlowStep("sendmessagetouser", "@player", "Ability assigned to slot @slot_index: @ability_name"),
                new FlowStep("progress_achievement", "@player", "abilities_assigned", 1)
            });

            FlowService.RegisterFlow("ability_slot_remove", new[]
            {
                new FlowStep("remove_ability_slot", "@player", "@slot_index"),
                new FlowStep("sendmessagetouser", "@player", "Ability removed from slot @slot_index"),
                new FlowStep("progress_achievement", "@player", "abilities_removed", 1)
            });

            FlowService.RegisterFlow("ability_slot_clear", new[]
            {
                new FlowStep("clear_all_ability_slots", "@player"),
                new FlowStep("sendmessagetouser", "@player", "All ability slots cleared"),
                new FlowStep("progress_achievement", "@player", "slots_cleared", 1)
            });

            FlowService.RegisterFlow("ability_slot_swap", new[]
            {
                new FlowStep("swap_ability_slots", "@player", "@slot1", "@slot2"),
                new FlowStep("sendmessagetouser", "@player", "Swapped abilities in slots @slot1 and @slot2"),
                new FlowStep("progress_achievement", "@player", "abilities_swapped", 1)
            });

            // Ability learning flows
            FlowService.RegisterFlow("ability_learn", new[]
            {
                new FlowStep("learn_ability", "@player", "@ability"),
                new FlowStep("sendmessagetouser", "@player", "New ability learned: @ability_name"),
                new FlowStep("progress_achievement", "@player", "abilities_learned", 1),
                new FlowStep("assign_ability_slot", "@player", "@auto_slot", "@ability")
            });

            FlowService.RegisterFlow("ability_forget", new[]
            {
                new FlowStep("forget_ability", "@player", "@ability"),
                new FlowStep("sendmessagetouser", "@player", "Ability forgotten: @ability_name"),
                new FlowStep("progress_achievement", "@player", "abilities_forgotten", 1),
                new FlowStep("remove_ability_slot", "@player", "@ability_slot")
            });

            // Ability unlock flows
            FlowService.RegisterFlow("ability_unlock", new[]
            {
                new FlowStep("unlock_ability", "@player", "@ability"),
                new FlowStep("sendmessagetouser", "@player", "Ability unlocked: @ability_name"),
                new FlowStep("progress_achievement", "@player", "abilities_unlocked", 1),
                new FlowStep("assign_ability_slot", "@player", "@auto_slot", "@ability")
            });

            FlowService.RegisterFlow("ability_lock", new[]
            {
                new FlowStep("lock_ability", "@player", "@ability"),
                new FlowStep("sendmessagetouser", "@player", "Ability locked: @ability_name"),
                new FlowStep("progress_achievement", "@player", "abilities_locked", 1)
            });

            // Admin ability flows
            FlowService.RegisterFlow("ability_admin_grant", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("grant_ability", "@target_player", "@ability"),
                new FlowStep("sendmessagetouser", "@target_player", "Admin granted ability: @ability_name"),
                new FlowStep("sendmessagetouser", "@requesting_admin", "Ability granted to @target_player_name")
            });

            FlowService.RegisterFlow("ability_admin_remove", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("remove_ability", "@target_player", "@ability"),
                new FlowStep("sendmessagetouser", "@target_player", "Admin removed ability: @ability_name"),
                new FlowStep("sendmessagetouser", "@requesting_admin", "Ability removed from @target_player_name")
            });

            FlowService.RegisterFlow("ability_admin_clear_all", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("clear_all_ability_slots", "@target_player"),
                new FlowStep("sendmessagetouser", "@target_player", "Admin cleared all your abilities"),
                new FlowStep("sendmessagetouser", "@requesting_admin", "All abilities cleared for @target_player_name")
            });

            // Ability modification flows
            FlowService.RegisterFlow("ability_modify_damage", new[]
            {
                new FlowStep("modify_ability_damage", "@player", "@ability", "@damage_modifier"),
                new FlowStep("sendmessagetouser", "@player", "Ability damage modified: @ability_name (+@damage_modifier%)"),
                new FlowStep("progress_achievement", "@player", "abilities_modified", 1)
            });

            FlowService.RegisterFlow("ability_modify_cooldown", new[]
            {
                new FlowStep("modify_ability_cooldown", "@player", "@ability", "@cooldown_modifier"),
                new FlowStep("sendmessagetouser", "@player", "Ability cooldown modified: @ability_name (@cooldown_modifier%)"),
                new FlowStep("progress_achievement", "@player", "abilities_modified", 1)
            });

            // Ability reset flows
            FlowService.RegisterFlow("ability_reset_cooldowns", new[]
            {
                new FlowStep("reset_all_cooldowns", "@player"),
                new FlowStep("sendmessagetouser", "@player", "All ability cooldowns reset"),
                new FlowStep("progress_achievement", "@player", "cooldowns_reset", 1)
            });

            FlowService.RegisterFlow("ability_reset_all", new[]
            {
                new FlowStep("clear_all_ability_slots", "@player"),
                new FlowStep("reset_all_cooldowns", "@player"),
                new FlowStep("sendmessagetouser", "@player", "All abilities and cooldowns reset"),
                new FlowStep("progress_achievement", "@player", "abilities_reset", 1)
            });
        }

        /// <summary>
        /// Get all ability flow names for registration.
        /// </summary>
        public static string[] GetAbilitiesFlowNames()
        {
            return new[]
            {
                "ability_cast_start", "ability_cast_success", "ability_cast_failed",
                "ability_cooldown_start", "ability_cooldown_end", "ability_cooldown_reduce",
                "ability_slot_assign", "ability_slot_remove", "ability_slot_clear", "ability_slot_swap",
                "ability_learn", "ability_forget", "ability_unlock", "ability_lock",
                "ability_admin_grant", "ability_admin_remove", "ability_admin_clear_all",
                "ability_modify_damage", "ability_modify_cooldown", "ability_reset_cooldowns", "ability_reset_all"
            };
        }
    }
}
