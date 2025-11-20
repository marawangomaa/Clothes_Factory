using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class InvoiceModel
    {
        public int ID { get; set; }

        public int InvoiceID { get; set; }
        public Invoice Invoice { get; set; }

        public int ModelID { get; set; }
        public Model Model { get; set; }

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice => UnitPrice * Quantity;
    }


}
