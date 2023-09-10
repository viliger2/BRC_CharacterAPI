using BepInEx;
using BepInEx.Configuration;
using CharacterAPI.CompatibilityPlugins;
using CharacterAPI.Hooks;
using Reptile;
using System.IO;
using UnityEngine;

namespace CharacterAPI
{
    [BepInPlugin("com.Viliger.CharacterAPI", "CharacterAPI", "0.9.0")]
    [BepInDependency("BrcCustomCharacters", BepInDependency.DependencyFlags.SoftDependency)]
    public class CharacterAPI : BaseUnityPlugin
    {
        public static BepInEx.Logging.ManualLogSource logger;

        public static string SavePath;

        public static ConfigEntry<bool> PerformSaveCleanUp;

        public static ConfigEntry<bool> LoadBRCCCharacters;

        public static ConfigEntry<bool> LoadBRCCPlugin;

        public void Awake()
        {
            AssetBundle bundle = AssetBundle.LoadFromFile(Path.Combine(System.IO.Path.GetDirectoryName(Info.Location), "Assets", "silence"));
            CoreHooks.silence = bundle.LoadAsset<AudioClip>("silence");

            SavePath = Paths.ConfigPath;

            logger = Logger;

            PerformSaveCleanUp = Config.Bind<bool>("Saving", "Save Clean Up", false, "Performs save clean up on each start. It removes saves for characters that are not curently enabled. Can be useful if save grows out of proportions with hundreds of characters or dozens save slots.");
            LoadBRCCCharacters = Config.Bind<bool>("BRCCustomCharacters Loader", "Load BRCCustomCharacters", true, "Loads characters made for BRCCustomCharacters as their own characters. It loads from \"BRCCustomCharacters\" folder.");
            LoadBRCCPlugin = Config.Bind<bool>("BRCCustomCharacters Loader", "Load BRCCustomCharacters from Plugin", false, "Loads characters FROM BRCCustomCharacters as their own characters. This is different from the option above. It means that any supported character loaded by BRCCustomCharacters will also recieve an independent copy. This, however, will not disable any replacements (voice, character, etc).");

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

            if (LoadBRCCCharacters.Value)
            {
                OtherMethodsLoaders.BrcCustomCharactersLoader.LoadBrcCCharacters(Path.Combine(System.IO.Path.GetDirectoryName(Info.Location), "BrcCustomCharacters"));
                //OtherMethodsLoaders.BrcCustomCharactersLoader.LoadBrcCCharacters(Path.Combine(Paths.PluginPath, "brcCustomCharacters", "CharAssets"));
            }

            if (BrcCustomCharactersCompat.enabled && LoadBRCCPlugin.Value)
            {
                BrcCustomCharactersCompat.LoadBrcCustomCharacters();
            }

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
