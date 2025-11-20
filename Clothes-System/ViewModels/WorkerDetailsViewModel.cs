using Application.Services;
using Clothes_System.Helpers;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Clothes_System.ViewModels
{
    public class WorkerDetailsViewModel : INotifyPropertyChanged
    {
        private readonly WorkerService _workerService;

        public Worker Worker { get; private set; }
        public decimal PaymentAmount { get; set; }
        public string SelectedPaymentType { get; set; } = "Salary";
        public int NewWorkerPieces { get; set; }
        public List<string> PaymentTypes { get; } = new() { "Salary", "Debt" };

        public ICommand AddWorkerPieceCommand { get; }
        public ICommand AddWorkerPaymentCommand { get; }

        public WorkerDetailsViewModel(Worker worker, WorkerService workerService)
        {
            Worker = worker;
            _workerService = workerService;

            AddWorkerPieceCommand = new AsyncRelayCommand(AddWorkerPiece);
            AddWorkerPaymentCommand = new AsyncRelayCommand(AddWorkerPayment);
        }

        private async Task AddWorkerPiece()
        {
            try
            {
                if (Worker == null || NewWorkerPieces <= 0)
                {
                    MessageBox.Show("Please enter a valid number of pieces.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                await _workerService.AddWorkerPieceAsync(Worker, NewWorkerPieces);

                Worker = await _workerService.GetWorkerByIdAsync(Worker.ID);
                OnPropertyChanged(nameof(Worker));

                MessageBox.Show($"✅ Added {NewWorkerPieces} pieces for {Worker.Name}.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                NewWorkerPieces = 0;
                OnPropertyChanged(nameof(NewWorkerPieces));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error Adding Pieces", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task AddWorkerPayment()
        {
            try
            {
                if (PaymentAmount <= 0)
                {
                    MessageBox.Show("Please enter a valid payment amount.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                Worker = await _workerService.AddWorkerPaymentAsync(Worker, PaymentAmount, SelectedPaymentType);
                OnPropertyChanged(nameof(Worker));

                MessageBox.Show($"💰 Payment of {PaymentAmount:C} recorded for {Worker.Name}.",
                    "Payment Added", MessageBoxButton.OK, MessageBoxImage.Information);

                PaymentAmount = 0;
                OnPropertyChanged(nameof(PaymentAmount));
                SelectedPaymentType = "Salary";
                OnPropertyChanged(nameof(SelectedPaymentType));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error Adding Payment", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
