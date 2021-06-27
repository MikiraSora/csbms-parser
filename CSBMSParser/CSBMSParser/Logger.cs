using System;
using System.Collections.Generic;
using System.Text;

namespace CSBMSParser
{
    public class Logger
    {
        public static Logger Instance { get; set; } = new Logger();

        public static Logger getGlobal() => Instance;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:命名样式", Justification = "<挂起>")]
        public virtual void fine(string str)
        {
            Console.WriteLine(str);
        }

        public virtual void severe(string v) => fine(v);

        public virtual void warning(string v) => fine(v);
    }
}
