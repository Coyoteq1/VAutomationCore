using VAutomationCore.Core.Config;

namespace VAuto.Zone.Services
{
    internal sealed class ZoneLifecycleConfigVersionMigration : IConfigMigration<ZoneJsonConfig>
    {
        public string Name => nameof(ZoneLifecycleConfigVersionMigration);
        public string SourceVersion => "1.0.0";
        public string TargetVersion => ZoneJsonConfig.CurrentConfigVersion;

        public MigrationReport DryRun(ZoneJsonConfig config)
        {
            var report = new MigrationReport { MigrationName = Name };
            if (config != null && string.IsNullOrWhiteSpace(config.ConfigVersion))
            {
                report.Changes.Add("Set ConfigVersion to current runtime version.");
            }
            if (config != null && string.IsNullOrWhiteSpace(config.SchemaVersion))
            {
                report.Changes.Add("Set SchemaVersion to current runtime version.");
            }

            return report;
        }

        public ZoneJsonConfig Apply(ZoneJsonConfig config)
        {
            config ??= new ZoneJsonConfig();
            if (string.IsNullOrWhiteSpace(config.ConfigVersion))
            {
                config.ConfigVersion = TargetVersion;
            }
            if (string.IsNullOrWhiteSpace(config.SchemaVersion))
            {
                config.SchemaVersion = TargetVersion;
            }

            return config;
        }
    }
}
