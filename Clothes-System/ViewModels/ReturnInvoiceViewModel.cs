using Application.DTOs;
using Application.Services;
using Clothes_System.Helpers;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Clothes_System.ViewModels
{
    public class MultiReturnInvoiceViewModel : INotifyPropertyChanged
    {
        private readonly InvoiceService _invoiceService;
        private readonly Action _closeWindow;

        public ObservableCollection<ReturnItemDto> ReturnItems { get; } = new();

        public int ClientId { get; }

        public decimal TotalRefund => ReturnItems.Sum(i => i.Total);

        public ICommand SaveCommand { get; }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }

        public MultiReturnInvoiceViewModel(int clientId, InvoiceService invoiceService, Action closeAction)
        {
            ClientId = clientId;
            _invoiceService = invoiceService;
            _closeWindow = closeAction;

            SaveCommand = new AsyncRelayCommand(SaveAsync, CanSave);

            _ = LoadReturnItemsAsync();
        }

        private async Task LoadReturnItemsAsync()
        {
            try
            {
                ErrorMessage = "Loading items...";
                var items = await _invoiceService.GetClientSellItemsAsync(ClientId);

                if (items == null || !items.Any())
                {
                    ErrorMessage = "No items found for this client";
                    return;
                }

                ReturnItems.Clear();

                var groupedItems = items
                    .Where(i => i.Quantity > 0) // Only include items with available quantity
                    .GroupBy(i => i.ItemId)
                    .Select(g =>
                    {
                        var dto = new ReturnItemDto
                        {
                            ItemId = g.Key,
                            ItemName = g.First().ItemName,
                            OriginalQty = g.Sum(i => i.Quantity),
                            Price = g.First().Price,
                            ReturnQty = 0
                        };

                        dto.PropertyChanged += (_, __) =>
                        {
                            OnPropertyChanged(nameof(TotalRefund));
                            (SaveCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
                        };

                        return dto;
                    });

                foreach (var dto in groupedItems)
                    ReturnItems.Add(dto);

                OnPropertyChanged(nameof(TotalRefund));
                ErrorMessage = ReturnItems.Any() ? string.Empty : "No returnable items found";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to load items: {ex.Message}";
                MessageBox.Show($"Failed loading items: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanSave() => ReturnItems.Any(i => i.ReturnQty > 0);

        private async Task SaveAsync()
        {
            try
            {
                var selectedItems = ReturnItems.Where(i => i.ReturnQty > 0).ToList();

                if (!selectedItems.Any())
                {
                    MessageBox.Show("Please select items to return", "Validation",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Validate return quantities
                foreach (var item in selectedItems)
                {
                    if (item.ReturnQty > item.OriginalQty)
                    {
                        MessageBox.Show($"Return quantity for {item.ItemName} cannot exceed original quantity ({item.OriginalQty})",
                            "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                decimal totalRefund = selectedItems.Sum(i => i.Total);

                ErrorMessage = "Processing return...";
                await _invoiceService.AddReturnInvoiceAsync(ClientId, selectedItems, totalRefund);

                // ✅ Update OriginalQty after return
                foreach (var item in selectedItems)
                {
                    item.OriginalQty -= item.ReturnQty;
                    item.ReturnQty = 0; // reset return qty
                }

                OnPropertyChanged(nameof(TotalRefund));
                (SaveCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();

                MessageBox.Show($"Return invoice saved successfully! Refund amount: {totalRefund:C}", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                _closeWindow?.Invoke();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
                MessageBox.Show($"Failed to save return: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}