using Application.Services;
using Clothes_System.Helpers;
using Clothes_System.Views;
using Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Clothes_System.ViewModels
{
    public class WorkerViewModel : INotifyPropertyChanged
    {
        private readonly WorkerService _workerService;

        private ObservableCollection<Worker> _workers = new();
        public ObservableCollection<Worker> Workers
        {
            get => _workers;
            set
            {
                _workers = value;
                OnPropertyChanged(nameof(Workers));
            }
        }

        private Worker _selectedWorker;
        public Worker SelectedWorker
        {
            get => _selectedWorker;
            set
            {
                _selectedWorker = value;
                OnPropertyChanged(nameof(SelectedWorker));
            }
        }

        private Worker _newWorker = new();
        public Worker NewWorker
        {
            get => _newWorker;
            set
            {
                _newWorker = value;
                OnPropertyChanged(nameof(NewWorker));
            }
        }

        private decimal _paymentAmount;
        public decimal PaymentAmount
        {
            get => _paymentAmount;
            set
            {
                _paymentAmount = value;
                OnPropertyChanged(nameof(PaymentAmount));
            }
        }

        private string _selectedPaymentType = "Salary";
        public string SelectedPaymentType
        {
            get => _selectedPaymentType;
            set
            {
                _selectedPaymentType = value;
                OnPropertyChanged(nameof(SelectedPaymentType));
            }
        }

        public ICommand LoadWorkersCommand { get; }
        public ICommand AddWorkerCommand { get; }
        public ICommand AddWorkerPaymentCommand { get; }
        public ICommand OpenWorkerDetailsCommand { get; }

        public WorkerViewModel(WorkerService workerService)
        {
            _workerService = workerService;

            LoadWorkersCommand = new AsyncRelayCommand(LoadWorkers);
            AddWorkerCommand = new AsyncRelayCommand(AddWorker);
            AddWorkerPaymentCommand = new AsyncRelayCommand(AddWorkerPayment);
            OpenWorkerDetailsCommand = new RelayCommand<Worker>(OpenWorkerDetails);
        }

        private async Task LoadWorkers()
        {
            try
            {
                var workers = await _workerService.GetAllWorkersAsync();
                Workers = new ObservableCollection<Worker>(workers);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error Loading Workers", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task AddWorker()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(NewWorker.Name) || string.IsNullOrWhiteSpace(NewWorker.Ph_Number))
                {
                    MessageBox.Show("Please fill in name and phone number.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                await _workerService.AddWorkerAsync(NewWorker);
                MessageBox.Show("Worker added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                NewWorker = new Worker();
                OnPropertyChanged(nameof(NewWorker));

                await LoadWorkers();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error Adding Worker", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task AddWorkerPayment()
        {
            try
            {
                if (SelectedWorker == null)
                {
                    MessageBox.Show("Please select a worker first.", "No Worker Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (PaymentAmount <= 0)
                {
                    MessageBox.Show("Please enter a valid payment amount.", "Invalid Amount", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                await _workerService.AddWorkerPaymentAsync(SelectedWorker, PaymentAmount, SelectedPaymentType);

                MessageBox.Show($"{SelectedPaymentType} of {PaymentAmount:C} added for {SelectedWorker.Name}.",
                    "Payment Added", MessageBoxButton.OK, MessageBoxImage.Information);

                PaymentAmount = 0;
                OnPropertyChanged(nameof(PaymentAmount));

                await LoadWorkers();

                // Reload payments if WorkerDetails is open
                if (SelectedWorker != null)
                {
                    SelectedWorker = await _workerService.GetWorkerByIdAsync(SelectedWorker.ID);
                    OnPropertyChanged(nameof(SelectedWorker));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error Adding Payment", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenWorkerDetails(Worker worker)
        {
            if (worker == null)
            {
                MessageBox.Show("Invalid worker selected.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var workerService = App.ServiceProvider.GetRequiredService<WorkerService>();
            var detailsVm = new WorkerDetailsViewModel(worker, workerService);
            var window = new WorkerDetailsWindow { DataContext = detailsVm };
            window.ShowDialog();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
