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
        public int Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; }
        public decimal PaymentAmount { get; set; }

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
            SaveCommand = new AsyncRelayCommand(SaveAsync);

            SelectedPaymentMethod = PaymentTypes[0];

            _ = LoadModels();
        }

        private async Task LoadModels()
        {
            var models = await _modelService.GetAllModelsAsync();
            Models.Clear();
            foreach (var m in models)
                Models.Add(m);
        }

        private void AddItem()
        {
            if (SelectedModel == null || Quantity <= 0)
                return;

            InvoiceItems.Add(new InvoiceItemViewDto
            {
                ModelId = SelectedModel.ID,
                ModelName = SelectedModel.Name,
                Quantity = Quantity,
                UnitPrice = UnitPrice > 0 ? UnitPrice : SelectedModel.SellPrice
            });

            SelectedModel = null;
            Quantity = 1;
            UnitPrice = 0;

            OnPropertyChanged(nameof(SelectedModel));
            OnPropertyChanged(nameof(Quantity));
            OnPropertyChanged(nameof(UnitPrice));
        }

        private async Task SaveAsync()
        {
            if (!InvoiceItems.Any())
            {
                MessageBox.Show("Add at least one item.", "Warning",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            decimal total = InvoiceItems.Sum(i => i.Total);

            try
            {
                await _invoiceService.AddSellInvoiceAsync(
                    _clientId,
                    InvoiceItems.ToList(),
                    total,
                    SelectedPaymentMethod,
                    PaymentAmount
                );

                MessageBox.Show("Invoice saved successfully.", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                _owner?.Close();
            }
            catch (Exception ex)
            {
                // Show the error in a MessageBox instead of crashing
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
