using System;

namespace VAutomationCore.Core.Lifecycle
{
    public sealed class ZoneRuntimeModeOptions
    {
        public ZoneRuntimeMode RuntimeMode { get; set; } = ZoneRuntimeMode.Legacy;
        public bool EcsZoneDetectionEnabled { get; set; }
        public bool EcsFlowExecutionEnabled { get; set; }
        public bool EcsTemplateLifecycleEnabled { get; set; }
        public bool LegacyZonePipelineEnabled { get; set; } = true;

        public static ZoneRuntimeModeOptions FromMode(ZoneRuntimeMode mode)
        {
            return mode switch
            {
                ZoneRuntimeMode.Legacy => new ZoneRuntimeModeOptions
                {
                    RuntimeMode = ZoneRuntimeMode.Legacy,
                    EcsZoneDetectionEnabled = false,
                    EcsFlowExecutionEnabled = false,
                    EcsTemplateLifecycleEnabled = false,
                    LegacyZonePipelineEnabled = true
                },
                ZoneRuntimeMode.Hybrid => new ZoneRuntimeModeOptions
                {
                    RuntimeMode = ZoneRuntimeMode.Hybrid,
                    EcsZoneDetectionEnabled = true,
                    EcsFlowExecutionEnabled = true,
                    EcsTemplateLifecycleEnabled = true,
                    LegacyZonePipelineEnabled = true
                },
                ZoneRuntimeMode.EcsOnly => new ZoneRuntimeModeOptions
                {
                    RuntimeMode = ZoneRuntimeMode.EcsOnly,
                    EcsZoneDetectionEnabled = true,
                    EcsFlowExecutionEnabled = true,
                    EcsTemplateLifecycleEnabled = true,
                    LegacyZonePipelineEnabled = false
                },
                _ => throw new InvalidOperationException("Invalid zone runtime mode.")
            };
        }
    }
}
