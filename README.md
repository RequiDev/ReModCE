# ReMod CE Unlocked
ReMod Community Edition Unlocked - A All-in-One VRChat mod to suit all your needs using [MelonLoader](https://github.com/LavaGang/MelonLoader)

## Unlocked version notes

This mod is for you if you don't want to run shady malicious obfuscated mods (which often times contain auth token loggers and remote access toolkits) but want risky functions in all worlds. 

Our computers belong to us and we should be able to do whatever we want with them. There are plenty of worlds where risky functions are disabled because of world tags and we shouldn't have to go beg for *permission* (in the case of emmVRC, not ReMod CE) to reenable them on a per-world basis for what should be basic functions available at all times. That said, please be nice and don't cheat or invade people's privacy.

I encourage you to review the code to see the differences with upstream ReMod CE, compile this yourself, and disable automatic updates. Reviewing code that originated upstream is probably unnecessary as it's already reviewed for safety by the VRChat Modding Group.

I might unlock the ability to favorite unlimited avatars without VRC+ in the future. I'm pretty inexperienced when it comes to programming and I just wanted to get something out quickly since emmVRC broke Astral Risk Acceptance on purpose.

## Installation & Usage
**You need at least MelonLoader v0.5.3 since VRChat Build 1169!**

Grab the latest loader from [the releases page]( https://github.com/VRChat-is-Awesome/ReModCE-Unlocked/releases/) and put it in your Mods directory.  

By default the loader will attempt to load the mod from your VRChat directory using the filename 'ReModCE.dll'. If it doesn't exist, it will attempt to download the latest Version from GitHub.  
If there is a newer version available on GitHub it will automatically update to that version.  

If you don't want automatic updates, you can open your MelonPreferences.cfg with any text editor and set **ParanoidMode** in the **[ReModCE]** category to **true**. That way the loader will notify if there is a new version available, but it won't load it until you download it yourself and replace the old version.  
In case you want to update you can always grab the latest and even previous versions of ReModCE.dll [here](https://github.com/VRChat-is-Awesome/ReModCE-Unlocked/releases/).

## Features
* Unlimited Avatar Favorites with VRC+ (Saved in the Cloud and protected by a PIN!)
* Search public avatars by name, description, author name and author id
* Recently Used Avatars/Avatar History (Remembers up to 25 avatars)
* Global Dynamic Bones with advanced settings so you have full control over where colliders go
* Fly/Noclip
* ESP/Player Highlighting
* Wireframe ESP (Players, Pickups, World)
* Third Person
* Small UI adjustments like adding a "Paste" button to input popups
* FBT Calibration Saver as already seen at [RequiDev/FBTSaver](https://github.com/RequiDev/FBTSaver)
* Teleport to other players
* Media Controls which syncs with your Spotify
* Quickly restart in either desktop or VR and automatically teleport to back where you were.
* Copy your current instance join link or join others by using a join link
* Keep track of your visited instances with Instance History
* Not sure if you have already joined an instance? Instance Dejavu will put an icon on instances you have visited before
* Automatically adjusts VRChat News to be collapsible or hide it completely
* Small utilities like disabling chairs in any world, copying user/avatar id and having confirmations for portals

## Hotkeys
* CTRL + F = Noclip
* CTRL + T = Thirdperson

## Forgot your PIN?
Head over to [here](https://remod-ce.requi.dev/api/pin.php) to reset it.