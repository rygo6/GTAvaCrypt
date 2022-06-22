The official root repository for this project is here https://github.com/rygo6/GTAvaCrypt. Be sure you are downloading it only from there.

This software is provided fully open sourced with no compiled DLLs and no obfuscated code, so it can be inspected by anyone to not be malicious. All PRs to original root repo  https://github.com/rygo6/GTAvaCrypt I personally validate and am open to anyone contributing.

Be careful to not download code from forked repositories unless you trust the fork. People can fork repositories and add malicious code without the original author having any control or knowledge of it. Be careful of importing closed source projects or compiled DLLs into an Unity Project as the Unity Editor typically has higher than average system privilidges and antivirus software will generally not catch malicious code.

# GeoTetra AvaCrypt V2.2

This is a rather invasive anti-avatar-ripping system to be used for VRChat. It will protect against your avatar being ripped, extracted and edited. It will also protect against your avatar being ripped and re-uploaded without edits.

This system will randomize all the vertices of your avatar's mesh, then write that to disk. Then rely on a custom shader with a 32 bit key to un-randomize the vertex positions in game. This is <b>not</b> done through blend shapes. Rather this will copy, and destructively edit, the 'Basis' layer of your mesh.

Technically this is more like 'Avatar Obfuscation' but calling it 'AvaObfs' didn't really have the same ring to it. But maybe it will keep evolving to use some advanced GPU encryption techniques some day...

## GTAvaCryptV2

AvaCrypt V1 has been a moderate success. Having gathered dozens of followers in it's Discord, a few imitators of similar technique, and even a few businesses trying to commercialize it. It is the positive feedback that makes me see this as being justified to continue on. But really AvaCrypt V1 was more a quick proof of concept. It does introduce a significant difficulty to avatar ripping but it can go so much further.

### New Features Of Version 2.2.1:

1. Added option under `Tools > GeoTetra > GTAvaCrypt > Unlock All Poi Materials In Hierarchy...` to unlock all Poiyomi 8 materials under a selected GameObject. *~thanks Meru for suggestion*
2.  Added option under `Tools > GeoTetra > GTAvaCrypt > Check for Update...` to automatically update the package if one is available to make future updates easier.

### New Features Of Version 2.2:

Upgraded to work with Poiyomi 8, fixed the terrible workflow with Poiyomi shader and can now be installed through Unity Package Manager.

1. The GTPoiyomiToon fork has now been made obsolete and this works with the official Poiyomi 8 package. 
2. You no longer have to right-click on the 'BitKeys' to mark them animatable. The bitkeys aren't even material properties anymore.
3. AvaCrypt will "inject" its code into the locked PoiyomiShader when you click "EncryptAvatar" on the AvaCryptV2Root. It does not alter the unlocked PoiyomiShader. So if you want to turn off the AvaCrypt obfuscation to see your mesh again, just unlock the PoiyomiToon material.

### New Features Of Version 2:
1. You no longer have to input keys into the Avatar 3.0 menu, the package will write the keys to the saved 3.0 avatar parameters file in your VRChat data folder.

2. The keys are now stored and transferred as 32 separate 1-bit bools. It still takes up 32 bits in parameter data, but now it fully utilizes all 32 bits, as before it effectively used maybe 20 of those bits. Before someone would have to brute force a maximum of 1,185,921 combinations. Now they would have to brute force a maximum of 4,294,967,296 combinations. If each brute force attempt took 0.1 seconds it would take over 13 years to brute force, people could put a small compute farm to do this in a number of days, but rarely will someone care to spend the money to do so. This means if you never take your avatar into a public lobby where someone could use a mod to read your keys, it is quite secure.

3. The mesh obfuscation math is now randomly generated and written into the shader each time you encrypt a mesh. What this means is just having the keys is no longer enough to decrypt an avatar. One would also have to decompile and reverse engineer the shader from the asset bundle itself. Currently this makes avatars un-rippable with any currently released tools for VRChat avatar ripping. As Unity 2019 changed the way the shaders get packed into an AssetBundle, now you can only get compiled shader bytecode out of the AssetBundle and no public tool has implemented functionality to do so. If someone were to rip an avatar with this they'd first need to implement a system to decompile the shader bytecode out of an asset bundle, analyze it to determine where the obfuscation math is, then reverse engineer that into C#, or other, to decrypt the mesh. Making such a shader decompiling and transpiling system is a significant difficulty for someone knowledgeable.

## But still, understand no encryption system is 100% secure.
<b>If someone is committed they can still rip your avatar. Nothing will be able to protect your assets 100% for certain. However this introduces significant complication.</b>

