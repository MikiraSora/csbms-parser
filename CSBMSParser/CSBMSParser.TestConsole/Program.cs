using System;

namespace CSBMSParser.TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var decoder = new BMSDecoder();
            var model = decoder.decode(@"F:\\test.bms");

            Console.ReadLine();
        }
    }
}
