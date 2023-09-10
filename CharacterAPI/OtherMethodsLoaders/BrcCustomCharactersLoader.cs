using BrcCustomCharactersLib;
using CharacterAPI.Hooks;
using Reptile;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CharacterAPI.OtherMethodsLoaders
{
    public static class BrcCustomCharactersLoader
    {
        public static void LoadBrcCCharacters(string pluginPath)
        {
            Directory.CreateDirectory(pluginPath);

            foreach (string filePath in Directory.GetFiles(pluginPath))
            {
                AssetBundle bundle = AssetBundle.LoadFromFile(filePath);
                if(bundle != null)
                {
                    CharacterDefinition definition = GetCharacterDefinition(bundle);
                    if(definition)
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

        private static CharacterDefinition GetCharacterDefinition(AssetBundle bundle)
        {
            // finding our CharacterDefinition
            GameObject[] objects = bundle.LoadAllAssets<GameObject>();
            foreach (GameObject obj in objects)
            {
                CharacterDefinition characterDefinition = obj.GetComponent<CharacterDefinition>();
                if (characterDefinition)
                {
                    return characterDefinition;
                }
            }

            return null;
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
