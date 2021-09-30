# SEdit
SEdit is an experimental runtime scene editor for "Pathfinder: Wrath of the Righteous". It's currently in very early development and isn't recommended for end user usage. For upcoming updates / features and fixes, you can visit the [trello board](https://trello.com/b/dSJWbnOi/sedit)

## Usage

### Controls
```
Keybinds:
        CTRL+M : Settings
        W : Enter translation/ move mode
        E : Enter rotation mode
        R : Enter scale mode
        Left Arrow : X-Axis
        Right Arrow: Z-Axis
        Up Arrow   : Y-Axis
        Down Arrow : Inverse direction
 ```       
### How to use it
After loading the game, press CTRL+M.
You should see the following:
![1](https://user-images.githubusercontent.com/64482285/135374798-7f2f6d2a-de94-4d77-96c8-140e766cd16f.png)
Click on settings.

After that, press on "Load SEdit into Scene"
![2](https://user-images.githubusercontent.com/64482285/135374800-5dd32f3e-1352-494f-961f-caacbec25201.png)

Now select the option "Show scenes"
![3](https://user-images.githubusercontent.com/64482285/135374802-deb50254-69fa-4ac6-ba71-1da939ad341d.png)

Look for the subscene you want to edit, usually \*\_static and press the button "Make current scene editable"
![4](https://user-images.githubusercontent.com/64482285/135374807-26e9e1d8-3cbe-4d7c-8380-5652d827d0de.png)

You should see the following changes:
![5](https://user-images.githubusercontent.com/64482285/135374817-cfa3e82b-86c6-4293-9c4d-5b0c031928f9.png)

Deselect "Show scenes" and select "Show active Bundles".
![6](https://user-images.githubusercontent.com/64482285/135374819-b58e78ae-d92c-4872-884a-badff882fc5e.png)

With "+" you can expand the bundle .You can select the item you want with shown buttons.
After selecting your object, you can close the overlay with CTRL+M.
With right-click, you can place the object at your desired position (Raycast)
![7](https://user-images.githubusercontent.com/64482285/135374824-d3938e8d-e9aa-4f43-bd71-a9e4a5a40960.png)

If everything worked, you'll be going to see your selected object with a glowing border.
![8](https://user-images.githubusercontent.com/64482285/135374826-a22b446d-bb9c-4371-8ddd-333b9570e150.png)

With the controls listed above, you can move the Game object. In the upper left is the current mode and on the right is the speed, which you can change by clicking inside it and entering your desired value.
![9](https://user-images.githubusercontent.com/64482285/135374843-a85b8192-00bc-4a1a-9539-5f6ca993c4ab.PNG)

Don't forget to save your changes!
![10](https://user-images.githubusercontent.com/64482285/135374847-42aad564-4e19-4f5c-bc07-ff7fc1e15558.png)

The save file is located under ..\\SEdit\\SEditData\\yourguid.json
![11](https://user-images.githubusercontent.com/64482285/135374849-e7d72234-04bc-4c85-ad1f-052c111588aa.png)

## How to compile it
Follow the [guide](https://github.com/OwlcatOpenSource/WrathModificationTemplate) for the official modding support template.
After everything works, place the SEdit folder into "..\WrathModificationTemplate\Assets\Modifications\" and compile like in the given example (Modification Tools -> Build)
For debugging purposes, use the [remote console](https://github.com/OwlcatOpenSource/RemoteConsole) and select the wanted channels.



## How to install it
Copy the new folder in *Build/*  to **%userprofile%\appdata\locallow\Owlcat Games\Pathfinder Wrath Of The Righteous\Modifications\*
*Create or add to  **%userprofile%\appdata\locallow\Owlcat Games\Pathfinder Wrath Of The Righteous\OwlcatModificationManangerSettings.json** the following lines
 ```json5
{
  "EnabledModifications": [ "SEdit" ] 
}
```
