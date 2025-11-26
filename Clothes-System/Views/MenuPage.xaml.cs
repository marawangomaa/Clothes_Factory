using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Clothes_System.Views;
using Clothes_System.ViewModels;
using Application.Services;

namespace Clothes_System.Views
{
    public partial class MenuPage : Page
    {
        private readonly ClinteService _clinteService;
        private readonly InvoiceService _invoiceService;
        private readonly MaterialService _materialService;
        private readonly WorkerService _workerService;
        private readonly StorageService _storageService;
        private readonly BankService _bankService;
        private readonly BankViewModel _bankViewModel;

        public MenuPage()
        {
            InitializeComponent();

            // Get all services from DI
            _clinteService = App.ServiceProvider.GetRequiredService<ClinteService>();
            _invoiceService = App.ServiceProvider.GetRequiredService<InvoiceService>();
            _materialService = App.ServiceProvider.GetRequiredService<MaterialService>();
            _workerService = App.ServiceProvider.GetRequiredService<WorkerService>();
            _storageService = App.ServiceProvider.GetRequiredService<StorageService>();
            _bankService = App.ServiceProvider.GetRequiredService<BankService>();
            _bankViewModel = App.ServiceProvider.GetRequiredService<BankViewModel>();
        }

        private MainWindow Main => (MainWindow)System.Windows.Application.Current.MainWindow;

        private void Clients_Click(object sender, RoutedEventArgs e)
        {
            Main.Navigate(new ClinteView(_clinteService, _invoiceService));
        }

        private void Materials_Click(object sender, RoutedEventArgs e)
        {
            Main.Navigate(new MaterialView(_materialService));
        }

        private void Workers_Click(object sender, RoutedEventArgs e)
        {
            Main.Navigate(new WorkerView(_workerService));
        }

        private void Storage_Click(object sender, RoutedEventArgs e)
        {
            Main.Navigate(new StorageView(_storageService, _materialService));
        }

        private void Bank_Click(object sender, RoutedEventArgs e)
        {
            Main.Navigate(new BankView(_bankViewModel));
        }
        private void BAnkUpdate_Click(object sender, RoutedEventArgs e)
        {
            Main.Navigate(new BankUpdateView(_bankViewModel));
        }

        // Add missing button handlers
        private void Invoices_Click(object sender, RoutedEventArgs e)
        {
            Main.Navigate(new InvoiceView());
        }

        private void Models_Click(object sender, RoutedEventArgs e)
        {
            Main.Navigate(new ModelView());
        }

        private void Scissors_Click(object sender, RoutedEventArgs e)
        {
            Main.Navigate(new ScissorView());
        }

        private async void Backup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var backupService = App.GetService<BackupService>();
                var result = await backupService.CreateBackupAsync();

                if (result.Success)
                {
                    MessageBox.Show(result.Message, "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(result.Message, "تحذير", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في النسخ الاحتياطي: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
