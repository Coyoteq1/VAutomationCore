using System;
using System.Collections.Generic;
using VAutomationCore.Core.Gameplay;

namespace VAutomationCore.Core.Gameplay.Shared.Contracts
{
    /// <summary>
    /// Interface for validating flow arguments.
    /// Each gameplay module can provide its own validator for gameplay-specific payload types.
    /// </summary>
    public interface IFlowArgValidator
    {
        /// <summary>
        /// The gameplay type this validator applies to.
        /// </summary>
        GameplayType GameplayType { get; }

        /// <summary>
        /// Validate a flow argument value against its definition.
        /// </summary>
        /// <param name="argDefinition">The argument definition.</param>
        /// <param name="value">The value to validate.</param>
        /// <returns>Validation result with any errors.</returns>
        FlowArgValidationResult Validate(FlowArgDefinition argDefinition, object? value);

        /// <summary>
        /// Validate a payload type name for Settings or RuleProfile argument kinds.
        /// </summary>
        /// <param name="payloadTypeName">The payload type name to validate.</param>
        /// <param name="argKind">The argument kind (must be Settings or RuleProfile).</param>
        /// <returns>True if the payload type is valid for this gameplay.</returns>
        bool ValidatePayloadType(string payloadTypeName, FlowArgKind argKind);

        /// <summary>
        /// Validate a prefab category for Prefab argument kinds.
        /// </summary>
        /// <param name="prefabCategory">The prefab category to validate.</param>
        /// <returns>True if the category exists in this gameplay's prefab set.</returns>
        bool ValidatePrefabCategory(string prefabCategory);

        /// <summary>
        /// Validate an entity role for Entity/Player/TargetEntity argument kinds.
        /// </summary>
        /// <param name="entityRoleName">The entity role name to validate.</param>
        /// <returns>True if the role exists in this gameplay's entity roles.</returns>
        bool ValidateEntityRole(string entityRoleName);

        /// <summary>
        /// Get all supported payload type names for this gameplay.
        /// </summary>
        IReadOnlyList<string> GetSupportedPayloadTypes();

        /// <summary>
        /// Get all supported entity role names for this gameplay.
        /// </summary>
        IReadOnlyList<string> GetSupportedEntityRoles();

        /// <summary>
        /// Get all supported prefab categories for this gameplay.
        /// </summary>
        IReadOnlyList<string> GetSupportedPrefabCategories();
    }

    /// <summary>
    /// Result of flow argument validation.
    /// </summary>
    public sealed class FlowArgValidationResult
    {
        public bool IsValid { get; init; }
        public string? ErrorMessage { get; init; }
        public string? WarningMessage { get; init; }
        public object? NormalizedValue { get; init; }

        public static FlowArgValidationResult Success() => new() { IsValid = true };
        
        public static FlowArgValidationResult Success(object normalizedValue) => new() 
        { 
            IsValid = true, 
            NormalizedValue = normalizedValue 
        };

        public static FlowArgValidationResult Error(string message) => new() 
        { 
            IsValid = false, 
            ErrorMessage = message 
        };

        public static FlowArgValidationResult Warning(string message) => new() 
        { 
            IsValid = true, 
            WarningMessage = message 
        };
    }
}
