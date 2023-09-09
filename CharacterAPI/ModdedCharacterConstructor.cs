using Reptile;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace CharacterAPI
{
    public class ModdedCharacterConstructor
    {
        public const int CHARACTER_STARTING_VALUE = (int)Characters.MAX + 1;
        public const int VOICE_STARTING_VALUE = (int)SfxCollectionID.MAX + 1;

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

        public bool canBlink;

        public List<AudioClip> audioClips = new List<AudioClip>();

        private List<Outfit> outfits = new List<Outfit>();

        private PersonalGraffiti personalGraffiti;

        // oh god, getting all graffiti names from GraffitiArt.Titles via reflection
        private FieldInfo[] graffitiNamesFieldInfos = typeof(Reptile.GraffitiArt.Titles).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).Where(fi => fi.IsLiteral && !fi.IsInitOnly).ToArray();

        public void AddOutfit(Material material, string name = "")
        {
            if (outfits.Count > 3)
            {
                CharacterAPI.logger.LogWarning($"Can't have more than 4 outfits on character {this.characterName}. Skipping outfit {name}...");
                return;
            }

            if (!material)
            {
                CharacterAPI.logger.LogWarning($"Outfit material was empty for character {this.characterName}. Skipping outfit {name}...");
                return;
            }

            outfits.Add(new Outfit { material = material, name = string.IsNullOrEmpty(name) ? GetOutfitNameBasedOnIndex(outfits.Count) : name });
        }

        public void AddPersonalGraffiti(string name, string author, Material material, Texture texture)
        {
            if (!material)
            {
                CharacterAPI.logger.LogWarning($"Personal graffiti {name} for character {this.characterName} has no material. Skipping personal graffiti...");
                return;
            }

            if (!texture)
            {
                CharacterAPI.logger.LogWarning($"Personal graffiti {name} for character {this.characterName} has no texture. Skipping personal graffiti...");
                return;
            }

            if (!CheckGraffitiName(name))
            {
                CharacterAPI.logger.LogWarning($"Personal graffiti {name}'s name for character {this.characterName} collides with existing game graffiti. Skipping personal graffiti...");
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
                CharacterAPI.logger.LogWarning("Attempted to add a charcter without a name. Skipping this character...");
                return false;
            }

            if (!characterPrefab)
            {
                CharacterAPI.logger.LogWarning($"Character {characterName} doesn't have a prefab. Skipping this character...");
                return false;
            }

            if (outfits.Count == 0)
            {
                CharacterAPI.logger.LogWarning($"Character {characterName} needs to have at least one outfit. Skipping this character...");
                return false;
            }

            if (!CheckForTransforms())
            {
                return false;
            }

            ModdedCharacter newCharacter = new ModdedCharacter();

            newCharacter.Name = characterName;

            newCharacter.characterEnum = (Characters)(CHARACTER_STARTING_VALUE + ModdedCharacter.ModdedCharacters.Count);

            newCharacter.outfitNames = new string[4];
            newCharacter.loadedCharacterMaterials = new Material[4];

            for (int i = 0; i < 4; i++)
            {
                newCharacter.outfitNames[i] = outfits[i % outfits.Count].name;
                newCharacter.loadedCharacterMaterials[i] = outfits[i % outfits.Count].material;
            }

            newCharacter.loadedCharacterFbxAssets = characterPrefab;

            var meshRenderer = characterPrefab.GetComponentInChildren<SkinnedMeshRenderer>();
            if(meshRenderer)
            {
                var mesh = meshRenderer.sharedMesh;
                if (mesh)
                {
                    if (mesh.blendShapeCount < 2 && canBlink)
                    {
                        CharacterAPI.logger.LogWarning($"Character {characterName} doesn't have nessesary shape keys\\blend shapes to blink, yet is marked as capable of blinking. Setting canBlink to false.");
                        canBlink = false;
                    }
                }
            }

            newCharacter.canBlink = canBlink;

            newCharacter.defaultOutfit = Mathf.Clamp(defaultOutfit, 0, 3);
            newCharacter.defaultMoveStyle = defaultMoveStyle;

            newCharacter.characterVoiceBase = tempAudioBase;

            newCharacter.freestyleHash = Animator.StringToHash("freestyle" + ((int)freestyleType + 1));
            if (bounceType == BounceType.bounce)
            {
                newCharacter.bounceHash = Animator.StringToHash("bounce");
            }
            else
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

            if (audioClips.Count > 0)
            {
                newCharacter.voiceId = (SfxCollectionID)(VOICE_STARTING_VALUE + ModdedCharacter.ModdedCharacters.Count);
                newCharacter.audioClips = audioClips;
            }
            else
            {
                newCharacter.voiceId = SfxCollectionID.NONE;
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

            ModdedCharacter.ModdedCharacters.Add(newCharacter);

            CharacterAPI.logger.LogMessage($"Character {characterName} with enum {newCharacter.characterEnum}{GetLogStringWithSfxAndGraffiti(newCharacter.voiceId, newCharacter.personalGrafitti)} successfully added.");

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

        private string GetLogStringWithSfxAndGraffiti(SfxCollectionID voiceId, GraffitiArt personalGraffiti)
        {
            string result = "";

            if (voiceId != SfxCollectionID.NONE)
            {
                result = string.Concat(result, $" and SfxCollectionID {voiceId}");
            }

            if(personalGraffiti != null)
            {
                result = string.Concat(result, $" and personal graffiti named \"{personalGraffiti.title}\"");
            }

            return result;
        }

        private bool CheckGraffitiName(string name)
        {
            bool available = true;

            foreach(FieldInfo field in graffitiNamesFieldInfos)
            {
                string value = (string)field.GetValue(null);
                if (value.Equals(name))
                {
                    return false;
                }
            }

            return available;
        }

        private bool CheckForTransforms()
        {
            bool transformsFound = true;

            var animator = characterPrefab.GetComponentInChildren<Animator>();
            if (!animator)
            {
                CharacterAPI.logger.LogWarning($"Character {characterName} doesn't have Animator component. Skipping this character...");
                return false;
            }

            var root = animator.transform.Find("root");
            if (!root) 
            { 
                CharacterAPI.logger.LogWarning($"Character {characterName} doesn't have \"root\" as animation bone base. Skipping this character...");
                return false;
            }

            if(!animator.transform.Find("mesh"))
            {
                CharacterAPI.logger.LogWarning($"Character {characterName} doesn't have mesh named \"mesh\" at prefab's base. Skipping this character...");
                return false;
            }

            transformsFound = CheckForSpecificTransformInRoot(root, "head", transformsFound);
            transformsFound = CheckForSpecificTransformInRoot(root, "jetpack", transformsFound);
            transformsFound = CheckForSpecificTransformInRoot(root, "footr", transformsFound);
            transformsFound = CheckForSpecificTransformInRoot(root, "footl", transformsFound);
            transformsFound = CheckForSpecificTransformInRoot(root, "leg2r", transformsFound);
            transformsFound = CheckForSpecificTransformInRoot(root, "leg2l", transformsFound);
            transformsFound = CheckForSpecificTransformInRoot(root, "handr", transformsFound);
            transformsFound = CheckForSpecificTransformInRoot(root, "handl", transformsFound);

            transformsFound = CheckForSpecificTransformInRoot(root.parent, "handlIK", transformsFound);
            transformsFound = CheckForSpecificTransformInRoot(root.parent, "handrIK", transformsFound);
            transformsFound = CheckForSpecificTransformInRoot(root.parent, "bmxFrame", transformsFound);
            transformsFound = CheckForSpecificTransformInRoot(root.parent, "phoneDirectionRoot", transformsFound);
            transformsFound = CheckForSpecificTransformInRoot(root.parent, "skateboard", transformsFound);

            return transformsFound;
        }

        private bool CheckForSpecificTransformInRoot(Transform transform, string bone, bool bonesFound)
        {
            bool boneExists = transform.Find(bone);
            if (!boneExists)
            {
                boneExists = transform.FindRecursive(bone);
            }
            if(!boneExists)
            {
                CharacterAPI.logger.LogWarning($"Character {characterName} doesn't have transform \"{bone}\". Skipping this character...");
            }

            return boneExists && bonesFound;
        }
    }
}
