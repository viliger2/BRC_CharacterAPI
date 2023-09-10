using BrcCustomCharactersLib;
using CharacterAPI.Hooks;
using Reptile;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

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

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void LoadBrcCCharacters(string pluginPath)
        {
            Directory.CreateDirectory(pluginPath);

            foreach (string filePath in Directory.GetFiles(pluginPath))
            {
                AssetBundle bundle = AssetBundle.LoadFromFile(filePath);
                if (bundle != null)
                {
                    CharacterDefinition definition = null;
                    GameObject[] objects = bundle.LoadAllAssets<GameObject>();
                    foreach (GameObject obj in objects)
                    {
                        definition = obj.GetComponent<CharacterDefinition>();
                        if (definition)
                        {
                            break;
                        }
                    }

                    //CharacterDefinition definition = GetCharacterDefinition(bundle);
                    if (definition)
                    {
                        using (var moddedCharacter = new ModdedCharacterConstructor())
                        {
                            moddedCharacter.characterName = definition.CharacterName;
                            moddedCharacter.characterPrefab = definition.gameObject;

                            foreach (Material outfit in definition.Outfits)
                            {
                                moddedCharacter.AddOutfit(outfit);
                            }

                            if (definition.Graffiti)
                            {
                                moddedCharacter.AddPersonalGraffiti(definition.GraffitiName, definition.GraffitiName, definition.Graffiti, definition.Graffiti.mainTexture);
                            }

                            if (definition.HasVoicesGood())
                            {
                                LoadVoicesFromArray(moddedCharacter.audioClips, definition.VoiceDie, CoreHooks.VOICE_DIE);
                                LoadVoicesFromArray(moddedCharacter.audioClips, definition.VoiceDieFall, CoreHooks.VOICE_DIEFALL);
                                LoadVoicesFromArray(moddedCharacter.audioClips, definition.VoiceTalk, CoreHooks.VOICE_TALK);
                                LoadVoicesFromArray(moddedCharacter.audioClips, definition.VoiceBoostTrick, CoreHooks.VOICE_BOOST);
                                LoadVoicesFromArray(moddedCharacter.audioClips, definition.VoiceCombo, CoreHooks.VOICE_COMBO);
                                LoadVoicesFromArray(moddedCharacter.audioClips, definition.VoiceGetHit, CoreHooks.VOICE_GETHIT);
                                LoadVoicesFromArray(moddedCharacter.audioClips, definition.VoiceJump, CoreHooks.VOICE_JUMP);
                            }
                            else
                            {
                                moddedCharacter.tempAudioBase = (Characters)definition.CharacterToReplace;
                            }

                            moddedCharacter.canBlink = definition.CanBlink;
                            moddedCharacter.usesCustomShader = !definition.UseReptileShader;

                            moddedCharacter.CreateModdedCharacter();
                        }
                    }
                }
            }
        }

        private static bool HasVoicesGood(this CharacterDefinition definition)
        {
            return definition.VoiceDie.Length != 0 && definition.VoiceDieFall.Length != 0 && definition.VoiceTalk.Length != 0 && definition.VoiceBoostTrick.Length != 0 && definition.VoiceCombo.Length != 0 && definition.VoiceGetHit.Length != 0 && definition.VoiceJump.Length != 0;
        }

        private static void LoadVoicesFromArray(List<AudioClip> audioClips, AudioClip[] source, string type)
        {
            foreach (AudioClip clip in source)
            {
                AudioClip newClip = UnityEngine.Object.Instantiate(clip);
                newClip.name += type;
                audioClips.Add(newClip);
            }
        }
    }
}
