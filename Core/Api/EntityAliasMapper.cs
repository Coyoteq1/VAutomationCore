using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Entities;
using Unity.Transforms;
using VAutomationCore.Core.Data;

namespace VAutomationCore.Core.Api
{
    /// <summary>
    /// Registers user-friendly aliases for ECS component types and maps aliases to component access.
    /// </summary>
    public static class EntityAliasMapper
    {
        private static readonly ConcurrentDictionary<string, Type> ComponentAliases = new(StringComparer.OrdinalIgnoreCase);

        private static readonly MethodInfo HasComponentGeneric = typeof(EntityManager)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .First(m => m.Name == "HasComponent" &&
                        m.IsGenericMethodDefinition &&
                        m.GetGenericArguments().Length == 1 &&
                        m.GetParameters().Length == 1 &&
                        m.GetParameters()[0].ParameterType == typeof(Entity));

        private static readonly MethodInfo GetComponentDataGeneric = typeof(EntityManager)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .First(m => m.Name == "GetComponentData" &&
                        m.IsGenericMethodDefinition &&
                        m.GetGenericArguments().Length == 1 &&
                        m.GetParameters().Length == 1 &&
                        m.GetParameters()[0].ParameterType == typeof(Entity));

        private static readonly MethodInfo SetComponentDataGeneric = typeof(EntityManager)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .First(m => m.Name == "SetComponentData" &&
                        m.IsGenericMethodDefinition &&
                        m.GetGenericArguments().Length == 1 &&
                        m.GetParameters().Length == 2 &&
                        m.GetParameters()[0].ParameterType == typeof(Entity));

        static EntityAliasMapper()
        {
            RegisterComponentAlias<LocalTransform>("local_transform");
            RegisterComponentAlias<Translation>("translation");
            RegisterComponentAlias<ConsoleRoleComponent>("console_role");
        }

        public static bool RegisterComponentAlias<T>(string alias, bool replace = false) where T : struct
        {
            return RegisterComponentAlias(alias, typeof(T), replace);
        }

        public static bool RegisterComponentAlias(string alias, Type componentType, bool replace = false)
        {
            if (string.IsNullOrWhiteSpace(alias) || componentType == null)
            {
                return false;
            }

            if (!IsSupportedComponentType(componentType))
            {
                return false;
            }

            var key = alias.Trim();
            if (replace)
            {
                ComponentAliases[key] = componentType;
                return true;
            }

            return ComponentAliases.TryAdd(key, componentType);
        }

        public static bool RegisterComponentAlias(string alias, string componentTypeName, bool replace = false)
        {
            if (string.IsNullOrWhiteSpace(componentTypeName))
            {
                return false;
            }

            var resolved = ResolveType(componentTypeName.Trim());
            return RegisterComponentAlias(alias, resolved, replace);
        }

        public static bool RemoveComponentAlias(string alias)
        {
            if (string.IsNullOrWhiteSpace(alias))
            {
                return false;
            }

            return ComponentAliases.TryRemove(alias.Trim(), out _);
        }

        public static bool TryResolveComponentAlias(string alias, out Type componentType)
        {
            componentType = null!;
            if (string.IsNullOrWhiteSpace(alias))
            {
                return false;
            }

            return ComponentAliases.TryGetValue(alias.Trim(), out componentType);
        }

        public static IReadOnlyDictionary<string, Type> GetAliases()
        {
            return new Dictionary<string, Type>(ComponentAliases, StringComparer.OrdinalIgnoreCase);
        }

        public static bool HasComponent(EntityManager em, EntityMap entityMap, string entityAlias, string componentAlias, out bool has, out string error)
        {
            has = false;
            error = string.Empty;

            if (!TryResolve(em, entityMap, entityAlias, componentAlias, out var entity, out var componentType, out error))
            {
                return false;
            }

            try
            {
                var method = HasComponentGeneric.MakeGenericMethod(componentType);
                has = (bool)method.Invoke(em, new object[] { entity })!;
                return true;
            }
            catch (Exception ex)
            {
                error = $"HasComponent failed: {ex.Message}";
                return false;
            }
        }

