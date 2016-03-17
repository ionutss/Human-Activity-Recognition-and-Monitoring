using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scanner
{
    public class Step
    {
        public int Type { get; set; }
        public double[] ValuesArray { get; set; }

        public Step(int type, double[] valuesArray)
        {
            this.Type = type;
            this.ValuesArray = valuesArray;
        }
    }
}
