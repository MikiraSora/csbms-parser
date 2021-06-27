using System;
using System.Collections.Generic;
using System.Text;

namespace CSBMSParser.Bmson
{
    public class StopEvent : BMSONObject
    {
        public long duration; // as value. Meaning of value depends on Channel.
    }
}
