using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UberDLMX
{
    class GxCommand
    {
        //public GxCommand();

        //
        // Summary:
        //     Command line parameter tag
        public char Tag { get; set; }
        //
        // Summary:
        //     Command line parameter value.
        public string Value { get; set; }
        //
        // Summary:
        //     Parameter is missing.
        public bool Missing { get; set; }

        //
        // Summary:
        //     Command line parameter as a string.
        //public override string ToString();
    }
}
