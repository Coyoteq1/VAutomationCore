using System.Collections.Generic;
using System.Text.Json.Serialization;
using ProjectM;
using Stunlock.Core;
using Unity.Entities;
using VAutomationCore.Core;

namespace VAutomationCore.Abstractions
{
    /// <summary>
    /// Data models for kit configuration system
    /// </summary>

    public class KitSettings
    {
        public bool ClearInventory { get; set; } = true;
        public string ZoneId { get; set; } = "";
        public string KitId { get; set; } = "";
    }

    public class BloodSettings
    {
        public bool FillBloodPool { get; set; }
        public bool GiveBloodPotion { get; set; }
        public string PrimaryType { get; set; } = "";
        public string SecondaryType { get; set; } = "";
        public int PrimaryQuality { get; set; } = 100;
        public int SecondaryQuality { get; set; } = 100;
        public int SecondaryBuffIndex { get; set; } = 0;
    }

    public class ArmorSet
    {
        [JsonPropertyName("Boots")]
        public string Boots { get; set; } = "";
        
        [JsonPropertyName("Chest")]
        public string Chest { get; set; } = "";
        
        [JsonPropertyName("Gloves")]
        public string Gloves { get; set; } = "";
        
        [JsonPropertyName("Legs")]
        public string Legs { get; set; } = "";
        
        [JsonPropertyName("MagicSource")]
        public string MagicSource { get; set; } = "";
        
        [JsonPropertyName("Head")]
        public string Head { get; set; } = "";
        
        [JsonPropertyName("Cloak")]
        public string Cloak { get; set; } = "";
        
        [JsonPropertyName("Bag")]
        public string Bag { get; set; } = "";
    }

    public class WeaponConfig
    {
        public string Name { get; set; } = "";
        public string InfuseSpellMod { get; set; } = "";
        public string SpellMod1 { get; set; } = "";
        public string SpellMod2 { get; set; } = "";
        public string StatMod1 { get; set; } = "";
        public float StatMod1Power { get; set; } = 1.0f;
        public string StatMod2 { get; set; } = "";
        public float StatMod2Power { get; set; } = 1.0f;
        public string StatMod3 { get; set; } = "";
        public float StatMod3Power { get; set; } = 1.0f;
        public string StatMod4 { get; set; } = "";
        public float StatMod4Power { get; set; } = 1.0f;
    }

    public class ItemConfig
    {
        public string Name { get; set; } = "";
        public int Amount { get; set; } = 1;
    }

    public class AbilityJewel
    {
        public string SpellMod1 { get; set; } = "";
        public float SpellMod1Power { get; set; } = 1.0f;
        public string SpellMod2 { get; set; } = "";
        public float SpellMod2Power { get; set; } = 1.0f;
        public string SpellMod3 { get; set; } = "";
        public float SpellMod3Power { get; set; } = 1.0f;
        public string SpellMod4 { get; set; } = "";
        public float SpellMod4Power { get; set; } = 1.0f;
    }

    public class AbilityConfig
    {
        public string Name { get; set; } = "";
        public AbilityJewel Jewel { get; set; } = new AbilityJewel();
    }

    public class KitConfiguration
    {
        public KitSettings Settings { get; set; } = new KitSettings();
        public BloodSettings Blood { get; set; } = new BloodSettings();
        public ArmorSet Armors { get; set; } = new ArmorSet();
        public List<WeaponConfig> Weapons { get; set; } = new List<WeaponConfig>();
        public List<ItemConfig> Items { get; set; } = new List<ItemConfig>();
        public Dictionary<string, AbilityConfig> Abilities { get; set; } = new Dictionary<string, AbilityConfig>();
        public List<string> PassiveSpells { get; set; } = new List<string>();
    }

    public class KitsData
    {
        public Dictionary<string, KitConfiguration> Kits { get; set; } = new Dictionary<string, KitConfiguration>();
    }

    /// <summary>
    /// Centralized kit management service for VAutomationCore.
    /// Provides methods to apply and remove kits based on item prefab GUIDs.
    /// </summary>
    public static class Kits
    {
        /// <summary>
        /// Callback delegate for kit-related events.
        /// </summary>
        public delegate void KitEvent(Entity playerEntity, string kitId, string zoneId);

