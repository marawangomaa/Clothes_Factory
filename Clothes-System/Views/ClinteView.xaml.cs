using Clothes_System.ViewModels;
using Application.Services;
using System.Windows;
using System.Windows.Controls;

namespace Clothes_System.Views
{
    public partial class ClinteView : Page
    {
        public ClinteView(ClinteService clinteService, InvoiceService invoiceService)
        {
            InitializeComponent();
            DataContext = new ClinteViewModel(clinteService, invoiceService);
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            // Navigate back to the MenuPage
            var main = (MainWindow)System.Windows.Application.Current.MainWindow;
            main.Navigate(new MenuPage());
        }
    }
}
