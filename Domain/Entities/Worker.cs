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

        // Daily pieces only for non-weekly workers
        public ObservableCollection<WorkerPiece> DailyPieces { get; set; } = new ();

        public ObservableCollection<BankTransaction> BankTransactions { get; set; } = new ();
    }

}
