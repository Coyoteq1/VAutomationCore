using System;
using System.Collections.Generic;
using System.Linq;
using VAutomationCore.Core.Gameplay.Shared.Contracts;
using VAutomationCore.Core.Gameplay.Shared.Types;

namespace VAutomationCore.Core.Gameplay
{
    /// <summary>
    /// Validation system for gameplay modules.
    /// Validates isolation, ownership, and typed flow definitions.
    /// </summary>
    public static class GameplayValidationSystem
    {
        /// <summary>
        /// Result of isolation validation.
        /// </summary>
        public sealed class IsolationValidationResult
        {
            public bool IsValid { get; init; }
            public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();
            public IReadOnlyList<string> Warnings { get; init; } = Array.Empty<string>();
            public IReadOnlyDictionary<string, int> FlowsByModule { get; init; } = new Dictionary<string, int>();
            public IReadOnlyDictionary<string, int> ZonesByModule { get; init; } = new Dictionary<string, int>();
        }

        /// <summary>
        /// Result of ownership validation.
        /// </summary>
        public sealed class OwnershipValidationResult
        {
            public bool IsValid { get; init; }
            public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();
            public IReadOnlyList<string> ZoneOwnershipConflicts { get; init; } = Array.Empty<string>();
            public IReadOnlyList<string> FlowOwnershipConflicts { get; init; } = Array.Empty<string>();
        }

        /// <summary>
        /// Full validation result combining all checks.
        /// </summary>
        public sealed class FullValidationResult
        {
            public bool IsValid { get; init; }
            public IsolationValidationResult Isolation { get; init; } = null!;
            public OwnershipValidationResult Ownership { get; init; } = null!;
            public IReadOnlyList<ModuleValidationResult> ModuleValidations { get; init; } = Array.Empty<ModuleValidationResult>();
            public IReadOnlyList<string> CrossModuleConflicts { get; init; } = Array.Empty<string>();
        }

        /// <summary>
        /// Validate isolation - ensure no gameplay-specific data in shared folders.
        /// </summary>
        public static IsolationValidationResult ValidateIsolation()
        {
            var errors = new List<string>();
            var warnings = new List<string>();
            
            var flowsByModule = new Dictionary<string, int>();
            var zonesByModule = new Dictionary<string, int>();

            // Count flows by module
            foreach (var flow in GameplayFlowRegistry.GetAllFlows())
            {
                var moduleName = flow.GameplayType.ToString();
                if (!flowsByModule.ContainsKey(moduleName))
                    flowsByModule[moduleName] = 0;
                flowsByModule[moduleName]++;
            }

            // Count zones by module
            foreach (var zone in ZoneRegistry.GetAllZones())
            {
                var moduleName = zone.GameplayType.ToString();
                if (!zonesByModule.ContainsKey(moduleName))
                    zonesByModule[moduleName] = 0;
                zonesByModule[moduleName]++;
            }

            // Check for Progression - this indicates unmigrated flows
            if (flowsByModule.ContainsKey("Progression"))
            {
                warnings.Add($"Found {flowsByModule["Progression"]} flows in Progression - these should be migrated to specific modules");
            }

            return new IsolationValidationResult
            {
                IsValid = errors.Count == 0,
                Errors = errors,
                Warnings = warnings,
                FlowsByModule = flowsByModule,
                ZonesByModule = zonesByModule
            };
        }

        /// <summary>
        /// Validate ownership - ensure each zone/flow has exactly one owner.
        /// </summary>
        public static OwnershipValidationResult ValidateOwnership()
        {
            var errors = new List<string>();
            var zoneConflicts = new List<string>();
            var flowConflicts = new List<string>();

            // Check zone ownership conflicts
            var zoneGroups = ZoneRegistry.GetAllZones()
                .GroupBy(z => z.ZoneId.ToLowerInvariant());
                
            foreach (var group in zoneGroups)
            {
                var owners = group.Select(z => z.GameplayType).Distinct().ToList();
                if (owners.Count > 1)
                {
                    zoneConflicts.Add($"Zone '{group.Key}' has {owners.Count} owners: {string.Join(", ", owners)}");
                }
            }

            // Check flow ownership conflicts
            var flowGroups = GameplayFlowRegistry.GetAllFlows()
                .GroupBy(f => f.Name.ToLowerInvariant());
                
            foreach (var group in flowGroups)
            {
                var owners = group.Select(f => f.GameplayType).Distinct().ToList();
                if (owners.Count > 1)
                {
                    flowConflicts.Add($"Flow '{group.Key}' has {owners.Count} owners: {string.Join(", ", owners)}");
                }
            }

            errors.AddRange(zoneConflicts);
            errors.AddRange(flowConflicts);

            return new OwnershipValidationResult
            {
                IsValid = errors.Count == 0,
                Errors = errors,
                ZoneOwnershipConflicts = zoneConflicts,
                FlowOwnershipConflicts = flowConflicts
            };
        }

        /// <summary>
        /// Validate all modules.
        /// </summary>
        public static IReadOnlyList<ModuleValidationResult> ValidateAllModules()
        {
            return GameplayRegistry.ValidateAllModules();
        }

