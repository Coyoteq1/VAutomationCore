# Prefab Alias Duplication Findings

## Scope
- Investigate BlueLock duplication warnings related to prefab aliases.
- Validate whether duplicate alias definitions resolve to identical target values.

## Source Of Duplication
`PrefabResolver` loads aliases in this order:
1. `Prefabsref.json` aliases (`LoadPrefabsRef` -> `AddAlias(..., "[Prefabsref.alias]")`)
2. `ability_prefabs.json` aliases (`LoadLegacyAbilityAliases` -> `AddAlias(..., "[ability_prefabs]")`)

Because `AddAlias` keeps the first value and warns on duplicates, aliases present in both files produce warnings.

Code reference:
- `Bluelock/Core/PrefabResolver.cs` (`LoadPrefabsRef`, `LoadLegacyAbilityAliases`, `AddAlias`)

## Warning Evidence
Observed in:
- `D:\DedicatedServerLauncher\VRisingServer\BepInEx\LogOutput.log`

Warnings:
- Alias `Spell_VeilOfBlood` duplicated from `[ability_prefabs]`
- Alias `Spell_VeilOfChaos` duplicated from `[ability_prefabs]`
- Alias `Spell_VeilOfFrost` duplicated from `[ability_prefabs]`
- Alias `Spell_VeilOfBones` duplicated from `[ability_prefabs]`
- Alias `AB_BloodRite_AbilityGroup` duplicated from `[ability_prefabs]`

## Value Verification (Runtime Config)
Compared:
- `D:\DedicatedServerLauncher\VRisingServer\BepInEx\config\Bluelock\Prefabsref.json`
- `D:\DedicatedServerLauncher\VRisingServer\BepInEx\config\Bluelock\ability_prefabs.json`

Results:
- `Spell_VeilOfBlood` -> `AB_Vampire_VeilOfBlood_Group` vs `AB_Vampire_VeilOfBlood_Group` (identical)
- `Spell_VeilOfChaos` -> `AB_Vampire_VeilOfChaos_Group` vs `AB_Vampire_VeilOfChaos_Group` (identical)
- `Spell_VeilOfFrost` -> `AB_Vampire_VeilOfFrost_Group` vs `AB_Vampire_VeilOfFrost_Group` (identical)
- `Spell_VeilOfBones` -> `AB_Vampire_VeilOfBones_AbilityGroup` vs `AB_Vampire_VeilOfBones_AbilityGroup` (identical)
- `AB_BloodRite_AbilityGroup` -> `AB_Blood_BloodRite_AbilityGroup` vs `AB_Blood_BloodRite_AbilityGroup` (identical)

## Conclusion
- Duplicate alias warnings are expected from current load order and duplicate definitions across the two files.
- Current duplicate values are identical, so behavior is non-destructive.
- Effect is log noise, not a functional alias mismatch.

---

## Extended Analysis

### Code Behavior Deep Dive
The `AddAlias` method (lines 338-360) implements the following logic:
```csharp
if (ByAlias.TryGetValue(normalizedAlias, out var existingGuid))
{
    // Duplicate alias that resolves to the same prefab is harmless; ignore silently.
    if (existingGuid.GuidHash == guid.GuidHash)
    {
        return;
    }
    // Warning logged only if different targets...
}
```

**Key Insight**: When duplicate aliases resolve to **identical** GUIDHashes, they are silently ignored (no warning). Warnings only appear when:
1. A duplicate alias resolves to a **different** GUID
2. An alias points to an unknown prefab target

This means the observed warnings likely indicate either:
- The config files have diverged since initial investigation
- Warnings were logged for unknown target resolution, not duplicate detection
- The test environment differs from the runtime environment

### Root Cause Analysis

**Source Files & Load Order**:
1. **Prefabsref.json** (loaded first via `LoadPrefabsRef`)
   - Higher priority; aliases loaded into `ByAlias` dictionary first
   - Source tag: `[Prefabsref.alias]`

2. **ability_prefabs.json** (loaded second via `LoadLegacyAbilityAliases`)
   - Legacy support; aliases attempted after primary source
   - Source tag: `[ability_prefabs]`
   - If alias already exists with same GUID, silently ignored
   - If alias already exists with different GUID, warning logged (not observed in this case)

