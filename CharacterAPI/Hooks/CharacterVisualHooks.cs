using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using CharacterAPI;
using Reptile;
using UnityEngine.TextCore.Text;

namespace CharacterAPI.Hooks
{
    public class CharacterVisualHooks
    {
        public static void InitHooks()
        {
            On.Reptile.CharacterVisual.GetCharacterFreestyleAnim += CharacterVisual_GetCharacterFreestyleAnim;
            On.Reptile.CharacterVisual.GetCharacterBounceAnim += CharacterVisual_GetCharacterBounceAnim;
        }

        private static int CharacterVisual_GetCharacterBounceAnim(On.Reptile.CharacterVisual.orig_GetCharacterBounceAnim orig, Reptile.Characters c)
        {
            if (Enum.IsDefined(typeof(Reptile.Characters), c))
            {
                return orig(c);
            }

            CharacterAPI.ModdedCharacter moddedCharacter = CharacterAPI.GetModdedCharacter(c);
            if (moddedCharacter != null)
            {
                return moddedCharacter.bounceHash;
            } 

            CharacterAPI.logger.LogWarning($"CharacterVisual::GetCharacterBounceAnim failed to find modded character {c}, replacing it with {Characters.metalHead}.");
            return orig(Characters.metalHead);
        }

        private static int CharacterVisual_GetCharacterFreestyleAnim(On.Reptile.CharacterVisual.orig_GetCharacterFreestyleAnim orig, Reptile.Characters c)
        {
            if(Enum.IsDefined(typeof(Reptile.Characters), c))
            {
                return orig(c);
            }

            CharacterAPI.ModdedCharacter moddedCharacter = CharacterAPI.GetModdedCharacter(c);
            if (moddedCharacter != null)
            {
                return moddedCharacter.freestyleHash;
            }

            CharacterAPI.logger.LogWarning($"CharacterVisual::GetCharacterFreestyleAnim failed to find modded character {c}, replacing it with {Characters.metalHead}.");
            return orig(c);
        }
    }
}
