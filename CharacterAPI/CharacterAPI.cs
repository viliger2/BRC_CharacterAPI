using BepInEx;
using BepInEx.Configuration;
using CharacterAPI.Hooks;
using Reptile;
using System.Collections.Generic;
using UnityEngine;

namespace CharacterAPI
{
    [BepInPlugin("com.Viliger.CharacterAPI", "CharacterAPI", "0.8.0")]
    public class CharacterAPI : BaseUnityPlugin
    {
        public static BepInEx.Logging.ManualLogSource logger;

        public static string SavePath;

        public static ConfigEntry<bool> PerformSaveCleanUp;

        public void Awake()
        {
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
