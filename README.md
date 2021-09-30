# SEdit
SEdit is an experimental runtime scene editor for "Pathfinder: Wrath of the Righteous". It's currently in very early development and isn't recommended for end user usage. For upcoming updates / features and fixes, you can visit the [trello board](https://trello.com/b/dSJWbnOi/sedit)

##Usage

###Controls
Keybinds:
        CTRL+M : Settings
        W : Enter translation/ move mode
        E : Enter rotation mode
        R : Enter scale mode
        Left Arrow : X-Axis
        Right Arrow: Z-Axis
        Up Arrow   : Y-Axis
        Down Arrow : Inverse direction
        
###How to use it

##How to compile it
Follow the [guide](https://github.com/OwlcatOpenSource/WrathModificationTemplate) for the official modding support template.
After everything works, place the SEdit folder into "..\WrathModificationTemplate\Assets\Modifications\" and compile like in the given example (Modification Tools -> Build)
For debugging purposes, use the [remote console](https://github.com/OwlcatOpenSource/RemoteConsole) and select the wanted channels.



##How to install it
Copy the new folder in *Build/*  to **%userprofile%\appdata\locallow\Owlcat Games\Pathfinder Wrath Of The Righteous\Modifications\*
*Create or add to  **%userprofile%\appdata\locallow\Owlcat Games\Pathfinder Wrath Of The Righteous\OwlcatModificationManangerSettings.json** the following lines
 ```json5
{
  "EnabledModifications": [ "SEdit" ] 
}
```
