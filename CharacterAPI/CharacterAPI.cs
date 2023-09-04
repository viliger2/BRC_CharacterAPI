using BepInEx;
using CharacterAPI.Hooks;
using Reptile;
using System.Collections.Generic;
using UnityEngine;

namespace CharacterAPI
{
    [BepInPlugin("com.Viliger.CharacterAPI", "CharacterAPI", "0.6.0")]
    public class CharacterAPI : BaseUnityPlugin
    {
        public static BepInEx.Logging.ManualLogSource logger;

        public const int CHARACTER_STARTING_VALUE = (int)Characters.MAX + 1;
        public const int VOICE_STARTING_VALUE = (int)SfxCollectionID.MAX + 1;

        internal static List<ModdedCharacter> ModdedCharacters = new List<ModdedCharacter>();

        public struct ModdedCharacter
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
            public Reptile.GraffitiArt personalGrafitti;
            public Characters characterGraffitiBase;
            public int freestyleHash;
            public int bounceHash;
            public Characters characterEnum;
            public SfxCollectionID voiceId;
            public List<AudioClip> audioClips;
        }

        public void Awake()
        {
            // TODO:
            // Save system
            // very future TODO:
            // custom models for outfits
            // custom animations

            logger = Logger;

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

            //ModdedCharacterLoader.LoadAssetBundle(Info);
        }

        public static ModdedCharacter GetModdedCharacter(Characters character)
        {
            return ModdedCharacters.Find(x => x.characterEnum == character);
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
