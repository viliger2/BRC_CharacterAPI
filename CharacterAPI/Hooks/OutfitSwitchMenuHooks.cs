using Mono.Cecil.Cil;
using MonoMod.Cil;
using Reptile;
using System;
using UnityEngine;

namespace CharacterAPI.Hooks
{
    public class OutfitSwitchMenuHooks
    {
        public static void InitHooks()
        {
            On.Reptile.OutfitSwitchMenu.GetOutfitUnlockable += OutfitSwitchMenu_GetOutfitUnlockable;
            IL.Reptile.OutfitSwitchMenu.SkinButtonClicked += OutfitSwitchMenu_SkinButtonClicked;
            IL.Reptile.OutfitSwitchMenu.SkinButtonSelected += OutfitSwitchMenu_SkinButtonSelected;
        }

        private static void OutfitSwitchMenu_SkinButtonSelected(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            if(c.TryGotoNext(MoveType.Before,
                x => x.MatchLdarg(out _),
                x => x.MatchLdfld<Reptile.OutfitSwitchMenu>("player"),
                x => x.MatchCallOrCallvirt<Reptile.Player>("get_CharacterConstructor")))
            {
                c.Index++;
                c.RemoveRange(6);
                c.Emit(OpCodes.Ldloc_0); // (int)character
                c.Emit(OpCodes.Ldarg_2); // skinIndex
                c.EmitDelegate<Func<Reptile.OutfitSwitchMenu, int, int, Material>>((outfitSwitchMenu, character, skinIndex) =>
                {
                    if(Enum.IsDefined(typeof(Characters), (Characters)character))
                    {
                        return outfitSwitchMenu.player.CharacterConstructor.GetCharacterMaterials()[character, skinIndex];
                    } else
                    {
                        var moddedCharacter = ModdedCharacter.GetModdedCharacter((Characters)character);
                        if (moddedCharacter != null)
                        {
                            Material material = moddedCharacter.loadedCharacterMaterials[skinIndex];
                            if(material)
                            {
                                CharacterAPI.AttemptToFixShaderCharacter(outfitSwitchMenu.player.characterConstructor.characterLoader, material);
                            }
                            return material;
                        } else
                        {
                            CharacterAPI.logger.LogWarning($"OutfitSwitchMenu::SkinButtonSelected failed to find modded character {character}, replacing it with {Characters.metalHead}.");
                            return outfitSwitchMenu.player.CharacterConstructor.GetCharacterMaterials()[(int)Characters.metalHead, skinIndex];
                        }
                    }
                });
            } else
            {
                CharacterAPI.logger.LogError("OutfitSwitchMenu::SkinButtonSelected hook failed.");
            }
        }

        private static void OutfitSwitchMenu_SkinButtonClicked(MonoMod.Cil.ILContext il)
        {

            ILCursor c = new ILCursor(il);
            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchLdarg(out _),
                x => x.MatchLdfld<Reptile.OutfitSwitchMenu>("player"),
                x => x.MatchCallOrCallvirt<Reptile.Player>("get_CharacterConstructor"),
                x => x.MatchCallOrCallvirt<Reptile.CharacterConstructor>("GetCharacterMaterials")))
            {
                c.Index++;
                c.RemoveRange(8);
                c.Emit(OpCodes.Ldarg_2);
                c.EmitDelegate<Func<Reptile.OutfitSwitchMenu, int, bool>>((osm, skinIndex) =>
                {
                    Characters character = osm.player.character;
                    if (Enum.IsDefined(typeof(Characters), character))
                    {
                        return osm.player.CharacterConstructor.GetCharacterMaterials()[(int)character, skinIndex] != null;
                    }

                    var moddedCharacter = ModdedCharacter.GetModdedCharacter(character);
                    if (moddedCharacter != null)
                    {
                        return moddedCharacter.loadedCharacterMaterials[skinIndex] != null;
                    }

                    CharacterAPI.logger.LogWarning($"OutfitSwitchMenu::SkinButtonClicked failed to find modded character {character}, replacing it with {Characters.metalHead}.");
                    return osm.player.CharacterConstructor.GetCharacterMaterials()[(int)Characters.metalHead, skinIndex] != null;
                });
            }
            else
            {
                CharacterAPI.logger.LogError("OutfitSwitchMenu::SkinButtonClicked hook failed.");
            }
        }

        private static Reptile.OutfitUnlockable OutfitSwitchMenu_GetOutfitUnlockable(On.Reptile.OutfitSwitchMenu.orig_GetOutfitUnlockable orig, Reptile.OutfitUnlockable[] outfitUnlockables, int[] characterIndexMap, int characterIndex, int outfitIndex)
        {
            Characters character = (Characters)characterIndex;
            if (Enum.IsDefined(typeof(Characters), character))
            {
                return orig(outfitUnlockables, characterIndexMap, characterIndex, outfitIndex);
            }

            var moddedCharacter = ModdedCharacter.GetModdedCharacter(character);
            if (moddedCharacter != null)
            {
                string name = moddedCharacter.outfitNames[outfitIndex];

                OutfitUnlockable outfitUnlockable = ScriptableObject.CreateInstance<OutfitUnlockable>();
                outfitUnlockable.outfitName = name != null ? name : moddedCharacter.Name;
                outfitUnlockable.character = character;
                outfitUnlockable.outfitIndex = outfitIndex;
                outfitUnlockable.IsDefault = true;
                return outfitUnlockable;
            }

            CharacterAPI.logger.LogWarning($"OutfitSwitchMenu::SkinButtonClicked failed to find modded character {character}, replacing it with {Characters.metalHead}.");
            return orig(outfitUnlockables, characterIndexMap, (int)Characters.metalHead, outfitIndex);
        }
    }
}
