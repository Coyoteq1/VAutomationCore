using System.Collections.Generic;
using VAutomationCore.Core.Gameplay.Shared.Contracts;

namespace VAutomationCore.Core.Gameplay.Arena.Data
{
    /// <summary>
    /// Arena-specific entity role definitions.
    /// This type is owned by Arena module - not shared.
    /// </summary>
    public sealed class ArenaEntityRoles
    {
        /// <summary>
        /// Get all entity roles defined for arena.
        /// </summary>
        public static IReadOnlyDictionary<string, EntityRoleDefinition> GetRoles()
        {
            return new Dictionary<string, EntityRoleDefinition>
            {
                // Player roles
                ["ArenaPlayer"] = new EntityRoleDefinition
                {
                    RoleName = "ArenaPlayer",
                    Description = "A player participating in arena",
                    IsPlayer = true,
                    IsTarget = false,
                    IsObjective = false
                },
                ["ArenaParticipant"] = new EntityRoleDefinition
                {
                    RoleName = "ArenaParticipant",
                    Description = "A player actively participating in a match",
                    IsPlayer = true,
                    IsTarget = false,
                    IsObjective = false
                },
                ["ArenaSpectator"] = new EntityRoleDefinition
                {
                    RoleName = "ArenaSpectator",
                    Description = "A player spectating the arena",
                    IsPlayer = true,
                    IsTarget = false,
                    IsObjective = false
                },
                
                // Team roles
                ["ArenaTeamMember"] = new EntityRoleDefinition
                {
                    RoleName = "ArenaTeamMember",
                    Description = "A player on a specific team",
                    IsPlayer = true,
                    IsTarget = false,
                    IsObjective = false
                },
                ["Team1Player"] = new EntityRoleDefinition
                {
                    RoleName = "Team1Player",
                    Description = "A player on team 1",
                    IsPlayer = true,
                    IsTarget = true,
                    IsObjective = false
                },
                ["Team2Player"] = new EntityRoleDefinition
                {
                    RoleName = "Team2Player",
                    Description = "A player on team 2",
                    IsPlayer = true,
                    IsTarget = true,
                    IsObjective = false
                },
                
                // Objective roles
                ["ArenaFlag"] = new EntityRoleDefinition
                {
                    RoleName = "ArenaFlag",
                    Description = "Capture the flag objective",
                    IsPlayer = false,
                    IsTarget = false,
                    IsObjective = true
                },
                ["ArenaCapturePoint"] = new EntityRoleDefinition
                {
                    RoleName = "ArenaCapturePoint",
                    Description = "King of the hill capture point",
                    IsPlayer = false,
                    IsTarget = false,
                    IsObjective = true
                },
                ["ArenaReward"] = new EntityRoleDefinition
                {
                    RoleName = "ArenaReward",
                    Description = "Reward chest or item",
                    IsPlayer = false,
                    IsTarget = false,
                    IsObjective = true
                },
                
                // Enemy roles
                ["ArenaEnemy"] = new EntityRoleDefinition
                {
                    RoleName = "ArenaEnemy",
                    Description = "Any enemy spawned in arena (PvE mode)",
                    IsPlayer = false,
                    IsTarget = true,
                    IsObjective = false
                },
                ["ArenaBasicEnemy"] = new EntityRoleDefinition
                {
                    RoleName = "ArenaBasicEnemy",
                    Description = "Basic enemy in wave mode",
                    IsPlayer = false,
                    IsTarget = true,
                    IsObjective = false
                },
                ["ArenaEliteEnemy"] = new EntityRoleDefinition
                {
                    RoleName = "ArenaEliteEnemy",
                    Description = "Elite enemy in wave mode",
                    IsPlayer = false,
                    IsTarget = true,
                    IsObjective = false
                },
                ["ArenaBoss"] = new EntityRoleDefinition
                {
                    RoleName = "ArenaBoss",
                    Description = "Boss enemy in boss rush mode",
                    IsPlayer = false,
                    IsTarget = true,
                    IsObjective = true
                },

                // Admin/system roles
                ["ArenaSpawnPoint"] = new EntityRoleDefinition
                {
                    RoleName = "ArenaSpawnPoint",
                    Description = "Spawn point for players",
                    IsPlayer = false,
                    IsTarget = false,
                    IsObjective = false
                },
                ["ArenaBoundary"] = new EntityRoleDefinition
                {
                    RoleName = "ArenaBoundary",
                    Description = "Arena boundary/wall",
                    IsPlayer = false,
                    IsTarget = false,
                    IsObjective = false
                }
            };
        }

        /// <summary>
        /// Get all player-related roles.
        /// </summary>
        public static IReadOnlyList<string> GetPlayerRoles()
        {
            return new[] { "ArenaPlayer", "ArenaParticipant", "ArenaSpectator", "ArenaTeamMember", "Team1Player", "Team2Player" };
        }

        /// <summary>
        /// Get all target roles (can be attacked).
        /// </summary>
        public static IReadOnlyList<string> GetTargetRoles()
        {
            return new[] { "Team1Player", "Team2Player", "ArenaEnemy", "ArenaBasicEnemy", "ArenaEliteEnemy", "ArenaBoss" };
        }

        /// <summary>
        /// Get all objective roles.
        /// </summary>
        public static IReadOnlyList<string> GetObjectiveRoles()
        {
            return new[] { "ArenaFlag", "ArenaCapturePoint", "ArenaReward", "ArenaBoss" };
        }
    }
}
