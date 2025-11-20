using Application.Services;
using Clothes_System.Helpers;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Clothes_System.ViewModels
{
    public class ModelDetailsViewModel : BaseViewModel
    {
        private readonly ModelService _modelService;
        public Model CurrentModel { get; }

        private int _addedPieces;
        public int AddedPieces
        {
            get => _addedPieces;
            set
            {
                _addedPieces = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddPiecesCommand { get; }

        public ModelDetailsViewModel(Model model, ModelService modelService)
        {
            CurrentModel = model;
            _modelService = modelService;
            AddPiecesCommand = new RelayCommand(async _ => await AddPieces());
        }

        private async Task AddPieces()
        {
            if (AddedPieces <= 0)
            {
                MessageBox.Show("Please enter a valid number of pieces.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            await _modelService.UpdateQuantityAsync(CurrentModel.ID, AddedPieces);
            MessageBox.Show($"{AddedPieces} pieces added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            AddedPieces = 0;
        }
    }
}
