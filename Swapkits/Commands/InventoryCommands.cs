using System;
using System.Linq;
using UnityEngine;
using Unity.Mathematics;
using VampireCommandFramework;
using ExtraSlots.Systems;
using ExtraSlots.Models;

namespace ExtraSlots.Commands
{
    /// <summary>
    /// Extra slots commands - uses . prefix.
    /// </summary>
    public static class SlotCommands
    {
        [Command(".", description: "Extra slots commands", adminOnly: false)]
        public static void SlotHelp(ChatCommandContext ctx)
        {
            ctx.Reply("Extra Slots Commands:");
            ctx.Reply("  .extra add <item> - Add item to extra slots");
            ctx.Reply("  .extra list - List extra slots");
            ctx.Reply("  .extra remove <item> - Remove item from extra");
            ctx.Reply("  .extra clear - Clear extra slots");
            ctx.Reply("  .extra transfer <slot> - Transfer slot to extra");
            ctx.Reply("  .extra equip <item> - Equip weapon (30 min cooldown)");
            ctx.Reply("  .extra swap <item> <slot> - Swap weapon (30 min cooldown)");
            ctx.Reply("  .extra quick <item> <slot> - Quick swap (30 min cooldown)");
        }
        
        [Command("extra add", description: "Add item to extra slots", adminOnly: false)]
        public static void AddToExtra(ChatCommandContext ctx, string itemId, int amount = 1)
        {
            var steamId = ctx.User.PlatformId;
            var item = new ItemData { ItemId = itemId, Amount = amount };
            
            if (ExtraSlotsService.Instance.AddItem(steamId, item))
            {
                ctx.Reply($"Added {amount}x {itemId} to extra slots");
            }
            else
            {
                ctx.Reply("Extra slots are full!");
            }
        }
        
        [Command("extra list", "el", description: "List extra slots", adminOnly: false)]
        public static void ListExtra(ChatCommandContext ctx)
        {
            var steamId = ctx.User.PlatformId;
            var items = ExtraSlotsService.Instance.GetItems(steamId);
            
            if (items.Count == 0)
            {
                ctx.Reply("Extra slots are empty");
                return;
            }
            
            ctx.Reply($"Extra Slots ({items.Count} items):");
            foreach (var item in items)
            {
                ctx.Reply($"  - {item.ItemId} x{item.Amount}");
            }
        }
        
        [Command("extra remove", description: "Remove item from extra slots", adminOnly: false)]
        public static void RemoveFromExtra(ChatCommandContext ctx, string itemId)
        {
            var steamId = ctx.User.PlatformId;
            
            if (ExtraSlotsService.Instance.RemoveItem(steamId, itemId))
            {
                ctx.Reply($"Removed {itemId} from extra slots");
            }
            else
            {
                ctx.Reply("Item not found in extra slots");
            }
        }
        
        [Command("extra clear", description: "Clear extra slots", adminOnly: false)]
        public static void ClearExtra(ChatCommandContext ctx)
        {
            var steamId = ctx.User.PlatformId;
            ExtraSlotsService.Instance.Clear(steamId);
            ctx.Reply("Extra slots cleared");
        }
        
        [Command("extra transfer", description: "Transfer item from main to extra", adminOnly: false)]
        public static void TransferToExtra(ChatCommandContext ctx, int slotIndex)
        {
            var steamId = ctx.User.PlatformId;
            
            if (ExtraSlotsService.Instance.TransferToExtra(steamId, slotIndex))
            {
                ctx.Reply($"Transferred slot {slotIndex} to extra slots");
            }
            else
            {
                ctx.Reply("Transfer failed");
            }
        }
        
        [Command("extra equip", description: "Equip weapon (30 min cooldown)", adminOnly: false)]
        public static void EquipNoCooldown(ChatCommandContext ctx, string itemId, int slot = 0)
        {
            var steamId = ctx.User.PlatformId;
            
            // Check cooldown first
            var (canUse, cooldown) = ExtraSlotsService.Instance.UseSlot(steamId);
            if (!canUse)
            {
                ctx.Reply($"Slot on cooldown. Try again in {cooldown} minutes.");
                return;
            }
            
            if (ExtraSlotsService.Instance.EquipWeaponNoCooldown(steamId, itemId, slot))
            {
                ctx.Reply($"Equipped {itemId} to slot {slot}");
            }
            else
            {
                ctx.Reply("Item not found in extra slots");
            }
        }
        
        [Command("extra swap", description: "Swap weapon any slot (30 min cooldown)", adminOnly: false)]
        public static void SwapAnySlot(ChatCommandContext ctx, string itemId, int targetSlot)
        {
            var steamId = ctx.User.PlatformId;
            
            // Check cooldown first
            var (canUse, cooldown) = ExtraSlotsService.Instance.UseSlot(steamId);
            if (!canUse)
            {
                ctx.Reply($"Slot on cooldown. Try again in {cooldown} minutes.");
                return;
            }
            
            if (ExtraSlotsService.Instance.SwapWeaponAnySlot(steamId, itemId, targetSlot))
            {
                ctx.Reply($"Swapped {itemId} to slot {targetSlot}");
            }
            else
            {
                ctx.Reply("Swap failed");
            }
        }
        
