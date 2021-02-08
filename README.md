# GeoTetra AvaCrypt

This is probably the most comprehensive, and invasive, anti-avatar-ripping system that has been publicly released for VRChat. It will protect against your avatar being ripped, extracted and edited. It will also protect against your avatar being ripped, and reuploaded without edits.

This system will randomize all the vertices of your avatar's mesh, then write that to disk. Then rely on a custom shader, with four decryption keys, to un-randomize the vertex positions in game. This is <b>not</b> done through blend shapes. Rather this will copy, and destructively edit, the 'Basis'' layer of your mesh.

## Understand no encryption system is 100% secure.
<b>If someone is committed they can still rip your avatar. Nothing will be able to protect your assets 100% for certain. However, with AvaCrypt it would take a knowledgeable Software Engineer multiple days to rip your avatar.</b>

## Caveats of this System

Please take the time to understand the caveats of this system, as it does produce a few complications you must be aware of.

1. For a user to see your avatar properly in VRChat, they must have your avatar fully shown. Shaders, animations and all. So this system is only ideal for avatars you use in worlds where most people are your friends.

2. You must enter the four "AvaCrypt Keys" into the Avatar 3.0 puppeting menu. Meaning, this system can only work with avatars that use the VRChat Avatar 3.0 SDK. These four "AvaCrypt Keys" should be saved in the latest version of VRChat.

3. Shaders must be manually edited to work with AvaCrypt. Currently there is a fork of PoiyomiToonShader which is maintained to work with AvaCrypt available here https://github.com/rygo6/GTPoiyomiToonShader. You must remove your existing version of PoiyomiToonShader and only use this GTPoiyomiToonShader fork.

  <i>If there are any other open-source shaders you would like edited to work with AvaCrypt, feel free to request it.</i>

## Usage Instructions

### Backup your poject before running these operations in case it doesn't work properly and causes diffucult to fix changes in your project. This is still alpha, a few users have reported that it does not work properly with their avatar mesh. If you find this to be the case with your avatar, join the [GeoTetra Discord](https://discord.gg/nbzqtaVP9J) and send me your avatar mesh if you are willing, then I will try to fix it.</span>

First, select the FBX file of your Avatar in the Project Pane in the Unity Editor. View the FBX file's "Import Settings" in the Inspector Pane, in here set "Normals" to "Import". This may not always be necessary, but I found it was for my avatars. If your avatar mesh is a .Asset rather than a .FBX, this may not work, I have not tested it heavily with .Asset mesh sources.

![Step 0](Textures/DocSteps0.png)

1. Add the `AvaCryptRoot` component onto the root GameObject of your avatar, next to the `VRCAvatarDescriptor` component. Take note of the four "Key" values which are shown in this component, these are the values you must enter into your Avatar 3.0 puppeting menu.
2. Ensure your `VRCAvatarDescriptor` has an AnimatorController specified in the 'FX Playable Layer' slot. <b>The AnimatorController you specify should not be shared between multiple avatars, AvaCrypt is going to write states into the controller which will need to be different for different avatars.</b>
3. Ensure there is also an `Animator` component on this root GameObject, and that its 'Controller' slot points to the same AnimatorController in the 'FX Playable Layer' slot on the `VRCAvatarDescriptor`.

![Steps 1](Textures/DocSteps1.png)

![Steps 2to3](Textures/DocSteps2to3.png)

4. In the 'Parameters' slot of your `VRCAvatarDescriptor` ensure you have an 'Expression Parameters' object with the following parameters to set to `Float`.
  - AvaCryptKey0
  - AvaCryptKey1
  - AvaCryptKey2
  - AvaCryptKey3

![Step 4](Textures/DocSteps4.png)

5. In the 'Menu' slot of your `VRCAvatarDescriptor` ensure you have an 'Expressions Menu' asset which has 'AvaCryptKeyMenu' set as a submenu. This menu asset can be found in `GTAvaCrypt/VrcExpressions` folder.

![Step 5](Textures/DocSteps5.png)

6. Ensure any meshes you wish to have encrypted use a shader that supports AvaCrypt. Currently the only shader which supports this is the fork of PoiyomiToonShader available here: https://github.com/rygo6/GTPoiyomiToonShader
7. On the `AvaCryptRoot` component click the 'Encrypt Avatar' button. This will make all necessary edits to your AnimatorController, and make a duplicate of your avatar which is encrypted. Be aware your duplicated avatar with "_Encrypted" appended to it's name will appear completely garbled in the editor. This is what other users will see if they do not have your avatar shown. Do not set the keys on the material inside the Unity Editor, only set the keys in the Avatar 3.0 puppeting menu in VRChat.
8. Go to the VRChat SDK Menu then 'Build and Publish' your avatar which has '_Encrypted' appended to the name.
9. If this is the first time you have uploaded this avatar, after upload completes, go to the GameObject of your encrypted avatar. Find the `Pipeline Manager` component and copy it's blueprint ID. Then paste the blueprint ID into the `Pipeline Manager` on the un-encrypted avatar and click 'Attach'.

If you have any more questions, or suggestions, feel free to join the GeoTetra discord:
https://discord.gg/nbzqtaVP9J
