using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Invoice
    {
        public int ID { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public string Number { get; set; }
        public string PaymentMethod { get; set; } // Cash, Credit, etc.
        public decimal TotalAmount { get; set; }
        public string InvoiceType { get; set; } // Buy, Sell, Return, Payment
        public string Type { get; set; } 
        public string? Description { get; set; }

        public int ClinteID { get; set; }
        public Clinte Clinte { get; set; }

        public int BankID { get; set; }
        public Bank Bank { get; set; }

        public ICollection<InvoiceModel> InvoiceModels { get; set; } = new List<InvoiceModel>();
        public ICollection<BankTransaction> Transactions { get; set; } = new List<BankTransaction>();
        public ICollection<InvoiceStorage> InvoiceStorages { get; set; } = new List<InvoiceStorage>();
    }
}
