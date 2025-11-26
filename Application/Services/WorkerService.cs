using Domain.Entities;
using Domain.Interfaces;
using Domain.Repository_Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services
{
    public class WorkerService
    {
        private readonly IWorkerRepository _workerRepo;
        private readonly IRepository<WorkerPieceDetail> _workerPieceDetailRepo;
        private readonly IRepository<Model> _modelRepo;
        private readonly BankService _bankService;

        public WorkerService(
            IWorkerRepository workerRepo,
            IRepository<WorkerPieceDetail> workerPieceDetailRepo,
            IRepository<Model> modelRepo,
            BankService bankService)
        {
            _workerRepo = workerRepo;
            _workerPieceDetailRepo = workerPieceDetailRepo;
            _modelRepo = modelRepo;
            _bankService = bankService;
        }

        // ========================
        // === WORKER OPERATIONS ===
        // ========================

        public async Task AddWorkerAsync(Worker worker)
        {
            if (worker == null) throw new ArgumentNullException(nameof(worker));
            if (string.IsNullOrWhiteSpace(worker.Name)) throw new Exception("اسم العامل مطلوب.");
            if (string.IsNullOrWhiteSpace(worker.Ph_Number)) throw new Exception("رقم هاتف العامل مطلوب.");

            // Initialize the new property
            worker.TotalPaidAmount = 0;

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

        public async Task AddWorkerPieceAsync(Worker worker, int modelId, int pieces)
        {
            if (worker == null || pieces <= 0)
                throw new Exception("عامل غير صالح أو عدد قطع غير صالح.");

            var model = await _modelRepo.GetByIdAsync(modelId);
            if (model == null)
                throw new Exception("النموذج غير موجود.");

            decimal totalAmount = pieces * model.MakingPrice;

            var workerPiece = new WorkerPieceDetail
            {
                WorkerID = worker.ID,
                ModelID = modelId,
                Pieces = pieces,
                TotalAmount = totalAmount,
                Date = DateTime.Now
            };

            await _workerPieceDetailRepo.AddAsync(workerPiece);
            await _workerPieceDetailRepo.SaveChangesAsync();

            // Reload the worker to get fresh data and avoid duplicates
            var updatedWorker = await GetWorkerByIdAsync(worker.ID);

            // Update the current worker object with fresh data
            worker.Price_Count = updatedWorker.Price_Count;
            worker.DailyPieces = updatedWorker.DailyPieces;
            worker.BankTransactions = updatedWorker.BankTransactions;
            worker.TotalPaidAmount = updatedWorker.TotalPaidAmount;
        }

        // ==========================
        // === PAYMENT OPERATIONS ===
        // ==========================

        public async Task<Worker> AddWorkerPaymentAsync(Worker worker, decimal amount, string paymentType)
        {
            if (worker == null)
                throw new ArgumentNullException(nameof(worker));

            if (amount <= 0)
                throw new Exception("يجب أن يكون المبلغ أكبر من الصفر.");

            // Use BankService to add transaction to bank
            await _bankService.AddTransactionAsync(
                description: $"{paymentType} - {worker.Name}",
                amount: amount,
                isIncome: false, // Payment to worker is always outcome from bank
                workerId: worker.ID
            );

            // Update the worker's TotalPaidAmount
            worker.TotalPaidAmount += amount;
            _workerRepo.Update(worker);
            await _workerRepo.SaveChangesAsync();

            // Reload worker with updated data
            return await GetWorkerByIdAsync(worker.ID);
        }

        // ==========================
        // === RESET OPERATIONS ===
        // ==========================

        public async Task ResetWorkerRecordsAsync(Worker worker)
        {
            if (worker == null)
                throw new ArgumentNullException(nameof(worker));

            // Delete all piece records for this worker
            var pieces = (await _workerPieceDetailRepo.GetAllAsync())
                .Where(p => p.WorkerID == worker.ID)
                .ToList();

            foreach (var piece in pieces)
            {
                _workerPieceDetailRepo.Remove(piece);
            }

            // Reset worker's data
            worker.Price_Count = 0;
            worker.TotalPaidAmount = 0; // ✅ Reset the paid amount
            worker.DailyPieces.Clear();

            _workerRepo.Update(worker);
            await _workerRepo.SaveChangesAsync();
            await _workerPieceDetailRepo.SaveChangesAsync();
        }

        // ==========================
        // === WEEKLY RESET OPERATIONS ===
        // ==========================

        public async Task ResetWeeklyWorkerRecordsAsync(Worker worker)
        {
            if (worker == null)
                throw new ArgumentNullException(nameof(worker));

            // Delete all piece records for this worker
            var pieces = (await _workerPieceDetailRepo.GetAllAsync())
                .Where(p => p.WorkerID == worker.ID)
                .ToList();

            foreach (var piece in pieces)
            {
                _workerPieceDetailRepo.Remove(piece);
            }

            // Reset worker's data
            worker.Price_Count = 0;
            worker.TotalPaidAmount = 0; // ✅ Reset the paid amount
            worker.DailyPieces.Clear();

            _workerRepo.Update(worker);
            await _workerRepo.SaveChangesAsync();
            await _workerPieceDetailRepo.SaveChangesAsync();
        }
    }
}