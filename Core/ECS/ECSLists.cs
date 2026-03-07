using System;
using System.Collections.Generic;
using System.Linq;
using ProjectM;
using Stunlock.Core;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using VAutomationCore.Core.Logging;

namespace VAutomationCore.Core.ECS
{
    /// <summary>
    /// Comprehensive ECS list system for accessing all ECS entities, components, and queries.
    /// Provides centralized access to ECS data structures and entity management.
    /// </summary>
    public static class ECSLists
    {
        private static readonly CoreLogger Log = new("ECSLists");

        #region Entity Lists

        /// <summary>
        /// Get all active entities in the world.
        /// </summary>
        public static List<Entity> GetAllEntities()
        {
            var entities = new List<Entity>();
            
            try
            {
                var world = Core.World;
                if (world != null && world.IsCreated)
                {
                    var query = world.EntityManager.CreateEntityQuery(ComponentType.ReadOnly<Entity>());
                    var entitiesArray = query.ToComponentDataArray<Entity>(Allocator.TempJob);
                    
                    entities.AddRange(entitiesArray);
                    
                    entitiesArray.Dispose();
                    query.Dispose();
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"Failed to get all entities: {ex.Message}");
            }
            
            return entities;
        }

        /// <summary>
        /// Get all player entities.
        /// </summary>
        public static List<Entity> GetAllPlayerEntities()
        {
            var players = new List<Entity>();
            
            try
            {
                var world = Core.World;
                if (world != null && world.IsCreated)
                {
                    var query = world.EntityManager.CreateEntityQuery(
                        ComponentType.ReadOnly<PlayerCharacter>(),
                        ComponentType.ReadOnly<Entity>()
                    );
                    
                    var entitiesArray = query.ToComponentDataArray<Entity>(Allocator.TempJob);
                    
                    players.AddRange(entitiesArray);
                    
                    entitiesArray.Dispose();
                    query.Dispose();
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"Failed to get all player entities: {ex.Message}");
            }
            
            return players;
        }

        /// <summary>
        /// Get all NPC entities.
        /// </summary>
        public static List<Entity> GetAllNPCEntities()
        {
            var npcs = new List<Entity>();
            
            try
            {
                var world = Core.World;
                if (world != null && world.IsCreated)
                {
                    var query = world.EntityManager.CreateEntityQuery(
                        ComponentType.ReadOnly<UnitStats>(),
                        ComponentType.ReadOnly<Entity>(),
                        ComponentType.Exclude<PlayerCharacter>()
                    );
                    
                    var entitiesArray = query.ToComponentDataArray<Entity>(Allocator.TempJob);
                    
                    npcs.AddRange(entitiesArray);
                    
                    entitiesArray.Dispose();
                    query.Dispose();
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"Failed to get all NPC entities: {ex.Message}");
            }
            
            return npcs;
        }

        /// <summary>
        /// Get all building entities.
        /// </summary>
        public static List<Entity> GetAllBuildingEntities()
        {
            var buildings = new List<Entity>();
            
            try
            {
                var world = Core.World;
                if (world != null && world.IsCreated)
                {
                    var query = world.EntityManager.CreateEntityQuery(
                        ComponentType.ReadOnly<Building>(),
                        ComponentType.ReadOnly<Entity>()
                    );
                    
                    var entitiesArray = query.ToComponentDataArray<Entity>(Allocator.TempJob);
                    
                    buildings.AddRange(entitiesArray);
                    
                    entitiesArray.Dispose();
                    query.Dispose();
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"Failed to get all building entities: {ex.Message}");
            }
            
            return buildings;
        }

        /// <summary>
        /// Get all item entities.
        /// </summary>
        public static List<Entity> GetAllItemEntities()
        {
            var items = new List<Entity>();
            
            try
            {
                var world = Core.World;
                if (world != null && world.IsCreated)
                {
                    var query = world.EntityManager.CreateEntityQuery(
                        ComponentType.ReadOnly<ItemData>(),
                        ComponentType.ReadOnly<Entity>()
                    );
                    
                    var entitiesArray = query.ToComponentDataArray<Entity>(Allocator.TempJob);
                    
                    items.AddRange(entitiesArray);
                    
                    entitiesArray.Dispose();
                    query.Dispose();
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"Failed to get all item entities: {ex.Message}");
            }
            
            return items;
        }

