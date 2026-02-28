using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace Bluelock.Tests
{
    public class PlatformContractTests
    {
        [Fact]
        public void Router_DispatchesBeforeDestroyingTransitionEvent()
        {
            var repoRoot = ResolveRepoRoot();
            var routerPath = Path.Combine(repoRoot, "Bluelock", "Systems", "ZoneTransitionRouterSystem.cs");
            Assert.True(File.Exists(routerPath), "ZoneTransitionRouterSystem.cs missing.");

            var text = File.ReadAllText(routerPath);
            var dispatchIndex = text.IndexOf("Dispatch(transition);", StringComparison.Ordinal);
            var destroyIndex = text.IndexOf("DestroyEntity(evtEntity);", StringComparison.Ordinal);

            Assert.True(dispatchIndex >= 0, "Router does not dispatch transition events.");
            Assert.True(destroyIndex >= 0, "Router does not destroy transition entities.");
            Assert.True(dispatchIndex < destroyIndex, "Router destroys event before dispatching transition.");
        }

        [Fact]
        public void LifecycleConfigs_DeclareSchemaVersion()
        {
            var repoRoot = ResolveRepoRoot();
            var blueLockConfig = Path.Combine(repoRoot, "Bluelock", "config", "VAuto.ZoneLifecycle.json");
            var cycleBornPlugin = Path.Combine(repoRoot, "CycleBorn", "Plugin.cs");

            Assert.True(File.Exists(blueLockConfig), "VAuto.ZoneLifecycle.json missing.");
            Assert.True(File.Exists(cycleBornPlugin), "CycleBorn/Plugin.cs missing.");

            var blText = File.ReadAllText(blueLockConfig);
            var cycleText = File.ReadAllText(cycleBornPlugin);

            Assert.Contains("\"schemaVersion\"", blText, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("SchemaVersion", cycleText, StringComparison.Ordinal);
            Assert.Contains("CurrentConfigVersion", cycleText, StringComparison.Ordinal);
        }

        [Fact]
        public void ZoneLifecycleVersionMigration_BackfillsVersionAndSchema()
        {
            var repoRoot = ResolveRepoRoot();
            var migrationPath = Path.Combine(repoRoot, "Bluelock", "Services", "ZoneLifecycleConfigVersionMigration.cs");
            Assert.True(File.Exists(migrationPath), "ZoneLifecycleConfigVersionMigration.cs missing.");

            var text = File.ReadAllText(migrationPath);
            Assert.Contains("SourceVersion => \"1.0.0\"", text, StringComparison.Ordinal);
            Assert.Contains("TargetVersion => ZoneJsonConfig.CurrentConfigVersion", text, StringComparison.Ordinal);
            Assert.Contains("string.IsNullOrWhiteSpace(config.ConfigVersion)", text, StringComparison.Ordinal);
            Assert.Contains("config.ConfigVersion = TargetVersion", text, StringComparison.Ordinal);
            Assert.Contains("string.IsNullOrWhiteSpace(config.SchemaVersion)", text, StringComparison.Ordinal);
            Assert.Contains("config.SchemaVersion = TargetVersion", text, StringComparison.Ordinal);
        }

        [Fact]
        public void Plugin_AppliesLifecycleMigrations_AndLocksRuntimeModeAtBoot()
        {
            var repoRoot = ResolveRepoRoot();
            var pluginPath = Path.Combine(repoRoot, "Bluelock", "Plugin.cs");
            Assert.True(File.Exists(pluginPath), "Bluelock/Plugin.cs missing.");

            var text = File.ReadAllText(pluginPath);
            Assert.Contains("ApplyZoneLifecycleMigrations(_jsonConfig)", text, StringComparison.Ordinal);
            Assert.Contains("_bootRuntimeModeLocked = true;", text, StringComparison.Ordinal);
            Assert.Contains("Runtime mode hot-reload change ignored", text, StringComparison.Ordinal);

            var hotReloadMethodStart = text.IndexOf("private void CheckForConfigChanges()", StringComparison.Ordinal);
            var hotReloadMethodEnd = text.IndexOf("private static void LogConfigValidationResult", StringComparison.Ordinal);
            Assert.True(hotReloadMethodStart >= 0 && hotReloadMethodEnd > hotReloadMethodStart,
                "Unable to locate CheckForConfigChanges method body.");

            var hotReloadMethod = text.Substring(hotReloadMethodStart, hotReloadMethodEnd - hotReloadMethodStart);
            Assert.DoesNotContain("_bootRuntimeMode =", hotReloadMethod, StringComparison.Ordinal);
        }

        [Fact]
        public void Plugin_RuntimeModeOptions_AreConfigDriven_NotForcedEcsOnly()
        {
            var repoRoot = ResolveRepoRoot();
            var pluginPath = Path.Combine(repoRoot, "Bluelock", "Plugin.cs");
            Assert.True(File.Exists(pluginPath), "Bluelock/Plugin.cs missing.");

            var text = File.ReadAllText(pluginPath);
            Assert.Contains("ZoneRuntimeModeOptions.FromMode(RuntimeModeValue)", text, StringComparison.Ordinal);
            Assert.DoesNotContain("ZoneRuntimeModeOptions.FromMode(ZoneRuntimeMode.EcsOnly)", text, StringComparison.Ordinal);
        }

        [Fact]
        public void Lifecycle_ActionChain_UsesParameterizedBossEnter_AndClearTemplateExit()
        {
            var repoRoot = ResolveRepoRoot();
            var pluginPath = Path.Combine(repoRoot, "Bluelock", "Plugin.cs");
            var configPath = Path.Combine(repoRoot, "Bluelock", "config", "VAuto.ZoneLifecycle.json");
            Assert.True(File.Exists(pluginPath), "Bluelock/Plugin.cs missing.");
            Assert.True(File.Exists(configPath), "Bluelock/config/VAuto.ZoneLifecycle.json missing.");

            var pluginText = File.ReadAllText(pluginPath);
            var configText = File.ReadAllText(configPath);

            Assert.Contains("case \"boss_enter\":", pluginText, StringComparison.Ordinal);
            Assert.Contains("TrySpawnTemplateManifest(parameter, zoneId, \"boss\", em)", pluginText, StringComparison.Ordinal);
            Assert.Contains("case \"clear_template\":", pluginText, StringComparison.Ordinal);
            Assert.Contains("TryClearZoneTemplate(zoneId, templateType, em)", pluginText, StringComparison.Ordinal);

            Assert.Contains("boss_enter:arena_default", configText, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("clear_template:boss", configText, StringComparison.OrdinalIgnoreCase);
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
