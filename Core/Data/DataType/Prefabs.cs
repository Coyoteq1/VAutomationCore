using System.Collections.Generic;
using Stunlock.Core;

namespace VAutomationCore.Core.Data.DataType
{
    /// <summary>
    /// Static data mappings for abilities, buffs, and spells with prefab name to GUID conversions.
    /// Similar structure to the Cycleborn Abilities.cs but focused on name-to-prefab mappings.
    /// </summary>
    public static class Prefabs
    {
        /// <summary>
        /// Dictionary mapping short names to full prefab names for abilities
        /// </summary>
        public static readonly Dictionary<string, string> Abilities = new()
        {
            // Vampire Abilities
            ["VeilOfChaos"] = "AB_Vampire_VeilOfChaos_Group",
            ["VeilOfBlood"] = "AB_Vampire_VeilOfBlood_Group", 
            ["VeilOfIllusion"] = "AB_Vampire_VeilOfIllusion_AbilityGroup",
            ["VeilOfFrost"] = "AB_Vampire_VeilOfFrost_Group",
            ["VeilOfBones"] = "AB_Vampire_VeilOfBones_Group",

            // Chaos Abilities
            ["MercilessCharge"] = "AB_Chaos_MercilessCharge_AbilityGroup",
            ["RagingTempest"] = "AB_Storm_RagingTempest_AbilityGroup",
            ["ChaosNova"] = "AB_Chaos_ChaosNova_AbilityGroup",
            ["ChaosBolt"] = "AB_Chaos_ChaosBolt_AbilityGroup",

            // Frost Abilities  
            ["CrystalLance"] = "AB_Frost_CrystalLance_AbilityGroup",
            ["ColdSnap"] = "AB_Frost_ColdSnap_AbilityGroup",
            ["ArcticLeap"] = "AB_Frost_ArcticLeap_AbilityGroup",
            ["FrostBat"] = "AB_Frost_FrostBat_Projectile",
            ["FrostShield"] = "AB_Frost_FrostShield_AbilityGroup",

            // Illusion Abilities
            ["PhantomAegis"] = "AB_Illusion_PhantomAegis_AbilityGroup",
            ["MistTrance"] = "AB_Illusion_MistTrance_AbilityGroup", 
            ["WispDance"] = "AB_Illusion_WispDance_AbilityGroup",
            ["Phantasm"] = "AB_Illusion_Phantasm_AbilityGroup",

            // Unholy Abilities
            ["CorruptedSkull"] = "AB_Unholy_CorruptedSkull_AbilityGroup",
            ["DeathKnight"] = "AB_Unholy_DeathKnight_AbilityGroup",
            ["ArmyOfTheDead"] = "AB_Unholy_ArmyOfTheDead_AbilityGroup",
            ["SkeletalMage"] = "AB_Unholy_SkeletalMage_AbilityGroup",

            // Storm Abilities
            ["Discharge"] = "AB_Storm_Discharge_AbilityGroup",
            ["LightningRing"] = "AB_Storm_LightningRing_AbilityGroup",
            ["StormCall"] = "AB_Storm_StormCall_AbilityGroup",
            ["ConjureFrost"] = "AB_Storm_ConjureFrost_AbilityGroup",

            // Blood Abilities
            ["BloodRage"] = "AB_Blood_BloodRage_AbilityGroup",
            ["BloodRite"] = "AB_Blood_BloodRite_AbilityGroup",
            ["BloodSwarm"] = "AB_Blood_BloodSwarm_AbilityGroup",
            ["BloodBurst"] = "AB_Blood_BloodBurst_AbilityGroup",

            // Movement Abilities
            ["ShadowStep"] = "AB_ShadowStep_AbilityGroup",
            ["BatSwarm"] = "AB_BatSwarm_AbilityGroup",
            ["ChaosBlink"] = "AB_ChaosBlink_AbilityGroup",
            ["PhaseWalk"] = "AB_PhaseWalk_AbilityGroup"
        };

