using Application.Services;
using Clothes_System.Helpers;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Clothes_System.ViewModels
{
    public class ModelViewModel : BaseViewModel
    {
        private readonly ModelService _modelService;
        public ObservableCollection<Model> Models { get; set; } = new();

        private string _name;
        private string _type;
        private string _code;
        private int _metrag;
        private decimal _makingPrice;
        private decimal _cost;
        private decimal _sellPrice;
        private string _image;

        public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }
        public string Type { get => _type; set { _type = value; OnPropertyChanged(); } }
        public string Code { get => _code; set { _code = value; OnPropertyChanged(); } }
        public int Metrag { get => _metrag; set { _metrag = value; OnPropertyChanged(); } }
        public decimal MakingPrice { get => _makingPrice; set { _makingPrice = value; OnPropertyChanged(); } }
        public decimal Cost { get => _cost; set { _cost = value; OnPropertyChanged(); } }
        public decimal SellPrice { get => _sellPrice; set { _sellPrice = value; OnPropertyChanged(); } }
        public string Image { get => _image; set { _image = value; OnPropertyChanged(); } }

        public ICommand AddModelCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand AddPiecesCommand { get; }
        public ICommand ViewDetailsCommand { get; }
        public ICommand EditNotesCommand { get; }
        public ICommand UpdateModelCommand { get; }

        public ModelViewModel(ModelService modelService)
        {
            _modelService = modelService;
            AddModelCommand = new RelayCommand(async _ => await AddModel());
            RefreshCommand = new RelayCommand(async _ => await LoadModels());
            AddPiecesCommand = new RelayCommand<Model>(async m => await AddPieces(m));
            ViewDetailsCommand = new RelayCommand<Model>(OpenDetailsWindow);
            EditNotesCommand = new RelayCommand<Model>(async m => await EditNotes(m));
            UpdateModelCommand = new RelayCommand<Model>(async m => await UpdateModel(m));

            _ = LoadModels();
        }

        private void OpenDetailsWindow(Model model)
        {
            if (model == null) return;

            var detailsWindow = new Views.ModelDetailsWindow(model);
            detailsWindow.ShowDialog();
            _ = LoadModels();
        }

        private async Task EditNotes(Model model)
        {
            if (model == null) return;

            var notesWindow = new Views.ModelNotesWindow(model, _modelService);
            notesWindow.ShowDialog();
            _ = LoadModels();
        }

        private async Task UpdateModel(Model model)
        {
            if (model == null) return;

            try
            {
                await _modelService.UpdateModelAsync(model);
                await LoadModels();
            }
            catch (Exception ex)
            {
                // Handle exception (show message to user)
                System.Windows.MessageBox.Show($"Error updating model: {ex.Message}", "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private async Task LoadModels()
        {
            Models = await _modelService.GetAllModelsAsync();
            OnPropertyChanged(nameof(Models));
        }

        private async Task AddModel()
        {
            try
            {
                var model = new Model
                {
                    Name = Name,
                    Type = Type,
                    Code = Code,
                    Metrag = Metrag,
                    MakingPrice = MakingPrice,
                    Cost = Cost,
                    SellPrice = SellPrice,
                    Image = Image,
                    Quantity = 0,
                    StorageID = null
                };

                await _modelService.AddModelAsync(model);

                // Clear the form after successful addition
                ClearForm();

                await LoadModels();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error adding model: {ex.Message}", "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void ClearForm()
        {
            Name = string.Empty;
            Type = string.Empty;
            Code = string.Empty;
            Metrag = 0;
            MakingPrice = 0;
            Cost = 0;
            SellPrice = 0;
            Image = string.Empty;
        }

        private async Task AddPieces(Model model)
        {
            await _modelService.UpdateQuantityAsync(model.ID, 10);
            await LoadModels();
        }
    }
}