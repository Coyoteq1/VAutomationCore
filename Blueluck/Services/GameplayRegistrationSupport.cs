using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Blueluck.Models;
using BepInEx.Logging;

namespace Blueluck.Services
{
    internal static class GameplayRegistrationSupport
    {
        private static readonly ManualLogSource Log = Logger.CreateLogSource("Blueluck.GameplayRegistration");

        public static GameplayRegistrationResult RegisterArena(string configDirectory)
        {
            return RegisterFamily(
                configDirectory,
                GameplayFamilies.Arena,
                GameplayZoneTypes.Arena,
                "arena.settings.json",
                "arena.zones.json",
                "arena.rules.json",
                "arena_flows.config.json",
                "arena_presets.config.json");
        }

        public static GameplayRegistrationResult RegisterBoss(string configDirectory)
        {
            return RegisterFamily(
                configDirectory,
                GameplayFamilies.Boss,
                GameplayZoneTypes.Boss,
                "boss.settings.json",
                "boss.zones.json",
                "boss.rules.json",
                "boss_flows.config.json",
                "boss_presets.config.json");
        }

        private static GameplayRegistrationResult RegisterFamily(
            string configDirectory,
            string gameplayType,
            string zoneType,
            string settingsFile,
            string zonesFile,
            string rulesFile,
            string flowsFile,
            string presetsFile)
        {
            var settings = LoadJson(Path.Combine(configDirectory, settingsFile), new GameplaySettingsConfig());
            var rules = LoadJson(Path.Combine(configDirectory, rulesFile), new GameplayRulesConfig());
            var flows = LoadJson(Path.Combine(configDirectory, flowsFile), new GameplayFlowsConfig());
            var presets = LoadJson(Path.Combine(configDirectory, presetsFile), new GameplayPresetsConfig());
            var diagnostics = new GameplayRegistrationDiagnostics { GameplayType = gameplayType };

            ZoneDefinition[] zones = gameplayType == GameplayFamilies.Arena
                ? LoadJson(Path.Combine(configDirectory, zonesFile), new ArenaZonesConfig()).Zones
                : LoadJson(Path.Combine(configDirectory, zonesFile), new BossZonesConfig()).Zones;

            var flowMap = flows.Flows
                .Where(x => !string.IsNullOrWhiteSpace(x.FlowId))
                .GroupBy(x => x.FlowId, StringComparer.OrdinalIgnoreCase)
                .Select(x => x.First())
                .ToDictionary(x => x.FlowId, x => NormalizeFlow(x), StringComparer.OrdinalIgnoreCase);
            var presetMap = presets.Presets
                .Where(x => !string.IsNullOrWhiteSpace(x.PresetId))
                .GroupBy(x => x.PresetId, StringComparer.OrdinalIgnoreCase)
                .Select(x => x.First())
                .ToDictionary(x => x.PresetId, x => NormalizePreset(x), StringComparer.OrdinalIgnoreCase);

            var resolvedZones = new List<ZoneDefinition>();
            foreach (var zone in zones ?? Array.Empty<ZoneDefinition>())
            {
                if (zone == null)
                {
                    continue;
                }

                zone.GameplayType = gameplayType;

                if (!string.Equals(zone.NormalizedZoneType, zoneType, StringComparison.OrdinalIgnoreCase))
                {
                    diagnostics.InvalidZones.Add($"{zone.Name}: zone type '{zone.Type}' is incompatible with gameplay '{gameplayType}'.");
                    continue;
                }

                ResolveZone(zone, settings, presetMap, flowMap, diagnostics);
                resolvedZones.Add(zone);
            }

            diagnostics.RegisteredZoneCount = resolvedZones.Count;
            diagnostics.RegisteredFlowCount = flowMap.Count;
            diagnostics.RegisteredPresetCount = presetMap.Count;

            return new GameplayRegistrationResult
            {
                GameplayType = gameplayType,
                Zones = resolvedZones.ToArray(),
                Flows = flowMap.Values.ToArray(),
                Presets = presetMap.Values.ToArray(),
                Rules = rules.Rules ?? Array.Empty<GameplayRuleProfileConfig>(),
                Settings = settings,
                Diagnostics = diagnostics
            };
        }

