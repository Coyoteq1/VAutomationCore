namespace VAutomationCore.Core.Config
{
    public interface IConfigMigration<T>
    {
        string Name { get; }
        string SourceVersion { get; }
        string TargetVersion { get; }
        MigrationReport DryRun(T config);
        T Apply(T config);
    }
}
