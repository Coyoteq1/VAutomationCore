using Unity.Entities;

namespace VAutomationCore.Core.Events
{
    public sealed class VisibilityModifiedEvent
    {
        public Entity Target { get; init; }
        public float VisibilityLevel { get; init; }
    }

    public sealed class DetectionTriggeredEvent
    {
        public Entity Detector { get; init; }
        public Entity Detected { get; init; }
        public string DetectionType { get; init; } = string.Empty;
    }

    public sealed class DetectionRangeChangedEvent
    {
        public Entity Target { get; init; }
        public float Range { get; init; }
    }

    public sealed class LineOfSightCheckedEvent
    {
        public Entity Observer { get; init; }
        public Entity Target { get; init; }
        public bool HasLineOfSight { get; init; }
    }
}
