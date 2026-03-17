using Unity.Entities;

namespace VAutomationCore.Core.ECS.Components
{
    public struct ZoneTransitionEvent
    {
        public Entity Player;
        public int OldZoneHash;
        public int NewZoneHash;
    }
}
