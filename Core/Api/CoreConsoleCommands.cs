using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using VampireCommandFramework;
using VAutomationCore.Core;

namespace VAutomationCore.Core.Api
{
    internal static class JobFlowSessionStore
    {
        private static readonly ConcurrentDictionary<ulong, EntityMap> SubjectEntityMaps = new();

        public static EntityMap GetOrCreate(ulong subjectId)
        {
            return SubjectEntityMaps.GetOrAdd(subjectId, _ => new EntityMap());
        }

        public static bool TryGet(ulong subjectId, out EntityMap map)
        {
            return SubjectEntityMaps.TryGetValue(subjectId, out map!);
        }

        public static void Remove(ulong subjectId)
        {
            SubjectEntityMaps.TryRemove(subjectId, out _);
        }
    }

    [CommandGroup("coreauth", "ca")]
    public static class CoreAuthCommands
    {
        [Command("help", shortHand: "h", description: "Show core role auth commands", adminOnly: false)]
        public static void Help(ChatCommandContext ctx)
        {
            ctx.Reply("[CoreAuth] Commands:");
            ctx.Reply("  .coreauth login admin <password>");
            ctx.Reply("  .coreauth login dev <password>");
            ctx.Reply("  .coreauth status");
            ctx.Reply("  .coreauth logout");
        }

        [Command("login admin", shortHand: "la", description: "Authenticate as Admin role", adminOnly: false)]
        public static void LoginAdmin(ChatCommandContext ctx, string password)
        {
            Login(ctx, password, ConsoleRoleAuthService.ConsoleRole.Admin);
        }

        [Command("login dev", shortHand: "ld", description: "Authenticate as Developer role", adminOnly: false)]
        public static void LoginDeveloper(ChatCommandContext ctx, string password)
        {
            Login(ctx, password, ConsoleRoleAuthService.ConsoleRole.Developer);
        }

        [Command("status", shortHand: "s", description: "Show current role auth status", adminOnly: false)]
        public static void Status(ChatCommandContext ctx)
        {
            if (!RequireAdminOrConsole(ctx))
            {
                return;
            }

            ConsoleRoleAuthService.Initialize();
            var subjectId = ResolveSubjectId(ctx);
            var active = ConsoleRoleAuthService.IsAuthorized(
                subjectId,
                ConsoleRoleAuthService.ConsoleRole.None,
                out var remaining,
                out var role);

            if (!active || role == ConsoleRoleAuthService.ConsoleRole.None)
            {
                ctx.Reply("[CoreAuth] No active role session.");
                return;
            }

            ctx.Reply($"[CoreAuth] Role: {role} | Remaining: {Math.Max(0d, remaining.TotalSeconds):F0}s");
        }

        [Command("logout", shortHand: "lo", description: "Revoke current role session", adminOnly: false)]
        public static void Logout(ChatCommandContext ctx)
        {
            if (!RequireAdminOrConsole(ctx))
            {
                return;
            }

            ConsoleRoleAuthService.Initialize();
            var subjectId = ResolveSubjectId(ctx);
            ConsoleRoleAuthService.Revoke(subjectId);
            JobFlowSessionStore.Remove(subjectId);
            ctx.Reply("[CoreAuth] Session revoked.");
        }

        private static void Login(ChatCommandContext ctx, string password, ConsoleRoleAuthService.ConsoleRole role)
        {
            if (!RequireAdminOrConsole(ctx))
            {
                return;
            }

            ConsoleRoleAuthService.Initialize();
            var subjectId = ResolveSubjectId(ctx);
            var ok = ConsoleRoleAuthService.Authenticate(subjectId, password, role, out var message);
            ctx.Reply(ok ? $"[CoreAuth] {message}" : $"[CoreAuth] {message}");
        }

        internal static bool RequireAdminOrConsole(ChatCommandContext ctx)
        {
            if (IsConsoleInvocation(ctx) || ctx.IsAdmin)
            {
                return true;
            }

            ctx.Reply("[CoreAuth] Admin or console access required.");
            return false;
        }

        internal static bool IsConsoleInvocation(ChatCommandContext ctx)
        {
            // Console invocation can be detected by checking if Event is null,
            // as VampireCommandFramework only provides Event for chat-context invocations
            return ctx?.Event == null;
        }

