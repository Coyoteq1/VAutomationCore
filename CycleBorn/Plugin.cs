using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using VampireCommandFramework;
using Unity.Mathematics;
using VAuto;
using VAutomationCore.Core;
using VAutomationCore.Core.Config;
using VAutomationCore.Core.Logging;
using VLifecycle.Services.Lifecycle;

namespace VLifecycle
{
    [BepInPlugin(MyPluginInfo.GUID, MyPluginInfo.NAME, MyPluginInfo.VERSION)]
    [BepInDependency("gg.coyote.VAutomationCore", "1.0.0")]
    [BepInDependency("gg.deca.VampireCommandFramework", "0.10.4")]
    [BepInProcess("VRisingServer.exe")]
    public class Plugin : BasePlugin
    {
        #region Logging
        private static readonly ManualLogSource _staticLog = BepInEx.Logging.Logger.CreateLogSource(MyPluginInfo.NAME);
        public new static ManualLogSource Log => _staticLog;
        public static ManualLogSource Logger => _staticLog;
        public static CoreLogger CoreLog { get; private set; }
        #endregion

        #region Harmony
        public static Plugin Instance { get; private set; }
        private Harmony _harmony;
        #endregion

        #region CFG Configuration Entries
        // General
        public static ConfigEntry<bool> GeneralEnabled;
        public static ConfigEntry<string> LogLevel;
        
        // Arena Lifecycle
        public static ConfigEntry<bool> ArenaSaveInventory;
        public static ConfigEntry<bool> ArenaRestoreInventory;
        public static ConfigEntry<bool> ArenaSaveBuffs;
        public static ConfigEntry<bool> ArenaRestoreBuffs;
        public static ConfigEntry<bool> ArenaClearBuffsOnExit;
        public static ConfigEntry<bool> ArenaResetAbilityCooldowns;
        public static ConfigEntry<bool> ArenaResetCooldownsOnExit;
        
        // Player State
        public static ConfigEntry<bool> PlayerSaveEquipment;
        public static ConfigEntry<bool> PlayerSaveBlood;
        public static ConfigEntry<bool> PlayerSaveSpells;
        public static ConfigEntry<bool> PlayerSaveHealth;
        public static ConfigEntry<bool> PlayerRestoreEquipment;
        public static ConfigEntry<bool> PlayerRestoreBlood;
        public static ConfigEntry<bool> PlayerRestoreSpells;
        public static ConfigEntry<bool> PlayerRestoreHealth;
        

        // Respawn
        public static ConfigEntry<bool> RespawnForceArenaRespawn;
        public static ConfigEntry<bool> RespawnTeleportToSpawn;
        public static ConfigEntry<bool> RespawnClearDebuffs;
        public static ConfigEntry<int> RespawnTeleportDelayMs;
        
        // Transitions
        public static ConfigEntry<int> TransitionsEnterDelayMs;
        public static ConfigEntry<int> TransitionsExitDelayMs;
        public static ConfigEntry<bool> TransitionsLockMovement;
        public static ConfigEntry<bool> TransitionsShowMessages;
        
        // Safety
        public static ConfigEntry<bool> SafetyRestoreOnError;
        public static ConfigEntry<bool> SafetyBlockEntryOnSaveFailure;
        public static ConfigEntry<bool> SafetyVerboseLogging;
        
        // Integration
        public static ConfigEntry<bool> IntegrationZoneTriggersLifecycle;
        public static ConfigEntry<bool> IntegrationAllowTrapOverrides;
        public static ConfigEntry<bool> IntegrationSendEvents;
        
        // Debug
        public static ConfigEntry<bool> DebugMode;
        public static ConfigEntry<bool> HotReloadEnabled;
        #endregion

        #region JSON Configuration
        private static string _configPath;
        private static string _legacyConfigPath;
        private static UnifiedLifecycleConfig _jsonConfig;
        private static DateTime _lastConfigCheck;
        private static System.Timers.Timer _hotReloadTimer;
        #endregion

