using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services
{
    public class BankService
    {
        private readonly IRepository<Bank> _bankRepo;
        private readonly IRepository<BankTransaction> _transactionRepo;
        private readonly IRepository<Worker> _workerRepo;
        private readonly IRepository<Invoice> _invoiceRepo;
        private readonly IRepository<Material> _materialRepo;
        private readonly IRepository<Clinte> _clientRepo;

        public BankService(
            IRepository<Bank> bankRepo,
            IRepository<BankTransaction> transactionRepo,
            IRepository<Worker> workerRepo,
            IRepository<Invoice> invoiceRepo,
            IRepository<Material> materialRepo,
            IRepository<Clinte> clientRepo)
        {
            _bankRepo = bankRepo;
            _transactionRepo = transactionRepo;
            _workerRepo = workerRepo;
            _invoiceRepo = invoiceRepo;
            _materialRepo = materialRepo;
            _clientRepo = clientRepo;
        }

        public async Task<Bank> GetOrCreateBankAsync()
        {
            var bank = (await _bankRepo.GetAllAsync()).FirstOrDefault();
            if (bank == null)
            {
                bank = new Bank { TotalAmount = 10000m };
                await _bankRepo.AddAsync(bank);
                await _bankRepo.SaveChangesAsync();
            }
            return bank;
        }

        public async Task<IEnumerable<BankTransaction>> GetAllTransactionsAsync()
        {
            await DebugTransactions();

            // Use GetAllIncludingAsync with the includes
            var transactions = await _transactionRepo.GetAllIncludingAsync(
                t => t.Worker,
                t => t.Invoice,
                t => t.Material
            );

            // Since we're including the related entities, we can set RelatedTo directly
            foreach (var transaction in transactions)
            {
                await SetRelatedToWithClientInfo(transaction);
            }

            return transactions.OrderByDescending(t => t.Date);
        }

        private async Task SetRelatedToWithClientInfo(BankTransaction transaction)
        {
            Console.WriteLine($"Setting RelatedTo for Transaction {transaction.ID}: " +
                             $"Worker={transaction.Worker?.Name ?? "NULL"}, " +
                             $"Invoice ID={transaction.Invoice?.ID ?? 0}, " +
                             $"Material={transaction.Material?.Name ?? "NULL"}");

            if (transaction.Worker != null)
            {
                transaction.RelatedTo = $"عامل: {transaction.Worker.Name}";
            }
            else if (transaction.Invoice != null)
            {
                // Load the client information for this invoice
                var invoiceWithClient = await _invoiceRepo.GetAllIncludingAsync(
                    i => i.Clinte
                );
                var currentInvoice = invoiceWithClient.FirstOrDefault(i => i.ID == transaction.Invoice.ID);

                if (currentInvoice?.Clinte != null)
                    transaction.RelatedTo = $"عميل: {currentInvoice.Clinte.Name}";
                else
                    transaction.RelatedTo = "فاتورة";
            }
            else if (transaction.Material != null)
            {
                transaction.RelatedTo = $"مادة: {transaction.Material.Name}";
            }
            else
            {
                transaction.RelatedTo = "أخرى";
            }

            Console.WriteLine($"Final RelatedTo: {transaction.RelatedTo}");
        }

        public async Task AddTransactionAsync(string description, decimal amount, bool isIncome,
            int? workerId = null, int? invoiceId = null, int? materialId = null)
        {
            var bank = await GetOrCreateBankAsync();
            var newTotal = isIncome ? bank.TotalAmount + amount : bank.TotalAmount - amount;

            if (newTotal < 0)
                throw new Exception("لا يوجد مال كافى فى البنك لاتمام العملية");

            string relatedTo = await GetRelatedToInfo(workerId, invoiceId, materialId);

            Console.WriteLine($"Creating transaction: " +
                             $"WorkerID={workerId}, " +
                             $"InvoiceID={invoiceId}, " +
                             $"MaterialID={materialId}, " +
                             $"RelatedTo={relatedTo}");

            var transaction = new BankTransaction
            {
                Date = DateTime.Now,
                Type = isIncome ? "دخل" : "خارج",
                Amount = amount,
                TotalAfterTransaction = newTotal,
                BankID = bank.ID,
                WorkerID = workerId,
                InvoiceID = invoiceId,
                MaterialID = materialId,
                RelatedTo = relatedTo
            };

            bank.TotalAmount = newTotal;

            await _transactionRepo.AddAsync(transaction);
            _bankRepo.Update(bank);

            await _transactionRepo.SaveChangesAsync();
            await _bankRepo.SaveChangesAsync();

            Console.WriteLine($"Transaction saved with ID: {transaction.ID}, RelatedTo: {transaction.RelatedTo}");
        }

        private async Task<string> GetRelatedToInfo(int? workerId, int? invoiceId, int? materialId)
        {
            if (workerId.HasValue)
            {
                var worker = await _workerRepo.GetByIdAsync(workerId.Value);
                return worker != null ? $"عامل: {worker.Name}" : "عامل";
            }
            else if (invoiceId.HasValue)
            {
                // Load invoice with client information
                var invoices = await _invoiceRepo.GetAllIncludingAsync(i => i.Clinte);
                var invoice = invoices.FirstOrDefault(i => i.ID == invoiceId.Value);

                if (invoice?.Clinte != null)
                    return $"عميل: {invoice.Clinte.Name}";
                return "فاتورة";
            }
            else if (materialId.HasValue)
            {
                var material = await _materialRepo.GetByIdAsync(materialId.Value);
                return material != null ? $"مادة: {material.Name}" : "مادة";
            }
            else
            {
                return "أخرى";
            }
        }

        public async Task DebugTransactions()
        {
            var transactions = await _transactionRepo.GetAllAsync();

            Console.WriteLine("=== TRANSACTION DEBUG INFO ===");
            foreach (var transaction in transactions)
            {
                Console.WriteLine($"ID: {transaction.ID}, " +
                                 $"Type: {transaction.Type}, " +
                                 $"RelatedTo: {transaction.RelatedTo}, " +
                                 $"WorkerID: {transaction.WorkerID}, " +
                                 $"InvoiceID: {transaction.InvoiceID}, " +
                                 $"MaterialID: {transaction.MaterialID}");
            }
            Console.WriteLine("=== END DEBUG ===");
        }

        public async Task TestBankTransactions()
        {
            try
            {
                // Create some test transactions
                await AddTransactionAsync("راتب عامل", 500m, false, workerId: 1);
                await AddTransactionAsync("بيع منتجات", 1000m, true, invoiceId: 1);
                await AddTransactionAsync("شراء مواد", 200m, false, materialId: 1);

                // Now load and display them
                var transactions = await GetAllTransactionsAsync();
                Console.WriteLine($"Loaded {transactions.Count()} transactions");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public async Task UpdateBankTotalAsync(decimal newAmount)
        {
            var bank = await GetOrCreateBankAsync();

            // Calculate the difference to create a transaction record
            decimal difference = newAmount - bank.TotalAmount;
            bool isIncome = difference > 0;
            decimal absoluteDifference = Math.Abs(difference);

            // Update bank total
            bank.TotalAmount = newAmount;

            // Create a transaction record for this adjustment
            var transaction = new BankTransaction
            {
                Date = DateTime.Now,
                Type = isIncome ? "دخل" : "خارج",
                Amount = absoluteDifference,
                TotalAfterTransaction = newAmount,
                BankID = bank.ID,
                RelatedTo = "تعديل رصيد البنك",
                WorkerID = null,
                InvoiceID = null,
                MaterialID = null
            };

            _bankRepo.Update(bank);
            await _transactionRepo.AddAsync(transaction);

            await _bankRepo.SaveChangesAsync();
            await _transactionRepo.SaveChangesAsync();
        }
    }
}