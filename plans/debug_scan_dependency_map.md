# Cross-Project Dependency Map

## Project Reference Table
| Project | Path | ProjectReferences | PackageReferences |
|---|---|---|---|
| Bluelock.Tests.csproj | tests\Bluelock.Tests\Bluelock.Tests.csproj | ..\\..\\VAutomationCore.csproj | Microsoft.NET.Test.Sdk; xunit; xunit.runner.visualstudio; Microsoft.Extensions.Logging.Abstractions |
| Swapkits.csproj | Swapkits\Swapkits.csproj | ../VAutomationCore.csproj | BepInEx.Unity.IL2CPP; VRising.VampireCommandFramework; Lib.Harmony |
| VAuto.Extensions.csproj | VAuto.Extensions\VAuto.Extensions.csproj |  | Newtonsoft.Json |
| VAutoannounce.csproj | VAutoannounce\VAutoannounce.csproj | ..\\VAutomationCore.csproj | BepInEx.Unity.IL2CPP; VRising.VampireCommandFramework; Lib.Harmony |
| VAutomationCore.csproj | VAutomationCore.csproj |  | Microsoft.SourceLink.GitHub; BepInEx.Unity.IL2CPP; RisingV.Shared; VRising.VampireCommandFramework; VRising.VAMP; Lib.Harmony; Il2CppInterop.Runtime |
| VAutoTraps.csproj | VAutoTraps\VAutoTraps.csproj | ../VAutomationCore.csproj | BepInEx.Unity.IL2CPP; VRising.VampireCommandFramework; Lib.Harmony |
| VAutoZone.csproj | Bluelock\VAutoZone.csproj | ../VAutomationCore.csproj | VRising.VampireCommandFramework; Lib.Harmony |
| Vlifecycle.csproj | CycleBorn\Vlifecycle.csproj | ../VAutomationCore.csproj | BepInEx.Unity.IL2CPP; RisingV.Scripting; RisingV.Shared; VRising.VampireCommandFramework; Lib.Harmony |

