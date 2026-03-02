# VAuto.Extensions

VAuto.Extensions contains shared extension helpers used across VAutomation modules.

## Description
- Shared extension/helper project for reusable utility methods and cross-module helpers.
- Provides common utilities for string, datetime, dictionary, collection, and exception handling.

## Quick Commands
```powershell
# Build
dotnet build VAuto.Extensions/VAuto.Extensions.csproj -c Release --nologo
```

## Extension Methods

### StringExtensions
- `TruncateWithEllipsis(maxLength)` - Truncate string with ellipsis
- `ToTitleCase()` - Convert to title case
- `ToSnakeCase()` - Convert to snake_case
- `IsNullOrWhiteSpace()` - Null/whitespace check

### DateTimeExtensions
- `ToUnixTimestamp()` - Convert to Unix timestamp
- `FromUnixTimestamp(timestamp)` - Convert from Unix timestamp
- `ToRelativeString()` - Human-readable relative time
- `StartOfDay()` / `EndOfDay()` - Day boundaries

### DictionaryExtensions
- `GetOrAdd(key, factory)` - Get or create value
- `TryGetValue(key, out value)` - Safe retrieval
- `Merge(other)` - Merge dictionaries

### CollectionExtensions
- `ForEach(action)` - Iterate with action
- `WhereNotNull()` - Filter nulls
- `ToHashSet()` - Distinct collection

### ExceptionExtensions
- `GetRootCause()` - Get innermost exception
- `ToDetailedString()` - Full exception details
- `IsTransient()` - Check if retry-able

## Services
- No standalone runtime service registration; this project provides extension/helper primitives.

## User GUIDs
- GUID/platform ID handling is delegated to consuming modules and core commands.
- Use `.jobs alias user <alias> [platformId]` from `VAutomationCore` when user mapping is needed.

## Community
- Join the V Rising Mods Community on Discord: [V Rising Mods Discord](https://discord.gg/68JZU5zaq7)
- Need ownership support? Visit: [Ownership Support Discord](https://discord.gg/Se4wU3s6md)

## Contributors
Special thanks to our contributors:
1. coyoteq1
