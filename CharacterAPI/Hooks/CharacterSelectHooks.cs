using Mono.Cecil.Cil;
using MonoMod.Cil;
using Reptile;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using static CharacterAPI.CharacterAPI;

namespace CharacterAPI.Hooks
{
    public class CharacterSelectHooks
    {
        private static List<SelectableCharacterWithMods> selectableCharactesWithMods = new List<SelectableCharacterWithMods>();

        private struct SelectableCharacterWithMods
        {
            public bool IsModdedCharacter;
            public Characters characterEnum;
        }

        public static void InitHooks()
        {
            On.Reptile.CharacterSelect.PopulateListOfSelectableCharacters += CharacterSelect_PopulateListOfSelectableCharacters;
            On.Reptile.CharacterSelect.CreateCharacterSelectCharacter += CharacterSelect_CreateCharacterSelectCharacter;
            IL.Reptile.CharacterSelect.Init += CharacterSelect_Init;
            IL.Reptile.CharacterSelect.DefineLocations += CharacterSelect_DefineLocations;
            IL.Reptile.CharacterSelect.MoveSelection += CharacterSelect_MoveSelection;
            IL.Reptile.CharacterSelect.UpdateCharactersInCircle += CharacterSelect_UpdateCharactersInCircle;
            On.Reptile.CharacterSelect.SetPlayerToCharacter += CharacterSelect_SetPlayerToCharacter;
        }

        private static void CharacterSelect_SetPlayerToCharacter(On.Reptile.CharacterSelect.orig_SetPlayerToCharacter orig, CharacterSelect self, int index)
        {
            orig(self, index);

            // writing new current character to current modded character save slot
            if (!Enum.IsDefined(typeof(Characters), self.player.character))
            {
                ModdedCharacter moddedCharacter = ModdedCharacter.GetModdedCharacter(self.player.character);
                if (moddedCharacter != null)    
                {
                    ModdedCharacterProgress.SetLastPlayedCharacter(Core.instance.saveManager.saveSlotHandler.currentSaveSlot.saveSlotId, moddedCharacter.GetHashCode());
                }
            }
            else
            {
                ModdedCharacterProgress.SetLastPlayedCharacter(Core.instance.saveManager.saveSlotHandler.currentSaveSlot.saveSlotId, -1);
            }

            // SAVE_MODDED_CHARACTERS
            ModdedCharacterProgress.SaveAsync();
        }

        private static void CharacterSelect_CreateCharacterSelectCharacter(On.Reptile.CharacterSelect.orig_CreateCharacterSelectCharacter orig, CharacterSelect self, Characters character, int numInCircle, CharacterSelectCharacter.CharSelectCharState startState)
        {
            if (Enum.IsDefined(typeof(Characters), character))
            {
                orig(self, character, numInCircle, startState);
            }
            else
            {
                Player player = self.player;
                CharacterVisual characterVisual = player.CharacterConstructor.CreateNewCharacterVisual(character, player.animatorController, true, player.motor.groundDetection.groundLimit);
                int outfit = Core.instance.SaveManager.CurrentSaveSlot.GetCharacterProgress(character).outfit;
                Material sharedMaterial = player.characterConstructor.CreateCharacterMaterial(character, outfit);

                characterVisual.mainRenderer.sharedMaterial = sharedMaterial;
                self.CharactersInCircle[numInCircle] = characterVisual.gameObject.AddComponent<CharacterSelectCharacter>();
                self.charactersInCircle[numInCircle].transform.position = self.characterPositions[numInCircle];
                self.charactersInCircle[numInCircle].transform.rotation = Quaternion.LookRotation(self.characterDirections[numInCircle] * -1f);
                self.charactersInCircle[numInCircle].transform.parent = self.tf;
                self.charactersInCircle[numInCircle].Init(self, character, self.charCollision, self.charTrigger, self.tf.position, UnityEngine.Object.Instantiate(self.swapSequence, self.charactersInCircle[numInCircle].transform).GetComponent<PlayableDirector>());
                self.charactersInCircle[numInCircle].SetState(startState);
            }
        }

        private static void CharacterSelect_PopulateListOfSelectableCharacters(On.Reptile.CharacterSelect.orig_PopulateListOfSelectableCharacters orig, CharacterSelect self, Player player)
        {
            orig(self, player);
            selectableCharactesWithMods.Clear();
            foreach (Characters character in self.selectableCharacters)
            {
                selectableCharactesWithMods.Add(new SelectableCharacterWithMods { characterEnum = character, IsModdedCharacter = false });
            }
            foreach (ModdedCharacter moddedCharacter in ModdedCharacter.ModdedCharacters)
            {
                if (self.player.character != moddedCharacter.characterEnum)
                {
                    selectableCharactesWithMods.Add(new SelectableCharacterWithMods { characterEnum = moddedCharacter.characterEnum, IsModdedCharacter = true });
                }
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
                    cs.characterSelectUI.SetCharacterInformation(selectableCharactesWithMods[cs.selection].characterEnum);
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
                    cs.characterSelectUI.SetCharacterInformation(selectableCharactesWithMods[cs.selection].characterEnum);
                }));
            }
            else
            {
                logger.LogError("Third CharacterSelect::Init hook failed.");
            }
        }
    }
}
