# Castle API

`CastleApi` is an in-memory registry for castle ownership and feature flags.

## Typical usage

```csharp
using VAutomationCore.Core.Api;

CastleApi.SetOwner("castle_alpha", 987654321);
CastleApi.SetPvpEnabled("castle_alpha", true);
```

## Methods

- `SetOwner(...)`
- `SetPvpEnabled(...)`
- `TryGetState(...)`
- `Remove(...)`
- `Snapshot()`
