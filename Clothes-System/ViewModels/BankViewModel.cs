using Application.Services;
using Clothes_System.Helpers;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Clothes_System.ViewModels
{
    public class BankViewModel : INotifyPropertyChanged
    {
        private readonly BankService _bankService;

        private decimal _amount;
        public decimal Amount
        {
            get => _amount;
            set
            {
                if (_amount != value)
                {
                    _amount = value;
                    OnPropertyChanged(nameof(Amount));
                }
            }
        }

        public ObservableCollection<BankTransaction> Trades { get; set; } = new();

        public ICommand LoadBankCommand { get; }

        public BankViewModel(BankService bankService)
        {
            _bankService = bankService;
            LoadBankCommand = new AsyncRelayCommand(async () => await LoadBank());
        }

        private async Task LoadBank()
        {
            var bank = await _bankService.GetOrCreateBankAsync();
            Amount = bank.TotalAmount;

            Trades.Clear();
            var transactions = (await _bankService.GetAllTransactionsAsync())
                                .OrderByDescending(t => t.Date);
            foreach (var t in transactions)
                Trades.Add(t);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