        /// <summary>
        /// Event fired when a kit is applied to a player.
        /// </summary>
        public static event KitEvent OnKitApplied;

        /// <summary>
        /// Event fired when a kit is removed from a player.
        /// </summary>
        public static event KitEvent OnKitRemoved;

        /// <summary>
        /// Converts a prefab name string to PrefabGUID using known mappings.
        /// </summary>
        /// <param name="prefabName">The prefab name to convert</param>
        /// <returns>PrefabGUID if found, empty otherwise</returns>
        private static PrefabGUID GetPrefabGuid(string prefabName)
        {
            if (string.IsNullOrEmpty(prefabName))
                return PrefabGUID.Empty;

            // First try to get GUID directly from KnownGuids
            var directGuid = VAutomationCore.Core.Data.DataType.Prefabs.GetPrefabGuid(prefabName);
            if (directGuid != PrefabGUID.Empty)
            {
                return directGuid;
            }

            // Try to get the full prefab name from our mappings
            var fullPrefabName = VAutomationCore.Core.Data.DataType.Prefabs.GetPrefabName(prefabName);
            if (!string.IsNullOrEmpty(fullPrefabName))
            {
                // Try to get GUID for the full name
                var fullGuid = VAutomationCore.Core.Data.DataType.Prefabs.GetPrefabGuid(fullPrefabName);
                if (fullGuid != PrefabGUID.Empty)
                {
                    return fullGuid;
                }
            }

            // Return empty GUID if not found
            return PrefabGUID.Empty;
        }