        internal static ulong ResolveSubjectId(ChatCommandContext ctx)
        {
            try
            {
                var platformId = ctx.User.PlatformId;
                if (platformId != 0)
                {
                    return platformId;
                }
            }
            catch
            {
                // Fall through to console subject.
            }

            return ConsoleRoleAuthService.ConsoleSubjectId;
        }

        internal static bool TryGetSenderCharacterEntity(ChatCommandContext ctx, out Entity entity)
        {
            entity = Entity.Null;
            if (ctx?.Event == null)
            {
                return false;
            }

            entity = ctx.Event.SenderCharacterEntity;
            return entity != Entity.Null;
        }
    }

    [CommandGroup("jobs", "jb")]
    public static class CoreJobFlowCommands
    {
        [Command("help", shortHand: "h", description: "Show job flow commands", adminOnly: false)]
        public static void Help(ChatCommandContext ctx)
        {
            ctx.Reply("[Jobs] Commands:");
            ctx.Reply("  .jobs flow add <flow> <action>");
            ctx.Reply("  .jobs flow remove <flow> | .jobs flow list");
            ctx.Reply("  .jobs action add <alias> <action> | .jobs action list");
            ctx.Reply("  .jobs alias self <alias> | .jobs alias user <alias> [platformId]");
            ctx.Reply("  .jobs alias list | .jobs alias clear [alias]");
            ctx.Reply("  .jobs component add <alias> <componentType>");
            ctx.Reply("  .jobs component list | .jobs component has <entityAlias> <componentAlias>");
            ctx.Reply("  .jobs run <flow>");
            ctx.Reply("  Note: job execution requires Developer role via .coreauth login dev.");
        }

        [Command("flow add", shortHand: "fa", description: "Register or replace one-step flow", adminOnly: false)]
        public static void FlowAdd(ChatCommandContext ctx, string flowName, string actionAliasOrName)
        {
            if (!CoreAuthCommands.RequireAdminOrConsole(ctx))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(flowName) || string.IsNullOrWhiteSpace(actionAliasOrName))
            {
                ctx.Reply("[Jobs] Usage: .jobs flow add <flow> <action>");
                return;
            }

            var ok = FlowService.RegisterFlow(
                flowName.Trim(),
                new[] { new FlowStep(actionAliasOrName.Trim()) },
                replace: true);

            ctx.Reply(ok
                ? $"[Jobs] Flow '{flowName}' registered with action '{actionAliasOrName}'."
                : $"[Jobs] Failed to register flow '{flowName}'.");
        }

        [Command("flow remove", shortHand: "fr", description: "Remove a flow", adminOnly: false)]
        public static void FlowRemove(ChatCommandContext ctx, string flowName)
        {
            if (!CoreAuthCommands.RequireAdminOrConsole(ctx))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(flowName))
            {
                ctx.Reply("[Jobs] Usage: .jobs flow remove <flow>");
                return;
            }

