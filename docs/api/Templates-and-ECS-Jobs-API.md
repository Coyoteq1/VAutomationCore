# Templates and ECS Jobs API

ECS job helpers are provided under `Core/ECS/JobSystemExtensions.cs`.

## Main types

- `IVAutoJob`
- `IVAutoJobWithEM`
- `IVAutoJobWithData<T>`
- `VAutoJobs`
- `JobBuilder`
- `JobFlow`
- `JobTemplates`
- `JobTimeRule`
- `JobTimeScheduler`

## Template helpers

- `JobTemplates.DestroyAll<T>()`
- `JobTemplates.LogAll<T>()`
- `JobTemplates.SetEnabled<T>(bool)`
- `JobTemplates.Flow(params Action<Entity>[])`

## EntityManager extensions

- `RunJob(...)`
- `RunJobs(...)`
- `RunBundle(...)`
- `RunScheduled(...)`
- `Jobs()` (fluent builder)
