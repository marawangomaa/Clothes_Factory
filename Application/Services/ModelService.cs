using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data.EF;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class ModelService
    {
        private readonly IRepository<Model> _modelRepository;
        private readonly IRepository<Storage> _storageRepository;
        private readonly StorageService _storageService; // Injecting StorageService

        public ModelService(IRepository<Model> modelRepository, IRepository<Storage> storageRepository, StorageService storageService)
        {
            _modelRepository = modelRepository;
            _storageRepository = storageRepository;
            _storageService = storageService; // Initializing StorageService
        }

        public async Task<ObservableCollection<Model>> GetAllModelsAsync()
        {
            var dbContext = (ClothesSystemDbContext)_modelRepository.GetDbContext();

            // Ensure we actually query the database directly
            var models = await dbContext.Models
                .Include(m => m.Storage) // optional: include related data if needed
                .ToListAsync();

            return new ObservableCollection<Model>(models);
        }

        public async Task AddModelAsync(Model model)
        {
            // Use StorageService to add or update storage
            var productName = model.Name; // Assuming model's name is used for storage
            var productType = model.Type; // Assuming model's type is used for storage

            // Add or update storage through the StorageService
            await _storageService.AddOrUpdateStorageAsync(productName, productType, model.Quantity.GetValueOrDefault());

            // Fetch the updated storage (now the ID should be set)
            var storage = await _storageRepository.GetFirstOrDefaultAsync(s => s.Product_Name == productName);

            // Now that the storage exists (or is updated), assign the storage ID to the model
            model.StorageID = storage.ID;

            // Add the model to the repository
            await _modelRepository.AddAsync(model);
            await _modelRepository.SaveChangesAsync(); // Save the model to the database

            // Update the storage count based on the model quantity
            storage.Number_Of_Products += model.Quantity.GetValueOrDefault();
            await _storageRepository.SaveChangesAsync(); // Save the updated storage
        }

        public async Task<Storage> GetStorageAsync()
        {
            return await _storageRepository.GetFirstOrDefaultAsync(s => s.Product_Name == "General");
        }

        public async Task AddStorageAsync(Storage storage)
        {
            await _storageRepository.AddAsync(storage);
        }

        public async Task UpdateQuantityAsync(int modelId, int addedQuantity)
        {
            if (addedQuantity <= 0)
                return;

            var dbContext = (ClothesSystemDbContext)_modelRepository.GetDbContext();

            var model = await dbContext.Models
                .Include(m => m.Storage)
                .FirstOrDefaultAsync(m => m.ID == modelId);

            if (model == null)
                throw new Exception("Model not found.");

            // Update model quantity
            model.Quantity ??= 0;
            model.Quantity += addedQuantity;
            dbContext.Models.Update(model);

            // Ensure model has a storage record
            Storage storage;
            if (model.Storage != null)
            {
                storage = model.Storage;
            }
            else
            {
                // Try to find storage for this product
                storage = await dbContext.Storages
                    .FirstOrDefaultAsync(s => s.Product_Name == model.Name);

                // If not found, create one
                if (storage == null)
                {
                    storage = new Storage
                    {
                        Product_Name = model.Name,
                        Product_Type = model.Type,
                        Number_Of_Products = 0
                    };
                    dbContext.Storages.Add(storage);
                    await dbContext.SaveChangesAsync(); // To get the ID
                }

                model.StorageID = storage.ID;
                dbContext.Models.Update(model);
            }

            // ✅ Update the storage count properly
            storage.Number_Of_Products += addedQuantity;
            dbContext.Storages.Update(storage);

            await dbContext.SaveChangesAsync();
        }
    }

}
