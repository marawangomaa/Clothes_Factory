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
                    (SaveCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
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

        public ICommand SaveCommand { get; }

        public PaymentInvoiceViewModel(int clientId, InvoiceService invoiceService, Window owner)
        {
            _clientId = clientId;
            _invoiceService = invoiceService ?? throw new ArgumentNullException(nameof(invoiceService));
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));

            SaveCommand = new AsyncRelayCommand(Save, CanSave);
        }

        private bool CanSave()
        {
            return Amount > 0;
        }

        private async Task Save()
        {
            try
            {
                if (Amount <= 0)
                {
                    ErrorMessage = "Please enter a valid amount greater than zero.";
                    return;
                }

                ErrorMessage = "Processing payment...";

                await _invoiceService.AddPaymentAsync(
                    _clientId,
                    Amount,
                    SelectedPaymentMethod
                );

                MessageBox.Show(
                    $"Payment of {Amount:C} recorded successfully!",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                _owner?.Close();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Payment failed: {ex.Message}";
                MessageBox.Show(
                    $"Failed to save payment.\n\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}