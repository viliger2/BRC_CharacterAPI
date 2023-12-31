﻿using Reptile;
using System;
using static CharacterAPI.CharacterAPI;

namespace CharacterAPI.Hooks
{
    public class CharacterSelectUIHooks
    {
        public static void InitHooks()
        {
            On.Reptile.CharacterSelectUI.SetCharacterInformation += CharacterSelectUI_SetCharacterInformation;
        }

        private static void CharacterSelectUI_SetCharacterInformation(On.Reptile.CharacterSelectUI.orig_SetCharacterInformation orig, Reptile.CharacterSelectUI self, Reptile.Characters character)
        {
            if (Enum.IsDefined(typeof(Characters), character))
            {
                orig(self, character);
            }
            else
            {
                ModdedCharacter moddedCharacter = ModdedCharacter.GetModdedCharacter(character);
                if (moddedCharacter != null)
                {
                    // SetCharacterOutfitsUnlocked replacement
                    self.characterUnlockedOutfitCountLabel.text = "4/4";
                    // SetCharacterName replacement
                    self.characterNameLabel.text = moddedCharacter.Name;
                    // SetCharacterSelectUIMoveStyle replacement

                    var progress = ModdedCharacterProgress.GetCharacterProgress(Core.instance.saveManager.CurrentSaveSlot.saveSlotId, moddedCharacter.GetHashCode());
                    if (progress != null)
                    {
                        self.SetCharacterSelectUIMoveStyle(progress.characterProgress.moveStyle);
                    }
                    else
                    {
                        self.SetCharacterSelectUIMoveStyle(moddedCharacter.defaultMoveStyle);
                    }
                }
                else
                {
                    CharacterAPI.logger.LogWarning($"CharacterSelectUI::SetCharacterInformation failed to setup character information for modded character {character}, replacing it with {Characters.metalHead}.");
                    orig(self, Characters.metalHead);
                }
            }
        }
    }
}