        /// <summary>
        /// Get all spawner entities.
        /// </summary>
        public static List<Entity> GetAllSpawnerEntities()
        {
            var spawners = new List<Entity>();
            
            try
            {
                var world = Core.World;
                if (world != null && world.IsCreated)
                {
                    var query = world.EntityManager.CreateEntityQuery(
                        ComponentType.ReadOnly<Spawner>(),
                        ComponentType.ReadOnly<Entity>()
                    );
                    
                    var entitiesArray = query.ToComponentDataArray<Entity>(Allocator.TempJob);
                    
                    spawners.AddRange(entitiesArray);
                    
                    entitiesArray.Dispose();
                    query.Dispose();
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"Failed to get all spawner entities: {ex.Message}");
            }
            
            return spawners;
        }

        #endregion

        #region Component Lists

        /// <summary>
        /// Get all entities with specific component type.
        /// </summary>
        public static List<Entity> GetEntitiesWithComponent<T>() where T : struct
        {
            var entities = new List<Entity>();
            
            try
            {
                var world = Core.World;
                if (world != null && world.IsCreated)
                {
                    var query = world.EntityManager.CreateEntityQuery(
                        ComponentType.ReadOnly<T>(),
                        ComponentType.ReadOnly<Entity>()
                    );
                    
                    var entitiesArray = query.ToComponentDataArray<Entity>(Allocator.TempJob);
                    
                    entities.AddRange(entitiesArray);
                    
                    entitiesArray.Dispose();
                    query.Dispose();
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"Failed to get entities with component {typeof(T).Name}: {ex.Message}");
            }
            
            return entities;
        }

        /// <summary>
        /// Get all entities with multiple component types.
        /// </summary>
        public static List<Entity> GetEntitiesWithComponents<T1, T2>() 
            where T1 : struct 
            where T2 : struct
        {
            var entities = new List<Entity>();
            
            try
            {
                var world = Core.World;
                if (world != null && world.IsCreated)
                {
                    var query = world.EntityManager.CreateEntityQuery(
                        ComponentType.ReadOnly<T1>(),
                        ComponentType.ReadOnly<T2>(),
                        ComponentType.ReadOnly<Entity>()
                    );
                    
                    var entitiesArray = query.ToComponentDataArray<Entity>(Allocator.TempJob);
                    
                    entities.AddRange(entitiesArray);
                    
                    entitiesArray.Dispose();
                    query.Dispose();
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"Failed to get entities with components {typeof(T1).Name}, {typeof(T2).Name}: {ex.Message}");
            }
            
            return entities;
        }

        /// <summary>
        /// Get all entities with three component types.
        /// </summary>
        public static List<Entity> GetEntitiesWithComponents<T1, T2, T3>() 
            where T1 : struct 
            where T2 : struct 
            where T3 : struct
        {
            var entities = new List<Entity>();
            
            try
            {
                var world = Core.World;
                if (world != null && world.IsCreated)
                {
                    var query = world.EntityManager.CreateEntityQuery(
                        ComponentType.ReadOnly<T1>(),
                        ComponentType.ReadOnly<T2>(),
                        ComponentType.ReadOnly<T3>(),
                        ComponentType.ReadOnly<Entity>()
                    );
                    
                    var entitiesArray = query.ToComponentDataArray<Entity>(Allocator.TempJob);
                    
                    entities.AddRange(entitiesArray);
                    
                    entitiesArray.Dispose();
                    query.Dispose();
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"Failed to get entities with components {typeof(T1).Name}, {typeof(T2).Name}, {typeof(T3).Name}: {ex.Message}");
            }
            
            return entities;
        }

        #endregion

        #region Prefab Lists

        /// <summary>
        /// Get all entities by prefab GUID.
        /// </summary>
        public static List<Entity> GetEntitiesByPrefabGUID(PrefabGUID prefabGuid)
        {
            var entities = new List<Entity>();
            
            try
            {
                var world = Core.World;
                if (world != null && world.IsCreated)
                {
                    var query = world.EntityManager.CreateEntityQuery(
                        ComponentType.ReadOnly<PrefabGUID>(),
                        ComponentType.ReadOnly<Entity>()
                    );
                    
                    var prefabGuids = query.ToComponentDataArray<PrefabGUID>(Allocator.TempJob);
                    var entityArray = query.ToEntityArray(Allocator.TempJob);
                    
                    for (int i = 0; i < prefabGuids.Length; i++)
                    {
                        if (prefabGuids[i].Equals(prefabGuid))
                        {
                            entities.Add(entityArray[i]);
                        }
                    }
                    
                    prefabGuids.Dispose();
                    entityArray.Dispose();
                    query.Dispose();
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"Failed to get entities by prefab GUID {prefabGuid}: {ex.Message}");
            }
            
            return entities;
        }

