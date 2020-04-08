# MMD4UnityTools

Be more efficient in HDRP to make mmd!

## Installation

Download and import the unitypackage to you project from the release tab.

After import the package, the "MMDExtensions" menu will appear.

![alt text][menu1]

*1.1.0.1 Update: Now you can find the menu item in both title bar and right click context menu.

![alt text][menu2]

## Usage

### MMD Extensions for material

#### Upgrade MMD4 Material (HDRP)

For the model converted by MMD4Mecanim, you can use `Upgrade MMD4 Material (HDRP)` to convert material that create by MMD4Mecanim. But first, you need to delete all the shader that contains in the MMD4Mecanim package, you can find them in `Assets/MMD4Mecanim/Shaders`.

To use this function, you'll need to extract all the materials from the fbx inspector by click the `Extract Materials` button.

![alt text][InstructionMMD4Mat]

Then you need to select both pmx file and materials just extracted and click `Upgrade MMD4 Material (HDRP)`.

*1.1.0.3 Update: Now you can only select the materials without select specific pmx file, but you can still specify the pmx file if you want

#### Upgrade Blender Materials

For the fbx model that export from blender mmdtools or Cats, you need to extract the material just like what we just do, because of the different of material name, we don't merge the function together, so agin, you need to select both pmx file and materials just extracted and click `Upgrade Blender Materials`.

*1.1.0.3 Update: Now you can only select the materials without select specific pmx file, but you can still specify the pmx file if you want

#### Upgrade ABC Model Material

For the abc model exported from MMD Bridge, you need to select both you abc model in the hierarchy and the pmx you use in MMD Bridge, then click `Upgrade ABC Model Material`, it will ask you to save the new materials where you can modify them later.

![alt text][InstructionUpgradeABC]


#### Create Materials From Textures

This use to create materials from the textures you selected, new materials will be save in the same folder level but the folder will be named as `Materials`

### MMD Extensions for VMD file

There are two method in the VMD menu that use for create camera animation and morph animation

![alt text][VMDMenu]

#### Create Camera Animation

For create a camera animation clip, you need to right click on a vmd file that used for camera and click `Create Camera Animation`, it will create a new clip in the same place.

#### Create Morph Animation

For create a morph animation clip, you need to select the game object in the hierarchy where a mesh renderer has the `BlendShapes` property, then select the vmd file and click `Create Morph Animation`, it will create a new clip in the same place.

![alt text][CreateMorphAnimation]

# Credits
[mmd-for-unity](https://github.com/mmd-for-unity-proj/mmd-for-unity)


[menu1]:https://raw.githubusercontent.com/ShiinaManatsu/MMD4UnityTools/master/Images/Menu1.png "Menu1"
[menu2]:https://raw.githubusercontent.com/ShiinaManatsu/MMD4UnityTools/master/Images/Menu2.png "Menu2"
[VMDMenu]:https://raw.githubusercontent.com/ShiinaManatsu/MMD4UnityTools/master/Images/VMDMenu.png "VMDMenu"
[InstructionUpgradeABC]:https://raw.githubusercontent.com/ShiinaManatsu/MMD4UnityTools/master/Images/InstructionUpgradeABC.png "InstructionUpgradeABC"
[InstructionMMD4Mat]:https://raw.githubusercontent.com/ShiinaManatsu/MMD4UnityTools/master/Images/InstructionMMD4Mat.png "InstructionMMD4Mat"
[CreateMorphAnimation]:https://raw.githubusercontent.com/ShiinaManatsu/MMD4UnityTools/master/Images/CreateMorphAnimation.png "CreateMorphAnimation"
