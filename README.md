# MMD4UnityTools

Be more efficient in HDRP to make mmd!

## Installation

Download and import the unitypackage to you project from the release tab.

After import the package, the "MMDExtensions" menu will appear.

### 2022.5 Update

Better organized menu structure, more easy to use, more functionalities!

For assets, all menus are under `MMDExtensions`.

For editor windows, the entry named `MMDExtensions` in the toolbar.

### For Alembic GameObject

A script named `AlembicLooseSeperator` can seperate alembic model to many submesh by linked face, the best use cace I think will be a alembic exported from Marvelous Design, you can fill materails with each fabric.

Add this script to a `AlembicStreamPlayer`, script will find where the abc file located and call blender to seperate it, then set submesh to meshfilter. You have to install blender first.

### For Assets

Tools categoried by resources, materials, animations, textures and so on.

##### `Resources/Import folder as junction`
Now you can link external folder to you project with junction point, this will create a junction point in your assets folder, it works similar as a shortcut, but unity will import assets from it, so you can have a assets library independent from you unity project and reuse them with only one copy. If your unity crashes after upgrade to another version, that maybe caused by juntion point, delete the folder you link in your project can fix it and reimport it later.

#### `Materials/From Standard PBR`

Toolset to create HDRP materials from textures you create with a metallic pbr workflow.

`Create HDRP Material` will create a hdrp material by textures you selected. By select `BaseColor`, `NormalMap`, `Metallic` and any other supported textures, will create a hdrp mask texture from them, this menu only create one material at once, selected texture must be used by a single material.

`Create HDRP Material By selected folders` this method will trade each folder a material, which contains textures used by a single material.

`Upgrade Mateirals` just replace your material with hdrp shader, and reassign textures to correct slots.

#### `Materials/Create/Selected As BaseColor`
Create a hdrp lit material use selected texture as basecolor.

#### `Materials/Upgrade`

Toolset to create or assign basecolor texture reading from pmx file.
For alembic exported from mmdbridge, you select pmx file and the alembic model imported to hierachy. This matchs materials by meshrender order to pmx mateiral order.
For models exported from blender with mmdtools, you select pmx file and materials create form model. This matchs materials by name.
For models create from mmd4mecanim, you select pmx file and materials create form model. This matchs materials by index extract from name.

The methods listed in material section may not works properly.

#### `Animation/Create`

Create camera animation not works properly.
To create morph animation, you select the mesh gameobject which have shapekey and vmd file, this will craete an animation clip for the selected gameobject.

#### `Animation/Set Interpolation For Stopmotion`
Set animation clip inTangent to `PositiveInfinity`, I use this for camera animation create from blender. When the game or you render to video, the fps are higher then 30fps, unity will interpolate camera between frame, not constantly, apply a const interpolation will gives you a stop motion animation or cut scene.

Unity can import blender file as gameobject, create a empty blender project, add a camera, the import camera aniamtion with [MMDTools](https://github.com/UuuNyaa/blender_mmd_uuunyaa_tools) then save to your unity project, expand blender obejct in unity, select animation clip and press `Ctrl D`, this will duplicate a clip you can modify

#### `Texture/InversChannel`

Inverse selected channel, most used for roughness texture.

### Editor Windows

#### `Material/Material Transfer`

Transfer mateiral from model to another model, they may have some same parts or newly exported, you can then transfer materials by vertex count or just orders to other model so you don't need to assign them again manually.

#### `Texture/`

`Gradient To Texture` Create a gradient texture from gradient curve.

`Texture Combiner` Craete a texture that you can fill each channel with a single texture, e.g. to create a hdrp mask texture from textures exported with pbr workflow.


# Credits

[mmd-for-unity](https://github.com/mmd-for-unity-proj/mmd-for-unity)