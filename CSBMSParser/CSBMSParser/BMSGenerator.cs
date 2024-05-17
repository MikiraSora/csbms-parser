using System;
using System.Collections.Generic;
using System.Text;

namespace CSBMSParser
{
    class BMSGenerator
    {
        private int[] random;

        private byte[] data;

        private bool ispms;

        public BMSGenerator(byte[] data, bool ispms, int[] random)
        {
            this.data = data;
            this.random = random;
            this.ispms = ispms;
        }

        public BMSModel generate(int[] random, Encoding encoding)
        {
            BMSDecoder decoder = new BMSDecoder();
            return decoder.decode(data, ispms, random, encoding);
        }

        public int[] getRandom()
        {
            return random;
        }
    }
}
