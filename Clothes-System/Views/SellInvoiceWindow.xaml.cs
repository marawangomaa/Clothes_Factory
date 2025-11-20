using Application.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Clothes_System.Views
{
    /// <summary>
    /// Interaction logic for SellInvoiceWindow.xaml
    /// </summary>
    public partial class SellInvoiceWindow : Window
    {
        public SellInvoiceWindow(int clientId)
        {
            InitializeComponent();

            var invoiceService = App.ServiceProvider.GetRequiredService<InvoiceService>();
            var modelService = App.ServiceProvider.GetRequiredService<ModelService>();

            DataContext = new ViewModels.SellInvoiceViewModel(clientId, invoiceService, modelService, this);
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => Close();
    }
}
