# Thronefall Multiplayer

This is the development repository for the WIP Thronefall Multiplayer mod.
It uses BepInEx to inject code into the game.

## Development Setup

This is written in CSharp so you will have to acquire an IDE that can read .sln files (for example JetBrains Rider)
You will also need an installation of Thronefall, copy these 4 dlls from Thronefall_Data/Managed to the lib folder
 * Assembly-CSharp.dll
 * AstarPathfindingProject.dll
 * MoreMountains.Feedbacks.dll
 * Rewired_Core.dll
 * MPUIKit.dll
 * Unity.TextMeshPro.dll
 * UnityEngine.UI.dll
 After that open the solution file and run the build.

## How to Run

Download BepInEx 6 and copy it into your Thronefall directory,
then inside of BepInEx/plugins/ThronefallMultiplayer
you need to copy com.badwolf.thronefall_mp.dll, LiteNetLib.dll and MMHOOK_Assembly-CSharp.dll.
Run your executable either directly or through Steam.

## TODO

* Make UI for hosting/connecting to servers
* Make UI to leave a server
