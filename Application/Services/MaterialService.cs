using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            if (string.IsNullOrWhiteSpace(material.Name)) throw new Exception("Material name is required.");
            if (material.TotalCost <= 0) throw new Exception("Total cost must be greater than 0.");

            var bank = await _bankService.GetOrCreateBankAsync();

            if (bank.TotalAmount < material.TotalCost)
                throw new Exception("Not enough money in the bank to buy this material.");

            // Deduct money via bank transaction
            await _bankService.AddTransactionAsync($"Buy Material: {material.Name}", material.TotalCost, false);

            material.BankID = bank.ID;
            await _materialRepo.AddAsync(material);
            await _materialRepo.SaveChangesAsync();
        }

    }
}
