using Mono.Cecil.Cil;
using MonoMod.Cil;
using Reptile;
using System;
using UnityEngine;
using static CharacterAPI.CharacterAPI;

namespace CharacterAPI.Hooks
{
    public class CharacterConstructorHooks
    {
        public static void InitHooks()
        {
            On.Reptile.CharacterConstructor.CreateCharacterMaterial += CharacterConstructor_CreateCharacterMaterial;
            On.Reptile.CharacterConstructor.CreateNewCharacterVisual += CharacterConstructor_CreateNewCharacterVisual;
        }

        private static CharacterVisual CharacterConstructor_CreateNewCharacterVisual(On.Reptile.CharacterConstructor.orig_CreateNewCharacterVisual orig, CharacterConstructor self, Characters character, RuntimeAnimatorController controller, bool IK, float setGroundAngleLimit)
        {
            if (Enum.IsDefined(typeof(Characters), character))
            {
                return orig(self, character,controller, IK, setGroundAngleLimit);
            }

            ModdedCharacter moddedCharacter = CharacterAPI.GetModdedCharacter(character);
            if (moddedCharacter != null)
            {
                CharacterVisual characterVisual = UnityEngine.Object.Instantiate(moddedCharacter.characterVisual).AddComponent<CharacterVisual>();
                SkinnedMeshRenderer meshRenderer = characterVisual.GetComponentInChildren<SkinnedMeshRenderer>();
                if (meshRenderer)
                {
                    CharacterAPI.AttemptToFixShaderCharacter(self.characterLoader, meshRenderer.material);
                }
                characterVisual.Init(character, controller, IK, setGroundAngleLimit);
                characterVisual.gameObject.SetActive(true);
                return characterVisual;
            }

            CharacterAPI.logger.LogWarning($"CharacterConstructor::CreateNewCharacterVisual failed to find modded character {character}, replacing it with {Characters.metalHead}.");
            return orig(self, Characters.metalHead, controller, IK, setGroundAngleLimit);
        }

        private static Material CharacterConstructor_CreateCharacterMaterial(On.Reptile.CharacterConstructor.orig_CreateCharacterMaterial orig, CharacterConstructor self, Characters character, int outfit)
        {
            if (Enum.IsDefined(typeof(Characters), character))
            {
                return orig(self, character, outfit);
            }

            ModdedCharacter moddedCharacter = CharacterAPI.GetModdedCharacter(character);
            if (moddedCharacter != null)
            {
                Material material = moddedCharacter.loadedCharacterMaterials[outfit];

                // should probably move this to initialization, but I am not sure how to load game's objects without Adressables
                CharacterAPI.AttemptToFixShaderCharacter(self.characterLoader, material);

                return material;
            }

            CharacterAPI.logger.LogWarning($"CharacterConstructor::CreateCharacterMaterial failed to find modded character {character}, replacing it with {Characters.metalHead}.");
            return orig(self, Characters.metalHead, outfit);
        }
    }
}
