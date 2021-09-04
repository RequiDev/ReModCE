[![main](https://img.shields.io/github/workflow/status/RequiDev/ReModCE/main?style=for-the-badge)](https://github.com/RequiDev/ReModCE/actions/workflows/main.yml)
[![All Releases](https://img.shields.io/github/downloads/RequiDev/ReModCE/total.svg?style=for-the-badge&logo=appveyor)](https://github.com/RequiDev/ReModCE/releases)
[![All Releases](https://img.shields.io/github/downloads/RequiDev/ReModCE/latest/total.svg?style=for-the-badge&logo=appveyor)](https://github.com/RequiDev/ReModCE/releases/latest)


# ReMod CE
ReMod Community Edition - A All-in-One VRChat mod to suit all your needs using [MelonLoader](https://github.com/LavaGang/MelonLoader)

# Description
This is essentially a public version of my invite-only VRChat mod. It's a cut-down version with no connection to the ReMod server and with it's security measures removed.

# Installation & Usage
Grab the latest loader (ReModCE.Loader.dll NOT ReModCE.dll) from the Releases (click [here](https://github.com/RequiDev/ReModCE/releases/latest)) and put it in your Mods directory.  

By default the loader will attempt to load the mod from your VRChat directory using the filename 'ReModCE.dll'. If it doesn't exist, it will attempt to download the latest Version from GitHub.  
If there is a newer version available on GitHub it will automatically update to that version.  

If you don't want automatic updates, you can open your MelonPreferences.cfg with any text editor and set ParanoidMode in the [ReModCE] category to true. This will make it so the loader will notify if there is a new version available, but it won't load it until you download it yourself and replace the old version.

# Features
* Unlimited Avatar Favorites with VRC+ (Saved in the Cloud and protected by a PIN!)
* Recently Used Avatars/Avatar History (Remembers up to 25 avatars)
* Global Dynamic Bones with advanced settings so you have full control over where colliders go
* Fly/Noclip (Not usable in Game/Club worlds)
* Third Person (Not usable in Game/Club worlds)
* Button Adjustment Component. Disable, Move or Resize any button in your quickmenu.
* Ingame Log that will show you anything that ReMod CE will log. At the moment it's a bit unstable (a lot of text will break it) and only supports ReMod CE. More options to come!
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
