using Unity.Entities;

namespace VAutomationCore.Core.Contracts
{
    public interface IZoneTemplateAdapter
    {
        void ApplyEnterTemplate(string zoneId, Entity player, EntityManager entityManager);
        void ApplyExitTemplate(string zoneId, Entity player, EntityManager entityManager);
    }
}
