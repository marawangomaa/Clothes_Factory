using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Material
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }       // Price per batch
        public int Quantity { get; set; }
        public decimal TotalCost { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;

        public int BankID { get; set; }
        public Bank Bank { get; set; }
    }

}
