using CharacterAPI.ExtensionMethods;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Reptile;
using System;
using UnityEngine;

namespace CharacterAPI.Hooks
{
    public class CharacterConstructorHooks
    {
        public static void InitHooks()
        {
            On.Reptile.CharacterConstructor.CreateCharacterMaterial += CharacterConstructor_CreateCharacterMaterial;
        }

        private static Material CharacterConstructor_CreateCharacterMaterial(On.Reptile.CharacterConstructor.orig_CreateCharacterMaterial orig, CharacterConstructor self, Characters character, int outfit)
        {
            if (Enum.IsDefined(typeof(Characters), character))
            {
                return orig(self, character, outfit);
            }
            else
            {
                CharacterSelectExtensions.SelectableCharacterWithMods characterWithMods = CharacterSelectExtensions.GetCharacterWithMods(character);
                return self.CreateCharacterMaterial(characterWithMods, outfit);
            }
        }

        private static void CharacterConstructor_SetMoveStyleSkinsForCharacter(MonoMod.Cil.ILContext il)
        {
            ILCursor c = new ILCursor(il);
            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchCallOrCallvirt<Reptile.Core>("get_Instance"),
                x => x.MatchCallOrCallvirt<Reptile.Core>("get_SaveManager"),
                x => x.MatchCallOrCallvirt<Reptile.SaveManager>("get_CurrentSaveSlot"),
                x => x.MatchLdarg(out _)))
            {
                c.Next.OpCode = OpCodes.Ldarg_2;
                c.Next.Operand = null;
                c.Index++;
                c.RemoveRange(5);
                c.EmitDelegate<Func<Characters, int>>((character) =>
                {
                    CharacterSelectExtensions.SelectableCharacterWithMods characterWithMods = CharacterSelectExtensions.GetCharacterWithMods(character);
                    if (characterWithMods.IsModdedCharacter)
                    {
                        return 0;
                    }
                    else
                    {
                        return Core.Instance.SaveManager.CurrentSaveSlot.GetCharacterProgress(character).moveStyleSkin;
                    }
                });
            }
            else
            {
                CharacterAPI.logger.LogError("CharacterConstructor::SetMoveStyleSkinsForCharacter hook failed.");
            }
        }
    }
}
