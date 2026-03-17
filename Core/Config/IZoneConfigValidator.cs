using System.Collections.Generic;

namespace VAutomationCore.Core.Config
{
    public interface IZoneConfigValidator
    {
        string Name { get; }
        bool Validate(string baseConfigPath, IList<string> errors, IList<string> warnings);
    }
}
