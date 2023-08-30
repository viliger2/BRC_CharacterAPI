using Reptile;
using UnityEngine;
using static CharacterAPI.ExtensionMethods.CharacterSelectExtensions;

namespace CharacterAPI.ExtensionMethods
{
    public static class CharacterConstructorExtensions
    {
        public static CharacterVisual CreateNewCharacterVisual(this CharacterConstructor instance, SelectableCharacterWithMods character, RuntimeAnimatorController controller, bool IK = false, float setGroundAngleLimit = 0f)
        {
            CharacterVisual characterVisual = UnityEngine.Object.Instantiate(character.moddedCharacter.characterVisual).AddComponent<CharacterVisual>();
            // fixing shaders
            // should probably move this to initialization, but I am not sure how to load game's objects without Adressables
            SkinnedMeshRenderer meshRenderer = characterVisual.GetComponentInChildren<SkinnedMeshRenderer>();
            if (meshRenderer)
            {
                CharacterAPI.AttemptToFixShaderCharacter(instance.characterLoader, meshRenderer.material);
            }
            // CharacterVisual doesn't keep Character enum and just uses it for some setup things, so we can safely ignore it for now
            characterVisual.Init(character.characterEnum, controller, IK, setGroundAngleLimit);
            characterVisual.gameObject.SetActive(true);
            return characterVisual;
        }

        public static Material CreateCharacterMaterial(this CharacterConstructor instance, SelectableCharacterWithMods character, int outfit)
        {
            Material material = character.moddedCharacter.loadedCharacterMaterials[outfit];

            // should probably move this to initialization, but I am not sure how to load game's objects without Adressables
            CharacterAPI.AttemptToFixShaderCharacter(instance.characterLoader, material);

            return material;
        }

    }
}
