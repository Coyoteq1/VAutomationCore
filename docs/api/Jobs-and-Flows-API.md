# Jobs and Flows API

This area combines command-driven job flows and ECS job utilities.

## Flow runtime APIs

Primary classes:
- `FlowService`
- `FlowDefinition`
- `FlowStep`
- `FlowExecutionResult`

Core methods:
- `FlowService.RegisterFlow(...)`
- `FlowService.RemoveFlow(...)`
- `FlowService.GetFlowNames()`
- `FlowService.RegisterActionAlias(...)`
- `FlowService.Execute(...)`
- `FlowService.ExecuteJobFlow(...)`

`ExecuteJobFlow(...)` enforces developer authorization through `ConsoleRoleAuthService`.

## Job command group

Implemented in `CoreJobFlowCommands` (`.jobs ...`):
- `flow add/remove/list`
- `action add/remove/list`
- `alias self/user/clear/list`
- `component add/list/has`
- `run <flow>`

## Minimal flow example

```csharp
using VAutomationCore.Core.Api;

FlowService.RegisterActionAlias("tele", "Teleport", replace: true);
FlowService.RegisterFlow("home", new[]
{
    new FlowStep("tele", new Unity.Mathematics.float3(10f, 10f, 10f))
}, replace: true);
```
