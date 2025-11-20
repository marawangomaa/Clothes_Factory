using Application.Services;
using Clothes_System.Helpers;
using Clothes_System.Views;
using Domain.Entities;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

namespace Clothes_System.ViewModels
{
    public class ClinteViewModel : INotifyPropertyChanged
    {
        private readonly ClinteService _clinteService;
        private readonly InvoiceService _invoiceService;

        public ObservableCollection<Clinte> Clintes { get; set; } = new();

        private Clinte _newClinte = new();
        public Clinte NewClinte
        {
            get => _newClinte;
            set { _newClinte = value; OnPropertyChanged(); }
        }

        private Clinte _selectedClinte;
        public Clinte SelectedClinte
        {
            get => _selectedClinte;
            set { _selectedClinte = value; OnPropertyChanged(); }
        }

        public ICommand LoadClientsCommand { get; }
        public ICommand AddClinteCommand { get; }
        public ICommand EditClinteCommand { get; }
        public ICommand DeleteClinteCommand { get; }
        public ICommand OpenDetailsCommand { get; }

        public ClinteViewModel(ClinteService clinteService, InvoiceService invoiceService)
        {
            _clinteService = clinteService;
            _invoiceService = invoiceService;

            LoadClientsCommand = new AsyncRelayCommand(LoadClientsAsync);
            AddClinteCommand = new AsyncRelayCommand(AddClinteAsync);
            EditClinteCommand = new AsyncRelayCommand(EditClinteAsync);
            DeleteClinteCommand = new AsyncRelayCommand(DeleteClinteAsync);
            OpenDetailsCommand = new RelayCommand(OpenDetails, CanOpenDetails);

            _ = LoadClientsAsync();
        }

        private bool CanOpenDetails(object? obj)
        {
            return SelectedClinte != null;
        }

        private void OpenDetails(object? obj)
        {
            var page = new ClientDetailsView(SelectedClinte);

            var frame = System.Windows.Application.Current.MainWindow.FindName("MainFrame")
                         as System.Windows.Controls.Frame;

            frame?.Navigate(page);
        }

        private async Task LoadClientsAsync()
        {
            Clintes.Clear();
            var clients = await _clinteService.GetAllClinetsAsync();
            foreach (var c in clients)
                Clintes.Add(c);
        }

        private async Task AddClinteAsync()
        {
            if (NewClinte == null ||
                string.IsNullOrWhiteSpace(NewClinte.Name) ||
                string.IsNullOrWhiteSpace(NewClinte.Ph_Number))
                return;

            await _clinteService.AddClinteAsync(NewClinte);
            await LoadClientsAsync();
            NewClinte = new Clinte();
        }

        private async Task EditClinteAsync()
        {
            if (SelectedClinte == null) return;
            await _clinteService.UpdateClinteAsync(SelectedClinte);
            await LoadClientsAsync();
        }

        private async Task DeleteClinteAsync()
        {
            if (SelectedClinte == null) return;
            await _clinteService.DeleteClinteAsync(SelectedClinte);
            await LoadClientsAsync();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
