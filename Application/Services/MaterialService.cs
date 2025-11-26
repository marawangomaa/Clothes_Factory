using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services
{
    public class MaterialService
    {
        private readonly IRepository<Material> _materialRepo;
        private readonly BankService _bankService;

        public MaterialService(IRepository<Material> materialRepo, BankService bankService)
        {
            _materialRepo = materialRepo;
            _bankService = bankService;
        }

        public async Task<IEnumerable<Material>> GetAllMaterialsAsync()
        {
            return await _materialRepo.GetAllAsync();
        }

        public async Task AddMaterialAsync(Material material)
        {
            if (material == null) throw new ArgumentNullException(nameof(material));
            if (string.IsNullOrWhiteSpace(material.Name)) throw new Exception("اسم المادة مطلوب.");
            if (material.TotalCost <= 0) throw new Exception("التكلفة الإجمالية يجب أن تكون أكبر من الصفر.");

            var bank = await _bankService.GetOrCreateBankAsync();

            if (bank.TotalAmount < material.TotalCost)
                throw new Exception("لا يوجد مال كافى فى البنك لشراء هذه المادة.");

            // Add material first to get the ID
            material.BankID = bank.ID;
            material.Date = DateTime.Now;

            await _materialRepo.AddAsync(material);
            await _materialRepo.SaveChangesAsync();

            // Now deduct money via bank transaction with the material ID
            await _bankService.AddTransactionAsync(
                description: $"شراء مادة: {material.Name}",
                amount: material.TotalCost,
                isIncome: false,
                materialId: material.ID  // This is the key - pass the material ID
            );
        }
    }
}