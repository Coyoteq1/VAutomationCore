# Blueluck

V Rising mod for enhanced gameplay with zones, kits, and flow system.

## Features

- **Zone Management** - Custom zone detection with PvP, PvE, sanctuary, raid, and more
- **Kit Commands** - Apply custom loadouts to players
- **Flow Registry** - Game state management with triggers and actions
- **Boss System** - Dynamic boss spawning and encounters
- **30+ Flow Types** - Coming soon!

## Supported Bosses

| Boss | Prefab |
|------|--------|
| Tourok | `CHAR_Bandit_Tourok_VBlood` |
| Gloomrot | `CHAR_Gloomrot_Purifier_VBlood` |
| StoneBreaker | `CHAR_Bandit_StoneBreaker_VBlood` |
| Stalker | `CHAR_Bandit_Stalker_VBlood` |
| Foreman | `CHAR_Bandit_Foreman_VBlood` |
| ArchMage | `CHAR_ArchMage_VBlood` |
| Dracula | `CHAR_Vampire_Dracula_VBlood` |
| Spider Queen | `CHAR_Spider_Queen_VBlood` |
| Wendigo | `CHAR_Wendigo_VBlood` |
| Winter Yeti | `CHAR_Winter_Yeti_VBlood` |
| Manticore | `CHAR_Manticore_VBlood` |
| Cursed Witch | `CHAR_Cursed_Witch_VBlood` |
| Poloma | `CHAR_Poloma_VBlood` |
| Blood Knight | `CHAR_Vampire_BloodKnight_VBlood` |
| Geomancer | `CHAR_Geomancer_Human_VBlood` |
| Harpy Matriarch | `CHAR_Harpy_Matriarch_VBlood` |
| Jade | `CHAR_VHunter_Jade_VBlood` |
| Bishop of Death | `CHAR_Undead_BishopOfDeath_VBlood` |
| Bishop of Shadows | `CHAR_Undead_BishopOfShadows_VBlood` |

## Current Flows (25 Active)

### Arena Flows
- `arena_enter` - Enable PvP, send arena message
- `arena_exit` - Disable PvP, safe zone message

### Boss Flows
- `boss_enter` - Spawn single boss
- `boss_exit` - Remove boss on defeat
- `boss_enter_multi` - Spawn 3 bosses
- `boss_enter_tourok` - Spawn Tourok
- `boss_enter_gloomrot` - Spawn Gloomrot
- `boss_enter_stonebreaker` - Spawn StoneBreaker
- `boss_enter_stalker` - Spawn Stalker
- `boss_enter_foreman` - Spawn Foreman
- `boss_enter_archmage` - Spawn ArchMage
- `boss_enter_dracula` - Spawn Dracula
- `boss_enter_spider_queen` - Spawn Spider Queen
- `boss_enter_wendigo` - Spawn Wendigo
- `boss_enter_yeti` - Spawn Yeti
- `boss_enter_manticore` - Spawn Manticore
- `boss_enter_cursed_witch` - Spawn Cursed Witch
- `boss_enter_poloma` - Spawn Poloma
- `boss_enter_bloodknight` - Spawn Blood Knight
- `boss_enter_geomancer` - Spawn Geomancer
- `boss_enter_harpy` - Spawn Harpy Matriarch
- `boss_enter_jade` - Spawn Jade
- `boss_enter_bishop_death` - Spawn Bishop of Death
- `boss_enter_bishop_shadows` - Spawn Bishop of Shadows

### Game Mode Flows
- `dungeon_enter` / `dungeon_exit` - Dungeon zones
- `pvp_ffa_enter` / `pvp_ffa_exit` - Free for all PvP
- `sanctuary_enter` / `sanctuary_exit` - Safe trading zones
- `raid_enter` / `raid_exit` - Territory battles
- `pve_arena_enter` / `pve_arena_exit` - PvE wave combat
- `duel_enter` / `duel_exit` - 1v1 duels
- `ctf_enter` / `ctf_exit` - Capture the flag

## Coming Soon: 30+ Additional Flows

New flow types in development for enhanced gameplay:
- Blood hunt events
- Territory control
- Tournament modes
- Quest triggers
- Custom win conditions
- And more!

## Installation

1. Build the solution
2. Place the DLL in your V Rising server's BepInEx/plugins folder

## Commands

- `!kit` - Apply kit loadouts
- `!zone` - Zone management
- `!snapshot` - Snapshot commands

## Configuration

Edit the JSON files in the `config` folder:
- `kits.json` - Kit definitions
- `zones.json` - Zone configurations
- `flows.json` - Flow settings

## License

Coyoteq1 2026
