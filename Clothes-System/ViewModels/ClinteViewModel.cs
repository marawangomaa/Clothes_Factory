using Application.Services;
using Clothes_System.Helpers;
using Clothes_System.Views;
using Domain.Entities;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

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
            set
            {
                _selectedClinte = value;
                OnPropertyChanged();
                // CommandManager will automatically refresh commands due to property change
            }
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
            DeleteClinteCommand = new RelayCommand(DeleteClinte, CanDeleteClinte);
            OpenDetailsCommand = new RelayCommand(OpenDetails, CanOpenDetails);

            _ = LoadClientsAsync();
        }

        private bool CanOpenDetails(object obj)
        {
            return SelectedClinte != null;
        }

        private bool CanDeleteClinte(object obj)
        {
            return SelectedClinte != null;
        }

        private void OpenDetails(object obj)
        {
            if (SelectedClinte == null)
            {
                MessageBox.Show("يرجى اختيار عميل أولاً", "تحذير", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var page = new ClientDetailsView(SelectedClinte);
            var frame = System.Windows.Application.Current.MainWindow.FindName("MainFrame") as System.Windows.Controls.Frame;
            frame?.Navigate(page);
        }

        private async Task LoadClientsAsync()
        {
            try
            {
                Clintes.Clear();
                var clients = await _clinteService.GetAllClinetsAsync();
                foreach (var c in clients)
                    Clintes.Add(c);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل العملاء: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task AddClinteAsync()
        {
            try
            {
                if (NewClinte == null ||
                    string.IsNullOrWhiteSpace(NewClinte.Name) ||
                    string.IsNullOrWhiteSpace(NewClinte.Ph_Number) ||
                    string.IsNullOrWhiteSpace(NewClinte.Location))
                {
                    MessageBox.Show("يرجى ملء جميع الحقول المطلوبة", "تحذير", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = await _clinteService.AddClinteAsync(NewClinte);

                if (result.Success)
                {
                    MessageBox.Show(result.Message, "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadClientsAsync();
                    NewClinte = new Clinte();
                }
                else
                {
                    MessageBox.Show(result.Message, "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في إضافة العميل: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task EditClinteAsync()
        {
            if (SelectedClinte == null)
            {
                MessageBox.Show("يرجى اختيار عميل أولاً", "تحذير", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var result = await _clinteService.UpdateClinteAsync(SelectedClinte);

                if (result.Success)
                {
                    MessageBox.Show(result.Message, "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadClientsAsync();
                }
                else
                {
                    MessageBox.Show(result.Message, "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تعديل العميل: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DeleteClinte(object obj)
        {
            if (SelectedClinte == null)
            {
                MessageBox.Show("يرجى اختيار عميل أولاً", "تحذير", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var result = MessageBox.Show(
                    $"هل أنت متأكد من حذف العميل '{SelectedClinte.Name}'؟",
                    "تأكيد الحذف",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    var deleteResult = await _clinteService.DeleteClinteAsync(SelectedClinte);

                    if (deleteResult.Success)
                    {
                        MessageBox.Show(deleteResult.Message, "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                        await LoadClientsAsync();
                    }
                    else
                    {
                        MessageBox.Show(deleteResult.Message, "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في حذف العميل: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}