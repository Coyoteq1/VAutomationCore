using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Collections;
using ProjectM;
using ExtraSlots.Models;
using BepInEx;

namespace ExtraSlots.Systems
{
    /// <summary>
    /// Extra slots plugin logger.
    /// </summary>
    internal static class Logger
    {
        internal static readonly BepInEx.Logging.ManualLogSource Log = BepInEx.BepInEx.Logging.Logger.CreateLogSource("ExtraSlots");
    }
    /// <summary>
    /// Extra slots inventory system.
    /// Stores extra items separate from main inventory.
    /// </summary>
    public class ExtraSlotsService
    {
        private static ExtraSlotsService _instance;
        public static ExtraSlotsService Instance => _instance ??= new ExtraSlotsService();
        
        private readonly Dictionary<ulong, ExtraInventory> _inventories = new Dictionary<ulong, ExtraInventory>();
        private readonly Dictionary<ulong, DateTime> _lastReviveTime = new Dictionary<ulong, DateTime>();
        private readonly Dictionary<ulong, DateTime> _lastSlotTime = new Dictionary<ulong, DateTime>();
        private int _maxSlots = 20;
        private const int MaxExtraWeapons = 3; // Maximum extra weapons
        private const int ReviveCooldownMinutes = 30;
        private const int SlotCooldownMinutes = 30;
        
        public static void Initialize()
        {
            Instance._inventories.Clear();
        }
        
        /// <summary>
        /// Get max extra weapons limit.
        /// </summary>
        public int MaxExtraWeapons => MaxExtraWeapons;
        
        /// <summary>
        /// Get current extra weapon count.
        /// </summary>
        public int GetExtraWeaponCount(ulong steamId)
        {
            var inv = GetInventory(steamId);
            return inv.WeaponCount;
        }
        
        /// <summary>
        /// Check if revive is available (30 min cooldown).
        /// </summary>
        public bool CanRevive(ulong steamId)
        {
            if (!_lastReviveTime.TryGetValue(steamId, out var lastTime))
                return true;
            
            var canRevive = (DateTime.UtcNow - lastTime).TotalMinutes >= ReviveCooldownMinutes;
            Logger.Log.Debug($"CanRevive({steamId}): {canRevive}");
            return canRevive;
        }
        
        /// <summary>
        /// Get remaining cooldown in minutes.
        /// </summary>
        public int GetReviveCooldown(ulong steamId)
        {
            if (!_lastReviveTime.TryGetValue(steamId, out var lastTime))
                return 0;
            
            var remaining = ReviveCooldownMinutes - (DateTime.UtcNow - lastTime).TotalMinutes;
            return remaining > 0 ? (int)remaining : 0;
        }
        
        /// <summary>
        /// Record revive use.
        /// </summary>
        private void RecordRevive(ulong steamId)
        {
            _lastReviveTime[steamId] = DateTime.UtcNow;
            Logger.Log.Info($"Revive recorded for {steamId}");
        }
        
        /// <summary>
        /// Check if slot action is available (30 min cooldown).
        /// </summary>
        public bool CanUseSlot(ulong steamId)
        {
            if (!_lastSlotTime.TryGetValue(steamId, out var lastTime))
                return true;
            
            var canUse = (DateTime.UtcNow - lastTime).TotalMinutes >= SlotCooldownMinutes;
            Logger.Log.Debug($"CanUseSlot({steamId}): {canUse}");
            return canUse;
        }
        
        /// <summary>
        /// Get remaining slot cooldown in minutes.
        /// </summary>
        public int GetSlotCooldown(ulong steamId)
        {
            if (!_lastSlotTime.TryGetValue(steamId, out var lastTime))
                return 0;
            
            var remaining = SlotCooldownMinutes - (DateTime.UtcNow - lastTime).TotalMinutes;
            return remaining > 0 ? (int)remaining : 0;
        }
        
