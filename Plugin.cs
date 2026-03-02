using System;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using VampireCommandFramework;
using VAutomationCore;
using VAuto.Core;
namespace VAutomationCore
{
    [BepInPlugin(MyPluginInfo.GUID, MyPluginInfo.NAME, MyPluginInfo.VERSION)]
    [BepInDependency("gg.deca.VampireCommandFramework", "0.10.4")]
    [BepInProcess("VRisingServer.exe")]
    [BepInProcess("VRising.exe")]
    public class Plugin : BasePlugin
    {
        private static ManualLogSource _log;
        public new static ManualLogSource Log => _log ??= BepInEx.Logging.Logger.CreateLogSource(MyPluginInfo.NAME);
        private Harmony _harmony;
        private static ConfigFile _configFile;
        private static ConfigEntry<bool> _configEnabled;

        public override void Load()
        {
            try
            {
                _configFile = new ConfigFile(Path.Combine(Paths.ConfigPath, "VAuto.Core.cfg"), true);
                _configEnabled = _configFile.Bind("General", "Enabled", true, "Enable or disable VAuto Core plugin.");
                if (_configEnabled != null && !_configEnabled.Value)
                {
                    Log.LogInfo("[VAutomationCore] Disabled via config.");
                    return;
                }

                Log.LogInfo($"[{MyPluginInfo.NAME}] Loading {MyPluginInfo.VERSION}...");

                Log.LogInfo($"[{MyPluginInfo.NAME}] Loaded core shared library.");
                LogStartupSummary();

                // Initialize PlayerAPI endpoints
                RegisterPlayerApiEndpoints();

                // Initialize Harmony for patching
                _harmony = new Harmony(MyPluginInfo.GUID);
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
                if (_harmony != null)
                {
<<<<<<< Updated upstream
                    var unpatchAll = typeof(Harmony).GetMethod("UnpatchAll", new[] { typeof(string) });
                    unpatchAll?.Invoke(_harmony, new object[] { MyPluginInfo.GUID });
=======
                    _harmony.UnpatchSelf();
                    _harmony = null;
                }

                if (_configFile != null)
                {
                    _configFile.Save();
                    _configFile = null;
                    _configEnabled = null;
>>>>>>> Stashed changes
                }
            }
            catch (Exception ex)
            {
                Log.LogError(ex);
            }

            return true;
        }

        private static void LogStartupSummary()
        {
            var cfgPath = Path.Combine(Paths.ConfigPath, "VAuto.Core.cfg");
            Log.LogInfo($"[{MyPluginInfo.NAME}] Startup Summary:");
            Log.LogInfo($"[{MyPluginInfo.NAME}]   Config: {cfgPath}");
            Log.LogInfo($"[{MyPluginInfo.NAME}]   Processes: VRisingServer.exe, VRising.exe");
        }

        private void RegisterPlayerApiEndpoints()
        {
            try
            {
                // Register PlayerAPI endpoints with VRisingServerApiPlugin
                // Note: This requires VRisingServerApiPlugin to be installed and loaded
                // ZUtility.API reference commented out - requires external dependency
                // var playerApi = new ZUtility.API.PlayerAPI();
                
                // Register endpoints - adjust based on actual VRisingServerApiPlugin API
                // Example registration (actual method depends on the plugin):
                // VRisingServerApiPlugin.RegisterEndpoint(playerApi);
                
                Log.LogInfo("[PlayerAPI] Endpoints would be registered (ZUtility not available)");
            }
            catch (Exception ex)
            {
                Log.LogWarning($"[PlayerAPI] Failed to register endpoints: {ex.Message}");
            }
        }
    }
}
