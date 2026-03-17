using Unity.Mathematics;

namespace VAutomationCore.Core.ECS.Components
{
    public struct ZoneComponent
    {
        public int ZoneHash;
        public int Priority;
        public float3 Center;
        public float EntryRadius;
        public float ExitRadius;
        public float EntryRadiusSq;
        public float ExitRadiusSq;
    }
}
