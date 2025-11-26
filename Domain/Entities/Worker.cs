using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Worker
    {
        public int ID { get; set; }
        public bool Is_Weekly { get; set; }
        public string Ph_Number { get; set; }
        public string Name { get; set; }
        public decimal Price_Count { get; set; }
        public decimal TotalPaidAmount { get; set; } // ✅ NEW: Track total paid amount separately

        // Daily pieces with model details for non-weekly workers
        public ObservableCollection<WorkerPieceDetail> DailyPieces { get; set; } = new();

        public ObservableCollection<BankTransaction> BankTransactions { get; set; } = new();

        // ✅ Calculated property for total owed amount from CURRENT pieces only
        public decimal TotalOwedAmount => DailyPieces?.Sum(p => p.TotalAmount) ?? 0;

        // ✅ Calculated property for remaining amount using the new TotalPaidAmount
        public decimal RemainingAmount => Math.Max(TotalOwedAmount - TotalPaidAmount, 0);

        // ✅ Property to show total payments received (uses the new property)
        public decimal TotalPaymentsReceived => TotalPaidAmount;
    }
}