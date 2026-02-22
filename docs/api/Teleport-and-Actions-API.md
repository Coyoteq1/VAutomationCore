# Teleport and Actions API

`GameActionService` is the action runtime used by flow steps and event hooks.

## Built-in actions

- `ApplyBuff`
- `CleanBuff`
- `RemoveBuff`
- `Teleport`
- `SetPosition`
- `SendMessageToAll`
- `SendMessageToPlatform`
- `SendMessageToUser`

## Important methods

- `InvokeAction(...)`
- `GetRegisteredActionNames()`
- `RegisterEventAction(...)`
- `TriggerEvent(...)`
- `TryTeleport(...)`
- `TrySetEntityPosition(...)`

## Teleport behavior

`TryTeleport(...)` attempts buff-based teleport first (`TeleportBuff`), then falls back to position component writes (`SpawnTransform`, `LocalTransform`, `Translation`, etc.) if needed.
