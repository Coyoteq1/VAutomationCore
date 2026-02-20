using System;
using System.Collections.Generic;

namespace ExtraSlots.Models
{
    /// <summary>
    /// Item data for extra inventory.
    /// </summary>
    [Serializable]
    public class ItemData
    {
        public string ItemId { get; set; }
        public int Amount { get; set; }
        public int Level { get; set; }
        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
    }
    
    /// <summary>
    /// Extra inventory for a player.
    /// </summary>
    [Serializable]
    public class ExtraInventory
    {
        public ulong SteamId { get; }
        public int MaxSlots { get; }
        public List<ItemData> Items { get; } = new List<ItemData>();
        public List<ItemData> Weapons { get; } = new List<ItemData>(); // Extra weapons (max 3)
        public DateTime LastUpdated { get; set; }
        
        public ExtraInventory(ulong steamId, int maxSlots)
        {
            SteamId = steamId;
            MaxSlots = maxSlots;
            LastUpdated = DateTime.UtcNow;
        }
        
        public int WeaponCount => Weapons.Count;
        
        public bool AddWeapon(ItemData item)
        {
            if (Weapons.Count >= 3) return false; // Max 3 weapons
            Weapons.Add(item);
            LastUpdated = DateTime.UtcNow;
            return true;
        }
        
        public bool RemoveWeapon(string itemId)
        {
            var item = Weapons.Find(x => x.ItemId == itemId);
            if (item == null) return false;
            Weapons.Remove(item);
            LastUpdated = DateTime.UtcNow;
            return true;
        }
        
        public bool AddItem(ItemData item)
        {
            if (Items.Count >= MaxSlots) return false;
            
            Items.Add(item);
            LastUpdated = DateTime.UtcNow;
            return true;
        }
        
        public bool RemoveItem(string itemId)
        {
            var item = Items.Find(x => x.ItemId == itemId);
            if (item == null) return false;
            
            Items.Remove(item);
            LastUpdated = DateTime.UtcNow;
            return true;
        }
        
        public List<ItemData> GetAllItems() => new List<ItemData>(Items);
        public List<ItemData> GetAllWeapons() => new List<ItemData>(Weapons);
        
        public void Clear()
        {
            Items.Clear();
            Weapons.Clear();
            LastUpdated = DateTime.UtcNow;
        }
    }
}
