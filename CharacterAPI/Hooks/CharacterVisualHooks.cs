using CharacterAPI.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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

            CharacterSelectExtensions.SelectableCharacterWithMods selectableCharacterWithMods = CharacterSelectExtensions.GetCharacterWithMods(c);
            return selectableCharacterWithMods.moddedCharacter.bounceHash;
        }

        private static int CharacterVisual_GetCharacterFreestyleAnim(On.Reptile.CharacterVisual.orig_GetCharacterFreestyleAnim orig, Reptile.Characters c)
        {
            if(Enum.IsDefined(typeof(Reptile.Characters), c))
            {
                return orig(c);
            }

            CharacterSelectExtensions.SelectableCharacterWithMods selectableCharacterWithMods = CharacterSelectExtensions.GetCharacterWithMods(c);
            return selectableCharacterWithMods.moddedCharacter.freestyleHash;
        }
    }
}
