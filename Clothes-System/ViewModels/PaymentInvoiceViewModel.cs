using Application.Services;
using Clothes_System.Helpers;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Clothes_System.ViewModels
{
    public class PaymentInvoiceViewModel : BaseViewModel
    {
        private readonly InvoiceService _invoiceService;
        private readonly Window _owner;
        private readonly int _clientId;

        // PAYMENT METHOD LIST
        public string[] PaymentTypes { get; } =
        {
            "cash",
            "instapay",
            "vodafonecash"
        };

        private string _selectedPaymentMethod = "cash";
        public string SelectedPaymentMethod
        {
            get => _selectedPaymentMethod;
            set
            {
                _selectedPaymentMethod = value;
                OnPropertyChanged();
            }
        }

        private decimal _amount;
        public decimal Amount
        {
            get => _amount;
            set
            {
                if (_amount != value)
                {
                    _amount = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand SaveCommand { get; }

        public PaymentInvoiceViewModel(int clientId, InvoiceService invoiceService, Window owner)
        {
            _clientId = clientId;
            _invoiceService = invoiceService ?? throw new ArgumentNullException(nameof(invoiceService));
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));

            SaveCommand = new AsyncRelayCommand(Save);
        }

        private async Task Save()
        {
            try
            {
                if (Amount <= 0)
                {
                    MessageBox.Show(
                        "Please enter a valid amount greater than zero.",
                        "Validation",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                await _invoiceService.AddPaymentAsync(
                    _clientId,
                    Amount,
                    SelectedPaymentMethod // <-- PAYMENT METHOD INCLUDED
                );

                MessageBox.Show(
                    "Payment recorded successfully.",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                _owner?.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to save payment.\n\n{ex.Message}",
                    "ERROR",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
