using Reptile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.TextCore.Text;
using static CharacterAPI.CharacterAPI;

namespace CharacterAPI.Hooks
{
    internal class CharacterSelectCharacterHooks
    {
        public static void InitHooks()
        {
            On.Reptile.CharacterSelectCharacter.Init += CharacterSelectCharacter_Init;
        }

        private static void CharacterSelectCharacter_Init(On.Reptile.CharacterSelectCharacter.orig_Init orig, Reptile.CharacterSelectCharacter self, Reptile.CharacterSelect SetCharacterSelect, Reptile.Characters setCharacter, UnityEngine.GameObject collider, UnityEngine.GameObject talkTrigger, UnityEngine.Vector3 centerPosition, UnityEngine.Playables.PlayableDirector swapCharacterSequence)
        {
            orig(self, SetCharacterSelect, setCharacter, collider, talkTrigger, centerPosition, swapCharacterSequence);
            if (!Enum.IsDefined(typeof(Characters), setCharacter))
            {
                ModdedCharacter moddedCharacter = CharacterAPI.GetModdedCharacter(setCharacter);
                self.freeStyleHash = moddedCharacter.freestyleHash;
            }       
        }
    }
}
