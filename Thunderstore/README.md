# BRC_CharacterAPI
Custom character loader for Bomb Rush Cyberfunk. Currently support importing models as custom characters with up to 4 outfits with custom names. You can also add custom personal graffiti and character can have custom sounds. The repository contains example of custom character under BRC_CharacterAPI_ExamplePlugin folder, everything there is commented so people even unfamiliar with code should be able to make and compile necessary plugin. You can find the example [here](https://github.com/viliger2/BRC_CharacterAPI/tree/main/BRC_CharacterAPI_ExamplePlugin).

You can find a guide on making custom characters [here](https://github.com/viliger2/BRC_CharacterAPI/wiki/Creating-new-character-via-plugin).

If you don't know how\want to compile a plugin, ActualMandM made a pluginless loader for compatable asset bundles. You can find it [here](https://thunderstore.io/c/bomb-rush-cyberfunk/p/MandM/BRC_CharacterLoader/) together with instructions.

Characters made for [BrcCustomCharacters](https://github.com/SGiygas/BrcCustomCharacters) can be loaded with a help of [CharacterAPI_BRCCCLoader](https://thunderstore.io/c/bomb-rush-cyberfunk/p/viliger/CharacterAPI_BRCCCLoader).

Saves are stored inside BepInEx\CharacterAPI\Saves.

## Known issues
* Custom personal graffiti load as gray squares if you painted custom personal graffiti and then disabled CharacterAPI.

## Long term goals (aka never ever)
* Custom models as outfits
* More than 4 outfits
* Custom animations

## I am a mod maker, why does my character have skates angled at wrong position? 
Now you can use skateOffsetL and skateOffsetR transforms to position skates. I'll write a wiki page sometime in the future. 

If you want the old method of fixing via bones you can still find it [here](https://github.com/viliger2/BRC_CharacterAPI/wiki/Why-are-my-skates-at-the-wrong-angle%3F).