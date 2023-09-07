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