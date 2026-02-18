using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace VLifecycle.Services.Lifecycle
{
    /// <summary>
    /// ZUI Spell Menu for arena spellbook management.
    /// Provides spell categorization, favorites, and quick access menus.
    /// </summary>
    public static class ZUISpellMenu
    {
        private static bool _isInitialized = false;
        private static Type _zuiApi;
        private static readonly Dictionary<string, MethodInfo> _zuiMethodCache = new();
        private static readonly Dictionary<string, List<SpellEntry>> _spellCategories = new();
        private static readonly List<string> _favoriteSpells = new();

        public struct SpellEntry
        {
            public string Name;
            public string PrefabGuid;
            public string Category;
            public bool IsFavorite;
        }

        /// <summary>
        /// Initialize ZUI spell menu - sets up reflection to ZUI API.
        /// </summary>
        public static void Initialize()
        {
            if (_isInitialized) return;

            try
            {
                _zuiApi = Type.GetType("ZUI.API.ZUI, ZUI", throwOnError: false);
                if (_zuiApi != null)
                {
                    CacheZuiMethods();
                    Plugin.Log.LogInfo("[ZUISpellMenu] ZUI API found - spell menu ready");
                }
                else
                {
                    Plugin.Log.LogWarning("[ZUISpellMenu] ZUI not found - using fallback commands only");
                }

                // Initialize spell categories
                InitializeSpellCategories();
                
                _isInitialized = true;
                Plugin.Log.LogInfo("[ZUISpellMenu] Initialized");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"[ZUISpellMenu] Init error: {ex.Message}");
            }
        }

        /// <summary>
        /// Initialize default spell categories.
        /// </summary>
        private static void InitializeSpellCategories()
        {
            _spellCategories.Clear();

            // Default categories
            _spellCategories["Combat"] = new List<SpellEntry>();
            _spellCategories["Support"] = new List<SpellEntry>();
            _spellCategories["Utility"] = new List<SpellEntry>();
            _spellCategories["Favorites"] = new List<SpellEntry>();
        }

        /// <summary>
        /// Register a spell in the menu system.
        /// </summary>
        public static void RegisterSpell(string name, string prefabGuid, string category, bool isFavorite = false)
        {
            var entry = new SpellEntry
            {
                Name = name,
                PrefabGuid = prefabGuid,
                Category = category,
                IsFavorite = isFavorite
            };

            if (!_spellCategories.ContainsKey(category))
            {
                _spellCategories[category] = new List<SpellEntry>();
            }

            _spellCategories[category].Add(entry);

            if (isFavorite)
            {
                _favoriteSpells.Add(name);
            }
        }

        /// <summary>
        /// Open the spell menu for a player.
        /// </summary>
        public static void OpenSpellMenu(string playerName)
        {
            if (_zuiApi != null)
            {
                Call("SetPlugin", new object[] { "VLifecycle" });
                Call("SetTargetWindow", new object[] { $"SpellMenu_{playerName}" });
                Call("SetUI", new object[] { 600, 500 });
                Call("HideTitleBar", Array.Empty<object>());
                Call("SetTitle", new object[] { "<color=#B30000>Arena Spell Menu</color>" });

                // Add categories as buttons
                Call("AddButton", new object[] { "Combat", ".spell category combat", 15f, 450f, 80f, 30f });
                Call("AddButton", new object[] { "Support", ".spell category support", 105f, 450f, 80f, 30f });
                Call("AddButton", new object[] { "Utility", ".spell category utility", 195f, 450f, 80f, 30f });
                Call("AddButton", new object[] { "Favorites", ".spell category favorites", 285f, 450f, 80f, 30f });

                // Render spells
                RenderSpellCategory("Combat", 410);
                RenderSpellCategory("Support", 370);
                RenderSpellCategory("Utility", 330);
                RenderSpellCategory("Favorites", 290);

                Call("Open", Array.Empty<object>());
            }
            else
            {
                Plugin.Log.LogInfo($"[ZUISpellMenu] Would open spell menu for {playerName} (ZUI not available)");
            }
        }

        /// <summary>
        /// Render a spell category section.
        /// </summary>
        private static void RenderSpellCategory(string category, int yOffset)
        {
            if (!_spellCategories.TryGetValue(category, out var spells) || spells.Count == 0)
            {
                return;
            }

            int currentY = yOffset;
            int xOffset = 15;

            foreach (var spell in spells.Take(7))
            {
                var cmd = $".spell cast \"{spell.Name}\"";
                Call("AddButton", new object[] { spell.Name, cmd, xOffset, currentY, 80f, 25f });
                xOffset += 85;

                if (xOffset > 520)
                {
                    xOffset = 15;
                    currentY -= 30;
                }
            }
        }

        /// <summary>
        /// Cache ZUI API methods once during initialization.
        /// </summary>
        private static void CacheZuiMethods()
        {
            _zuiMethodCache.Clear();
            if (_zuiApi == null)
            {
                return;
            }

            foreach (var method in _zuiApi.GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                var key = BuildMethodKey(method.Name, method.GetParameters().Length);
                if (!_zuiMethodCache.ContainsKey(key))
                {
                    _zuiMethodCache[key] = method;
                }
            }
        }

        /// <summary>
        /// Build a deterministic key for method lookup.
        /// </summary>
        private static string BuildMethodKey(string methodName, int argCount)
        {
            return $"{methodName}:{argCount}";
        }

        /// <summary>
        /// Add a spell to favorites.
        /// </summary>
        public static void ToggleFavorite(string spellName)
        {
            if (_favoriteSpells.Contains(spellName))
            {
                _favoriteSpells.Remove(spellName);
            }
            else
            {
                _favoriteSpells.Add(spellName);
            }
        }

        /// <summary>
        /// Get all registered spells.
        /// </summary>
        public static List<SpellEntry> GetAllSpells()
        {
            return _spellCategories.Values
                .SelectMany(x => x)
                .Distinct()
                .ToList();
        }

        /// <summary>
        /// Get spells by category.
        /// </summary>
        public static List<SpellEntry> GetSpellsByCategory(string category)
        {
            return _spellCategories.TryGetValue(category, out var spells) 
                ? spells 
                : new List<SpellEntry>();
        }

        /// <summary>
        /// Call ZUI API method via reflection.
        /// </summary>
        private static void Call(string methodName, object[] args)
        {
            if (_zuiApi == null) return;

            try
            {
                var key = BuildMethodKey(methodName, args.Length);
                if (_zuiMethodCache.TryGetValue(key, out var method))
                {
                    method.Invoke(null, args);
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning($"[ZUISpellMenu] ZUI call failed: {ex.Message}");
            }
        }
    }
}
