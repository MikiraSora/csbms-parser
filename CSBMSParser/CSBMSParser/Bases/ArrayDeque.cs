using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSBMSParser.Bases
{
    internal class ArrayDeque<T> : List<T>
    {
        public bool IsEmpty()
        {
            return Count == 0;
        }

        public T GetLast()
        {
            if (Count == 0)
                return default;

            var ret = this.LastOrDefault();
            //RemoveAt(Count - 1);

            return ret;
        }

        public void RemoveLast()
        {
            if (Count == 0)
                return;
            RemoveAt(Count - 1);
        }
    }
}
