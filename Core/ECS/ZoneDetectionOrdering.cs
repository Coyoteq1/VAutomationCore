using VAutomationCore.Core.ECS.Components;

namespace VAutomationCore.Core.ECS
{
    public static class ZoneDetectionOrdering
    {
        public static int Compare(ZoneComponent a, ZoneComponent b)
        {
            var byPriority = b.Priority.CompareTo(a.Priority);
            if (byPriority != 0)
            {
                return byPriority;
            }

            var byEntryRadius = b.EntryRadiusSq.CompareTo(a.EntryRadiusSq);
            if (byEntryRadius != 0)
            {
                return byEntryRadius;
            }

            return a.ZoneHash.CompareTo(b.ZoneHash);
        }
    }
}
