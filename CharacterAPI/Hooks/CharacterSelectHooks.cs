using CharacterAPI.ExtensionMethods;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Reptile;
using System;
using static CharacterAPI.CharacterAPI;
using static CharacterAPI.ExtensionMethods.CharacterSelectExtensions;
using static CharacterAPI.ExtensionMethods.CharacterSelectUIExtensions;

namespace CharacterAPI.Hooks
{
    public class CharacterSelectHooks
    {
        public static void InitHooks()
        {
            On.Reptile.CharacterSelect.PopulateListOfSelectableCharacters += CharacterSelect_PopulateListOfSelectableCharacters;
            On.Reptile.CharacterSelect.CreateCharacterSelectCharacter += CharacterSelect_CreateCharacterSelectCharacter;
            IL.Reptile.CharacterSelect.Init += CharacterSelect_Init;
            IL.Reptile.CharacterSelect.DefineLocations += CharacterSelect_DefineLocations;
            IL.Reptile.CharacterSelect.MoveSelection += CharacterSelect_MoveSelection;
            IL.Reptile.CharacterSelect.UpdateCharactersInCircle += CharacterSelect_UpdateCharactersInCircle;
        }

        private static void CharacterSelect_CreateCharacterSelectCharacter(On.Reptile.CharacterSelect.orig_CreateCharacterSelectCharacter orig, CharacterSelect self, Characters character, int numInCircle, CharacterSelectCharacter.CharSelectCharState startState)
        {
            if (Enum.IsDefined(typeof(Characters), character))
            {
                orig(self, character, numInCircle, startState);
            }
            else
            {
                SelectableCharacterWithMods moddedCharacter = CharacterSelectExtensions.GetCharacterWithMods(character);
                self.CreateCharacterSelectCharacter(moddedCharacter, numInCircle, startState);
            }
        }

        private static void CharacterSelect_PopulateListOfSelectableCharacters(On.Reptile.CharacterSelect.orig_PopulateListOfSelectableCharacters orig, CharacterSelect self, Player player)
        {
            orig(self, player);
            foreach (Characters character in self.selectableCharacters)
            {
                selectableCharactesWithMods.Add(new SelectableCharacterWithMods { characterEnum = character, IsModdedCharacter = false });
            }
            foreach (CharacterAPI.ModdedCharacter moddedCharacter in CharacterAPI.ModdedCharacters)
            {
                selectableCharactesWithMods.Add(new SelectableCharacterWithMods { characterEnum = (Characters)CharacterSelectExtensions.STARTING_VALUE + CharacterAPI.ModdedCharacters.IndexOf(moddedCharacter), IsModdedCharacter = true, moddedCharacter = moddedCharacter });
            }
        }

        private static void CharacterSelect_UpdateCharactersInCircle(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            //if (num2 >= selectableCharacters.Count)
            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchLdloc(out _),
                x => x.MatchLdarg(out _),
                x => x.MatchLdfld<Reptile.CharacterSelect>("selectableCharacters")))
            {
                c.Index++;
                c.RemoveRange(3);
                c.EmitDelegate(() => { return selectableCharactesWithMods.Count; });
            }
            else
            {
                logger.LogError("First CharacterSelect::UpdateCharactersInCircle hook failed.");
            }

