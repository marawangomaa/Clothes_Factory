using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Bank
    {
        public int ID { get; set; }
        public decimal TotalAmount { get; set; }

        // One bank → many transactions
        public ICollection<BankTransaction> Transactions { get; set; } = new List<BankTransaction>();
    }

}
