using Application.DTOs;
using Application.Services;
using Clothes_System.Helpers;
using Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Clothes_System.ViewModels
{
    public class ClientDetailsViewModel : BaseViewModel
    {
        private readonly InvoiceService _invoiceService;

        public Clinte Client { get; }
        public ObservableCollection<InvoiceDisplayDto> Invoices { get; set; } = new();

        public ICommand OpenSellInvoiceCommand { get; }
        public ICommand OpenReturnInvoiceCommand { get; }
        public ICommand OpenPaymentInvoiceCommand { get; }

        public ClientDetailsViewModel(Clinte client, InvoiceService invoiceService)
        {
            Client = client ?? throw new ArgumentNullException(nameof(client));
            _invoiceService = invoiceService ?? throw new ArgumentNullException(nameof(invoiceService));

            OpenSellInvoiceCommand = new RelayCommand(async _ => await OpenSellAsync());
            OpenReturnInvoiceCommand = new RelayCommand(async _ => await OpenReturnAsync());
            OpenPaymentInvoiceCommand = new RelayCommand(async _ => await OpenPaymentAsync());

            _ = LoadInvoicesAsync();
        }

        public async Task LoadInvoicesAsync()
        {
            try
            {
                Invoices.Clear();

                if (Client.ID == 0)
                    return;

                var list = await _invoiceService.GetClientInvoicesAsync(Client.ID);

                if (list != null && list.Any())
                {
                    foreach (var i in list)
                    {
                        i.PropertyChanged += Invoice_PropertyChanged;
                        Invoices.Add(i);
                    }
                }


                OnPropertyChanged(nameof(Invoices));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading invoices: {ex.Message}", "Error");
            }
        }

        private async Task OpenSellAsync()
        {
            var win = new Views.SellInvoiceWindow(Client.ID);
            win.ShowDialog();
            await LoadInvoicesAsync();
            await RefreshClientAsync();
        }

        private async Task OpenReturnAsync()
        {
            var win = new Views.MultiReturnInvoiceWindow(Client.ID);
            win.ShowDialog();
            await LoadInvoicesAsync();
            await RefreshClientAsync();
        }

        private async Task OpenPaymentAsync()
        {
            var win = new Views.PaymentInvoiceWindow(Client.ID);
            win.ShowDialog();
            await LoadInvoicesAsync();
            await RefreshClientAsync();
        }

        private async Task RefreshClientAsync()
        {
            var repo = App.ServiceProvider.GetRequiredService<Domain.Interfaces.IRepository<Clinte>>();
            var updatedClient = await repo.GetByIdAsync(Client.ID);

            if (updatedClient != null)
            {
                Client.Debt = updatedClient.Debt;
                Client.Name = updatedClient.Name;
                Client.Ph_Number = updatedClient.Ph_Number;

                OnPropertyChanged(nameof(Client));
            }
        }

        private async void Invoice_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(InvoiceDisplayDto.Date))
                return;

            if (sender is not InvoiceDisplayDto dto)
                return;

            try
            {
                // fetch repository
                var repo = App.ServiceProvider.GetRequiredService<Domain.Interfaces.IRepository<Invoice>>();
                var dbInvoice = await repo.GetByIdAsync(dto.Id);

                if (dbInvoice != null)
                {
                    dbInvoice.Date = dto.Date; // <-- THIS SAVES THE NEW VALUE
                    repo.Update(dbInvoice);
                    await repo.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating date: {ex.Message}");
            }
        }

    }
}
