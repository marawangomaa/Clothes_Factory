using Domain.Entities;
using Domain.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services
{
    public class ClinteService
    {
        private readonly IRepository<Clinte> _ClintRepo;

        public ClinteService(IRepository<Clinte> ClintRepo)
        {
            _ClintRepo = ClintRepo;
        }

        public async Task<IEnumerable<Clinte>> GetAllClinetsAsync()
        {
            return await _ClintRepo.GetAllAsync();
        }

        public async Task<(bool Success, string Message)> AddClinteAsync(Clinte clinte)
        {
            if (clinte == null)
                return (false, "Client data cannot be null.");

            if (string.IsNullOrWhiteSpace(clinte.Name) ||
                string.IsNullOrWhiteSpace(clinte.Ph_Number) ||
                string.IsNullOrWhiteSpace(clinte.Location))
            {
                return (false, "Please fill all required fields: Name, Phone, and Location.");
            }

            try
            {
                await _ClintRepo.AddAsync(clinte);
                await _ClintRepo.SaveChangesAsync();
                return (true, "Client added successfully.");
            }
            catch (System.Exception ex)
            {
                return (false, $"Database error: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> UpdateClinteAsync(Clinte clinte)
        {
            if (clinte == null)
                return (false, "Client cannot be null.");

            try
            {
                _ClintRepo.Update(clinte);
                await _ClintRepo.SaveChangesAsync();
                return (true, "Client updated successfully.");
            }
            catch (System.Exception ex)
            {
                return (false, $"Update failed: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> DeleteClinteAsync(Clinte clinte)
        {
            if (clinte == null)
                return (false, "Client cannot be null.");

            try
            {
                _ClintRepo.Remove(clinte);
                await _ClintRepo.SaveChangesAsync();
                return (true, "Client deleted successfully.");
            }
            catch (System.Exception ex)
            {
                return (false, $"Delete failed: {ex.Message}");
            }
        }
    }
}
