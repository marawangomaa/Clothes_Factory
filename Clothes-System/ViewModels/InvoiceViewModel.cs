using Application.DTOs;
using Application.Services;
using Clothes_System.Helpers;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Clothes_System.ViewModels
{
    public class InvoiceViewModel : BaseViewModel
    {
        private readonly InvoiceService _invoiceService;
        private readonly IRepository<Clinte> _clientRepo;
        private readonly IRepository<Model> _modelRepo;

        public ObservableCollection<Clinte> Clients { get; } = new();
        public ObservableCollection<Model> Models { get; } = new();

        public ObservableCollection<InvoiceDisplayDto> ClientInvoices { get; } = new();
        public ObservableCollection<SellItemDto> SellItems { get; } = new();

        public ICommand RefreshCommand { get; }
        public ICommand SellCommand { get; }
        public ICommand ReturnCommand { get; }
        public ICommand PaymentCommand { get; }

        public InvoiceViewModel()
        {
            var sp = App.ServiceProvider;

            _invoiceService = sp.GetRequiredService<InvoiceService>();
            _clientRepo = sp.GetRequiredService<IRepository<Clinte>>();
            _modelRepo = sp.GetRequiredService<IRepository<Model>>();

            RefreshCommand = new AsyncRelayCommand(LoadAllAsync);
            SellCommand = new AsyncRelayCommand(ProcessSellAsync);
            ReturnCommand = new AsyncRelayCommand(ProcessReturnAsync);
            PaymentCommand = new AsyncRelayCommand(ProcessPaymentAsync);

            _ = LoadAllAsync();
        }

        // ---------------------------
        // Properties
        // ---------------------------
        private Clinte _selectedClient;
        public Clinte SelectedClient
        {
            get => _selectedClient;
            set
            {
                _selectedClient = value;
                OnPropertyChanged();
                _ = LoadClientInvoices();
                _ = LoadClientSellItems();
            }
        }

        private Model _selectedModel;
        public Model SelectedModel
        {
            get => _selectedModel;
            set { _selectedModel = value; OnPropertyChanged(); }
        }

        private int _quantity = 1;
        public int Quantity
        {
            get => _quantity;
            set { _quantity = value; OnPropertyChanged(); }
        }

        private decimal _paymentAmount;
        public decimal PaymentAmount
        {
            get => _paymentAmount;
            set { _paymentAmount = value; OnPropertyChanged(); }
        }

        // ---------------------------
        // Loading Data
        // ---------------------------
        private async Task LoadAllAsync()
        {
            try
            {
                Clients.Clear();
                foreach (var c in await _clientRepo.GetAllAsync())
                    Clients.Add(c);

                Models.Clear();
                foreach (var m in await _modelRepo.GetAllAsync())
                    Models.Add(m);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async Task LoadClientInvoices()
        {
            if (SelectedClient == null) return;

            ClientInvoices.Clear();
            var invoices = await _invoiceService.GetClientInvoicesAsync(SelectedClient.ID);

            foreach (var inv in invoices)
                ClientInvoices.Add(inv);

            OnPropertyChanged(nameof(ClientInvoices)); // 🔥 ensure UI refresh
        }


        private async Task LoadClientSellItems()
        {
            if (SelectedClient == null) return;

            SellItems.Clear();
            var items = await _invoiceService.GetClientSellItemsAsync(SelectedClient.ID);

            foreach (var item in items)
                SellItems.Add(item);
        }

        // ---------------------------
        // Sell (Single-item invoice)
        // ---------------------------
        private async Task ProcessSellAsync()
        {
            if (SelectedClient == null)
            {
                MessageBox.Show("Select a client.");
                return;
            }

            if (SelectedModel == null)
            {
                MessageBox.Show("Select a model.");
                return;
            }

            if (Quantity <= 0)
            {
                MessageBox.Show("Quantity must be > 0.");
                return;
            }

            var item = new InvoiceItemViewDto
            {
                ModelId = SelectedModel.ID,
                Quantity = Quantity,
                UnitPrice = SelectedModel.SellPrice
            };

            await _invoiceService.AddSellInvoiceAsync(
                SelectedClient.ID,
                new List<InvoiceItemViewDto> { item },
                item.UnitPrice * item.Quantity,
                "Cash"
            );

            MessageBox.Show("Sell invoice created.");

            await LoadClientInvoices();
            await LoadClientSellItems();
        }

        // ---------------------------
        // Return (Single item)
        // ---------------------------
        private async Task ProcessReturnAsync()
        {
            if (SelectedClient == null)
            {
                MessageBox.Show("Select a client.");
                return;
            }

            if (SelectedModel == null)
            {
                MessageBox.Show("Select a model.");
                return;
            }

            if (Quantity <= 0)
            {
                MessageBox.Show("Quantity must be > 0.");
                return;
            }

            var confirm = MessageBox.Show("Confirm return?", "Confirm",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes) return;

            var item = new ReturnItemDto
            {
                ItemId = SelectedModel.ID,
                ItemName = SelectedModel.Name,
                OriginalQty = 0,   // optional – not needed
                ReturnQty = Quantity,
                Price = SelectedModel.SellPrice
            };

            var result = await _invoiceService.CreateReturnInvoiceAsync(
                SelectedClient.ID,
                new List<ReturnItemDto> { item }
            );

            if (!result.Success)
            {
                MessageBox.Show(result.Message);
                return;
            }

            MessageBox.Show("Return invoice created.");

            await LoadClientInvoices();
            await LoadClientSellItems();
        }

        // ---------------------------
        // Payment
        // ---------------------------
        private async Task ProcessPaymentAsync()
        {
            if (SelectedClient == null)
            {
                MessageBox.Show("Select a client.");
                return;
            }

            if (PaymentAmount <= 0)
            {
                MessageBox.Show("Enter a valid payment amount.");
                return;
            }

            await _invoiceService.AddPaymentAsync(SelectedClient.ID, PaymentAmount);

            MessageBox.Show("Payment recorded.");

            await LoadClientInvoices();
        }
    }
}
