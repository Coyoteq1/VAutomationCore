# Command Cheat Sheet

Quick command reference for operators and developers.

## Core auth commands

- `.coreauth help`
- `.coreauth login dev <password>`
- `.coreauth login admin <password>`
- `.coreauth status`
- `.coreauth logout`

Note:
- `Developer` role is required for `.jobs run`.

## Jobs and flow commands

- `.jobs help`
- `.jobs flow add <flow> <action>`
- `.jobs flow remove <flow>`
- `.jobs flow list`
- `.jobs action add <alias> <action>`
- `.jobs action remove <alias>`
- `.jobs action list`
- `.jobs alias self <alias>`
- `.jobs alias user <alias> [platformId]`
- `.jobs alias clear [alias|*]`
- `.jobs alias list`
- `.jobs component add <alias> <componentType>`
- `.jobs component list`
- `.jobs component has <entityAlias> <componentAlias>`
- `.jobs run <flow> [stopOnFailure]`

## Lifecycle commands

- `.lifecycle help` or `.lc h`
- `.lifecycle status` or `.lc s`
- `.lifecycle enter [zone]` or `.lc e [zone]`
- `.lifecycle exit` or `.lc x`
- `.lifecycle config` or `.lc c`
- `.lifecycle stages` or `.lc st`
- `.lifecycle trigger <stage>` or `.lc t <stage>`

## Typical operator sequences

1. Validate auth:
- `.coreauth status`
- `.coreauth login dev <password>`

2. Run flow:
- `.jobs flow list`
- `.jobs run <flow>`

3. Check lifecycle:
- `.lifecycle status`
- `.lifecycle stages`
