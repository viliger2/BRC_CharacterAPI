# BRC_CharacterAPI
Custom character loader for Bomb Rush Cyberfunk. Currently support importing models as custom characters with up to 4 outfits with custom names. You can also add custom personal graffiti. This repository contains example of custom character under BRC_CharacterAPI_ExamplePlugin folder, everything there is commented so people even unfamiliar with code should be able to make and compile necessary plugin.

All assmblies in this repository are stripped down versions made with NStrip.

Please consider making a back-up of your save. I've made necessary precautions in the code so you shouldn't bork your save, but just to be safe until I get enough reports that things are stable - back it up.

## Known issues
* Custom character state (select outfit, ride, skin) do not save between game restarts. This will hopefully come in the future.
* Upon quitting the game with custom characters and then stating the game again, you will start as Red. This is made to prevent crashing and save corruption, since if you put custom character in the save game won't know what to load AND if you decide to delete the mod you save will be broken.
* Custom personal sprays load as gray squares. This will also hopefully be fixed in the future with implementation of save system.

## Short term goals (things that will most likely come in near future)
* Implement custom voice support
* Implement save system for custom characters (currently nothing you setup your custom character with saves on exit)
* Maybe rewrite parts of the code, since by the end I got lazy and started using On hooks instead of IL

## Long term goals (aka never ever)
* Custom models as outfits
* More than 4 outfits
* Custom animations

# I am a mod maker, why does my character have skates angled at wrong position? 
I've made a wiki page covering on how to fix it. You can find it ![here](https://github.com/viliger2/BRC_CharacterAPI/wiki/Why-is-are-my-skates-at-the-wrong-angle%3F).
