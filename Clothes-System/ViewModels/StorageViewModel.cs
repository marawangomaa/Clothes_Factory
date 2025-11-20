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
    public class StorageViewModel
    {
        private readonly StorageService _storageService;

        public ObservableCollection<Storage> StorageItems { get; set; } = new();

        public ICommand LoadStorageCommand { get; }

        public StorageViewModel(StorageService storageService)
        {
            _storageService = storageService;
            LoadStorageCommand = new AsyncRelayCommand(async () => await LoadStorage());
        }

        private async Task LoadStorage()
        {
            StorageItems.Clear();
            var items = await _storageService.GetAllStorageAsync();
            foreach (var item in items)
                StorageItems.Add(item);
        }
    }
}
