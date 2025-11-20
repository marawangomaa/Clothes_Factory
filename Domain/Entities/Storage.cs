using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Storage
    {
        public int ID { get; set; }
        public string Product_Type { get; set; }
        public string Product_Name { get; set; }
        public int Number_Of_Products { get; set; }

        //public ICollection<Material> Materials { get; set; }
        public ICollection<Model> Models { get; set; }
        public ICollection<InvoiceStorage> InvoiceStorages { get; set; }
    }

}
