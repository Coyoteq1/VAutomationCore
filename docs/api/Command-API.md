# Command API

`CommandApi` is for command registration and flow/action alias integration.

## Typical usage

```csharp
using System.Reflection;
using VAutomationCore.Core.Api;

CommandApi.RegisterAllFromAssembly(Assembly.GetExecutingAssembly());
CommandApi.RegisterActionAlias("teleport_home", "Teleport", replace: true);
```

## Methods

- `RegisterAllFromAssembly(...)`
- `RegisterActionAlias(...)`
- `RemoveActionAlias(...)`
- `GetActionAliases()`
- `GetRegisteredFlowNames()`
- `CanRunJobs(subjectId)`
