using Domain.Entities;
using Domain.Interfaces;
using Domain.Repository_Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class WorkerService
    {
        private readonly IWorkerRepository _workerRepo;
        private readonly IRepository<WorkerPiece> _workerPieceRepo;
        private readonly BankService _bankService;
        private readonly IRepository<BankTransaction> _transactionRepo;
        private readonly IRepository<Bank> _bankRepo;

        public WorkerService(
            IWorkerRepository workerRepo,
            IRepository<WorkerPiece> workerPieceRepo,
            BankService bankService,
            IRepository<BankTransaction> transactionRepo,
            IRepository<Bank> bankRepo)
        {
            _workerRepo = workerRepo;
            _workerPieceRepo = workerPieceRepo;
            _bankService = bankService;
            _transactionRepo = transactionRepo;
            _bankRepo = bankRepo;
        }

        // ========================
        // === WORKER OPERATIONS ===
        // ========================

        public async Task AddWorkerAsync(Worker worker)
        {
            if (worker == null) throw new ArgumentNullException(nameof(worker));
            if (string.IsNullOrWhiteSpace(worker.Name)) throw new Exception("Worker name is required.");
            if (string.IsNullOrWhiteSpace(worker.Ph_Number)) throw new Exception("Worker phone number is required.");

            await _workerRepo.AddAsync(worker);
            await _workerRepo.SaveChangesAsync();
        }

        public async Task<IEnumerable<Worker>> GetAllWorkersAsync()
        {
            return await _workerRepo.GetAllWithDetailsAsync();
        }

        public async Task<Worker> GetWorkerByIdAsync(int id)
        {
            var workers = await _workerRepo.GetAllWithDetailsAsync();
            var worker = workers.FirstOrDefault(w => w.ID == id);

            if (worker == null)
                throw new Exception($"Worker with ID {id} not found.");

            return worker;
        }

        // ==========================
        // === PIECE (WORK ENTRY) ===
        // ==========================

        public async Task AddWorkerPieceAsync(Worker worker, int pieces)
        {
            if (worker == null || pieces <= 0)
                throw new Exception("Invalid worker or piece count.");

            var workerPiece = new WorkerPiece
            {
                WorkerID = worker.ID,
                Pieces = pieces,
                Date = DateTime.Now
            };

            await _workerPieceRepo.AddAsync(workerPiece);
            await _workerPieceRepo.SaveChangesAsync();

            worker.Price_Count += pieces;

            if (worker.DailyPieces == null)
                worker.DailyPieces = new ObservableCollection<WorkerPiece>();

            worker.DailyPieces.Add(workerPiece);

            _workerRepo.Update(worker);
            await _workerRepo.SaveChangesAsync();
        }

        // ==========================
        // === PAYMENT OPERATIONS ===
        // ==========================

        public async Task<Worker> AddWorkerPaymentAsync(Worker worker, decimal amount, string paymentType)
        {
            if (worker == null)
                throw new ArgumentNullException(nameof(worker));

            if (amount <= 0)
                throw new Exception("Amount must be greater than zero.");

            // Always OUTCOME (money leaving the bank)
            var bank = await _bankService.GetOrCreateBankAsync();

            if (bank.TotalAmount < amount)
                throw new Exception("Not enough money in the bank.");

            decimal newTotal = bank.TotalAmount - amount;

            var transaction = new BankTransaction
            {
                Date = DateTime.Now,
                Type = "خارج",
                Amount = amount,
                TotalAfterTransaction = newTotal,
                WorkerID = worker.ID,
                BankID = bank.ID
            };

            bank.TotalAmount = newTotal;

            await _transactionRepo.AddAsync(transaction);
            _bankRepo.Update(bank);

            await _transactionRepo.SaveChangesAsync();
            await _bankRepo.SaveChangesAsync();

            // Add to worker memory list
            if (worker.BankTransactions == null)
                worker.BankTransactions = new ObservableCollection<BankTransaction>();

            worker.BankTransactions.Add(transaction);

            return await GetWorkerByIdAsync(worker.ID);
        }
    }
}
