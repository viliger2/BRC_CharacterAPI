# BRC_CharacterAPI
Custom character loader for Bomb Rush Cyberfunk. Currently support importing models as custom characters with up to 4 outfits with custom names. You can also add custom personal graffiti and character can have custom sounds. The repository contains example of custom character under BRC_CharacterAPI_ExamplePlugin folder, everything there is commented so people even unfamiliar with code should be able to make and compile necessary plugin. You can find the example [here](https://github.com/viliger2/BRC_CharacterAPI/tree/main/BRC_CharacterAPI_ExamplePlugin).

You can find a guide on making custom characters [here](https://github.com/viliger2/BRC_CharacterAPI/wiki/Creating-new-character-via-plugin).

If you don't know how\want to compile a plugin, ActualMandM made a pluginless loader for compatable asset bundles. You can find it [here](https://thunderstore.io/c/bomb-rush-cyberfunk/p/MandM/BRC_CharacterLoader/) together with instructions.

## Known issues
* Custom personal graffiti load as gray squares if you painted custom personal graffiti and then disabled CharacterAPI.

## Short term goals (things that will most likely come in near future)
* ~~Implement custom voice support~~ Done
* ~~Implement save system for custom characters (currently nothing you setup your custom character with saves on exit)~~ Done
* ~~Maybe rewrite parts of the code, since by the end I got lazy and started using On hooks instead of IL (will be done once I get enought reports that new version is stable)~~ Done

## Long term goals (aka never ever)
* Custom models as outfits
* More than 4 outfits
* Custom animations

## I am a mod maker, why does my character have skates angled at wrong position? 
I've made a wiki page covering on how to fix it. You can find it [here](https://github.com/viliger2/BRC_CharacterAPI/wiki/Why-are-my-skates-at-the-wrong-angle%3F).