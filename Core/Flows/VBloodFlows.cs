using System.Collections.Generic;
using VAutomationCore.Core.Api;
using VAutomationCore.Core.Services;

namespace VAutomationCore.Core.Flows
{
    /// <summary>
    /// VBlood system flows for managing VBlood hunts, tracking, and progression.
    /// Handles blood altar interactions, VBlood unit tracking, and VBlood progression unlocks.
    /// </summary>
    public static class VBloodFlows
    {
        /// <summary>
        /// Register all VBlood-related flows with the FlowService.
        /// </summary>
        public static void RegisterVBloodFlows()
        {
            // VBlood tracking start flows
            FlowService.RegisterFlow("vblood_track_start", new[]
            {
                new FlowStep("bloodaltar_track_start", "@blood_altar", "@vblood_unit", "@player"),
                new FlowStep("sendmessagetouser", "@player", "Started tracking VBlood unit: @vblood_name"),
                new FlowStep("sendmessagetoall", "@player_name started tracking @vblood_name!"),
                new FlowStep("progress_achievement", "@player", "vblood_tracks_started", 1)
            });

            FlowService.RegisterFlow("vblood_track_start_altar", new[]
            {
                new FlowStep("bloodaltar_track_start", "@blood_altar", "@vblood_unit", "@player"),
                new FlowStep("sendmessagetouser", "@player", "Blood altar activated: Tracking @vblood_name"),
                new FlowStep("progress_achievement", "@player", "altar_activations", 1)
            });

            // VBlood tracking stop flows
            FlowService.RegisterFlow("vblood_track_stop_success", new[]
            {
                new FlowStep("bloodaltar_track_stop", "@blood_altar", "@vblood_unit", "@player"),
                new FlowStep("sendmessagetouser", "@player", "VBlood unit defeated: @vblood_name!"),
                new FlowStep("sendmessagetoall", "@player_name defeated @vblood_name!"),
                new FlowStep("progress_achievement", "@player", "vblood_kills", 1),
                new FlowStep("unlock_progression", "@player", "vblood", "@vblood_guid")
            });

            FlowService.RegisterFlow("vblood_track_stop_failed", new[]
            {
                new FlowStep("bloodaltar_track_stop", "@blood_altar", "@vblood_unit", "@player"),
                new FlowStep("sendmessagetouser", "@player", "VBlood hunt failed: @vblood_name escaped"),
                new FlowStep("progress_achievement", "@player", "vblood_failures", 1)
            });

            FlowService.RegisterFlow("vblood_track_stop_timeout", new[]
            {
                new FlowStep("bloodaltar_track_stop", "@blood_altar", "@vblood_unit", "@player"),
                new FlowStep("sendmessagetouser", "@player", "VBlood hunt timed out: @vblood_name"),
                new FlowStep("progress_achievement", "@player", "vblood_timeouts", 1)
            });

            // VBlood altar management flows
            FlowService.RegisterFlow("vblood_altar_activate", new[]
            {
                new FlowStep("bloodaltar_track_start", "@blood_altar", "@dummy_unit", "@player"),
                new FlowStep("sendmessagetouser", "@player", "Blood altar activated and ready"),
                new FlowStep("progress_achievement", "@player", "altar_uses", 1)
            });

            FlowService.RegisterFlow("vblood_altar_reset", new[]
            {
                new FlowStep("bloodaltar_track_stop", "@blood_altar", "@all_units", "@player"),
                new FlowStep("sendmessagetouser", "@player", "Blood altar reset - all tracking stopped"),
                new FlowStep("progress_achievement", "@player", "altar_resets", 1)
            });

            // Admin VBlood tracking flows
            FlowService.RegisterFlow("vblood_admin_track_all", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("bloodaltar_track_start", "@all_altars", "@all_vblood_units", "@target_player"),
                new FlowStep("sendmessagetouser", "@requesting_admin", "Admin: Started tracking all VBlood units"),
                new FlowStep("sendmessagetouser", "@target_player", "Admin started all VBlood tracking for you")
            });

            FlowService.RegisterFlow("vblood_admin_stop_all", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("bloodaltar_track_stop", "@all_altars", "@all_vblood_units", "@target_player"),
                new FlowStep("sendmessagetouser", "@requesting_admin", "Admin: Stopped all VBlood tracking"),
                new FlowStep("sendmessagetouser", "@target_player", "Admin stopped all VBlood tracking for you")
            });

            // VBlood tracking status flows
            FlowService.RegisterFlow("vblood_track_status", new[]
            {
                new FlowStep("bloodaltar_track_start", "@blood_altar", "@status_check", "@player"),
                new FlowStep("sendmessagetouser", "@player", "Tracking status: @tracking_info"),
                new FlowStep("progress_achievement", "@player", "tracking_checks", 1)
            });

            // VBlood reward flows
            FlowService.RegisterFlow("vblood_reward_claim", new[]
            {
                new FlowStep("bloodaltar_track_stop", "@blood_altar", "@vblood_unit", "@player"),
                new FlowStep("grant_progression", "@player", "vblood_reward", "@reward_type"),
                new FlowStep("sendmessagetouser", "@player", "VBlood reward claimed: @reward_name"),
                new FlowStep("progress_achievement", "@player", "rewards_claimed", 1)
            });

            // VBlood progression flows
            FlowService.RegisterFlow("vblood_progression_unlock", new[]
            {
                new FlowStep("unlock_vblood_progression", "@player", "@vblood_guid"),
                new FlowStep("unlock_spellbook_abilities", "@player"),
                new FlowStep("sendmessagetouser", "@player", "VBlood progression unlocked! New abilities available."),
                new FlowStep("progress_achievement", "@player", "vblood_unlock", 1)
            });

            FlowService.RegisterFlow("vblood_milestone", new[]
            {
                new FlowStep("progress_achievement", "@player", "vblood_milestone", 1),
                new FlowStep("grant_progression", "@player", "vblood", "milestone_reward"),
                new FlowStep("sendmessagetouser", "@player", "VBlood milestone reached! New rewards unlocked.")
            });
        }

        /// <summary>
        /// Get all VBlood flow names for registration.
        /// </summary>
        public static string[] GetVBloodFlowNames()
        {
            return new[]
            {
                "vblood_track_start", "vblood_track_start_altar", "vblood_track_stop_success",
                "vblood_track_stop_failed", "vblood_track_stop_timeout", "vblood_altar_activate",
                "vblood_altar_reset", "vblood_admin_track_all", "vblood_admin_stop_all",
                "vblood_track_status", "vblood_reward_claim", "vblood_progression_unlock", "vblood_milestone"
            };
        }
    }
}
