using Reptile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            } else
            {
                ModdedCharacter moddedCharacter = CharacterAPI.GetModdedCharacter(character);

                // SetCharacterOutfitsUnlocked replacement
                self.characterUnlockedOutfitCountLabel.text = "4/4";
                // SetCharacterName replacement
                self.characterNameLabel.text = moddedCharacter.Name;
                // SetCharacterSelectUIMoveStyle replacement
                self.SetCharacterSelectUIMoveStyle(moddedCharacter.defaultMoveStyle);
            }
        }
    }
}
