using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Blueluck.Models
{
    public static class GameplayFamilies
    {
        public const string Arena = "arena";
        public const string Boss = "boss";
    }

    public static class GameplayZoneTypes
    {
        public const string Arena = "arena";
        public const string Boss = "boss";
    }

    public sealed class GameplaySettingsConfig
    {
        [JsonPropertyName("detection")]
        public ZoneDetectionConfig? Detection { get; set; }

        [JsonPropertyName("defaultRuleProfileId")]
        public string? DefaultRuleProfileId { get; set; }
    }

    public sealed class GameplayRulesConfig
    {
        [JsonPropertyName("rules")]
        public GameplayRuleProfileConfig[] Rules { get; set; } = Array.Empty<GameplayRuleProfileConfig>();
    }

    public sealed class GameplayRuleProfileConfig
    {
        [JsonPropertyName("ruleProfileId")]
        public string RuleProfileId { get; set; } = string.Empty;

        [JsonPropertyName("settings")]
        public Dictionary<string, JsonElement> Settings { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    }

    public sealed class GameplayFlowsConfig
    {
        [JsonPropertyName("flows")]
        public GameplayFlowDefinitionConfig[] Flows { get; set; } = Array.Empty<GameplayFlowDefinitionConfig>();
    }

    public sealed class GameplayFlowDefinitionConfig
    {
        [JsonPropertyName("flowId")]
        public string FlowId { get; set; } = string.Empty;

        [JsonPropertyName("gameplayType")]
        public string GameplayType { get; set; } = string.Empty;

        [JsonPropertyName("supportedZoneTypes")]
        public string[] SupportedZoneTypes { get; set; } = Array.Empty<string>();

        [JsonPropertyName("actions")]
        public FlowAction[] Actions { get; set; } = Array.Empty<FlowAction>();
    }

    public sealed class GameplayPresetsConfig
    {
        [JsonPropertyName("presets")]
        public GameplayPresetConfig[] Presets { get; set; } = Array.Empty<GameplayPresetConfig>();
    }

    public sealed class GameplayPresetConfig
    {
        [JsonPropertyName("presetId")]
        public string PresetId { get; set; } = string.Empty;

        [JsonPropertyName("gameplayType")]
        public string GameplayType { get; set; } = string.Empty;

        [JsonPropertyName("supportedZoneTypes")]
        public string[] SupportedZoneTypes { get; set; } = Array.Empty<string>();

        [JsonPropertyName("entryFlows")]
        public string[] EntryFlows { get; set; } = Array.Empty<string>();

        [JsonPropertyName("exitFlows")]
        public string[] ExitFlows { get; set; } = Array.Empty<string>();

        [JsonPropertyName("tickFlows")]
        public string[] TickFlows { get; set; } = Array.Empty<string>();

        [JsonPropertyName("ruleProfileId")]
        public string? RuleProfileId { get; set; }

        [JsonPropertyName("displayName")]
        public string? DisplayName { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("session")]
        public GameSessionConfig? Session { get; set; }

        [JsonPropertyName("sessionLifecycle")]
        public SessionLifecycleConfig? SessionLifecycle { get; set; }

        [JsonPropertyName("objective")]
        public GameObjectiveConfig? Objective { get; set; }
    }

    public sealed class GameSessionConfig
    {
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; } = false;

        [JsonPropertyName("minPlayers")]
        public int MinPlayers { get; set; } = 1;

        [JsonPropertyName("maxPlayers")]
        public int MaxPlayers { get; set; } = 16;

        [JsonPropertyName("countdownSeconds")]
        public int CountdownSeconds { get; set; } = 10;

        [JsonPropertyName("readyTimeoutSeconds")]
        public int ReadyTimeoutSeconds { get; set; } = 120;

        [JsonPropertyName("matchDurationSeconds")]
        public int MatchDurationSeconds { get; set; }

        [JsonPropertyName("autoStartWhenReady")]
        public bool AutoStartWhenReady { get; set; } = true;

        [JsonPropertyName("requireAllPresentReady")]
        public bool RequireAllPresentReady { get; set; } = true;

        [JsonPropertyName("allowLateJoin")]
        public bool AllowLateJoin { get; set; }

        [JsonPropertyName("lateJoinGraceSeconds")]
        public int LateJoinGraceSeconds { get; set; } = 30;

        [JsonPropertyName("postMatchResetDelaySeconds")]
        public int PostMatchResetDelaySeconds { get; set; } = 5;

        [JsonPropertyName("freezeDuringCountdown")]
        public bool FreezeDuringCountdown { get; set; } = true;

        [JsonPropertyName("countdownFreezeBuffPrefab")]
        public string? CountdownFreezeBuffPrefab { get; set; } = "Buff_General_Freeze";

        [JsonPropertyName("resetOnEmpty")]
        public bool ResetOnEmpty { get; set; } = true;
    }

    public sealed class SessionLifecycleConfig
    {
        [JsonPropertyName("prepareFlows")]
        public string[] PrepareFlows { get; set; } = Array.Empty<string>();

        [JsonPropertyName("lobbyOpenFlows")]
        public string[] LobbyOpenFlows { get; set; } = Array.Empty<string>();

        [JsonPropertyName("playerReadyFlows")]
        public string[] PlayerReadyFlows { get; set; } = Array.Empty<string>();

        [JsonPropertyName("playerUnreadyFlows")]
        public string[] PlayerUnreadyFlows { get; set; } = Array.Empty<string>();

        [JsonPropertyName("countdownFlows")]
        public string[] CountdownFlows { get; set; } = Array.Empty<string>();

        [JsonPropertyName("startFlows")]
        public string[] StartFlows { get; set; } = Array.Empty<string>();

        [JsonPropertyName("lateJoinFlows")]
        public string[] LateJoinFlows { get; set; } = Array.Empty<string>();

        [JsonPropertyName("victoryFlows")]
        public string[] VictoryFlows { get; set; } = Array.Empty<string>();

        [JsonPropertyName("defeatFlows")]
        public string[] DefeatFlows { get; set; } = Array.Empty<string>();

        [JsonPropertyName("endFlows")]
        public string[] EndFlows { get; set; } = Array.Empty<string>();

        [JsonPropertyName("resetFlows")]
        public string[] ResetFlows { get; set; } = Array.Empty<string>();

        [JsonPropertyName("tickFlows")]
        public string[] TickFlows { get; set; } = Array.Empty<string>();
    }

    public sealed class GameObjectiveConfig
    {
        [JsonPropertyName("objectiveType")]
        public string ObjectiveType { get; set; } = "timer_only";

        [JsonPropertyName("endMatchOnObjective")]
        public bool EndMatchOnObjective { get; set; } = true;

        [JsonPropertyName("treatTimeoutAsDraw")]
        public bool TreatTimeoutAsDraw { get; set; } = true;
    }

    public sealed class ArenaZonesConfig
    {
        [JsonPropertyName("zones")]
        public ArenaZoneConfig[] Zones { get; set; } = Array.Empty<ArenaZoneConfig>();
    }

    public sealed class BossZonesConfig
    {
        [JsonPropertyName("zones")]
        public BossZoneConfig[] Zones { get; set; } = Array.Empty<BossZoneConfig>();
    }

    public sealed class GameplayRegistrationDiagnostics
    {
        public string GameplayType { get; init; } = string.Empty;
        public List<string> Warnings { get; } = new();
        public List<string> InvalidZones { get; } = new();
        public List<string> DroppedFlows { get; } = new();
        public List<string> IgnoredPresets { get; } = new();
        public int RegisteredZoneCount { get; set; }
        public int RegisteredFlowCount { get; set; }
        public int RegisteredPresetCount { get; set; }

        public bool HasIssues =>
            Warnings.Count > 0 ||
            InvalidZones.Count > 0 ||
            DroppedFlows.Count > 0 ||
            IgnoredPresets.Count > 0;
    }

    public sealed class GameplayRegistrationResult
    {
        public string GameplayType { get; init; } = string.Empty;
        public ZoneDefinition[] Zones { get; init; } = Array.Empty<ZoneDefinition>();
        public GameplayFlowDefinitionConfig[] Flows { get; init; } = Array.Empty<GameplayFlowDefinitionConfig>();
        public GameplayPresetConfig[] Presets { get; init; } = Array.Empty<GameplayPresetConfig>();
        public GameplayRuleProfileConfig[] Rules { get; init; } = Array.Empty<GameplayRuleProfileConfig>();
        public GameplaySettingsConfig Settings { get; init; } = new();
        public GameplayRegistrationDiagnostics Diagnostics { get; init; } = new();
    }
}
