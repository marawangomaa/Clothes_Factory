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
                var items = await _invoiceService.GetClientSellItemsAsync(ClientId);
                if (items == null) return;

                ReturnItems.Clear();

                var groupedItems = items
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
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed loading items: " + ex.Message);
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
                    MessageBox.Show("No items selected to return");
                    return;
                }

                decimal totalRefund = selectedItems.Sum(i => i.Total);

                await _invoiceService.AddReturnInvoiceAsync(ClientId, selectedItems, totalRefund);

                // ✅ Update OriginalQty after return
                foreach (var item in selectedItems)
                {
                    item.OriginalQty -= item.ReturnQty;
                    item.OnPropertyChanged(nameof(item.OriginalQty)); // 🔹 Notify UI
                    item.ReturnQty = 0; // reset return qty
                    item.OnPropertyChanged(nameof(item.ReturnQty));   // update Total too
                }

                OnPropertyChanged(nameof(TotalRefund));
                (SaveCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();

                MessageBox.Show("Return Invoice Saved ✔");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
