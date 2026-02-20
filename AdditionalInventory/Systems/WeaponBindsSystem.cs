using System;
using System.Collections.Generic;
using UnityEngine;
using ExtraSlots.Models;

namespace ExtraSlots.Systems
{
    /// <summary>
    /// Keybind system for extra weapons.
    /// </summary>
    public class WeaponBindsSystem
    {
        private static WeaponBindsSystem _instance;
        public static WeaponBindsSystem Instance => _instance ??= new WeaponBindsSystem();
        
        private readonly Dictionary<ulong, Dictionary<int, string>> _keybinds = new Dictionary<ulong, Dictionary<int, string>>();
        
        /// <summary>
        /// Bind weapon to key.
        /// </summary>
        public void BindKey(ulong steamId, KeyCode key, string weaponId)
        {
            if (!_keybinds.ContainsKey(steamId))
                _keybinds[steamId] = new Dictionary<int, string>();
            
            _keybinds[steamId][(int)key] = weaponId;
        }
        
        /// <summary>
        /// Unbind key.
        /// </summary>
        public void UnbindKey(ulong steamId, KeyCode key)
        {
            if (_keybinds.ContainsKey(steamId))
                _keybinds[steamId].Remove((int)key);
        }
        
        /// <summary>
        /// Get weapon for key.
        /// </summary>
        public string GetWeaponForKey(ulong steamId, KeyCode key)
        {
            if (_keybinds.TryGetValue(steamId, out var binds))
            {
                if (binds.TryGetValue((int)key, out var weaponId))
                    return weaponId;
            }
            return null;
        }
        
        /// <summary>
        /// Check if key is bound.
        /// </summary>
        public bool IsKeyBound(ulong steamId, KeyCode key)
        {
            return GetWeaponForKey(steamId, key) != null;
        }
        
        /// <summary>
        /// Get all binds for player.
        /// </summary>
        public Dictionary<int, string> GetBinds(ulong steamId)
        {
            return _keybinds.TryGetValue(steamId, out var binds) ? binds : new Dictionary<int, string>();
        }
        
        /// <summary>
        /// Clear all binds.
        /// </summary>
        public void ClearBinds(ulong steamId)
        {
            _keybinds.Remove(steamId);
        }
    }
    
    /// <summary>
    /// Quick access for keybinds.
    /// </summary>
    public static class Binds
    {
        private static WeaponBindsSystem _sys => WeaponBindsSystem.Instance;
        
        public static void Bind(ulong steamId, KeyCode key, string weaponId) 
            => _sys.BindKey(steamId, key, weaponId);
        
        public static void Unbind(ulong steamId, KeyCode key) 
            => _sys.UnbindKey(steamId, key);
        
        public static string GetWeapon(ulong steamId, KeyCode key) 
            => _sys.GetWeaponForKey(steamId, key);
        
        public static bool IsBound(ulong steamId, KeyCode key) 
            => _sys.IsKeyBound(steamId, key);
    }
}
