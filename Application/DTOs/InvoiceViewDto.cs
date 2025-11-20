using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class InvoiceViewDto
    {
        public int InvoiceId { get; set; }
        public string InvoiceType { get; set; } = string.Empty; // Sell, Return, Payment
        public DateTime Date { get; set; }
        public List<InvoiceItemViewDto> Items { get; set; } = new();
        public decimal TotalAmount => Items.Sum(i => i.Total); // sum of all items
        public decimal PaidAmount { get; set; } // total paid
        public decimal RemainingAmount => TotalAmount - PaidAmount; // debt left
        public string PaymentMethod { get; set; } = string.Empty;
    }
}
