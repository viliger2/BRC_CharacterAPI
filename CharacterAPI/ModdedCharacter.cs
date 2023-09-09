using Reptile;
using System.Collections.Generic;
using UnityEngine;

namespace CharacterAPI
{
    internal class ModdedCharacter
    {
        internal static List<ModdedCharacter> ModdedCharacters = new List<ModdedCharacter>();

        public string Name;
        public string[] outfitNames;
        public Material[] loadedCharacterMaterials;
        public GameObject loadedCharacterFbxAssets;
        public GameObject characterVisual;
        public int defaultOutfit;
        public MoveStyle defaultMoveStyle;
        public Characters characterVoiceBase;
        public bool usePersonalGrafitti;
        public Reptile.GraffitiArt? personalGrafitti;
        public Characters characterGraffitiBase;
        public int freestyleHash;
        public int bounceHash;
        public Characters characterEnum;
        public SfxCollectionID voiceId;
        public List<AudioClip> audioClips = new List<AudioClip>();
        public bool canBlink;

        private int? Hash = null;

        public override int GetHashCode()
        {
            if (Hash == null)
            {
                unchecked // Overflow is fine, just wrap
                {
                    int hash = (int)2166136261;
                    // Suitable nullity checks etc, of course :)
                    hash = (hash * 16777619) + Name.GetHashCode();
                    foreach (string outfitName in outfitNames)
                    {
                        hash = (hash * 16777619) + outfitName.GetHashCode();
                    }
                    //hash = (hash * 16777619) + outfitNames.GetHashCode();
                    hash = (hash * 16777619) + usePersonalGrafitti.GetHashCode();
                    hash = (hash * 16777619) + defaultMoveStyle.GetHashCode();
                    hash = (hash * 16777619) + defaultOutfit.GetHashCode();
                    Hash = hash;
                }
            }

            return (int)Hash;
        }

        public static ModdedCharacter GetModdedCharacter(Characters character)
        {
            return ModdedCharacters.Find(x => x.characterEnum == character);
        }

        public static ModdedCharacter GetModdedCharacter(int hash)
        {
            return ModdedCharacters.Find(x => x.GetHashCode() == hash);
        }

    }
}
