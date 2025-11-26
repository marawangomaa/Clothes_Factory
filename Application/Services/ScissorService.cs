using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Application.Services
{
    public class ScissorService
    {
        private readonly IRepository<Scissor> _scissorRepository;

        public ScissorService(IRepository<Scissor> scissorRepository)
        {
            _scissorRepository = scissorRepository;
        }

        public async Task<ObservableCollection<Scissor>> GetAllScissorsAsync()
        {
            var list = await _scissorRepository.GetAllAsync();
            return new ObservableCollection<Scissor>(list);
        }

        public async Task AddCutAsync(string modelName, int modelMetrag, decimal meters)
        {
            if (string.IsNullOrWhiteSpace(modelName))
                throw new Exception("Model name is required.");

            var cut = new Scissor
            {
                Model = modelName,
                ModelMetrag = modelMetrag, // ✅ Store the model's metrag
                Number = meters.ToString(),
                Date = DateTime.Now
            };

            await _scissorRepository.AddAsync(cut);
            await _scissorRepository.SaveChangesAsync();
        }
    }
}
