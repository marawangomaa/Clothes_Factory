using Application.Services;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Clothes_System.Helpers;

namespace Clothes_System.ViewModels
{
    public class MaterialViewModel : INotifyPropertyChanged
    {
        private readonly MaterialService _materialService;

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public ObservableCollection<Material> Materials { get; set; } = new();

        private Material _newMaterial = new Material();
        public Material NewMaterial
        {
            get => _newMaterial;
            set
            {
                _newMaterial = value;
                OnPropertyChanged(nameof(NewMaterial));
            }
        }

        public ICommand LoadMaterialsCommand { get; }
        public ICommand AddMaterialCommand { get; }

        public MaterialViewModel(MaterialService materialService)
        {
            _materialService = materialService;

            LoadMaterialsCommand = new AsyncRelayCommand(async () => await LoadMaterials());
            AddMaterialCommand = new AsyncRelayCommand(async () => await AddMaterial());
        }

        private async Task LoadMaterials()
        {
            Materials.Clear();
            var materials = await _materialService.GetAllMaterialsAsync();
            foreach (var m in materials)
            {
                Materials.Add(m);
            }
        }

        private async Task AddMaterial()
        {
            if (NewMaterial != null && !string.IsNullOrWhiteSpace(NewMaterial.Name))
            {
                try
                {
                    await _materialService.AddMaterialAsync(NewMaterial);
                    await LoadMaterials();

                    // Reset input fields (this clears the TextBoxes)
                    NewMaterial = new Material();
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message);
                }
            }
        }
    }
}
