# NuGet Release

## Package

- `PackageId`: `VAutomationCore`
- `Version`: `1.0.0`
- `Published`: `https://www.nuget.org/packages/VAutomationCore/1.0.0`
- `Project`: `VAutomationCore.csproj`
- `Output folder`: `artifacts/nuget`

## Files To Upload

- `artifacts/nuget/VAutomationCore.1.0.0.nupkg`
- `artifacts/nuget/VAutomationCore.1.0.0.snupkg` (optional but recommended for symbols)

If your feed does not support symbols, upload only the `.nupkg`.

## Build And Pack

```powershell
dotnet build VAutomationCore.csproj -c Release --nologo
```

Build generates:

- `artifacts/nuget/VAutomationCore.1.0.0.nupkg`
- `artifacts/nuget/VAutomationCore.1.0.0.snupkg`

## Push Commands

### NuGet.org

```powershell
dotnet nuget push artifacts/nuget/VAutomationCore.1.0.0.nupkg --source https://api.nuget.org/v3/index.json --api-key "YOUR_REAL_NUGET_API_KEY" --skip-duplicate
dotnet nuget push artifacts/nuget/VAutomationCore.1.0.0.snupkg --source https://api.nuget.org/v3/index.json --api-key "YOUR_REAL_NUGET_API_KEY" --skip-duplicate
```

Important:
- Do not use `<...>` placeholders in PowerShell commands.
- Use the real token value from `https://www.nuget.org/account/apikeys`.

### Local Feed

```powershell
dotnet nuget push artifacts/nuget/VAutomationCore.1.0.0.nupkg --source LocalVAutomationCoreLibs
```

## Notes

- Package metadata is in `VAutomationCore.csproj`.
- Readme and icon are included in package output from:
  - `README.md`
  - `packaging/VAutomationCore/icon.png`
- Current project mapping keeps physical files under `Core/` while exposing linked paths as `VAuto/` in project metadata.
- If package is already signed, `NU3001` can appear during re-sign attempts; verify step should still pass.
