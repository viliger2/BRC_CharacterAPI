using BepInEx;
using BepInEx.Configuration;
using CharacterAPI.Hooks;
using Reptile;
using System.IO;
using UnityEngine;

namespace CharacterAPI
{
    [BepInPlugin("com.Viliger.CharacterAPI", "CharacterAPI", "0.9.3")]
    public class CharacterAPI : BaseUnityPlugin
    {
        public static BepInEx.Logging.ManualLogSource logger;

        public static string DllPath;

        public static string SavePath;

        public static string NewSavePath;

        public static ConfigEntry<bool> PerformSaveCleanUp;

        public void Awake()
        {
            AssetBundle bundle = AssetBundle.LoadFromFile(Path.Combine(System.IO.Path.GetDirectoryName(Info.Location), "Assets", "silence"));
            if (!bundle)
            {
                logger.LogError("Asset bundle \"silence\" is either not present or broken. Please consider reinstalling CharacterAPI from scratch.");
                return;
            }
            CoreHooks.silence = bundle.LoadAsset<AudioClip>("silence");

            SavePath = Paths.ConfigPath;
            NewSavePath = Paths.BepInExRootPath + "\\CharacterAPI";
            DllPath = Info.Location;

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
