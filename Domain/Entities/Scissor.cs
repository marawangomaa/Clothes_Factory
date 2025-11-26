using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Scissor
    {
        public int ID { get; set; }
        public string Number { get; set; }
        public string Model { get; set; }
        public int ModelMetrag { get; set; } // ✅ Add this property
        public DateTime Date { get; set; }
    }

}
