using Mono.Cecil.Cil;
using MonoMod.Cil;
using Reptile;
using System;
using UnityEngine;
using static CharacterAPI.CharacterAPI;

namespace CharacterAPI.Hooks
{
    public class PlayerHooks
    {
        public static void InitHooks()
        {
            On.Reptile.Player.PlayVoice += Player_PlayVoice;
        }

        // better off redoing this with IL hook, but for now leave it as is
        private static void Player_PlayVoice(On.Reptile.Player.orig_PlayVoice orig, Player self, AudioClipID audioClipID, VoicePriority voicePriority, bool fromPlayer)
        {
            Characters character = self.character;
            if (Enum.IsDefined(typeof(Characters), character))
            {
                orig(self, audioClipID, voicePriority, fromPlayer);
            }
            else
            {
                var moddedCharacter = CharacterAPI.GetModdedCharacter(character);
                if (fromPlayer)
                {
                    self.audioManager.PlayVoice(ref self.currentVoicePriority, moddedCharacter.tempAudioCharacter, audioClipID, self.playerGameplayVoicesAudioSource, voicePriority);
                }
                else
                {
                    self.audioManager.PlaySfxGameplay(self.audioManager.characterToVoiceCollection[(int)moddedCharacter.tempAudioCharacter], audioClipID);
                }
            }
        }
    }
}
