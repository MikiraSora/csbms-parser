using System;
using System.Collections.Generic;
using System.Text;

namespace CSBMSParser
{
    public class ChartInformation
    {
        public string path;

        public int lntype;

        public int[] selectedRandoms;

        public ChartInformation(string path, int lntype, int[] selectedRandoms)
        {
            this.path = path;
            this.lntype = lntype;
            this.selectedRandoms = selectedRandoms;
        }
    }
}