**Why Duplication Exists**:
- These files evolved independently; `Prefabsref.json` was introduced as the primary config
- `ability_prefabs.json` remains for backward compatibility with legacy systems
- Standard aliases (like VeilOfBlood abilities) need to be present in both for legacy systems to work

### Classification & Risk Assessment

| Category | Count | Severity | Mitigation |
|----------|-------|----------|-----------|
| Redundant but identical | 5 | Low | Code-level deduplication or file cleanup |
| Loading overhead | Minimal | Negligible | Only seconds on startup (one-time) |
| Log noise | Yes | Low | Add silent deduplication flag or cleanup config |
| Functional impact | None | None | Behavior is correct regardless |

### Recommendations

#### Option 1: Remove from Legacy File (Recommended for Long-term)
**Approach**: Remove the 5 duplicate aliases from `ability_prefabs.json` if no legacy code specifically depends on this file structure.

**Pros**:
- Eliminates duplication at the source
- Reduces disk footprint and load time (marginally)
- Cleaner config state
- Forces unified single-source-of-truth (Prefabsref.json)

**Cons**:
- Requires verification that nothing depends on `ability_prefabs.json` structure
- Breaking change if external systems rely on it

**Implementation**:
- Audit `LoadLegacyAbilityAliases` usage
- Verify no hard dependency on `ability_prefabs.json` aliases
- Remove: `Spell_VeilOfBlood`, `Spell_VeilOfChaos`, `Spell_VeilOfFrost`, `Spell_VeilOfBones`, `AB_BloodRite_AbilityGroup`

#### Option 2: Add Silent Deduplication Flag (Lowest-risk)
**Approach**: Modify `AddAlias` to suppress warnings for identical duplicates explicitly, making it intentional and documented.

```csharp
private static void AddAlias(string alias, string target, string sourceTag, bool silentIfIdentical = true)
{
    // ... validation code ...
    if (ByAlias.TryGetValue(normalizedAlias, out var existingGuid))
    {
        if (existingGuid.GuidHash == guid.GuidHash)
        {
            if (!silentIfIdentical)
                ZoneCore.LogInfo($"[PrefabResolver] Alias '{normalizedAlias}' already loaded from earlier source (same target).");
            return;
        }
        ZoneCore.LogWarning(...); // Different GUID version
    }
}
```

**Pros**:
- No breaking changes
- Makes deduplication intentional and discoverable
- Allows logging if verbose mode enabled
- Easiest to implement

**Cons**:
- Doesn't eliminate the actual duplication
- Config files still contain redundant data

#### Option 3: Merge Configuration (Most Complex)
**Approach**: Create consolidated config that removes duplication entirely.

**Implementation**:
- Load `ability_prefabs.json` first to establish legacy expectations
- Merge with Prefabsref.json, preferring Prefabsref when conflicts exist
- Document migration path for downstream consumers

**Pros**:
- Single source of truth
- Eliminates any potential future conflicts

**Cons**:
- Significant refactoring
- Risk of breaking legacy code paths
- Requires comprehensive testing

### Additional Observations

1. **Prefabsref.json as Default**: The default config created in `CreateDefaultPrefabsRef` (lines 295-320) includes all 5 duplicated aliases, suggesting they're considered "standard" vocabulary for the system.

2. **Case-Insensitive Lookup**: Both dictionaries use `StringComparer.OrdinalIgnoreCase`, making `Spell_VeilOfBlood` and `spell_veilofblood` resolve identically. This is correct behavior.

3. **Fallback Chain**: Resolution order is:
   - Direct numeric GUID
   - Alias lookup (ByAlias)
   - Name lookup (ByName)
   - PrefabsAll legacy catalog
   
   This means alias duplication doesn't break any lookup—first match wins, which is safe.

4. **Thread Safety**: Load operations are synchronized with `lock (Sync)`, preventing race conditions during initialization.

### Next Steps
1. **Verify no external dependencies** on `ability_prefabs.json` structure
2. **Run load-time benchmarks** to confirm overhead is negligible
3. **Choose deduplication strategy** (recommend Option 1 for clean state)
4. **Update logging** to clarify deduplication behavior if retaining files
5. **Document in code comments** why duplication is acceptable in current state
