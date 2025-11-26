using Application.Services;
using Clothes_System.Helpers;
using Domain.Entities;
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
    public class WorkerDetailsViewModel : INotifyPropertyChanged
    {
        private readonly WorkerService _workerService;
        private readonly ModelService _modelService;

        private Worker _worker;
        public Worker Worker
        {
            get => _worker;
            private set
            {
                _worker = value;
                OnPropertyChanged(nameof(Worker));
                OnPropertyChanged(nameof(TotalOwedAmount));
                OnPropertyChanged(nameof(RemainingAmount));
                OnPropertyChanged(nameof(TotalPaymentsReceived));
            }
        }

        public decimal TotalOwedAmount => Worker?.TotalOwedAmount ?? 0;
        public decimal RemainingAmount => Worker?.RemainingAmount ?? 0;
        public decimal TotalPaymentsReceived => Worker?.TotalPaymentsReceived ?? 0;

        public decimal PaymentAmount { get; set; }
        public string SelectedPaymentType { get; set; } = "راتب";
        public int NewWorkerPieces { get; set; }
        public Model SelectedModel { get; set; }

        public List<string> PaymentTypes { get; } = new() { "راتب", "ديون" };
        public ObservableCollection<Model> Models { get; } = new();

        public ICommand AddWorkerPieceCommand { get; }
        public ICommand AddWorkerPaymentCommand { get; }
        public ICommand ResetWorkerCommand { get; }
        public ICommand LoadModelsCommand { get; }

        public WorkerDetailsViewModel(Worker worker, WorkerService workerService, ModelService modelService)
        {
            Worker = worker;
            _workerService = workerService;
            _modelService = modelService;

            AddWorkerPieceCommand = new AsyncRelayCommand(AddWorkerPiece);
            AddWorkerPaymentCommand = new AsyncRelayCommand(AddWorkerPayment);
            ResetWorkerCommand = new AsyncRelayCommand(ResetWorker);
            LoadModelsCommand = new AsyncRelayCommand(LoadModels);

            _ = LoadModels();
        }

        private async Task LoadModels()
        {
            try
            {
                var models = await _modelService.GetAllModelsAsync();
                Models.Clear();
                foreach (var model in models)
                {
                    Models.Add(model);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل النماذج: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task AddWorkerPiece()
        {
            try
            {
                if (Worker == null || SelectedModel == null || NewWorkerPieces <= 0)
                {
                    MessageBox.Show("يرجى اختيار نموذج وإدخال عدد قطع صالح.", "إدخال غير صالح", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                await _workerService.AddWorkerPieceAsync(Worker, SelectedModel.ID, NewWorkerPieces);

                // Reload worker to get fresh data and avoid duplicates
                Worker = await _workerService.GetWorkerByIdAsync(Worker.ID);
                RefreshAllProperties();

                decimal totalAmount = NewWorkerPieces * SelectedModel.MakingPrice;
                MessageBox.Show($"✅ تم إضافة {NewWorkerPieces} قطعة من {SelectedModel.Name}.\nالمبلغ المستحق: {totalAmount:C}", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);

                NewWorkerPieces = 0;
                SelectedModel = null;
                OnPropertyChanged(nameof(NewWorkerPieces));
                OnPropertyChanged(nameof(SelectedModel));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "خطأ في إضافة القطع", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task AddWorkerPayment()
        {
            try
            {
                if (PaymentAmount <= 0)
                {
                    MessageBox.Show("يرجى إدخال مبلغ صالح.", "إدخال غير صالح", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                Worker = await _workerService.AddWorkerPaymentAsync(Worker, PaymentAmount, SelectedPaymentType);
                RefreshAllProperties();

                MessageBox.Show($"💰 تم تسديد {PaymentAmount:C} للعامل {Worker.Name}.\nالمبلغ المتبقي: {RemainingAmount:C}",
                    "تمت الإضافة", MessageBoxButton.OK, MessageBoxImage.Information);

                PaymentAmount = 0;
                SelectedPaymentType = "راتب";
                OnPropertyChanged(nameof(PaymentAmount));
                OnPropertyChanged(nameof(SelectedPaymentType));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "خطأ في إضافة الدفعة", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ResetWorker()
        {
            try
            {
                if (Worker == null) return;

                var result = MessageBox.Show($"هل أنت متأكد من إعادة تعيين سجلات العامل {Worker.Name}؟\nسيتم حذف جميع القطع والحسابات ولكن تبقى الدفعات في البنك.",
                    "تأكيد إعادة التعيين", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    if (Worker.Is_Weekly)
                    {
                        await _workerService.ResetWeeklyWorkerRecordsAsync(Worker);
                    }
                    else
                    {
                        await _workerService.ResetWorkerRecordsAsync(Worker);
                    }

                    Worker = await _workerService.GetWorkerByIdAsync(Worker.ID);
                    RefreshAllProperties();

                    MessageBox.Show("✅ تم إعادة تعيين سجلات العامل بنجاح.", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "خطأ في إعادة التعيين", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshAllProperties()
        {
            OnPropertyChanged(nameof(Worker));
            OnPropertyChanged(nameof(TotalOwedAmount));
            OnPropertyChanged(nameof(RemainingAmount));
            OnPropertyChanged(nameof(TotalPaymentsReceived));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}