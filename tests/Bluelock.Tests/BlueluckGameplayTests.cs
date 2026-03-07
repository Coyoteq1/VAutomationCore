using System;
using System.Threading;
using Blueluck.Models;
using Blueluck.Services;
using Xunit;

namespace Bluelock.Tests
{
    public class BlueluckGameplayTests
    {
        [Fact]
        public void Resolve_UsesResolvedPresetSessionDefinition()
        {
            var service = new GamePresetService();
            service.Initialize();

            var zone = new ArenaZoneConfig
            {
                Hash = 2001,
                Name = "Test Arena",
                RespawnEnabled = false,
                ResolvedPresets = new[]
                {
                    new GameplayPresetConfig
                    {
                        PresetId = "arena_pvp_core",
                        Session = new GameSessionConfig
                        {
                            Enabled = true,
                            MinPlayers = 2,
                            CountdownSeconds = 8
                        },
                        SessionLifecycle = new SessionLifecycleConfig
                        {
                            LateJoinFlows = new[] { "arena_late_join" }
                        },
                        Objective = new GameObjectiveConfig
                        {
                            ObjectiveType = "last_player_standing"
                        }
                    }
                }
            };

            var resolved = service.Resolve(zone);

            Assert.True(resolved.Session.Enabled);
            Assert.Equal(2, resolved.Session.MinPlayers);
            Assert.Equal(8, resolved.Session.CountdownSeconds);
            Assert.Equal("last_player_standing", resolved.Objective.ObjectiveType);
            Assert.Contains("arena_late_join", resolved.Lifecycle.LateJoinFlows);
            Assert.True(resolved.IsValid);
        }

        [Fact]
        public void SessionTimer_ExecutesOnlyAfterProcessTick()
        {
            var service = new SessionTimerService();
            service.Initialize();

            var session = new GameSession { ZoneHash = 2001, ZoneName = "Arena" };
            var executions = 0;
            var eventId = service.Schedule(session, "delayed", TimeSpan.FromMilliseconds(10), () => Interlocked.Increment(ref executions));

            Assert.NotNull(eventId);
            Assert.Equal(0, executions);

            Thread.Sleep(30);
            Assert.Equal(0, executions);

            service.ProcessTick();

            Assert.Equal(1, executions);
        }
    }
}
