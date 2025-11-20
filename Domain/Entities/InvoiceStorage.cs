using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class InvoiceStorage
    {
        public int InvoiceID { get; set; }
        public Invoice Invoice { get; set; }

        public int StorageID { get; set; }
        public Storage Storage { get; set; }
    }

}
