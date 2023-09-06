using Mono.Cecil.Cil;
using MonoMod.Cil;
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
            IL.Reptile.AudioManager.PlayVoice_Characters_AudioClipID += AudioManager_PlayVoice_Characters_AudioClipID1;
            IL.Reptile.AudioManager.PlayVoice_refVoicePriority_Characters_AudioClipID_AudioSource_VoicePriority += AudioManager_PlayVoice_refVoicePriority_Characters_AudioClipID_AudioSource_VoicePriority1;
        }

        private static void AudioManager_PlayVoice_refVoicePriority_Characters_AudioClipID_AudioSource_VoicePriority1(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchLdarg(out _),
                x => x.MatchLdfld<Reptile.AudioManager>("characterToVoiceCollection")))
            {
                c.Index++;
                c.RemoveRange(3);
                c.Emit(OpCodes.Ldarg_2); // character
                c.EmitDelegate<Func<Reptile.AudioManager, Characters, SfxCollectionID>>((am, c) =>
                {
                    if (Enum.IsDefined(typeof(Characters), c))
                    {
                        return am.characterToVoiceCollection[(int)c];
                    }

                    CharacterAPI.ModdedCharacter moddedCharacter = CharacterAPI.GetModdedCharacter(c);
                    if (moddedCharacter != null)
                    {
                        if (moddedCharacter.voiceId != SfxCollectionID.NONE)
                        {
                            return moddedCharacter.voiceId;
                        }
                        else
                        {
                            return am.characterToVoiceCollection[(int)moddedCharacter.characterVoiceBase];
                        }
                    } else
                    {
                        CharacterAPI.logger.LogWarning($"AudioManager::AudioManager_PlayVoice_refVoicePriority_Characters_AudioClipID_AudioSource_VoicePriority failed to find voice for character {c}, replacing it with {Characters.metalHead}.");
                        return am.characterToVoiceCollection[(int)Characters.metalHead];
                    }
                });
            }
            else
            {
                CharacterAPI.logger.LogError("AudioManager::AudioManager_PlayVoice_refVoicePriority_Characters_AudioClipID_AudioSource_VoicePriority hook failed.");
            }
        }

        private static void AudioManager_PlayVoice_Characters_AudioClipID1(MonoMod.Cil.ILContext il)
        {
            ILCursor c = new ILCursor(il);
            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchLdarg(out _),
                x => x.MatchLdfld<Reptile.AudioManager>("characterToVoiceCollection")))
            {
                c.RemoveRange(4);
                c.Emit(OpCodes.Ldarg_0); // self
                c.Emit(OpCodes.Ldarg_1); // character
                c.EmitDelegate<Func<Reptile.AudioManager, Characters, SfxCollectionID>>((am, c) =>
                {
                    if (Enum.IsDefined(typeof(Characters), c))
                    {
                        return am.characterToVoiceCollection[(int)c];
                    }

                    CharacterAPI.ModdedCharacter moddedCharacter = CharacterAPI.GetModdedCharacter(c);
                    if (moddedCharacter != null)
                    {
                        if (moddedCharacter.voiceId != SfxCollectionID.NONE)
                        {
                            return moddedCharacter.voiceId;
                        }
                        else
                        {
                            return am.characterToVoiceCollection[(int)moddedCharacter.characterVoiceBase];
                        }
                    } else
                    {
                        CharacterAPI.logger.LogWarning($"AudioManager::PlayVoice_Characters_AudioClipID failed to find voice for character {c}, replacing it with {Characters.metalHead}.");
                        return am.characterToVoiceCollection[(int)Characters.metalHead];
                    }
                });
            }
            else
            {
                CharacterAPI.logger.LogError("AudioManager::PlayVoice_Characters_AudioClipID hook failed.");
            }
        }

        private static Reptile.SfxCollectionID AudioManager_GetCharacterVoiceSfxCollection(On.Reptile.AudioManager.orig_GetCharacterVoiceSfxCollection orig, Reptile.AudioManager self, Reptile.Characters character)
        {
            if (Enum.IsDefined(typeof(Characters), character))
            {
                return orig(self, character);
            }

            var moddedCharacter = CharacterAPI.GetModdedCharacter(character);
            if (moddedCharacter != null)
            {
                if (moddedCharacter.voiceId != SfxCollectionID.NONE)
                {
                    return moddedCharacter.voiceId;
                }
                else
                {
                    return orig(self, moddedCharacter.characterVoiceBase);
                }
            }

            CharacterAPI.logger.LogWarning($"AudioManager::GetCharacterVoiceSfxCollection failed to find voice for character {character}, replacing it with {Characters.metalHead}.");
            return orig(self, Characters.metalHead);
        }
    }
}
