# VAutomationCore

This repository follows the V Rising modding documentation published at:

- https://wiki.vrisingmods.com/

## Guide Index (from the wiki)

### For Users

- Manual mod installation: https://wiki.vrisingmods.com/user/Mod_Install.html
- Using server mods in-game: https://wiki.vrisingmods.com/user/Using_Server_Mods.html
- Server configuration overview: https://wiki.vrisingmods.com/user/server-configuration.html
- User docs landing page: https://wiki.vrisingmods.com/user/

### For Developers

- Developer docs landing page: https://wiki.vrisingmods.com/dev/
- Open source reference mods: https://wiki.vrisingmods.com/dev/open%20source.html
- Developer resources: https://wiki.vrisingmods.com/dev/resources.html

### Community / Troubleshooting

- Community FAQ: https://wiki.vrisingmods.com/community/faq.html

## Baseline Mod Setup Checklist

Use this checklist when working on V Rising server mod projects:

1. Install BepInEx and run the game/server once to generate folders.
2. Place plugin `.dll` files in `BepInEx/plugins`.
3. Place and review config files in `BepInEx/config`.
4. Verify load status and errors in `BepInEx/LogOutput.log`.
5. Validate in-game/admin command behavior after startup.
6. Confirm server/client configuration matches the wiki guidance.
