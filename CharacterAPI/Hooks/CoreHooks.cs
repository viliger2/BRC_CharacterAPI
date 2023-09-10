using Reptile;
using System.Collections.Generic;
using UnityEngine;

namespace CharacterAPI.Hooks
{
    public class CoreHooks
    {
        // for sound effects
        internal const string VOICE_BOOST = "_boost";
        internal const string VOICE_COMBO = "_combo";
        internal const string VOICE_DIE = "_die";
        internal const string VOICE_DIEFALL = "_falldamage";
        internal const string VOICE_GETHIT = "_gethit";
        internal const string VOICE_JUMP = "_jump";
        internal const string VOICE_TALK = "_talk";

        internal static AudioClip silence = null;

        public static void InitHooks()
        {
            // Gonna assume this is essentially game's start up
            On.Reptile.Core.CreateSubSystems += Core_CreateSubSystems;
        }

        private static void Core_CreateSubSystems(On.Reptile.Core.orig_CreateSubSystems orig, Reptile.Core self)
        {
            orig(self);
            foreach (ModdedCharacter moddedCharacter in ModdedCharacter.ModdedCharacters)
            {
                AddModdedCharacterSfx(self, moddedCharacter);
            }
            if (CharacterAPI.PerformSaveCleanUp.Value)
            {
                ModdedCharacterProgress.PerformSaveCleanUp();
            }
            // Maybe in the future, but for now we don't need it
            //ModdedCharacterProgress.PerformSaveValidation();
        }

        private static void AddModdedCharacterSfx(Core self, ModdedCharacter moddedCharacter)
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
            var clips = audioClipSourceList.FindAll(x => x.name.Contains(nameCondition));
            if(clips.Count== 0)
            {
                clips.Add(silence);
            }

            return new SfxCollection.RandomAudioClipContainer
            {
                clipID = audioClipID,
                lastRandomClip = 0,
                clips = clips.ToArray()
            };
        }
    }
}
