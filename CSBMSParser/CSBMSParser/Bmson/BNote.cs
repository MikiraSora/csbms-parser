using System;
using System.Collections.Generic;
using System.Text;

namespace CSBMSParser.Bmson
{
    public class BNote : BMSONObject
    {
        public int id; // as it is.
        public int[] id_set; // as it is.
        public string condition;
        public int interval;
    }
}
