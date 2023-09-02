using Mono.Cecil.Cil;
using MonoMod.Cil;
using Reptile;
using System;

namespace CharacterAPI.Hooks
{
    public class PlayerHooks
    {
        public static void InitHooks()
        {
            IL.Reptile.Player.PlayVoice += Player_PlayVoice;
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
                    if(Enum.IsDefined(typeof(Characters), p.character))
                    {
                        p.audioManager.PlaySfxGameplay(p.audioManager.characterToVoiceCollection[(int)p.character], aci);
                    }

                    CharacterAPI.ModdedCharacter moddedCharacter = CharacterAPI.GetModdedCharacter(p.character);
                    if(moddedCharacter.voiceId != SfxCollectionID.NONE)
                    {
                        p.audioManager.PlaySfxGameplay(moddedCharacter.voiceId, aci);                    }
                    else
                    {
                        p.audioManager.PlaySfxGameplay(p.audioManager.characterToVoiceCollection[(int)moddedCharacter.characterVoiceBase], aci);
                    }
                });
            } else
            {
                CharacterAPI.logger.LogError("Player::PlayVoice hook failed.");
            }
        }
    }
}
