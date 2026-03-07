using System;
using System.Collections.Generic;
using VAutomationCore.Core.Gameplay;

namespace VAutomationCore.Core.Gameplay.Shared.Types
{
    /// <summary>
    /// Shared flow definition used across all gameplay modules.
    /// This is the infrastructure-level type - gameplay-specific flow data stays in the gameplay module.
    /// </summary>
    public sealed class FlowDefinition
    {
        /// <summary>
        /// Unique flow name.
        /// </summary>
        public string Name { get; init; } = string.Empty;

        /// <summary>
        /// The gameplay type that owns this flow.
        /// </summary>
        public GameplayType GameplayType { get; init; }

        /// <summary>
        /// Human-readable description.
        /// </summary>
        public string Description { get; init; } = string.Empty;

        /// <summary>
        /// Whether this flow requires admin privileges.
        /// </summary>
        public bool AdminOnly { get; init; }

        /// <summary>
        /// Whether the flow is enabled by default.
        /// </summary>
        public bool EnabledByDefault { get; init; } = true;

        /// <summary>
        /// Zone types this flow can be bound to.
        /// </summary>
        public IReadOnlyList<string> SupportedZoneTypes { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Tags for categorization and filtering.
        /// </summary>
        public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Typed argument definitions for this flow.
        /// </summary>
        public IReadOnlyList<FlowArgDefinition> Arguments { get; init; } = Array.Empty<FlowArgDefinition>();

        /// <summary>
        /// Permission required to execute this flow.
        /// </summary>
        public string? RequiredPermission { get; init; }

        /// <summary>
        /// Cooldown in seconds between executions.
        /// </summary>
        public float CooldownSeconds { get; init; }

        /// <summary>
        /// Whether this flow can be executed during match active state.
        /// </summary>
        public bool AllowedDuringMatch { get; init; } = true;

        /// <summary>
        /// Custom metadata for this flow.
        /// </summary>
        public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();
    }

    /// <summary>
    /// Flow execution context passed to flow handlers.
    /// </summary>
    public sealed class FlowExecutionContext
    {
        /// <summary>
        /// The flow being executed.
        /// </summary>
        public FlowDefinition Flow { get; init; } = null!;

        /// <summary>
        /// Arguments provided for execution.
        /// </summary>
        public IReadOnlyDictionary<string, object?> Arguments { get; init; } = new Dictionary<string, object?>();

        /// <summary>
        /// The player initiating the flow (if any).
        /// </summary>
        public object? InitiatingPlayer { get; init; }

        /// <summary>
        /// The zone context (if any).
        /// </summary>
        public string? ZoneContext { get; init; }

        /// <summary>
        /// Execution timestamp.
        /// </summary>
        public DateTime ExecutedAt { get; init; } = DateTime.UtcNow;

        /// <summary>
        /// Whether this is a dry run (validation only).
        /// </summary>
        public bool IsDryRun { get; init; }
    }
}
