using Mono.Cecil.Cil;
using MonoMod.Cil;
using Reptile;
using System;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

namespace CharacterAPI.Hooks
{
    public class PlayerHooks
    {
        public static void InitHooks()
        {
            IL.Reptile.Player.PlayVoice += Player_PlayVoice;
            On.Reptile.Player.SetOutfit += Player_SetOutfit;
            IL.Reptile.Player.Init += Player_Init;
        }

        private static void Player_Init(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            if(c.TryGotoNext(MoveType.Before, 
                x => x.MatchLdarg(out _),
                x => x.MatchLdarg(out _),
                x => x.MatchLdarg(out _),
                x => x.MatchCallOrCallvirt<Reptile.Player>("SetCharacter")))
            {
                c.Index += 3;
                c.Remove();
                c.EmitDelegate<Action<Reptile.Player, Reptile.Characters, int>>((player, character, outfit) =>
                {
                    if (!player.isAI)
                    {
                        int saveSlotId = Core.Instance.saveManager.CurrentSaveSlot.saveSlotId;
                        var slot = ModdedCharacterProgress.GetModdedSaveSlot(saveSlotId);
                        if (slot != null)
                        {
                            var moddedCharacter = CharacterAPI.GetModdedCharacter(slot.lastPlayedCharacter);
                            if (moddedCharacter != null)
                            {
                                CharacterAPI.logger.LogMessage($"Modded character with hash {slot.lastPlayedCharacter} is {moddedCharacter.Name}, loading as modded chacater.");
                                var moddedCharacteProgress = slot.GetModdedCharacterProgress(slot.lastPlayedCharacter);
                                player.SetCharacter(moddedCharacter.characterEnum, moddedCharacteProgress.characterProgress.outfit);
                                return;
                            }
                            else
                            {
                                CharacterAPI.logger.LogMessage($"Modded character with hash {slot.lastPlayedCharacter} could not be found, loading as {character}.");
                            }
                        }
                    }

                    player.SetCharacter(character, outfit);
                });
            } else
            {
                CharacterAPI.logger.LogError("Player::Init hook failed.");
            }

        }

        private static void Player_SetOutfit(On.Reptile.Player.orig_SetOutfit orig, Player self, int setOutfit)
        {
            orig(self, setOutfit);

            // SAVE_MODDED_CHARACTERS
            ModdedCharacterProgress.SaveAsync();
        }

        private static void Player_PlayVoice(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            if(c.TryGotoNext(MoveType.Before,
                x => x.MatchLdarg(out _),
                x => x.MatchLdfld<Reptile.Player>("audioManager"),
                x => x.MatchLdarg(out _),
                x => x.MatchLdfld<Reptile.Player>("audioManager"),
                x => x.MatchLdfld<Reptile.AudioManager>("characterToVoiceCollection"),
                x => x.MatchLdarg(out _),
                x => x.MatchLdfld<Reptile.Player>("character")))
            {
                c.Index++;
                c.RemoveRange(10);
                c.Emit(OpCodes.Ldarg_1);
                c.EmitDelegate<Action<Reptile.Player, Reptile.AudioClipID>>((p, aci) =>
                {
                    if (Enum.IsDefined(typeof(Characters), p.character))
                    {
                        p.audioManager.PlaySfxGameplay(p.audioManager.characterToVoiceCollection[(int)p.character], aci);
                    }
                    else
                    {
                        CharacterAPI.ModdedCharacter moddedCharacter = CharacterAPI.GetModdedCharacter(p.character);
                        if (moddedCharacter != null)
                        {
                            if (moddedCharacter.voiceId != SfxCollectionID.NONE)
                            {
                                p.audioManager.PlaySfxGameplay(moddedCharacter.voiceId, aci);
                            }
                            else
                            {
                                p.audioManager.PlaySfxGameplay(p.audioManager.characterToVoiceCollection[(int)moddedCharacter.characterVoiceBase], aci);
                            }
                        }
                        else
                        {
                            CharacterAPI.logger.LogWarning($"Player::PlayVoice couldn't find modded character {p.character}, replacing with {Characters.metalHead}.");
                            p.audioManager.PlaySfxGameplay(p.audioManager.characterToVoiceCollection[(int)Characters.metalHead], aci);
                        }
                    }
                });
            } else
            {
                CharacterAPI.logger.LogError("Player::PlayVoice hook failed.");
            }
        }
    }
}
