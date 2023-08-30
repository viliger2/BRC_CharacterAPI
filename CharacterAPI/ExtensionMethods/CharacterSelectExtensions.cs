using Reptile;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace CharacterAPI.ExtensionMethods
{
    public static class CharacterSelectExtensions
    {
        public const int STARTING_VALUE = (int)Characters.MAX + 1;

        public static List<SelectableCharacterWithMods> selectableCharactesWithMods = new List<SelectableCharacterWithMods>();

        public struct SelectableCharacterWithMods
        {
            public Characters characterEnum;
            public bool IsModdedCharacter;
            public CharacterAPI.ModdedCharacter moddedCharacter;
        }

        public static SelectableCharacterWithMods GetCharacterWithMods(Characters character)
        {
            return selectableCharactesWithMods.Find(x => x.characterEnum == character);
        }

        public static void CreateCharacterSelectCharacter(this Reptile.CharacterSelect instance, SelectableCharacterWithMods character, int numInCircle, CharacterSelectCharacter.CharSelectCharState startState)
        {
            Player player = instance.player;
            CharacterVisual characterVisual = player.CharacterConstructor.CreateNewCharacterVisual(character, player.animatorController, true, player.motor.groundDetection.groundLimit);
            int outfit = Core.instance.SaveManager.CurrentSaveSlot.GetCharacterProgress(character.characterEnum).outfit;
            Material sharedMaterial = player.characterConstructor.CreateCharacterMaterial(character, outfit);

            characterVisual.mainRenderer.sharedMaterial = sharedMaterial;
            instance.CharactersInCircle[numInCircle] = characterVisual.gameObject.AddComponent<CharacterSelectCharacter>();
            instance.charactersInCircle[numInCircle].transform.position = instance.characterPositions[numInCircle];
            instance.charactersInCircle[numInCircle].transform.rotation = Quaternion.LookRotation(instance.characterDirections[numInCircle] * -1f);
            instance.charactersInCircle[numInCircle].transform.parent = instance.tf;
            instance.charactersInCircle[numInCircle].Init(instance, character.characterEnum, instance.charCollision, instance.charTrigger, instance.tf.position, UnityEngine.Object.Instantiate(instance.swapSequence, instance.charactersInCircle[numInCircle].transform).GetComponent<PlayableDirector>());
            instance.charactersInCircle[numInCircle].SetState(startState);
        }
    }
}
