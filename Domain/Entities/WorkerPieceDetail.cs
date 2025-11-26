using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class WorkerPieceDetail
    {
        public int ID { get; set; }
        public int WorkerID { get; set; }
        public Worker Worker { get; set; }

        public int ModelID { get; set; }
        public Model Model { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;
        public int Pieces { get; set; }
        public decimal TotalAmount { get; set; } // Pieces * Model.MakingPrice
    }
}
