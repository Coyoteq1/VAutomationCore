using Unity.Entities;
using Unity.Mathematics;
using VAuto.Zone.Core;
using VAuto.Zone.Services;
using VAutomationCore.Core.ECS;
using VAutomationCore.Core.ECS.Components;

namespace VAuto.Zone.Systems
{
    public class ZoneBootstrapSystem : SystemBase
    {
        private bool _initialized;

        public override void OnCreate()
        {
        }

        public override void OnUpdate()
        {
            if (_initialized) return;
            _initialized = true;

            ZoneHashUtility.Initialize();
            var em = EntityManager;

            var zones = ZoneConfigService.GetAllZones();
            foreach (var zone in zones)
            {
                var zoneHash = ZoneHashUtility.GetZoneHash(zone.Id);

                ZoneHashUtility.CacheZoneCenter(zoneHash, new float3(zone.CenterX, zone.CenterY, zone.CenterZ));

                var entryRadius = zone.EntryRadius > 0 ? zone.EntryRadius : zone.Radius;
                var exitRadius = zone.ExitRadius > 0 ? zone.ExitRadius : entryRadius;

                var entity = em.CreateEntity();
                em.AddComponentData(entity, new ZoneComponent
                {
                    ZoneHash = zoneHash,
                    Priority = zone.Priority,
                    Center = new float3(zone.CenterX, zone.CenterY, zone.CenterZ),
                    EntryRadius = entryRadius,
                    ExitRadius = exitRadius,
                    EntryRadiusSq = entryRadius * entryRadius,
                    ExitRadiusSq = exitRadius * exitRadius
                });
            }

            ZoneCore.LogInfo($"[ZoneBootstrapSystem] Bootstrapped {zones.Count} zones to ECS");
        }
    }
}
