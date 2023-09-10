using BrcCustomCharactersLib;
using CharacterAPI.Hooks;
using Reptile;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using static CharacterAPI.OtherMethodsLoaders.BrcCustomCharactersLoader;

namespace CharacterAPI.CompatibilityPlugins
{
    public static class BrcCustomCharactersCompat
    {
        private static bool? _enabled;

        public static bool enabled
        {
            get
            {
                if (_enabled == null)
                {
                    _enabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("BrcCustomCharacters");
                }

                return (bool)_enabled;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void LoadBrcCustomCharacters()
        {
            Type type = typeof(AssetDatabase);
            Dictionary<Guid, CharacterDefinition> characterObjects = (Dictionary<Guid, CharacterDefinition>)type.GetField("_characterObjects", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
            //Dictionary<Guid, string> _characterBundlePaths = (Dictionary<Guid, string>)type.GetField("_characterBundlePaths", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
            //Dictionary<Guid, SfxCollection> _characterSfxCollections = (Dictionary<Guid, SfxCollection>)type.GetField("_characterSfxCollections", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
            //Dictionary<Guid, GameObject> _characterVisuals = (Dictionary<Guid, GameObject>)type.GetField("_characterVisuals", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
            //Dictionary<Characters, List<Guid>> _characterReplacementIds = (Dictionary<Characters, List<Guid>>)type.GetField("_characterReplacementIds", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);

            foreach (var characterObject in characterObjects.Values)
            {
                using (var moddedCharacter = new ModdedCharacterConstructor())
                {
                    moddedCharacter.characterName = characterObject.CharacterName;
                    moddedCharacter.characterPrefab = characterObject.gameObject;

                    foreach (Material outfit in characterObject.Outfits)
                    {
                        moddedCharacter.AddOutfit(outfit);
                    }

                    if (characterObject.Graffiti)
                    {
                        moddedCharacter.AddPersonalGraffiti(characterObject.GraffitiName, characterObject.GraffitiName, characterObject.Graffiti, characterObject.Graffiti.mainTexture);
                    }

                    if (characterObject.HasVoicesGood())
                    {
                        LoadVoicesFromArray(moddedCharacter.audioClips, characterObject.VoiceDie, CoreHooks.VOICE_DIE);
                        LoadVoicesFromArray(moddedCharacter.audioClips, characterObject.VoiceDieFall, CoreHooks.VOICE_DIEFALL);
                        LoadVoicesFromArray(moddedCharacter.audioClips, characterObject.VoiceTalk, CoreHooks.VOICE_TALK);
                        LoadVoicesFromArray(moddedCharacter.audioClips, characterObject.VoiceBoostTrick, CoreHooks.VOICE_BOOST);
                        LoadVoicesFromArray(moddedCharacter.audioClips, characterObject.VoiceCombo, CoreHooks.VOICE_COMBO);
                        LoadVoicesFromArray(moddedCharacter.audioClips, characterObject.VoiceGetHit, CoreHooks.VOICE_GETHIT);
                        LoadVoicesFromArray(moddedCharacter.audioClips, characterObject.VoiceJump, CoreHooks.VOICE_JUMP);
                    }
                    else
                    {
                        moddedCharacter.tempAudioBase = (Characters)characterObject.CharacterToReplace;
                    }

                    moddedCharacter.canBlink = characterObject.CanBlink;
                    moddedCharacter.usesCustomShader = !characterObject.UseReptileShader;

                    moddedCharacter.CreateModdedCharacter();
                }
            }

            //_characterBundlePaths.Clear();
            //_characterSfxCollections.Clear();
            //_characterVisuals.Clear();
            //_characterReplacementIds.Clear();
            //characterObjects.Clear();
        }
    }
}
