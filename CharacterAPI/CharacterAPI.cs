using BepInEx;
using BepInEx.Configuration;
using CharacterAPI.Hooks;
using Reptile;
using System.Collections.Generic;
using UnityEngine;

namespace CharacterAPI
{
    [BepInPlugin("com.Viliger.CharacterAPI", "CharacterAPI", "0.7.0")]
    public class CharacterAPI : BaseUnityPlugin
    {
        public static BepInEx.Logging.ManualLogSource logger;

        public static string SavePath;

        public const int CHARACTER_STARTING_VALUE = (int)Characters.MAX + 1;
        public const int VOICE_STARTING_VALUE = (int)SfxCollectionID.MAX + 1;

        internal static List<ModdedCharacter> ModdedCharacters = new List<ModdedCharacter>();

        public static ConfigEntry<bool> PerformSaveCleanUp;

        public class ModdedCharacter
        {
            public string Name;
            public string[] outfitNames;
            public Material[] loadedCharacterMaterials;
            public GameObject loadedCharacterFbxAssets;
            public GameObject characterVisual;
            public int defaultOutfit;
            public MoveStyle defaultMoveStyle;
            public Characters characterVoiceBase;
            public bool usePersonalGrafitti;
            public Reptile.GraffitiArt? personalGrafitti;
            public Characters characterGraffitiBase;
            public int freestyleHash;
            public int bounceHash;
            public Characters characterEnum;
            public SfxCollectionID voiceId;
            public List<AudioClip> audioClips = new List<AudioClip>();

            public override int GetHashCode()
            {
                unchecked // Overflow is fine, just wrap
                {
                    int hash = (int)2166136261;
                    // Suitable nullity checks etc, of course :)
                    hash = (hash * 16777619) + Name.GetHashCode();
                    foreach(string outfitName in outfitNames)
                    {
                        hash = (hash * 16777619) + outfitName.GetHashCode();
                    }
                    //hash = (hash * 16777619) + outfitNames.GetHashCode();
                    hash = (hash * 16777619) + usePersonalGrafitti.GetHashCode();
                    hash = (hash * 16777619) + defaultMoveStyle.GetHashCode();
                    hash = (hash * 16777619) + defaultOutfit.GetHashCode();
                    return hash;
                }
            }

        }

        public void Awake()
        {
            // TODO:
            // Save system
            // very future TODO:
            // custom models for outfits
            // custom animations

            SavePath = Paths.ConfigPath;

            logger = Logger;

            PerformSaveCleanUp = Config.Bind<bool>("Saving", "Save Clean Up", false, "Performs save clean up on each start. It removes saves for characters that are not curently enabled. Can be useful if save grows out of proportions with hundreds of characters or dozens save slots.");

            AudioManagerHooks.InitHooks();
            CharacterConstructorHooks.InitHooks();
            CharacterSelectCharacterHooks.InitHooks();
            CharacterSelectHooks.InitHooks();
            CharacterSelectUIHooks.InitHooks();
            CharacterVisualHooks.InitHooks();
            CoreHooks.InitHooks();
            GraffitiArtInfoHooks.InitHooks();
            OutfitSwitchMenuHooks.InitHooks();
            PlayerHooks.InitHooks();
            SaveSlotDataHooks.InitHooks();
            StyleSwitchMenuHooks.InitHooks();

            ModdedCharacterProgress.LoadAsync();

            //ModdedCharacterLoader.LoadAssetBundle(Info);
        }

        public static ModdedCharacter GetModdedCharacter(Characters character)
        {
            return ModdedCharacters.Find(x => x.characterEnum == character);
        }

        public static ModdedCharacter GetModdedCharacter(int hash)
        {
            return ModdedCharacters.Find(x => x.GetHashCode() == hash);
        }

        public static void AttemptToFixShaderCharacter(CharacterLoader? loader, Material material)
        {
            if (!material) return;
            if (loader == null) return;

            Material metalHeadMaterial = loader.GetCharacterMaterial(Characters.metalHead, 0);
            material.shader = metalHeadMaterial.shader;
            //UnityEngine.Object.Destroy(metalHeadMaterial);
        }

        public static void AttemtToFixShaderGraffiti(GraffitiLoader? loader, Material material)
        {
            if (!material) return;
            if (loader == null) return;

            Material gameMaterial = loader.graffitiArtInfo.FindByCharacter(Characters.metalHead).graffitiMaterial;
            material.shader = gameMaterial.shader;
            //UnityEngine.Object.Destroy(gameMaterial);
        }
    }
}
