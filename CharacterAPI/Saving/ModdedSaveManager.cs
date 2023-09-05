using Reptile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CharacterAPI.Saving
{
    public class ModdedSaveManager : IDisposable, IInterruptable
    {
        private SaveManager saveManager;

        private List<ATransaction> saveTransactions;

        private List<ATransaction> loadTransactions;

        private ModdedSaveSlotHandler moddedSaveSlotHandler;

        private Task currentSaveTask;

        private CancellationTokenSource cancellationTokenSource;

        public ModdedCharacterProgressData CurrentSaveSlot => moddedSaveSlotHandler.CurrentSaveSlot;

        public ModdedSaveManager(AStorage storage, AErrorHandler errorHandler, SaveManager saveManager)
        {
            this.saveManager = saveManager;
            saveTransactions = new List<ATransaction>();
            loadTransactions = new List<ATransaction>();
            this.moddedSaveSlotHandler = new ModdedSaveSlotHandler(storage);
        }

        public void Dispose()
        {
            this.saveManager = null;
            this.moddedSaveSlotHandler.Dispose();
            this.moddedSaveSlotHandler = null;
            saveTransactions.Clear();
            loadTransactions.Clear();
        }

        public void InterruptWork()
        {
            StopSaveFileDelayTask();
        }

        public ATransaction SaveModdedCharactersData()
        {
            ATransaction aTransaction = moddedSaveSlotHandler.SaveModdedCharacterSaveSlot(saveManager.saveSlotSettings.currentSaveSlot);
            return aTransaction;
        }

        public TransactionsYieldInstruction LoadModdedCharactersData(DirectoryInfoTransaction directoryInfoTransaction)
        {
            TransactionsYieldInstruction transactionsYieldInstruction = moddedSaveSlotHandler.LoadSaveSlotsFromFile(directoryInfoTransaction);
            AddLoadTransactions(transactionsYieldInstruction);
            return transactionsYieldInstruction;
        }

        public void AddSaveTransactionToTracker(ATransaction transaction)
        {
            AddSaveTransaction(transaction);
            transaction.OnTransactionDone += OnSaveTransactionDone;
        }

        public void AddLoadTransactionToTracker(ATransaction transaction)
        {
            AddLoadTransaction(transaction);
            transaction.OnTransactionDone += OnLoadTransactionDone;
        }

        public DirectoryInfoTransaction LoadSaveSlotDirectoryInfo()
        {
            DirectoryInfoTransaction directoryInfoTransaction = moddedSaveSlotHandler.LoadSaveSlotDirectoryInfo();
            AddLoadTransactionToTracker(directoryInfoTransaction);
            return directoryInfoTransaction;
        }

        public void AddSaveSlotData(ModdedCharacterProgressData saveSlotData, string fileName)
        {
            moddedSaveSlotHandler.AddCharacterProgress(saveSlotData, fileName);
        }

        public void SetCurrentSaveSlotFromSettings()
        {
            moddedSaveSlotHandler.SetCurrentSaveSlotDataBySlotId(saveManager.saveSlotSettings.currentSaveSlot);
        }

        private void AddLoadTransaction(ATransaction loadTransaction)
        {
            loadTransactions.Add(loadTransaction);
        }

        private void OnSaveTransactionDone(ATransaction saveTransaction)
        {
            saveTransaction.OnTransactionDone -= OnSaveTransactionDone;
            RemoveSaveTransaction(saveTransaction);
            if (saveTransaction.Status == ATransaction.TransactionStatus.RETRY)
            {
                RetryPreviousSaveTransaction(saveTransaction);
            }
            else if (saveTransaction.HasError)
            {
                HandleSaveTransactionError(saveTransaction);
            }
        }

        private void AddSaveTransaction(ATransaction saveTransaction)
        {
            saveTransactions.Add(saveTransaction);
        }

        private void RemoveSaveTransaction(ATransaction saveTransaction)
        {
            saveTransactions.Remove(saveTransaction);
        }

        private void HandleSaveTransactionError(ATransaction saveTransaction)
        {
            IError error = saveManager.errorHandler.HandleSaveTransactionError(saveTransaction, saveManager.uiManager.Overlay);
            if (error != null)
            {
                error.OnErrorHandlerFinished += OnSaveErrorHandlingFinished;
            }
        }

        private void OnSaveErrorHandlingFinished(IError error)
        {
            error.OnErrorHandlerFinished -= OnSaveErrorHandlingFinished;
            if (error is ITransactionError)
            {
                ITransactionError transactionError = (ITransactionError)error;
                if (error.ErrorHandlingResult == ErrorHandlingResult.RETRY)
                {
                    RetryPreviousSaveTransaction(transactionError.Transaction);
                }
            }
        }

        private void RetryPreviousSaveTransaction(ATransaction saveTransaction)
        {
            saveTransaction.ScheduleForReset();
            saveManager.storage.EnqueueTransaction(saveTransaction);
            AddSaveTransactionToTracker(saveTransaction);
        }

        private void StopSaveFileDelayTask()
        {
            if (currentSaveTask != null && !currentSaveTask.IsCompleted)
            {
                cancellationTokenSource.Cancel();
            }
            currentSaveTask = null;
            cancellationTokenSource = null;
        }
       
        private void AddLoadTransactions(IMultiTransactionHandler multiTransactionHandler)
        {
            int transactionsCount = multiTransactionHandler.GetTransactionsCount();
            for (int i = 0; i < transactionsCount; i++)
            {
                ATransaction transaction = multiTransactionHandler.GetTransaction(i);
                AddLoadTransactionToTracker(transaction);
            }
        }

        private void OnLoadTransactionDone(ATransaction loadTransaction)
        {
            loadTransaction.OnTransactionDone -= OnLoadTransactionDone;
            RemoveLoadTransaction(loadTransaction);
        }

        private void RemoveLoadTransaction(ATransaction loadTransaction)
        {
            loadTransactions.Remove(loadTransaction);
        }

    }
}
