using Blueluck.Services;
using HarmonyLib;
using ProjectM;

namespace Blueluck.Patches
{
    [HarmonyPatch(typeof(ServerBootstrapSystem), nameof(ServerBootstrapSystem.OnUpdate))]
    internal static class FallbackZoneDetectionPatch
    {
        [HarmonyPostfix]
        private static void OnUpdatePostfix()
        {
            FallbackZoneDetectionService.ProcessTick();
        }
    }
}
