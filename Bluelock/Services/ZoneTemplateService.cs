using System;
using System.Collections.Generic;
using System.Linq;
using ProjectM;
using Unity.Entities;
using Unity.Mathematics;
using VAuto.Zone.Core;
using VAuto.Zone.Models;

namespace VAuto.Zone.Services
{
    public static class ZoneTemplateService
    {
        public static bool TryValidateZoneSpawnLocation(string zoneId, out string error)
        {
            error = string.Empty;
            if (string.IsNullOrWhiteSpace(zoneId))
            {
                error = "Zone id is required.";
                return false;
            }

            var zone = ZoneConfigService.GetZoneById(zoneId);
            if (zone == null)
            {
                error = $"Zone '{zoneId}' not found.";
                return false;
            }

            if (!TryFindSpawnLocationConflict(zoneId, zone, out var conflictingZoneId))
            {
                return true;
            }

            error = $"Template spawn blocked: zone '{zoneId}' matches location with active template zone '{conflictingZoneId}'.";
            return false;
        }

        public static TemplateSpawnResult SpawnZoneTemplateType(string zoneId, string templateType, EntityManager em)
        {
            if (em == default || em.World == null || !em.World.IsCreated)
            {
                return new TemplateSpawnResult
                {
                    Success = false,
                    Error = "EntityManager world not ready"
                };
            }

            var zone = ZoneConfigService.GetZoneById(zoneId);
            if (zone == null)
            {
                return new TemplateSpawnResult
                {
                    Success = false,
                    Error = $"Zone '{zoneId}' not found"
                };
            }

            if (!zone.Templates.TryGetValue(templateType, out var templateName) ||
                string.IsNullOrWhiteSpace(templateName))
            {
                return new TemplateSpawnResult
                {
                    Success = false,
                    Error = $"Template type '{templateType}' not configured for zone '{zoneId}'"
                };
            }

            return SpawnZoneTemplate(zoneId, templateType, templateName, em);
        }

        public static TemplateSpawnResult SpawnZoneTemplate(string zoneId, string templateType, string templateName, EntityManager em)
        {
            if (em == default || em.World == null || !em.World.IsCreated)
            {
                return new TemplateSpawnResult
                {
                    Success = false,
                    Error = "EntityManager world not ready"
                };
            }

            if (string.IsNullOrWhiteSpace(zoneId) || string.IsNullOrWhiteSpace(templateType) || string.IsNullOrWhiteSpace(templateName))
            {
                return new TemplateSpawnResult
                {
                    Success = false,
                    Error = "Zone, template type, and template name are required."
                };
            }

            var zone = ZoneConfigService.GetZoneById(zoneId);
            if (zone == null)
            {
                return new TemplateSpawnResult
                {
                    Success = false,
                    Error = $"Zone '{zoneId}' not found"
                };
            }

            if (ZoneTemplateRegistry.IsSpawned(zoneId, templateType))
            {
                return new TemplateSpawnResult
                {
                    Success = true,
                    Status = "AlreadySpawned",
                    ZoneId = zoneId,
                    TemplateType = templateType,
                    TemplateName = templateName
                };
            }

            if (TryFindSpawnLocationConflict(zoneId, zone, out var conflictingZoneId))
            {
                return new TemplateSpawnResult
                {
                    Success = false,
                    ZoneId = zoneId,
                    TemplateType = templateType,
                    TemplateName = templateName,
                    Error = $"Template spawn blocked: location matches active template zone '{conflictingZoneId}'."
                };
            }

            var origin = new float3(zone.CenterX, zone.CenterY, zone.CenterZ);
            var result = BuildingService.Instance.SpawnTemplateNeutral(templateName, em, origin);
            result.ZoneId = zoneId;
            result.TemplateType = templateType;
            result.TemplateName ??= templateName;

            if (result.Success && result.Entities.Count > 0)
            {
                var metadata = new TemplateSpawnMetadata
                {
                    SpawnedAt = DateTime.UtcNow,
                    EntityCount = result.Entities.Count,
                    TemplateName = templateName,
                    OriginPosition = origin,
                    Rotation = quaternion.identity
                };

                if (!ZoneTemplateRegistry.RegisterEntities(zoneId, templateType, result.Entities, metadata))
                {
                    // Capacity exceeded: cleanup spawned entities
                    foreach (var entity in result.Entities)
                    {
                        if (em.Exists(entity))
                        {
                            em.DestroyEntity(entity);
                        }
                    }

                    result.Success = false;
                    result.Error = $"Template '{templateType}' would exceed registry limits";
                    result.Entities.Clear();
                }
            }

            return result;
        }

