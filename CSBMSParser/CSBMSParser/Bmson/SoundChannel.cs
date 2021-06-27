using System;
using System.Collections.Generic;
using System.Text;

namespace CSBMSParser.Bmson
{
    // sound channel.
    public class SoundChannel
    {
        public string name; // as sound file name
        public Note[] notes; // as notes using this sound.
    }
}
