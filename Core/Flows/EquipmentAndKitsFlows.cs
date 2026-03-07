using System.Collections.Generic;
using VAutomationCore.Core.Api;
using VAutomationCore.Core.Services;

namespace VAutomationCore.Core.Flows
{
    /// <summary>
    /// Equipment and kit system flows for managing inventory, equipment slots, and item kits.
    /// Handles equipment swapping, inventory management, kit creation, and item distribution.
    /// </summary>
    public static class EquipmentAndKitsFlows
    {
        /// <summary>
        /// Register all equipment and kit-related flows with the FlowService.
        /// </summary>
        public static void RegisterEquipmentAndKitsFlows()
        {
            // Equipment slot flows
            FlowService.RegisterFlow("equipment_equip", new[]
            {
                new FlowStep("equip_item", "@player", "@item", "@slot"),
                new FlowStep("sendmessagetouser", "@player", "Equipped @item_name to @slot_name"),
                new FlowStep("progress_achievement", "@player", "items_equipped", 1)
            });

            FlowService.RegisterFlow("equipment_unequip", new[]
            {
                new FlowStep("unequip_item", "@player", "@slot"),
                new FlowStep("sendmessagetouser", "@player", "Unequipped item from @slot_name"),
                new FlowStep("progress_achievement", "@player", "items_unequipped", 1)
            });

            FlowService.RegisterFlow("equipment_swap", new[]
            {
                new FlowStep("swap_equipment", "@player", "@slot1", "@slot2"),
                new FlowStep("sendmessagetouser", "@player", "Swapped equipment between @slot1_name and @slot2_name"),
                new FlowStep("progress_achievement", "@player", "equipment_swapped", 1)
            });

            FlowService.RegisterFlow("equipment_clear_all", new[]
            {
                new FlowStep("clear_all_equipment", "@player"),
                new FlowStep("sendmessagetouser", "@player", "All equipment cleared"),
                new FlowStep("progress_achievement", "@player", "equipment_cleared", 1)
            });

            // Inventory management flows
            FlowService.RegisterFlow("inventory_add", new[]
            {
                new FlowStep("add_inventory_item", "@player", "@item", "@quantity"),
                new FlowStep("sendmessagetouser", "@player", "Added @quantity x @item_name to inventory"),
                new FlowStep("progress_achievement", "@player", "items_added", 1)
            });

            FlowService.RegisterFlow("inventory_remove", new[]
            {
                new FlowStep("remove_inventory_item", "@player", "@item", "@quantity"),
                new FlowStep("sendmessagetouser", "@player", "Removed @quantity x @item_name from inventory"),
                new FlowStep("progress_achievement", "@player", "items_removed", 1)
            });

            FlowService.RegisterFlow("inventory_transfer", new[]
            {
                new FlowStep("transfer_inventory_item", "@source_player", "@target_player", "@item", "@quantity"),
                new FlowStep("sendmessagetouser", "@source_player", "Transferred @quantity x @item_name to @target_player_name"),
                new FlowStep("sendmessagetouser", "@target_player", "Received @quantity x @item_name from @source_player_name"),
                new FlowStep("progress_achievement", "@source_player", "items_transferred", 1)
            });

            FlowService.RegisterFlow("inventory_clear", new[]
            {
                new FlowStep("clear_inventory", "@player"),
                new FlowStep("sendmessagetouser", "@player", "Inventory cleared"),
                new FlowStep("progress_achievement", "@player", "inventory_cleared", 1)
            });

            // Kit creation flows
            FlowService.RegisterFlow("kit_create", new[]
            {
                new FlowStep("create_kit", "@player", "@kit_name", "@kit_items"),
                new FlowStep("sendmessagetouser", "@player", "Kit created: @kit_name"),
                new FlowStep("progress_achievement", "@player", "kits_created", 1)
            });

            FlowService.RegisterFlow("kit_save", new[]
            {
                new FlowStep("save_kit", "@player", "@kit_name", "@current_equipment"),
                new FlowStep("sendmessagetouser", "@player", "Equipment saved as kit: @kit_name"),
                new FlowStep("progress_achievement", "@player", "kits_saved", 1)
            });

            FlowService.RegisterFlow("kit_load", new[]
            {
                new FlowStep("load_kit", "@player", "@kit_name"),
                new FlowStep("sendmessagetouser", "@player", "Kit loaded: @kit_name"),
                new FlowStep("progress_achievement", "@player", "kits_loaded", 1)
            });

            FlowService.RegisterFlow("kit_delete", new[]
            {
                new FlowStep("delete_kit", "@player", "@kit_name"),
                new FlowStep("sendmessagetouser", "@player", "Kit deleted: @kit_name"),
                new FlowStep("progress_achievement", "@player", "kits_deleted", 1)
            });

            // Kit distribution flows
            FlowService.RegisterFlow("kit_distribute", new[]
            {
                new FlowStep("distribute_kit", "@target_players", "@kit_name"),
                new FlowStep("sendmessagetoall", "Kit distributed: @kit_name"),
                new FlowStep("progress_achievement", "@all_players", "kits_received", 1)
            });

            FlowService.RegisterFlow("kit_distribute_party", new[]
            {
                new FlowStep("distribute_kit_party", "@player", "@kit_name"),
                new FlowStep("sendmessagetouser", "@player", "Kit distributed to party: @kit_name"),
                new FlowStep("progress_achievement", "@party_members", "kits_received", 1)
            });

            // Equipment preset flows
            FlowService.RegisterFlow("preset_save", new[]
            {
                new FlowStep("save_equipment_preset", "@player", "@preset_name"),
                new FlowStep("sendmessagetouser", "@player", "Equipment preset saved: @preset_name"),
                new FlowStep("progress_achievement", "@player", "presets_saved", 1)
            });

            FlowService.RegisterFlow("preset_load", new[]
            {
                new FlowStep("load_equipment_preset", "@player", "@preset_name"),
                new FlowStep("sendmessagetouser", "@player", "Equipment preset loaded: @preset_name"),
                new FlowStep("progress_achievement", "@player", "presets_loaded", 1)
            });

            FlowService.RegisterFlow("preset_delete", new[]
            {
                new FlowStep("delete_equipment_preset", "@player", "@preset_name"),
                new FlowStep("sendmessagetouser", "@player", "Equipment preset deleted: @preset_name"),
                new FlowStep("progress_achievement", "@player", "presets_deleted", 1)
            });

            // Item durability flows
            FlowService.RegisterFlow("durability_repair", new[]
            {
                new FlowStep("repair_item", "@player", "@item", "@repair_amount"),
                new FlowStep("sendmessagetouser", "@player", "Item repaired: @item_name (+@repair_amount durability)"),
                new FlowStep("progress_achievement", "@player", "items_repaired", 1)
            });

            FlowService.RegisterFlow("durability_damage", new[]
            {
                new FlowStep("damage_item", "@player", "@item", "@damage_amount"),
                new FlowStep("sendmessagetouser", "@player", "Item damaged: @item_name (-@damage_amount durability)"),
                new FlowStep("progress_achievement", "@player", "items_damaged", 1)
            });

            FlowService.RegisterFlow("durability_break", new[]
            {
                new FlowStep("break_item", "@player", "@item"),
                new FlowStep("sendmessagetouser", "@player", "Item broken: @item_name"),
                new FlowStep("progress_achievement", "@player", "items_broken", 1)
            });

            // Admin equipment flows
            FlowService.RegisterFlow("equipment_admin_give", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("give_equipment", "@target_player", "@item", "@quantity"),
                new FlowStep("sendmessagetouser", "@target_player", "Admin gave you @quantity x @item_name"),
                new FlowStep("sendmessagetouser", "@requesting_admin", "Gave equipment to @target_player_name")
            });

