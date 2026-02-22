using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Unity.Entities;
using Unity.Collections;
using ProjectM;
using VampireCommandFramework;
using VAutomationCore.Core.Logging;
using ExtraSlots.Systems;
using ExtraSlots.Models;

[BepInPlugin("gg.coyote.ExtraSlots", "ExtraSlots", "1.0.0")]
[BepInDependency("gg.coyote.VAutomationCore", "1.0.0")]
[BepInDependency("gg.deca.VampireCommandFramework", "0.10.4")]
[BepInDependency("gg.coyote.lifecycle", "1.0.0")]
[BepInProcess("VRisingServer.exe")]
public class ExtraSlotsPlugin : BasePlugin
{
    #region Logging
    private static readonly ManualLogSource _staticLog = BepInEx.Logging.Logger.CreateLogSource("ExtraSlots");
    public new static ManualLogSource Log => _staticLog;
    public static ManualLogSource Logger => _staticLog;
    public static CoreLogger CoreLog { get; private set; }
    #endregion

    public static ExtraSlotsPlugin Instance { get; private set; }
    public static Harmony Harmony;
    public static EntityManager EM;

    #region CFG Configuration Entries
    public static ConfigEntry<bool> GeneralEnabled;
    public static ConfigEntry<string> LogLevel;
    public static ConfigEntry<bool> DebugMode;
    #endregion

    public override void Load()
    {
        Instance = this;
        Log.LogInfo("[ExtraSlots] Loading...");

        try
        {
            // Initialize logger
            CoreLog = new CoreLogger(_staticLog, "ExtraSlots");

            // Bind configuration
            BindConfiguration();

            // Check if enabled
            if (GeneralEnabled != null && !GeneralEnabled.Value)
            {
                Log.LogInfo("[ExtraSlots] Disabled via config.");
                return;
            }

            Harmony = new Harmony("gg.coyote.ExtraSlots");

            // Initialize services
            ExtraSlotsService.Initialize();

            // Apply patches
            Harmony.PatchAll(typeof(ExtraSlotsPlugin).Assembly);

            // Register commands
            CommandRegistry.RegisterAll(Assembly.GetExecutingAssembly());
            Log.LogInfo("[ExtraSlots] Commands registered");

            // Log startup summary
            LogStartupSummary();

            Log.LogInfo("[ExtraSlots] Loaded successfully!");
        }
        catch (Exception ex)
        {
            Log.LogError(ex);
        }
    }

    private void BindConfiguration()
    {
        var configFile = new ConfigFile(Paths.ConfigPath + "/VAuto.Swapkits.cfg", true);

        GeneralEnabled = configFile.Bind("General", "Enabled", true, "Enable or disable Extra Slots plugin");
        LogLevel = configFile.Bind("General", "LogLevel", "Info", "Log level (Debug, Info, Warning, Error)");
        DebugMode = configFile.Bind("Debug", "DebugMode", false, "Enable debug mode");
    }

    private static void LogStartupSummary()
    {
        Logger.LogInfo("=== ExtraSlots Startup Summary ===");
        Logger.LogInfo($"  Enabled: {GeneralEnabled?.Value ?? true}");
        Logger.LogInfo($"  Debug Mode: {DebugMode?.Value ?? false}");
        Logger.LogInfo("==================================");
    }

}
