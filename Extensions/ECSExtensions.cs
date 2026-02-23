using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Entities;
using Il2CppInterop.Runtime;
using ProjectM;

namespace VAuto.Extensions
{
    /// <summary>
    /// IL2CPP-compatible ECS extension methods for Entity operations
    /// </summary>
    public static class ECSExtensions
    {
        /// <summary>
        /// Unsafe raw component read (matches KindredExtract signature)
        /// </summary>
        public unsafe static T Read<T>(this Entity entity) where T : struct
        {
            var ct = new ComponentType(Il2CppType.Of<T>());
            if (ct.IsZeroSized)
                return new T();
            
            void* rawPointer = UnifiedCore.EntityManager.GetComponentDataRawRO(entity, ct.TypeIndex);
            return Marshal.PtrToStructure<T>(new IntPtr(rawPointer));
        }

        /// <summary>
        /// Unsafe raw component write (matches KindredExtract signature)
        /// </summary>
        public unsafe static void Write<T>(this Entity entity, T componentData) where T : struct
        {
            var ct = new ComponentType(Il2CppType.Of<T>());
            byte[] byteArray = StructureToByteArray(componentData);
            int size = Marshal.SizeOf<T>();
            
            fixed (byte* p = byteArray)
            {
                UnifiedCore.EntityManager.SetComponentDataRaw(entity, ct.TypeIndex, p, size);
            }
        }

        /// <summary>
        /// Check if entity has component (matches KindredExtract signature)
        /// </summary>
        public static bool Has<T>(this Entity entity)
        {
            var ct = new ComponentType(Il2CppType.Of<T>());
            return UnifiedCore.EntityManager.HasComponent(entity, ct);
        }

        /// <summary>
        /// Add component to entity (matches KindredExtract signature)
        /// </summary>
        public static void Add<T>(this Entity entity)
        {
            var ct = new ComponentType(Il2CppType.Of<T>());
            UnifiedCore.EntityManager.AddComponent(entity, ct);
        }

        /// <summary>
        /// Remove component from entity (matches KindredExtract signature)
        /// </summary>
        public static void Remove<T>(this Entity entity)
        {
            var ct = new ComponentType(Il2CppType.Of<T>());
            UnifiedCore.EntityManager.RemoveComponent(entity, ct);
        }

        /// <summary>
        /// Get prefab name from PrefabGUID (matches KindredExtract signature)
        /// </summary>
        public static string LookupName(this PrefabGUID prefabGuid)
        {
            var prefabCollectionSystem = UnifiedCore.Server.GetExistingSystemManaged<PrefabCollectionSystem>();
            return prefabCollectionSystem.PrefabGuidToNameDictionary.TryGetValue(prefabGuid, out var name)
                ? $"{name} PrefabGuid({prefabGuid.GuidHash})"
                : "GUID Not Found";
        }

        /// <summary>
        /// Helper to marshal struct to byte array
        /// </summary>
        public static byte[] StructureToByteArray<T>(T structure) where T : struct
        {
            int size = Marshal.SizeOf(structure);
            byte[] byteArray = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(structure, ptr, true);
            Marshal.Copy(ptr, byteArray, 0, size);
            Marshal.FreeHGlobal(ptr);
            return byteArray;
        }

        /// <summary>
        /// Safe entity read component (IL2CPP compatible)
        /// </summary>
        public static T Read<T>(this Entity entity, EntityManager em) where T : struct
        {
            try
            {
                if (!em.Exists(entity))
                {
                    Plugin.Log.LogWarning($"[ECSExtensions] Attempting to read from non-existent entity: {entity}");
                    return default;
                }

                if (em.HasComponent<T>(entity))
                {
                    return em.GetComponentData<T>(entity);
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"[ECSExtensions] Error reading component {typeof(T).Name} from entity {entity}: {ex.Message}");
            }
            return default;
        }

        /// <summary>
        /// Safe entity write component using EntityCommandBuffer for thread safety (IL2CPP compatible)
        /// </summary>
        public static void Write<T>(this Entity entity, EntityManager em, T component) where T : struct
        {
            try
            {
                if (!em.Exists(entity))
                {
                    Plugin.Log.LogWarning($"[ECSExtensions] Attempting to write to non-existent entity: {entity}");
                    return;
                }

                var ecb = new EntityCommandBuffer(Allocator.Temp);
                if (em.HasComponent<T>(entity))
                {
                    ecb.SetComponent(entity, component);
                }
                else
                {
                    ecb.AddComponent(entity, component);
                }
                ecb.Playback(em);
                ecb.Dispose();
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"[ECSExtensions] Error writing component {typeof(T).Name} to entity {entity}: {ex.Message}");
            }
        }

        /// <summary>
        /// Get entities by component type using IL2CPP-safe EntityQueryBuilder
        /// </summary>
        public static NativeArray<Entity> GetEntitiesByComponentType<T>(EntityManager em, bool includeDisabled = false) where T : struct
        {
            EntityQueryOptions options = EntityQueryOptions.Default;
            if (includeDisabled) options |= EntityQueryOptions.IncludeDisabled;

            var entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
                .AddAll(new ComponentType(Il2CppType.Of<T>(), ComponentType.AccessMode.ReadWrite));

            var query = em.CreateEntityQuery(ref entityQueryBuilder);
            var entities = query.ToEntityArray(Allocator.Temp);
            entityQueryBuilder.Dispose();
            return entities;
        }

        /// <summary>
        /// Get entities by component types using IL2CPP-safe EntityQueryBuilder
        /// </summary>
        public static NativeArray<Entity> GetEntitiesByComponentTypes<T1, T2>(EntityManager em, bool includeDisabled = false)
            where T1 : struct
            where T2 : struct
        {
            EntityQueryOptions options = EntityQueryOptions.Default;
            if (includeDisabled) options |= EntityQueryOptions.IncludeDisabled;

            var entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
                .AddAll(new ComponentType(Il2CppType.Of<T1>(), ComponentType.AccessMode.ReadWrite))
                .AddAll(new ComponentType(Il2CppType.Of<T2>(), ComponentType.AccessMode.ReadWrite));

            var query = em.CreateEntityQuery(ref entityQueryBuilder);
            var entities = query.ToEntityArray(Allocator.Temp);
            entityQueryBuilder.Dispose();
            return entities;
        }
    }
}