        /// <summary>
        /// Get all entities by prefab name.
        /// </summary>
        public static List<Entity> GetEntitiesByPrefabName(string prefabName)
        {
            var entities = new List<Entity>();
            
            try
            {
                var world = Core.World;
                if (world != null && world.IsCreated)
                {
                    var query = world.EntityManager.CreateEntityQuery(
                        ComponentType.ReadOnly<PrefabGUID>(),
                        ComponentType.ReadOnly<Entity>()
                    );
                    
                    var prefabGuids = query.ToComponentDataArray<PrefabGUID>(Allocator.TempJob);
                    var entityArray = query.ToEntityArray(Allocator.TempJob);
                    
                    for (int i = 0; i < prefabGuids.Length; i++)
                    {
                        var guidName = prefabGuids[i].LookupName();
                        if (guidName.Contains(prefabName, StringComparison.OrdinalIgnoreCase))
                        {
                            entities.Add(entityArray[i]);
                        }
                    }
                    
                    prefabGuids.Dispose();
                    entityArray.Dispose();
                    query.Dispose();
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"Failed to get entities by prefab name {prefabName}: {ex.Message}");
            }
            
            return entities;
        }

        /// <summary>
        /// Get all unique prefab GUIDs from entities.
        /// </summary>
        public static List<PrefabGUID> GetAllUniquePrefabGUIDs()
        {
            var prefabGuids = new List<PrefabGUID>();
            
            try
            {
                var world = Core.World;
                if (world != null && world.IsCreated)
                {
                    var query = world.EntityManager.CreateEntityQuery(
                        ComponentType.ReadOnly<PrefabGUID>()
                    );
                    
                    var prefabGuidArray = query.ToComponentDataArray<PrefabGUID>(Allocator.TempJob);
                    
                    prefabGuids.AddRange(prefabGuidArray.Distinct());
                    
                    prefabGuidArray.Dispose();
                    query.Dispose();
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"Failed to get all unique prefab GUIDs: {ex.Message}");
            }
            
            return prefabGuids;
        }

        #endregion

        #region Position Lists

        /// <summary>
        /// Get all entities within radius of position.
        /// </summary>
        public static List<Entity> GetEntitiesWithinRadius(float3 center, float radius)
        {
            var entities = new List<Entity>();
            
            try
            {
                var world = Core.World;
                if (world != null && world.IsCreated)
                {
                    var query = world.EntityManager.CreateEntityQuery(
                        ComponentType.ReadOnly<Translation>(),
                        ComponentType.ReadOnly<Entity>()
                    );
                    
                    var translations = query.ToComponentDataArray<Translation>(Allocator.TempJob);
                    var entityArray = query.ToEntityArray(Allocator.TempJob);
                    
                    for (int i = 0; i < translations.Length; i++)
                    {
                        var distance = math.distance(center, translations[i].Value);
                        if (distance <= radius)
                        {
                            entities.Add(entityArray[i]);
                        }
                    }
                    
                    translations.Dispose();
                    entityArray.Dispose();
                    query.Dispose();
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"Failed to get entities within radius {radius} of {center}: {ex.Message}");
            }
            
            return entities;
        }

        /// <summary>
        /// Get all entities within bounding box.
        /// </summary>
        public static List<Entity> GetEntitiesWithinBounds(float3 min, float3 max)
        {
            var entities = new List<Entity>();
            
            try
            {
                var world = Core.World;
                if (world != null && world.IsCreated)
                {
                    var query = world.EntityManager.CreateEntityQuery(
                        ComponentType.ReadOnly<Translation>(),
                        ComponentType.ReadOnly<Entity>()
                    );
                    
                    var translations = query.ToComponentDataArray<Translation>(Allocator.TempJob);
                    var entityArray = query.ToEntityArray(Allocator.TempJob);
                    
                    for (int i = 0; i < translations.Length; i++)
                    {
                        var pos = translations[i].Value;
                        if (pos.x >= min.x && pos.x <= max.x &&
                            pos.y >= min.y && pos.y <= max.y &&
                            pos.z >= min.z && pos.z <= max.z)
                        {
                            entities.Add(entityArray[i]);
                        }
                    }
                    
                    translations.Dispose();
                    entityArray.Dispose();
                    query.Dispose();
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"Failed to get entities within bounds {min} to {max}: {ex.Message}");
            }
            
            return entities;
        }

