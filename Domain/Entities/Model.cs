using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Model
    {
        public int ID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string? Image { get; set; }
        public decimal Cost { get; set; }
        public int? Metrag { get; set; }
        public decimal SellPrice { get; set; }
        public decimal MakingPrice { get; set; } // ✅ Add this property
        public int? Quantity { get; set; } // available in storage

        public int? StorageID { get; set; }
        public Storage Storage { get; set; }

        public ICollection<InvoiceModel> InvoiceModels { get; set; } = new List<InvoiceModel>();

        public string? Notes { get; set; }
    }

}
