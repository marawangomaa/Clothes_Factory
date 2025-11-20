using Application.DTOs;
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class InvoiceDisplayDto : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string prop = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount => TotalAmount - PaidAmount;
    public string PaymentMethod { get; set; } = string.Empty;
    public List<InvoiceItemViewDto> Items { get; set; } = new();

    private DateTime _date;
    public DateTime Date
    {
        get => _date;
        set
        {
            if (_date != value)
            {
                _date = value;
                OnPropertyChanged();
            }
        }
    }
}
