using System;
using System.ComponentModel;

namespace Application.DTOs
{
    public class ReturnItemDto : INotifyPropertyChanged
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public int OriginalQty { get; set; } // How many the client owns

        private int _returnQty;
        public int ReturnQty
        {
            get => _returnQty;
            set
            {
                if (value > OriginalQty) value = OriginalQty; // can't return more than owned

                if (_returnQty != value)
                {
                    _returnQty = value;
                    OnPropertyChanged(nameof(ReturnQty));
                    OnPropertyChanged(nameof(Total));
                }
            }
        }

        public decimal Price { get; set; } // Unit price

        public decimal Total => ReturnQty * Price;

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
