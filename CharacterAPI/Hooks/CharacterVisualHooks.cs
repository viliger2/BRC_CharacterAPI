using Reptile;
using System;
using UnityEngine;

namespace CharacterAPI.Hooks
{
    public class CharacterVisualHooks
    {
        public static void InitHooks()
        {
            On.Reptile.CharacterVisual.GetCharacterFreestyleAnim += CharacterVisual_GetCharacterFreestyleAnim;
            On.Reptile.CharacterVisual.GetCharacterBounceAnim += CharacterVisual_GetCharacterBounceAnim;
            On.Reptile.CharacterVisual.SetInlineSkatesPropsMode += CharacterVisual_SetInlineSkatesPropsMode;
        }

        private static void CharacterVisual_SetInlineSkatesPropsMode(On.Reptile.CharacterVisual.orig_SetInlineSkatesPropsMode orig, CharacterVisual self, CharacterVisual.MoveStylePropMode mode)
        {
            orig(self, mode);
            if (mode == CharacterVisual.MoveStylePropMode.ACTIVE)
            {
                // just mirror what BRCCC does, but no need to check if character is modded or not
                // if we can't fine transforms then it doesn't support it, simple as that
                // hopefully together with BRCCC it won't break it
                Transform offsetL = self.footL.Find("skateOffsetL");
                Transform offsetR = self.footR.Find("skateOffsetR");
                if (offsetL && offsetR)
                {
                    self.moveStyleProps.skateL.transform.SetLocalPositionAndRotation(offsetL.localPosition, offsetL.localRotation);
                    self.moveStyleProps.skateL.transform.localScale = offsetL.localScale;
                    self.moveStyleProps.skateR.transform.SetLocalPositionAndRotation(offsetR.localPosition, offsetR.localRotation);
                    self.moveStyleProps.skateR.transform.localScale = offsetR.localScale;
                }
            }
        }

        private static int CharacterVisual_GetCharacterBounceAnim(On.Reptile.CharacterVisual.orig_GetCharacterBounceAnim orig, Reptile.Characters c)
        {
            if (Enum.IsDefined(typeof(Reptile.Characters), c))
            {
                return orig(c);
            }

            ModdedCharacter moddedCharacter = ModdedCharacter.GetModdedCharacter(c);
            if (moddedCharacter != null)
            {
                return moddedCharacter.bounceHash;
            }

            CharacterAPI.logger.LogWarning($"CharacterVisual::GetCharacterBounceAnim failed to find modded character {c}, replacing it with {Characters.metalHead}.");
            return orig(Characters.metalHead);
        }

        private static int CharacterVisual_GetCharacterFreestyleAnim(On.Reptile.CharacterVisual.orig_GetCharacterFreestyleAnim orig, Reptile.Characters c)
        {
            if (Enum.IsDefined(typeof(Reptile.Characters), c))
            {
                return orig(c);
            }

            ModdedCharacter moddedCharacter = ModdedCharacter.GetModdedCharacter(c);
            if (moddedCharacter != null)
            {
                return moddedCharacter.freestyleHash;
            }

            CharacterAPI.logger.LogWarning($"CharacterVisual::GetCharacterFreestyleAnim failed to find modded character {c}, replacing it with {Characters.metalHead}.");
            return orig(c);
        }
    }
}
