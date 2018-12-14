using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UberDLMX.Model
{
    class Configuration
    {
        public string Manufacturer { get; set; }
        public string ClientAdd { get; set; }
        public string Name { get; set; }
        public string Authentication { get; set; }
        public string Password { get; set; }
        public string SObisValue { get; set; }
        public string RObisValue { get; set; }
        public string Model { get; set; }
        public int ImportExport { get; set; }

        public string IpAddress { get; set; }
        public int Port { get; set; }

        //"Manufacturer": "HPL TEST ANBU Export(Reading)",
        //"ClientAdd":"32", 
        //"Name": "ln",
        //"Authentication": "Low",
        //"Password": "1111111111111111",
        //"SObisValue": "0.0.96.1.0.255:2",
        //"RObisValue": "1.0.1.8.0.255:2", 
        //"Model":"HPL TEST ANBU Export", 
        //"ImportExport":"2"
    }
}
