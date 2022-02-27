The official root repository for this project is here https://github.com/rygo6/GTAvaCrypt. Be sure you are downloading it only from there.

Be careful to not download code from forked repositories unless you trust the fork. People can fork repositories and add malicious code without the original author having any control or knowledge of it.

# GeoTetra AvaCrypt V2

This is a rather invasive anti-avatar-ripping system to be used for VRChat. It will protect against your avatar being ripped, extracted and edited. It will also protect against your avatar being ripped and re-uploaded without edits.

This system will randomize all the vertices of your avatar's mesh, then write that to disk. Then rely on a custom shader with a 32 bit key to un-randomize the vertex positions in game. This is <b>not</b> done through blend shapes. Rather this will copy, and destructively edit, the 'Basis' layer of your mesh.

## GTAvaCryptV2

AvaCrypt V1 has been a moderate success. Having gathered dozens of followers in it's Discord, a few imitators of similar technique, and even a few businesses trying to commercialize it. It is the positive feedback that makes me see this as being justified to continue on. But really AvaCrypt V1 was more a quick proof of concept. It does introduce a significant difficulty to avatar ripping but it can go so much further.

### New Features Of Version 2:
1. You no longer have to input keys into the Avatar 3.0 menu, the package will write the keys to the saved 3.0 avatar parameters file in your VRChat data folder.

2. The keys are now stored and transferred as 32 separate 1-bit bools. It still takes up 32 bits in parameter data, but now it fully utilizes all 32 bits, as before it effectively used maybe 20 of those bits. Before someone would have to brute force a maximum of 1,185,921 combinations. Now they would have to brute force a maximum of 4,294,967,296 combinations. If each brute force attempt took 0.1 seconds it would take over 13 years to brute force, people could put a small compute farm to do this in a number of days, but rarely will someone care to spend the money to do so. This means if you never take your avatar into a public lobby where someone could use a mod to read your keys, it is quite secure.

3. The mesh obfuscation math is now randomly generated and written into the shader each time you encrypt a mesh. What this means is just having the keys is no longer enough to decrypt an avatar. One would also have to decompile and reverse engineer the shader from the asset bundle itself. Currently this makes avatars un-rippable with any currently released tools for VRChat avatar ripping. As Unity 2019 changed the way the shaders get packed into an AssetBundle, now you can only get compiled shader bytecode out of the AssetBundle and no public tool has implemented functionality to do so. If someone were to rip an avatar with this they'd first need to implement a system to decompile the shader bytecode out of an asset bundle, analyze it to determine where the obfuscation math is, then reverse engineer that into C#, or other, to decrypt the mesh. Making such a shader decompiling and transpiling system is a significant difficulty for someone knowledgeable.

## But still, understand no encryption system is 100% secure.
<b>If someone is committed they can still rip your avatar. Nothing will be able to protect your assets 100% for certain. However this introduces significant complication.</b>

I am up-front about how exactly you would undo AvaCrypt so that people can be on the lookout for when certain anti-ripping techniques may be compromised. Someone did write a mod that could compromise AvaCryptV1 if you were in the same world as them, someone else wrote a script that could maybe brute force the AvaCryptV1 obfuscation some percentage of the time if your distortion value was too low. 

The key now being truly 32 bits does introduce a significant enough barrier to brute forcing that most probably just won't spend the time to do. So this is quite effective against avatars ripped purely off the VRChat API. 

Going in public worlds is more compromising as someone could read your keys through a mod, so they could download the avatar, re-upload it and use those keys, but until there is a tool for decompiling and transpiling shader bytecode they wouldn't be able to decrypt and make use of the assets.