        #endregion

        #region Zone Lists

        /// <summary>
        /// Get all entities in specific zone.
        /// </summary>
        public static List<Entity> GetEntitiesInZone(int zoneId)
        {
            var entities = new List<Entity>();
            
            try
            {
                var world = Core.World;
                if (world != null && world.IsCreated)
                {
                    var query = world.EntityManager.CreateEntityQuery(
                        ComponentType.ReadOnly<ZoneHash>(),
                        ComponentType.ReadOnly<Entity>()
                    );
                    
                    var zoneHashes = query.ToComponentDataArray<ZoneHash>(Allocator.TempJob);
                    var entityArray = query.ToEntityArray(Allocator.TempJob);
                    
                    for (int i = 0; i < zoneHashes.Length; i++)
                    {
                        if (zoneHashes[i].ZoneId == zoneId)
                        {
                            entities.Add(entityArray[i]);
                        }
                    }
                    
                    zoneHashes.Dispose();
                    entityArray.Dispose();
                    query.Dispose();
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"Failed to get entities in zone {zoneId}: {ex.Message}");
            }
            
            return entities;
        }

        /// <summary>
        /// Get all entities in multiple zones.
        /// </summary>
        public static List<Entity> GetEntitiesInZones(int[] zoneIds)
        {
            var entities = new List<Entity>();
            
            try
            {
                var world = Core.World;
                if (world != null && world.IsCreated)
                {
                    var query = world.EntityManager.CreateEntityQuery(
                        ComponentType.ReadOnly<ZoneHash>(),
                        ComponentType.ReadOnly<Entity>()
                    );
                    
                    var zoneHashes = query.ToComponentDataArray<ZoneHash>(Allocator.TempJob);
                    var entityArray = query.ToEntityArray(Allocator.TempJob);
                    
                    for (int i = 0; i < zoneHashes.Length; i++)
                    {
                        if (zoneIds.Contains(zoneHashes[i].ZoneId))
                        {
                            entities.Add(entityArray[i]);
                        }
                    }
                    
                    zoneHashes.Dispose();
                    entityArray.Dispose();
                    query.Dispose();
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"Failed to get entities in zones {string.Join(",", zoneIds)}: {ex.Message}");
            }
            
            return entities;
        }

        #endregion

        #region Tag Lists

        /// <summary>
        /// Get all entities with specific tag.
        /// </summary>
        public static List<Entity> GetEntitiesWithTag(string tagName)
        {
            var entities = new List<Entity>();
            
            try
            {
                var world = Core.World;
                if (world != null && world.IsCreated)
                {
                    var query = world.EntityManager.CreateEntityQuery(
                        ComponentType.ReadOnly<Tag>(),
                        ComponentType.ReadOnly<Entity>()
                    );
                    
                    var tags = query.ToComponentDataArray<Tag>(Allocator.TempJob);
                    var entityArray = query.ToEntityArray(Allocator.TempJob);
                    
                    for (int i = 0; i < tags.Length; i++)
                    {
                        if (tags[i].Name.Equals(tagName, StringComparison.OrdinalIgnoreCase))
                        {
                            entities.Add(entityArray[i]);
                        }
                    }
                    
                    tags.Dispose();
                    entityArray.Dispose();
                    query.Dispose();
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"Failed to get entities with tag {tagName}: {ex.Message}");
            }
            
            return entities;
        }

