using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class BankService
    {
        private readonly IRepository<Bank> _bankRepo;
        private readonly IRepository<BankTransaction> _transactionRepo;

        public BankService(IRepository<Bank> bankRepo, IRepository<BankTransaction> transactionRepo)
        {
            _bankRepo = bankRepo;
            _transactionRepo = transactionRepo;
        }

        // ✅ Get or create the main bank
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

        // ✅ Get all transactions ordered by date
        public async Task<IEnumerable<BankTransaction>> GetAllTransactionsAsync()
        {
            var transactions = await _transactionRepo.GetAllAsync();
            return transactions.OrderByDescending(t => t.Date);
        }

        // ✅ Add a transaction (income or outcome)
        public async Task AddTransactionAsync(string description, decimal amount, bool isIncome)
        {
            var bank = await GetOrCreateBankAsync();
            var newTotal = isIncome ? bank.TotalAmount + amount : bank.TotalAmount - amount;

            if (newTotal < 0)
                throw new Exception("لا يوجد مال كافى فى البنك لاتمام العملية");

            var transaction = new BankTransaction
            {
                Date = DateTime.Now,
                Type = isIncome ? "دخل" : "خارج",
                Amount = amount,
                TotalAfterTransaction = newTotal,
                BankID = bank.ID
            };

            bank.TotalAmount = newTotal;

            await _transactionRepo.AddAsync(transaction);
            _bankRepo.Update(bank);

            await _transactionRepo.SaveChangesAsync();
            await _bankRepo.SaveChangesAsync();
        }
    }
}
