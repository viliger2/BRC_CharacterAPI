using Reptile;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CharacterAPI
{
    public class ModdedCharacterProgress
    {
        public const int MODDED_CHARACTER_SAVE_VERSION = 1;
        public const string MODDED_CHARACTER_SAVE_FOLDER = "CharacterAPISaves";
        public const string MODDED_CHARACTER_SAVE_FILE_NAME = "ModdedCharactersProgress.capi";

        public class ModdedSaveSlot
        {
            public List<ModdedCharacterProgressStruct> moddedCharacterProgress = new List<ModdedCharacterProgressStruct>();
            public int lastPlayedCharacter;

            public ModdedCharacterProgressStruct GetModdedCharacterProgress(int hash)
            {
                return moddedCharacterProgress.Find(x => x.characterHash == hash);
            }
        }

        public class ModdedCharacterProgressStruct
        {
            public CharacterProgress characterProgress = new CharacterProgress();
            public int characterHash;
        }

        private static Dictionary<int, ModdedSaveSlot> moddedSaveSlots = new Dictionary<int, ModdedSaveSlot>();

        public static ModdedCharacterProgressStruct? GetCharacterProgress(int saveSlotId, int hash)
        {
            var result = GetModdedSaveSlot(saveSlotId);
            if(result != null)
            {
                return result.GetModdedCharacterProgress(hash);
            }
            return null;
        }

        public static ModdedCharacterProgressStruct CreateNewModdedCharacterProgress(int saveSlotId, int hash, Characters character, bool unlocked, int outfit, MoveStyle moveStyle, int moveStyleSkin)
        {
            var saveSlot = GetModdedSaveSlot(saveSlotId);
            if(saveSlot == null)
            {
                saveSlot = CreateNewModdedSaveSlot(saveSlotId, hash);
            }

            ModdedCharacterProgressStruct progress = new ModdedCharacterProgressStruct();
            progress.characterHash = hash;
            if (progress.characterProgress == null)
            {
                progress.characterProgress = new CharacterProgress();
            }
            progress.characterProgress.unlocked = unlocked;
            progress.characterProgress.character = character;
            progress.characterProgress.outfit = outfit;
            progress.characterProgress.moveStyle = moveStyle;
            progress.characterProgress.moveStyleSkin = moveStyleSkin;

            saveSlot.moddedCharacterProgress.Add(progress);

            return progress;
        }

        public static ModdedSaveSlot? GetModdedSaveSlot(int saveSlotId)
        {
            if(moddedSaveSlots.TryGetValue(saveSlotId, out var result))
            {
                return result;
            };
            return null;
        }

        public static ModdedSaveSlot CreateNewModdedSaveSlot(int saveSlotId, int lastPlayerCharacter = -1)
        {
            var saveSlot = new ModdedSaveSlot();
            if (saveSlot.moddedCharacterProgress == null)
            {
                saveSlot.moddedCharacterProgress = new List<ModdedCharacterProgressStruct>();
            }
            saveSlot.lastPlayedCharacter = lastPlayerCharacter;

            moddedSaveSlots.Add(saveSlotId, saveSlot);

            return saveSlot;
        }

        public static void SetLastPlayedCharacter(int saveSlotId, int lastPlayerCharacter)
        {
            var saveSlot = GetModdedSaveSlot(saveSlotId);
            if (saveSlot == null)
            {
                saveSlot = CreateNewModdedSaveSlot(saveSlotId, lastPlayerCharacter);
            }

            saveSlot.lastPlayedCharacter = lastPlayerCharacter;
        }

        public static void PerformSaveCleanUp()
        {
            foreach(var saveSlot in moddedSaveSlots)
            {
                foreach(var characterProgress in saveSlot.Value.moddedCharacterProgress)
                {
                    var moddedCharacter = CharacterAPI.GetModdedCharacter(characterProgress.characterHash);
                    if (moddedCharacter == null)
                    {
                        saveSlot.Value.moddedCharacterProgress.Remove(characterProgress);
                    }
                }
                if(saveSlot.Value.moddedCharacterProgress.Count == 0)
                {
                    moddedSaveSlots.Remove(saveSlot.Key);
                }
            }
        }

        public static void PerformSaveValidation()
        {
            foreach (var saveSlot in moddedSaveSlots)
            {
                foreach (var characterProgress in saveSlot.Value.moddedCharacterProgress)
                {
                    var moddedCharacter = CharacterAPI.GetModdedCharacter(characterProgress.characterHash);
                    if (moddedCharacter == null)
                    {
                        characterProgress.characterProgress.character = Characters.NONE;
                    }
                    else
                    {
                        characterProgress.characterProgress.character = moddedCharacter.characterEnum;
                    }
                }
            }
        }

        public static async void SaveAsync()
        {
            string filePath = Path.Combine(CharacterAPI.SavePath, MODDED_CHARACTER_SAVE_FOLDER, MODDED_CHARACTER_SAVE_FILE_NAME);

            try
            {
                // we don't need to check, create directory does nothing if it exists
                Directory.CreateDirectory(Path.Combine(CharacterAPI.SavePath, MODDED_CHARACTER_SAVE_FOLDER));

                using (var stream = File.Open(filePath, FileMode.Create))
                {
                    using (var writer = new BinaryWriter(stream))
                    {
                        writer.Write(MODDED_CHARACTER_SAVE_VERSION);
                        writer.Write(moddedSaveSlots.Count);
                        foreach (var moddedSaveSlot in moddedSaveSlots)
                        {
                            writer.Write(moddedSaveSlot.Key);
                            writer.Write(moddedSaveSlot.Value.lastPlayedCharacter);
                            writer.Write(moddedSaveSlot.Value.moddedCharacterProgress.Count);
                            foreach (var moddedCharacterProgress in moddedSaveSlot.Value.moddedCharacterProgress)
                            {
                                writer.Write(moddedCharacterProgress.characterHash);
                                moddedCharacterProgress.characterProgress.Write(writer);
                            }
                        }
                        writer.Close();
                    }
                    stream.Close();
                }
            }
            catch (Exception e)
            {
                CharacterAPI.logger.LogError($"Exception during modded character save creation. Exception: {e}, Message: {e.Message}.");
                // we don't need half written, damaged file
                File.Delete(filePath);
            }
        }

        public static async void LoadAsync()
        {
            string filePath = Path.Combine(CharacterAPI.SavePath, MODDED_CHARACTER_SAVE_FOLDER, MODDED_CHARACTER_SAVE_FILE_NAME);

            try
            {
                if (File.Exists(filePath))
                {
                    int charactersCount = 0;
                    int saveSlotIdCount = 0;
                    using (var stream = File.OpenRead(filePath))
                    {
                        using (var reader = new BinaryReader(stream))
                        {
                            int version = reader.ReadInt32();
                            if (version < 1)
                            {
                                return;
                            }

                            int dictionaryCount = reader.ReadInt32();
                            for (int i = 0; i < dictionaryCount; i++)
                            {
                                int saveSlotId = reader.ReadInt32();

                                ModdedSaveSlot moddedSaveSlot = new ModdedSaveSlot();
                                moddedSaveSlot.lastPlayedCharacter = reader.ReadInt32();
                                int listCount = reader.ReadInt32();
                                if (moddedSaveSlot.moddedCharacterProgress == null)
                                {
                                    moddedSaveSlot.moddedCharacterProgress = new List<ModdedCharacterProgressStruct>();
                                }
                                for (int k = 0; k < listCount; k++)
                                {
                                    int characterHash = reader.ReadInt32();
                                    CharacterProgress characterProgress = new CharacterProgress();
                                    characterProgress.Read(reader);
                                    moddedSaveSlot.moddedCharacterProgress.Add(new ModdedCharacterProgressStruct { characterHash = characterHash, characterProgress = characterProgress });
                                    charactersCount++;
                                }
                                moddedSaveSlots.Add(saveSlotId, moddedSaveSlot);
                                saveSlotIdCount++;
                            }
                            reader.Close();
                        }
                        stream.Close();
                    }
                    CharacterAPI.logger.LogMessage($"Successfully loaded {saveSlotIdCount} saves with {charactersCount} characters total between them.");
                }
            }
            catch (Exception e)
            {
                CharacterAPI.logger.LogError($"Exception during modded character save reading. Exception: {e}, Message: {e.Message}.");
                // we don't need damaged file
                File.Delete(filePath);
            }
        }

    }
}
