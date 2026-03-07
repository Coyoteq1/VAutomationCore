using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VampireCommandFramework;
using VAutomationCore.Core.Logging;
using VAutomationCore.Core.Automation;
using VAutomationCore.Core;
using VAutomationCore.Core.Contracts;
using VAutomationCore.Core.Services;
using VAutomationCore.Core.Extensions;

namespace VAutomationCore.Core.Automation
{
    /// <summary>
    /// ModCommunication-based automation system for dynamic command registration and execution.
    /// This approach is cleaner, safer, and more scalable than Harmony patching.
    /// 
    /// Architecture:
    /// - Static commands (.vreg, .vlist, etc.) registered via CommandRegistry
    /// - Dynamic commands stored as JSON rules with ModCommunication identifiers
    /// - ModCommunicationService for inter-mod communication
    /// - No Harmony patching required
    /// </summary>
    public static class CrossModsCommandsPatch
    {
        private static readonly CoreLogger _log = new CoreLogger("CrossModsCommandsPatch");
        private const string CoreModId = "VAutomationCore";
        private const string AutomationCommandTopic = "automation.command";
        private const string BloodyBossCommandTopic = "bloodyboss.command";
        private const string BloodyBossCommandsTopic = "bloodyboss.commands";
        private const string BloodyBossSpawnTopic = "bloodyboss.spawn";
        private static bool _subscribed;

        private sealed class DynamicCommandMessage
        {
            public string Command { get; set; } = string.Empty;
            public object[]? Args { get; set; }
        }

