using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Unity.Entities;
using Unity.Collections;
using ProjectM;
using VampireCommandFramework;
using ExtraSlots.Systems;
using ExtraSlots.Models;

[BepInPlugin("gg.coyote.ExtraSlots", "ExtraSlots", "1.0.0")]
[BepInDependency("gg.coyote.VAutomationCore", "1.0.0")]
[BepInDependency("gg.deca.VampireCommandFramework", "0.10.4")]
public class ExtraSlotsPlugin : BasePlugin
{
    public static ExtraSlotsPlugin Instance { get; private set; }
    public static ManualLogSource Log;
    public static Harmony Harmony;
    public static EntityManager EM;
    
    public override void Load()
    {
        Instance = this;
        Log = Log;
        Harmony = new Harmony("gg.coyote.ExtraSlots");
        
        // Initialize services
        ExtraSlotsService.Initialize();
        
        // Apply patches
        Harmony.PatchAll(typeof(ExtraSlotsPlugin).Assembly);
        
        Log.LogInfo("ExtraSlots loaded!");
    }
    
    [BepInDependency("gg.coyote.lifecycle", "1.0.0")]
    public override void Update()
    {
        // Update service
    }
}
