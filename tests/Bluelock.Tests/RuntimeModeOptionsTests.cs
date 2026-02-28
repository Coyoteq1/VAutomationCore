using VAutomationCore.Core.Lifecycle;
using Xunit;

namespace Bluelock.Tests
{
    public class RuntimeModeOptionsTests
    {
        [Fact]
        public void Legacy_Mode_MapsToLegacyOnly()
        {
            var options = ZoneRuntimeModeOptions.FromMode(ZoneRuntimeMode.Legacy);
            Assert.Equal(ZoneRuntimeMode.Legacy, options.RuntimeMode);
            Assert.False(options.EcsZoneDetectionEnabled);
            Assert.False(options.EcsFlowExecutionEnabled);
            Assert.False(options.EcsTemplateLifecycleEnabled);
            Assert.True(options.LegacyZonePipelineEnabled);
        }

        [Fact]
        public void Hybrid_Mode_MapsToMixedPipeline()
        {
            var options = ZoneRuntimeModeOptions.FromMode(ZoneRuntimeMode.Hybrid);
            Assert.Equal(ZoneRuntimeMode.Hybrid, options.RuntimeMode);
            Assert.True(options.EcsZoneDetectionEnabled);
            Assert.True(options.EcsFlowExecutionEnabled);
            Assert.True(options.EcsTemplateLifecycleEnabled);
            Assert.True(options.LegacyZonePipelineEnabled);
        }

        [Fact]
        public void EcsOnly_Mode_MapsToEcsPipelineOnly()
        {
            var options = ZoneRuntimeModeOptions.FromMode(ZoneRuntimeMode.EcsOnly);
            Assert.Equal(ZoneRuntimeMode.EcsOnly, options.RuntimeMode);
            Assert.True(options.EcsZoneDetectionEnabled);
            Assert.True(options.EcsFlowExecutionEnabled);
            Assert.True(options.EcsTemplateLifecycleEnabled);
            Assert.False(options.LegacyZonePipelineEnabled);
        }
    }
}
