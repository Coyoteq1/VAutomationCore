using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using VAutomationCore.Core.Api;
using Xunit;

namespace Bluelock.Tests
{
    public sealed class RequiresBepInExCoreFactAttribute : FactAttribute
    {
        public RequiresBepInExCoreFactAttribute()
        {
            try
            {
                Assembly.Load("BepInEx.Core");
            }
            catch
            {
                Skip = "Requires BepInEx.Core runtime assembly.";
            }
        }
    }

    public class RoleAuthJobFlowTests
    {
        [RequiresBepInExCoreFact]
        public void ExecuteJobFlow_RequiresDeveloperRole()
        {
            ConfigureAuthForTest(enabled: true);
            try
            {
                const ulong adminSubject = 1001;
                const ulong developerSubject = 1002;

                SeedSession(adminSubject, ConsoleRoleAuthService.ConsoleRole.Admin);
                SeedSession(developerSubject, ConsoleRoleAuthService.ConsoleRole.Developer);

                var entityMap = new EntityMap();

                var adminResult = FlowService.ExecuteJobFlow("missing_flow", entityMap, adminSubject);
                Assert.False(adminResult.Success);
                Assert.Contains("Developer authorization required", adminResult.ErrorMessage, StringComparison.OrdinalIgnoreCase);

                var developerResult = FlowService.ExecuteJobFlow("missing_flow", entityMap, developerSubject);
                Assert.False(developerResult.Success);
                Assert.DoesNotContain("Developer authorization required", developerResult.ErrorMessage, StringComparison.OrdinalIgnoreCase);
            }
            finally
            {
                ResetAuthState();
            }
        }

        private static void ConfigureAuthForTest(bool enabled)
        {
            var authType = typeof(ConsoleRoleAuthService);
            var configType = authType.GetNestedType("ConsoleRoleAuthConfig", BindingFlags.NonPublic)
                             ?? throw new InvalidOperationException("ConsoleRoleAuthConfig type not found.");

            var config = Activator.CreateInstance(configType)
                         ?? throw new InvalidOperationException("Failed to create ConsoleRoleAuthConfig instance.");
            configType.GetProperty("Enabled")?.SetValue(config, enabled);
            configType.GetProperty("SessionMinutes")?.SetValue(config, 30);
            configType.GetProperty("AdminPasswordHashes")?.SetValue(config, new List<string>());
            configType.GetProperty("DeveloperPasswordHashes")?.SetValue(config, new List<string>());

            authType.GetField("_config", BindingFlags.NonPublic | BindingFlags.Static)?.SetValue(null, config);
            authType.GetField("_initialized", BindingFlags.NonPublic | BindingFlags.Static)?.SetValue(null, true);

            ClearSessions();
        }

        private static void SeedSession(ulong subjectId, ConsoleRoleAuthService.ConsoleRole role)
        {
            var authType = typeof(ConsoleRoleAuthService);
            var sessionType = authType.GetNestedType("SessionState", BindingFlags.NonPublic)
                              ?? throw new InvalidOperationException("SessionState type not found.");
            var session = Activator.CreateInstance(sessionType)
                          ?? throw new InvalidOperationException("Failed to create SessionState instance.");

            sessionType.GetField("<Role>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(session, role);
            sessionType.GetField("<ExpiresAtUtc>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(session, DateTime.UtcNow.AddMinutes(10));

            var sessionsField = authType.GetField("Sessions", BindingFlags.NonPublic | BindingFlags.Static)
                               ?? throw new InvalidOperationException("Sessions field not found.");
            var sessions = sessionsField.GetValue(null) as IDictionary
                           ?? throw new InvalidOperationException("Sessions dictionary not available.");

            sessions[subjectId] = session;
        }

        private static void ResetAuthState()
        {
            ClearSessions();
            typeof(ConsoleRoleAuthService).GetField("_initialized", BindingFlags.NonPublic | BindingFlags.Static)
                ?.SetValue(null, false);
        }

        private static void ClearSessions()
        {
            var sessionsField = typeof(ConsoleRoleAuthService).GetField("Sessions", BindingFlags.NonPublic | BindingFlags.Static)
                               ?? throw new InvalidOperationException("Sessions field not found.");
            var sessions = sessionsField.GetValue(null) as IDictionary
                           ?? throw new InvalidOperationException("Sessions dictionary not available.");
            sessions.Clear();
        }
    }
}
