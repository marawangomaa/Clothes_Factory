using Application.Services;
using Clothes_System.Helpers;
using Domain.Entities;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Clothes_System.ViewModels
{
    public class ScissorViewModel : BaseViewModel
    {
        private readonly ScissorService _scissorService;
        private readonly ModelService _modelService;

        public ObservableCollection<Scissor> Cuts { get; set; } = new();
        public ObservableCollection<Model> Models { get; set; } = new();

        private Model _selectedModel;
        private decimal _meters;
        private decimal _totalMetersCut;

        public Model SelectedModel
        {
            get => _selectedModel;
            set { _selectedModel = value; OnPropertyChanged(); }
        }

        public decimal Meters
        {
            get => _meters;
            set { _meters = value; OnPropertyChanged(); }
        }

        public decimal TotalMetersCut
        {
            get => _totalMetersCut;
            set { _totalMetersCut = value; OnPropertyChanged(); }
        }

        public ICommand AddCutCommand { get; }
        public ICommand RefreshCommand { get; }

        public ScissorViewModel(ScissorService scissorService, ModelService modelService)
        {
            _scissorService = scissorService;
            _modelService = modelService;

            AddCutCommand = new AsyncRelayCommand(AddCut);
            RefreshCommand = new AsyncRelayCommand(LoadData);

            _ = LoadData();
        }

        private async Task LoadData()
        {
            try
            {
                var cuts = await _scissorService.GetAllScissorsAsync();
                var models = await _modelService.GetAllModelsAsync();

                await App.Current.Dispatcher.InvokeAsync(() =>
                {
                    Cuts.Clear();
                    foreach (var c in cuts)
                        Cuts.Add(c);

                    Models.Clear();
                    foreach (var m in models)
                        Models.Add(m);

                    TotalMetersCut = Cuts.Sum(c =>
                    {
                        if (decimal.TryParse(c.Number, out var num))
                            return num;
                        return 0;
                    });
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task AddCut()
        {
            if (SelectedModel == null || Meters <= 0)
            {
                MessageBox.Show("Please select a model and enter valid meters.", "Invalid input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string modelName = SelectedModel.Name; // ✅ capture before await

            await _scissorService.AddCutAsync(modelName, Meters);
            await LoadData();

            MessageBox.Show($"Cut added for model '{modelName}' ({Meters} meters).", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            SelectedModel = null;
            Meters = 0;
        }

    }
}
