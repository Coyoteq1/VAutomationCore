using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Xunit;

namespace Bluelock.Tests
{
    public class ConfigSemanticTests
    {
        [Fact]
        public void ZonesConfig_HasUniqueIds_AndValidRadii()
        {
            var repoRoot = ResolveRepoRoot();
            var path = Path.Combine(repoRoot, "Bluelock", "config", "VAuto.Zones.json");
            Assert.True(File.Exists(path), "VAuto.Zones.json missing");

            using var doc = JsonDocument.Parse(File.ReadAllText(path));
            var zones = doc.RootElement.GetProperty("zones");

            var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var zone in zones.EnumerateArray())
            {
                var id = zone.GetProperty("id").GetString() ?? string.Empty;
                Assert.False(string.IsNullOrWhiteSpace(id), "Zone id is empty.");
                Assert.True(ids.Add(id), $"Duplicate zone id detected: {id}");

                var radius = zone.TryGetProperty("radius", out var radiusProp) ? radiusProp.GetDouble() : 0;
                var entryRadius = zone.TryGetProperty("entryRadius", out var entryProp) ? entryProp.GetDouble() : radius;
                var exitRadius = zone.TryGetProperty("exitRadius", out var exitProp) ? exitProp.GetDouble() : radius;

                Assert.True(entryRadius <= exitRadius, $"Zone {id} has entryRadius > exitRadius");
            }
        }

        [Fact]
        public void ZoneLifecycleConfig_ReferencesExistingFlowFiles()
        {
            var repoRoot = ResolveRepoRoot();
            var zonesPath = Path.Combine(repoRoot, "Bluelock", "config", "VAuto.Zones.json");
            var flowDir = Path.Combine(repoRoot, "Bluelock", "config", "flows");
            Assert.True(File.Exists(zonesPath), "VAuto.Zones.json missing");

            using var zonesDoc = JsonDocument.Parse(File.ReadAllText(zonesPath));
            var zones = zonesDoc.RootElement.GetProperty("zones");

            foreach (var zone in zones.EnumerateArray())
            {
                if (!zone.TryGetProperty("flowId", out var flowIdProp))
                {
                    continue;
                }

                var flowId = flowIdProp.GetString();
                if (string.IsNullOrWhiteSpace(flowId))
                {
                    continue;
                }

                var flowPath = Path.Combine(flowDir, flowId + ".json");
                Assert.True(File.Exists(flowPath), $"Missing flow file '{flowId}.json' for zone '{zone.GetProperty("id").GetString()}'");
                JsonDocument.Parse(File.ReadAllText(flowPath));
            }
        }

        private static string ResolveRepoRoot()
        {
            var dir = Directory.GetCurrentDirectory();
            while (!string.IsNullOrWhiteSpace(dir))
            {
                if (File.Exists(Path.Combine(dir, "VAutomationCore.csproj")))
                {
                    return dir;
                }

                dir = Directory.GetParent(dir)?.FullName;
            }

            throw new InvalidOperationException("Repository root not found.");
        }
    }
}
