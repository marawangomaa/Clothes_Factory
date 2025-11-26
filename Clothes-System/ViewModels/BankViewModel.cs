using Application.Services;
using Clothes_System.Helpers;
using Domain.Entities;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

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

    private decimal _newAmount;
    public decimal NewAmount
    {
        get => _newAmount;
        set
        {
            if (_newAmount != value)
            {
                _newAmount = value;
                OnPropertyChanged(nameof(NewAmount));
                OnPropertyChanged(nameof(AmountDifference));
            }
        }
    }

    public string AmountDifference
    {
        get
        {
            if (NewAmount == 0) return "";
            decimal difference = NewAmount - Amount;
            return difference >= 0 ?
                $"+{difference:C}" :
                $"{difference:C}";
        }
    }

    public ObservableCollection<BankTransaction> Trades { get; set; } = new();

    public ICommand LoadBankCommand { get; }
    public ICommand UpdateAmountCommand { get; }

    public BankViewModel(BankService bankService)
    {
        _bankService = bankService;
        LoadBankCommand = new AsyncRelayCommand(async () => await LoadBank());
        UpdateAmountCommand = new AsyncRelayCommand(async () => await UpdateAmount(), () => NewAmount >= 0);
    }

    private async Task LoadBank()
    {
        var bank = await _bankService.GetOrCreateBankAsync();
        Amount = bank.TotalAmount;
        NewAmount = 0; // Reset the input field

        Trades.Clear();
        var transactions = (await _bankService.GetAllTransactionsAsync())
                            .OrderByDescending(t => t.Date);
        foreach (var t in transactions)
            Trades.Add(t);

        OnPropertyChanged(nameof(AmountDifference));
    }

    private async Task UpdateAmount()
    {
        try
        {
            if (NewAmount < 0)
            {
                // Show error message
                return;
            }

            await _bankService.UpdateBankTotalAsync(NewAmount);
            await LoadBank(); // Refresh the data

            // Show success message
        }
        catch (Exception ex)
        {
            // Show error message
            Console.WriteLine($"Error updating bank amount: {ex.Message}");
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}