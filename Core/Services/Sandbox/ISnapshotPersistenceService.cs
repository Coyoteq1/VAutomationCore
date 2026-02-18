using System.Collections.Generic;

namespace VAuto.Core.Services
{
    internal interface ISnapshotPersistenceService
    {
        List<BaselineRow> ReadBaseline(string path);
        List<DeltaRow> ReadDelta(string path);
        void WriteBaseline(string path, IEnumerable<BaselineRow> rows);
        void WriteDelta(string path, IEnumerable<DeltaRow> rows);
    }
}
