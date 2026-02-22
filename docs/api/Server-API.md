# Server API

`ServerApi` provides safe server/ECS entry points.

## Typical usage

```csharp
using VAutomationCore.Core.Api;

if (ServerApi.TryGetEntityManager(out var em))
{
    // ECS work
}
```

## Methods

- `IsReady()`
- `TryGetServerWorld(out World world)`
- `TryGetEntityManager(out EntityManager entityManager)`
- `GetServerTimeUtc()`
