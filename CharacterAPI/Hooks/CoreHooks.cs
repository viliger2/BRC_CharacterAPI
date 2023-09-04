using Reptile;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Bindings;

namespace CharacterAPI.Hooks
{
    public class CoreHooks
    {
        // for sound effects
        private const string VOICE_BOOST = "_boost";
        private const string VOICE_COMBO = "_combo";
        private const string VOICE_DIE = "_die";
        private const string VOICE_DIEFALL = "_falldamage";
        private const string VOICE_GETHIT = "_gethit";
        private const string VOICE_JUMP = "_jump";
        private const string VOICE_TALK = "_talk";

        public static void InitHooks()
        {
            // Gonna assume this is essentially game's start up
            On.Reptile.Core.CreateSubSystems += Core_CreateSubSystems;
        }

        private static void Core_CreateSubSystems(On.Reptile.Core.orig_CreateSubSystems orig, Reptile.Core self)
        {
            orig(self);
            foreach (CharacterAPI.ModdedCharacter moddedCharacter in CharacterAPI.ModdedCharacters)
            {
                AddModdedCharacterSfx(self, moddedCharacter);
            }
        }

        private static void AddModdedCharacterSfx(Core self, CharacterAPI.ModdedCharacter moddedCharacter)
        {
            if (moddedCharacter.voiceId != Reptile.SfxCollectionID.NONE)
            {
                var sfxCollection = ScriptableObject.CreateInstance<Reptile.SfxCollection>();
                sfxCollection.name = $"Voice{moddedCharacter.Name}Collection";

                sfxCollection.audioClipContainers = new SfxCollection.RandomAudioClipContainer[7];

                sfxCollection.audioClipContainers[0] = CreateRandomAudioClipContainer(AudioClipID.VoiceBoostTrick, moddedCharacter.audioClips, VOICE_BOOST);
                sfxCollection.audioClipContainers[1] = CreateRandomAudioClipContainer(AudioClipID.VoiceCombo, moddedCharacter.audioClips, VOICE_COMBO);
                sfxCollection.audioClipContainers[2] = CreateRandomAudioClipContainer(AudioClipID.VoiceDie, moddedCharacter.audioClips, VOICE_DIE);
                sfxCollection.audioClipContainers[3] = CreateRandomAudioClipContainer(AudioClipID.VoiceDieFall, moddedCharacter.audioClips, VOICE_DIEFALL);
                sfxCollection.audioClipContainers[4] = CreateRandomAudioClipContainer(AudioClipID.VoiceGetHit, moddedCharacter.audioClips, VOICE_GETHIT);
                sfxCollection.audioClipContainers[5] = CreateRandomAudioClipContainer(AudioClipID.VoiceJump, moddedCharacter.audioClips, VOICE_JUMP);
                sfxCollection.audioClipContainers[6] = CreateRandomAudioClipContainer(AudioClipID.VoiceTalk, moddedCharacter.audioClips, VOICE_TALK);

                self.sfxLibrary.sfxCollectionIDDictionary.Add(moddedCharacter.voiceId, sfxCollection);
            }
        }

        private static SfxCollection.RandomAudioClipContainer CreateRandomAudioClipContainer(AudioClipID audioClipID, List<AudioClip> audioClipSourceList, string nameCondition)
        {
            return new SfxCollection.RandomAudioClipContainer
            {
                clipID = audioClipID,
                lastRandomClip = 0,
                clips = audioClipSourceList.FindAll(x => x.name.Contains(nameCondition)).ToArray()
            };
        }
    }
}
