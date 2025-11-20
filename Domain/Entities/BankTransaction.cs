using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class BankTransaction
    {
        public int ID { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public string Type { get; set; } // "Income" or "Outcome"
        public decimal Amount { get; set; }
        public decimal TotalAfterTransaction { get; set; }

        public int? InvoiceID { get; set; }
        public Invoice Invoice { get; set; }

        public int? MaterialID { get; set; }
        public Material Material { get; set; }

        public int? WorkerID { get; set; }
        public Worker Worker { get; set; }

        public int BankID { get; set; }
        public Bank Bank { get; set; }
    }

}
