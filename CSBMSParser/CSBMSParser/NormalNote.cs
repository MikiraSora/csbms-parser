using System;
using System.Collections.Generic;
using System.Text;

namespace CSBMSParser
{
    /**
	 * 通常ノート
	 * 
	 * @author exch
	 */
    public class NormalNote : Note
    {

        public NormalNote(int wav)
        {
            this.setWav(wav);
        }

        public NormalNote(int wav, long start, long duration)
        {
            this.setWav(wav);
            this.setMicroStarttime(start);
            this.setMicroDuration(duration);
        }

    }
}
