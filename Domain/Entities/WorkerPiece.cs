using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class WorkerPiece
    {
        public int ID { get; set; }
        public int WorkerID { get; set; }
        public Worker Worker { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;
        public int Pieces { get; set; }
    }
}
