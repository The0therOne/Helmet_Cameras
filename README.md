# Helmet Cameras
Mod adds pov helmet cameras for players, that displays video on the second large monitor at ship. For better CameraMan/ShipGuy/Operator experience for my opinion.

# Features
- Helmets cameras for more than 4 players.
- Mod displays videos from helmet's cameras to second large monitor. It means, that security camera outside the ship now displays on small monitor above the large. Inside ship camera, now is turned off.
- Config for setting quality (High Quality mode. 0 - vanilla (48x48), 1 - vanilla+ (128x128), 2 - mid quality (256x256), 3 - high quality (512x512), 4 - Very High Quality (1024x1024) Default: 0 (Vanilla)
- Synced videos on monitor for all players

# Releases

# Version 1.0.0
- Release
- Some non-critical visual bugs on camera

# Version 1.0.1
- Fix case when cameras MORE than players on server actually connected.

# Version 1.0.3
- Fix case when active camera's player left the game, other cameras were not available.

# Version 1.0.4
- Added config file for turning off/on High Quality Monitor (config/RickArg.lethalcompany.helmetcameras.cfg) Default Value: true

# Version 2.0.0
- Full rework of mod. Thanks a lot for permission to Solo (Solo's Bodycams) use his way to get indexes of players! Im gonna leave this link as my gratitude: https://thunderstore.io/c/lethal-company/p/CapyCat/Solos_Bodycams/
- NOT require changing HideManagerGameObject
- Support more players
- Support radar booster
- Less resources require (Now its only 1 camera that change position)
- New quality options in config
- Less camera glitches, but still it has some.

# Version 2.0.1
- Now you can set renderDistance in config. Default: 70 (in 2.0.0 renderDistance was 150)

# Version 2.1.0
- NOW HELMET CAMERAS NOT FPS EATER. If you need more fps, just lower fps of helmet cameras through config!
- Added config setting for helmet cameras' FPS (High fps of camera was reason of very low game fps.
- Changed default value of renderDistance to 20 (was 70). If you want, you can higher value of it. It has low FPS affection. (Same for monitor quality)

# Version 2.1.1
- Propably fixed cosmetics rendering. It should be better than before. Please, if problem not solved, make an issue on github and tell me about problem with details.

# Version 2.1.2
- Fixed "light flickering" when helmet cameras and FOV Adjust (with hideVisor=true) were installed together.
Thanks a lot to creator of modpack (https://thunderstore.io/c/lethal-company/p/McDoobies/McDoobies_Pack/) for issue on github and detailed info about bug!

# Version 2.1.3
- Performance boost. Now camera freeze (as vanilla radar) while local player not in ship.
Sorry for 2 fixes in 5 min that you have to download ;)
WARNING: If something goes wrong, please downgrade version to 2.1.2 and make an issue on github.