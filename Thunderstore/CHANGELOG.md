<details>
<summary>0.9.5 </summary>

* Added BepinIncompatability attribute for CrewBoom and BombRushMP/All City Network.
  * _This basically means mod would not load at all if it detects either of these two mods. Actually CrewBoom already had incompat implementation with CharacterAPI, but since selecting custom CharacterAPI characters with BombRushMP/All City Network outright crashes the game I was asked to just make the mods incompatible. Don't expect any fixes, I've moved on from modding BRC._

</details>
<details>
<summary>0.9.4 </summary>

* Fixed an issue with game crashing on exit while trying to save vanilla save file.
	* _Didn't have that many reports on this one, but decided to "fix" it anyway. And by "fixing" I mean that you might encounter a small stutter whenever modded characters are saved (changing character, outfit and ride). I'll look into fixing stutter in the future._
* Made ModdedCharacter class public.
	* _This is for mod support, if someone wants to get information whether modded character exists or not. Done mainly to support SlopCrew, but obviously it is now open to everyone._

</details>

<details>
<summary>0.9.3 </summary>

* Fixed an issue with creating new save file.

</details>

<details>
<summary>0.9.2 </summary>

* Moved save location from config folder to CharacterAPI folder under Bepin
	* _As it turns out r2modman empties entire mod folder on each update. While it doesn't matter for saves, it matters for characters created with Loaders, so I decided to move everything related to this mod into its own folder. For CharacterAPI you don't have to do anything, save will be moved automatically. For CharacterAPI BRCCCLoader back up your characters before updating via r2modman, if you update manually they will be moved to new location._
* Added a check and a message if prefab with silent sound is not present or damaged.

</details>

<details>
<summary>0.9.1 </summary>

