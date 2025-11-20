using Application.Services;
using Clothes_System.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace Clothes_System.Views
{
    public partial class MultiReturnInvoiceWindow : Window
    {
        public MultiReturnInvoiceWindow(int clientId)
        {
            InitializeComponent();

            // ✅ Get InvoiceService from DI
            var invoiceService = App.ServiceProvider.GetRequiredService<InvoiceService>();

            // ✅ Bind ViewModel and pass Close action
            DataContext = new MultiReturnInvoiceViewModel(clientId, invoiceService, Close);
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