        /// <summary>
        /// Run full validation suite.
        /// </summary>
        public static FullValidationResult ValidateAll()
        {
            var isolation = ValidateIsolation();
            var ownership = ValidateOwnership();
            var moduleValidations = ValidateAllModules();
            
            var crossModuleConflicts = new List<string>();
            crossModuleConflicts.AddRange(isolation.Warnings);
            crossModuleConflicts.AddRange(ownership.Errors);

            // Check for invalid payload type references in flows
            foreach (var flow in GameplayFlowRegistry.GetAllFlows())
            {
                if (flow.Arguments == null) continue;
                
                foreach (var arg in flow.Arguments)
                {
                    // Validate payload types
                    if (!string.IsNullOrEmpty(arg.PayloadTypeName))
                    {
                        var validator = GameplayRegistry.GetValidator(flow.GameplayType);
                        if (validator != null)
                        {
                            if (!validator.ValidatePayloadType(arg.PayloadTypeName, arg.Kind))
                            {
                                crossModuleConflicts.Add(
                                    $"Flow '{flow.Name}' argument '{arg.Name}' references invalid payload type '{arg.PayloadTypeName}'");
                            }
                        }
                    }
                    
                    // Validate entity roles
                    if (!string.IsNullOrEmpty(arg.EntityRoleName))
                    {
                        var validator = GameplayRegistry.GetValidator(flow.GameplayType);
                        if (validator != null)
                        {
                            if (!validator.ValidateEntityRole(arg.EntityRoleName))
                            {
                                crossModuleConflicts.Add(
                                    $"Flow '{flow.Name}' argument '{arg.Name}' references invalid entity role '{arg.EntityRoleName}'");
                            }
                        }
                    }
                    
                    // Validate prefab categories
                    if (!string.IsNullOrEmpty(arg.PrefabCategory))
                    {
                        var validator = GameplayRegistry.GetValidator(flow.GameplayType);
                        if (validator != null)
                        {
                            if (!validator.ValidatePrefabCategory(arg.PrefabCategory))
                            {
                                crossModuleConflicts.Add(
                                    $"Flow '{flow.Name}' argument '{arg.Name}' references invalid prefab category '{arg.PrefabCategory}'");
                            }
                        }
                    }
                }
            }

            var allErrors = new List<string>();
            allErrors.AddRange(isolation.Errors);
            allErrors.AddRange(ownership.Errors);
            allErrors.AddRange(crossModuleConflicts);
            foreach (var mv in moduleValidations)
            {
                allErrors.AddRange(mv.Errors);
            }

            return new FullValidationResult
            {
                IsValid = allErrors.Count == 0,
                Isolation = isolation,
                Ownership = ownership,
                ModuleValidations = moduleValidations,
                CrossModuleConflicts = crossModuleConflicts
            };
        }

        /// <summary>
        /// Get comprehensive validation summary.
        /// </summary>
        public static string GetValidationSummary()
        {
            var result = ValidateAll();
            
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("=== Full Validation Summary ===");
            sb.AppendLine();
            
            sb.AppendLine($"Overall Valid: {result.IsValid}");
            sb.AppendLine();
            
            // Isolation
            sb.AppendLine("--- Isolation ---");
            sb.AppendLine($"Valid: {result.Isolation.IsValid}");
            sb.AppendLine($"Flows by module:");
            foreach (var kvp in result.Isolation.FlowsByModule)
            {
                sb.AppendLine($"  {kvp.Key}: {kvp.Value}");
            }
            sb.AppendLine($"Zones by module:");
            foreach (var kvp in result.Isolation.ZonesByModule)
            {
                sb.AppendLine($"  {kvp.Key}: {kvp.Value}");
            }
            
            if (result.Isolation.Warnings.Count > 0)
            {
                sb.AppendLine("Warnings:");
                foreach (var w in result.Isolation.Warnings)
                {
                    sb.AppendLine($"  - {w}");
                }
            }
            sb.AppendLine();
            
            // Ownership
            sb.AppendLine("--- Ownership ---");
            sb.AppendLine($"Valid: {result.Ownership.IsValid}");
            if (result.Ownership.ZoneOwnershipConflicts.Count > 0)
            {
                sb.AppendLine("Zone Conflicts:");
                foreach (var c in result.Ownership.ZoneOwnershipConflicts)
                {
                    sb.AppendLine($"  - {c}");
                }
            }
            if (result.Ownership.FlowOwnershipConflicts.Count > 0)
            {
               sb.AppendLine("Flow Conflicts:");
                foreach (var c in result.Ownership.FlowOwnershipConflicts)
                {
                    sb.AppendLine($"  - {c}");
                }
            }
            sb.AppendLine();
            
            // Module validations
            sb.AppendLine("--- Module Validations ---");
            foreach (var mv in result.ModuleValidations)
            {
                sb.AppendLine($"Module: {mv.IsValid} (flows={mv.FlowCount}, zones={mv.ZoneCount})");
                if (mv.Errors.Count > 0)
                {
                    foreach (var e in mv.Errors)
                    {
                        sb.AppendLine($"  ERROR: {e}");
                    }
                }
            }
            sb.AppendLine();
            
            // Cross-module conflicts
            if (result.CrossModuleConflicts.Count > 0)
            {
                sb.AppendLine("--- Cross-Module Conflicts ---");
                foreach (var c in result.CrossModuleConflicts)
                {
                    sb.AppendLine($"  - {c}");
                }
            }
            
            return sb.ToString();
        }
    }
}
