﻿using Reptile;
using System.Collections.Generic;
using UnityEngine;

namespace CharacterAPI
{
    public class ModdedCharacterConstructor
    {
        private struct Outfit
        {
            public string name;
            public Material material;
        }

        private struct PersonalGraffiti
        {
            public string name;
            public string author;
            public Material material;
            public Texture texture;
        }

        public enum FreestyleType
        {
            freestyle1, freestyle2, freestyle3, freestyle4, freestyle5, freestyle6, freestyle7, freestyle8, freestyle9, freestyle10, freestyle11, freestyle12, freestyle13, freestyle14, freestyle15, freestyle16, freestyle17, freestyle18, freestyle19
        }

        public enum BounceType
        {
            bounce, softbounce1, softbounce2, softbounce3, softbounce4, softbounce5, softbounce6, softbounce7, softbounce8, softbounce9, softbounce10, softbounce11, softbounce12, softbounce13, softbounce14, softbounce15
        }

        public string? characterName;

        public GameObject? characterPrefab;

        public int defaultOutfit = 0;

        public MoveStyle defaultMoveStyle = MoveStyle.SKATEBOARD;

        public Characters tempAudioBase = Characters.metalHead;

        public Characters personalGraffitiBase = Characters.metalHead;

        public FreestyleType freestyleType = FreestyleType.freestyle1;

        public BounceType bounceType = BounceType.bounce;

        private List<Outfit> outfits = new List<Outfit>();

        private PersonalGraffiti personalGraffiti;

        public void AddOutfit(Material material, string name = "")
        {
            if (outfits.Count > 4)
            {
                CharacterAPI.logger.LogWarning($"Can't have more than 4 outfits on character {this.characterName}, ignoring outfit {name}.");
                return;
            }

            if (!material)
            {
                CharacterAPI.logger.LogWarning($"on outfit creation material was empty, ignoging outfit {name}.");
                return;
            }

            outfits.Add(new Outfit { material = material, name = string.IsNullOrEmpty(name) ? GetOutfitNameBasedOnIndex(outfits.Count) : name });
        }

        public void AddPersonalGraffiti(string name, string author, Material material, Texture texture)
        {
            if (!material)
            {
                CharacterAPI.logger.LogWarning($"Personal graffiti {name} for character {this.characterName} has no material, ignoring...");
                return;
            }

            if (!texture)
            {
                CharacterAPI.logger.LogWarning($"Personal graffiti {name} for character {this.characterName} has no texture, ignoring...");
                return;
            }

            personalGraffiti = new PersonalGraffiti
            {
                author = author,
                name = name,
                material = material,
                texture = texture
            };
        }

        public bool CreateModdedCharacter()
        {
            if (string.IsNullOrEmpty(characterName))
            {
                CharacterAPI.logger.LogWarning("Attempted to add charcter without a name. Ignoring...");
                return false;
            }

            if (!characterPrefab)
            {
                CharacterAPI.logger.LogWarning($"Character {characterName} doesn't have a prefab. Ignoring...");
                return false;
            }

            if (outfits.Count == 0)
            {
                CharacterAPI.logger.LogWarning($"Character {characterName} needs to have at least one outfit. Ignoring...");
                return false;
            }

            CharacterAPI.ModdedCharacter newCharacter = new CharacterAPI.ModdedCharacter();

            newCharacter.Name = characterName;

            newCharacter.outfitNames = new string[4];
            newCharacter.loadedCharacterMaterials = new Material[4];

            for (int i = 0; i < 4; i++)
            {
                newCharacter.outfitNames[i] = outfits[i % outfits.Count].name;
                newCharacter.loadedCharacterMaterials[i] = outfits[i % outfits.Count].material;
            }

            newCharacter.loadedCharacterFbxAssets = characterPrefab;

            newCharacter.defaultOutfit = Mathf.Clamp(defaultOutfit, 0, 3);
            newCharacter.defaultMoveStyle = defaultMoveStyle;

            newCharacter.tempAudioCharacter = tempAudioBase;

            newCharacter.freestyleHash = Animator.StringToHash("freestyle" + ((int)freestyleType + 1));
            if (bounceType == BounceType.bounce)
            {
                newCharacter.bounceHash = Animator.StringToHash("bounce");
            } else 
            {
                newCharacter.bounceHash = Animator.StringToHash("softBounce" + ((int)freestyleType));
            }

            if (personalGraffiti.material && personalGraffiti.texture)
            {
                newCharacter.usePersonalGrafitti = true;

                newCharacter.personalGrafitti = new GraffitiArt();
                newCharacter.personalGrafitti.graffitiMaterial = personalGraffiti.material;
                newCharacter.personalGrafitti.title = personalGraffiti.name;
                newCharacter.personalGrafitti.artistName = personalGraffiti.author;
                newCharacter.personalGrafitti.graffitiSize = GraffitiSize.S;

                newCharacter.personalGrafitti.unlockable = ScriptableObject.CreateInstance<Reptile.Phone.GraffitiAppEntry>();
                newCharacter.personalGrafitti.unlockable.Title = newCharacter.personalGrafitti.title;
                newCharacter.personalGrafitti.unlockable.Artist = newCharacter.personalGrafitti.artistName;
                newCharacter.personalGrafitti.unlockable.Size = newCharacter.personalGrafitti.graffitiSize;
                newCharacter.personalGrafitti.unlockable.GraffitiTexture = personalGraffiti.texture;
            }
            else
            {
                newCharacter.usePersonalGrafitti = false;
                newCharacter.characterGraffitiBase = personalGraffitiBase;
            }

            // creating characterVisual
            //ConstructCharacterVisual(sanic);

            //characterToConstruct is enum
            GameObject gameObject = new GameObject($"{newCharacter.Name}Visuals");

            //GameObject characterModel = CreateCharacterFbx(characterToConstruct);
            GameObject characterModel = UnityEngine.Object.Instantiate(newCharacter.loadedCharacterFbxAssets);

            //InitCharacterModel(characterModel, gameObject);
            characterModel.transform.SetParent(gameObject.transform, false);

            //InitSkinnedMeshRendererForModel(characterToConstruct, characterModel);
            SkinnedMeshRenderer skinnedMeshRenderer = characterModel.GetComponentInChildren<SkinnedMeshRenderer>();
            skinnedMeshRenderer.sharedMaterial = UnityEngine.Object.Instantiate(newCharacter.loadedCharacterMaterials[0]);
            skinnedMeshRenderer.receiveShadows = false;

            //InitAnimatorForModel(characterModel);
            characterModel.GetComponentInChildren<Animator>().applyRootMotion = false;

            //InitCharacterVisuals(gameObject);
            gameObject.SetActive(false);

            newCharacter.characterVisual = gameObject;

            CharacterAPI.ModdedCharacters.Add(newCharacter);

            CharacterAPI.logger.LogMessage($"Character {characterName} successfully added.");

            return true;
        }

        private string GetOutfitNameBasedOnIndex(int i)
        {
            switch (i)
            {
                case 0:
                    return "U_SKIN_SPRING";
                case 1:
                    return "U_SKIN_SUMMER";
                case 2:
                    return "U_SKIN_AUTUMN";
                case 3:
                    return "U_SKIN_WINTER";
                default:
                    return "U_SKIN_SPRING";
            }
        }

    }
}