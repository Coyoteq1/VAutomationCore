using System.Collections.Generic;

namespace VAuto.Core.Services
{
    internal sealed class SnapshotPersistenceService : ISnapshotPersistenceService
    {
        public List<BaselineRow> ReadBaseline(string path)
            => SandboxCsvWriter.ReadBaseline(path);

        public List<DeltaRow> ReadDelta(string path)
            => SandboxCsvWriter.ReadDelta(path);

        public void WriteBaseline(string path, IEnumerable<BaselineRow> rows)
            => SandboxCsvWriter.WriteBaseline(path, rows);

        public void WriteDelta(string path, IEnumerable<DeltaRow> rows)
            => SandboxCsvWriter.WriteDelta(path, rows);
    }
}
