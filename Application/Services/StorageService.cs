using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class StorageService
    {
        private readonly IRepository<Storage> _storageRepo;

        public StorageService(IRepository<Storage> storageRepo)
        {
            _storageRepo = storageRepo;
        }

        public async Task<IEnumerable<Storage>> GetAllStorageAsync()
        {
            return await _storageRepo.GetAllAsync();
        }

        public async Task AddOrUpdateStorageAsync(string productName, string productType, int number)
        {
            if (string.IsNullOrWhiteSpace(productName))
                throw new Exception("Product name is required.");

            var existing = await _storageRepo.GetFirstOrDefaultAsync(s => s.Product_Name == productName);

            if (existing != null)
            {
                existing.Number_Of_Products += number;
                _storageRepo.Update(existing);
            }
            else
            {
                var newStorage = new Storage
                {
                    Product_Name = productName,
                    Product_Type = productType,
                    Number_Of_Products = number
                };
                await _storageRepo.AddAsync(newStorage);
            }

            await _storageRepo.SaveChangesAsync();
        }

        public async Task RemoveFromStorageAsync(string productName, int number)
        {
            var existing = await _storageRepo.GetFirstOrDefaultAsync(s => s.Product_Name == productName);
            if (existing == null)
                throw new Exception("Product not found in storage.");

            if (existing.Number_Of_Products < number)
                throw new Exception("Not enough items in storage.");

            existing.Number_Of_Products -= number;
            _storageRepo.Update(existing);
            await _storageRepo.SaveChangesAsync();
        }
    }
}
