using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using CharacterAPI;

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
            return moddedCharacter.bounceHash;
        }

        private static int CharacterVisual_GetCharacterFreestyleAnim(On.Reptile.CharacterVisual.orig_GetCharacterFreestyleAnim orig, Reptile.Characters c)
        {
            if(Enum.IsDefined(typeof(Reptile.Characters), c))
            {
                return orig(c);
            }

            CharacterAPI.ModdedCharacter moddedCharacter = CharacterAPI.GetModdedCharacter(c);
            return moddedCharacter.freestyleHash;
        }
    }
}
