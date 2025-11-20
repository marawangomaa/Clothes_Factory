using Application.Services;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Clothes_System.Helpers;

namespace Clothes_System.ViewModels
{
    public class MaterialViewModel
    {
        private readonly MaterialService _materialService;

        public ObservableCollection<Material> Materials { get; set; } = new();
        public Material NewMaterial { get; set; } = new Material();

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
                    NewMaterial = new Material(); // Reset for next entry
                }
                catch (Exception ex)
                {
                    // Show error message (optional: use MessageBox or pass to UI)
                    System.Windows.MessageBox.Show(ex.Message);
                }
            }
        }
    }
}
