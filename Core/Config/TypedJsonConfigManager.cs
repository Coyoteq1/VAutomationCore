using System;
using System.IO;
using System.Text.Json;

namespace VAutomationCore.Core.Config
{
    /// <summary>
    /// Shared typed JSON config loader/saver with optional validation.
    /// Centralizes file IO, directory creation, and error handling.
    /// </summary>
    public static class TypedJsonConfigManager
    {
        public static bool TryLoadOrCreate<T>(
            string path,
            Func<T> defaultFactory,
            out T config,
            out bool createdDefault,
            JsonSerializerOptions? options = null,
            Func<T, (bool IsValid, string Error)>? validator = null,
            Action<string>? logInfo = null,
            Action<string>? logWarning = null,
            Action<string>? logError = null)
        {
            createdDefault = false;
            config = defaultFactory();

            if (string.IsNullOrWhiteSpace(path))
            {
                logError?.Invoke("Config path is empty.");
                createdDefault = true;
                return false;
            }

            EnsureDirectory(path, logError);

            if (!File.Exists(path))
            {
                createdDefault = true;
                config = defaultFactory();
                var saved = TrySave(path, config, options, logInfo, logError);
                if (!saved)
                {
                    logWarning?.Invoke($"Failed creating default config at '{path}'.");
                }
                else
                {
                    logInfo?.Invoke($"Created default config at '{path}'.");
                }

                return saved;
            }

            try
            {
                var json = File.ReadAllText(path);
                var loaded = JsonSerializer.Deserialize<T>(json, options);
                if (loaded == null)
                {
                    createdDefault = true;
                    config = defaultFactory();
                    logWarning?.Invoke($"Deserialized null config at '{path}'. Reverting to defaults.");
                    TrySave(path, config, options, logInfo, logError);
                    return false;
                }

                if (validator != null)
                {
                    var validation = validator(loaded);
                    if (!validation.IsValid)
                    {
                        createdDefault = true;
                        config = defaultFactory();
                        logWarning?.Invoke($"Config validation failed for '{path}': {validation.Error}. Reverting to defaults.");
                        TrySave(path, config, options, logInfo, logError);
                        return false;
                    }
                }

                config = loaded;
                return true;
            }
            catch (Exception ex)
            {
                createdDefault = true;
                config = defaultFactory();
                logError?.Invoke($"Failed to load config '{path}': {ex.Message}. Reverting to defaults.");
                TrySave(path, config, options, logInfo, logError);
                return false;
            }
        }

        public static bool TrySave<T>(
            string path,
            T config,
            JsonSerializerOptions? options = null,
            Action<string>? logInfo = null,
            Action<string>? logError = null)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                logError?.Invoke("Config path is empty.");
                return false;
            }

            try
            {
                EnsureDirectory(path, logError);
                var json = JsonSerializer.Serialize(config, options);
                var tempPath = $"{path}.tmp";
                File.WriteAllText(tempPath, json);
                File.Move(tempPath, path, true);
                logInfo?.Invoke($"Saved config '{path}'.");
                return true;
            }
            catch (Exception ex)
            {
                logError?.Invoke($"Failed to save config '{path}': {ex.Message}");
                return false;
            }
        }

        private static void EnsureDirectory(string path, Action<string>? logError)
        {
            try
            {
                var dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrWhiteSpace(dir))
                {
                    Directory.CreateDirectory(dir);
                }
            }
            catch (Exception ex)
            {
                logError?.Invoke($"Failed to create config directory for '{path}': {ex.Message}");
            }
        }
    }
}
