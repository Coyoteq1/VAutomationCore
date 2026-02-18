using System;
using System.IO;
using System.Linq;
using System.Text;
using VAuto.Core.Services;
using Xunit;

namespace Bluelock.Tests
{
    public class SandboxSnapshotTests
    {
        [Fact]
        public void CsvRoundTrip_BaselineAndDelta_WritesAndReadsRows()
        {
            var tempRoot = Path.Combine(Path.GetTempPath(), "sandbox-snapshot-tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempRoot);

            try
            {
                var baselinePath = Path.Combine(tempRoot, "baseline.csv.gz");
                var deltaPath = Path.Combine(tempRoot, "delta.csv.gz");
                var now = DateTime.UtcNow;

                var baselineRows = new[]
                {
                    new BaselineRow
                    {
                        Version = 1,
                        SnapshotId = "snap-1",
                        PlayerKey = "alice",
                        CharacterName = "Alice",
                        PlatformId = 42UL,
                        ZoneId = "sandbox_alpha",
                        CapturedUtc = now,
                        RowType = "component",
                        ComponentType = "ResearchComponent",
                        AssemblyQualifiedType = "ProjectM.ResearchComponent, ProjectM",
                        Existed = true,
                        PayloadBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes("{\"guid\":123}")),
                        PayloadHash = "ABC"
                    }
                };

                var deltaRows = new[]
                {
                    new DeltaRow
                    {
                        Version = 1,
                        SnapshotId = "snap-1",
                        PlayerKey = "alice",
                        CharacterName = "Alice",
                        PlatformId = 42UL,
                        ZoneId = "sandbox_alpha",
                        CapturedUtc = now,
                        RowType = "component_changed",
                        Operation = "changed",
                        ComponentType = "ResearchComponent",
                        BeforePayloadBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes("{\"guid\":123}")),
                        AfterPayloadBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes("{\"guid\":456}")),
                        TechGuid = 456,
                        TechName = "GUID:456",
                        EntityIndex = 12,
                        EntityVersion = 1,
                        PrefabGuid = 99,
                        PrefabName = "Prefab_99",
                        PosX = 1.5f,
                        PosY = 2.5f,
                        PosZ = 3.5f
                    }
                };

                SandboxCsvWriter.WriteBaseline(baselinePath, baselineRows);
                SandboxCsvWriter.WriteDelta(deltaPath, deltaRows);

                var baselineRoundTrip = SandboxCsvWriter.ReadBaseline(baselinePath);
                var deltaRoundTrip = SandboxCsvWriter.ReadDelta(deltaPath);

