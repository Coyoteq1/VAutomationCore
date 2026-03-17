using EcsZoneState = VAutomationCore.Core.ECS.Components.EcsPlayerZoneState;
using ModelZoneState = VAutomationCore.Models.PlayerZoneState;

namespace VAutomationCore.Core.ECS
{
    public static class ZoneStateMapper
    {
        public static ModelZoneState ToModel(EcsZoneState component, System.Func<int, string> zoneIdResolver)
        {
            var zoneId = zoneIdResolver?.Invoke(component.CurrentZoneHash) ?? string.Empty;
            return new ModelZoneState
            {
                CurrentZoneId = zoneId,
                PreviousZoneId = string.Empty,
                WasInZone = component.CurrentZoneHash != 0,
                IsInAnyZone = component.CurrentZoneHash != 0
            };
        }

        public static EcsZoneState ToComponent(ModelZoneState model, System.Func<string, int> zoneHashResolver)
        {
            var zoneHash = zoneHashResolver?.Invoke(model?.CurrentZoneId ?? string.Empty) ?? 0;
            return new EcsZoneState
            {
                CurrentZoneHash = zoneHash
            };
        }
    }
}
