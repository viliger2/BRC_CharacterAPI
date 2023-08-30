using Reptile;
using UnityEngine;
using UnityEngine.Playables;
using static CharacterAPI.ExtensionMethods.CharacterSelectExtensions;

namespace CharacterAPI.ExtensionMethods
{
    public static class CharacterSelectCharacterExtensions
    {
        public static void Init(this CharacterSelectCharacter instance, CharacterSelect SetCharacterSelect, SelectableCharacterWithMods setCharacter, GameObject collider, GameObject talkTrigger, Vector3 centerPosition, PlayableDirector swapCharacterSequence)
        {
            // TODO: CharacterSelectCharacter keeps Characters enum and it is used extensively to check for existing NPC (I guess so you won't encounter your double)
            // and for some animation stuff when you select the character
            instance.Init(SetCharacterSelect, setCharacter.characterEnum, collider, talkTrigger, centerPosition, swapCharacterSequence);
            instance.freeStyleHash = setCharacter.moddedCharacter.freestyleHash;
        }
    }
}
