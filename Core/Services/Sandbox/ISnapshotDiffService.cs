using System.Collections.Generic;

namespace VAuto.Core.Services
{
    internal interface ISnapshotDiffService
    {
        IEnumerable<DeltaRow> ComputeComponentDelta(IEnumerable<BaselineRow> pre, IEnumerable<BaselineRow> post);
        IEnumerable<DeltaRow> ExtractOpenedTech(IEnumerable<BaselineRow> preComponents, IEnumerable<BaselineRow> postComponents);
        IEnumerable<DeltaRow> ComputeEntityDelta(IEnumerable<ZoneEntityEntry> preZoneEntities, IEnumerable<ZoneEntityEntry> postZoneEntities);
    }
}
