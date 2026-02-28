using System.Collections.Generic;
using VAutomationCore.Core.ECS;
using VAutomationCore.Core.ECS.Components;
using Xunit;

namespace Bluelock.Tests
{
    public class ZoneDetectionOrderingTests
    {
        [Fact]
        public void Compare_SortsByPriorityThenEntryRadiusThenZoneHash()
        {
            var zones = new List<ZoneComponent>
            {
                new ZoneComponent { ZoneHash = 30, Priority = 1, EntryRadiusSq = 100f },
                new ZoneComponent { ZoneHash = 20, Priority = 2, EntryRadiusSq = 25f },
                new ZoneComponent { ZoneHash = 10, Priority = 2, EntryRadiusSq = 25f },
                new ZoneComponent { ZoneHash = 40, Priority = 2, EntryRadiusSq = 225f },
            };

            zones.Sort(ZoneDetectionOrdering.Compare);

            Assert.Equal(40, zones[0].ZoneHash); // highest priority, largest radius
            Assert.Equal(10, zones[1].ZoneHash); // same priority/radius as hash 20, lower hash first
            Assert.Equal(20, zones[2].ZoneHash);
            Assert.Equal(30, zones[3].ZoneHash); // lower priority
        }
    }
}