        /// <summary>
        /// Get all entities with multiple tags.
        /// </summary>
        public static List<Entity> GetEntitiesWithTags(string[] tagNames)
        {
            var entities = new List<Entity>();
            
            try
            {
                var world = Core.World;
                if (world != null && world.IsCreated)
                {
                    var query = world.EntityManager.CreateEntityQuery(
                        ComponentType.ReadOnly<Tag>(),
                        ComponentType.ReadOnly<Entity>()
                    );
                    
                    var tags = query.ToComponentDataArray<Tag>(Allocator.TempJob);
                    var entityArray = query.ToEntityArray(Allocator.TempJob);
                    
                    for (int i = 0; i < tags.Length; i++)
                    {
                        if (tagNames.Contains(tags[i].Name, StringComparer.OrdinalIgnoreCase))
                        {
                            entities.Add(entityArray[i]);
                        }
                    }
                    
                    tags.Dispose();
                    entityArray.Dispose();
                    query.Dispose();
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"Failed to get entities with tags {string.Join(",", tagNames)}: {ex.Message}");
            }
            
            return entities;
        }

        #endregion

        #region Status Lists

        /// <summary>
        /// Get all active entities.
        /// </summary>
        public static List<Entity> GetActiveEntities()
        {
            var entities = new List<Entity>();
            
            try
            {
                var world = Core.World;
                if (world != null && world.IsCreated)
                {
                    var query = world.EntityManager.CreateEntityQuery(
                        ComponentType.ReadOnly<Enabled>(),
                        ComponentType.ReadOnly<Entity>()
                    );
                    
                    var entityArray = query.ToEntityArray(Allocator.TempJob);
                    
                    entities.AddRange(entityArray);
                    
                    entityArray.Dispose();
                    query.Dispose();
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"Failed to get active entities: {ex.Message}");
            }
            
            return entities;
        }

        /// <summary>
        /// Get all disabled entities.
        /// </summary>
        public static List<Entity> GetDisabledEntities()
        {
            var entities = new List<Entity>();
            
            try
            {
                var world = Core.World;
                if (world != null && world.IsCreated)
                {
                    var query = world.EntityManager.CreateEntityQuery(
                        ComponentType.ReadOnly<Disabled>(),
                        ComponentType.ReadOnly<Entity>()
                    );
                    
                    var entityArray = query.ToEntityArray(Allocator.TempJob);
                    
                    entities.AddRange(entityArray);
                    
                    entityArray.Dispose();
                    query.Dispose();
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"Failed to get disabled entities: {ex.Message}");
            }
            
            return entities;
        }

        /// <summary>
        /// Get all destroyed entities.
        /// </summary>
        public static List<Entity> GetDestroyedEntities()
        {
            var entities = new List<Entity>();
            
            try
            {
                var world = Core.World;
                if (world != null && world.IsCreated)
                {
                    var query = world.EntityManager.CreateEntityQuery(
                        ComponentType.ReadOnly<Destroyed>(),
                        ComponentType.ReadOnly<Entity>()
                    );
                    
                    var entityArray = query.ToEntityArray(Allocator.TempJob);
                    
                    entities.AddRange(entityArray);
                    
                    entityArray.Dispose();
                    query.Dispose();
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"Failed to get destroyed entities: {ex.Message}");
            }
            
            return entities;
        }

        #endregion

        #region Specialized Lists

        /// <summary>
        /// Get all entities with health.
        /// </summary>
        public static List<Entity> GetEntitiesWithHealth()
        {
            return GetEntitiesWithComponent<Health>();
        }

        /// <summary>
        /// Get all entities with inventory.
        /// </summary>
        public static List<Entity> GetEntitiesWithInventory()
        {
            return GetEntitiesWithComponent<Inventory>();
        }

        /// <summary>
        /// Get all entities with abilities.
        /// </summary>
        public static List<Entity> GetEntitiesWithAbilities()
        {
            return GetEntitiesWithComponent<Ability>();
        }

        /// <summary>
        /// Get all entities with buffs.
        /// </summary>
        public static List<Entity> GetEntitiesWithBuffs()
        {
            return GetEntitiesWithComponent<Buff>();
        }

        /// <summary>
        /// Get all entities with equipment.
        /// </summary>
        public static List<Entity> GetEntitiesWithEquipment()
        {
            return GetEntitiesWithComponent<Equipment>();
        }

        /// <summary>
        /// Get all entities with movement.
        /// </summary>
        public static List<Entity> GetEntitiesWithMovement()
        {
            return GetEntitiesWithComponent<Movement>();
        }