            // num2 = selectableCharacters.Count + num2;
            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchLdarg(out _),
                x => x.MatchLdfld<Reptile.CharacterSelect>("selectableCharacters")))
            {
                c.RemoveRange(3);
                c.EmitDelegate(() => { return selectableCharactesWithMods.Count; });
            }
            else
            {
                logger.LogError("Second CharacterSelect::UpdateCharactersInCircle hook failed.");
            }

            //if (charactersInCircle[num] == null || charactersInCircle[num].character != selectableCharacters[num2])
            //{
            //    if (charactersInCircle[num] != null)
            //    {
            //        Object.Destroy(charactersInCircle[num].gameObject);
            //    }
            //    CreateCharacterSelectCharacter(selectableCharacters[num2], num, CharacterSelectCharacter.CharSelectCharState.BOUNCING);
            //}
            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchLdarg(out _),
                x => x.MatchLdfld<Reptile.CharacterSelect>("charactersInCircle"),
                x => x.MatchLdloc(out _)))
            {
                c.Index++;
                c.RemoveRange(37);
                c.Emit(OpCodes.Ldloc_1); // num2
                c.Emit(OpCodes.Ldloc_0); // num
                c.EmitDelegate<Action<Reptile.CharacterSelect, int, int>>((cs, num2, num) =>
                {
                    if (cs.charactersInCircle[num] == null || cs.charactersInCircle[num].character != selectableCharactesWithMods[num2].characterEnum)
                    {
                        if (cs.charactersInCircle[num] != null)
                        {
                            UnityEngine.Object.Destroy(cs.charactersInCircle[num].gameObject);
                        }
                        cs.CreateCharacterSelectCharacter(selectableCharactesWithMods[num2].characterEnum, num, CharacterSelectCharacter.CharSelectCharState.BOUNCING);
                    }
                });
            }
            else
            {
                logger.LogError("Third CharacterSelect::UpdateCharactersInCircle hook failed.");
            }

            //else if ((charactersInCircle[selectionInCircle] == null || charactersInCircle[selectionInCircle].character != selectableCharacters[selection]) && charactersInCircle[selectionInCircle].character != selectableCharacters[selection])
            //{
            //    if (charactersInCircle[selectionInCircle] != null)
            //    {
            //        Object.Destroy(charactersInCircle[selectionInCircle].gameObject);
            //    }
            //    CreateCharacterSelectCharacter(selectableCharacters[selection], selectionInCircle, CharacterSelectCharacter.CharSelectCharState.BOUNCING);
            //}
            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchLdarg(out _),
                x => x.MatchLdfld<Reptile.CharacterSelect>("charactersInCircle"),
                x => x.MatchLdarg(out _),
                x => x.MatchLdfld<Reptile.CharacterSelect>("selectionInCircle")))
            {
                c.Index++;
                c.RemoveRange(56);
                c.EmitDelegate<Action<Reptile.CharacterSelect>>((cs) =>
                {
                    if ((cs.charactersInCircle[cs.selectionInCircle] == null || cs.charactersInCircle[cs.selectionInCircle].character != selectableCharactesWithMods[cs.selection].characterEnum) && cs.charactersInCircle[cs.selectionInCircle].character != selectableCharactesWithMods[cs.selection].characterEnum)
                    {
                        if (cs.charactersInCircle[cs.selectionInCircle] != null)
                        {
                            UnityEngine.Object.Destroy(cs.charactersInCircle[cs.selectionInCircle].gameObject);
                        }
                        cs.CreateCharacterSelectCharacter(selectableCharactesWithMods[cs.selection].characterEnum, cs.selectionInCircle, CharacterSelectCharacter.CharSelectCharState.BOUNCING);
                    }
                });
            }
            else
            {
                logger.LogError("Fourth CharacterSelect::UpdateCharactersInCircle hook failed.");
            }

            //logger.LogMessage("CharacterSelect::UpdateCharactersInCircle is now: " + il.ToString());
        }

        private static void CharacterSelect_MoveSelection(ILContext il)
        {
            //if (num >= selectableCharacters.Count)
            ILCursor c = new ILCursor(il);
            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchLdloc(out _),
                x => x.MatchLdarg(out _),
                x => x.MatchLdfld<Reptile.CharacterSelect>("selectableCharacters")))
            {
                c.Index++;
                c.RemoveRange(3);
                c.EmitDelegate(() => { return selectableCharactesWithMods.Count; });
            }
            else
            {
                logger.LogError("First CharacterSelect::MoveSelection hook failed.");
            }

            //num = selectableCharacters.Count - 1;
            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchLdarg(out _),
                x => x.MatchLdfld<Reptile.CharacterSelect>("selectableCharacters")))
            {
                c.RemoveRange(3);
                c.EmitDelegate(() => { return selectableCharactesWithMods.Count; });
            }
            else
            {
                logger.LogError("Second CharacterSelect::MoveSelection hook failed.");
            }

            //characterSelectUI.SetCharacterInformation(selectableCharacters[selection]);
            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchLdarg(out _),
                x => x.MatchLdfld<Reptile.CharacterSelect>("characterSelectUI"),
                x => x.MatchLdarg(out _),
                x => x.MatchLdfld<Reptile.CharacterSelect>("selectableCharacters"),
                x => x.MatchLdarg(out _),
                x => x.MatchLdfld<Reptile.CharacterSelect>("selection")))
            {
                c.RemoveRange(8);
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Action<Reptile.CharacterSelect>>((cs) =>
                {
                    cs.characterSelectUI.SetCharacterInformation(selectableCharactesWithMods[cs.selection]);
                });
            }
            else
            {
                logger.LogError("Third CharacterSelect::MoveSelection hook failed.");
            }

            //logger.LogMessage("CharacterSelect::MoveSelection is now: " + il.ToString());
        }

        private static void CharacterSelect_DefineLocations(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            //charactersToPlaceInCircleCount = Mathf.Min(5, selectableCharacters.Count);
            if (c.TryGotoNext(MoveType.Before,
                //x => x.MatchCallOrCallvirt<List<Reptile.Characters>>("get_Count")
                x => x.MatchLdfld<Reptile.CharacterSelect>("selectableCharacters")
                ))
            {
                c.Index--;
                c.RemoveRange(3);
                c.EmitDelegate(() => { return selectableCharactesWithMods.Count; });
            }
            else
            {
                logger.LogError("CharacterSelect::DefineLocations hook failed.");
            }

            //logger.LogMessage("CharacterSelect::DefineLocations is now: " + il.ToString());
        }

        private static void CharacterSelect_Init(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            //for (int i = 0; i < charactersToPlaceInCircleCount; i++)
            //{
            //    CreateCharacterSelectCharacter(selectableCharacters[i], i, CharacterSelectCharacter.CharSelectCharState.BOUNCING);
            //}
            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchLdcI4(out _),
                x => x.MatchStloc(out _),
                x => x.MatchBr(out _)))
            {
                c.RemoveRange(19);
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Action<Reptile.CharacterSelect>>((cs) =>
                {
                    for (int i = 0; i < cs.charactersToPlaceInCircleCount; i++)
                    {
                        var characterWithMods = selectableCharactesWithMods[i];
                        cs.CreateCharacterSelectCharacter(characterWithMods.characterEnum, i, CharacterSelectCharacter.CharSelectCharState.BOUNCING);
                    }
                });
            }
            else
            {
                logger.LogError("First CharacterSelect::Init hook failed.");
            }

            //characterSelectUI.SetAvailableCharacterCount(selectableCharacters.Count);
            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchLdarg(out _),
                x => x.MatchLdfld<Reptile.CharacterSelect>("characterSelectUI"),
                x => x.MatchLdarg(out _),
                x => x.MatchLdfld<Reptile.CharacterSelect>("selectableCharacters")))
            {
                c.RemoveRange(6);
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Action<Reptile.CharacterSelect>>((cs) =>
                {
                    cs.characterSelectUI.SetAvailableCharacterCount(selectableCharactesWithMods.Count);
                });
            }
            else
            {
                logger.LogError("Second CharacterSelect::Init hook failed.");
            }

            //characterSelectUI.SetCharacterInformation(selectableCharacters[selection]);
            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchLdarg(out _),
                x => x.MatchLdfld<Reptile.CharacterSelect>("characterSelectUI"),
                x => x.MatchLdarg(out _),
                x => x.MatchLdfld<Reptile.CharacterSelect>("selectableCharacters"),
                x => x.MatchLdarg(out _),
                x => x.MatchLdfld<Reptile.CharacterSelect>("selection")))
            {
                c.RemoveRange(8);
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Action<Reptile.CharacterSelect>>(((cs) =>
                {
                    cs.characterSelectUI.SetCharacterInformation(selectableCharactesWithMods[cs.selection]);
                }));
            }
            else
            {
                logger.LogError("Third CharacterSelect::Init hook failed.");
            }

            //logger.LogMessage("CharacterSelect::Init is now: " + il.ToString());
        }

    }
}
