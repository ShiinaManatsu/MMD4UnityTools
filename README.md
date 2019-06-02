# MMD4UnityTools

Tools for unity helps make mmd easy, target as 2019 or newer, in 2018 you'll need some modify

## Installation

Download the unitypackage to you project

## Usage

After install there will be a "MMDHelper" menu added

menu: 
![alt text][menu]

### Auto Assign

This can fill the abc file material with pmx material list exported from pmx editor

#### Step by step

* Get pmx editor plugin [Here](https://github.com/ShiinaManatsu/PmxEditorPlugins "PmxEditorPlugins")
* Load your model and save to json ![alt text][save]
* Load all model files to the untiy project
* Add .abc model to the scene
* Select model, all texture needed and json file just created
* Press auto assign then finish assign and will save all material to assets

### MMD4Material To HDMaterial

Select the material created by MMD4Mecanim and right click on it and click MMDHelper-Upgrade To Lit Material

### Texture To HDMaterial

Select texture file right click on it and click MMDHelper-Create Lit Material wil create hdrp lit material with selected texture


[menu]:https://raw.githubusercontent.com/ShiinaManatsu/MMD4UnityTools/master/Images/Menu.png "Menu"
[save]:https://raw.githubusercontent.com/ShiinaManatsu/MMD4UnityTools/master/Images/SaveJson.png "Save to json"