        /// <summary>
        /// Get all entities with spell casting.
        /// </summary>
        public static List<Entity> GetEntitiesWithSpellCasting()
        {
            return GetEntitiesWithComponent<SpellCasting>();
        }

        #endregion

        #region Count Lists

        /// <summary>
        /// Get count of all entities.
        /// </summary>
        public static int GetEntityCount()
        {
            try
            {
                var world = Core.World;
                if (world != null && world.IsCreated)
                {
                    var query = world.EntityManager.CreateEntityQuery(ComponentType.ReadOnly<Entity>());
                    var count = query.CalculateEntityCount();
                    query.Dispose();
                    return count;
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"Failed to get entity count: {ex.Message}");
            }
            
            return 0;
        }

        /// <summary>
        /// Get count of entities with specific component.
        /// </summary>
        public static int GetEntityCount<T>() where T : struct
        {
            try
            {
                var world = Core.World;
                if (world != null && world.IsCreated)
                {
                    var query = world.EntityManager.CreateEntityQuery(
                        ComponentType.ReadOnly<T>(),
                        ComponentType.ReadOnly<Entity>()
                    );
                    var count = query.CalculateEntityCount();
                    query.Dispose();
                    return count;
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"Failed to get entity count for component {typeof(T).Name}: {ex.Message}");
            }
            
            return 0;
        }

        /// <summary>
        /// Get count of entities by prefab GUID.
        /// </summary>
        public static int GetEntityCountByPrefabGUID(PrefabGUID prefabGuid)
        {
            try
            {
                var world = Core.World;
                if (world != null && world.IsCreated)
                {
                    var query = world.EntityManager.CreateEntityQuery(
                        ComponentType.ReadOnly<PrefabGUID>(),
                        ComponentType.ReadOnly<Entity>()
                    );
                    
                    var prefabGuids = query.ToComponentDataArray<PrefabGUID>(Allocator.TempJob);
                    var count = prefabGuids.Count(pg => pg.Equals(prefabGuid));
                    
                    prefabGuids.Dispose();
                    query.Dispose();
                    return count;
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"Failed to get entity count for prefab GUID {prefabGuid}: {ex.Message}");
            }
            
            return 0;
        }

        #endregion

        #region Utility Lists

        /// <summary>
        /// Get all component types present in the world.
        /// </summary>
        public static List<Type> GetAllComponentTypes()
        {
            var componentTypes = new List<Type>();
            
            try
            {
                var world = Core.World;
                if (world != null && world.IsCreated)
                {
                    var allComponentTypes = world.EntityManager.Debug.GetExistingComponentTypes();
                    componentTypes.AddRange(allComponentTypes);
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"Failed to get all component types: {ex.Message}");
            }
            
            return componentTypes;
        }

        /// <summary>
        /// Get all archetype information.
        /// </summary>
        public static List<string> GetAllArchetypeInfo()
        {
            var archetypes = new List<string>();
            
            try
            {
                var world = Core.World;
                if (world != null && world.IsCreated)
                {
                    var allArchetypes = world.EntityManager.Debug.GetExistingArchetypes();
                    foreach (var archetype in allArchetypes)
                    {
                        var info = $"Archetype: {archetype.ToString()}, Entities: {archetype.EntityCount}";
                        archetypes.Add(info);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"Failed to get archetype info: {ex.Message}");
            }
            
            return archetypes;
        }

        /// <summary>
        /// Get ECS system performance information.
        /// </summary>
        public static Dictionary<string, object> GetECSPerformanceInfo()
        {
            var performance = new Dictionary<string, object>();
            
            try
            {
                var world = Core.World;
                if (world != null && world.IsCreated)
                {
                    performance["TotalEntities"] = GetEntityCount();
                    performance["PlayerEntities"] = GetAllPlayerEntities().Count;
                    performance["NPCEntities"] = GetAllNPCEntities().Count;
                    performance["BuildingEntities"] = GetAllBuildingEntities().Count;
                    performance["ItemEntities"] = GetAllItemEntities().Count;
                    performance["SpawnerEntities"] = GetAllSpawnerEntities().Count;
                    performance["ComponentTypes"] = GetAllComponentTypes().Count;
                    performance["ArchetypeCount"] = GetAllArchetypeInfo().Count;
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"Failed to get ECS performance info: {ex.Message}");
            }
            
            return performance;
        }

        #endregion
    }
}
