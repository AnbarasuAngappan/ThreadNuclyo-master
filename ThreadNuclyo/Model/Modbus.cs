using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreadNuclyo.Model
{
    class Modbus
    {
        public byte DeviceID { get; set; }
        public string RiD { get; set; }
        public string Address { get; set; }
        public int Quantity { get; set; }

        public string DEAddress { get; set; }
        public int DEQuantity { get; set; }
        public string DEDatatype { get; set; }
    }
}
