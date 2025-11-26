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

        public ICommand LoadWorkersCommand { get; }
        public ICommand AddWorkerCommand { get; }
        public ICommand OpenWorkerDetailsCommand { get; }

        public WorkerViewModel(WorkerService workerService)
        {
            _workerService = workerService;

            LoadWorkersCommand = new AsyncRelayCommand(LoadWorkers);
            AddWorkerCommand = new AsyncRelayCommand(AddWorker);
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
                MessageBox.Show(ex.Message, "خطأ في تحميل العمال", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task AddWorker()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(NewWorker.Name) || string.IsNullOrWhiteSpace(NewWorker.Ph_Number))
                {
                    MessageBox.Show("يرجى ملء الاسم ورقم الهاتف.", "خطأ في التحقق", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                await _workerService.AddWorkerAsync(NewWorker);
                MessageBox.Show("تم إضافة العامل بنجاح!", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);

                NewWorker = new Worker();
                OnPropertyChanged(nameof(NewWorker));

                await LoadWorkers();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "خطأ في إضافة العامل", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenWorkerDetails(Worker worker)
        {
            if (worker == null)
            {
                MessageBox.Show("عامل غير صالح.", "خطأ", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var workerService = App.ServiceProvider.GetRequiredService<WorkerService>();
            var modelService = App.ServiceProvider.GetRequiredService<ModelService>();
            var detailsVm = new WorkerDetailsViewModel(worker, workerService, modelService);
            var window = new WorkerDetailsWindow { DataContext = detailsVm };
            window.ShowDialog();

            // Refresh the workers list after closing details window
            _ = LoadWorkers();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}