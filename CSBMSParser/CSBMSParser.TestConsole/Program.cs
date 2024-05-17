using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSBMSParser.TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var decoder = new BMSDecoder();
            var model = decoder.decode(@"F:\bms\problematic BMS\職権を乱用するRainbow\ubmchallenge (2).bms", encoding: System.Text.Encoding.GetEncoding("Shift-JIS"));
            var timeline = model.getAllTimeLines();

            var notes = timeline.Select(x => x.getBackGroundNotes().Concat(x.getNotes().Concat(x.getHiddenNotes()))).SelectMany(x => x).OfType<NormalNote>().OrderBy(x => x.getMicroTime()).ToArray();
            var wavList = model.getWavList();

            foreach (var tl in timeline)
            {
                Console.WriteLine($"timeline: {tl.getMilliTime()} stop: {tl.getStop()}");
            }

            foreach (var note in notes)
            {
                var wavId = note.getWav();
                var msTime = note.getMilliTime();
                if (wavId >= 0)
                    Console.WriteLine($"{msTime} : {wavId} : {wavList[wavId]}");
                else
                    Console.WriteLine($"{msTime} unknown wavId {wavId}");
            }

            Console.ReadLine();
        }
    }
}