        public override void Load()
        {
            Instance = this;
            
            // Initialize CoreLogger first
            CoreLog = new CoreLogger("VLifecycle");
            CoreLog.Info($"Loading v{MyPluginInfo.VERSION}...");

            try
            {
                // Global exception hooks for diagnostics
                AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
                {
                    try { Log.LogError($"[GlobalUnhandled] {(args.ExceptionObject as Exception)?.ToString() ?? args.ExceptionObject}"); }
                    catch { }
                };
                System.Threading.Tasks.TaskScheduler.UnobservedTaskException += (sender, args) =>
                {
                    try { Log.LogError($"[GlobalUnobservedTask] {args.Exception}"); args.SetObserved(); }
                    catch { }
                };
#if DEBUG
                Log.LogInfo("[Diagnostics] DEBUG build detected; verbose debug logging enabled");
#endif
                // Initialize unified config paths
                _configPath = Path.Combine(Paths.ConfigPath, "VAuto.Lifecycle.json");
                _legacyConfigPath = Path.Combine(Paths.ConfigPath, "VAutoZone", "config", "sandbox_defaults.json");

                RunMigration();
                LoadJsonConfiguration();
                var legacyCfgPath = Path.Combine(Paths.ConfigPath, "VLifecycle.cfg");
                if (File.Exists(legacyCfgPath))
                {
                    Log.LogWarning("[VLifecycle] VLifecycle.cfg is deprecated. Use VAuto.Lifecycle.json.");
                }

                // Check if enabled
                if (GeneralEnabled != null && !GeneralEnabled.Value)
                {
                    Log.LogInfo("[VLifecycle] Disabled via config.");
                    return;
                }

                // Register commands with VCF
                AnnouncementService.Initialize(CoreLog);
                CommandRegistry.RegisterAll(Assembly.GetExecutingAssembly());
                CoreLog.Info("Commands registered");
                LogStartupSummary();

                if (MyPluginInfo.Vlifecycle.EnableHarmony)
                {
                    _harmony = new Harmony(MyPluginInfo.Vlifecycle.HarmonyId);
                    var target = VLifecycle.Services.Lifecycle.RespawnPreventionPatches.ResolveInitializeNewSpawnChainOnUpdate();
                    if (target != null)
                    {
                        var prefix = new HarmonyMethod(typeof(VLifecycle.Services.Lifecycle.RespawnPreventionPatches)
                            .GetMethod(nameof(VLifecycle.Services.Lifecycle.RespawnPreventionPatches.InitializeNewSpawnChainPrefix), BindingFlags.Public | BindingFlags.Static));
                        _harmony.Patch(target, prefix: prefix);
                        CoreLog.Info("Harmony patch applied: InitializeNewSpawnChainSystem.OnUpdate");
                    }
                    else
                    {
                        CoreLog.Warning("Harmony patch skipped: InitializeNewSpawnChainSystem.OnUpdate not found");
                    }
                }

                // Start hot-reload monitoring if enabled
                if (HotReloadEnabled?.Value == true)
                {
                    StartHotReloadMonitoring();
                }

                Log.LogInfo($"[{MyPluginInfo.NAME}] Loaded successfully.");
            }
            catch (Exception ex)
            {
                UnifiedCore.LogException(ex);
                Log.LogError(ex);
            }
        }

        private void RunMigration()
        {
            try
            {
                // Check if legacy config exists and migrate if needed
                if (File.Exists(_legacyConfigPath))
                {
                    Log.LogInfo("[VLifecycle] Legacy sandbox config detected, running migration...");
                    LifecycleConfigMigration.RunMigration(_configPath, _legacyConfigPath);
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"[VLifecycle] Migration failed: {ex.Message}");
            }
        }

        private void LoadJsonConfiguration()
        {
            try
            {
                if (File.Exists(_configPath))
                {
                    var jsonContent = File.ReadAllText(_configPath);
                    _jsonConfig = JsonSerializer.Deserialize<UnifiedLifecycleConfig>(jsonContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        ReadCommentHandling = JsonCommentHandling.Skip,
                        AllowTrailingCommas = true
                    });
                    Log.LogInfo($"[VLifecycle] Loaded unified JSON configuration from {_configPath}");
                }
                else
                {
                    _jsonConfig = new UnifiedLifecycleConfig();
                    SaveJsonConfiguration();
                    Log.LogInfo($"[VLifecycle] Created new unified JSON configuration at {_configPath}");
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"[VLifecycle] Failed to load JSON configuration: {ex.Message}");
                _jsonConfig = new UnifiedLifecycleConfig();
            }
        }