        /// <summary>
        /// Dictionary mapping buff names to prefab names
        /// </summary>
        public static readonly Dictionary<string, string> Buffs = new()
        {
            // Blood Type Buffs
            ["BloodType_Warrior"] = "Buff_BloodType_Warrior",
            ["BloodType_Rogue"] = "Buff_BloodType_Rogue", 
            ["BloodType_Brute"] = "Buff_BloodType_Brute",
            ["BloodType_Scholar"] = "Buff_BloodType_Scholar",
            ["BloodType_Creature"] = "Buff_BloodType_Creature",
            ["BloodType_Draculin"] = "Buff_BloodType_Draculin",

            // PvP Buffs
            ["PvP_Enabled"] = "Buff_PvP_Enabled",
            ["PvP_DamageReduction"] = "Buff_PvP_DamageReduction",
            ["PvP_CooldownReduction"] = "Buff_PvP_CooldownReduction",

            // Protection Buffs
            ["HolyResistance"] = "Buff_HolyResistance",
            ["SilverResistance"] = "Buff_SilverResistance", 
            ["GarlicResistance"] = "Buff_GarlicResistance",
            ["SunResistance"] = "Buff_SunResistance",

            // Combat Buffs
            ["PhysicalPower"] = "Buff_PhysicalPower",
            ["SpellPower"] = "Buff_SpellPower",
            ["CriticalStrike"] = "Buff_CriticalStrike",
            ["MovementSpeed"] = "Buff_MovementSpeed",
            ["Haste"] = "Buff_Haste",
            ["Regeneration"] = "Buff_Regeneration",

            // Special Buffs
            ["Invisibility"] = "Buff_Invisibility",
            ["Invincibility"] = "Buff_Invincibility",
            ["Immortality"] = "Buff_Immortality",
            ["GodMode"] = "Buff_GodMode",

            // Duration Buffs
            ["ShortDuration"] = "Buff_ShortDuration",
            ["MediumDuration"] = "Buff_MediumDuration", 
            ["LongDuration"] = "Buff_LongDuration",
            ["Permanent"] = "Buff_Permanent"
        };

        /// <summary>
        /// Dictionary mapping spell names to prefab names
        /// </summary>
        public static readonly Dictionary<string, string> Spells = new()
        {
            // Passive Spells - Frost
            ["SpellPassive_Frost_T01_ColdSoul"] = "SpellPassive_Frost_T01_ColdSoul",
            ["SpellPassive_Frost_T02_ChillWeave"] = "SpellPassive_Frost_T02_ChillWeave",
            ["SpellPassive_Frost_T03_Bastion"] = "SpellPassive_Frost_T03_Bastion",
            ["SpellPassive_Frost_T04_DarkEnchantment"] = "SpellPassive_Frost_T04_DarkEnchantment",

            // Passive Spells - Chaos
            ["SpellPassive_Chaos_T01_ChaosKindling"] = "SpellPassive_Chaos_T01_ChaosKindling",
            ["SpellPassive_Chaos_T03_Overpower"] = "SpellPassive_Chaos_T03_Overpower",
            ["SpellPassive_Chaos_T04_RavenousStrikes"] = "SpellPassive_Chaos_T04_RavenousStrikes",

            // Passive Spells - Unholy
            ["SpellPassive_Unholy_T01_ArcaneAnimator"] = "SpellPassive_Unholy_T01_ArcaneAnimator",
            ["SpellPassive_Unholy_T04_EmbraceMayhem"] = "SpellPassive_Unholy_T04_EmbraceMayhem",

            // Passive Spells - Storm
            ["SpellPassive_Storm_T01_LightningFastStrikes"] = "SpellPassive_Storm_T01_LightningFastStrikes",
            ["SpellPassive_Storm_T02_EnhancedConductivity"] = "SpellPassive_Storm_T02_EnhancedConductivity",
            ["SpellPassive_Storm_T03_HungerForPower"] = "SpellPassive_Storm_T03_HungerForPower",
            ["SpellPassive_Storm_T04_TurbulentVelocity"] = "SpellPassive_Storm_T04_TurbulentVelocity",

            // Passive Spells - Illusion
            ["SpellPassive_Illusion_T02_FlowingSorcery"] = "SpellPassive_Illusion_T02_FlowingSorcery",
            ["SpellPassive_Illusion_T04_WickedPower"] = "SpellPassive_Illusion_T04_WickedPower",

            // Passive Spells - Blood
            ["SpellPassive_Blood_T01_BloodSpray"] = "SpellPassive_Blood_T01_BloodSpray",
            ["SpellPassive_Blood_T02_BloodTypeEfficiency"] = "SpellPassive_Blood_T02_BloodTypeEfficiency",

            // Active Spells - Veils
            ["Spell_VeilOfChaos"] = "Spell_VeilOfChaos",
            ["Spell_VeilOfBlood"] = "Spell_VeilOfBlood",
            ["Spell_VeilOfIllusion"] = "Spell_VeilOfIllusion", 
            ["Spell_VeilOfFrost"] = "Spell_VeilOfFrost",
            ["Spell_VeilOfBones"] = "Spell_VeilOfBones",

            // Active Spells - Projectiles
            ["Spell_ChaosVolley"] = "Spell_ChaosVolley",
            ["Spell_FrostBat"] = "Spell_FrostBat",
            ["Spell_CorruptedSkull"] = "Spell_CorruptedSkull",
            ["Spell_LightningRing"] = "Spell_LightningRing",

            // Active Spells - Area Effects
            ["Spell_FrostNova"] = "Spell_FrostNova",
            ["Spell_ChaosNova"] = "Spell_ChaosNova",
            ["Spell_BloodSwarm"] = "Spell_BloodSwarm",
            ["Spell_Phantasm"] = "Spell_Phantasm"
        };

