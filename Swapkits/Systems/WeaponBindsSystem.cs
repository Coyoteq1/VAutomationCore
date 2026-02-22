using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.Json;
using BepInEx;
using BepInEx.Logging;
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
        private static readonly ManualLogSource Log = BepInEx.Logging.Logger.CreateLogSource("ExtraSlots.Binds");
        private static readonly string BindsPath = Path.Combine(Paths.ConfigPath, "VAuto.Swapkits.binds.json");
        
        private readonly Dictionary<ulong, Dictionary<int, string>> _keybinds = new Dictionary<ulong, Dictionary<int, string>>();
        private readonly Dictionary<string, KeyCode> _keyAliases = new Dictionary<string, KeyCode>(StringComparer.OrdinalIgnoreCase);
        private bool _restrictToAliases;
        private bool _bindsLoaded;

        private WeaponBindsSystem()
        {
            Configure(null, false);
        }

        /// <summary>
        /// Configure key aliases and restrictions from plugin config.
        /// </summary>
        public void Configure(string keyAliasesCsv, bool restrictToAliases)
        {
            _restrictToAliases = restrictToAliases;
            _keyAliases.Clear();

            RegisterDefaultAliases();
            RegisterCustomAliases(keyAliasesCsv);

            // Ensure persisted player binds are loaded once during startup.
            if (!_bindsLoaded)
            {
                LoadBindings();
            }
        }

        /// <summary>
        /// Try to parse a key from alias or Unity KeyCode name.
        /// </summary>
        public bool TryParseKey(string rawKey, out KeyCode keyCode, out string error)
        {
            keyCode = KeyCode.None;
            error = string.Empty;

            if (string.IsNullOrWhiteSpace(rawKey))
            {
                error = "Key is required.";
                return false;
            }

            var token = rawKey.Trim();
            if (_keyAliases.TryGetValue(token, out keyCode))
            {
                return true;
            }

            if (Enum.TryParse(token, true, out keyCode) && keyCode != KeyCode.None)
            {
                if (_restrictToAliases && !_keyAliases.Values.Contains(keyCode))
                {
                    error = $"Key '{rawKey}' is not allowed by server alias policy.";
                    return false;
                }

                return true;
            }

            error = $"Invalid key '{rawKey}'. Use '.extra keys' to see allowed aliases.";
            return false;
        }

        /// <summary>
        /// Get configured key aliases.
        /// </summary>
        public IReadOnlyDictionary<string, KeyCode> GetKeyAliases()
        {
            return new Dictionary<string, KeyCode>(_keyAliases, StringComparer.OrdinalIgnoreCase);
        }

        public bool IsRestrictedToAliases => _restrictToAliases;
        
        /// <summary>
        /// Bind weapon to key.
        /// </summary>
        public void BindKey(ulong steamId, KeyCode key, string weaponId)
        {
            if (!_keybinds.ContainsKey(steamId))
                _keybinds[steamId] = new Dictionary<int, string>();
            
            _keybinds[steamId][(int)key] = weaponId;
            SaveBindings();
        }
        
        /// <summary>
        /// Unbind key.
        /// </summary>
        public void UnbindKey(ulong steamId, KeyCode key)
        {
            if (_keybinds.ContainsKey(steamId))
            {
                _keybinds[steamId].Remove((int)key);
                SaveBindings();
            }
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
            if (_keybinds.Remove(steamId))
            {
                SaveBindings();
            }
        }

        /// <summary>
        /// Load persisted bind mappings from disk.
        /// </summary>
        public void LoadBindings()
        {
            _bindsLoaded = true;

            try
            {
                if (!File.Exists(BindsPath))
                {
                    return;
                }

                var json = File.ReadAllText(BindsPath);
                var data = JsonSerializer.Deserialize<PersistedBindsStore>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true
                });

                _keybinds.Clear();
                if (data?.Players == null)
                {
                    return;
                }

                foreach (var playerEntry in data.Players)
                {
                    if (!ulong.TryParse(playerEntry.Key, out var steamId))
                    {
                        continue;
                    }

                    var playerBinds = new Dictionary<int, string>();
                    if (playerEntry.Value != null)
                    {
                        foreach (var bindEntry in playerEntry.Value)
                        {
                            if (!int.TryParse(bindEntry.Key, out var keyCodeValue))
                            {
                                continue;
                            }

                            if (string.IsNullOrWhiteSpace(bindEntry.Value))
                            {
                                continue;
                            }

                            playerBinds[keyCodeValue] = bindEntry.Value.Trim();
                        }
                    }

                    if (playerBinds.Count > 0)
                    {
                        _keybinds[steamId] = playerBinds;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogWarning($"Failed to load binds from '{BindsPath}': {ex.Message}");
                _keybinds.Clear();
            }
        }

        /// <summary>
        /// Save bind mappings to disk.
        /// </summary>
        public void SaveBindings()
        {
            try
            {
                var data = new PersistedBindsStore();
                foreach (var player in _keybinds)
                {
                    var binds = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    foreach (var bind in player.Value)
                    {
                        if (string.IsNullOrWhiteSpace(bind.Value))
                        {
                            continue;
                        }

                        binds[bind.Key.ToString()] = bind.Value;
                    }

                    if (binds.Count > 0)
                    {
                        data.Players[player.Key.ToString()] = binds;
                    }
                }

                var directory = Path.GetDirectoryName(BindsPath);
                if (!string.IsNullOrWhiteSpace(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(BindsPath, json);
            }
            catch (Exception ex)
            {
                Log.LogWarning($"Failed to save binds to '{BindsPath}': {ex.Message}");
            }
        }

        private void RegisterDefaultAliases()
        {
            // Keep existing user-facing shortcuts as defaults.
            RegisterAlias("1", KeyCode.Alpha1);
            RegisterAlias("2", KeyCode.Alpha2);
            RegisterAlias("3", KeyCode.Alpha3);
            RegisterAlias("4", KeyCode.Alpha4);
            RegisterAlias("5", KeyCode.Alpha5);
            RegisterAlias("6", KeyCode.Alpha6);
            RegisterAlias("7", KeyCode.Alpha7);
            RegisterAlias("8", KeyCode.Alpha8);
            RegisterAlias("9", KeyCode.Alpha9);
            RegisterAlias("0", KeyCode.Alpha0);

            RegisterAlias("F1", KeyCode.F1);
            RegisterAlias("F2", KeyCode.F2);
            RegisterAlias("F3", KeyCode.F3);
            RegisterAlias("F4", KeyCode.F4);
            RegisterAlias("F5", KeyCode.F5);
            RegisterAlias("F6", KeyCode.F6);
            RegisterAlias("F7", KeyCode.F7);
            RegisterAlias("F8", KeyCode.F8);
            RegisterAlias("F9", KeyCode.F9);
            RegisterAlias("F10", KeyCode.F10);
            RegisterAlias("F11", KeyCode.F11);
            RegisterAlias("F12", KeyCode.F12);

            RegisterAlias("Q", KeyCode.Q);
            RegisterAlias("W", KeyCode.W);
            RegisterAlias("E", KeyCode.E);
            RegisterAlias("R", KeyCode.R);
            RegisterAlias("T", KeyCode.T);
            RegisterAlias("Y", KeyCode.Y);
        }

        private void RegisterCustomAliases(string keyAliasesCsv)
        {
            if (string.IsNullOrWhiteSpace(keyAliasesCsv))
            {
                return;
            }

            var entries = keyAliasesCsv.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var entry in entries)
            {
                var pair = entry.Split('=', 2, StringSplitOptions.RemoveEmptyEntries);
                if (pair.Length != 2)
                {
                    continue;
                }

                var alias = pair[0].Trim();
                var rawKeyCode = pair[1].Trim();
                if (string.IsNullOrWhiteSpace(alias) || string.IsNullOrWhiteSpace(rawKeyCode))
                {
                    continue;
                }

                if (!Enum.TryParse(rawKeyCode, true, out KeyCode parsed) || parsed == KeyCode.None)
                {
                    continue;
                }

                RegisterAlias(alias, parsed);
            }
        }

        private void RegisterAlias(string alias, KeyCode keyCode)
        {
            if (string.IsNullOrWhiteSpace(alias) || keyCode == KeyCode.None)
            {
                return;
            }

            _keyAliases[alias.Trim()] = keyCode;
        }

        private sealed class PersistedBindsStore
        {
            public Dictionary<string, Dictionary<string, string>> Players { get; set; } = new(StringComparer.OrdinalIgnoreCase);
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