        /// <summary>
        /// Initialize ModCommunication automation system.
        /// This should be called during plugin initialization.
        /// </summary>
        public static void Initialize()
        {
            try
            {
                // Initialize the communication service
                ModCommunicationService.Instance.Initialize();
                if (!_subscribed)
                {
                    SubscribeToCommands((fromMod, payload) =>
                    {
                        try
                        {
                            switch (payload)
                            {
                                case DynamicCommandMessage envelope when !string.IsNullOrWhiteSpace(envelope.Command):
                                    _ = AutomationService.Instance.ExecuteRule(envelope.Command);
                                    break;
                                case string cmd when !string.IsNullOrWhiteSpace(cmd):
                                    _ = AutomationService.Instance.ExecuteRule(cmd);
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            _log.Error($"Failed to process dynamic command message from '{fromMod}': {ex.Message}");
                        }
                    });
                    SubscribeToBloodyBossCommands();
                    _subscribed = true;
                }
                
                _log.Info("ModCommunication automation system initialized");
            }
            catch (Exception ex)
            {
                _log.Error($"Failed to initialize ModCommunication automation: {ex.Message}");
            }
        }

        /// <summary>
        /// Register a dynamic command for ModCommunication execution.
        /// </summary>
        public static bool RegisterModCommunicationCommand(string commandName, AutomationRule rule)
        {
            try
            {
                // Store the rule in AutomationService
                AutomationService.Instance.RegisterRule(rule);
                
                _log.Info($"ModCommunication command '{commandName}' registered");
                return true;
            }
            catch (Exception ex)
            {
                _log.Error($"Failed to register ModCommunication command '{commandName}': {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Execute a dynamic command via ModCommunication.
        /// </summary>
        public static void ExecuteCommand(string commandName, object[] args = null)
        {
            try
            {
                // Send the command to other mods via ModCommunication.
                ModCommunicationService.Instance.SendToMod(CoreModId, AutomationCommandTopic, new DynamicCommandMessage
                {
                    Command = commandName,
                    Args = args
                });
                
                _log.Info($"Command '{commandName}' executed via ModCommunication");
            }
            catch (Exception ex)
            {
                _log.Error($"Failed to execute ModCommunication command '{commandName}': {ex.Message}");
            }
        }

        /// <summary>
        /// Subscribe to dynamic command execution requests.
        /// </summary>
        public static void SubscribeToCommands(Action<string, object> handler)
        {
            try
            {
                ModCommunicationService.Instance.Subscribe(CoreModId, AutomationCommandTopic, handler);
                _log.Info("Subscribed to dynamic command execution requests");
            }
            catch (Exception ex)
            {
                _log.Error($"Failed to subscribe to dynamic commands: {ex.Message}");
            }
        }

        private static void SubscribeToBloodyBossCommands()
        {
            try
            {
                Action<string, object> handler = HandleBloodyBossCommandMessage;
                ModCommunicationService.Instance.Subscribe(CoreModId, BloodyBossCommandTopic, handler);
                ModCommunicationService.Instance.Subscribe(CoreModId, BloodyBossCommandsTopic, handler);
                ModCommunicationService.Instance.Subscribe(CoreModId, BloodyBossSpawnTopic, handler);
                _log.Info($"Subscribed to BloodyBoss command bridge topics: {BloodyBossCommandTopic}, {BloodyBossCommandsTopic}, {BloodyBossSpawnTopic}");
            }
            catch (Exception ex)
            {
                _log.Error($"Failed to subscribe to BloodyBoss bridge topics: {ex.Message}");
            }
        }

        private static void HandleBloodyBossCommandMessage(string fromMod, object payload)
        {
            try
            {
                var commands = ExtractBloodyBossCommands(payload).ToArray();
                if (commands.Length == 0)
                {
                    _log.Warning($"BloodyBoss bridge received no executable command from '{fromMod}'");
                    return;
                }

                var entityManager = UnifiedCore.EntityManager;
                if (entityManager == default)
                {
                    _log.Error($"BloodyBoss bridge cannot execute commands from '{fromMod}': EntityManager unavailable");
                    return;
                }

                foreach (var command in commands)
                {
                    var normalized = NormalizeBloodyBossCommand(command);
                    if (string.IsNullOrWhiteSpace(normalized))
                    {
                        continue;
                    }

                    if (!TryExecVcf(entityManager, normalized))
                    {
                        _log.Warning($"BloodyBoss bridge failed to execute command from '{fromMod}': {normalized}");
                        return;
                    }
                }

                _log.Info($"BloodyBoss bridge executed {commands.Length} command(s) from '{fromMod}'");
            }
            catch (Exception ex)
            {
                _log.Error($"Failed to execute BloodyBoss bridge message from '{fromMod}': {ex.Message}");
            }
        }

        private static IEnumerable<string> ExtractBloodyBossCommands(object payload)
        {
            if (payload == null)
            {
                yield break;
            }

            if (payload is string singleCommand)
            {
                yield return singleCommand;
                yield break;
            }

            if (TryGetPayloadValue(payload, new[] { "command", "cmd" }, out var single) &&
                single is string command &&
                !string.IsNullOrWhiteSpace(command))
            {
                yield return command;
            }

            if (TryGetPayloadValue(payload, new[] { "commands", "cmds" }, out var many))
            {
                foreach (var item in EnumerateStringCommands(many))
                {
                    yield return item;
                }
            }
        }

        private static IEnumerable<string> EnumerateStringCommands(object? payload)
        {
            if (payload == null)
            {
                yield break;
            }

            if (payload is string single)
            {
                yield return single;
                yield break;
            }

            if (payload is IEnumerable<string> strings)
            {
                foreach (var command in strings.Where(command => !string.IsNullOrWhiteSpace(command)))
                {
                    yield return command;
                }

                yield break;
            }

            if (payload is System.Collections.IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                {
                    if (item is string command && !string.IsNullOrWhiteSpace(command))
                    {
                        yield return command;
                    }
                }
            }
        }

        private static string NormalizeBloodyBossCommand(string command)
        {
            var trimmed = command?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(trimmed))
            {
                return string.Empty;
            }

            if (trimmed.StartsWith(".bb ", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(trimmed, ".bb", StringComparison.OrdinalIgnoreCase))
            {
                return trimmed;
            }

            if (trimmed.StartsWith("bb ", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(trimmed, "bb", StringComparison.OrdinalIgnoreCase))
            {
                return $".{trimmed}";
            }

            if (trimmed.StartsWith(".", StringComparison.Ordinal))
            {
                return trimmed;
            }

            return $".bb {trimmed}";
        }

        private static bool TryExecVcf(Unity.Entities.EntityManager entityManager, string command)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(command) || !UnifiedCore.IsInitialized || entityManager == default)
                {
                    return false;
                }

                var registryType = Type.GetType("VampireCommandFramework.CommandRegistry, VampireCommandFramework");
                if (registryType == null)
                {
                    return false;
                }

                foreach (var method in registryType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
                {
                    if (!method.Name.Contains("Handle", StringComparison.OrdinalIgnoreCase) &&
                        !method.Name.Contains("Execute", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var parameters = method.GetParameters();
                    if (parameters.Length == 1 && parameters[0].ParameterType == typeof(string))
                    {
                        method.Invoke(null, new object[] { command });
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error($"VCF command execution failed for '{command}': {ex.Message}");
            }

            return false;
        }

        private static bool TryGetPayloadValue(object payload, string[] names, out object? value)
        {
            value = null;
            if (payload == null || names == null || names.Length == 0)
            {
                return false;
            }

            if (payload is IReadOnlyDictionary<string, object> readOnlyDictionary)
            {
                foreach (var pair in readOnlyDictionary)
                {
                    if (names.Any(name => string.Equals(pair.Key, name, StringComparison.OrdinalIgnoreCase)))
                    {
                        value = pair.Value;
                        return true;
                    }
                }
            }

            if (payload is IDictionary<string, object> dictionary)
            {
                foreach (var pair in dictionary)
                {
                    if (names.Any(name => string.Equals(pair.Key, name, StringComparison.OrdinalIgnoreCase)))
                    {
                        value = pair.Value;
                        return true;
                    }
                }
            }

            var property = payload
                .GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .FirstOrDefault(candidate =>
                    candidate.CanRead &&
                    names.Any(name => string.Equals(candidate.Name, name, StringComparison.OrdinalIgnoreCase)));
            if (property == null)
            {
                return false;
            }

            value = property.GetValue(payload);
            return true;
        }
    }
}
