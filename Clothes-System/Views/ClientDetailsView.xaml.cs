using Application.Services;
using Clothes_System.ViewModels;
using Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;

namespace Clothes_System.Views
{
    public partial class ClientDetailsView : Page
    {
        public ClientDetailsView(Clinte client)
        {
            InitializeComponent();

            var invoiceService = App.ServiceProvider.GetRequiredService<InvoiceService>();

            DataContext = new ClientDetailsViewModel(client, invoiceService);
        }
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            // Navigate back to the MenuPage
            var main = (MainWindow)System.Windows.Application.Current.MainWindow;
            main.Navigate(new MenuPage());
        }
        private void InvoicesDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is DataGrid grid && grid.SelectedItem is InvoiceDisplayDto selectedInvoice)
            {
                // Open the invoice details window
                var win = new InvoiceDetailsWindow(selectedInvoice);
                win.ShowDialog();
            }
        }
    }
}
