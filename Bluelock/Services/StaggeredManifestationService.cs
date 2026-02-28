using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using VAuto.Zone.Core;
using VAuto.Zone.Models;

namespace VAuto.Zone.Services
{
    public static class StaggeredManifestationService
    {
        private sealed class PendingManifestation
        {
            public string ZoneId { get; init; } = string.Empty;
            public string TemplateType { get; init; } = string.Empty;
            public string TemplateName { get; init; } = string.Empty;
            public float3 Origin { get; init; }
            public List<TemplateEntityEntry> Entries { get; init; } = new();
            public List<Entity> SpawnedEntities { get; } = new();
            public int NextIndex { get; set; }
            public DateTime NextSpawnUtc { get; set; }
            public float DelaySeconds { get; init; } = 0.1f;
            public int LevelOverride { get; init; } = 99;
        }

        private static readonly Dictionary<string, PendingManifestation> _pendingByZoneType =
            new(StringComparer.OrdinalIgnoreCase);

        public static void Initialize()
        {
            _pendingByZoneType.Clear();
        }

        public static TemplateSpawnResult QueueTemplate(
            string zoneId,
            string templateType,
            string templateName,
            EntityManager em,
            float delaySeconds = 0.1f,
            int levelOverride = 99)
        {
            var result = new TemplateSpawnResult
            {
                ZoneId = zoneId ?? string.Empty,
                TemplateType = templateType ?? string.Empty,
                TemplateName = templateName ?? string.Empty
            };

            if (em == default || em.World == null || !em.World.IsCreated)
            {
                result.Error = "EntityManager world not ready.";
                return result;
            }

            if (string.IsNullOrWhiteSpace(zoneId) || string.IsNullOrWhiteSpace(templateType) || string.IsNullOrWhiteSpace(templateName))
            {
                result.Error = "Zone, template type, and template name are required.";
                return result;
            }

            if (ZoneTemplateRegistry.IsSpawned(zoneId, templateType))
            {
                result.Success = true;
                result.Status = "AlreadySpawned";
                return result;
            }

            var pendingKey = BuildKey(zoneId, templateType);
            if (_pendingByZoneType.ContainsKey(pendingKey))
            {
                result.Success = true;
                result.Status = "AlreadyQueued";
                return result;
            }

            if (!TemplateRepository.TryLoadTemplate(templateName, out var template) || template == null || template.Entities.Count == 0)
            {
                result.Error = $"Template '{templateName}' not found or empty.";
                return result;
            }

            var zone = ZoneConfigService.GetZoneById(zoneId);
            if (zone == null)
            {
                result.Error = $"Zone '{zoneId}' not found.";
                return result;
            }

            _pendingByZoneType[pendingKey] = new PendingManifestation
            {
                ZoneId = zoneId,
                TemplateType = templateType,
                TemplateName = templateName,
                Origin = new float3(zone.CenterX, zone.CenterY, zone.CenterZ),
                Entries = template.Entities.ToList(),
                NextSpawnUtc = DateTime.UtcNow,
                DelaySeconds = Math.Max(0.02f, delaySeconds),
                LevelOverride = levelOverride
            };

            result.Success = true;
            result.Status = "Queued";
            return result;
        }

        public static int Cancel(string zoneId, string templateType, EntityManager em)
        {
            if (string.IsNullOrWhiteSpace(zoneId) || string.IsNullOrWhiteSpace(templateType))
            {
                return 0;
            }

            var key = BuildKey(zoneId, templateType);
            if (!_pendingByZoneType.TryGetValue(key, out var pending))
            {
                return 0;
            }

            var destroyed = 0;
            foreach (var entity in pending.SpawnedEntities)
            {
                if (em.Exists(entity))
                {
                    em.DestroyEntity(entity);
                    destroyed++;
                }
            }

            _pendingByZoneType.Remove(key);
            return destroyed;
        }

        public static void ProcessPending(EntityManager em)
        {
            if (_pendingByZoneType.Count == 0 || em == default || em.World == null || !em.World.IsCreated)
            {
                return;
            }

            var now = DateTime.UtcNow;
            var completed = new List<string>();

            foreach (var kvp in _pendingByZoneType)
            {
                var key = kvp.Key;
                var pending = kvp.Value;

                if (pending.NextIndex >= pending.Entries.Count)
                {
                    FinalizePending(pending, em);
                    completed.Add(key);
                    continue;
                }

                if (now < pending.NextSpawnUtc)
                {
                    continue;
                }

                var entry = pending.Entries[pending.NextIndex];
                pending.NextIndex++;
                pending.NextSpawnUtc = now.AddSeconds(pending.DelaySeconds);

                if (!BuildingService.Instance.TrySpawnTemplateEntryNeutral(
                        entry,
                        em,
                        pending.Origin,
                        0f,
                        pending.LevelOverride,
                        out var spawned,
                        out var spawnError))
                {
                    ZoneCore.LogWarning($"[BlueLock] Staggered manifestation skipped one entity in zone '{pending.ZoneId}' template '{pending.TemplateName}': {spawnError}");
                    continue;
                }

                pending.SpawnedEntities.Add(spawned);
            }

            foreach (var key in completed)
            {
                _pendingByZoneType.Remove(key);
            }
        }

        private static void FinalizePending(PendingManifestation pending, EntityManager em)
        {
            if (pending.SpawnedEntities.Count == 0)
            {
                return;
            }

            var tracked = ZoneTemplateService.TrackExternalSpawn(
                pending.ZoneId,
                pending.TemplateType,
                pending.TemplateName,
                pending.SpawnedEntities,
                pending.Origin);

            if (!tracked.Success)
            {
                foreach (var entity in pending.SpawnedEntities)
                {
                    if (em.Exists(entity))
                    {
                        em.DestroyEntity(entity);
                    }
                }

                ZoneCore.LogWarning($"[BlueLock] Staggered manifestation rollback for zone '{pending.ZoneId}' template '{pending.TemplateName}': {tracked.Error}");
                return;
            }

            ZoneCore.LogInfo($"[BlueLock] Staggered manifestation complete: zone='{pending.ZoneId}' type='{pending.TemplateType}' template='{pending.TemplateName}' entities={pending.SpawnedEntities.Count}.");
        }

        private static string BuildKey(string zoneId, string templateType)
        {
            return $"{zoneId}::{templateType}";
        }
    }
}
