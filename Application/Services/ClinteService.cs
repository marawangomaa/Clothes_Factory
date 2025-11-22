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
                return (false, "بيانات العميل لا يمكن ان تكون فارغة");

            if (string.IsNullOrWhiteSpace(clinte.Name) ||
                string.IsNullOrWhiteSpace(clinte.Ph_Number) ||
                string.IsNullOrWhiteSpace(clinte.Location))
            {
                return (false, "من فضلك املاء جميع الخانات: الاسم, رقم الهاتف, و الموقع");
            }

            try
            {
                await _ClintRepo.AddAsync(clinte);
                await _ClintRepo.SaveChangesAsync();
                return (true, "تم اضافة العميل بنجاح");
            }
            catch (System.Exception ex)
            {
                return (false, $"خظء فى قاعدة البيانات: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> UpdateClinteAsync(Clinte clinte)
        {
            if (clinte == null)
                return (false, "بيانات العميل لا يمكن ان تكون فارغة");

            try
            {
                _ClintRepo.Update(clinte);
                await _ClintRepo.SaveChangesAsync();
                return (true, "تم تعديل العميل بنجاح");
            }
            catch (System.Exception ex)
            {
                return (false, $"فشل التحديث: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> DeleteClinteAsync(Clinte clinte)
        {
            if (clinte == null)
                return (false, "لا يمكن ان تكون بيانات العميل فارغة");

            try
            {
                _ClintRepo.Remove(clinte);
                await _ClintRepo.SaveChangesAsync();
                return (true, "تم حذف العميل بنجاح");
            }
            catch (System.Exception ex)
            {
                return (false, $"فشل الحزف: {ex.Message}");
            }
        }
    }
}
