using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text.Json;
using Unity.Mathematics;
using VAutomationCore.Core.Logging;
using ProjectM;

namespace VAutomationCore.Core.Config
{
    /// <summary>
    /// Centralized configuration service with concurrent caching and lazy loading.
    /// Provides safe JSON serialization/deserialization for mod configurations.
    /// </summary>
    public static class ConfigService
    {
        private static readonly ConcurrentDictionary<string, object> _configs = new();
        private static readonly string _configPath;
        private static readonly CoreLogger _log = new("ConfigService");
        private static readonly JsonSerializerOptions _jsonOptions = CreateJsonOptions();

        static ConfigService()
        {
            _configPath = Path.Combine(BepInEx.Paths.ConfigPath, "VAuto");

            if (!Directory.Exists(_configPath))
            {
                Directory.CreateDirectory(_configPath);
            }
        }

        /// <summary>
        /// Gets the root folder where VAutomationCore configuration files are stored.
        /// </summary>
        public static string ConfigRootPath => _configPath;

        /// <summary>
        /// Gets a configuration object of type T, loading from disk if not cached.
        /// </summary>
        /// <typeparam name="T">The configuration type (must have parameterless constructor).</typeparam>
        /// <param name="fileName">Optional custom filename. Defaults to type name + ".json".</param>
        /// <returns>The loaded or default configuration.</returns>
        public static T GetConfig<T>(string? fileName = null) where T : new()
        {
            var configFile = ResolveConfigFileName<T>(fileName);
            var cacheKey = GetCacheKey<T>(configFile);
            var fullPath = Path.Combine(_configPath, configFile);

            return (T)_configs.GetOrAdd(cacheKey, _ =>
            {
                if (!File.Exists(fullPath))
                {
                    var defaultConfig = new T();
                    TrySaveConfig(defaultConfig, configFile);
                    _log.Info($"Created default config: {configFile}");
                    return defaultConfig;
                }

                try
                {
                    var json = File.ReadAllText(fullPath);
                    var result = JsonSerializer.Deserialize<T>(json, _jsonOptions);

                    if (result == null)
                    {
                        _log.Warning($"Failed to deserialize {configFile}, using defaults");
                        return new T();
                    }

                    _log.Debug($"Loaded config: {configFile}");
                    return result;
                }
                catch (Exception ex)
                {
                    _log.Exception(ex, $"Error loading {configFile}, using defaults");
                    return new T();
                }
            });
        }

        /// <summary>
        /// Attempts to get a configuration object without throwing exceptions to callers.
        /// </summary>
        public static bool TryGetConfig<T>(out T config, string? fileName = null) where T : new()
        {
            try
            {
                config = GetConfig<T>(fileName);
                return true;
            }
            catch (Exception ex)
            {
                _log.Exception(ex, $"TryGetConfig failed for {ResolveConfigFileName<T>(fileName)}");
                config = new T();
                return false;
            }
        }

        /// <summary>
        /// Saves a configuration object to disk and updates the cache.
        /// </summary>
        /// <typeparam name="T">The configuration type.</typeparam>
        /// <param name="config">The configuration to save.</param>
        /// <param name="fileName">Optional custom filename.</param>
        public static void SaveConfig<T>(T config, string? fileName = null)
        {
            TrySaveConfig(config, fileName);
        }

        /// <summary>
        /// Attempts to save a configuration object and returns success/failure.
        /// </summary>
        public static bool TrySaveConfig<T>(T config, string? fileName = null)
        {
            var configFile = ResolveConfigFileName<T>(fileName);
            var fullPath = Path.Combine(_configPath, configFile);
            var cacheKey = GetCacheKey<T>(configFile);

            try
            {
                var json = JsonSerializer.Serialize(config, _jsonOptions);
                File.WriteAllText(fullPath, json);
                _configs[cacheKey] = config!;
                _log.Info($"Saved config: {configFile}");
                return true;
            }
            catch (Exception ex)
            {
                _log.Exception(ex, $"Error saving {configFile}");
                return false;
            }
        }

        /// <summary>
        /// Reloads a configuration from disk, bypassing cache.
        /// </summary>
        /// <typeparam name="T">The configuration type.</typeparam>
        /// <param name="fileName">Optional custom filename.</param>
        /// <returns>The freshly loaded configuration.</returns>
        public static T ReloadConfig<T>(string? fileName = null) where T : new()
        {
            var configFile = ResolveConfigFileName<T>(fileName);
            var cacheKey = GetCacheKey<T>(configFile);
            _configs.TryRemove(cacheKey, out _);

            return GetConfig<T>(configFile);
        }

        /// <summary>
        /// Gets the full path to a specific config file for type T.
        /// </summary>
        public static string GetConfigFullPath<T>(string? fileName = null)
        {
            var configFile = ResolveConfigFileName<T>(fileName);
            return Path.Combine(_configPath, configFile);
        }

        /// <summary>
        /// Checks if a configuration file exists.
        /// </summary>
        /// <typeparam name="T">The configuration type.</typeparam>
        /// <param name="fileName">Optional custom filename.</param>
        /// <returns>True if the file exists.</returns>
        public static bool ConfigExists<T>(string? fileName = null)
        {
            var fullPath = GetConfigFullPath<T>(fileName);
            return File.Exists(fullPath);
        }

        /// <summary>
        /// Clears all cached configurations.
        /// </summary>
        public static void ClearCache()
        {
            _configs.Clear();
            _log.Info("Config cache cleared");
        }

        private static string ResolveConfigFileName<T>(string? fileName)
        {
            return string.IsNullOrWhiteSpace(fileName) ? $"{typeof(T).Name}.json" : fileName;
        }

        private static string GetCacheKey<T>(string configFile)
        {
            return $"{typeof(T).FullName}:{configFile.Trim().ToLowerInvariant()}";
        }

        private static JsonSerializerOptions CreateJsonOptions()
        {
            return new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters =
                {
                    new Float3JsonConverter(),
                    new Float2JsonConverter(),
                    new Int2JsonConverter(),
                    new PrefabGuidJsonConverter(),
                    new QuaternionJsonConverter()
                }
            };
        }
    }
}
