[![main](https://img.shields.io/github/workflow/status/RequiDev/ReModCE/main?style=for-the-badge)](https://github.com/RequiDev/ReModCE/actions/workflows/main.yml)
[![All Releases](https://img.shields.io/github/downloads/RequiDev/ReModCE/total.svg?style=for-the-badge&logo=appveyor)](https://github.com/RequiDev/ReModCE/releases)
[![All Releases](https://img.shields.io/github/downloads/RequiDev/ReModCE/latest/total.svg?style=for-the-badge&logo=appveyor)](https://github.com/RequiDev/ReModCE/releases/latest)


# ReMod CE
ReMod Community Edition - A All-in-One VRChat mod to suit all your needs using [MelonLoader](https://github.com/LavaGang/MelonLoader)

# Description
This is essentially a public version of my invite-only VRChat mod. It's a cut-down version with no connection to the ReMod server and with it's security measures removed.  

If you want a feature or have a bug report, head over to the issues page and create an issue for it!

# Installation & Usage
Grab the latest loader from [here](https://api.vrcmg.com/v0/mods/246/ReModCE.Loader.dll) and put it in your Mods directory.  

By default the loader will attempt to load the mod from your VRChat directory using the filename 'ReModCE.dll'. If it doesn't exist, it will attempt to download the latest Version from GitHub.  
If there is a newer version available on GitHub it will automatically update to that version.  

If you don't want automatic updates, you can open your MelonPreferences.cfg with any text editor and set **ParanoidMode** in the **[ReModCE]** category to **true**. That way the loader will notify if there is a new version available, but it won't load it until you download it yourself and replace the old version.  
In case you want to update you can always grab the latest and even previous versions of ReModCE.dll [here](https://github.com/RequiDev/ReModCE/releases/).

# Features
* Unlimited Avatar Favorites with VRC+ (Saved in the Cloud and protected by a PIN!)
* Search public avatars by name, description, author name and author id
* Recently Used Avatars/Avatar History (Remembers up to 25 avatars)
* Global Dynamic Bones with advanced settings so you have full control over where colliders go
* Fly/Noclip (Not usable in Game/Club worlds)
* ESP/Player Highlighting (Not usable in Game/Club worlds)
* Wireframe ESP (Players, Pickups, World) (Not usable in Game/Club worlds)
* Third Person (Not usable in Game/Club worlds)
* Small UI adjustments like adding a "Paste" button to input popups.
* FBT Calibration Saver as already seen at [RequiDev/FBTSaver](https://github.com/RequiDev/FBTSaver)

# Hotkeys
* CTRL + F = Noclip
* CTRL + T = Thirdperson

# Credits
[loukylor](https://github.com/loukylor) - For selected VRChat function reflections and his button mover code which I am using  
[knah](https://github.com/knah) - For his usage of GitHub Action which I've looked at to figure out on how to do CI on GitHub

# Notable Mentions
[emmVRC](https://github.com/emmVRC) - They inspired me to create this project in the first place. This would not exist without them  
