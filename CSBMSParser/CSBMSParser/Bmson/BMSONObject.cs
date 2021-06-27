using System;
using System.Collections.Generic;
using System.Text;

namespace CSBMSParser.Bmson
{
    public class BMSONObject : IComparable<BMSONObject>
    {
        public int y; // as locate( 240BPM,1sec = 960 )

        public int CompareTo(BMSONObject other)
        {
            return y - other.y;
        }
    }
}
