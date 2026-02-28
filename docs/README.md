# Docs Index

Start here for framework and lifecycle documentation.

## Description

- Central docs index for `VAutomationCore`, `Bluelock`, and `CycleBorn`.
- Use this page as the primary entrypoint for architecture, API, and operations references.

## Services

- Core services and APIs: `Core-System-Usage.md` + `docs/api/*`
- Bluelock runtime/services: lifecycle, zone, template, ECS routing docs
- CycleBorn lifecycle/operator services: lifecycle config and snapshot references

## User GUIDs

- User GUIDs are handled as player `platformId` values in command/API flows.
- Reference command usage in root README: `.jobs alias user <alias> [platformId]`.

## NuGet

- Package page: `https://www.nuget.org/packages/VAutomationCore/1.0.0`
- Install:
  - `<PackageReference Include="VAutomationCore" Version="1.0.0" />`

## Install Channels

- Thunderstore (V Rising): `https://thunderstore.io/c/v-rising/`
- NuGet: `https://www.nuget.org/packages/VAutomationCore`

## Community and Support

- Join the V Rising Mods Community on Discord: [V Rising Mods Discord](https://discord.gg/68JZU5zaq7)
- Need ownership support? Visit: [Ownership Support Discord](https://discord.gg/Se4wU3s6md)
- Auth/Maintainer: `coyoteq1`

## Core usage

- `docs/Core-System-Usage.md`

## Command Surface (Authoritative)

- Bluelock: `.zone .match .spawn .template .tag .enter .exit .unlockprefab`
- VAutomationCore: `.coreauth .jobs`
- CycleBorn: `.lifecycle`
- `.arena` is excluded by build and not part of active runtime commands.

## Lifecycle

- `docs/Lifecycle-Snapshot-Usage.md`
- `docs/Lifecycle-Config-Reference.md`

## API references

- `docs/api/Server-API.md`
- `docs/api/Player-API.md`
- `docs/api/Command-API.md`
- `docs/api/Castle-API.md`
- `docs/api/Jobs-and-Flows-API.md`
- `docs/api/Teleport-and-Actions-API.md`
- `docs/api/Templates-and-ECS-Jobs-API.md`
- `docs/api/Mount-Chunk-Template-Notes.md`

## Project map

- `docs/File-Folder-Map.md`

## Wiki

- `docs/wiki/framework-wiki.html`
- Hosted URL: `https://coyoteq1.github.io/D-VAutomationCore-VAutomationCore/wiki/framework-wiki.html`
- Docs root URL: `https://coyoteq1.github.io/D-VAutomationCore-VAutomationCore/`

## Operations

- `docs/Architecture-Ownership-Map.md`
- `docs/Incident-Rollback-Playbook.md`
- `docs/ECS-Authoritative-Implementation-Plan.md`
- `docs/CONTRIBUTING.md`
