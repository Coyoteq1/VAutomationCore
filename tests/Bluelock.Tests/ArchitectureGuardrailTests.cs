using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VAutomationCore.Core.Events;
using VAutomationCore.Core.Lifecycle;
using Xunit;

namespace Bluelock.Tests
{
    public class ArchitectureGuardrailTests
    {
        [Fact]
        public void CoreContracts_ArePresent()
        {
            Assert.NotNull(typeof(IZoneLifecycleContext));
            Assert.NotNull(typeof(IZoneEnterStep));
            Assert.NotNull(typeof(IZoneExitStep));
            Assert.NotNull(typeof(IZoneLifecycleStepRegistry));
            Assert.NotNull(typeof(TypedEventBus));
        }

        [Fact]
        public void PluginProjectReferences_FollowGuardrails()
        {
            var repoRoot = ResolveRepoRoot();
            var pluginProjects = new[]
            {
                Path.Combine(repoRoot, "Bluelock", "VAutoZone.csproj"),
                Path.Combine(repoRoot, "CycleBorn", "Vlifecycle.csproj"),
                Path.Combine(repoRoot, "VAutoannounce", "VAutoannounce.csproj"),
                Path.Combine(repoRoot, "VAutoTraps", "VAutoTraps.csproj")
            };

            var allowedByProject = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase)
            {
                // Temporary exception: Bluelock currently references CycleBorn.
                ["VAutoZone.csproj"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "../VAutomationCore.csproj",
                    "../CycleBorn/Vlifecycle.csproj"
                },
                ["Vlifecycle.csproj"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "../VAutomationCore.csproj"
                },
                ["VAutoannounce.csproj"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "..\\VAutomationCore.csproj",
                    "../VAutomationCore.csproj"
                },
                ["VAutoTraps.csproj"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "../VAutomationCore.csproj"
                }
            };

            foreach (var projectPath in pluginProjects)
            {
                Assert.True(File.Exists(projectPath), $"Project file missing: {projectPath}");
                var fileName = Path.GetFileName(projectPath);
                Assert.True(allowedByProject.TryGetValue(fileName, out var allowed), $"No guardrail rule for {fileName}");

                var lines = File.ReadAllLines(projectPath);
                var projectRefs = lines
                    .Select(line => line.Trim())
                    .Where(line => line.StartsWith("<ProjectReference Include=", StringComparison.OrdinalIgnoreCase))
                    .Select(line => ExtractIncludeValue(line))
                    .Select(NormalizeProjectReferencePath)
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .ToArray();

                foreach (var projectRef in projectRefs)
                {
                    Assert.Contains(projectRef, allowed.Select(NormalizeProjectReferencePath));
                }
            }
        }

        [Fact]
        public void RepositoryRoot_RejectsClutterAndUnexpectedSourceFiles()
        {
            var repoRoot = ResolveRepoRoot();

            var rootLogs = Directory.GetFiles(repoRoot, "*.log", SearchOption.TopDirectoryOnly);
            Assert.Empty(rootLogs);

            var rootZips = Directory.GetFiles(repoRoot, "*.zip", SearchOption.TopDirectoryOnly);
            Assert.Empty(rootZips);

            var allowedRootCs = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "GlobalUsings.cs",
                "IService.cs",
                "MyPluginInfo.cs",
                "Plugin.cs",
                "PrefabGuidConverter.cs",
                "VAutoLogger.cs"
            };

            var rootCs = Directory.GetFiles(repoRoot, "*.cs", SearchOption.TopDirectoryOnly)
                .Select(Path.GetFileName)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .ToArray();

            foreach (var fileName in rootCs)
            {
                Assert.Contains(fileName, allowedRootCs);
            }
        }

        private static string ResolveRepoRoot()
        {
            var current = AppContext.BaseDirectory;
            while (!string.IsNullOrWhiteSpace(current))
            {
                var candidate = Path.Combine(current, "VAutomationCore.csproj");
                if (File.Exists(candidate))
                {
                    return current;
                }

                var parent = Directory.GetParent(current);
                if (parent == null)
                {
                    break;
                }

                current = parent.FullName;
            }

            throw new DirectoryNotFoundException("Failed to resolve repository root from test base directory.");
        }

        private static string ExtractIncludeValue(string trimmedProjectReferenceLine)
        {
            const string marker = "Include=\"";
            var start = trimmedProjectReferenceLine.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (start < 0)
            {
                return string.Empty;
            }

            start += marker.Length;
            var end = trimmedProjectReferenceLine.IndexOf('"', start);
            if (end < 0 || end <= start)
            {
                return string.Empty;
            }

            return trimmedProjectReferenceLine.Substring(start, end - start);
        }

        private static string NormalizeProjectReferencePath(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return string.Empty;
            }

            var normalized = raw.Trim();
            while (normalized.Contains("\\\\", StringComparison.Ordinal))
            {
                normalized = normalized.Replace("\\\\", "\\", StringComparison.Ordinal);
            }

            return normalized.Replace('\\', '/');
        }
    }
}
