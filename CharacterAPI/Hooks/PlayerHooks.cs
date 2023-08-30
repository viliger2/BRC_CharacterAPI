using CharacterAPI.ExtensionMethods;
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
            IL.Reptile.Player.SetCharacter += Player_SetCharacter;
            IL.Reptile.Player.SetOutfit += Player_SetOutfit;
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
                CharacterSelectExtensions.SelectableCharacterWithMods characterWithMods = CharacterSelectExtensions.GetCharacterWithMods(character);
                if (fromPlayer)
                {
                    self.audioManager.PlayVoice(ref self.currentVoicePriority, characterWithMods.moddedCharacter.tempAudioCharacter, audioClipID, self.playerGameplayVoicesAudioSource, voicePriority);
                }
                else
                {
                    self.audioManager.PlaySfxGameplay(self.audioManager.characterToVoiceCollection[(int)characterWithMods.moddedCharacter.tempAudioCharacter], audioClipID);
                }
            }
        }

        private static void Player_SetOutfit(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchLdarg(out _),
                x => x.MatchLdfld<Reptile.Player>("characterVisual")))
            {
                c.Index++;
                c.RemoveRange(9);
                c.Emit(OpCodes.Ldarg_1);
                c.EmitDelegate<Action<Reptile.Player, int>>((p, setOutfit) =>
                {
                    Characters character = p.character;
                    CharacterSelectExtensions.SelectableCharacterWithMods characterWithMods = CharacterSelectExtensions.GetCharacterWithMods(character);
                    if (characterWithMods.IsModdedCharacter)
                    {
                        p.characterVisual.GetComponentInChildren<SkinnedMeshRenderer>().material = p.characterConstructor.CreateCharacterMaterial(characterWithMods, setOutfit);
                    }
                    else
                    {
                        p.characterVisual.GetComponentInChildren<SkinnedMeshRenderer>().material = p.characterConstructor.CreateCharacterMaterial(character, setOutfit);
                    }
                });
            }
            else
            {
                logger.LogError("Player::SetOutfit hook failed.");
            }
        }

        private static void Player_SetCharacter(MonoMod.Cil.ILContext il)
        {
            ILCursor c = new ILCursor(il);
            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchLdarg(out _),
                x => x.MatchLdarg(out _),
                x => x.MatchLdfld<Reptile.Player>("characterConstructor"),
                x => x.MatchLdarg(out _),
                x => x.MatchLdfld<Reptile.Player>("character")))
            {
                c.Index++;
                c.RemoveRange(16);
                //c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Action<Reptile.Player>>((p) =>
                {
                    Characters character = p.character;
                    logger.LogMessage("SetCharacter - current character: " + character.ToString());
                    //CharacterSelectExtensions.CharacterWithMods characterWithMods = CharacterSelectExtensions.selectableCharactesWithMods.Find(x => x.characterEnum == character);
                    if (Enum.IsDefined(typeof(Characters), character))
                    {
                        p.characterVisual = p.characterConstructor.CreateNewCharacterVisual(character, p.animatorController, !p.isAI, p.motor.groundDetection.groundLimit);
                    }
                    else
                    {
                        CharacterSelectExtensions.SelectableCharacterWithMods characterWithMods = CharacterSelectExtensions.GetCharacterWithMods(character);
                        p.characterVisual = p.characterConstructor.CreateNewCharacterVisual(characterWithMods, p.animatorController, !p.isAI, p.motor.groundDetection.groundLimit);
                    }
                });
            }
            else
            {
                logger.LogError("Player::SetCharacter hook failed.");
            }
        }
    }
}
