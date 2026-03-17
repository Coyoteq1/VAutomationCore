using Unity.Entities;

namespace VAutomationCore.Core.Events
{
    public sealed class PlayerEnteredZoneEvent
    {
        public Entity Player { get; init; }
        public string ZoneId { get; init; } = string.Empty;
    }

    public sealed class PlayerExitedZoneEvent
    {
        public Entity Player { get; init; }
        public string ZoneId { get; init; } = string.Empty;
    }

    public sealed class PlayerDownedEvent
    {
        public Entity Player { get; init; }
        public Entity Attacker { get; init; }
    }

    public sealed class BossKilledEvent
    {
        public Entity Killer { get; init; }
        public Entity Boss { get; init; }
    }
}
