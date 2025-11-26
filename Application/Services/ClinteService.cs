using Domain.Entities;
using Domain.Interfaces;
using System;
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
                // Remove the line that sets Debt to 0 to allow the value from UI
                // clinte.Debt = 0; // REMOVED THIS LINE

                await _ClintRepo.AddAsync(clinte);
                await _ClintRepo.SaveChangesAsync();
                return (true, "تم اضافة العميل بنجاح");
            }
            catch (Exception ex)
            {
                return (false, $"خطأ فى قاعدة البيانات: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> UpdateClinteAsync(Clinte clinte)
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
                _ClintRepo.Update(clinte);
                await _ClintRepo.SaveChangesAsync();
                return (true, "تم تعديل العميل بنجاح");
            }
            catch (Exception ex)
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
                // Check if client has any invoices or transactions before deleting
                // You might want to add this check based on your business logic

                _ClintRepo.Remove(clinte);
                await _ClintRepo.SaveChangesAsync();
                return (true, "تم حذف العميل بنجاح");
            }
            catch (Exception ex)
            {
                return (false, $"فشل الحذف: {ex.Message}");
            }
        }
    }
}