            var ok = FlowService.RemoveFlow(flowName.Trim());
            ctx.Reply(ok ? $"[Jobs] Flow '{flowName}' removed." : $"[Jobs] Flow '{flowName}' not found.");
        }

        [Command("flow list", shortHand: "fl", description: "List registered flows", adminOnly: false)]
        public static void FlowList(ChatCommandContext ctx)
        {
            var names = FlowService.GetFlowNames().OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToArray();
            if (names.Length == 0)
            {
                ctx.Reply("[Jobs] No flows registered.");
                return;
            }

            ctx.Reply($"[Jobs] Flows ({names.Length}): {string.Join(", ", names)}");
        }

        [Command("action add", shortHand: "aa", description: "Register or replace an action alias", adminOnly: false)]
        public static void ActionAdd(ChatCommandContext ctx, string alias, string actionName)
        {
            if (!CoreAuthCommands.RequireAdminOrConsole(ctx))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(alias) || string.IsNullOrWhiteSpace(actionName))
            {
                ctx.Reply("[Jobs] Usage: .jobs action add <alias> <action>");
                return;
            }

            var ok = FlowService.RegisterActionAlias(alias.Trim(), actionName.Trim(), replace: true);
            ctx.Reply(ok
                ? $"[Jobs] Action alias '{alias}' => '{actionName}'."
                : $"[Jobs] Failed to register action alias '{alias}'.");
        }

        [Command("action remove", shortHand: "ar", description: "Remove action alias", adminOnly: false)]
        public static void ActionRemove(ChatCommandContext ctx, string alias)
        {
            if (!CoreAuthCommands.RequireAdminOrConsole(ctx))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(alias))
            {
                ctx.Reply("[Jobs] Usage: .jobs action remove <alias>");
                return;
            }

            var ok = FlowService.RemoveActionAlias(alias.Trim());
            ctx.Reply(ok ? $"[Jobs] Action alias '{alias}' removed." : $"[Jobs] Action alias '{alias}' not found.");
        }

        [Command("action list", shortHand: "al", description: "List action aliases", adminOnly: false)]
        public static void ActionList(ChatCommandContext ctx)
        {
            var aliases = FlowService.GetActionAliases()
                .OrderBy(kvp => kvp.Key, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (aliases.Length == 0)
            {
                ctx.Reply("[Jobs] No action aliases registered.");
                return;
            }

            ctx.Reply($"[Jobs] Action aliases ({aliases.Length}):");
            foreach (var kvp in aliases)
            {
                ctx.Reply($"  {kvp.Key} => {kvp.Value}");
            }
        }

        [Command("alias self", shortHand: "as", description: "Map your character entity to alias", adminOnly: false)]
        public static void AliasSelf(ChatCommandContext ctx, string alias)
        {
            if (!CoreAuthCommands.RequireAdminOrConsole(ctx))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(alias))
            {
                ctx.Reply("[Jobs] Usage: .jobs alias self <alias>");
                return;
            }

            if (!CoreAuthCommands.TryGetSenderCharacterEntity(ctx, out var senderCharacter))
            {
                ctx.Reply("[Jobs] Character entity unavailable. Use '.jobs alias user' from console.");
                return;
            }

            var subjectId = CoreAuthCommands.ResolveSubjectId(ctx);
            var map = JobFlowSessionStore.GetOrCreate(subjectId);
            map.Map(alias.Trim(), senderCharacter, replace: true);

            ctx.Reply($"[Jobs] Alias '{alias}' mapped to character entity {senderCharacter.Index}:{senderCharacter.Version}.");
        }

        [Command("alias user", shortHand: "au", description: "Map a user entity by platform id", adminOnly: false)]
        public static void AliasUser(ChatCommandContext ctx, string alias, ulong platformId = 0)
        {
            if (!CoreAuthCommands.RequireAdminOrConsole(ctx))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(alias))
            {
                ctx.Reply("[Jobs] Usage: .jobs alias user <alias> [platformId]");
                return;
            }

            var targetPlatformId = platformId;
            if (targetPlatformId == 0)
            {
                targetPlatformId = CoreAuthCommands.ResolveSubjectId(ctx);
                if (targetPlatformId == ConsoleRoleAuthService.ConsoleSubjectId || targetPlatformId == 0)
                {
                    ctx.Reply("[Jobs] Platform id is required from console.");
                    return;
                }
            }

            var subjectId = CoreAuthCommands.ResolveSubjectId(ctx);
            var map = JobFlowSessionStore.GetOrCreate(subjectId);
            if (!map.TryMapUserByPlatformId(alias.Trim(), targetPlatformId, replace: true))
            {
                ctx.Reply($"[Jobs] Failed to map alias '{alias}' to platform '{targetPlatformId}'.");
                return;
            }

            if (map.TryGet(alias.Trim(), out var entity))
            {
                ctx.Reply($"[Jobs] Alias '{alias}' mapped to user entity {entity.Index}:{entity.Version} (platform {targetPlatformId}).");
            }
            else
            {
                ctx.Reply($"[Jobs] Alias '{alias}' mapped (platform {targetPlatformId}).");
            }
        }

        [Command("alias clear", shortHand: "ac", description: "Clear one alias or all aliases", adminOnly: false)]
        public static void AliasClear(ChatCommandContext ctx, string alias = "*")
        {
            if (!CoreAuthCommands.RequireAdminOrConsole(ctx))
            {
                return;
            }

            var subjectId = CoreAuthCommands.ResolveSubjectId(ctx);
            var map = JobFlowSessionStore.GetOrCreate(subjectId);
            if (string.Equals(alias, "*", StringComparison.OrdinalIgnoreCase))
            {
                map.Clear();
                ctx.Reply("[Jobs] All aliases cleared.");
                return;
            }

            var removed = map.Remove(alias.Trim());
            ctx.Reply(removed ? $"[Jobs] Alias '{alias}' removed." : $"[Jobs] Alias '{alias}' not found.");
        }

        [Command("alias list", shortHand: "asl", description: "List aliases in your entity map", adminOnly: false)]
        public static void AliasList(ChatCommandContext ctx)
        {
            var subjectId = CoreAuthCommands.ResolveSubjectId(ctx);
            if (!JobFlowSessionStore.TryGet(subjectId, out var map))
            {
                ctx.Reply("[Jobs] No aliases mapped.");
                return;
            }

            var snapshot = map.Snapshot()
                .OrderBy(kvp => kvp.Key, StringComparer.OrdinalIgnoreCase)
                .ToArray();
            if (snapshot.Length == 0)
            {
                ctx.Reply("[Jobs] No aliases mapped.");
                return;
            }

            ctx.Reply($"[Jobs] Aliases ({snapshot.Length}):");
            foreach (var kvp in snapshot)
            {
                ctx.Reply($"  {kvp.Key} => {kvp.Value.Index}:{kvp.Value.Version}");
            }
        }

        [Command("component add", shortHand: "ca", description: "Register component alias from type name", adminOnly: false)]
        public static void ComponentAliasAdd(ChatCommandContext ctx, string alias, string componentTypeName)
        {
            if (!CoreAuthCommands.RequireAdminOrConsole(ctx))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(alias) || string.IsNullOrWhiteSpace(componentTypeName))
            {
                ctx.Reply("[Jobs] Usage: .jobs component add <alias> <componentType>");
                return;
            }

            var ok = EntityAliasMapper.RegisterComponentAlias(alias.Trim(), componentTypeName.Trim(), replace: true);
            ctx.Reply(ok
                ? $"[Jobs] Component alias '{alias}' registered."
                : $"[Jobs] Failed to register component alias '{alias}' (type not found or unsupported).");
        }

        [Command("component list", shortHand: "cl", description: "List component aliases", adminOnly: false)]
        public static void ComponentAliasList(ChatCommandContext ctx)
        {
            var aliases = EntityAliasMapper.GetAliases()
                .OrderBy(kvp => kvp.Key, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (aliases.Length == 0)
            {
                ctx.Reply("[Jobs] No component aliases registered.");
                return;
            }

            ctx.Reply($"[Jobs] Component aliases ({aliases.Length}):");
            foreach (var kvp in aliases)
            {
                ctx.Reply($"  {kvp.Key} => {kvp.Value.FullName}");
            }
        }

        [Command("component has", shortHand: "ch", description: "Check if aliased entity has aliased component", adminOnly: false)]
        public static void ComponentHas(ChatCommandContext ctx, string entityAlias, string componentAlias)
        {
            var subjectId = CoreAuthCommands.ResolveSubjectId(ctx);
            var map = JobFlowSessionStore.GetOrCreate(subjectId);

            try
            {
                var em = UnifiedCore.EntityManager;
                if (!EntityAliasMapper.HasComponent(em, map, entityAlias, componentAlias, out var has, out var error))
                {
                    ctx.Reply($"[Jobs] {error}");
                    return;
                }

                ctx.Reply($"[Jobs] Entity alias '{entityAlias}' has component '{componentAlias}': {has}");
            }
            catch (Exception ex)
            {
                ctx.Reply($"[Jobs] Component check failed: {ex.Message}");
            }
        }

        [Command("run", shortHand: "r", description: "Execute a registered flow as a job", adminOnly: false)]
        public static void RunFlow(ChatCommandContext ctx, string flowName, bool stopOnFailure = true)
        {
            if (string.IsNullOrWhiteSpace(flowName))
            {
                ctx.Reply("[Jobs] Usage: .jobs run <flow>");
                return;
            }

            var subjectId = CoreAuthCommands.ResolveSubjectId(ctx);
            var map = JobFlowSessionStore.GetOrCreate(subjectId);
            var result = FlowService.ExecuteJobFlow(flowName.Trim(), map, subjectId, stopOnFailure);
            if (result.Success)
            {
                ctx.Reply($"[Jobs] Flow '{flowName}' executed ({result.ExecutedSteps}/{result.TotalSteps} step(s)).");
                return;
            }

            var detail = string.IsNullOrWhiteSpace(result.ErrorMessage)
                ? "Flow failed."
                : result.ErrorMessage;
            ctx.Reply($"[Jobs] {detail} (executed={result.ExecutedSteps}, failed={result.FailedSteps}, total={result.TotalSteps})");
        }
    }
}
