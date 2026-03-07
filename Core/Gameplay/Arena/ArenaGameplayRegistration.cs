using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Unity.Mathematics;
using VAutomationCore.Core.Flows;

namespace VAutomationCore.Core.Gameplay.Arena
{
    public static class ArenaGameplayRegistration
    {
        private const string GameplayId = "arena";

        public static void Register()
        {
            var settings = GameplayJsonConfigService.LoadOrCreate(GameplayType.Arena, "arena.settings.json", () => new ArenaSettingsConfig());
            var zones = GameplayJsonConfigService.LoadOrCreate(GameplayType.Arena, "arena.zones.json", () => new[] { new ArenaZoneConfig() });
            GameplayJsonConfigService.LoadOrCreate(GameplayType.Arena, "arena.rules.json", () => new[] { new ArenaRuleProfileConfig() });
            GameplayJsonConfigService.LoadOrCreate(GameplayType.Arena, "arena.prefabs.json", () => new ArenaPrefabConfig());

            var flowDefinitions = LoadFlowDefinitionsFromConfig();
            GameplayFlowRegistry.RegisterFlows(flowDefinitions, replace: true);

            ArenaFlows.RegisterArenaFlows();

            var zoneDefinitions = zones.Select(ToZoneDefinition).ToArray();
            foreach (var zone in zoneDefinitions)
            {
                ZoneRegistry.Register(zone, replace: true);
            }

            GameplayRegistry.Register(
                new GameplayDefinition
                {
                    Id = GameplayId,
                    GameplayType = GameplayType.Arena,
                    DisplayName = "Arena",
                    Enabled = settings.Enabled,
                    ConfigDirectory = GameplayJsonConfigService.GetGameplayConfigDirectory(GameplayType.Arena),
                    ZoneTypes = zoneDefinitions.Select(zone => zone.ZoneType).Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
                    FlowNames = flowDefinitions.Select(flow => flow.Name).ToArray()
                },
                replace: true);
        }

        private static IReadOnlyList<GameplayFlowDefinition> LoadFlowDefinitionsFromConfig()
        {
            var configPath = GameplayJsonConfigService.GetConfigPath(GameplayType.Arena, "arena_flows.config.json");

            if (!File.Exists(configPath))
            {
                configPath = Path.Combine(AppContext.BaseDirectory, "config", "gameplay", "arena_flows.config.json");
            }

            if (!File.Exists(configPath))
            {
                return Array.Empty<GameplayFlowDefinition>();
            }

            try
            {
                var json = File.ReadAllText(configPath);
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (!root.TryGetProperty("flows", out var flowsElement))
                {
                    return Array.Empty<GameplayFlowDefinition>();
                }

                var definitions = new List<GameplayFlowDefinition>();

                foreach (var flowProperty in flowsElement.EnumerateObject())
                {
                    var flowName = flowProperty.Name;
                    var flowConfig = flowProperty.Value;

                    var definition = new GameplayFlowDefinition
                    {
                        Name = flowName,
                        GameplayType = GameplayType.Arena,
                        Description = flowConfig.GetPropertyOrDefault("description", string.Empty),
                        AdminOnly = flowConfig.GetPropertyOrDefault("adminOnly", false),
                        SupportedZoneTypes = flowConfig.GetPropertyOrDefault("supportedZoneTypes", new[] { "arena" }),
                        Tags = flowConfig.GetPropertyOrDefault("tags", new[] { "arena" })
                    };

                    definitions.Add(definition);
                }

                return definitions;
            }
            catch (Exception)
            {
                return Array.Empty<GameplayFlowDefinition>();
            }
        }

        private static GameplayZoneDefinition ToZoneDefinition(ArenaZoneConfig config)
        {
            var center = config.Center ?? Array.Empty<float>();
            return new GameplayZoneDefinition
            {
                ZoneId = config.ZoneId,
                GameplayType = GameplayType.Arena,
                ZoneType = string.IsNullOrWhiteSpace(config.ZoneType) ? "arena" : config.ZoneType,
                ZoneShape = string.IsNullOrWhiteSpace(config.ZoneShape) ? "Sphere" : config.ZoneShape,
                Center = new float3(
                    center.Length > 0 ? center[0] : 0f,
                    center.Length > 1 ? center[1] : 0f,
                    center.Length > 2 ? center[2] : 0f),
                Radius = config.Radius,
                Enabled = config.Enabled,
                RuleProfileId = config.RuleProfileId ?? string.Empty,
                EntryFlows = config.EntryFlows?.ToArray() ?? Array.Empty<string>(),
                ExitFlows = config.ExitFlows?.ToArray() ?? Array.Empty<string>(),
                TickFlows = config.TickFlows?.ToArray() ?? Array.Empty<string>()
            };
        }
    }

    internal static class JsonElementExtensions
    {
        public static T GetPropertyOrDefault<T>(this JsonElement element, string propertyName, T defaultValue)
        {
            if (element.TryGetProperty(propertyName, out var property))
            {
                if (typeof(T) == typeof(bool))
                {
                    return (T)(object)property.GetBoolean();
                }
                if (typeof(T) == typeof(string))
                {
                    return (T)(object)(property.GetString() ?? string.Empty);
                }
                if (typeof(T) == typeof(string[]))
                {
                    if (property.ValueKind == JsonValueKind.Array)
                    {
                        var array = property.EnumerateArray().Select(x => x.GetString() ?? string.Empty).ToArray();
                        return (T)(object)array;
                    }
                }
            }
            return defaultValue;
        }
    }
}