        /// <summary>
        /// Dictionary mapping item names to prefab names
        /// </summary>
        public static readonly Dictionary<string, string> Items = new()
        {
            // Armor - T9 Shadow Matter
            ["Item_Armor_Headgear_T09_ShadowMatter"] = "Item_Armor_Headgear_T09_ShadowMatter",
            ["Item_Armor_Chest_T09_ShadowMatter"] = "Item_Armor_Chest_T09_ShadowMatter",
            ["Item_Armor_Legs_T09_ShadowMatter"] = "Item_Armor_Legs_T09_ShadowMatter",
            ["Item_Armor_Gloves_T09_ShadowMatter"] = "Item_Armor_Gloves_T09_ShadowMatter",
            ["Item_Armor_Boots_T09_ShadowMatter"] = "Item_Armor_Boots_T09_ShadowMatter",

            // Armor - T9 Dracula Sets
            ["Item_Boots_T09_Dracula_Warrior"] = "Item_Boots_T09_Dracula_Warrior",
            ["Item_Chest_T09_Dracula_Warrior"] = "Item_Chest_T09_Dracula_Warrior",
            ["Item_Gloves_T09_Dracula_Warrior"] = "Item_Gloves_T09_Dracula_Warrior",
            ["Item_Legs_T09_Dracula_Warrior"] = "Item_Legs_T09_Dracula_Warrior",

            ["Item_Boots_T09_Dracula_Rogue"] = "Item_Boots_T09_Dracula_Rogue",
            ["Item_Chest_T09_Dracula_Rogue"] = "Item_Chest_T09_Dracula_Rogue",
            ["Item_Gloves_T09_Dracula_Rogue"] = "Item_Gloves_T09_Dracula_Rogue",
            ["Item_Legs_T09_Dracula_Rogue"] = "Item_Legs_T09_Dracula_Rogue",

            ["Item_Boots_T09_Dracula_Brute"] = "Item_Boots_T09_Dracula_Brute",
            ["Item_Chest_T09_Dracula_Brute"] = "Item_Chest_T09_Dracula_Brute",
            ["Item_Gloves_T09_Dracula_Brute"] = "Item_Gloves_T09_Dracula_Brute",
            ["Item_Legs_T09_Dracula_Brute"] = "Item_Legs_T09_Dracula_Brute",

            ["Item_Boots_T09_Dracula_Scholar"] = "Item_Boots_T09_Dracula_Scholar",
            ["Item_Chest_T09_Dracula_Scholar"] = "Item_Chest_T09_Dracula_Scholar",
            ["Item_Gloves_T09_Dracula_Scholar"] = "Item_Gloves_T09_Dracula_Scholar",
            ["Item_Legs_T09_Dracula_Scholar"] = "Item_Legs_T09_Dracula_Scholar",

            // Weapons - T9 Shadow Matter
            ["Item_Weapon_Sword_T09_ShadowMatter"] = "Item_Weapon_Sword_T09_ShadowMatter",
            ["Item_Weapon_Axe_T09_ShadowMatter"] = "Item_Weapon_Axe_T09_ShadowMatter",
            ["Item_Weapon_Mace_T09_ShadowMatter"] = "Item_Weapon_Mace_T09_ShadowMatter",
            ["Item_Weapon_Spear_T09_ShadowMatter"] = "Item_Weapon_Spear_T09_ShadowMatter",
            ["Item_Weapon_Slashers_T09_ShadowMatter"] = "Item_Weapon_Slashers_T09_ShadowMatter",
            ["Item_Weapon_Reaper_T09_ShadowMatter"] = "Item_Weapon_Reaper_T09_ShadowMatter",
            ["Item_Weapon_Crossbow_T09_ShadowMatter"] = "Item_Weapon_Crossbow_T09_ShadowMatter",
            ["Item_Weapon_Longbow_T09_ShadowMatter"] = "Item_Weapon_Longbow_T09_ShadowMatter",
            ["Item_Weapon_Pistols_T09_ShadowMatter"] = "Item_Weapon_Pistols_T09_ShadowMatter",
            ["Item_Weapon_GreatSword_T09_ShadowMatter"] = "Item_Weapon_GreatSword_T09_ShadowMatter",
            ["Item_Weapon_Whip_T09_ShadowMatter"] = "Item_Weapon_Whip_T09_ShadowMatter",

            // Weapons - Unique T08 Variations
            ["Item_Weapon_Sword_Unique_T08_Variation01"] = "Item_Weapon_Sword_Unique_T08_Variation01",
            ["Item_Weapon_Axe_Unique_T08_Variation01"] = "Item_Weapon_Axe_Unique_T08_Variation01",
            ["Item_Weapon_Mace_Unique_T08_Variation01"] = "Item_Weapon_Mace_Unique_T08_Variation01",
            ["Item_Weapon_Spear_Unique_T08_Variation01"] = "Item_Weapon_Spear_Unique_T08_Variation01",
            ["Item_Weapon_Slashers_Unique_T08_Variation01"] = "Item_Weapon_Slashers_Unique_T08_Variation01",
            ["Item_Weapon_Reaper_Unique_T08_Variation01"] = "Item_Weapon_Reaper_Unique_T08_Variation01",
            ["Item_Weapon_Crossbow_Unique_T08_Variation01"] = "Item_Weapon_Crossbow_Unique_T08_Variation01",
            ["Item_Weapon_Longbow_Unique_T08_Variation01"] = "Item_Weapon_Longbow_Unique_T08_Variation01",
            ["Item_Weapon_Pistols_Unique_T08_Variation01"] = "Item_Weapon_Pistols_Unique_T08_Variation01",
            ["Item_Weapon_GreatSword_Unique_T08_Variation01"] = "Item_Weapon_GreatSword_Unique_T08_Variation01",
            ["Item_Weapon_Whip_Unique_T08_Variation01"] = "Item_Weapon_Whip_Unique_T08_Variation01",
            ["Item_Weapon_TwinBlades_Unique_T08_Variation01"] = "Item_Weapon_TwinBlades_Unique_T08_Variation01",
            ["Item_Weapon_Claws_Unique_T08_Variation01"] = "Item_Weapon_Claws_Unique_T08_Variation01",

            // Magic Sources
            ["Item_MagicSource_General_T08_Frost"] = "Item_MagicSource_General_T08_Frost",
            ["Item_MagicSource_General_T08_Blood"] = "Item_MagicSource_General_T08_Blood",
            ["Item_MagicSource_General_T08_Illusion"] = "Item_MagicSource_General_T08_Illusion",
            ["Item_MagicSource_General_T08_Unholy"] = "Item_MagicSource_General_T08_Unholy",
            ["Item_MagicSource_General_T08_Storm"] = "Item_MagicSource_General_T08_Storm",
            ["Item_MagicSource_General_T08_Chaos"] = "Item_MagicSource_General_T08_Chaos",

            // Cloaks and Bags
            ["Item_Cloak_Main_T03_Phantom"] = "Item_Cloak_Main_T03_Phantom",
            ["Item_NewBag_T06"] = "Item_NewBag_T06",

            // Consumables
            ["Item_Consumable_Potion_Healing_T03"] = "Item_Consumable_Potion_Healing_T03",
            ["Item_Consumable_Potion_SpellPower_T03"] = "Item_Consumable_Potion_SpellPower_T03",
            ["Item_Consumable_Potion_PhysicalPower_T03"] = "Item_Consumable_Potion_PhysicalPower_T03",
            ["Item_Consumable_HealingPotion_T02"] = "Item_Consumable_HealingPotion_T02",
            ["Item_Consumable_BloodRoseBrew"] = "Item_Consumable_BloodRoseBrew",
            ["Item_Consumable_VerminSalve"] = "Item_Consumable_VerminSalve",
            ["Item_Consumable_Brew_Rage_T03"] = "Item_Consumable_Brew_Rage_T03",
            ["Item_Consumable_Brew_Wrangler_T03"] = "Item_Consumable_Brew_Wrangler_T03",
            ["Item_Consumable_GarlicResistance_T03"] = "Item_Consumable_GarlicResistance_T03",
            ["Item_Consumable_HolyResistance_T03"] = "Item_Consumable_HolyResistance_T03",
            ["Item_Consumable_SilverResistance_T03"] = "Item_Consumable_SilverResistance_T03"
        };