                Assert.Single(baselineRoundTrip);
                Assert.Single(deltaRoundTrip);
                Assert.Equal("alice", baselineRoundTrip[0].PlayerKey);
                Assert.Equal("sandbox_alpha", baselineRoundTrip[0].ZoneId);
                Assert.Equal("component_changed", deltaRoundTrip[0].RowType);
                Assert.Equal(456L, deltaRoundTrip[0].TechGuid);
            }
            finally
            {
                if (Directory.Exists(tempRoot))
                {
                    Directory.Delete(tempRoot, recursive: true);
                }
            }
        }

        [Fact]
        public void DeltaComputer_DetectsComponentAndEntityChanges()
        {
            var preComponents = new[]
            {
                new BaselineRow
                {
                    AssemblyQualifiedType = "ProjectM.ResearchComponent, ProjectM",
                    ComponentType = "ResearchComponent",
                    Existed = true,
                    PayloadBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes("{\"guid\":111}"))
                }
            };

            var postComponents = new[]
            {
                new BaselineRow
                {
                    AssemblyQualifiedType = "ProjectM.ResearchComponent, ProjectM",
                    ComponentType = "ResearchComponent",
                    Existed = true,
                    PayloadBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes("{\"guid\":222}"))
                }
            };

            var componentDelta = SandboxDeltaComputer.ComputeComponentDelta(preComponents, postComponents).ToArray();
            Assert.Single(componentDelta);
            Assert.Equal("component_changed", componentDelta[0].RowType);
            Assert.Equal("changed", componentDelta[0].Operation);

            var preEntities = new[]
            {
                new ZoneEntityEntry { EntityIndex = 1, EntityVersion = 1, PrefabGuidHash = 10, PrefabName = "A" },
                new ZoneEntityEntry { EntityIndex = 2, EntityVersion = 1, PrefabGuidHash = 20, PrefabName = "B" }
            };
            var postEntities = new[]
            {
                new ZoneEntityEntry { EntityIndex = 1, EntityVersion = 1, PrefabGuidHash = 11, PrefabName = "A2" },
                new ZoneEntityEntry { EntityIndex = 3, EntityVersion = 1, PrefabGuidHash = 30, PrefabName = "C" }
            };

            var entityDelta = SandboxDeltaComputer.ComputeEntityDelta(preEntities, postEntities).ToArray();
            Assert.Contains(entityDelta, row => row.RowType == "entity_created" && row.EntityIndex == 3);
            Assert.Contains(entityDelta, row => row.RowType == "entity_removed" && row.EntityIndex == 2);
            Assert.Contains(entityDelta, row => row.RowType == "entity_prefab_changed" && row.EntityIndex == 1);
        }

        [Fact]
        public void DeltaComputer_ExtractsOpenedTechFromPayloadDiff()
        {
            var pre = new[]
            {
                new BaselineRow
                {
                    PayloadBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes("{\"items\":[{\"guid\":1001}]}"))
                }
            };
            var post = new[]
            {
                new BaselineRow
                {
                    PayloadBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes("{\"items\":[{\"guid\":1001},{\"guid\":2002}]}"))
                }
            };

            var techDelta = SandboxDeltaComputer.ExtractOpenedTech(pre, post).ToArray();
            Assert.Contains(techDelta, row => row.RowType == "tech_opened" && row.TechGuid == 2002);
            Assert.DoesNotContain(techDelta, row => row.TechGuid == 1001);
        }

        [Fact]
        public void SnapshotStore_TracksPendingActiveAndDirtyState()
        {
            SandboxSnapshotStore.ClearAll();

            var pending = new SandboxPendingContext
            {
                CharacterName = "Alice",
                PlatformId = 42UL,
                ZoneId = "sandbox_alpha",
                SnapshotId = "snap-1",
                CapturedUtc = DateTime.UtcNow
            };

            var key = SandboxSnapshotStore.UpsertPendingContext(pending);
            Assert.False(string.IsNullOrWhiteSpace(key));

            var taken = SandboxSnapshotStore.TryTakePendingContext("Alice", 42UL, out var takenKey, out var takenContext);
            Assert.True(taken);
            Assert.Equal(key, takenKey);
            Assert.NotNull(takenContext);

            var baseline = new SandboxBaselineSnapshot
            {
                PlayerKey = key,
                CharacterName = "Alice",
                PlatformId = 42UL,
                ZoneId = "sandbox_alpha",
                SnapshotId = "snap-1",
                CapturedUtc = DateTime.UtcNow,
                Rows = Array.Empty<BaselineRow>()
            };
            var delta = new SandboxDeltaSnapshot
            {
                PlayerKey = key,
                CharacterName = "Alice",
                PlatformId = 42UL,
                ZoneId = "sandbox_alpha",
                SnapshotId = "snap-1",
                CapturedUtc = DateTime.UtcNow,
                Rows = Array.Empty<DeltaRow>()
            };

            SandboxSnapshotStore.PutActiveSnapshots(key, baseline, delta);
            Assert.True(SandboxSnapshotStore.TryGetActiveSnapshots("Alice", 42UL, out _, out var activeBaseline, out var activeDelta));
            Assert.NotNull(activeBaseline);
            Assert.NotNull(activeDelta);

            SandboxSnapshotStore.MarkDirty();
            Assert.True(SandboxSnapshotStore.IsDirty);
            SandboxSnapshotStore.MarkClean();
            Assert.False(SandboxSnapshotStore.IsDirty);
        }
    }
}