        [Command("extra quick", description: "Quick swap weapon (30 min cooldown)", adminOnly: false)]
        public static void QuickSwap(ChatCommandContext ctx, string itemId, int inventorySlot)
        {
            var steamId = ctx.User.PlatformId;
            
            // Check cooldown first
            var (canUse, cooldown) = ExtraSlotsService.Instance.UseSlot(steamId);
            if (!canUse)
            {
                ctx.Reply($"Slot on cooldown. Try again in {cooldown} minutes.");
                return;
            }
            
            if (ExtraSlotsService.Instance.QuickSwapWeapon(steamId, itemId, inventorySlot))
            {
                ctx.Reply($"Quick swapped {itemId} to inventory slot {inventorySlot}");
            }
            else
            {
                ctx.Reply("Quick swap failed");
            }
        }
        
        // KeyBind Commands
        [Command("extra bind", description: "Bind weapon to key", adminOnly: false)]
        public static void BindKey(ChatCommandContext ctx, string key, string itemId)
        {
            var steamId = ctx.User.PlatformId;
            var keyCode = ParseKey(key);
            
            if (keyCode == KeyCode.None)
            {
                ctx.Reply("Invalid key. Use: F1-F12, 1-9, Q, W, E, R, T, Y");
                return;
            }
            
            WeaponBindsSystem.Instance.BindKey(steamId, keyCode, itemId);
            ctx.Reply($"Bound {itemId} to {key}");
        }
        
        [Command("extra unbind", description: "Unbind key", adminOnly: false)]
        public static void UnbindKey(ChatCommandContext ctx, string key)
        {
            var steamId = ctx.User.PlatformId;
            var keyCode = ParseKey(key);
            
            if (keyCode == KeyCode.None)
            {
                ctx.Reply("Invalid key");
                return;
            }
            
            WeaponBindsSystem.Instance.UnbindKey(steamId, keyCode);
            ctx.Reply($"Unbound key {key}");
        }
        
        [Command("extra binds", description: "List keybinds", adminOnly: false)]
        public static void ListBinds(ChatCommandContext ctx)
        {
            var steamId = ctx.User.PlatformId;
            var binds = WeaponBindsSystem.Instance.GetBinds(steamId);
            
            if (binds.Count == 0)
            {
                ctx.Reply("No keybinds set");
                return;
            }
            
            ctx.Reply("Keybinds:");
            foreach (var bind in binds)
            {
                ctx.Reply($"  {(KeyCode)bind.Key} -> {bind.Value}");
            }
        }
        
        private static KeyCode ParseKey(string key)
        {
            key = key.ToUpper();
            return key switch
            {
                "1" => KeyCode.Alpha1,
                "2" => KeyCode.Alpha2,
                "3" => KeyCode.Alpha3,
                "4" => KeyCode.Alpha4,
                "5" => KeyCode.Alpha5,
                "6" => KeyCode.Alpha6,
                "7" => KeyCode.Alpha7,
                "8" => KeyCode.Alpha8,
                "9" => KeyCode.Alpha9,
                "F1" => KeyCode.F1,
                "F2" => KeyCode.F2,
                "F3" => KeyCode.F3,
                "F4" => KeyCode.F4,
                "F5" => KeyCode.F5,
                "F6" => KeyCode.F6,
                "F7" => KeyCode.F7,
                "F8" => KeyCode.F8,
                "F9" => KeyCode.F9,
                "F10" => KeyCode.F10,
                "F11" => KeyCode.F11,
                "F12" => KeyCode.F12,
                "Q" => KeyCode.Q,
                "W" => KeyCode.W,
                "E" => KeyCode.E,
                "R" => KeyCode.R,
                "T" => KeyCode.T,
                "Y" => KeyCode.Y,
                _ => KeyCode.None
            };
        }
        
        // Revive Commands
        [Command("revive", "rv", description: "Instant self revive (30 min cooldown)", adminOnly: false)]
        public static void SelfRevive(ChatCommandContext ctx)
        {
            var steamId = ctx.User.PlatformId;
            var (success, cooldown) = ExtraSlotsService.Instance.SelfRevive(steamId);
            
            if (success)
            {
                ctx.Reply("You have been revived instantly!");
            }
            else
            {
                ctx.Reply($"Revive on cooldown. Try again in {cooldown} minutes.");
            }
        }
        
        [Command("revive here", "rvh", description: "Revive at current position (30 min cooldown)", adminOnly: false)]
        public static void ReviveHere(ChatCommandContext ctx)
        {
            var steamId = ctx.User.PlatformId;
            var (success, cooldown) = ExtraSlotsService.Instance.SelfReviveAt(steamId, float3.zero);
            
            if (success)
            {
                ctx.Reply("Revived at your position!");
            }
            else
            {
                ctx.Reply($"Revive on cooldown. Try again in {cooldown} minutes.");
            }
        }
    }
}
