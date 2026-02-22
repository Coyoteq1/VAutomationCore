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
    public static ConfigEntry<bool> BindRestrictToAliases;
    public static ConfigEntry<string> BindKeyAliases;
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
            WeaponBindsSystem.Instance.Configure(
                BindKeyAliases?.Value,
                BindRestrictToAliases?.Value ?? false);

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

    public override bool Unload()
    {
        try
        {
            WeaponBindsSystem.Instance.SaveBindings();
            Harmony?.UnpatchSelf();
            Harmony = null;
            return true;
        }
        catch (Exception ex)
        {
            Log.LogError(ex);
            return false;
        }
    }

    private void BindConfiguration()
    {
        var configFile = new ConfigFile(Paths.ConfigPath + "/VAuto.Swapkits.cfg", true);

        GeneralEnabled = configFile.Bind("General", "Enabled", true, "Enable or disable Extra Slots plugin");
        LogLevel = configFile.Bind("General", "LogLevel", "Info", "Log level (Debug, Info, Warning, Error)");
        DebugMode = configFile.Bind("Debug", "DebugMode", false, "Enable debug mode");

        BindRestrictToAliases = configFile.Bind(
            "Binds",
            "RestrictToAliases",
            false,
            "If true, only configured key aliases are allowed for .extra bind/.extra unbind.");

        BindKeyAliases = configFile.Bind(
            "Binds",
            "KeyAliases",
            "1=Alpha1,2=Alpha2,3=Alpha3,4=Alpha4,5=Alpha5,6=Alpha6,7=Alpha7,8=Alpha8,9=Alpha9,0=Alpha0,F1=F1,F2=F2,F3=F3,F4=F4,F5=F5,F6=F6,F7=F7,F8=F8,F9=F9,F10=F10,F11=F11,F12=F12,Q=Q,W=W,E=E,R=R,T=T,Y=Y",
            "Comma/semicolon separated alias mapping: alias=UnityKeyCode (ex: heal=H,slot1=Alpha1).");
    }

    private static void LogStartupSummary()
    {
        Logger.LogInfo("=== ExtraSlots Startup Summary ===");
        Logger.LogInfo($"  Enabled: {GeneralEnabled?.Value ?? true}");
        Logger.LogInfo($"  Debug Mode: {DebugMode?.Value ?? false}");
        Logger.LogInfo($"  Binds RestrictToAliases: {BindRestrictToAliases?.Value ?? false}");
        Logger.LogInfo("==================================");
    }

}
