using BepInEx;
using MonoMod.Cil;
using UnityEngine.TextCore.Text;
using UnityEngine;
using Reptile;
using CharacterAPI;

namespace ExamplePllugin
{
    [BepInPlugin(ModGuid, ModName, ModVer)]
    [BepInDependency("com.Viliger.CharacterAPI")] // this is hard dependency on CharacterAPI, without it the mod will not work
    // Remember to rename your class
    public class CharacterAPIExamplePlugin : BaseUnityPlugin
    {
        // This is your mod's guid, remember to change it, otherwise it will not load if mod with the same name is already present
        public const string ModGuid = "com.Viliger.CharacterAPI_Beat";
        // This is your mod's name, you can do whatever with it
        public const string ModName = "CharacterAPI_Beat";
        // This is your mod's version
        public const string ModVer = "0.9.0";

        public void Awake()
        {
            // This will load our asset bundle with name "beat"
            AssetBundle bundle = AssetBundle.LoadFromFile(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Info.Location), "beat"));

            // Here we will start making our new character
            ModdedCharacterConstructor beat = new ModdedCharacterConstructor();
            // In-game character name, will show up on selection on dance pad
            beat.characterName = "Beat";
            // Character's prefab, main model with all transforms
            beat.characterPrefab = bundle.LoadAsset<GameObject>("beat_no_blades");
            // Character's default outfit, since save system is not implemented yet,
            // this will determine what outfit character will use on start-up
            // can be from 0 to 3
            beat.defaultOutfit = 0;
            // Character's default movement option, values are BMX, SKATEBOARD and INLINE
            // ON_FOOT is used when character puts away the ride, SPECIAL is for story things and MAX is for math stuff
            // outside of first three, other values should not be used
            beat.defaultMoveStyle = MoveStyle.INLINE;
            // Character's audio base, since custom audio is not yet implemented, we use existing character as base
            // for our sounds, can take pretty much any value outside of MAX and NONE
            beat.tempAudioBase = Characters.legendFace;
            // Character's freestyle, it determines they do once you leave the dance pad and they just stand around dancing
            beat.freestyleType = ModdedCharacterConstructor.FreestyleType.freestyle16;
            // Character's bounce, what they do when they are selectable on dance pad
            beat.bounceType = ModdedCharacterConstructor.BounceType.softbounce3;
            // Character's outfits, you need at least one. You can also skip filling the names (second parameter) if
            // you want to use in-game strings (spring, summer, autumn, winter)
            beat.AddOutfit(bundle.LoadAsset<Material>("beatDefault.mat"), "Jet Set");
            beat.AddOutfit(bundle.LoadAsset<Material>("beatFuture.mat"), "Future");
            beat.AddOutfit(bundle.LoadAsset<Material>("beatCombo.mat"), "Combo");
            beat.AddOutfit(bundle.LoadAsset<Material>("beatCorn.mat"), "Corn");
            // Character's graffiti, it is entirely optional, but if you want,
            // you need to create a material with your image
            // image should be square
            beat.AddPersonalGraffiti("Beat", "Beat", bundle.LoadAsset<Material>("beatGraffiti.mat"), bundle.LoadAsset<Texture>("graffitiTexture.png"));
            // If you don't want to make custom graffiti (since there are issues with them for now)
            // you can comment the line above and uncommend this one and select character that will be
            // base for our personal graffiti
            //beat.personalGraffitiBase = Characters.blockGuy;

            // This line finalized and add character to the game
            // if you character is not in the game, remember to check the log for errors
            beat.CreateModdedCharacter();
        }
    }
}