        /// <summary>
        /// Applies a kit configuration to the target entity.
        /// </summary>
        /// <param name="userEntity">The user/caster entity</param>
        /// <param name="targetEntity">The target entity to apply the kit to</param>
        /// <param name="kitConfig">The kit configuration to apply</param>
        /// <returns>True if kit was applied successfully, false otherwise</returns>
        public static bool ApplyKit(Entity userEntity, Entity targetEntity, KitConfiguration kitConfig)
        {
            try
            {
                var em = UnifiedCore.EntityManager;
                if (!em.Exists(userEntity) || !em.Exists(targetEntity) || kitConfig == null)
                {
                    return false;
                }

                // Clear inventory if specified
                if (kitConfig.Settings.ClearInventory)
                {
                    Items.ClearInventory(targetEntity);
                }

                // Apply blood settings
                ApplyBloodSettings(targetEntity, kitConfig.Blood);

                // Apply armor items
                ApplyArmorItems(userEntity, targetEntity, kitConfig.Armors);

                // Apply weapons
                ApplyWeapons(userEntity, targetEntity, kitConfig.Weapons);

                // Apply general items
                ApplyGeneralItems(userEntity, targetEntity, kitConfig.Items);

                // Apply abilities
                ApplyAbilities(userEntity, targetEntity, kitConfig.Abilities);

                // Apply passive spells
                ApplyPassiveSpells(userEntity, targetEntity, kitConfig.PassiveSpells);

                // Fire event
                OnKitApplied?.Invoke(targetEntity, kitConfig.Settings.KitId, kitConfig.Settings.ZoneId);

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Removes a kit from the target entity by clearing inventory and removing abilities.
        /// </summary>
        /// <param name="targetEntity">The target entity to remove the kit from</param>
        /// <param name="kitConfig">The kit configuration to remove (for knowing what to remove)</param>
        /// <returns>True if kit was removed successfully, false otherwise</returns>
        public static bool RemoveKit(Entity targetEntity, KitConfiguration kitConfig)
        {
            try
            {
                var em = UnifiedCore.EntityManager;
                if (!em.Exists(targetEntity) || kitConfig == null)
                {
                    return false;
                }

                // Clear inventory
                Items.ClearInventory(targetEntity);

                // Remove abilities
                RemoveAbilities(targetEntity, kitConfig.Abilities);

                // Remove passive spells
                RemovePassiveSpells(targetEntity, kitConfig.PassiveSpells);

                // Fire event
                OnKitRemoved?.Invoke(targetEntity, kitConfig.Settings.KitId, kitConfig.Settings.ZoneId);

                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void ApplyBloodSettings(Entity targetEntity, BloodSettings blood)
        {
            // Blood pool filling would be implemented here
            // This would require access to blood system components
        }

        private static void ApplyArmorItems(Entity userEntity, Entity targetEntity, ArmorSet armors)
        {
            var armorPieces = new[]
            {
                ("Boots", armors.Boots),
                ("Chest", armors.Chest),
                ("Gloves", armors.Gloves),
                ("Legs", armors.Legs),
                ("MagicSource", armors.MagicSource),
                ("Head", armors.Head),
                ("Cloak", armors.Cloak),
                ("Bag", armors.Bag)
            };

            foreach (var (slot, prefabName) in armorPieces)
            {
                if (!string.IsNullOrEmpty(prefabName))
                {
                    var prefabGuid = GetPrefabGuid(prefabName);
                    if (prefabGuid != PrefabGUID.Empty)
                    {
                        Items.AddItem(userEntity, targetEntity, prefabGuid);
                    }
                }
            }
        }

        private static void ApplyWeapons(Entity userEntity, Entity targetEntity, List<WeaponConfig> weapons)
        {
            foreach (var weapon in weapons)
            {
                if (!string.IsNullOrEmpty(weapon.Name))
                {
                    var prefabGuid = GetPrefabGuid(weapon.Name);
                    if (prefabGuid != PrefabGUID.Empty)
                    {
                        Items.AddItem(userEntity, targetEntity, prefabGuid);
                    }
                }
            }
        }

        private static void ApplyGeneralItems(Entity userEntity, Entity targetEntity, List<ItemConfig> items)
        {
            foreach (var item in items)
            {
                if (!string.IsNullOrEmpty(item.Name))
                {
                    var prefabGuid = GetPrefabGuid(item.Name);
                    if (prefabGuid != PrefabGUID.Empty)
                    {
                        Items.AddItem(userEntity, targetEntity, prefabGuid, item.Amount);
                    }
                }
            }
        }

        private static void ApplyAbilities(Entity userEntity, Entity targetEntity, Dictionary<string, AbilityConfig> abilities)
        {
            foreach (var ability in abilities.Values)
            {
                if (!string.IsNullOrEmpty(ability.Name))
                {
                    var prefabGuid = GetPrefabGuid(ability.Name);
                    if (prefabGuid != PrefabGUID.Empty)
                    {
                        Abilities.GrantAbility(userEntity, targetEntity, prefabGuid);
                    }
                }
            }
        }

        private static void ApplyPassiveSpells(Entity userEntity, Entity targetEntity, List<string> passiveSpells)
        {
            foreach (var spell in passiveSpells)
            {
                if (!string.IsNullOrEmpty(spell))
                {
                    var prefabGuid = GetPrefabGuid(spell);
                    if (prefabGuid != PrefabGUID.Empty)
                    {
                        Abilities.GrantAbility(userEntity, targetEntity, prefabGuid);
                    }
                }
            }
        }

        private static void RemoveAbilities(Entity targetEntity, Dictionary<string, AbilityConfig> abilities)
        {
            foreach (var ability in abilities.Values)
            {
                if (!string.IsNullOrEmpty(ability.Name))
                {
                    var prefabGuid = GetPrefabGuid(ability.Name);
                    if (prefabGuid != PrefabGUID.Empty)
                    {
                        Abilities.RemoveAbility(targetEntity, prefabGuid);
                    }
                }
            }
        }

        private static void RemovePassiveSpells(Entity targetEntity, List<string> passiveSpells)
        {
            foreach (var spell in passiveSpells)
            {
                if (!string.IsNullOrEmpty(spell))
                {
                    var prefabGuid = GetPrefabGuid(spell);
                    if (prefabGuid != PrefabGUID.Empty)
                    {
                        Abilities.RemoveAbility(targetEntity, prefabGuid);
                    }
                }
            }
        }
    }
}
