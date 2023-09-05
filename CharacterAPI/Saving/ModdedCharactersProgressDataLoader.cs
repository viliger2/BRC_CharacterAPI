using Reptile;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharacterAPI.Saving
{
    public class ModdedCharactersProgressDataLoader : AUserSaveDataLoader
    {
        private ModdedSaveManager moddedSaveManager;

        //public ModdedCharactersProgressDataLoader(SaveManager saveManager, AStorage storage, AErrorHandler errorHandler, UIManager uIManager, IGameTextLocalizer localizer, ModdedSaveManager moddedSaveManager)
        //{
        //    this.saveManager = saveManager;
        //    this.moddedSaveManager = moddedSaveManager;
        //    this.storage = storage;
        //    this.uIManager = uIManager;
        //    this.localizer = localizer;
        //}

        public ModdedCharactersProgressDataLoader(SaveManager saveManager, AStorage storage, AErrorHandler errorHandler, UIManager uIManager, IGameTextLocalizer localizer, ModdedSaveManager moddedSaveManager) : base(saveManager, storage, errorHandler, uIManager, localizer)
        {
            this.moddedSaveManager = moddedSaveManager;
        }

        public override IEnumerator LoadSaveDataASync()
        {
            yield return LoadModdedCharactersProgressAsync();
        }

        private IEnumerator LoadModdedCharactersProgressAsync()
        {
            DirectoryInfoTransaction directoryInfoTransaction = moddedSaveManager.LoadSaveSlotDirectoryInfo();
            yield return LoadDirectoryInfoASync(directoryInfoTransaction, base.HandleLoadUserSaveDataErrorASync, base.LoadDirectoryInfoASync);
            if(isSuccess)
            {
                yield return LoadUserSaveSlotsFromDirectoryInfoASync(directoryInfoTransaction);
            }
        }

        private IEnumerator LoadUserSaveSlotsFromDirectoryInfoASync(DirectoryInfoTransaction directoryInfoTransaction)
        {
            TransactionsYieldInstruction loadSaveSlotsTransaction = moddedSaveManager.LoadModdedCharactersData(directoryInfoTransaction);
            yield return loadSaveSlotsTransaction;
            yield return OnLoadUserSaveSlotsDoneASync(loadSaveSlotsTransaction);
        }

        private IEnumerator OnLoadUserSaveSlotsDoneASync(TransactionsYieldInstruction loadSaveSlotsYieldInstruction)
        {
            int transactionCount = loadSaveSlotsYieldInstruction.GetTransactionsCount();
            for (int i = 0; i < transactionCount; i++)
            {
                ATransaction loadSaveSlotTransaction = loadSaveSlotsYieldInstruction.GetTransaction(i);
                if (loadSaveSlotTransaction is LoadTransaction<ModdedCharacterProgressData>)
                {
                    while (loadSaveSlotTransaction.Status == ATransaction.TransactionStatus.RETRY)
                    {
                        yield return RetryInPlace(loadSaveSlotTransaction);
                    }
                    yield return HandleLoadUserSaveSlotFileTransactionASync(loadSaveSlotTransaction);
                }
            }
        }

        private IEnumerator RetryInPlace(ATransaction transaction)
        {
            transaction.ScheduleForReset();
            moddedSaveManager.AddLoadTransactionToTracker(transaction);
            storage.EnqueueTransaction(transaction);
            yield return new TransactionYieldInstruction(transaction, storage);
        }

        private IEnumerator HandleLoadUserSaveSlotFileTransactionASync(ATransaction transaction)
        {
            LoadTransaction<ModdedCharacterProgressData> loadTransaction = transaction as LoadTransaction<ModdedCharacterProgressData>;
            if (loadTransaction.IsSuccess)
            {
                HandleLoadUserSaveSlotSuccess(loadTransaction);
            }
            else
            {
                yield return HandleLoadUserSaveSlotErrorASync(loadTransaction);
            }
        }

        private void HandleLoadUserSaveSlotSuccess(LoadTransaction<ModdedCharacterProgressData> loadSaveSlotTransaction)
        {
            ModdedCharacterProgressData data = loadSaveSlotTransaction.Data;
            moddedSaveManager.AddSaveSlotData(data, loadSaveSlotTransaction.FileName);
            moddedSaveManager.SetCurrentSaveSlotFromSettings();
        }

        private IEnumerator HandleLoadUserSaveSlotErrorASync(LoadTransaction<ModdedCharacterProgressData> loadSaveSlotTransaction)
        {
            if (loadSaveSlotTransaction.Status != ATransaction.TransactionStatus.FILE_NOT_FOUND)
            {
                yield return HandleLoadUserSaveDataErrorASync(loadSaveSlotTransaction, base.DoTransactionASync, base.HandleLoadUserSaveDataErrorASync);
            }
        }
    }
}
