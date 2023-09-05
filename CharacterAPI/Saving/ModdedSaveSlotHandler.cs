using Reptile;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharacterAPI.Saving
{
    public class ModdedSaveSlotHandler : IDisposable
    {
        public const string MODDED_SAVE_FILE_NAME = "CharacterAPIModdedCharacterProgress";

        public const string MODDED_SAVE_FILE_EXT = ".capi";

        public const string MODDED_SAVE_SLOT_FORMAT = "CharacterAPIModdedCharacterProgress{0}.capi";

        public const string MODDED_SAVE_DIRECTORY = "ModdedCharactersSaveData";

        private List<string> directorySaveSlotFileNames;

        private Dictionary<int, string> loadedSaveSlotFileNames;

        private AStorage storage;

        private List<ModdedCharacterProgressData> saveSlots;

        private ModdedCharacterProgressData currentSaveSlot;

        public ModdedCharacterProgressData CurrentSaveSlot => currentSaveSlot;

        public int SaveSlotCount => saveSlots.Count;

        public ModdedSaveSlotHandler(AStorage storage) 
        {
            this.storage = storage;
            saveSlots = new List<ModdedCharacterProgressData>();
            loadedSaveSlotFileNames = new Dictionary<int, string>();
            directorySaveSlotFileNames= new List<string>();
        }

        public void Dispose()
        {
            storage = null;
            saveSlots = null;
            currentSaveSlot = null;
            directorySaveSlotFileNames.Clear();
            directorySaveSlotFileNames = null;
            loadedSaveSlotFileNames.Clear();
            loadedSaveSlotFileNames = null;
        }

        public void AddNewModdedCharacterSaveSlot(int saveSlotId)
        {
            ModdedCharacterProgressData newData = new ModdedCharacterProgressData { saveSlotId = saveSlotId };
            string text = GenerateNewSaveSlotFileName(saveSlotId);
            directorySaveSlotFileNames.Add(text);
            AddNewSaveSlotData(newData, text);
        }

        public LoadTransaction<ModdedCharacterProgressData> LoadSaveSlot(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return null;
            }

            ModdedCharacterProgressData data = new ModdedCharacterProgressData();
            return storage.LoadSaveSlotFromStorage(data, fileName, MODDED_SAVE_DIRECTORY, DataSerializerFactory.FormatType.COMPRESSED_GAME_DATA);
        }

        public ATransaction SaveModdedCharacterSaveSlot(int saveSlotId)
        {
            if(saveSlotId <= -1)
            {
                return null;
            }

            string saveSlotFileName = GetSaveSlotFileName(saveSlotId);
            if(string.IsNullOrEmpty(saveSlotFileName))
            {
                return null;
            }

            return storage.SaveDataAsSaveSlotAtStorage(currentSaveSlot, saveSlotFileName, MODDED_SAVE_DIRECTORY, DataSerializerFactory.FormatType.COMPRESSED_GAME_DATA);
        }

        public DirectoryInfoTransaction LoadSaveSlotDirectoryInfo()
        {
            return storage.GetSlotsInfoFromStorage("SaveData");
        }

        public TransactionsYieldInstruction LoadSaveSlotsFromFile(DirectoryInfoTransaction directoryInfoTransaction)
        {
            GetExistingSaveSlotNames(directoryInfoTransaction);
            ATransaction[] array = new ATransaction[directorySaveSlotFileNames.Count];
            for (int i = 0; i < directorySaveSlotFileNames.Count; i++)
            {
                array[i] = LoadSaveSlot(directorySaveSlotFileNames[i]);
            }
            return new TransactionsYieldInstruction(array, storage);
        }

        public ModdedCharacterProgressData GetModdedCharacterProgressData(int saveSlotId)
        {
            for (int i = 0; i < saveSlots.Count; i++)
            {
                if (saveSlots[i].saveSlotId == saveSlotId)
                {
                    return saveSlots[i];
                }
            }
            return null;
        }

        public void SetCurrentSaveSlotDataBySlotId(int saveSlotId)
        {
            currentSaveSlot = GetModdedCharacterProgressData(saveSlotId);
        } 

        public void AddCharacterProgress(ModdedCharacterProgressData moddedCharacterData, string saveSlotFileName)
        {
            AddModdedCharacterProgressData(moddedCharacterData, saveSlotFileName);
        }

        // Reptile.SaveSlotHandler
        private void AddModdedCharacterProgressData(ModdedCharacterProgressData saveSlotData, string saveSlotFileName)
        {
            ModdedCharacterProgressData saveSlotData2 = GetModdedCharacterProgressData(saveSlotData.saveSlotId);
            if (saveSlotData2 == null)
            {
                AddNewSaveSlotData(saveSlotData, saveSlotFileName);
            }
        }

        private string GenerateNewSaveSlotFileName(int saveSlotId)
        {
            string text = string.Format(MODDED_SAVE_SLOT_FORMAT, saveSlotId);
            if (directorySaveSlotFileNames.Contains(text))
            {
                for (int i = saveSlotId + 1; i < 100; i++)
                {
                    text = $"GameProgress{i}.brp";
                    if (!directorySaveSlotFileNames.Contains(text))
                    {
                        break;
                    }
                }
            }
            return text;
        }
        
        private void AddNewSaveSlotData(ModdedCharacterProgressData newSaveSlotData, string saveSlotFileName)
        {
            AddSaveSlotFileName(newSaveSlotData.saveSlotId, saveSlotFileName);
            saveSlots.Add(newSaveSlotData);
        }

        private void AddSaveSlotFileName(int saveSlotId, string saveSlotFileName)
        {
            if (loadedSaveSlotFileNames.ContainsKey(saveSlotId))
            {
                loadedSaveSlotFileNames[saveSlotId] = saveSlotFileName;
            }
            else
            {
                loadedSaveSlotFileNames.Add(saveSlotId, saveSlotFileName);
            }
        }

        private string GetSaveSlotFileName(int saveSlotId)
        {
            if (loadedSaveSlotFileNames.ContainsKey(saveSlotId))
            {
                return loadedSaveSlotFileNames[saveSlotId];
            }
            return string.Empty;
        }

        private void GetExistingSaveSlotNames(DirectoryInfoTransaction directoryInfoTransaction)
        {
            directorySaveSlotFileNames.Clear();
            string[] directoryFileNames = directoryInfoTransaction.DirectoryFileNames;
            for (int i = 0; i < directoryFileNames.Length; i++)
            {
                string fileName = Path.GetFileName(directoryFileNames[i]);
                string extension = Path.GetExtension(directoryFileNames[i]);
                if (fileName.Contains(MODDED_SAVE_FILE_NAME) && extension.Equals(MODDED_SAVE_FILE_EXT) && !fileName.Contains("_backup") && !fileName.Contains("Corrupted"))
                {
                    directorySaveSlotFileNames.Add(fileName);
                }
            }
        }

    }
}