            FlowService.RegisterFlow("equipment_admin_remove", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("remove_equipment", "@target_player", "@item", "@quantity"),
                new FlowStep("sendmessagetouser", "@target_player", "Admin removed @quantity x @item_name from you"),
                new FlowStep("sendmessagetouser", "@requesting_admin", "Removed equipment from @target_player_name")
            });

            FlowService.RegisterFlow("equipment_admin_clear", new[]
            {
                new FlowStep("admin_validate_user", "@requesting_admin"),
                new FlowStep("clear_all_equipment", "@target_player"),
                new FlowStep("sendmessagetouser", "@target_player", "Admin cleared all your equipment"),
                new FlowStep("sendmessagetouser", "@requesting_admin", "Cleared equipment for @target_player_name")
            });

            // Equipment status flows
            FlowService.RegisterFlow("equipment_status", new[]
            {
                new FlowStep("check_equipment", "@player"),
                new FlowStep("sendmessagetouser", "@player", "Equipment status: @equipment_info"),
                new FlowStep("progress_achievement", "@player", "equipment_checks", 1)
            });

            FlowService.RegisterFlow("inventory_status", new[]
            {
                new FlowStep("check_inventory", "@player"),
                new FlowStep("sendmessagetouser", "@player", "Inventory status: @inventory_info"),
                new FlowStep("progress_achievement", "@player", "inventory_checks", 1)
            });

            // Equipment upgrade flows
            FlowService.RegisterFlow("equipment_upgrade", new[]
            {
                new FlowStep("upgrade_equipment", "@player", "@item", "@upgrade_materials"),
                new FlowStep("sendmessagetouser", "@player", "Equipment upgraded: @item_name"),
                new FlowStep("progress_achievement", "@player", "equipment_upgraded", 1)
            });

            FlowService.RegisterFlow("equipment_enchant", new[]
            {
                new FlowStep("enchant_equipment", "@player", "@item", "@enchantment"),
                new FlowStep("sendmessagetouser", "@player", "Equipment enchanted: @item_name with @enchantment_name"),
                new FlowStep("progress_achievement", "@player", "equipment_enchanted", 1)
            });
        }

        /// <summary>
        /// Get all equipment and kit flow names for registration.
        /// </summary>
        public static string[] GetEquipmentAndKitsFlowNames()
        {
            return new[]
            {
                "equipment_equip", "equipment_unequip", "equipment_swap", "equipment_clear_all",
                "inventory_add", "inventory_remove", "inventory_transfer", "inventory_clear",
                "kit_create", "kit_save", "kit_load", "kit_delete", "kit_distribute", "kit_distribute_party",
                "preset_save", "preset_load", "preset_delete",
                "durability_repair", "durability_damage", "durability_break",
                "equipment_admin_give", "equipment_admin_remove", "equipment_admin_clear",
                "equipment_status", "inventory_status", "equipment_upgrade", "equipment_enchant"
            };
        }
    }
}
