using Reptile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace CharacterAPI
{
    public class ModdedCharacterConstructor : IDisposable
    {
        private const int CHARACTER_STARTING_VALUE = (int)Characters.MAX + 1;
        private const int VOICE_STARTING_VALUE = (int)SfxCollectionID.MAX + 1;

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

        /// <summary>
        /// Values for freestyleType.
        /// </summary>
        public enum FreestyleType
        {
            freestyle1, freestyle2, freestyle3, freestyle4, freestyle5, freestyle6, freestyle7, freestyle8, freestyle9, freestyle10, freestyle11, freestyle12, freestyle13, freestyle14, freestyle15, freestyle16, freestyle17, freestyle18, freestyle19
        }

        /// <summary>
        /// Values for bounceType. Yes, there are 15 softBounces and one normal bounce.
        /// </summary>
        public enum BounceType
        {
            bounce, softbounce1, softbounce2, softbounce3, softbounce4, softbounce5, softbounce6, softbounce7, softbounce8, softbounce9, softbounce10, softbounce11, softbounce12, softbounce13, softbounce14, softbounce15
        }

        /// <summary>
        /// Compatability field, used for loading ModdedCharacters characters.
        /// </summary>
        public enum Skate
        {
            Left, Right
        }

        /// <summary>
        /// Character name. Required field.
        /// </summary>
        public string? characterName;

        /// <summary>
        /// Character prefab that was made in Unity Editor. It will be checked for nessesary bones and will be skipped if it doesn't have them. Required field.
        /// </summary>
        public GameObject? characterPrefab;

        /// <summary>
        /// Character's default outfit. Can be values from 0 to 3. Optional field, default value is 0.
        /// </summary>
        public int defaultOutfit = 0;

        /// <summary>
        /// Character's default movestyle. Can be SKATEBOARD, BMX or INLINE. Optional field, default value is SKATEBOARD.
        /// </summary>
        public MoveStyle defaultMoveStyle = MoveStyle.SKATEBOARD;

        /// <summary>
        /// Character's audio base. Used when character doesn't have custom voice. Optional field, default value is metalHead.
        /// </summary>
        public Characters tempAudioBase = Characters.metalHead;

        /// <summary>
        /// Character's personal graffiti base. Used when character doesn't have custom graffiti. Optional field, default value is metalHead.
        /// </summary>
        public Characters personalGraffitiBase = Characters.metalHead;

        /// <summary>
        /// Freestyle is a dance that character does when NOT picked and left to dance on cypher. Optional field, default value is freestyle1
        /// </summary>
        public FreestyleType freestyleType = FreestyleType.freestyle1;

        /// <summary>
        /// Bounce is a small animation that character does on character select screen. Optional field, default value is bounce.
        /// </summary>
        public BounceType bounceType = BounceType.bounce;

        /// <summary>
        /// Whether character can blink or not. If set to true character's blend shapes will be checked and if it doesn't have enough of them then it will be set back to false. Optional field, default value is false.
        /// </summary>
        public bool canBlink = false;

        /// <summary>
        /// List of all sound effects for the character. Sound clips need to be appropriately named so they can be separated into correct arrays. Optional field.
        /// </summary>
        public List<AudioClip> audioClips = new List<AudioClip>();

        /// <summary>
        /// Whether character uses custom shaders. Setting this to true will disable shader replacement with in-game shader. Optional field, default value is false
        /// </summary>
        public bool usesCustomShader = false;

        /// <summary>
        /// Can be used to skip transforms check. Only use if you know what you are doing, otherwise badly made model will crash the game.
        /// </summary>
        public bool skipTransformsCheck = false;

        private List<Outfit> outfits = new List<Outfit>();

        private PersonalGraffiti personalGraffiti;

        private ModdedCharacter.SkatesPosition positionL;

        private ModdedCharacter.SkatesPosition positionR;

        // oh god, getting all graffiti names from GraffitiArt.Titles via reflection
        private static FieldInfo[] graffitiNamesFieldInfos = typeof(Reptile.GraffitiArt.Titles).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).Where(fi => fi.IsLiteral && !fi.IsInitOnly).ToArray();

        /// <summary>
        /// Adds additional outfit to character. Character should have at least one outfit and can't have more than 4.
        /// </summary>
        /// <param name="material">Unity Material containing texture that would count as outfit.</param>
        /// <param name="name">Outfit name, can be left blank to use in-game naming scheme of seasons.</param>
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

        /// <summary>
        /// Adds personal graffiti to character,
        /// </summary>
        /// <param name="name">Graffiti's name. It will be shown in bottom left corner when graffiti is painted.</param>
        /// <param name="author">Graffiti's author. Personal graffiti don't really use author's name for anything, but we still fill it just to be safe.</param>
        /// <param name="material">Graffiti's material. Material's shader will be replaced with in-game shader for graffiti.</param>
        /// <param name="texture">Graffiti's texture. Just like with author we don't really need it, since the texture is only seen on the phone and personal graffiti are not shown there, but we fill it to not crash the game.</param>
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
        
        /// <summary>
        /// Creates modded character. Before attemting to create it multiple checks will occur whether required fields are filled and whether model is compatible with the game.
        /// </summary>
        /// <returns>Boolean - whether character was created or not.</returns>
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

            //if (positionL == null && positionR != null)
            //{
            //    CharacterAPI.logger.LogWarning($"Character {characterName} hsa set right skate position but not left.");
            //} else if (positionL != null && positionR == null)
            //{
            //    CharacterAPI.logger.LogWarning($"Character {characterName} hsa set left skate position but not right.");
            //}

            if (!CheckForTransforms() && !skipTransformsCheck)
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
            newCharacter.usesCustomShader = usesCustomShader;

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
                newCharacter.audioClips = new List<AudioClip>(audioClips);
            }
            else
            {
                newCharacter.voiceId = SfxCollectionID.NONE;
            }

            //if (positionL != null && positionR != null)
            //{
            //    newCharacter.positionL = positionL;
            //    newCharacter.positionR = positionR;
            //}

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

        public void AddSkatePosition(Skate skate, Vector3 position, Vector3 rotation, Vector3 scale)
        {
            AddSkatePosition(skate, position, Quaternion.Euler(rotation), scale);
        }

        public void AddSkatePosition(Skate skate, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            ModdedCharacter.SkatesPosition tempPosition = new ModdedCharacter.SkatesPosition();
            tempPosition.position = position;
            tempPosition.rotation = rotation;
            tempPosition.scale = scale;

            if (skate == Skate.Left)
            {
                positionL = tempPosition;
            }
            else
            {
                positionR = tempPosition;
            }
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

            //if(!animator.transform.Find("mesh"))
            //{
            //    CharacterAPI.logger.LogWarning($"Character {characterName} doesn't have mesh named \"mesh\" at prefab's base. Skipping this character...");
            //    return false;
            //}

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

        public void Dispose()
        {
            audioClips.Clear();
            outfits.Clear();
        }
    }
}