        private static void ResolveZone(
            ZoneDefinition zone,
            GameplaySettingsConfig settings,
            Dictionary<string, GameplayPresetConfig> presetMap,
            Dictionary<string, GameplayFlowDefinitionConfig> flowMap,
            GameplayRegistrationDiagnostics diagnostics)
        {
            var resolvedEntry = new List<string>();
            var resolvedExit = new List<string>();
            var resolvedTick = new List<string>();
            var resolvedPresets = new List<GameplayPresetConfig>();
            string? presetRuleProfileId = null;

            foreach (var presetId in zone.PresetIds ?? Array.Empty<string>())
            {
                if (string.IsNullOrWhiteSpace(presetId))
                {
                    continue;
                }

                if (!presetMap.TryGetValue(presetId, out var preset))
                {
                    diagnostics.IgnoredPresets.Add($"{zone.Name}: preset '{presetId}' not found.");
                    continue;
                }

                if (!IsCompatible(zone.GameplayType, zone.NormalizedZoneType, preset.GameplayType, preset.SupportedZoneTypes))
                {
                    diagnostics.IgnoredPresets.Add($"{zone.Name}: preset '{presetId}' is incompatible with gameplay '{zone.GameplayType}' and zone type '{zone.NormalizedZoneType}'.");
                    continue;
                }

                Append(resolvedEntry, preset.EntryFlows);
                Append(resolvedExit, preset.ExitFlows);
                Append(resolvedTick, preset.TickFlows);
                resolvedPresets.Add(preset);
                presetRuleProfileId ??= preset.RuleProfileId;
            }

            Append(resolvedEntry, zone.EntryFlows);
            Append(resolvedExit, zone.ExitFlows);
            Append(resolvedTick, zone.TickFlows);

            zone.ResolvedEntryFlows = ValidateLifecycle(zone, resolvedEntry, flowMap, diagnostics, "enter");
            zone.ResolvedExitFlows = ValidateLifecycle(zone, resolvedExit, flowMap, diagnostics, "exit");
            zone.ResolvedTickFlows = ValidateLifecycle(zone, resolvedTick, flowMap, diagnostics, "tick");
            zone.ResolvedPresets = resolvedPresets.ToArray();
            zone.ResolvedRuleProfileId = !string.IsNullOrWhiteSpace(zone.RuleProfileId)
                ? zone.RuleProfileId
                : !string.IsNullOrWhiteSpace(presetRuleProfileId)
                    ? presetRuleProfileId
                    : settings.DefaultRuleProfileId;
        }

        private static string[] ValidateLifecycle(
            ZoneDefinition zone,
            List<string> lifecycleFlows,
            Dictionary<string, GameplayFlowDefinitionConfig> flowMap,
            GameplayRegistrationDiagnostics diagnostics,
            string lifecycle)
        {
            var resolved = new List<string>();
            foreach (var flowId in lifecycleFlows)
            {
                if (!flowMap.TryGetValue(flowId, out var flow))
                {
                    diagnostics.DroppedFlows.Add($"{zone.Name}: {lifecycle} flow '{flowId}' not found.");
                    continue;
                }

                if (!IsCompatible(zone.GameplayType, zone.NormalizedZoneType, flow.GameplayType, flow.SupportedZoneTypes))
                {
                    diagnostics.DroppedFlows.Add($"{zone.Name}: {lifecycle} flow '{flowId}' is incompatible with gameplay '{zone.GameplayType}' and zone type '{zone.NormalizedZoneType}'.");
                    continue;
                }

                resolved.Add(flowId);
            }

            return resolved.ToArray();
        }

        private static void Append(List<string> target, IEnumerable<string>? items)
        {
            if (items == null)
            {
                return;
            }

            foreach (var item in items)
            {
                if (string.IsNullOrWhiteSpace(item))
                {
                    continue;
                }

                if (!target.Contains(item, StringComparer.OrdinalIgnoreCase))
                {
                    target.Add(item);
                }
            }
        }

        private static bool IsCompatible(string zoneGameplayType, string zoneType, string itemGameplayType, string[] supportedZoneTypes)
        {
            if (!string.Equals(Normalize(zoneGameplayType), Normalize(itemGameplayType), StringComparison.Ordinal))
            {
                return false;
            }

            if (supportedZoneTypes == null || supportedZoneTypes.Length == 0)
            {
                return false;
            }

            return supportedZoneTypes.Any(x => string.Equals(Normalize(x), Normalize(zoneType), StringComparison.Ordinal));
        }

        private static GameplayFlowDefinitionConfig NormalizeFlow(GameplayFlowDefinitionConfig flow)
        {
            flow.GameplayType = Normalize(flow.GameplayType);
            flow.SupportedZoneTypes = (flow.SupportedZoneTypes ?? Array.Empty<string>())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(Normalize)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
            flow.Actions ??= Array.Empty<FlowAction>();
            return flow;
        }

        private static GameplayPresetConfig NormalizePreset(GameplayPresetConfig preset)
        {
            preset.GameplayType = Normalize(preset.GameplayType);
            preset.SupportedZoneTypes = (preset.SupportedZoneTypes ?? Array.Empty<string>())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(Normalize)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
            preset.EntryFlows ??= Array.Empty<string>();
            preset.ExitFlows ??= Array.Empty<string>();
            preset.TickFlows ??= Array.Empty<string>();
            return preset;
        }

        private static string Normalize(string? value)
        {
            return (value ?? string.Empty).Trim().ToLowerInvariant();
        }

        private static T LoadJson<T>(string path, T fallback) where T : class
        {
            try
            {
                if (!File.Exists(path))
                {
                    return fallback;
                }

                var json = File.ReadAllText(path);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReadCommentHandling = JsonCommentHandling.Skip
                };
                options.Converters.Add(new ZoneDefinitionJsonConverter());
                return JsonSerializer.Deserialize<T>(json, options) ?? fallback;
            }
            catch (Exception ex)
            {
                Log.LogWarning($"[GameplayRegistration] Failed to load '{path}': {ex.Message}");
                return fallback;
            }
        }
    }

    public static class ArenaGameplayRegistration
    {
        public static GameplayRegistrationResult Register(string configDirectory)
        {
            return GameplayRegistrationSupport.RegisterArena(configDirectory);
        }
    }

    public static class BossGameplayRegistration
    {
        public static GameplayRegistrationResult Register(string configDirectory)
        {
            return GameplayRegistrationSupport.RegisterBoss(configDirectory);
        }
    }
}
