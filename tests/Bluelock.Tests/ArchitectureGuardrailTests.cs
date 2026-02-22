using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VAutomationCore.Core.Api;
using VAutomationCore.Core.Data;
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
        public void CoreReleaseContracts_ArePresent()
        {
            Assert.NotNull(typeof(ConsoleRoleAuthService));
            Assert.NotNull(typeof(EntityAliasMapper));
            Assert.NotNull(typeof(EntityMap));
            Assert.NotNull(typeof(FlowService));
            Assert.NotNull(typeof(ConsoleRoleComponent));
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
                Path.Combine(repoRoot, "VAutoTraps", "VAutoTraps.csproj"),
                Path.Combine(repoRoot, "Swapkits", "Swapkits.csproj")
            };

            var allowedByProject = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["VAutoZone.csproj"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "../VAutomationCore.csproj"
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
                },
                ["Swapkits.csproj"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
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

            var rootZips = Directory.GetFiles(repoRoot, "*.zip", SearchOption.TopDirectoryOnly)
                .Select(Path.GetFileName)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .ToArray();

            var allowedRootZips = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "VAutomationCore-1.0.1.zip",
                "VAutomationZone-1.0.1.zip"
            };

            foreach (var zip in rootZips)
            {
                Assert.Contains(zip, allowedRootZips);
            }

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

        [Fact]
        public void Repository_RejectsExtractorDependencyPatterns()
        {
            var repoRoot = ResolveRepoRoot();
            var disallowedPatterns = new[]
            {
                "KindredExtract",
                "ComponentExtractors",
                "EntityDebug.RegisterExtractor"
            };

            var disallowed = new List<string>();
            foreach (var file in EnumerateTextFiles(repoRoot))
            {
                var relPath = Path.GetRelativePath(repoRoot, file).Replace('\\', '/');
                if (string.Equals(relPath, "tests/Bluelock.Tests/ArchitectureGuardrailTests.cs", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var lineNumber = 0;
                foreach (var line in File.ReadLines(file))
                {
                    lineNumber++;
                    foreach (var pattern in disallowedPatterns)
                    {
                        if (!line.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        disallowed.Add($"{relPath}:{lineNumber}:{pattern}");
                    }
                }
            }

            Assert.True(disallowed.Count == 0,
                "Disallowed extractor/dependency patterns found:\n" + string.Join('\n', disallowed));
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

        private static IEnumerable<string> EnumerateTextFiles(string repoRoot)
        {
            static bool ShouldSkipDirectory(string path)
            {
                var normalized = path.Replace('\\', '/');
                return normalized.Contains("/.git/", StringComparison.OrdinalIgnoreCase)
                    || normalized.Contains("/bin/", StringComparison.OrdinalIgnoreCase)
                    || normalized.Contains("/obj/", StringComparison.OrdinalIgnoreCase)
                    || normalized.Contains("/artifacts/", StringComparison.OrdinalIgnoreCase)
                    || normalized.Contains("/out/", StringComparison.OrdinalIgnoreCase)
                    || normalized.Contains("/_bepinex_out/", StringComparison.OrdinalIgnoreCase);
            }

            var stack = new Stack<string>();
            stack.Push(repoRoot);
            while (stack.Count > 0)
            {
                var current = stack.Pop();
                foreach (var dir in Directory.EnumerateDirectories(current))
                {
                    if (ShouldSkipDirectory(dir))
                    {
                        continue;
                    }

                    stack.Push(dir);
                }

                foreach (var file in Directory.EnumerateFiles(current))
                {
                    var name = Path.GetFileName(file);
                    if (name.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)
                        || name.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase)
                        || name.EndsWith(".props", StringComparison.OrdinalIgnoreCase)
                        || name.EndsWith(".targets", StringComparison.OrdinalIgnoreCase))
                    {
                        yield return file;
                    }
                }
            }
        }
    }
}
