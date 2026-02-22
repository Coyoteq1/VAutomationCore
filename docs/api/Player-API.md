# Player API

`PlayerApi` manages subject-scoped `EntityMap` aliases.

## Typical usage

```csharp
using Unity.Entities;
using VAutomationCore.Core.Api;

ulong subjectId = 1234567890;
var map = PlayerApi.GetOrCreateEntityMap(subjectId);
Entity someEntity = default;
PlayerApi.TryMapEntity(subjectId, "self", someEntity, replace: true);
PlayerApi.TryResolveEntity(subjectId, "self", out var resolved);
```

## Methods

- `GetOrCreateEntityMap(...)`
- `TryGetEntityMap(...)`
- `RemoveEntityMap(...)`
- `GetActiveSubjects()`
- `TryMapEntity(...)`
- `TryResolveEntity(...)`