However if you discover or hear about a tool to decompile the shader bytecode from 2019 Unity Asset Bundles please drop a note in the [GeoTetra Discord](https://discord.gg/nbzqtaVP9J). I actually have a long list of anti-ripping schemes I can progressively implement, but will only do so as necessary.

## Caveats of this System

Please take the time to understand the caveats of this system, as it does produce a few complications you must be aware of.

1. For a user to see your avatar properly in VRChat, they must have your avatar fully shown. Shaders, animations and all. So this system is only ideal for avatars you use in worlds where most people are your friends.

2. This synchronizes the key with Avatar 3.0 parameters and does take up 32 bits. So this system can only work with avatars that use the VRChat Avatar 3.0 SDK. 

3. Shaders must be manually edited to work with AvaCrypt. Currently there is a fork of PoiyomiToonShader which is maintained to work with AvaCrypt available here https://github.com/rygo6/GTPoiyomiToonShader. You must remove your existing version of PoiyomiToonShader and only use this GTPoiyomiToonShader fork.

 <i>In the past I offered to potentially edit shaders to work with this. Which I never really had time to do, but do not think I will offer that anymore. As making a more comprehensive anti-ripping scheme than what this currently is that could also obfuscate shaders will require a shader designed from the ground up for it. Which is where I will probably be taking V3 of this, it will come with a completely new shader with more low-level anti-ripping techniques in it. I will probably only want to put time towards this new shader going forward rather than adapting old shaders. I would be curious to hear anything you think would be essential for a new avatar shader though.</i>

## Usage Instructions

### Backup your poject before running these operations in case it doesn't work properly and causes difficult to fix, or impossible to fix, changes in your project. 

### Really do it. Close Unity, all your programs, and make a full clean copy of your entire Unity Project. Or better yet, learn to use git. A small percentage of avatars did have odd things in their mesh that just wouldn't work, or could cause errors, and the script could leave some assets in the project in a rather messed up state.

#### Upgrading from V1

If you are upgrading from V1 you will want to clear out everything previously related. This is not a small delta change, many things are fundamentally changed. Also it is made to work with the latest Poiyomi which has also introduced significant changes since V1.

1. Select your avatar and delete the AvaCryptRoot V1 component.
2. Delete the AvaCrypt key entries from your VRCExpressionParameters.
3. Remove the AvaCrypt key menu from VRCExpressionsMenu.
4. Delete the entire GTAvaCrypt folder and the entire GTPoiyomiShader folder. Please note that this did upgrade to use the latest Poiyomi, so when you pull in the new Poiyomi all your shader refs will be broken! But if you go to each material and select the new version under '.poiyomi/PoiyomiToon' it should repopulate the new shader with however you had it configured previously.
5. After you install the new packages, there is a new button on the AvaCryptV2Root component under the 'Debug' foldout at the bottom of it that says 'Delete AvaCryptV1 Objects From Controller'. This should delete all the old AvaCryptV1 layers and blend trees. But still go into the FX AnimatorController and delete any old AvaCrypt keys or layers. You can also delete all the 'AvaCryptKey0' 'AvaCryptKey100' animation files it generated next to your controller.

#### Import AvaCrypt and Poiyomi.

1. Ensure you are using latest VRC SDK.
2. Download the code for GTAvaCryptV2 and GTPoiyomiShader by clicking the 'Code' button in the upper right, followed by 'Download Zip'. Import it into your project. The V2 branch of Poiyomi is here: https://github.com/rygo6/GTPoiyomiToonShader/tree/feature/v2

#### Setup VRC Components.

1. Add the `AvaCryptV2Root` component onto the root GameObject of your avatar, next to the `VRCAvatarDescriptor` component.

![Steps 1](Textures/DocSteps1.png)

2. Ensure your `VRCAvatarDescriptor` has an AnimatorController specified in the 'FX Playable Layer' slot. <b>The AnimatorController you specify should not be shared between multiple avatars, AvaCrypt is going to write states into the controller which will need to be different for different avatars.</b>
3. Ensure there is also an `Animator` component on this root GameObject, and that its 'Controller' slot points to the same AnimatorController in the 'FX Playable Layer' slot on the `VRCAvatarDescriptor`.

![Steps 1](Textures/DocSteps2to3.png)

5. In the 'Parameters' slot of your `VRCAvatarDescriptor` ensure you have an 'Expression Parameters' object.

![Step 4](Textures/DocSteps4.png)

6. <i>Optional V1 Cleanup step.</i> If you have AvaCrypV1 installed, go to 'AvaCryptV2Root' component and under the 'Debug' foldout at the bottom click that button that says 'Delete AvaCryptV1 Objects From Controller'. This should delete all the old AvaCryptV1 layers and blend trees. But still go into the FX AnimatorController and delete any old AvaCrypt keys or layers you see. You can also delete all the 'AvaCryptKey0' 'AvaCryptKey100' animation files it previously generated next to your controller.

#### Setup New Poiyomi

1. This step is a real annoyance, may figure how to automate it. So apologize, but for now this is what you have to do.
2. New Poiyomi shaders can Lock and Unlock to optimize the resulting code. When you Encrypt a mesh, since it writes out new shader code, all of your Poiyomi Materials need to Unlocked! So click through each Poiyomi material your avatar uses and Unlock them.

![Step 4](Textures/DocStepsPoiyomiUnlock.png)

3. Also in the new Poiyomi you have to right click on any parameter you want to be animatable in-game, all the keys need to be animatable. So go to each material you are using with Poiyomi on your avatar, scroll down to the very bottom under the 'Debug' foldout and right-click on _Key0-_Key31. When you right-click a little clock icon should appear to the left of it.
4. Also ensure the 'Enable AvaCrypt' bool is checked.
   
![Step 4](Textures/DocStepsPoiyomi.png)

#### Encrypting and Uploading

1. Ensure any meshes you wish to have encrypted are using the new V2 edit of Poiyomi. It will skip over mesh that do not use this shader.
2. On the `AvaCryptV2Root` component click the 'Encrypt Avatar' button. This will make all necessary edits to your AnimatorController, and make a duplicate of your avatar which is encrypted. Be aware your duplicated avatar with "_Encrypted" appended to it's name will appear completely garbled in the editor. This is what other users will see if they do not have your avatar shown. Do not set the keys on the material inside the Unity Editor.
3Go to the VRChat SDK Menu then 'Build and Publish' your avatar which has '_Encrypted' appended to the name.

#### Writing Keys

1. If this is the first time you have uploaded this avatar, after upload completes, go to the GameObject of your encrypted avatar. Find the `Pipeline Manager` component and copy it's blueprint ID. Then paste the blueprint ID into the `Pipeline Manager` on the un-encrypted avatar and click 'Attach'.
2. Now on the AvaCryptV2Root component click the 'Write Keys' button. This will actually read in and alter the saved 3.0 parameters from your VRChat folder to include the new key so you don't have to enter them in-game. <i>This also means if you "Reset Avatar" in game through the 3.0 menu, it will reset your keys and you will need to re-export them with the 'Write Keys' button!</i>
3. This should provide ample error dialogues or console errors, so ensure no errors came up!. It should popup a success dialogue if it did it correctly. If there were issues make sure the 'Vrc Saved Params Path' actually points to your LocalAvatarData folder.
4. You only need to run 'Write Keys' once on first setup, or when you change keys.


If you have any more questions, or suggestions, feel free to join the GeoTetra discord:
https://discord.gg/nbzqtaVP9J