        /// <summary>
        /// Dictionary mapping known PrefabGUID values (from your kits class and other sources)
        /// </summary>
        public static readonly Dictionary<string, PrefabGUID> KnownGuids = new()
        {
            ["AbilityGroupSlot"] = new PrefabGUID(-633717863),
            ["External_Inventory"] = new PrefabGUID(1183666186),
            
            // Combat and PvP Buffs
            ["Buff_InCombat_PvPVampire"] = new PrefabGUID(-1234567890), // Placeholder - replace with actual GUID
            ["Buff_InCombat_PvPPlayer"] = new PrefabGUID(-1234567891), // Placeholder - replace with actual GUID
            ["Buff_PvP_Enabled"] = new PrefabGUID(-1234567892), // Placeholder - replace with actual GUID
            
            // Boosted Player Buffs (from KindredCommands example)
            ["BoostedBuff1"] = new PrefabGUID(-1234567893), // Placeholder - replace with actual GUID
            ["BoostedBuff2"] = new PrefabGUID(-1234567894), // Placeholder - replace with actual GUID
            
            // Common Status Buffs
            ["Buff_GarlicResistance"] = new PrefabGUID(-1234567895), // Placeholder - replace with actual GUID
            ["Buff_HolyResistance"] = new PrefabGUID(-1234567896), // Placeholder - replace with actual GUID
            ["Buff_SilverResistance"] = new PrefabGUID(-1234567897), // Placeholder - replace with actual GUID
            ["Buff_SunResistance"] = new PrefabGUID(-1234567898), // Placeholder - replace with actual GUID
        };

