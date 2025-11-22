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
        private decimal _cost;
        private decimal _sellPrice;
        private string _image;

        public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }
        public string Type { get => _type; set { _type = value; OnPropertyChanged(); } }
        public string Code { get => _code; set { _code = value; OnPropertyChanged(); } }
        public int Metrag { get => _metrag; set { _metrag = value; OnPropertyChanged(); } }
        public decimal Cost { get => _cost; set { _cost = value; OnPropertyChanged(); } }
        public decimal SellPrice { get => _sellPrice; set { _sellPrice = value; OnPropertyChanged(); } }
        public string Image { get => _image; set { _image = value; OnPropertyChanged(); } }

        public ICommand AddModelCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand AddPiecesCommand { get; }
        public ICommand ViewDetailsCommand { get; }


        public ModelViewModel(ModelService modelService)
        {
            _modelService = modelService;
            AddModelCommand = new RelayCommand(async _ => await AddModel());
            RefreshCommand = new RelayCommand(async _ => await LoadModels());
            AddPiecesCommand = new RelayCommand<Model>(async m => await AddPieces(m));
            ViewDetailsCommand = new RelayCommand<Model>(OpenDetailsWindow);

            // 🔄 Load models initially
            _ = LoadModels();
        }
        private void OpenDetailsWindow(Model model)
        {
            if (model == null) return;

            var detailsWindow = new Views.ModelDetailsWindow(model);
            detailsWindow.ShowDialog();
        }

        private async Task LoadModels()
        {
            Models = await _modelService.GetAllModelsAsync();
            OnPropertyChanged(nameof(Models));
        }

        private async Task AddModel()
        {
            var model = new Model
            {
                Name = Name,
                Type = Type,
                Code = Code,
                Metrag = Metrag,
                Cost = Cost,
                SellPrice = SellPrice,
                Image = Image,
                Quantity = 0, // Initially 0, but it will be linked to storage
                StorageID = null // Initially null, will be handled below
            };

            // Add the model using the updated AddModelAsync in the service
            await _modelService.AddModelAsync(model);

            // Reload models after adding
            await LoadModels();
        }

        private async Task AddPieces(Model model)
        {
            await _modelService.UpdateQuantityAsync(model.ID, 10); // e.g. +10 pieces
            await LoadModels();
        }
    }
}
