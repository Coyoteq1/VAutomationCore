using System.Collections.Generic;

namespace VAuto.Core.Services
{
    internal sealed class SnapshotDiffService : ISnapshotDiffService
    {
        public IEnumerable<DeltaRow> ComputeComponentDelta(IEnumerable<BaselineRow> pre, IEnumerable<BaselineRow> post)
            => SandboxDeltaComputer.ComputeComponentDelta(pre, post);

        public IEnumerable<DeltaRow> ExtractOpenedTech(IEnumerable<BaselineRow> preComponents, IEnumerable<BaselineRow> postComponents)
            => SandboxDeltaComputer.ExtractOpenedTech(preComponents, postComponents);

        public IEnumerable<DeltaRow> ComputeEntityDelta(IEnumerable<ZoneEntityEntry> preZoneEntities, IEnumerable<ZoneEntityEntry> postZoneEntities)
            => SandboxDeltaComputer.ComputeEntityDelta(preZoneEntities, postZoneEntities);
    }
}
