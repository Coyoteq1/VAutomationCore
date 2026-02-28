using System.Collections.Generic;

namespace VAutomationCore.Core.Config
{
    public sealed class MigrationReport
    {
        public bool HasChanges => Changes.Count > 0;
        public string MigrationName { get; set; } = string.Empty;
        public List<string> Changes { get; set; } = new();
    }
}
