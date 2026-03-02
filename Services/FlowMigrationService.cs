using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using VAutomationCore.Core.Logging;

namespace VAutomationCore.Services
{
    /// <summary>
    /// Service for migrating legacy flow configurations to the unified Flow Registry format.
    /// Handles conversion from Bluelock format to CycleBorn format.
    /// </summary>
    public static class FlowMigrationService
    {
        private static readonly CoreLogger Log = new("FlowMigrationService");

        /// <summary>
        /// Represents the legacy Bluelock flow format.
        /// </summary>
        public class LegacyBluelockFlow
        {
            public string FlowId { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public List<LegacyFlowAction> OnEnter { get; set; } = new();
            public List<LegacyFlowAction> OnExit { get; set; } = new();
            public List<LegacyFlowAction> MustFlows { get; set; } = new();
        }

        /// <summary>
        /// Represents a legacy flow action.
        /// </summary>
        public class LegacyFlowAction
        {
            public string Action { get; set; } = string.Empty;
            public bool Critical { get; set; }
        }

        /// <summary>
        /// Represents the unified CycleBorn flow registry format.
        /// </summary>
        public class UnifiedFlowRegistry
        {
            public string SchemaVersion { get; set; } = "1.0.0";
            public string ModuleId { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public List<string> MigratedFrom { get; set; } = new();
            public Dictionary<string, UnifiedFlow> Flows { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Represents a unified flow definition.
        /// </summary>
        public class UnifiedFlow
        {
            public string Description { get; set; } = string.Empty;
            public List<UnifiedFlowStep> Steps { get; set; } = new();
        }

        /// <summary>
        /// Represents a unified flow step.
        /// </summary>
        public class UnifiedFlowStep
        {
            public string Action { get; set; } = string.Empty;
            public bool ContinueOnFailure { get; set; } = true;
            public List<string> Args { get; set; } = new();
        }

        /// <summary>
        /// Migrates Bluelock flow files to unified CycleBorn registry.
        /// </summary>
        /// <param name="sourceFiles">Array of Bluelock flow file paths</param>
        /// <param name="targetPath">Target unified registry file path</param>
        /// <param name="moduleId">Module ID for the unified registry</param>
        /// <returns>True if migration succeeded</returns>
        public static bool MigrateBluelockFlows(string[] sourceFiles, string targetPath, string moduleId)
        {
            try
            {
                var unifiedRegistry = new UnifiedFlowRegistry
                {
                    ModuleId = moduleId,
                    Description = $"Migrated zone flows from Bluelock config/flows/*.json",
                    MigratedFrom = sourceFiles.ToList()
                };

                foreach (var sourceFile in sourceFiles)
                {
                    if (!File.Exists(sourceFile))
                    {
                        Log.LogWarning($"Source file not found: {sourceFile}");
                        continue;
                    }

                    var json = File.ReadAllText(sourceFile);
                    var legacyFlow = JsonSerializer.Deserialize<LegacyBluelockFlow>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (legacyFlow == null)
                    {
                        Log.LogWarning($"Failed to deserialize: {sourceFile}");
                        continue;
                    }

                    // Convert onEnter to flow
                    if (legacyFlow.OnEnter?.Count > 0)
                    {
                        var enterFlowName = $"{legacyFlow.FlowId}.enter";
                        unifiedRegistry.Flows[enterFlowName] = new UnifiedFlow
                        {
                            Description = $"{legacyFlow.Description} - Enter flow (migrated from {Path.GetFileName(sourceFile)} onEnter)",
                            Steps = ConvertActions(legacyFlow.OnEnter)
                        };
                    }

                    // Convert onExit to flow
                    if (legacyFlow.OnExit?.Count > 0)
                    {
                        var exitFlowName = $"{legacyFlow.FlowId}.exit";
                        unifiedRegistry.Flows[exitFlowName] = new UnifiedFlow
                        {
                            Description = $"{legacyFlow.Description} - Exit flow (migrated from {Path.GetFileName(sourceFile)} onExit)",
                            Steps = ConvertActions(legacyFlow.OnExit)
                        };
                    }

                    // Convert mustFlows to flow
                    if (legacyFlow.MustFlows?.Count > 0)
                    {
                        var mustFlowName = $"{legacyFlow.FlowId}.must";
                        unifiedRegistry.Flows[mustFlowName] = new UnifiedFlow
                        {
                            Description = $"{legacyFlow.Description} - Must execute flow (migrated from {Path.GetFileName(sourceFile)} mustFlows)",
                            Steps = ConvertActions(legacyFlow.MustFlows)
                        };
                    }

                    Log.LogInfo($"Migrated flow: {legacyFlow.FlowId} from {sourceFile}");
                }

                // Write unified registry
                var targetDir = Path.GetDirectoryName(targetPath);
                if (!string.IsNullOrEmpty(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                }

                var outputJson = JsonSerializer.Serialize(unifiedRegistry, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(targetPath, outputJson);
                Log.LogInfo($"Migration complete. Unified registry written to: {targetPath}");

                return true;
            }
            catch (Exception ex)
            {
                Log.LogError($"Migration failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Converts legacy actions to unified steps.
        /// Bluelock "critical: false" = ContinueOnFailure: true
        /// Bluelock "critical: true" = ContinueOnFailure: false
        /// </summary>
        private static List<UnifiedFlowStep> ConvertActions(List<LegacyFlowAction> actions)
        {
            return actions.Select(a => new UnifiedFlowStep
            {
                Action = a.Action,
                // In Bluelock: critical=false means continue on failure
                // In unified: ContinueOnFailure=true means continue on failure
                ContinueOnFailure = !a.Critical
            }).ToList();
        }

        /// <summary>
        /// Example migration path for this specific project.
        /// Migrated from Bluelock/config/flows/*.json to CycleBorn/Configuration/flows.registry.json
        /// </summary>
        public static void RunVAutomationCoreMigration()
        {
            var sourceFiles = new[]
            {
                "Bluelock/config/flows/A1.json",
                "Bluelock/config/flows/B1.json",
                "Bluelock/config/flows/T3.json",
                "Bluelock/config/flows/ZoneDefault.json"
            };

            var targetPath = "CycleBorn/Configuration/flows.registry.json";
            var moduleId = "bluelock.zones";

            if (MigrateBluelockFlows(sourceFiles, targetPath, moduleId))
            {
                // Delete source files after successful migration
                foreach (var file in sourceFiles)
                {
                    try
                    {
                        if (File.Exists(file))
                        {
                            File.Delete(file);
                            Log.LogInfo($"Deleted migrated file: {file}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.LogWarning($"Failed to delete {file}: {ex.Message}");
                    }
                }

                // Remove empty directory
                try
                {
                    var flowsDir = "Bluelock/config/flows";
                    if (Directory.Exists(flowsDir) && !Directory.EnumerateFileSystemEntries(flowsDir).Any())
                    {
                        Directory.Delete(flowsDir);
                        Log.LogInfo($"Deleted empty directory: {flowsDir}");
                    }
                }
                catch (Exception ex)
                {
                    Log.LogWarning($"Failed to delete flows directory: {ex.Message}");
                }
            }
        }
    }
}
