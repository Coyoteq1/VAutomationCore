using System;
using System.IO;
using BepInEx;
using VAutomationCore.Core.Config;

namespace VAutomationCore.Core.Gameplay
{
    public static class GameplayJsonConfigService
    {
        public static string GetGameplayConfigDirectory(GameplayType gameplayType)
        {
            return Path.Combine(Paths.ConfigPath, "VAutomationCore", "gameplay", gameplayType.ToString().ToLowerInvariant());
        }

        public static string GetConfigPath(GameplayType gameplayType, string fileName)
        {
            return Path.Combine(GetGameplayConfigDirectory(gameplayType), fileName);
        }

        public static T LoadOrCreate<T>(GameplayType gameplayType, string fileName, Func<T> defaultFactory)
        {
            var path = GetConfigPath(gameplayType, fileName);
            TypedJsonConfigManager.TryLoadOrCreate(
                path,
                defaultFactory,
                out T config,
                out _,
                options: null,
                validator: null,
                logInfo: null,
                logWarning: null,
                logError: null);

            return config;
        }
    }
}