I am up-front about how exactly you would undo AvaCrypt so that people can be on the lookout for when certain anti-ripping techniques may be compromised. Someone did write a mod that could compromise AvaCryptV1 if you were in the same world as them, someone else wrote a script that [could maybe brute force AvaCryptV1 if your distortion ratio is low and the mesh is simpler](https://gitlab.com/-/snippets/2202094), but that script didn't manage to undo any of my avatars.

The key now being truly 32 bits does introduce a significant enough barrier to brute forcing that most probably just won't spend the time to do. So this is quite effective against avatars ripped purely off the VRChat API. 

Going in public worlds is more compromising as someone could read your keys through a mod, so they could download the avatar, re-upload it and use those keys, but they would still have to decompile and reverse engineer the shader bytecode to get the assets in an usable state.

## Caveats of this System

Please take the time to understand the caveats of this system, as it does produce a few complications you must be aware of.

1. For a user to see your avatar properly in VRChat, they must have your avatar fully shown. Shaders, animations and all. So this system is only ideal for avatars you use in worlds where most people are your friends.

2. This synchronizes the key with Avatar 3.0 parameters and does take up 32 bits. So this system can only work with avatars that use the VRChat Avatar 3.0 SDK. 

3. Shaders must be manually edited to work with AvaCrypt. Currently this system will inject itself into the official Poiyomi 8 ToonShader when you click "Encrypt Mesh". Any material which is not Poiyomi 8 will simply be ignored.

 <i>This could be adapted to other shaders but I really don't have time to port it to every shader out there, so I will only support the latest Poiyomi for now.</i>

## Usage Instructions

### Backup your poject before running these operations in case it doesn't work properly and causes difficult to fix, or impossible to fix, changes in your project. 

### Really do it. Close Unity, all your programs, and make a full clean copy of your entire Unity Project. Or better yet, learn to use git. A small percentage of avatars did have odd things in their mesh that just wouldn't work, or could cause errors, and the script could leave some assets in the project in a rather messed up state.

[comment]: <> (#### Upgrading from V2.2.1 Onwards...)

[comment]: <> (1. If you installed via Unity Package Manager click `Tools > GeoTetra > GTAvaCrypt > Check for Update...`.)

[comment]: <> (If you didn't install via Unity Package Manager, delete and re-import. But should use Unity Package Managers, make it easier and keeps your Assets folder cleaner.)

#### Upgrading from V2.2 to V2.2.1

Hopefully this is the last time you have to do anything but click a single button to upgrade. 

1. In the Unity Editor click `Window > Package Manager`. Then in the Package Manager window click the `+` in the upper left corner and select `Add package from git url...` and then paste `https://github.com/rygo6/GTAvaCrypt.git` in the field and click `Add`. This will update the package if its already in the list.

*If you get an error about git not being installed, you may need to install the git package from here: https://git-scm.com/*

#### Upgrading from V2 to V2.2

Upgrade should be relatively painless and not break anything. Future upgrades from here should be even simpler as it no longer uses an altered poiyomi and installs through the package manager.

1. Delete the entire GTPoiyomiToon folder and import the official package from https://github.com/poiyomi/PoiyomiToonShader.
2. Delete the old GTAvaCrypt folder. 
3. In the Unity Editor click `Window > Package Manager`. Then in the Package Manager window click the `+` in the upper left corner and select `Add package from git url...` and then paste `https://github.com/rygo6/GTAvaCrypt.git` in the field and click `Add`. This will clone the package via the Package Manager into the Package folder.

*If you get an error about git not being installed, you may need to install the git package from here: https://git-scm.com/*

#### Upgrading from V1

If you are upgrading from V1 you will want to clear out everything previously related. This is not a small delta change, many things are fundamentally changed. Also it is made to work with the latest Poiyomi which has also introduced significant changes since V1.

1. Select your avatar and delete the AvaCryptRoot V1 component.
2. Delete the AvaCrypt key entries from your VRCExpressionParameters.
3. Remove the AvaCrypt key menu from VRCExpressionsMenu.
4. Delete the entire GTAvaCrypt folder and the entire GTPoiyomiShader folder. Please note that this did upgrade to use the latest Poiyomi, so when you pull in the new Poiyomi all your shader refs will be broken! But if you go to each material and select the new version under '.poiyomi/PoiyomiToon' it should repopulate the new shader with however you had it configured previously.
5. After you install the new packages, there is a new button on the AvaCryptV2Root component under the 'Debug' foldout at the bottom of it that says 'Delete AvaCryptV1 Objects From Controller'. This should delete all the old AvaCryptV1 layers and blend trees. But still go into the FX AnimatorController and delete any old AvaCrypt keys or layers. You can also delete all the 'AvaCryptKey0' 'AvaCryptKey100' animation files it generated next to your controller.

#### Install AvaCrypt and Poiyomi.

1. Ensure you are using latest VRC SDK.
2. Download the Poiyomi 8 package from https://github.com/poiyomi/PoiyomiToonShader and import it into your Unity project.
3. In the Unity Editor click `Window > Package Manager`. Then in the Package Manager window click the `+` in the upper left corner and select `Add package from git url...` and then paste `https://github.com/rygo6/GTAvaCrypt.git` in the field and click `Add`. This will clone the package via the Package Manager into the Package folder.

*If you get an error about git not being installed, you may need to install the git package from here: https://git-scm.com/*

#### Setup VRC Components.

1. Add the `AvaCryptV2Root` component onto the root GameObject of your avatar, next to the `VRCAvatarDescriptor` component.

![Steps 1](Textures/DocSteps1.png)

2. Ensure your `VRCAvatarDescriptor` has an AnimatorController specified in the 'FX Playable Layer' slot. <b>The AnimatorController you specify should not be shared between multiple avatars, AvaCrypt is going to write states into the controller which will need to be different for different avatars.</b>
3. Ensure there is also an `Animator` component on this root GameObject, and that its 'Controller' slot points to the same AnimatorController in the 'FX Playable Layer' slot on the `VRCAvatarDescriptor`.

![Steps 1](Textures/DocSteps2to3.png)

5. In the 'Parameters' slot of your `VRCAvatarDescriptor` ensure you have an 'Expression Parameters' object.

![Step 4](Textures/DocSteps4.png)

6. <i>Optional V1 Cleanup step.</i> If you have AvaCrypV1 installed, go to 'AvaCryptV2Root' component and under the 'Debug' foldout at the bottom click that button that says 'Delete AvaCryptV1 Objects From Controller'. This should delete all the old AvaCryptV1 layers and blend trees. But still go into the FX AnimatorController and delete any old AvaCrypt keys or layers you see. You can also delete all the 'AvaCryptKey0' 'AvaCryptKey100' animation files it previously generated next to your controller.

#### Encrypting and Uploading

1. Ensure any meshes you wish to have encrypted are using Poiyomi 8. It will skip over meshes that do not use this shader.
2. On the `AvaCryptV2Root` component click the 'Encrypt Avatar' button. This will lock all of your Poiyomi materials, make all necessary edits to your AnimatorController, and make a duplicate of your avatar which is encrypted. Be aware your duplicated avatar with "_Encrypted" appended to it's name will appear completely garbled in the editor. This is what other users will see if they do not have your avatar shown. *Do not set the keys on the material inside the Unity Editor.*
3. Go to the VRChat SDK Menu then 'Build and Publish' your avatar which has '_Encrypted' appended to the name.

#### Writing Keys

1. If this is the first time you have uploaded this avatar, after upload completes, go to the GameObject of your encrypted avatar. Find the `Pipeline Manager` component and copy it's blueprint ID. Then paste the blueprint ID into the `Pipeline Manager` on the un-encrypted avatar and click 'Attach'.
2. Now on the AvaCryptV2Root component click the 'Write Keys' button. Ensure VRC is closed when you do this, as VRC might disallow writing to the file. This will actually read in and alter the saved 3.0 parameters from your VRChat folder to include the new key so you don't have to enter them in-game. <i>This also means if you "Reset Avatar" in game through the 3.0 menu, it will reset your keys and you will need to re-export them with the 'Write Keys' button!</i>
3. This should provide ample error dialogues or console errors, so ensure no errors came up!. It should popup a success dialogue if it did it correctly. If there were issues make sure the 'Vrc Saved Params Path' actually points to your LocalAvatarData folder.
4. You only need to run 'Write Keys' once on first setup, or when you change keys.

*Ensure VRChat is closed when you write keys otherwise VRChat may just overwrite them immediately with zeroes! ~thanks Meru*

#### Un-Encrypting Poiyomi Material in Editor

If you wish to see your avatar again as normal and not encrypted, unlock all of your Poiyomi materials. AvaCrypt only writes itself into the locked Poiyomi shader files, so you can fully turn it off just by unlocking the materials again. 

If you do unlock any of the Poiyomi materials you will need to click the 'Encrypt Avatar' button again before uploaded, as it is during that process that it will inject itself into the locked Poiyomi shaders.




If you have any more questions, or suggestions, feel free to join the GeoTetra discord:
https://discord.gg/nbzqtaVP9J