        /// <summary>
        /// Gets the full prefab name for a short name across all dictionaries
        /// </summary>
        /// <param name="shortName">The short name to look up</param>
        /// <returns>Full prefab name if found, null otherwise</returns>
        public static string GetPrefabName(string shortName)
        {
            if (string.IsNullOrEmpty(shortName))
                return null;

            if (Abilities.TryGetValue(shortName, out var ability))
                return ability;

            if (Buffs.TryGetValue(shortName, out var buff))
                return buff;

            if (Spells.TryGetValue(shortName, out var spell))
                return spell;

            if (Items.TryGetValue(shortName, out var item))
                return item;

            return null;
        }

        /// <summary>
        /// Gets a PrefabGUID from the KnownGuids dictionary
        /// </summary>
        /// <param name="key">The key to look up</param>
        /// <returns>PrefabGUID if found, empty otherwise</returns>
        public static PrefabGUID GetPrefabGuid(string key)
        {
            if (string.IsNullOrEmpty(key))
                return PrefabGUID.Empty;

            if (KnownGuids.TryGetValue(key, out var guid))
                return guid;

            return PrefabGUID.Empty;
        }

        /// <summary>
        /// Checks if a prefab name exists in any dictionary
        /// </summary>
        /// <param name="prefabName">The prefab name to check</param>
        /// <returns>True if found, false otherwise</returns>
        public static bool HasPrefab(string prefabName)
        {
            if (string.IsNullOrEmpty(prefabName))
                return false;

            return Abilities.ContainsValue(prefabName) ||
                   Buffs.ContainsValue(prefabName) ||
                   Spells.ContainsValue(prefabName) ||
                   Items.ContainsValue(prefabName);
        }

        /// <summary>
        /// Gets all short names for a given category
        /// </summary>
        /// <param name="category">The category to get names for</param>
        /// <returns>Array of short names in the category</returns>
        public static string[] GetNamesInCategory(string category)
        {
            return category.ToLower() switch
            {
                "abilities" => Abilities.Keys.ToArray(),
                "buffs" => Buffs.Keys.ToArray(),
                "spells" => Spells.Keys.ToArray(),
                "items" => Items.Keys.ToArray(),
                _ => Array.Empty<string>()
            };
        }
    }
}
