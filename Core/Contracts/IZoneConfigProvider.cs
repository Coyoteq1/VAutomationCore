using System.Collections.Generic;
using Unity.Mathematics;

namespace VAutomationCore.Core.Contracts
{
    public readonly struct ZoneRuntimeDescriptor
    {
        public ZoneRuntimeDescriptor(
            int zoneHash,
            string zoneId,
            float3 center,
            float entryRadiusSq,
            float exitRadiusSq,
            int priority,
            string flowId)
        {
            ZoneHash = zoneHash;
            ZoneId = zoneId ?? string.Empty;
            Center = center;
            EntryRadiusSq = entryRadiusSq;
            ExitRadiusSq = exitRadiusSq;
            Priority = priority;
            FlowId = flowId ?? string.Empty;
        }

        public int ZoneHash { get; }
        public string ZoneId { get; }
        public float3 Center { get; }
        public float EntryRadiusSq { get; }
        public float ExitRadiusSq { get; }
        public int Priority { get; }
        public string FlowId { get; }
    }

    public interface IZoneConfigProvider
    {
        IReadOnlyList<ZoneRuntimeDescriptor> GetZones();
        bool TryGetZoneByHash(int zoneHash, out ZoneRuntimeDescriptor zone);
    }
}
