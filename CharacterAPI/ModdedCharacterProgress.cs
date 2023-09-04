using Reptile;
using System.Collections.Generic;
using UnityEngine;

namespace CharacterAPI
{
    public class ModdedCharacterProgress
    {

        public static Dictionary<Characters, CharacterProgress> moddedCharacterProgress = new Dictionary<Characters, Reptile.CharacterProgress>();

        public static CharacterProgress NewModdedCharacterProgress(Characters character, int outfit, MoveStyle moveStyle, int moveStyleSkin)
        {
            moveStyleSkin = Mathf.Clamp(moveStyleSkin, 0, 9);
            outfit = Mathf.Clamp(outfit, 0, 3);

            CharacterProgress progress = new CharacterProgress();
            progress.character = character;
            progress.unlocked = true;
            progress.outfit = outfit;
            progress.moveStyle = moveStyle;
            progress.moveStyleSkin = moveStyleSkin;
            moddedCharacterProgress.Add(character, progress);

            return progress;
        }

        public static CharacterProgress GetModdedCharacterProgress(int moddedCharacter)
        {
            return GetModdedCharacterProgress((Characters)moddedCharacter);
        }

        public static CharacterProgress GetModdedCharacterProgress(Characters moddedCharacter)
        {
            moddedCharacterProgress.TryGetValue(moddedCharacter, out var progress);
            return progress;
        }

        public static CharacterProgress NewModdedCharacterProgress(Characters moddedCharacter)
        {
            return NewModdedCharacterProgress(moddedCharacter, 0, 0, 0);
        }

    }
}