        public static bool TryGetComponent(EntityManager em, EntityMap entityMap, string entityAlias, string componentAlias, out object component, out string error)
        {
            component = default!;
            error = string.Empty;

            if (!TryResolve(em, entityMap, entityAlias, componentAlias, out var entity, out var componentType, out error))
            {
                return false;
            }

            try
            {
                var hasMethod = HasComponentGeneric.MakeGenericMethod(componentType);
                var exists = (bool)hasMethod.Invoke(em, new object[] { entity })!;
                if (!exists)
                {
                    error = $"Entity alias '{entityAlias}' does not contain component alias '{componentAlias}'.";
                    return false;
                }

                var getMethod = GetComponentDataGeneric.MakeGenericMethod(componentType);
                component = getMethod.Invoke(em, new object[] { entity })!;
                return true;
            }
            catch (Exception ex)
            {
                error = $"GetComponent failed: {ex.Message}";
                return false;
            }
        }

        public static bool TrySetComponent(EntityManager em, EntityMap entityMap, string entityAlias, string componentAlias, object componentValue, out string error)
        {
            error = string.Empty;
            if (!TryResolve(em, entityMap, entityAlias, componentAlias, out var entity, out var componentType, out error))
            {
                return false;
            }

            if (componentValue == null)
            {
                error = "Component value is null.";
                return false;
            }

            if (componentValue.GetType() != componentType)
            {
                error = $"Component value type mismatch. Expected '{componentType.FullName}', got '{componentValue.GetType().FullName}'.";
                return false;
            }

            try
            {
                var hasMethod = HasComponentGeneric.MakeGenericMethod(componentType);
                var exists = (bool)hasMethod.Invoke(em, new object[] { entity })!;
                if (!exists)
                {
                    error = $"Entity alias '{entityAlias}' does not contain component alias '{componentAlias}'.";
                    return false;
                }

                var setMethod = SetComponentDataGeneric.MakeGenericMethod(componentType);
                setMethod.Invoke(em, new[] { (object)entity, componentValue });
                return true;
            }
            catch (Exception ex)
            {
                error = $"SetComponent failed: {ex.Message}";
                return false;
            }
        }

        private static bool TryResolve(EntityManager em, EntityMap entityMap, string entityAlias, string componentAlias, out Entity entity, out Type componentType, out string error)
        {
            entity = Entity.Null;
            componentType = null!;
            error = string.Empty;

            if (entityMap == null)
            {
                error = "Entity map is null.";
                return false;
            }

            if (!entityMap.TryGet(entityAlias, out entity))
            {
                error = $"Entity alias '{entityAlias}' not found.";
                return false;
            }

            if (entity == Entity.Null || !em.Exists(entity))
            {
                error = $"Entity alias '{entityAlias}' is invalid or no longer exists.";
                return false;
            }

            if (!TryResolveComponentAlias(componentAlias, out componentType))
            {
                error = $"Component alias '{componentAlias}' not registered.";
                return false;
            }

            return true;
        }

        private static bool IsSupportedComponentType(Type type)
        {
            if (type == null || !type.IsValueType)
            {
                return false;
            }

            return typeof(IComponentData).IsAssignableFrom(type)
                   || typeof(IBufferElementData).IsAssignableFrom(type);
        }

        private static Type ResolveType(string fullName)
        {
            var direct = Type.GetType(fullName, throwOnError: false, ignoreCase: false);
            if (direct != null)
            {
                return direct;
            }

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var resolved = asm.GetType(fullName, throwOnError: false, ignoreCase: false);
                    if (resolved != null)
                    {
                        return resolved;
                    }
                }
                catch
                {
                    // Ignore dynamic/load failures and continue scanning.
                }
            }

            return null!;
        }
    }
}
