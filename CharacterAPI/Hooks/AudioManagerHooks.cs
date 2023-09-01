using Reptile;
using System;

namespace CharacterAPI.Hooks
{
    public class AudioManagerHooks
    {
        // TODO: write proper audio management, for now just replace character with whatever people want
        public static void InitHooks()
        {
            On.Reptile.AudioManager.GetCharacterVoiceSfxCollection += AudioManager_GetCharacterVoiceSfxCollection;
            On.Reptile.AudioManager.PlayVoice_Characters_AudioClipID += AudioManager_PlayVoice_Characters_AudioClipID;
            On.Reptile.AudioManager.PlayVoice_refVoicePriority_Characters_AudioClipID_AudioSource_VoicePriority += AudioManager_PlayVoice_refVoicePriority_Characters_AudioClipID_AudioSource_VoicePriority;
        }

        private static void AudioManager_PlayVoice_refVoicePriority_Characters_AudioClipID_AudioSource_VoicePriority(On.Reptile.AudioManager.orig_PlayVoice_refVoicePriority_Characters_AudioClipID_AudioSource_VoicePriority orig, AudioManager self, ref VoicePriority currentPriority, Characters character, AudioClipID audioClipID, UnityEngine.AudioSource audioSource, VoicePriority playbackPriority)
        {
            if (Enum.IsDefined(typeof(Characters), character))
            {
                orig(self, ref currentPriority, character, audioClipID, audioSource, playbackPriority);
            }
            else
            {
                var moddedCharacter = CharacterAPI.GetModdedCharacter(character);
                orig(self, ref currentPriority, moddedCharacter.tempAudioCharacter, audioClipID, audioSource, playbackPriority);
            }
        }

        private static VoicePriority AudioManager_PlayVoice_Characters_AudioClipID(On.Reptile.AudioManager.orig_PlayVoice_Characters_AudioClipID orig, AudioManager self, Characters character, AudioClipID audioClipID)
        {
            if (Enum.IsDefined(typeof(Characters), character))
            {
                return orig(self, character, audioClipID);
            }
            var moddedCharacter = CharacterAPI.GetModdedCharacter(character);
            return orig(self, moddedCharacter.tempAudioCharacter, audioClipID);
        }

        private static Reptile.SfxCollectionID AudioManager_GetCharacterVoiceSfxCollection(On.Reptile.AudioManager.orig_GetCharacterVoiceSfxCollection orig, Reptile.AudioManager self, Reptile.Characters character)
        {
            if (Enum.IsDefined(typeof(Characters), character))
            {
                return orig(self, character);
            }
            var moddedCharacter = CharacterAPI.GetModdedCharacter(character);
            return orig(self, moddedCharacter.tempAudioCharacter);
        }
    }
}