* Removed BRCCustomCharacters (BRCCC) support and moved it to its own plugin.
	* _I separated loading of BRCCC into its own plugin loader which can be found [here](https://thunderstore.io/c/bomb-rush-cyberfunk/p/viliger/CharacterAPI_BRCCCLoader). I did it because of two reasons. First, implementing loader directly into CharacterAPI transforms it from library into a mod, I want to keep it as a library so if someone wants to either take over from me, implement their own solution to loading characters or make their own loader, we would be 100% sure that the library itself won't do anything. Second, BRCCC is licensed under GPL-3 and it prevents me from adding necessary dependencies to this mod (that's why we needed BRCCC in the first place with 0.9.0). Separate loader solves both issues. If you used BRCCC characters with 0.9.0 you don't have to do anything but to download a new loader, it will grab everything automatically._

</details>

<details>
<summary>0.9.0 </summary>

* Implemented BRCCustomCharacters (BRCCC) support. 
	* Support is done in two modes - loading asset bundles created for BRCCC or loading them directly from BRCCC, controlled by options "Load BRCCustomCharacters" and "Load BRCCustomCharacters from Plugin" respectively. 
	* **You will need BRCCustomCharacters for this feature to work.**
	* First mode loads asset bundles directly, you have to put them inside "BrcCustomCharacters" folder.
	* Second mode loads characters directly from BRCCC plugin, HOWEVER, and this is important, **replacements will still be in place.** Let's say you have a character that replaces Red, his voice and his personal graffiti and then decide to enable this option. Then Red will be replaced by new character, his voice and graffiti also will be replaced AND you will get a new character separate from Red's replacement. In essence, you will get two characters. This mode is here mostly for comparability and maybe SlopCrew. 
	* First mode is enabled by default, second is disabled. You can have both on and off at the same time.
	* This is not tested with SlopCrew, but what most likely will happen is that your "new" character will show up as Red to everyone else and your "replacement" character will show up as usual if other people have the mod.
		* _Please, send all crash logs to me via discord or github, this is highly experimental._
* Added silent sound for custom characters to prevent crashing.
	* _If character had custom sounds but one of seven needed arrays was empty for whatever reason, whenever the game would try to play a sound from that array it would crash. I decided to just add silent sounds to that array so crashes would stop. It is kinda hacky, but it is substantially easier than fixing it in code._
* Added custom shader support. Set usesCustomShader to true and your character's shader will not be replaced with game's shader.
* Implemented BRCCC skates positioning support.
	* _If you don't like fixing bones - well, here we are. It was mostly done for comparability with BRCCC characters, but can be used for plugin characters all the same. Just add skateOffsetL and skateOffsetR transforms to footL and footR respectively and adjust the position\angle. Then the mod will automatically find them and if present will use their position for skates._

</details>

<details>
<summary>0.8.0 </summary>

* Minor performance improvements.
* Fixed issue where loaded modded character didn't use correct move style on character select screen.
* Added option to allow modded characters to blink. Set canBlink to true in your ModdedCharacterConstructor to use it.
	* _This is not a magic solution where you set it and it will just work. You will need to setup Shape keys (or blend shapes as Unity calls them) in Blender or your modelling software of choice. Base game characters that can blink use two meshes with open and closed eyes and then scale them from 0 to 100 and in reverse for blinking. This feature is not tested, but should be fairly straightforward to implement if you have experience with shape keys._
* Removed save warning from mod page.
	* _It has been almost two weeks since initial release and I haven't got a single report of broken save file. So I am going to assume that saving works as intended and there is no risk of breaking your save file._
</details>

<details>
<summary>0.7.1 </summary>

* Fixed an issue where all AIs (and by extension SlowCrew players) were loaded as last played custom character.
</details>

<details>
<summary>0.7.0 </summary>

* Implemented save system.
	* The game now remembers last custom character played, all custom characters loadouts (outfit, ride, its skin), loads applied custom personal graffiti.
	* Saves can be found inside your Bepin config folder, inside CharacterAPISaves folder.
	* If you disable last modded character that you played as, you will be loaded as Red, while its graffiti will be replaced with Red's.
	* Completely disabling CharacterAPI will result in applied custom personal graffiti loaded as gray squares. This is something I might fix in the future, since currently custom graffiti are saved into main save, however, unlike with characters, game safely handles missing graffiti and loads replacement in a form of gray square.
	* Modded character saves are saved per save slot.
	* Characters are kept in save independant of if they are loaded or not as long as they have been saved, special config option is added to clear save file off them if you want, since their saves do take RAM.
	* Characters are indentified by their hash, that is made from combination of their name, outfit names, default outfit and ride.
		* _With this release all "major" planned features are complete. Once I get enough reports that things are stable I will do another code pass and release it as 1.0, after that updates will slow down considerably._
</details>

<details>
<summary>0.6.0 </summary>

* Implemented custom voice suppport.
	* _It is not as straightforward as I wish it would be, but the guide covers it and example plugin has everything you need._
</details>
<details>
<summary>0.5.2 </summary>

* Fixed currently played as modded character being selectable on dance pad.
	* _This comes with pretty decent refactor job, so while I tested everything that it afffects, you still might encounter a new crash. Please, report all crashes on github or in my DMs on discord._
* SlowCrew is not longer incompatible.
	* _SlopCrew now assignes modded characters as Red for everyone who don't have the mods. While you won't see other people using mods, at least you can now yourself play as modded character and be allowed to join AND not crash the server._
</details>
<details>
<summary>0.5.1 </summary>

* Fixed character list doubling every time you change character via dance pad.
* Made SlopCrew incompatible.
	* _Sadly, playing on SlopCrew with new characters added via CharacterAPI made everyone who don't have the mod crash their game. While SlopCrew added autokick for those players, I decided, for now, to just completely disable ChracterAPI if SlowCrew is present among mods. Once me and NotNite come up with a solution we both agree on, mod will continue disabling itself in presence of SlowCrew._
* Fixed max outfits check for new modded character being incorrect.
</details>
<details>
<summary>0.5.01 </summary>

* Readme fix because I love markdown.
</details>
<details>
<summary>0.5.0 </summary>

* Initial release
</details>