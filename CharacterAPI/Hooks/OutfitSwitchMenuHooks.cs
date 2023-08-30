using CharacterAPI.ExtensionMethods;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Reptile;
using System;
using UnityEngine;
using static CharacterAPI.ExtensionMethods.CharacterSelectExtensions;

namespace CharacterAPI.Hooks
{
    public class OutfitSwitchMenuHooks
    {
        public static void InitHooks()
        {
            On.Reptile.OutfitSwitchMenu.GetOutfitUnlockable += OutfitSwitchMenu_GetOutfitUnlockable;
            On.Reptile.OutfitSwitchMenu.SkinButtonSelected += OutfitSwitchMenu_SkinButtonSelected;
            IL.Reptile.OutfitSwitchMenu.SkinButtonClicked += OutfitSwitchMenu_SkinButtonClicked;
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
                    else
                    {
                        //CharacterWithMods characterWithMods = selectableCharactesWithMods.Find(x => x.characterEnum == character);
                        SelectableCharacterWithMods characterWithMods = GetCharacterWithMods(character);
                        return characterWithMods.moddedCharacter.loadedCharacterMaterials[skinIndex] != null;
                    }
                });
            }
            else
            {
                CharacterAPI.logger.LogError("OutfitSwitchMenu::SkinButtonClicked hook failed.");
            }
        }

        // TODO: replace with IL hook since I just rewrote whole function except where you get the material
        private static void OutfitSwitchMenu_SkinButtonSelected(On.Reptile.OutfitSwitchMenu.orig_SkinButtonSelected orig, OutfitSwitchMenu self, MenuTimelineButton clickedButton, int skinIndex)
        {
            Characters character = self.player.character;
            if (Enum.IsDefined(typeof(Characters), character))
            {
                orig(self, clickedButton, skinIndex);
            }
            else
            {
                if (!self.IsTransitioning && self.buttonClicked == null)
                {
                    //CharacterWithMods characterWithMods = selectableCharactesWithMods.Find(x => x.characterEnum == character);
                    SelectableCharacterWithMods characterWithMods = GetCharacterWithMods(character);
                    Material material = self.player.characterConstructor.CreateCharacterMaterial(characterWithMods, skinIndex);
                    if (material)
                    {
                        self.previewCharacterVisual.mainRenderer.material = material;
                    }
                }
            }
        }

        private static Reptile.OutfitUnlockable OutfitSwitchMenu_GetOutfitUnlockable(On.Reptile.OutfitSwitchMenu.orig_GetOutfitUnlockable orig, Reptile.OutfitUnlockable[] outfitUnlockables, int[] characterIndexMap, int characterIndex, int outfitIndex)
        {
            Characters character = (Characters)characterIndex;
            if (Enum.IsDefined(typeof(Characters), character))
            {
                return orig(outfitUnlockables, characterIndexMap, characterIndex, outfitIndex);
            }

            SelectableCharacterWithMods characterWithMods = GetCharacterWithMods(character);
            string name = characterWithMods.moddedCharacter.outfitNames[outfitIndex];

            OutfitUnlockable outfitUnlockable = ScriptableObject.CreateInstance<OutfitUnlockable>();
            outfitUnlockable.outfitName = name != null ? name : characterWithMods.moddedCharacter.Name; // TODO: give people option to name the skins as they want
            outfitUnlockable.character = character;
            outfitUnlockable.outfitIndex = outfitIndex;
            outfitUnlockable.IsDefault = true;
            return outfitUnlockable;
        }
    }
}