        private void SaveJsonConfiguration()
        {
            try
            {
                var jsonContent = JsonSerializer.Serialize(_jsonConfig, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                File.WriteAllText(_configPath, jsonContent);
                Log.LogInfo($"[VLifecycle] Saved unified JSON configuration to {_configPath}");
            }
            catch (Exception ex)
            {
                Log.LogError($"[VLifecycle] Failed to save JSON configuration: {ex.Message}");
            }
        }

        private void StartHotReloadMonitoring()
        {
            _lastConfigCheck = DateTime.UtcNow;
            _hotReloadTimer = new System.Timers.Timer(5000);
            _hotReloadTimer.Elapsed += (_, _) => CheckForConfigChanges();
            _hotReloadTimer.Start();
            Log.LogInfo("[VLifecycle] Hot-reload monitoring started.");
        }

        private void CheckForConfigChanges()
        {
            try
            {
                if (File.Exists(_configPath))
                {
                    var lastModified = File.GetLastWriteTime(_configPath);
                    if (lastModified > _lastConfigCheck)
                    {
                        _lastConfigCheck = lastModified;
                        LoadJsonConfiguration();
                        Log.LogInfo("[VLifecycle] Configuration hot-reloaded successfully");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"[VLifecycle] Error checking configuration changes: {ex.Message}");
            }
        }

        private static void LogStartupSummary()
        {
            Log.LogInfo("[VLifecycle] Startup Summary:");
            Log.LogInfo($"[VLifecycle]   Config(JSON): {_configPath}");
            Log.LogInfo($"[VLifecycle]   Legacy Config(JSON): {_legacyConfigPath}");
            Log.LogInfo($"[VLifecycle]   Sandbox Main Arena: {SandboxDefaultArenaId}");
            Log.LogInfo($"[VLifecycle]   Sandbox Experimental Arena: {SandboxExperimentalArenaId}");
            Log.LogInfo("[VLifecycle]   Command Roots: lifecycle");
        }

        #region Sandbox Configuration Accessors
        public static SandboxRuntimeConfig GetSandboxRuntimeConfig(string arenaId = "")
        {
            var sandbox = _jsonConfig?.Sandbox ?? new SandboxSection();
            return sandbox.ResolveRuntime(arenaId);
        }

        public static bool SandboxEnabled => GetSandboxRuntimeConfig().Enabled;
        public static bool SandboxAutoApplyUnlocks => GetSandboxRuntimeConfig().AutoApplyUnlocks;
        public static bool SandboxSuppressVBloodFeed => GetSandboxRuntimeConfig().SuppressVBloodFeed;
        public static float SandboxDespawnDelaySeconds => (float)GetSandboxRuntimeConfig().DespawnDelaySeconds;
        public static string SandboxDefaultArenaId => GetSandboxRuntimeConfig().DefaultArenaId;
        public static string SandboxExperimentalArenaId => _jsonConfig?.Sandbox?.ExperimentalArenaId ?? "sandbox_experimental";
        public static bool SandboxRoutingEnabled => _jsonConfig?.Sandbox?.RouteArenaById ?? true;

        public static bool IsExperimentalSandboxArena(string arenaId)
        {
            var sandbox = _jsonConfig?.Sandbox ?? new SandboxSection();
            return sandbox.IsExperimentalArena(arenaId);
        }
        #endregion

        #region Stages Configuration Accessors
        public static bool StageOnEnterEnabled => _jsonConfig?.Stages?.OnEnter?.Enabled ?? true;
        public static bool StageIsInZoneEnabled => _jsonConfig?.Stages?.IsInZone?.Enabled ?? true;
        public static bool StageOnExitEnabled => _jsonConfig?.Stages?.OnExit?.Enabled ?? true;
        #endregion

        #region Public Configuration Accessors (CFG - Deprecated)
        public static bool IsEnabled => _jsonConfig?.Lifecycle?.Enabled ?? GeneralEnabled?.Value ?? true;
        public static bool SaveInventory => _jsonConfig?.Lifecycle?.Arena?.SaveInventory ?? ArenaSaveInventory?.Value ?? true;
        public static bool RestoreInventory => _jsonConfig?.Lifecycle?.Arena?.RestoreInventory ?? ArenaRestoreInventory?.Value ?? true;
        public static bool SaveBuffs => _jsonConfig?.Lifecycle?.Arena?.SaveBuffs ?? ArenaSaveBuffs?.Value ?? true;
        public static bool RestoreBuffs => _jsonConfig?.Lifecycle?.Arena?.RestoreBuffs ?? ArenaRestoreBuffs?.Value ?? true;
        public static bool ClearBuffsOnExit => _jsonConfig?.Lifecycle?.Arena?.ClearArenaBuffsOnExit ?? ArenaClearBuffsOnExit?.Value ?? true;
        public static bool ResetCooldownsOnEnter => _jsonConfig?.Lifecycle?.Arena?.ResetAbilityCooldowns ?? ArenaResetAbilityCooldowns?.Value ?? true;
        public static bool ResetCooldownsOnExit => _jsonConfig?.Lifecycle?.Arena?.ResetCooldownsOnExit ?? ArenaResetCooldownsOnExit?.Value ?? false;
        public static bool SaveEquipment => _jsonConfig?.Lifecycle?.PlayerState?.SaveEquipment ?? PlayerSaveEquipment?.Value ?? true;
        public static bool SaveBlood => _jsonConfig?.Lifecycle?.PlayerState?.SaveBlood ?? PlayerSaveBlood?.Value ?? true;
        public static bool SaveSpells => _jsonConfig?.Lifecycle?.PlayerState?.SaveSpells ?? PlayerSaveSpells?.Value ?? true;
        public static bool SaveHealth => _jsonConfig?.Lifecycle?.PlayerState?.SaveHealth ?? PlayerSaveHealth?.Value ?? true;
        public static bool RestoreEquipment => PlayerRestoreEquipment?.Value ?? true;
        public static bool RestoreBlood => PlayerRestoreBlood?.Value ?? true;
        public static bool RestoreSpells => PlayerRestoreSpells?.Value ?? true;
        public static bool RestoreHealth => _jsonConfig?.Lifecycle?.PlayerState?.RestoreHealth ?? PlayerRestoreHealth?.Value ?? true;
        public static bool ForceArenaRespawn => _jsonConfig?.Lifecycle?.Respawn?.ForceArenaRespawn ?? RespawnForceArenaRespawn?.Value ?? false;
        public static bool TeleportToSpawnOnRespawn => _jsonConfig?.Lifecycle?.Respawn?.TeleportToArenaSpawn ?? RespawnTeleportToSpawn?.Value ?? true;
        public static bool ClearDebuffsOnRespawn => _jsonConfig?.Lifecycle?.Respawn?.ClearTemporaryDebuffs ?? RespawnClearDebuffs?.Value ?? true;
        public static int RespawnDelayMs => _jsonConfig?.Lifecycle?.Respawn?.RespawnTeleportDelayMs ?? RespawnTeleportDelayMs?.Value ?? 1000;
        public static int EnterDelayMs => _jsonConfig?.Lifecycle?.Transitions?.EnterDelayMs ?? TransitionsEnterDelayMs?.Value ?? 0;
        public static int ExitDelayMs => _jsonConfig?.Lifecycle?.Transitions?.ExitDelayMs ?? TransitionsExitDelayMs?.Value ?? 0;
        public static bool LockMovementDuringTransition => _jsonConfig?.Lifecycle?.Transitions?.LockMovementDuringTransition ?? TransitionsLockMovement?.Value ?? false;
        public static bool ShowTransitionMessages => _jsonConfig?.Lifecycle?.Transitions?.ShowTransitionMessages ?? TransitionsShowMessages?.Value ?? true;
        public static bool RestoreOnError => _jsonConfig?.Lifecycle?.Safety?.RestoreOnError ?? SafetyRestoreOnError?.Value ?? true;
        public static bool BlockEntryOnSaveFailure => _jsonConfig?.Lifecycle?.Safety?.BlockEntryOnSaveFailure ?? SafetyBlockEntryOnSaveFailure?.Value ?? true;
        public static bool VerboseLogging => _jsonConfig?.Lifecycle?.Safety?.VerboseLogging ?? SafetyVerboseLogging?.Value ?? false;
        public static bool ZoneTriggersLifecycle => _jsonConfig?.Lifecycle?.Integration?.ZoneTriggersLifecycle ?? IntegrationZoneTriggersLifecycle?.Value ?? true;
        public static bool AllowTrapOverrides => _jsonConfig?.Lifecycle?.Integration?.AllowTrapOverrides ?? IntegrationAllowTrapOverrides?.Value ?? true;
        public static bool SendLifecycleEvents => _jsonConfig?.Lifecycle?.Integration?.SendLifecycleEvents ?? IntegrationSendEvents?.Value ?? true;
        public static bool DebugModeEnabled => DebugMode?.Value ?? false;
        #endregion

        public override bool Unload()
        {
            try
            {
                _hotReloadTimer?.Dispose();
                _hotReloadTimer = null;
                _harmony?.UnpatchSelf();
                _harmony = null;
                // ArenaLifecycleManager.Instance.Shutdown(); // Excluded from headless builds
                Log.LogInfo("[VLifecycle] Unloaded.");
                return true;
            }
            catch (Exception ex)
            {
                UnifiedCore.LogException(ex);
                Log.LogError(ex);
                return false;
            }
        }
    }

    #region Unified Configuration Classes
    
    /// <summary>
    /// Unified lifecycle configuration with lifecycle, sandbox, and stages.
    /// </summary>
    public class UnifiedLifecycleConfig
    {
        public string Version { get; set; } = "1.0.0";
        public LifecycleSection Lifecycle { get; set; } = new();
        public SandboxSection Sandbox { get; set; } = new();
        public StagesSection Stages { get; set; } = new();
    }

    public class LifecycleSection
    {
        public bool Enabled { get; set; } = true;
        public ArenaSection Arena { get; set; } = new();
        public PlayerStateSection PlayerState { get; set; } = new();
        public RespawnSection Respawn { get; set; } = new();
        public TransitionsSection Transitions { get; set; } = new();
        public SafetySection Safety { get; set; } = new();
        public IntegrationSection Integration { get; set; } = new();
    }

    public class SandboxSection
    {
        public bool Enabled { get; set; } = true;
        public bool AutoApplyUnlocks { get; set; } = true;
        public bool SuppressVBloodFeed { get; set; } = true;
        public double DespawnDelaySeconds { get; set; } = 2.0;
        public string DefaultArenaId { get; set; } = "sandbox_main";

        // Second sandbox profile: same lifecycle flow, different tuning.
        public bool ExperimentalEnabled { get; set; } = true;
        public bool ExperimentalAutoApplyUnlocks { get; set; } = true;
        public bool ExperimentalSuppressVBloodFeed { get; set; } = false;
        public double ExperimentalDespawnDelaySeconds { get; set; } = 0.5;
        public string ExperimentalArenaId { get; set; } = "sandbox_experimental";
        public List<string> ExperimentalArenaAliases { get; set; } = new() { "sandbox_experimental", "sandbox_exp" };
        public bool RouteArenaById { get; set; } = true;
        public string ExperimentalArenaIdMatchToken { get; set; } = "experimental";

        public SandboxRuntimeConfig ResolveRuntime(string arenaId)
        {
            var normalizedArenaId = (arenaId ?? string.Empty).Trim();
            if (RouteArenaById && IsExperimentalArena(normalizedArenaId))
            {
                return new SandboxRuntimeConfig(
                    profileId: SandboxRuntimeConfig.ExperimentalProfileId,
                    enabled: ExperimentalEnabled,
                    autoApplyUnlocks: ExperimentalAutoApplyUnlocks,
                    suppressVBloodFeed: ExperimentalSuppressVBloodFeed,
                    despawnDelaySeconds: ExperimentalDespawnDelaySeconds,
                    defaultArenaId: string.IsNullOrWhiteSpace(ExperimentalArenaId) ? "sandbox_experimental" : ExperimentalArenaId);
            }

            return new SandboxRuntimeConfig(
                profileId: SandboxRuntimeConfig.MainProfileId,
                enabled: Enabled,
                autoApplyUnlocks: AutoApplyUnlocks,
                suppressVBloodFeed: SuppressVBloodFeed,
                despawnDelaySeconds: DespawnDelaySeconds,
                defaultArenaId: string.IsNullOrWhiteSpace(DefaultArenaId) ? "sandbox_main" : DefaultArenaId);
        }

        public bool IsExperimentalArena(string arenaId)
        {
            if (string.IsNullOrWhiteSpace(arenaId))
            {
                return false;
            }

            var candidate = arenaId.Trim();
            if (!string.IsNullOrWhiteSpace(ExperimentalArenaId) &&
                string.Equals(candidate, ExperimentalArenaId, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (ExperimentalArenaAliases != null)
            {
                foreach (var alias in ExperimentalArenaAliases)
                {
                    if (string.IsNullOrWhiteSpace(alias))
                    {
                        continue;
                    }

                    if (string.Equals(candidate, alias.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(ExperimentalArenaIdMatchToken) &&
                candidate.Contains(ExperimentalArenaIdMatchToken, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }
    }

    public sealed class SandboxRuntimeConfig
    {
        public const string MainProfileId = "main";
        public const string ExperimentalProfileId = "experimental";

        public string ProfileId { get; }
        public bool Enabled { get; }
        public bool AutoApplyUnlocks { get; }
        public bool SuppressVBloodFeed { get; }
        public double DespawnDelaySeconds { get; }
        public string DefaultArenaId { get; }

        public SandboxRuntimeConfig(
            string profileId,
            bool enabled,
            bool autoApplyUnlocks,
            bool suppressVBloodFeed,
            double despawnDelaySeconds,
            string defaultArenaId)
        {
            ProfileId = string.IsNullOrWhiteSpace(profileId) ? MainProfileId : profileId;
            Enabled = enabled;
            AutoApplyUnlocks = autoApplyUnlocks;
            SuppressVBloodFeed = suppressVBloodFeed;
            DespawnDelaySeconds = despawnDelaySeconds;
            DefaultArenaId = string.IsNullOrWhiteSpace(defaultArenaId) ? "sandbox_main" : defaultArenaId;
        }
    }

    public class StagesSection
    {
        public StageConfig OnEnter { get; set; } = new() { Enabled = true, Description = "Stage triggered when player enters a zone" };
        public StageConfig IsInZone { get; set; } = new() { Enabled = true, Description = "Stage triggered repeatedly while player remains in zone" };
        public StageConfig OnExit { get; set; } = new() { Enabled = true, Description = "Stage triggered when player exits a zone" };
    }

    public class StageConfig
    {
        public bool Enabled { get; set; } = true;
        public string Description { get; set; } = "";
    }

    public class ArenaSection
    {
        public bool SaveInventory { get; set; } = true;
        public bool RestoreInventory { get; set; } = true;
        public bool SaveBuffs { get; set; } = true;
        public bool RestoreBuffs { get; set; } = true;
        public bool ClearArenaBuffsOnExit { get; set; } = true;
        public bool ResetAbilityCooldowns { get; set; } = true;
        public bool ResetCooldownsOnExit { get; set; } = false;
    }

    public class PlayerStateSection
    {
        public bool SaveEquipment { get; set; } = true;
        public bool SaveBlood { get; set; } = true;
        public bool SaveSpells { get; set; } = true;
        public bool SaveHealth { get; set; } = true;
        public bool RestoreHealth { get; set; } = true;
    }

    public class RespawnSection
    {
        public bool ForceArenaRespawn { get; set; } = false;
        public bool TeleportToArenaSpawn { get; set; } = true;
        public bool ClearTemporaryDebuffs { get; set; } = true;
        public int RespawnTeleportDelayMs { get; set; } = 1000;
    }

    public class TransitionsSection
    {
        public int EnterDelayMs { get; set; } = 0;
        public int ExitDelayMs { get; set; } = 0;
        public bool LockMovementDuringTransition { get; set; } = false;
        public bool ShowTransitionMessages { get; set; } = true;
    }

    public class SafetySection
    {
        public bool RestoreOnError { get; set; } = true;
        public bool BlockEntryOnSaveFailure { get; set; } = true;
        public bool VerboseLogging { get; set; } = false;
    }

    public class IntegrationSection
    {
        public bool ZoneTriggersLifecycle { get; set; } = true;
        public bool AllowTrapOverrides { get; set; } = true;
        public bool SendLifecycleEvents { get; set; } = true;
    }
    
    #endregion

    #region Legacy Configuration Classes (for migration)
    
    /// <summary>
    /// Legacy sandbox defaults configuration (VAutoZone).
    /// </summary>
    public class LegacySandboxDefaults
    {
        public bool Enabled { get; set; } = true;
        public bool AutoApplyUnlocks { get; set; } = true;
        public bool SuppressVBloodFeed { get; set; } = true;
        public double DespawnDelaySeconds { get; set; } = 2.0;
    }
    
    #endregion

    #region Migration Service
    
    /// <summary>
    /// Handles migration from legacy configuration files to unified VAuto.Lifecycle.json.
    /// </summary>
    public static class LifecycleConfigMigration
    {
        private static readonly ManualLogSource _log = BepInEx.Logging.Logger.CreateLogSource("VLifecycle.Migration");
        
        public static void RunMigration(string configPath, string legacyPath)
        {
            try
            {
                // Check if migration is needed
                if (!File.Exists(legacyPath))
                {
                    _log.LogDebug("[Migration] No legacy config found, skipping migration.");
                    return;
                }

                _log.LogInfo("[Migration] ===============================================");
                _log.LogInfo("[Migration] Configuration Migration");
                _log.LogInfo("[Migration] ===============================================");
                _log.LogInfo($"[Migration] Found legacy config: {legacyPath}");

                // Read legacy config
                var legacyJson = File.ReadAllText(legacyPath);
                var legacyConfig = JsonSerializer.Deserialize<LegacySandboxDefaults>(legacyJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (legacyConfig == null)
                {
                    _log.LogWarning("[Migration] Failed to parse legacy config, skipping migration.");
                    return;
                }

                _log.LogInfo("[Migration] Migrating settings...");
                _log.LogInfo($"[Migration]   - Sandbox.Enabled: {legacyConfig.Enabled}");
                _log.LogInfo($"[Migration]   - Sandbox.AutoApplyUnlocks: {legacyConfig.AutoApplyUnlocks}");
                _log.LogInfo($"[Migration]   - Sandbox.SuppressVBloodFeed: {legacyConfig.SuppressVBloodFeed}");
                _log.LogInfo($"[Migration]   - Sandbox.DespawnDelaySeconds: {legacyConfig.DespawnDelaySeconds}");

                // Load or create unified config
                UnifiedLifecycleConfig unifiedConfig;
                if (File.Exists(configPath))
                {
                    var existingJson = File.ReadAllText(configPath);
                    unifiedConfig = JsonSerializer.Deserialize<UnifiedLifecycleConfig>(existingJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new UnifiedLifecycleConfig();
                    _log.LogInfo($"[Migration] Loaded existing unified config from: {configPath}");
                }
                else
                {
                    unifiedConfig = new UnifiedLifecycleConfig();
                    _log.LogInfo("[Migration] Creating new unified config.");
                }

                // Merge legacy sandbox settings into unified config
                if (unifiedConfig.Sandbox == null)
                {
                    unifiedConfig.Sandbox = new SandboxSection();
                }

                unifiedConfig.Sandbox.Enabled = legacyConfig.Enabled;
                unifiedConfig.Sandbox.AutoApplyUnlocks = legacyConfig.AutoApplyUnlocks;
                unifiedConfig.Sandbox.SuppressVBloodFeed = legacyConfig.SuppressVBloodFeed;
                unifiedConfig.Sandbox.DespawnDelaySeconds = legacyConfig.DespawnDelaySeconds;

                // Save unified config
                var newJson = JsonSerializer.Serialize(unifiedConfig, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                File.WriteAllText(configPath, newJson);

                _log.LogInfo($"[Migration] New config written to: {configPath}");
                _log.LogInfo("[Migration] Legacy config preserved (delete manually to clean up)");
                _log.LogInfo("[Migration] ===============================================");
            }
            catch (Exception ex)
            {
                _log.LogError($"[Migration] Failed to migrate configuration: {ex.Message}");
            }
        }
    }
    
    #endregion
    
}
