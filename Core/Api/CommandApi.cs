using System.Collections.Generic;
using System.Linq;
using VampireCommandFramework;

namespace VAutomationCore.Core.Api
{
    /// <summary>
    /// Command-related APIs for runtime command/flow integration.
    /// </summary>
    public static class CommandApi
    {
        public static void RegisterAllFromAssembly(System.Reflection.Assembly assembly)
        {
            CommandRegistry.RegisterAll(assembly);
        }

        public static bool RegisterActionAlias(string alias, string actionName, bool replace = true)
        {
            return FlowService.RegisterActionAlias(alias, actionName, replace);
        }

        public static bool RemoveActionAlias(string alias)
        {
            return FlowService.RemoveActionAlias(alias);
        }

        public static IReadOnlyDictionary<string, string> GetActionAliases()
        {
            return FlowService.GetActionAliases();
        }

        public static IReadOnlyCollection<string> GetRegisteredFlowNames()
        {
            return FlowService.GetFlowNames().OrderBy(x => x).ToArray();
        }

        public static bool CanRunJobs(ulong subjectId)
        {
            return ConsoleRoleAuthService.CanUseJobs(subjectId, out _, out _);
        }
    }
}
