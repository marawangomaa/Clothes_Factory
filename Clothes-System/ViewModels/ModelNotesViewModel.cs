using Application.Services;
using Clothes_System.Helpers;
using Domain.Entities;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Clothes_System.ViewModels
{
    public class ModelNotesViewModel : BaseViewModel
    {
        public event Action CloseRequested;
        private readonly ModelService _modelService;
        public Model CurrentModel { get; }

        private string _notes;
        public string Notes
        {
            get => _notes;
            set
            {
                _notes = value;
                OnPropertyChanged();
            }
        }

        public ICommand SaveNotesCommand { get; }
        public ICommand CloseCommand { get; }

        public ModelNotesViewModel(Model model, ModelService modelService)
        {
            CurrentModel = model;
            _modelService = modelService;
            Notes = model.Notes ?? "";

            SaveNotesCommand = new RelayCommand(async _ => await SaveNotes());
            CloseCommand = new RelayCommand(_ => CloseRequested?.Invoke());
        }

        private async Task SaveNotes()
        {
            try
            {
                await _modelService.UpdateNotesAsync(CurrentModel.ID, Notes);
                CurrentModel.Notes = Notes;

                MessageBox.Show("تم حفظ الملاحظات بنجاح!", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                CloseRequested?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في حفظ الملاحظات: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}