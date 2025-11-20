using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Clinte
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Ph_Number { get; set; }
        public string Location { get; set; }
        public decimal Debt { get; set; }

        public ICollection<Invoice> Invoices { get; set; }
    }

}