        public static TemplateSpawnResult TrackExternalSpawn(
            string zoneId,
            string templateType,
            string templateName,
            IEnumerable<Entity> entities,
            float3 origin)
        {
            var entityList = entities?.Where(entity => entity != Entity.Null).ToList() ?? new List<Entity>();
            var result = new TemplateSpawnResult
            {
                ZoneId = zoneId ?? string.Empty,
                TemplateType = templateType ?? string.Empty,
                TemplateName = templateName ?? string.Empty,
                Entities = entityList
            };

            if (string.IsNullOrWhiteSpace(zoneId) || string.IsNullOrWhiteSpace(templateType) || entityList.Count == 0)
            {
                result.Success = false;
                result.Error = "No entities to track for template spawn.";
                return result;
            }

            if (ZoneTemplateRegistry.IsSpawned(zoneId, templateType))
            {
                result.Success = true;
                result.Status = "AlreadySpawned";
                result.Entities = new List<Entity>();
                return result;
            }

            var metadata = new TemplateSpawnMetadata
            {
                SpawnedAt = DateTime.UtcNow,
                EntityCount = entityList.Count,
                TemplateName = templateName,
                OriginPosition = origin,
                Rotation = quaternion.identity
            };

            if (!ZoneTemplateRegistry.RegisterEntities(zoneId, templateType, entityList, metadata))
            {
                result.Success = false;
                result.Error = $"Template '{templateType}' would exceed registry limits";
                result.Entities = new List<Entity>();
                return result;
            }

            result.Success = true;
            result.Status = "Tracked";
            return result;
        }

        public static List<TemplateSpawnResult> SpawnAllZoneTemplates(string zoneId, EntityManager em)
        {
            var results = new List<TemplateSpawnResult>();
            var zone = ZoneConfigService.GetZoneById(zoneId);
            if (zone == null)
            {
                return results;
            }

            foreach (var kvp in zone.Templates)
            {
                results.Add(SpawnZoneTemplateType(zoneId, kvp.Key, em));
            }

            return results;
        }

        public static int ClearZoneTemplateType(string zoneId, string templateType, EntityManager em)
        {
            if (em == default || em.World == null || !em.World.IsCreated)
            {
                return 0;
            }

            if (!ZoneTemplateRegistry.TryGetEntities(zoneId, templateType, out var entities))
            {
                return 0;
            }

            var toDestroy = entities.ToList();
            var destroyed = 0;
            foreach (var entity in toDestroy)
            {
                if (em.Exists(entity))
                {
                    em.DestroyEntity(entity);
                    destroyed++;
                }
            }

            ZoneTemplateRegistry.ClearZoneType(zoneId, templateType);
            return destroyed;
        }

        public static int ClearAllZoneTemplates(string zoneId, EntityManager em)
        {
            if (em == default || em.World == null || !em.World.IsCreated)
            {
                return 0;
            }

            var total = 0;
            var templateTypes = ZoneTemplateRegistry.GetSpawnedTypes(zoneId);
            foreach (var templateType in templateTypes.ToList())
            {
                total += ClearZoneTemplateType(zoneId, templateType, em);
            }

            return total;
        }

        public static List<TemplateSpawnResult> RebuildZoneTemplates(string zoneId, EntityManager em)
        {
            ClearAllZoneTemplates(zoneId, em);
            return SpawnAllZoneTemplates(zoneId, em);
        }

        public static bool IsTemplateSpawned(string zoneId, string templateType)
            => ZoneTemplateRegistry.IsSpawned(zoneId, templateType);

        public static List<string> GetSpawnedTemplateTypes(string zoneId)
            => ZoneTemplateRegistry.GetSpawnedTypes(zoneId).ToList();

        public static int GetTemplateEntityCount(string zoneId, string templateType)
            => ZoneTemplateRegistry.GetEntityCount(zoneId, templateType);

        public static Dictionary<string, int> GetZoneTemplateStats(string zoneId)
        {
            var stats = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var type in ZoneTemplateRegistry.GetSpawnedTypes(zoneId))
            {
                stats[type] = ZoneTemplateRegistry.GetEntityCount(zoneId, type);
            }

            return stats;
        }

        private static bool TryFindSpawnLocationConflict(string zoneId, ZoneDefinition targetZone, out string conflictingZoneId)
        {
            conflictingZoneId = string.Empty;
            if (targetZone == null)
            {
                return false;
            }

            foreach (var activeZoneId in ZoneTemplateRegistry.GetSpawnedZones())
            {
                if (string.IsNullOrWhiteSpace(activeZoneId) ||
                    string.Equals(activeZoneId, zoneId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var activeZone = ZoneConfigService.GetZoneById(activeZoneId);
                if (activeZone == null)
                {
                    continue;
                }

                if (!ZoneConfigService.AreZonesSameLocation(targetZone, activeZone))
                {
                    continue;
                }

                conflictingZoneId = activeZoneId;
                return true;
            }

            return false;
        }
    }
}
