using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using ProjectM;
using Unity.Collections;
using Unity.Entities;
using VAutomationCore.Core.Events;
using VAutomationCore.Core.Logging;

namespace VAutomationCore.Core.Patches
{
    /// <summary>
    /// Emits gameplay-safe player downed events from the verified server-side downed system.
    /// The exact system name is verified from KindredExtract query extraction.
    /// </summary>
    [HarmonyPatch(typeof(VampireDownedServerEventSystem), nameof(VampireDownedServerEventSystem.OnUpdate))]
    internal static class DownedEventSystemPatch
    {
        private static readonly string[] PlayerMemberNames =
        {
            "Player", "Character", "Downed", "Victim", "Target", "Died"
        };

        private static readonly string[] AttackerMemberNames =
        {
            "Attacker", "Source", "Killer", "Damager", "Causer", "Instigator"
        };

        [HarmonyPostfix]
        private static void OnUpdatePostfix(VampireDownedServerEventSystem __instance)
        {
            try
            {
                var em = __instance.EntityManager;
                var seenEntities = new HashSet<int>();
                foreach (var query in GetCandidateQueries(__instance))
                {
                    NativeArray<Entity> eventEntities = default;
                    try
                    {
                        eventEntities = query.ToEntityArray(Allocator.Temp);
                        foreach (var eventEntity in eventEntities)
                        {
                            if (!seenEntities.Add(eventEntity.Index) || !em.Exists(eventEntity))
                            {
                                continue;
                            }

                            if (!TryExtractDownedContext(em, eventEntity, out var player, out var attacker))
                            {
                                continue;
                            }

                            EventDispatcher.Publish(new VAutomationCore.Core.Events.PlayerDownedEvent
                            {
                                Player = player,
                                Attacker = attacker
                            });
                        }
                    }
                    finally
                    {
                        if (eventEntities.IsCreated)
                        {
                            eventEntities.Dispose();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CoreLogger.LogException(ex, "Error processing downed events");
            }
        }

        private static List<EntityQuery> GetCandidateQueries(object system)
        {
            var queries = new List<EntityQuery>();
            foreach (var field in system.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (field.FieldType != typeof(EntityQuery))
                {
                    continue;
                }

                var includeField = field.Name.Contains("Downed", StringComparison.OrdinalIgnoreCase)
                    || field.Name.Contains("Event", StringComparison.OrdinalIgnoreCase)
                    || queries.Count == 0;
                if (!includeField)
                {
                    continue;
                }

                if (field.GetValue(system) is EntityQuery query)
                {
                    queries.Add(query);
                }
            }

            return queries;
        }

        private static bool TryExtractDownedContext(EntityManager em, Entity eventEntity, out Entity player, out Entity attacker)
        {
            player = Entity.Null;
            attacker = Entity.Null;

            var componentTypes = em.GetComponentTypes(eventEntity);
            try
            {
                foreach (var componentType in componentTypes)
                {
                    if (!TryReadComponentData(em, eventEntity, componentType, out var componentData))
                    {
                        continue;
                    }

                    if (componentData == null)
                    {
                        continue;
                    }

                    CaptureEntityMembers(componentData, ref player, ref attacker);
                    if (player != Entity.Null && attacker != Entity.Null)
                    {
                        break;
                    }
                }
            }
            finally
            {
                componentTypes.Dispose();
            }

            return player != Entity.Null;
        }

        private static bool TryReadComponentData(EntityManager em, Entity eventEntity, ComponentType componentType, out object? componentData)
        {
            componentData = null;

            try
            {
                var managedType = ResolveManagedType(componentType.GetManagedType());
                if (managedType == null)
                {
                    return false;
                }

                var getter = typeof(EntityManager)
                    .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                    .FirstOrDefault(m => m.Name == nameof(EntityManager.GetComponentData)
                        && m.IsGenericMethodDefinition
                        && m.GetParameters().Length == 1);

                if (getter == null)
                {
                    return false;
                }

                var concreteGetter = getter.MakeGenericMethod(managedType);
                componentData = concreteGetter.Invoke(em, new object[] { eventEntity });
                return componentData != null;
            }
            catch
            {
                return false;
            }
        }

        private static Type? ResolveManagedType(object? rawType)
        {
            if (rawType == null)
            {
                return null;
            }

            if (rawType is Type directType)
            {
                return directType;
            }

            var rawClrType = rawType.GetType();
            var aqn = rawClrType.GetProperty("AssemblyQualifiedName", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(rawType) as string;
            var resolved = ResolveTypeByName(aqn);
            if (resolved != null)
            {
                return resolved;
            }

            var fullName = rawClrType.GetProperty("FullName", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(rawType) as string;
            return ResolveTypeByName(fullName);
        }

        private static Type? ResolveTypeByName(string? typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
            {
                return null;
            }

            var resolved = Type.GetType(typeName, false);
            if (resolved != null)
            {
                return resolved;
            }

            var shortName = typeName.Split(',')[0].Trim();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                resolved = assembly.GetType(shortName, false);
                if (resolved != null)
                {
                    return resolved;
                }
            }

            return null;
        }

        private static void CaptureEntityMembers(object componentData, ref Entity player, ref Entity attacker)
        {
            var fallbackEntities = new List<Entity>(2);
            var componentType = componentData.GetType();

            foreach (var field in componentType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (field.FieldType != typeof(Entity))
                {
                    continue;
                }

                CaptureEntityValue(field.Name, (Entity)field.GetValue(componentData)!, ref player, ref attacker, fallbackEntities);
            }

            foreach (var property in componentType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (property.PropertyType != typeof(Entity) || property.GetIndexParameters().Length != 0 || !property.CanRead)
                {
                    continue;
                }

                object? rawValue;
                try
                {
                    rawValue = property.GetValue(componentData);
                }
                catch
                {
                    continue;
                }

                if (rawValue is Entity entityValue)
                {
                    CaptureEntityValue(property.Name, entityValue, ref player, ref attacker, fallbackEntities);
                }
            }

            if (player == Entity.Null && fallbackEntities.Count > 0)
            {
                player = fallbackEntities[0];
            }

            if (attacker == Entity.Null && fallbackEntities.Count > 1)
            {
                attacker = fallbackEntities[1];
            }
        }

        private static void CaptureEntityValue(string memberName, Entity value, ref Entity player, ref Entity attacker, List<Entity> fallbackEntities)
        {
            if (value == Entity.Null)
            {
                return;
            }

            if (player == Entity.Null && Matches(memberName, PlayerMemberNames))
            {
                player = value;
                return;
            }

            if (attacker == Entity.Null && Matches(memberName, AttackerMemberNames))
            {
                attacker = value;
                return;
            }

            if (!fallbackEntities.Contains(value))
            {
                fallbackEntities.Add(value);
            }
        }

        private static bool Matches(string memberName, IEnumerable<string> candidates)
        {
            foreach (var candidate in candidates)
            {
                if (memberName.Contains(candidate, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}