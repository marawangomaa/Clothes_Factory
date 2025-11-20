using Application.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace Clothes_System.Views
{
    public partial class PaymentInvoiceWindow : Window
    {
        public PaymentInvoiceWindow(int clientId)
        {
            InitializeComponent();

            var invoiceService = App.ServiceProvider.GetRequiredService<InvoiceService>();

            DataContext = new ViewModels.PaymentInvoiceViewModel(
                clientId,
                invoiceService,
                this
            );
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
