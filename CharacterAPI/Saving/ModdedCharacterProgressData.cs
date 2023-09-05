using Reptile;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharacterAPI.Saving
{
    public class ModdedCharacterProgressData : ISaveable
    {
        public struct ModdedCharacterProgress1
        {
            public CharacterProgress characterProgress;

            public int moddedCharacterHash;
        }

        public int version = 1;

        private List<ModdedCharacterProgress1> totalModdedCharacterProgress;

        //public CharacterProgress moddedCharacterProgress;

        //public int moddedCharacterHash;

        public int saveSlotId;

        public ModdedCharacterProgressData() 
        {
            totalModdedCharacterProgress = new List<ModdedCharacterProgress1>();
        }

        public void Read(BinaryReader reader)
        {
            version = reader.ReadInt32();
            if (version >= 0)
            {
                int count = reader.ReadInt32();
                for(int i = 0; i < count; i++)
                {
                    ModdedCharacterProgress1 newChar = new ModdedCharacterProgress1();
                    newChar.moddedCharacterHash = reader.ReadInt32();
                    newChar.characterProgress = new CharacterProgress();
                    newChar.characterProgress.Read(reader);
                }
            }
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(version);
            writer.Write(totalModdedCharacterProgress.Count);
            foreach(ModdedCharacterProgress1 progress in totalModdedCharacterProgress)
            {
                writer.Write(progress.moddedCharacterHash);
                progress.characterProgress.Write(writer);
            }
        }

        public ModdedCharacterProgress1 GetCharacterProgress(int hash)
        {
            return totalModdedCharacterProgress.Find(x => x.moddedCharacterHash == hash);
            //if(saveData.Equals(default(ModdedCharacterProgress1)))
            //{
            //    saveData.moddedCharacterHash = hash;
            //    saveData.characterProgress = new CharacterProgress();
            //}

            //saveData.characterProgress.character = character;

            //return saveData;
        }

        public ModdedCharacterProgress1 CreateNewModdedCharacterProgress(int hash, Characters character, bool unlocked, int outfit, MoveStyle moveStyle, int moveStyleSkin)
        {
            ModdedCharacterProgress1 progress = new ModdedCharacterProgress1();
            progress.moddedCharacterHash = hash;
            progress.characterProgress = new CharacterProgress();
            progress.characterProgress.unlocked = unlocked;
            progress.characterProgress.character = character;
            progress.characterProgress.outfit = outfit;
            progress.characterProgress.moveStyle = moveStyle;
            progress.characterProgress.moveStyleSkin = moveStyleSkin;

            totalModdedCharacterProgress.Add(progress);

            return progress;
        }
    }
}
