using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class InvoiceItemViewDto
    {
        public int ModelId { get; set; }
        public string ModelName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; } // per-unit price
        public decimal Total => UnitPrice * Quantity; // total per item
    }
}
