﻿using CharacterAPI.ExtensionMethods;
using MonoMod.Cil;
using Reptile;
using System;

namespace CharacterAPI.Hooks
{
    // TODO: implement custom character saving, maybe make it a separate file?
    public class SaveSlotDataHooks
    {
        public static void InitHooks()
        {
            On.Reptile.SaveSlotData.GetCharacterProgress += SaveSlotData_GetCharacterProgress;
            On.Reptile.SaveSlotData.Read += SaveSlotData_Read;
            IL.Reptile.SaveSlotData.Write += SaveSlotData_Write;
        }

        private static void SaveSlotData_Write(MonoMod.Cil.ILContext il)
        {
            ILCursor c = new ILCursor(il);
            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchLdarg(out _),
                x => x.MatchLdarg(out _),
                x => x.MatchLdflda<Reptile.SaveSlotData>("currentCharacter")))
            {
                c.Index += 2;
                c.RemoveRange(4);
                c.EmitDelegate<Action<System.IO.BinaryWriter, Reptile.SaveSlotData>>((bw, ssd) =>
                {
                    Characters character = ssd.currentCharacter;
                    if (Enum.IsDefined(typeof(Characters), character))
                    {
                        bw.Write(character.ToString());
                    }
                    else
                    {
                        bw.Write(Characters.metalHead.ToString());
                    }
                });
            }
            else
            {
                CharacterAPI.logger.LogError("SaveSlotData::Write hook failed.");
            }

        }

        private static void SaveSlotData_Read(On.Reptile.SaveSlotData.orig_Read orig, SaveSlotData self, System.IO.BinaryReader reader)
        {
            orig(self, reader);
            if (!Enum.IsDefined(typeof(Characters), self.currentCharacter))
            {
                self.currentCharacter = Reptile.Characters.metalHead;
            }
        }

        private static Reptile.CharacterProgress SaveSlotData_GetCharacterProgress(On.Reptile.SaveSlotData.orig_GetCharacterProgress orig, Reptile.SaveSlotData self, Reptile.Characters character)
        {
            if (Enum.IsDefined(typeof(Characters), character))
            {
                return orig(self, character);
            }

            var progress = ModdedCharacterProgress.GetModdedCharacterProgress(character);
            if (progress == null)
            {
                var characterWithMods = CharacterSelectExtensions.GetCharacterWithMods(character);
                return ModdedCharacterProgress.NewModdedCharacterProgress(character, characterWithMods.moddedCharacter.defaultOutfit, characterWithMods.moddedCharacter.defaultMoveStyle, 0);
            }
            return progress;
        }
    }
}