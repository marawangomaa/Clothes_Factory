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
        private readonly StorageService _storageService;
        public event EventHandler ModelsChanged;

        public ModelService(IRepository<Model> modelRepository, IRepository<Storage> storageRepository, StorageService storageService)
        {
            _modelRepository = modelRepository;
            _storageRepository = storageRepository;
            _storageService = storageService;
        }

        public async Task<ObservableCollection<Model>> GetAllModelsAsync()
        {
            var dbContext = (ClothesSystemDbContext)_modelRepository.GetDbContext();
            var models = await dbContext.Models
                .Include(m => m.Storage)
                .ToListAsync();

            return new ObservableCollection<Model>(models);
        }

        public async Task AddModelAsync(Model model)
        {
            var productName = model.Name;
            var productType = model.Type;

            await _storageService.AddOrUpdateStorageAsync(productName, productType, model.Quantity.GetValueOrDefault());

            var storage = await _storageRepository.GetFirstOrDefaultAsync(s => s.Product_Name == productName);

            model.StorageID = storage.ID;

            await _modelRepository.AddAsync(model);
            await _modelRepository.SaveChangesAsync();

            storage.Number_Of_Products += model.Quantity.GetValueOrDefault();
            await _storageRepository.SaveChangesAsync();

            ModelsChanged?.Invoke(this, EventArgs.Empty);
        }

        public async Task UpdateModelAsync(Model model)
        {
            var dbContext = (ClothesSystemDbContext)_modelRepository.GetDbContext();
            var existingModel = await dbContext.Models
                .Include(m => m.Storage)
                .FirstOrDefaultAsync(m => m.ID == model.ID);

            if (existingModel == null)
                throw new Exception("Model not found.");

            // Update model properties
            existingModel.Name = model.Name;
            existingModel.Type = model.Type;
            existingModel.Code = model.Code;
            existingModel.Metrag = model.Metrag;
            existingModel.MakingPrice = model.MakingPrice;
            existingModel.Cost = model.Cost;
            existingModel.SellPrice = model.SellPrice;
            existingModel.Image = model.Image;

            // Update storage if name or type changed
            if (existingModel.Storage != null &&
                (existingModel.Storage.Product_Name != model.Name ||
                 existingModel.Storage.Product_Type != model.Type))
            {
                existingModel.Storage.Product_Name = model.Name;
                existingModel.Storage.Product_Type = model.Type;
                dbContext.Storages.Update(existingModel.Storage);
            }

            dbContext.Models.Update(existingModel);
            await dbContext.SaveChangesAsync();

            ModelsChanged?.Invoke(this, EventArgs.Empty);
        }

        public async Task<Storage> GetStorageAsync()
        {
            return await _storageRepository.GetFirstOrDefaultAsync(s => s.Product_Name == "General");
        }

        public async Task AddStorageAsync(Storage storage)
        {
            await _storageRepository.AddAsync(storage);
            ModelsChanged?.Invoke(this, EventArgs.Empty);
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

            model.Quantity ??= 0;
            model.Quantity += addedQuantity;
            dbContext.Models.Update(model);

            Storage storage;
            if (model.Storage != null)
            {
                storage = model.Storage;
            }
            else
            {
                storage = await dbContext.Storages
                    .FirstOrDefaultAsync(s => s.Product_Name == model.Name);

                if (storage == null)
                {
                    storage = new Storage
                    {
                        Product_Name = model.Name,
                        Product_Type = model.Type,
                        Number_Of_Products = 0
                    };
                    dbContext.Storages.Add(storage);
                    await dbContext.SaveChangesAsync();
                }

                model.StorageID = storage.ID;
                dbContext.Models.Update(model);
            }

            storage.Number_Of_Products += addedQuantity;
            dbContext.Storages.Update(storage);

            await dbContext.SaveChangesAsync();
            ModelsChanged?.Invoke(this, EventArgs.Empty);
        }

        public async Task UpdateNotesAsync(int modelId, string notes)
        {
            var dbContext = (ClothesSystemDbContext)_modelRepository.GetDbContext();
            var model = await dbContext.Models.FirstOrDefaultAsync(m => m.ID == modelId);

            if (model == null)
                throw new Exception("Model not found.");

            model.Notes = notes;
            dbContext.Models.Update(model);
            await dbContext.SaveChangesAsync();

            ModelsChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}