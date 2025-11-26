using Application.DTOs;
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
    public class SellInvoiceViewModel : BaseViewModel
    {
        private readonly InvoiceService _invoiceService;
        private readonly ModelService _modelService;
        private readonly Window _owner;

        public ObservableCollection<Model> Models { get; } = new();
        public ObservableCollection<InvoiceItemViewDto> InvoiceItems { get; } = new();

        private Model _selectedModel;
        public Model SelectedModel
        {
            get => _selectedModel;
            set
            {
                _selectedModel = value;
                OnPropertyChanged();

                // When a model is selected, set the UnitPrice to its SellPrice
                if (_selectedModel != null)
                {
                    UnitPrice = _selectedModel.SellPrice;
                    OnPropertyChanged(nameof(UnitPrice));
                }
            }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        private int _quantity = 1;
        public int Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalAmount)); // Update total when quantity changes
            }
        }

        private decimal _unitPrice;
        public decimal UnitPrice
        {
            get => _unitPrice;
            set
            {
                _unitPrice = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalAmount)); // Update total when price changes
            }
        }

        private decimal _paymentAmount;
        public decimal PaymentAmount
        {
            get => _paymentAmount;
            set
            {
                _paymentAmount = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(RemainingAmount)); // Update remaining when payment changes
            }
        }

        // Add calculated properties for better UI feedback
        public decimal TotalAmount => InvoiceItems.Sum(i => i.Total);
        public decimal RemainingAmount => TotalAmount - PaymentAmount;

        // ---------------------------
        // PAYMENT TYPES (STRING ONLY)
        // ---------------------------
        private string _selectedPaymentMethod;
        public string SelectedPaymentMethod
        {
            get => _selectedPaymentMethod;
            set
            {
                _selectedPaymentMethod = value;
                OnPropertyChanged();
            }
        }

        public string[] PaymentTypes { get; } =
        {
            "cash",
            "instapay",
            "vodafonecash"
        };

        // ---------------------------

        public ICommand AddItemCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand RemoveItemCommand { get; }

        private readonly int _clientId;

        public SellInvoiceViewModel(
            int clientId,
            InvoiceService invoiceService,
            ModelService modelService,
            Window owner)
        {
            _clientId = clientId;
            _invoiceService = invoiceService;
            _modelService = modelService;
            _owner = owner;

            AddItemCommand = new RelayCommand(_ => AddItem());
            SaveCommand = new AsyncRelayCommand(SaveAsync); // Remove CanSave parameter
            RemoveItemCommand = new RelayCommand(RemoveItem);

            SelectedPaymentMethod = PaymentTypes[0];

            _ = LoadModels();
        }

        private async Task LoadModels()
        {
            try
            {
                var models = await _modelService.GetAllModelsAsync();
                Models.Clear();
                foreach (var m in models)
                    Models.Add(m);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to load models: {ex.Message}";
            }
        }

        private void AddItem()
        {
            if (SelectedModel == null)
            {
                ErrorMessage = "Please select a model";
                return;
            }

            if (Quantity <= 0)
            {
                ErrorMessage = "Quantity must be greater than 0";
                return;
            }

            if (UnitPrice <= 0)
            {
                ErrorMessage = "Unit price must be greater than 0";
                return;
            }

            // Check if item already exists
            var existingItem = InvoiceItems.FirstOrDefault(i => i.ModelId == SelectedModel.ID);
            if (existingItem != null)
            {
                // Update existing item quantity
                existingItem.Quantity += Quantity;
                // Refresh the collection to update UI
                var index = InvoiceItems.IndexOf(existingItem);
                InvoiceItems.Remove(existingItem);
                InvoiceItems.Insert(index, existingItem);
            }
            else
            {
                // Add new item
                InvoiceItems.Add(new InvoiceItemViewDto
                {
                    ModelId = SelectedModel.ID,
                    ModelName = SelectedModel.Name,
                    Quantity = Quantity,
                    UnitPrice = UnitPrice
                });
            }

            // Reset form
            SelectedModel = null;
            Quantity = 1;
            UnitPrice = 0;
            ErrorMessage = string.Empty;

            // Notify property changes
            OnPropertyChanged(nameof(SelectedModel));
            OnPropertyChanged(nameof(Quantity));
            OnPropertyChanged(nameof(UnitPrice));
            OnPropertyChanged(nameof(TotalAmount));
            OnPropertyChanged(nameof(RemainingAmount));
        }

        private void RemoveItem(object parameter)
        {
            if (parameter is InvoiceItemViewDto item)
            {
                InvoiceItems.Remove(item);
                OnPropertyChanged(nameof(TotalAmount));
                OnPropertyChanged(nameof(RemainingAmount));
                (SaveCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        private bool CanSave()
        {
            return InvoiceItems.Any() && PaymentAmount >= 0 && PaymentAmount <= TotalAmount;
        }

        private async Task SaveAsync()
        {
            if (!InvoiceItems.Any())
            {
                MessageBox.Show("Please add at least one item.", "Warning",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (PaymentAmount < 0 || PaymentAmount > TotalAmount)
            {
                MessageBox.Show("Payment amount must be between 0 and total amount.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                ErrorMessage = "Saving invoice...";

                await _invoiceService.AddSellInvoiceAsync(
                    _clientId,
                    InvoiceItems.ToList(),
                    TotalAmount,
                    SelectedPaymentMethod,
                    PaymentAmount
                );

                MessageBox.Show("Invoice saved successfully!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                _owner?.Close();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
                MessageBox.Show($"Failed to save invoice: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}