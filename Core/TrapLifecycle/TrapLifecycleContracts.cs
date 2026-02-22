using Unity.Mathematics;

namespace VAutomationCore.Core.TrapLifecycle
{
    /// <summary>
    /// Context passed into trap lifecycle policy checks.
    /// </summary>
    public struct TrapLifecycleContext
    {
        public ulong CharacterId;
        public string ZoneId;
        public float3 Position;
        public string LifecycleStage;
    }

    /// <summary>
    /// Result of evaluating a trap lifecycle policy decision.
    /// </summary>
    public struct TrapLifecycleDecision
    {
        public bool OverrideTriggered;
        public bool ForceBuffClearOnExit;
        public string Reason;

        public static TrapLifecycleDecision None(string reason = "No override")
        {
            return new TrapLifecycleDecision
            {
                OverrideTriggered = false,
                ForceBuffClearOnExit = false,
                Reason = reason ?? "No override"
            };
        }
    }

    /// <summary>
    /// Shared contract for trap lifecycle policy providers.
    /// </summary>
    public interface ITrapLifecyclePolicy
    {
        bool IsEnabled { get; }
        TrapLifecycleDecision OnBeforeLifecycleEnter(TrapLifecycleContext ctx);
        TrapLifecycleDecision OnBeforeLifecycleExit(TrapLifecycleContext ctx);
    }
}
