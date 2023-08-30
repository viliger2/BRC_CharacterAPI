using static CharacterAPI.ExtensionMethods.CharacterSelectExtensions;

namespace CharacterAPI.ExtensionMethods
{
    public static class CharacterSelectUIExtensions
    {

        public static void SetCharacterInformation(this Reptile.CharacterSelectUI instance, SelectableCharacterWithMods character)
        {
            if (character.IsModdedCharacter)
            {
                // SetCharacterOutfitsUnlocked replacement
                instance.characterUnlockedOutfitCountLabel.text = "4/4";
                // SetCharacterName replacement
                instance.characterNameLabel.text = character.moddedCharacter.Name;
                // SetCharacterSelectUIMoveStyle replacement
                instance.SetCharacterSelectUIMoveStyle(character.moddedCharacter.defaultMoveStyle);
            }
            else
            {
                instance.SetCharacterInformation(character.characterEnum);
            }
        }

    }
}