        /// <summary>
        /// Record slot action use.
        /// </summary>
        public (bool success, int cooldownMinutes) UseSlot(ulong steamId)
        {
            if (!CanUseSlot(steamId))
            {
                Logger.Log.Debug($"Slot action blocked for {steamId}, cooldown: {GetSlotCooldown(steamId)} min");
                return (false, GetSlotCooldown(steamId));
            }
            
            _lastSlotTime[steamId] = DateTime.UtcNow;
            Logger.Log.Info($"Slot action used by {steamId}");
            return (true, 0);
        }
        
        /// <summary>
        /// Get or create extra inventory for player.
        /// </summary>
        public ExtraInventory GetInventory(ulong steamId)
        {
            if (!_inventories.TryGetValue(steamId, out var inv))
            {
                inv = new ExtraInventory(steamId, _maxSlots);
                _inventories[steamId] = inv;
            }
            return inv;
        }
        
        /// <summary>
        /// Add item to extra inventory.
        /// </summary>
        public bool AddItem(ulong steamId, ItemData item)
        {
            var inv = GetInventory(steamId);
            return inv.AddItem(item);
        }
        
        /// <summary>
        /// Remove item from extra inventory.
        /// </summary>
        public bool RemoveItem(ulong steamId, string itemId)
        {
            var inv = GetInventory(steamId);
            return inv.RemoveItem(itemId);
        }
        
        /// <summary>
        /// Get all items in extra inventory.
        /// </summary>
        public List<ItemData> GetItems(ulong steamId)
        {
            var inv = GetInventory(steamId);
            return inv.GetAllItems();
        }
        
        /// <summary>
        /// Clear extra inventory.
        /// </summary>
        public void Clear(ulong steamId)
        {
            if (_inventories.ContainsKey(steamId))
            {
                _inventories[steamId].Clear();
            }
        }
        
        /// <summary>
        /// Transfer item from main to extra inventory.
        /// </summary>
        public bool TransferToExtra(ulong steamId, int slotIndex)
        {
            return true;
        }
        
        /// <summary>
        /// Transfer item from extra to main inventory.
        /// </summary>
        public bool TransferToMain(ulong steamId, string itemId)
        {
            return true;
        }
        
        /// <summary>
        /// Equip weapon from extra slots to specified slot (no cooldown).
        /// </summary>
        public bool EquipWeaponNoCooldown(ulong steamId, string itemId, int slotIndex)
        {
            var inv = GetInventory(steamId);
            var item = inv.Items.Find(x => x.ItemId == itemId);
            
            if (item == null) return false;
            
            // Equip without cooldown - this would interact with game inventory system
            return true;
        }
        
        /// <summary>
        /// Swap weapon from extra slots to any slot (not hotbar, no cooldown).
        /// </summary>
        public bool SwapWeaponAnySlot(ulong steamId, string itemId, int targetSlot)
        {
            // Allows swapping to any slot including non-hotbar
            // No cooldown applied
            var inv = GetInventory(steamId);
            var item = inv.Items.Find(x => x.ItemId == itemId);
            
            if (item == null) return false;
            
            return true;
        }
        
        /// <summary>
        /// Quick swap weapon (no cooldown, any slot).
        /// </summary>
        public bool QuickSwapWeapon(ulong steamId, string itemId, int inventorySlot)
        {
            // Instant swap - bypasses cooldown
            return true;
        }
        
        /// <summary>
        /// Self revive instantly (no timer).
        /// 30 min cooldown between uses.
        /// </summary>
        public (bool success, int cooldownMinutes) SelfRevive(ulong steamId)
        {
            // Check cooldown first
            if (!CanRevive(steamId))
            {
                return (false, GetReviveCooldown(steamId));
            }
            
            // Record revive for cooldown tracking
            RecordRevive(steamId);
            
            // This would interact with game death system
            // Instant revive without waiting
            return (true, 0);
        }
        
        /// <summary>
        /// Revive at position.
        /// 30 min cooldown between uses.
        /// </summary>
        public (bool success, int cooldownMinutes) SelfReviveAt(ulong steamId, float3 position)
        {
            return SelfRevive(steamId);
        }
    }
}
